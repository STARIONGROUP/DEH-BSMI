﻿// -------------------------------------------------------------------------------------------------
//  <copyright file="XlReportGenerator.cs" company="Starion Group S.A.">
// 
//    Copyright 2025 Starion Group S.A.
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
    using System.Data;
    using System.IO;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    
    using ClosedXML.Excel;

    using DEHBSMI.Tools.Extensions;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using System.Diagnostics;
    
    using System.Globalization;
    using System.Reflection;

    /// <summary>
    /// The purpose of the <see cref="XlReportGenerator"/> is to generate an XL based
    /// report from an ECSS-E-TM-10-25 model according the BSMI structure
    /// </summary>
    public class XlReportGenerator : IXlReportGenerator
    {
        /// <summary>
        /// The <see cref="ILogger"/> used to log
        /// </summary>
        private readonly ILogger<XlReportGenerator> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="XlReportGenerator"/> class.
        /// </summary>
        /// <param name="loggerFactory">
        /// The (injected) <see cref="ILoggerFactory"/> used to set up logging
        /// </param>
        public XlReportGenerator(ILoggerFactory loggerFactory = null)
        {
            this.logger = loggerFactory == null ? NullLogger<XlReportGenerator>.Instance : loggerFactory.CreateLogger<XlReportGenerator>();
        }

        /// <summary>
        /// Generates an Excel spreadsheet based BSMI export
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that contains the data that is to be generated
        /// </param>
        /// <param name="specifications">
        /// The <see cref="RequirementsSpecification"/>s that need to be taken into account for report generation
        /// </param>
        /// <param name="outputReport">
        /// The <see cref="FileInfo"/> where the result is to be generated
        /// </param>
        /// <param name="unallocatedBsmiCode">
        /// The value for the BSMI parameter for unallocated requirements
        /// </param>
        public void Generate(Iteration iteration, IEnumerable<RequirementsSpecification> specifications, FileInfo outputReport, string unallocatedBsmiCode)
        {
            if (iteration == null)
            {
                throw new ArgumentNullException(nameof(iteration));
            }

            if (outputReport == null)
            {
                throw new ArgumentNullException(nameof(outputReport));
            }

            var sw = Stopwatch.StartNew();

            this.logger.LogInformation("Start Generating the XL BMSI Report");

            using var workbook = new XLWorkbook();
            this.AddInfoSheet(workbook, iteration, specifications);

            // generate all requirements in a sheet - following the specification and grouping
            this.GenerateRequirementSheet(workbook, iteration, specifications);

            // generate an option sheet
            foreach (Option option in iteration.Option)
            {
                this.GenerateBsmiSheet(workbook, iteration, specifications, option, unallocatedBsmiCode);
            }

            this.logger.LogInformation("Saving BSMI file to {0}", outputReport.FullName);

            workbook.SaveAs(outputReport.FullName);

            this.logger.LogInformation("Generated Excel BSMI report in {0} [ms]", sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Generate the requirements in a dedicated sheet
        /// </summary>
        /// <param name="workbook">
        /// The <see cref="XLWorkbook"/> in which the requirements sheet is to be generation
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that contains the data
        /// </param>
        private void GenerateRequirementSheet(XLWorkbook workbook, Iteration iteration, IEnumerable<RequirementsSpecification> specifications)
        {
            var requirementsWorksheet = workbook.Worksheets.Add("Requirements");

            var dataTable = new DataTable();
            dataTable.Locale = CultureInfo.InvariantCulture;

            dataTable.Columns.Add("Specification", typeof(string));
            dataTable.Columns.Add("Group", typeof(string));
            dataTable.Columns.Add("Requirements Shortname", typeof(string));
            dataTable.Columns.Add("Requirements Name", typeof(string));
            dataTable.Columns.Add("Requirements Text", typeof(string));
            dataTable.Columns.Add("Owner", typeof(string));
            dataTable.Columns.Add("Categories", typeof(string));

            dataTable.Columns.Add("Derives", typeof(string));
            dataTable.Columns.Add("Derived By", typeof(string));

            foreach (var requirementsSpecification in specifications)
            {
                var nonGroupedRequirements = requirementsSpecification.Requirement.Where(x => x.Group == null && !x.IsDeprecated);

                foreach (var requirement in nonGroupedRequirements)
                {
                    this.AddRequirementToDataTable(dataTable, requirement, null);
                }

                foreach (var requirementsGroup in requirementsSpecification.GetAllContainedGroups())
                {
                    var groupedRequirements = requirementsSpecification.Requirement.Where(x => x.Group == requirementsGroup && !x.IsDeprecated);

                    foreach (var requirement in groupedRequirements)
                    {
                        this.AddRequirementToDataTable(dataTable, requirement, requirementsGroup);
                    }
                }
            }

            requirementsWorksheet.Cell(1, 1).InsertTable(dataTable, "Requirements", true);

            requirementsWorksheet.Cells();

            this.FormatSheet(requirementsWorksheet);
        }

        /// <summary>
        /// Adds a Requirement to the Requirements table
        /// </summary>
        /// <param name="dataTable">
        /// The target <see cref="DataTable"/>
        /// </param>
        /// <param name="requirement">
        /// The <see cref="Requirement"/> that is to be added
        /// </param>
        /// <param name="requirementsGroup">
        /// The <see cref="RequirementsGroup"/> that the <see cref="Requirement"/> belongs to
        /// </param>
        private void AddRequirementToDataTable(DataTable dataTable, Requirement requirement, RequirementsGroup requirementsGroup)
        {
            var requirementDataRow = dataTable.NewRow();

            var specification = requirement.Container as RequirementsSpecification;
            requirementDataRow["Specification"] = $"{specification.ShortName}";

            if (requirementsGroup == null)
            {
                requirementDataRow["Group"] = "";
            }
            else
            {
                requirementDataRow["Group"] = $"{requirementsGroup.Path('\\')}";
            }

            requirementDataRow["Requirements Shortname"] = $"{requirement.ShortName}";
            requirementDataRow["Requirements Name"] = $"{requirement.Name}";
            requirementDataRow["Requirements Text"] = $"{requirement.QueryDefinitionContent()}";
            requirementDataRow["Owner"] = $"{requirement.Owner?.ShortName}";
            requirementDataRow["Categories"] = $"{requirement.GetAllCategoryShortNames()}";

            requirementDataRow["Derives"] = string.Join(" ; ",this.ComputeIncomingRequirementShortnames(requirement));

            requirementDataRow["Derived By"] = string.Join(" ; ", this.ComputeOutgoingRequirementShortnames(requirement));

            dataTable.Rows.Add(requirementDataRow);
        }

        /// <summary>
        /// Generate the BSMI requirements for an Option in a dedicated sheet
        /// </summary>
        /// <param name="workbook">
        /// The <see cref="XLWorkbook"/> in which the requirements sheet is to be generation
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that contains the data
        /// </param>
        /// <param name="specifications">
        /// The <see cref="RequirementsSpecification"/>s that need to be taken into account for report generation
        /// </param>
        /// <param name="option">
        /// The <see cref="Option"/> for which the requirements are generated
        /// </param>
        /// <param name="unallocatedBsmiCode">
        /// The value for the BSMI parameter for unallocated requirements
        /// </param>
        private void GenerateBsmiSheet(XLWorkbook workbook, Iteration iteration, IEnumerable<RequirementsSpecification> specifications, Option option, string unallocatedBsmiCode)
        {
            var optionWorksheet = workbook.Worksheets.Add($"{option.ShortName}");

            var nestedElementTreeGenerator = new NestedElementTreeGenerator();
            var nestedElements = nestedElementTreeGenerator.Generate(option, false);

            var binaryRelationships = iteration.Relationship.OfType<BinaryRelationship>();

            var requirementPayloads = new Dictionary<Guid, RequirementPayload>();

            foreach (var nestedElement in nestedElements)
            {
                var bsmiNestedParameter = nestedElement.NestedParameter.SingleOrDefault(x => x.UserFriendlyShortName == "BSMI" && x.AssociatedParameter.ClassKind == ClassKind.Parameter);
                if (bsmiNestedParameter != null)
                {
                    var parameter = bsmiNestedParameter.AssociatedParameter as Parameter;
                    var elementDefinition = parameter.Container as ElementDefinition;

                    var equipmentSatisfiesRequirement = binaryRelationships
                        .Where(x => x.Source == elementDefinition && 
                                    x.Target is Requirement);

                    foreach (var binaryRelationship in equipmentSatisfiesRequirement)
                    {
                        var requirement = binaryRelationship.Target as Requirement;

                        if (requirement == null || requirement.IsDeprecated)
                        {
                            continue;
                        }

                        var isRequirementContainedByInlcudedSpecification = specifications.Any(requirementsSpecification => requirementsSpecification.Requirement.Contains(requirement));

                        if (isRequirementContainedByInlcudedSpecification )
                        {
                            if (!requirementPayloads.TryGetValue(requirement.Iid, out var requirementPayload))
                            {
                                requirementPayload = new RequirementPayload(requirement);
                                requirementPayload.Bsmi = bsmiNestedParameter.ActualValue;
                                requirementPayloads.Add(requirement.Iid, requirementPayload);
                            }

                            requirementPayload.BinaryRelationships.Add(binaryRelationship);
                            requirementPayload.NestedElement.Add(nestedElement);

                            if (requirementPayload.Bsmi != bsmiNestedParameter.ActualValue)
                            {
                                this.logger.LogWarning("The requirement has been linked to multiple BSMI");
                            }
                        }
                    }
                }
            }

            // assign unallocated requirements a specific bsmi code
            foreach (var specification in specifications)
            {
                foreach (var requirement in specification.Requirement)
                {
                    if (requirement.IsDeprecated)
                    {
                        this.logger.LogInformation("Deprecated Requirement {Iid}:{ShortName} has been ignored", requirement.Iid, requirement.ShortName);
                        continue;
                    }

                    var allocatedRequirements = requirementPayloads.Where(x => x.Value.Requirement == requirement);
                    if (!allocatedRequirements.Any())
                    {
                        if (!requirementPayloads.TryGetValue(requirement.Iid, out var requirementPayload))
                        {
                            var simpleParameterValue = requirement.ParameterValue.SingleOrDefault(x => x.ParameterType.ShortName == "BSMI");

                            var bsmiValue = unallocatedBsmiCode;

                            if (simpleParameterValue != null && simpleParameterValue.Value[0] != "-")
                            {
                                bsmiValue = simpleParameterValue.Value[0];
                            }
                            else
                            {
                                this.logger.LogWarning("The requirement has not been linked to a BSMI and is therefore added to BSMI 9999");
                            }

                            requirementPayload = new RequirementPayload(requirement);
                            requirementPayload.Bsmi = bsmiValue;
                            requirementPayloads.Add(requirement.Iid, requirementPayload);
                        }
                    }
                }
            }

            var dataTable = new DataTable();
            dataTable.Locale = CultureInfo.InvariantCulture;

            dataTable.Columns.Add("Object Level", typeof(string));
            dataTable.Columns.Add("UID", typeof(string));
            dataTable.Columns.Add("BSMI Nummer", typeof(string));
            dataTable.Columns.Add("Eistekst - EN", typeof(string));
            dataTable.Columns.Add("Object Type", typeof(string));
            dataTable.Columns.Add("Derives", typeof(string));
            dataTable.Columns.Add("Derived By", typeof(string));
            dataTable.Columns.Add("Categories", typeof(string));

            var payloads = requirementPayloads.Values.OrderBy(x => x.Bsmi);

            foreach (var payload in payloads)
            {
                var requirementDataRow = dataTable.NewRow();

                var objectLevel = "-";
                try
                {
                    objectLevel = payload.Bsmi.ComputeObjectLevel();
                }
                catch (Exception)
                {
                    this.logger.LogWarning("the object level for requirement {Requirement} could not be computed", payload.Requirement.Iid);
                }

                requirementDataRow["Object Level"] = objectLevel;
                requirementDataRow["UID"] = payload.Requirement.ShortName;
                requirementDataRow["BSMI Nummer"] = payload.Bsmi;
                requirementDataRow["Eistekst - EN"] = payload.Requirement.QueryDefinitionContent();
                requirementDataRow["Object Type"] = "Requirement";
                requirementDataRow["Categories"] = $"{payload.Requirement.GetAllCategoryShortNames()}";
                requirementDataRow["Derives"] = string.Join(" ; ", this.ComputeIncomingRequirementShortnames(payload.Requirement));
                requirementDataRow["Derived By"] = string.Join(" ; ", this.ComputeOutgoingRequirementShortnames(payload.Requirement));

                dataTable.Rows.Add(requirementDataRow);
            }

            optionWorksheet.Cell(1, 1).InsertTable(dataTable, $"{option.ShortName}", true);

            this.FormatSheet(optionWorksheet);
        }

        /// <summary>
        /// Computes the shortnames of requirements for which there is a <see cref="BinaryRelationship"/>
        /// to the subject <paramref name="requirement"/> from a source <see cref="Requirement"/>
        /// </summary>
        /// <param name="requirement">
        /// The subject <see cref="Requirement"/> for which source requirements are to be computed
        /// </param>
        /// <returns>
        /// a list of requirement shortnames
        /// </returns>
        private IReadOnlyList<string> ComputeIncomingRequirementShortnames(Requirement requirement)
        {
            var incoming = requirement.QueryRelationships
                .OfType<BinaryRelationship>()
                .Where(x => x.Target == requirement && x.Source is Requirement);

            var incomingRequirementShortnames = new List<string>();

            foreach (var incomingBinaryRelationship in incoming)
            {
                if (incomingBinaryRelationship.Source is Requirement source && !source.IsDeprecated)
                {
                    incomingRequirementShortnames.Add(source.ShortName);
                }
            }

            return incomingRequirementShortnames;
        }

        /// <summary>
        /// Computes the shortnames of requirements for which there is a <see cref="BinaryRelationship"/>
        /// from the subject <paramref name="requirement"/> to a target <see cref="Requirement"/>
        /// </summary>
        /// <param name="requirement">
        /// The subject <see cref="Requirement"/> for which target requirements are to be computed
        /// </param>
        /// <returns>
        /// a list of requirement shortnames
        /// </returns>
        private IReadOnlyList<string> ComputeOutgoingRequirementShortnames(Requirement requirement)
        {
            var outgoing = requirement.QueryRelationships
                .OfType<BinaryRelationship>()
                .Where(x => x.Source == requirement && x.Target is Requirement);

            var outgoingRequirementShortnames = new List<string>();

            foreach (var outgoingBinaryRelationship in outgoing)
            {
                if (outgoingBinaryRelationship.Target is Requirement target && !target.IsDeprecated)
                {
                    outgoingRequirementShortnames.Add(target.ShortName);
                }
            }

            return outgoingRequirementShortnames;
        }

        /// <summary>
        /// Adds a worksheet that contains information about the model and generator
        /// </summary>
        /// <param name="workbook">
        /// The target <see cref="XLWorkbook"/> to which the info worksheet is added
        /// </param>
        /// <param name="iteration">
        /// The root <see cref="Iteration"/>
        /// </param>
        /// <param name="specifications">
        /// The <see cref="RequirementsSpecification"/>s that need to be taken into account for report generation
        /// </param>
        private void AddInfoSheet(XLWorkbook workbook, Iteration iteration, IEnumerable<RequirementsSpecification> specifications)
        {
            var engineeringModel = iteration.Container as EngineeringModel;
            var engineeringModelSetup = engineeringModel.EngineeringModelSetup;
            var iterationSetup = iteration.IterationSetup;

            this.logger.LogDebug("Add info sheet");

            var infoWorksheet = workbook.Worksheets.Add("BSMI Info");

            infoWorksheet.Cell(1, 1).Value = "BSMI Reporting";
            infoWorksheet.Cell(1, 2).Value = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

            infoWorksheet.Cell(2, 1).Value = "Generation Date";
            infoWorksheet.Cell(2, 2).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            infoWorksheet.Cell(3, 1).Value = "Model - name";
            infoWorksheet.Cell(3, 2).Value = engineeringModelSetup.Name;

            infoWorksheet.Cell(4, 1).Value = "Model - short name";
            infoWorksheet.Cell(4, 2).Value = engineeringModelSetup.ShortName;

            infoWorksheet.Cell(4, 1).Value = "Model - Definition";
            infoWorksheet.Cell(4, 2).Value = engineeringModelSetup.QueryDefinitionContent();

            infoWorksheet.Cell(5, 1).Value = "Iteration - nr";
            infoWorksheet.Cell(5, 2).Value = iterationSetup.CreatedOn;

            infoWorksheet.Cell(6, 1).Value = "Iteration - Description";
            infoWorksheet.Cell(6, 2).Value = iterationSetup.Description;

            infoWorksheet.Cell(7, 1).Value = "Requirement Specifications";
            infoWorksheet.Cell(7, 2).Value = !specifications.Any()
                ? "ALL"
                : string.Join(", ", specifications.Select(x => $"{x.Name} [{x.ShortName}]"  ));

            this.FormatSheet(infoWorksheet);
        }

        /// <summary>
        /// Format the provided sheet
        /// </summary>
        /// <param name="worksheet">
        /// The <see cref="IXLWorksheet"/> that is to be formatted
        /// </param>
        private void FormatSheet(IXLWorksheet worksheet)
        {
            try
            {
                worksheet.Rows().AdjustToContents();
                worksheet.Columns().AdjustToContents();
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Problem loading fonts when adjusting to contents");
            }
        }

        /// <summary>
        /// Queries the name of the report type that is generated by the current <see cref="IReportGenerator"/>
        /// </summary>
        /// <returns>
        /// human-readable name of the report type
        /// </returns>
        public string QueryReportType()
        {
            return "Excel";
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
                case ".xlsm":
                    return new Tuple<bool, string>(true, ".xlsm is a supported report extension");
                case ".xltm":
                    return new Tuple<bool, string>(true, ".xltm is a supported report extension");
                case ".xlsx":
                    return new Tuple<bool, string>(true, ".xlsx is a supported report extension");
                case ".xltx":
                    return new Tuple<bool, string>(true, ".xltx is a supported report extension");
                default:
                    return new Tuple<bool, string>(false, $"The Extension of the output file '{outputPath.Extension}' is not supported. Supported extensions are '.xlsx', '.xlsm', '.xltx' and '.xltm'");
            }
        }
    }
}
