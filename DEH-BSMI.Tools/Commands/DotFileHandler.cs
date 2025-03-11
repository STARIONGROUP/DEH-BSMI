// -------------------------------------------------------------------------------------------------
//  <copyright file="DotFileHandler.cs" company="Starion Group S.A.">
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
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal.DAL;

    using DEHBSMI.Tools.Generators;

    using Spectre.Console;

    using STARIONGROUP.DEHCSV.Services;

    /// <summary>
    /// Abstract super class from which all Report <see cref="DotFileHandler"/>s need to derive
    /// </summary>
    public abstract class DotFileHandler : ReportHandler
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
        /// <param name="traceabilityDotFileGenerator">
        /// The <see cref="ITraceabilityDotFileGenerator"/> used to generate a DOT file
        /// </param>
        protected DotFileHandler(IDataSourceSelector dataSourceSelector, IIterationReader iterationReader, ITraceabilityDotFileGenerator traceabilityDotFileGenerator) : base(dataSourceSelector, iterationReader)
        {
            this.TraceabilityDotFileGenerator = traceabilityDotFileGenerator ?? throw new ArgumentNullException(nameof(traceabilityDotFileGenerator));
        }

        /// <summary>
        /// Gets or sets the <see cref="IReportGenerator"/> used to generate a UML report
        /// </summary>
        public ITraceabilityDotFileGenerator TraceabilityDotFileGenerator { get; private set; }

        /// <summary>
        /// Gets or sets the name of the specifications and associated Categories
        /// </summary>
        public List<string> Specification { get; set; }

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

            var isValidExtension = this.TraceabilityDotFileGenerator.IsValidReportExtension(this.OutputReport);
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

                foreach (var spec in Specification)
                {
                    AnsiConsole.MarkupLine($"[green] --specification: {Markup.Escape(spec)}[/]");
                }

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

                var engineeringModel = iteration.Container as EngineeringModel;

                var engineeringModelRdlChain = session.GetEngineeringModelRdlChain(engineeringModel);

                var categories = engineeringModelRdlChain.SelectMany(x => x.DefinedCategory).ToList();

                AnsiConsole.MarkupLine($"[green] Generating Report[/]");
                await Task.Delay(250);

                var tuples = this.ConvertSpecificationArgument(iteration, this.Specification, categories);

                this.TraceabilityDotFileGenerator.Generate(iteration, tuples, this.OutputReport);

                AnsiConsole.MarkupLine($"[grey]LOG:[/] Requirement {this.TraceabilityDotFileGenerator.QueryReportType()} report generated at [bold]{this.OutputReport.FullName}[/]");
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
        /// Converts the <paramref name="specifications"/> to the specification and category combinations
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that contains the <see cref="RequirementsSpecification"/>s
        /// </param>
        /// <param name="specifications">
        /// the combination of specification and category shortnames
        /// </param>
        /// <param name="possibleCategories">
        /// the categories that are valid for the open iterations
        /// </param>
        /// <returns>
        /// specification and category combinations
        /// </returns>
        private List<Tuple<RequirementsSpecification, IEnumerable<Category>>> ConvertSpecificationArgument(Iteration iteration, List<string> specifications, List<Category> possibleCategories)
        {
            var tuples = new List<Tuple<RequirementsSpecification, IEnumerable<Category>>>();

            foreach (var specification in specifications)
            {
                var parts = specification.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 1)
                    throw new ArgumentException("Specification input must contain at least a specification shortname.");

                var specificationShortName = parts[0];
                var categoryShortnames = parts.Skip(1).ToList();

                var requirementsSpecification = iteration.RequirementsSpecification.SingleOrDefault(x => x.ShortName == specificationShortName);

                if (requirementsSpecification == null)
                {
                    continue;
                }

                var specCategories = new List<Category>();

                foreach (var categoryShortname in categoryShortnames)
                {
                    var category = possibleCategories.SingleOrDefault(x => x.ShortName == categoryShortname);
                    if (category != null)
                    {
                        specCategories.Add(category);
                    }
                }

                tuples.Add(new Tuple<RequirementsSpecification, IEnumerable<Category>>(requirementsSpecification, specCategories));
            }

            return tuples;
        }
    }
}
