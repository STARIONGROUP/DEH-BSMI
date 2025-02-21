// -------------------------------------------------------------------------------------------------
//  <copyright file="ReportHandler.cs" company="Starion Group S.A.">
// 
//    Copyright 2019-2025 Starion Group S.A.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  ------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using CDP4Common.EngineeringModelData;

namespace DEHBSMI.Tools.Commands
{
    using System;
    using System.CommandLine.Invocation;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using CDP4Dal;
    using CDP4Dal.DAL;
    
    using DEHBSMI.Tools.Generators;
    using DEHBSMI.Tools.Resources;

    using Spectre.Console;
    
    using STARIONGROUP.DEHCSV.Services;

    /// <summary>
    /// Abstract super class from which all Report <see cref="ICommandHandler"/>s need to derive
    /// </summary>
    public abstract class ReportHandler
    {
        /// <summary>
        /// The (injected) <see cref="IDataSourceSelector"/> used to select the appropriate
        /// implementation of the <see cref="IDal"/>
        /// </summary>
        private readonly IDataSourceSelector dataSourceSelector;

        /// <summary>
        /// The (injected) <see cref="IIterationReader"/> used to read <see cref="Iteration"/> data 
        /// from the selected data source
        /// </summary>
        private readonly IIterationReader iterationReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportHandler"/>
        /// </summary>
        /// <param name="dataSourceSelector">
        /// The (injected) <see cref="IDataSourceSelector"/> used to select the appropriate
        /// implementation of the <see cref="IDal"/>
        /// </param>
        /// <param name="iterationReader">
        /// The (injected) <see cref="IIterationReader"/> used to read <see cref="Iteration"/> data 
        /// from the selected data source
        /// </param>
        /// <param name="reportGenerator">
        /// The <see cref="IReportGenerator"/> used to generate a UML report
        /// </param>
        protected ReportHandler(IDataSourceSelector dataSourceSelector,
            IIterationReader iterationReader, IReportGenerator reportGenerator)
        {
            this.dataSourceSelector = dataSourceSelector ?? throw new ArgumentNullException(nameof(dataSourceSelector));
            this.iterationReader = iterationReader ?? throw new ArgumentNullException(nameof(iterationReader));
            this.ReportGenerator = reportGenerator ?? throw new ArgumentNullException(nameof(reportGenerator));
        }

        /// <summary>
        /// Gets or sets the <see cref="IReportGenerator"/> used to generate a UML report
        /// </summary>
        public IReportGenerator ReportGenerator { get; private set; }

        /// <summary>
        /// Gets or sets the value indicating whether the logo should be shown or not
        /// </summary>
        public bool NoLogo { get; set; }

        /// <summary>
        /// Gets or sets the username that is used to connected to the selected data source
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password that is used to connected to the selected data source
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the Uri of the ECSS-E-TM-10-25 data source
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// Gets or sets the shortname of the selected Engineering Model
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets the number of the selected Iteration
        /// </summary>
        public int Iteration { get; set; }

