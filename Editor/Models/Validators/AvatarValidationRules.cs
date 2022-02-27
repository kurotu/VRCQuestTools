// <copyright file="AvatarValidationRules.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;

namespace KRT.VRCQuestTools.Models.Validators
{
    /// <summary>
    /// Rules container.
    /// </summary>
    internal class AvatarValidationRules
    {
        private static List<IAvatarValidationRule> rules = new List<IAvatarValidationRule>();

        /// <summary>
        /// Gets registered rules.
        /// </summary>
        internal static IAvatarValidationRule[] Rules => rules.ToArray();

        /// <summary>
        /// Add a rule.
        /// </summary>
        /// <param name="rule">Rule to add.</param>
        internal static void Add(IAvatarValidationRule rule)
        {
            rules.Add(rule);
        }
    }
}
