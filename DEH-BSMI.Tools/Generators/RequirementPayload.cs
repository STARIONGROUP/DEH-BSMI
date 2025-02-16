// -------------------------------------------------------------------------------------------------
//  <copyright file="RequirementPayload.cs" company="Starion Group S.A.">
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
    using System.Collections.Generic;

    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// The purpose of the <see cref="RequirementPayload"/> class is to combine <see cref="Requirement"/>s,
    /// <see cref="BinaryRelationship"/>s and <see cref="NestedElement"/>s
    /// </summary>
    public class RequirementPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementPayload"/>
        /// </summary>
        public RequirementPayload(Requirement requirement)
        {
            this.Requirement = requirement;

            this.NestedElement = new List<NestedElement>();
            this.BinaryRelationships = new List<BinaryRelationship>();
        }

        /// <summary>
        /// Gets or sets the value of the BSMI parameter
        /// </summary>
        public string Bsmi { get; set; }

        /// <summary>
        /// The <see cref="Requirement"/>
        /// </summary>
        public Requirement Requirement { get; private set; }

        /// <summary>
        /// A list of <see cref="NestedElement"/>s that are related to this <see cref="Requirement"/>
        /// </summary>
        public List<NestedElement> NestedElement { get; private set; }

        /// <summary>
        /// A list of <see cref="BinaryRelationship"/> that have this requirement either as source or target
        /// </summary>
        public List<BinaryRelationship> BinaryRelationships { get; private set; }
    }
}
