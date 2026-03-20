// <copyright file="AvatarValidationRulesTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models.Validators;
using NUnit.Framework;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="AvatarValidationRules"/>.
    /// </summary>
    public class AvatarValidationRulesTests
    {
        [Test]
        public void Rules_ReturnsNonNullArray()
        {
            var rules = AvatarValidationRules.Rules;
            Assert.IsNotNull(rules);
        }

        [Test]
        public void Rules_ContainsRegisteredRules()
        {
            // InitializeOnLoadMethod should have registered MissingNdmfRule and MissingScriptsRule
            var rules = AvatarValidationRules.Rules;
            Assert.Greater(rules.Length, 0);
        }

        [Test]
        public void Rules_ReturnsNewArrayEachTime()
        {
            var rules1 = AvatarValidationRules.Rules;
            var rules2 = AvatarValidationRules.Rules;
            Assert.AreNotSame(rules1, rules2);
        }
    }
}
