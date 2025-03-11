// -------------------------------------------------------------------------------------------------
//  <copyright file="RequirementsReportHandler.cs" company="Starion Group S.A.">
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

namespace DEHBSMI.Tools.Commands
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine.Invocation;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;

    using CDP4Dal.DAL;

    using DEHBSMI.Tools.Generators;

    using Spectre.Console;

    using STARIONGROUP.DEHCSV.Services;

    /// <summary>
    /// Abstract super class from which all Report <see cref="RequirementsReportHandler"/>s need to derive
    /// </summary>
    public abstract class RequirementsReportHandler : ReportHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsReportHandler"/>
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
        protected RequirementsReportHandler(IDataSourceSelector dataSourceSelector, IIterationReader iterationReader, IReportGenerator reportGenerator) : base(dataSourceSelector, iterationReader)
        {
            this.ReportGenerator = reportGenerator ?? throw new ArgumentNullException(nameof(reportGenerator));
        }

        /// <summary>
        /// Gets or sets the <see cref="IReportGenerator"/> used to generate a UML report
        /// </summary>
        public IReportGenerator ReportGenerator { get; private set; }

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
        /// Asynchronously invokes the <see cref="ICommandHandler"/>
        /// </summary>
        /// <param name="context">
        /// The <see cref="InvocationContext"/> 
        /// </param>
        /// <returns>
        /// 0 when successful, another if not
        /// </returns>
        public override async Task<int> InvokeAsync(InvocationContext context)
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

                this.ReportGenerator.Generate(iteration, requirementsSpecifications, this.OutputReport, this.UnallocatedBsmiCode);

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
    }
}
