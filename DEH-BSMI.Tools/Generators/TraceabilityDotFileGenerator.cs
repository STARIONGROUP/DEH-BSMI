// -------------------------------------------------------------------------------------------------
//  <copyright file="TraceabilityDotFileGenerator.cs" company="Starion Group S.A.">
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

namespace DEHBSMI.Tools.Generators
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using DEHBSMI.Tools.Extensions;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    /// The purpose of the <see cref="TraceabilityDotFileGenerator"/> is to generate a Graphviz
    /// dot file that displays traceability from the various levels of the requirements
    /// </summary>
    public class TraceabilityDotFileGenerator : ITraceabilityDotFileGenerator
    {
        /// <summary>
        /// The <see cref="ILogger"/> used to log
        /// </summary>
        private readonly ILogger<TraceabilityDotFileGenerator> logger;

        /// <summary>
        /// The requirements for which a diagram is to be made
        /// </summary>
        private readonly HashSet<Requirement> requirements;

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceabilityDotFileGenerator"/> class.
        /// </summary>
        /// <param name="loggerFactory">
        /// The (injected) <see cref="ILoggerFactory"/> used to set up logging
        /// </param>
        public TraceabilityDotFileGenerator(ILoggerFactory loggerFactory = null)
        {
            logger = loggerFactory == null ? NullLogger<TraceabilityDotFileGenerator>.Instance : loggerFactory.CreateLogger<TraceabilityDotFileGenerator>();

            requirements = new HashSet<Requirement>();
        }

        /// <summary>
        /// Generates the dot file
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> for which the traceability dot file is to be generated
        /// </param>
        /// <param name="regionConfigurations">
        /// The <see cref="RequirementsSpecification"/> and <see cref="Category"/> combinations that are used to
        /// generate sub-graphs
        /// </param>
        /// <param name="dotFileInfo">
        /// the generated dot file
        /// </param>
        public void Generate(Iteration iteration, IEnumerable<Tuple<RequirementsSpecification, IEnumerable<Category>>> regionConfigurations, FileInfo dotFileInfo)
        {
            if (iteration == null)
            {
                throw new ArgumentNullException(nameof(iteration));
            }

            if (regionConfigurations == null)
            {
                throw new ArgumentNullException(nameof(regionConfigurations));
            }

            if (!regionConfigurations.Any())
            {
                throw new ArgumentException(nameof(regionConfigurations));
            }

            if (dotFileInfo == null)
            {
                throw new ArgumentNullException(nameof(dotFileInfo));
            }

            var content = GenerateContents(iteration, regionConfigurations);

            System.IO.File.WriteAllText(dotFileInfo.FullName, content);
        }

        /// <summary>
        /// Generate the contents of the dot file
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> for which the dot file is to be generated
        /// </param>
        /// <param name="regionConfigurations">
        /// configuration for <see cref="RequirementsSpecification"/> and applicable <see cref="Category"/>
        /// </param>
        /// <returns>
        /// a string that contains the generated dot notation
        /// </returns>
        private string GenerateContents(Iteration iteration, IEnumerable<Tuple<RequirementsSpecification, IEnumerable<Category>>> regionConfigurations)
        {
            logger.LogInformation("Start Generating the traceability dot-file");

            var sb = new StringBuilder();

            var engineeringModel = iteration.Container as EngineeringModel;
            var shortname = engineeringModel.EngineeringModelSetup.ShortName;

            sb.AppendLine($"digraph {shortname.CleanupShortName()} {{");
            sb.AppendLine("  rankdir=LR;");
            sb.AppendLine("  node [shape=box, style=filled, fillcolor=white, fontname=\"Helvetica\"];");
            sb.AppendLine();

            foreach (var configuration in regionConfigurations)
            {
                var requirementsSpecificationSubGraph = GenerateRequirementsSpecificationSubGraph(configuration);

                sb.Append(requirementsSpecificationSubGraph);
            }

            sb.AppendLine();

            var relationships = GeneratedRelationships(iteration);

            sb.Append(relationships);

            sb.AppendLine("}");

            logger.LogInformation("Finished Generating the traceability dot-file");

            return sb.ToString();
        }

        /// <summary>
        /// Generate the dot notation for a specific <see cref="RequirementsSpecification"/>
        /// </summary>
        /// <param name="configuration">
        /// a tuple of <see cref="RequirementsSpecification"/> and applicable <see cref="Category"/>
        /// </param>
        /// <returns>
        /// a string that contains the generated dot notation
        /// </returns>
        private string GenerateRequirementsSpecificationSubGraph(Tuple<RequirementsSpecification, IEnumerable<Category>> configuration)
        {
            var requirementSpecification = configuration.Item1;
            var categories = configuration.Item2;

            var sb = new StringBuilder();

            sb.AppendLine();
            sb.AppendLine($"    subgraph specification_{requirementSpecification.ShortName.CleanupShortName()} {{");
            sb.AppendLine($"      label=\"{requirementSpecification.ShortName.CleanupShortName()}\";");
            sb.AppendLine($"      style=filled;");
            sb.AppendLine($"      color=lightgrey;");

            foreach (var category in categories)
            {
                sb.AppendLine($"        subgraph category_{category.ShortName.CleanupShortName()} {{");
                sb.AppendLine($"          label=\"{category.ShortName.CleanupShortName()}\";");
                sb.AppendLine($"          ");

                foreach (var requirement in requirementSpecification.Requirement
                             .Where(x => !x.IsDeprecated)
                             .OrderBy(x => x.ShortName))
                {
                    if (requirement.IsMemberOfCategory(category))
                    {
                        if (requirements.Add(requirement))
                        {
                            if (requirement.Definition.FirstOrDefault() != null)
                            {
                                var requirementText =
                                    $"{requirement.ShortName} [{requirement.Owner.ShortName}] \r\n {requirement.Definition.FirstOrDefault().Content.Replace("\"", "\\\"")} \r\n {requirement.GetAllCategoryShortNames()} \r\n {requirement.Owner.ShortName}";

                                sb.AppendLine(
                                    $"          {requirement.ShortName.CleanupShortName()} [label=\"{requirement.ShortName.CleanupShortName()}\", tooltip=\"{requirementText}\"];");
                            }
                        }
                    }
                }

                sb.AppendLine("        }");
            }
            sb.AppendLine();

            sb.AppendLine("    }");

            return sb.ToString();
        }

        /// <summary>
        /// Generates the relationships between the requirements present in the iteration 
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that contains the 
        /// </param>
        /// <returns>
        /// a dot-notation string
        /// </returns>
        private string GeneratedRelationships(Iteration iteration)
        {
            var sb = new StringBuilder();

            foreach (var relationship in iteration.Relationship.OfType<BinaryRelationship>())
            {
                if (requirements.Contains(relationship.Source) && requirements.Contains(relationship.Target))
                {
                    var sourceRequirement = relationship.Source as Requirement;
                    var targetRequirement = relationship.Target as Requirement;

                    sb.AppendLine($"  {sourceRequirement.ShortName.CleanupShortName()} -> {targetRequirement.ShortName.CleanupShortName()} [tooltip=\"{sourceRequirement.ShortName} -> {targetRequirement.ShortName}\"];");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Queries the name of the report type that is generated by the current <see cref="IReportGenerator"/>
        /// </summary>
        /// <returns>
        /// human-readable name of the report type
        /// </returns>
        public string QueryReportType()
        {
            return "DOT";
        }

        /// <summary>
        /// Verifies whether the extension of the <paramref name="outputPath"/> is valid or not
        /// </summary>
        /// <param name="outputPath">
        /// The subject <see cref="FileInfo"/> to check
        /// </param>
        /// <returns>
        /// A Tuple of bool and string, where the string contains a description of the verification.
        /// Either stating that the extension is valid or not.
        /// </returns>
        public Tuple<bool, string> IsValidReportExtension(FileInfo outputPath)
        {
            if (outputPath == null)
            {
                throw new ArgumentNullException(nameof(outputPath));
            }

            switch (outputPath.Extension)
            {
                case ".dot":
                    return new Tuple<bool, string>(true, ".dot is a supported report extension");
                default:
                    return new Tuple<bool, string>(false, $"The Extension of the output file '{outputPath.Extension}' is not supported. Supported extensions is '.dot'");
            }
        }
    }
}
