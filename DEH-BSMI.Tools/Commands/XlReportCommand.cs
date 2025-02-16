// -------------------------------------------------------------------------------------------------
//  <copyright file="XlReportCommand.cs" company="Starion Group S.A.">
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
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.IO;

    using DEHBSMI.Tools.Generators;

    using STARIONGROUP.DEHCSV.Services;

    /// <summary>
    /// The purpose of the <see cref="XlReportCommand"/> is to convert ECSS-E-TM-10-25 data to
    /// a so-called BSMI structure in the form of a Excel spreadsheet report
    /// </summary>
    public class XlReportCommand : ReportCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XlReportCommand"/>
        /// </summary>
        public XlReportCommand() : base("excel-report", "Generates a tabular report of the 10-25 model")
        {
            var outputReportOption = new Option<FileInfo>(
                name: "--output-report",
                description: "The path to the tabular report file. Supported extensions are '.xlsx', '.xlsm', '.xltx' and '.xltm'",
                getDefaultValue: () => new FileInfo("requirements-report.xlsx"));
            outputReportOption.AddAlias("-o");
            outputReportOption.IsRequired = true;
            this.AddOption(outputReportOption);
        }

        /// <summary>
        /// The Command Handler of the <see cref="XlReportCommand"/>
        /// </summary>
        public new class Handler : ReportHandler, ICommandHandler
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
            /// <param name="xlReportGenerator">
            /// The (injected) <see cref="IXlReportGenerator"/> that is used to generate the
            /// excel report
            /// </param>
            public Handler(IDataSourceSelector dataSourceSelector,
                IIterationReader iterationReader, IXlReportGenerator xlReportGenerator) 
                : base(dataSourceSelector, iterationReader, xlReportGenerator)
            {
            }
        }
    }
}
