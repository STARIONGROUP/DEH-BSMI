// -------------------------------------------------------------------------------------------------
//  <copyright file="DotFileCommand.cs" company="Starion Group S.A.">
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
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.IO;

    using DEHBSMI.Tools.Generators;

    using STARIONGROUP.DEHCSV.Services;

    /// <summary>
    /// The purpose of the <see cref="DotFileCommand"/> is to convert ECSS-E-TM-10-25 data to
    /// into a dotfile that can be processed by GraphViz
    /// </summary>
    public class DotFileCommand : ReportCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DotFileCommand"/>
        /// </summary>
        public DotFileCommand() : base("dot-report", "Generates dot file")
        {
            var specOption = new Option<List<string>>(
                name: "--specification",
                description: "Requirement specification shortname followed by category shortnames, e.g. SPEC:CAT1:CAT2");
            specOption.AllowMultipleArgumentsPerToken = false;
            specOption.Arity = ArgumentArity.ZeroOrMore;
            specOption.AddAlias("-spec");
            this.AddOption(specOption);

            var outputReportOption = new Option<FileInfo>(
                name: "--output-report",
                description: "The path to the dot file. Supported extensions is '.dot'",
                getDefaultValue: () => new FileInfo("dot-report.dot"));
            outputReportOption.AddAlias("-o");
            outputReportOption.IsRequired = true;
            this.AddOption(outputReportOption);
        }

        /// <summary>
        /// The Command Handler of the <see cref="XlReportCommand"/>
        /// </summary>
        public new class Handler : DotFileHandler, ICommandHandler
        {
            /// <summary>
            /// Initializes a nwe instance of the <see cref="Handler"/> class.
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
            /// The (injected) <see cref="ITraceabilityDotFileGenerator"/> that is used to generate the
            /// excel report
            /// </param>
            public Handler(IDataSourceSelector dataSourceSelector,
                IIterationReader iterationReader, ITraceabilityDotFileGenerator traceabilityDotFileGenerator)
                : base(dataSourceSelector, iterationReader, traceabilityDotFileGenerator)
            {
            }
        }
    }
}