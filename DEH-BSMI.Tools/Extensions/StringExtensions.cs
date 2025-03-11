// -------------------------------------------------------------------------------------------------
//  <copyright file="StringExtensions.cs" company="Starion Group S.A.">
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

    /// <summary>
    /// Extension methods for strings
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Computes the Object Level based on the input 4 digit input string
        /// </summary>
        /// <param name="input">
        /// a 4 digit input string
        /// </param>
        /// <returns>
        /// the object level
        /// </returns>
        public static string ComputeObjectLevel(this string input)
        {
            // Ensure input is exactly 4 characters
            if (input.Length != 4 || !input.All(char.IsDigit))
                throw new ArgumentException("Input must be a 4-digit numeric string");

            // Count trailing zeros
            int zeroCount = input.Reverse().TakeWhile(c => c == '0').Count();

            // Determine level (1 to 4)
            return (4 - zeroCount).ToString();
        }

        public static string CleanupShortName(this string shortname)
        {
            return shortname.Replace('-', '_').Replace(' ', '_');
        }
    }
}
