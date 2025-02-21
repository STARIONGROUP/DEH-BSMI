// -------------------------------------------------------------------------------------------------
//  <copyright file="ReportCommand.cs" company="Starion Group S.A.">
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
    using System.IO;

    /// <summary>
    /// Abstract super class from which all report commands shall inherit
    /// </summary>
    public abstract class ReportCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportCommand"/>
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="description">The description of the command, shown in help.</param>
        protected ReportCommand(string name, string description = null) : base(name, description)
        {
            var noLogoOption = new Option<bool>(
                name: "--no-logo",
                description: "Suppress the logo",
                getDefaultValue: () => false);
            this.AddOption(noLogoOption);

            var dataSourceUriOption = new Option<string>(
                name: "--data-source",
                description: "The URI of the ECSS-E-TM-10-25 data source from which the requirement export is to be generated");
            dataSourceUriOption.AddAlias("-ds");
            dataSourceUriOption.IsRequired = true;
            this.AddOption(dataSourceUriOption);

            var usernameOption = new Option<string>(
                name: "--username",
                description: "The username that is used to open the selected data source");
            usernameOption.AddAlias("-u");
            usernameOption.IsRequired = true;
            this.AddOption(usernameOption);

            var passwordOption = new Option<string>(
                name: "--password",
                description: "The password that is used to open the selected data source");
            passwordOption.AddAlias("-p");
            passwordOption.IsRequired = true;
            this.AddOption(passwordOption);

            var modelOption = new Option<string>(
                name: "--model",
                description: "The EngineeringModel shortname");
            modelOption.AddAlias("-m");
            modelOption.IsRequired = true;
            this.AddOption(modelOption);

            var iterationOption = new Option<int>(
                name: "--iteration",
                description: "the Iteration number");
            iterationOption.AddAlias("-i");
            iterationOption.IsRequired = true;
            this.AddOption(iterationOption);

            var domainOfExpertiseOption = new Option<string>(
                name: "--domainofexpertise",
                description: "The Domain of Expertise shortname"
            );
            domainOfExpertiseOption.AddAlias("-d");
            domainOfExpertiseOption.IsRequired = true;
            this.AddOption(domainOfExpertiseOption);

            var sourceSpecificationOption = new Option<string>(
                    name: "--source-specification",
                    description: "The Specification from which the report is generated. If not specified all available non-deprecated specifications are taken into account"
            );
            sourceSpecificationOption.AddAlias("-spec");
            sourceSpecificationOption.IsRequired = false;
            this.AddOption(sourceSpecificationOption);

            var unallocatedBsmiCodeOption = new Option<string>(
                name: "--unallocated-bsmi-code",
                description: "the value of the BSMI parameter for unallocated requirements",
                getDefaultValue: () => "9999"
            );
            unallocatedBsmiCodeOption.AddAlias("-ubc");
            unallocatedBsmiCodeOption.IsRequired = false;
            this.AddOption(unallocatedBsmiCodeOption);

            var autoOpenReportOption = new Option<bool>(
                name: "--auto-open-report",
                description: "Open the generated report with its default application",
                getDefaultValue: () => false);
            autoOpenReportOption.AddAlias("-a");
            autoOpenReportOption.IsRequired = false;
            this.AddOption(autoOpenReportOption);
        }
    }
}