        /// <summary>
        /// Gets or sets the shortname of the selected Domain of Expertise
        /// </summary>
        public string DomainOfExpertise { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="FileInfo"/> where the BSMI report is to be generated
        /// </summary>
        public FileInfo OutputReport { get; set; }

        /// <summary>
        /// Gets or sets the name of the specification that needs to be processed. If empty or null then all non-deprecated
        /// Requirements Specifications are taken into account
        /// </summary>
        public string SourceSpecification { get; set; }

        /// <summary>
        /// Gets or sets the value for the BSMI parameter for unallocated requirements
        /// </summary>
        public string UnallocatedBsmiCode { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the generated report needs to be automatically be
        /// opened once generated.
        /// </summary>
        public bool AutoOpenReport { get; set; }

        /// <summary>
        /// Invokes the <see cref="ICommandHandler"/>
        /// </summary>
        /// <param name="context">
        /// The <see cref="InvocationContext"/> 
        /// </param>
        /// <returns>
        /// 0 when successful, another if not
        /// </returns>
        public int Invoke(InvocationContext context)
        {
            throw new NotSupportedException("Please use InvokeAsync");
        }

        /// <summary>
        /// Asynchronously invokes the <see cref="ICommandHandler"/>
        /// </summary>
        /// <param name="context">
        /// The <see cref="InvocationContext"/> 
        /// </param>
        /// <returns>
        /// 0 when successful, another if not
        /// </returns>
        public async Task<int> InvokeAsync(InvocationContext context)
        {
            if (!this.InputValidation())
            {
                return -1;
            }

            var isValidExtension = this.ReportGenerator.IsValidReportExtension(this.OutputReport);
            if (!isValidExtension.Item1)
            {
                AnsiConsole.WriteLine("");
                AnsiConsole.MarkupLine($"[red] {isValidExtension.Item2} [/]");
                AnsiConsole.WriteLine("");
                return -1;
            }

            try
            {
                AnsiConsole.MarkupLine("[yellow]Initializing report parameters...[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[green] --no-logo: {Markup.Escape(this.NoLogo.ToString(CultureInfo.InvariantCulture))}[/]");
                AnsiConsole.MarkupLine($"[green] --username: {Markup.Escape(this.Username)}[/]");
                AnsiConsole.MarkupLine($"[green] --data-source: {Markup.Escape(this.DataSource)}[/]");
                AnsiConsole.MarkupLine($"[green] --model: {Markup.Escape(this.Model)}[/]");
                AnsiConsole.MarkupLine($"[green] --iteration: {Markup.Escape(this.Iteration.ToString(CultureInfo.InvariantCulture))}[/]");
                AnsiConsole.MarkupLine($"[green] --domainofexpertise: {Markup.Escape(this.DomainOfExpertise)}[/]");
                AnsiConsole.MarkupLine($"[green] --source-specification: {Markup.Escape(string.IsNullOrEmpty(this.SourceSpecification) ? "All" : this.SourceSpecification)}[/]");
                AnsiConsole.MarkupLine($"[green] --unallocated-bsmi-code: {Markup.Escape(this.UnallocatedBsmiCode)}[/]");
                AnsiConsole.MarkupLine($"[green] --auto-open-report: {Markup.Escape(this.AutoOpenReport.ToString(CultureInfo.InvariantCulture))}[/]");

                AnsiConsole.WriteLine();
                await Task.Delay(500);

                AnsiConsole.MarkupLine($"[green] Connecting to source[/]");
                await Task.Delay(250);

                var uri = this.CreateUriFromDataSource(this.DataSource);
                var session = this.CreateSession(uri);
                await session.Open(false);

                AnsiConsole.MarkupLine($"[green] Reading Iteration data[/]");
                await Task.Delay(250);

                var iteration = await this.iterationReader.ReadAsync(session, this.Model, this.Iteration, this.DomainOfExpertise);

                var requirementsSpecifications = new List<RequirementsSpecification>();
                if (string.IsNullOrEmpty(this.SourceSpecification))
                {
                    var specs = iteration.RequirementsSpecification.Where(x => !x.IsDeprecated);
                    requirementsSpecifications.AddRange(specs);
                }
                else
                {
                    var spec = iteration.RequirementsSpecification.SingleOrDefault(x => x.ShortName == this.SourceSpecification);
                    if (spec != null)
                    {
                        requirementsSpecifications.Add(spec);
                    }
                }

                AnsiConsole.MarkupLine($"[green] Generating Report[/]");
                await Task.Delay(250);

                this.ReportGenerator.Generate(iteration, requirementsSpecifications, this.OutputReport);

                AnsiConsole.MarkupLine($"[grey]LOG:[/] Requirement {this.ReportGenerator.QueryReportType()} report generated at [bold]{this.OutputReport.FullName}[/]");
                AnsiConsole.WriteLine();

                await Task.Delay(500);

                if (this.AutoOpenReport)
                {
                    this.ExecuteAutoOpen();
                }

                return 0;
            }
            catch (IOException ex)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[red]The report file could not be generated or opened. Make sure the file is not open and try again.[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[green]Dropping to impulse speed[/]");
                AnsiConsole.WriteLine();
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[red]An exception occurred[/]");
                AnsiConsole.MarkupLine("[green]Dropping to impulse speed[/]");
                AnsiConsole.MarkupLine("[red]please report an issue at[/]");
                AnsiConsole.MarkupLine("[link] https://github.com/STARIONGROUP [/]");
                AnsiConsole.WriteLine();
                AnsiConsole.WriteException(ex);

                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Converts the data source to a <see cref="Uri"/>
        /// </summary>
        /// <param name="dataSource">
        /// The string based uri of the ECSS-E-TM-10-25 
        /// </param>
        /// <returns>
        /// a <see cref="Uri"/> representation of the <paramref name="dataSource"/>
        /// </returns>
        /// <exception cref="FileNotFoundException">
        /// thrown when the <paramref name="dataSource"/> is a file (not http or https) and
        /// when it cannot be found.
        /// </exception>
        private Uri CreateUriFromDataSource(string dataSource)
        {
            if (this.DataSource.StartsWith("http"))
            {
                return new Uri(this.DataSource);
            }

            var fileInfo = new FileInfo(this.DataSource);

            if (fileInfo.Exists)
            {
                return new Uri(fileInfo.FullName);
            }

            var baseUri = new Uri(System.AppContext.BaseDirectory);
            var fileUri = new Uri(baseUri, this.DataSource);

            if (File.Exists(fileUri.AbsolutePath))
            {
                return fileUri;
            }

            throw new FileNotFoundException($"The datasource was not found ({fileUri.AbsolutePath})", dataSource);
        }

        /// <summary>
        /// Creates a session object based on the datasource, username and password
        /// </summary>
        /// <param name="uri">
        /// The <see cref="Uri"/> for which the <see cref="ISession"/> is to be created.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ISession"/>
        /// </returns>
        private ISession CreateSession(Uri uri)
        {
            var dal = this.dataSourceSelector.Select(uri);

            var credentials = new Credentials(this.Username, this.Password, uri);

            ICDPMessageBus bus = new CDPMessageBus();
            
            var session = new Session(dal, credentials, bus);

            return session;
        }

        /// <summary>
        /// validates the options
        /// </summary>
        /// <returns>
        /// 0 when successful, -1 when not
        /// </returns>
        protected bool InputValidation()
        {
            if (!this.NoLogo)
            {
                AnsiConsole.Markup($"[blue]{ResourceLoader.QueryLogo()}[/]");
            }

            return true;
        }

        /// <summary>
        /// Automatically opens the generated report
        /// </summary>
        /// <param name="ctx">
        /// Spectre Console <see cref="StatusContext"/>
        /// </param>
        protected void ExecuteAutoOpen()
        {
            if (this.AutoOpenReport)
            {
                AnsiConsole.WriteLine("Opening generated report");
                Thread.Sleep(1500);

                try
                {
                    Process.Start(new ProcessStartInfo(this.OutputReport.FullName)
                    { UseShellExecute = true });
                    AnsiConsole.WriteLine("Generated report opened");
                }
                catch
                {
                    AnsiConsole.WriteLine("Opening of generated report failed, please open manually");
                    Thread.Sleep(1500);
                }
            }
        }
    }
}
