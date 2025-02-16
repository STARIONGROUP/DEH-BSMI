// -------------------------------------------------------------------------------------------------
//  <copyright file="DefinedThingExtensions.cs" company="Starion Group S.A.">
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

namespace DEHBSMI.Tools.Extensions
{
    using System;
    using System.Linq;

    using CDP4Common.CommonData;

    /// <summary>
    /// Extension methods for the <see cref="DefinedThing"/> class
    /// </summary>
    public static class DefinedThingExtensions
    {
        /// <summary>
        /// Queries the first <see cref="Definition"/> and returns the content as a sting
        /// </summary>
        /// <param name="definedThing">
        /// The subject <see cref="DefinedThing"/>
        /// </param>
        /// <returns>
        /// The content of the first <see cref="Definition"/> if it exists, an empty string otherwise
        /// </returns>
        public static string QueryDefinitionContent(this DefinedThing definedThing)
        {
            if (definedThing == null)
            {
                throw new ArgumentNullException(nameof(definedThing));
            }

            var content = "";
            var definition = definedThing.Definition.SingleOrDefault();
            if (definition != null)
            {
                content = definition.Content;
            }

            return content;
        }
    }
}
