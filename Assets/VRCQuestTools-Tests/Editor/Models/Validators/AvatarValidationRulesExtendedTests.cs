// <copyright file="AvatarValidationRulesExtendedTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Linq;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Validators;
using NUnit.Framework;

namespace KRT.VRCQuestTools.Tests.Models.Validators
{
    [TestFixture]
    internal class AvatarValidationRulesExtendedTests
    {
        [Test]
        public void Rules_IsNotNull()
        {
            Assert.IsNotNull(AvatarValidationRules.Rules);
        }

        [Test]
        public void Rules_HasAtLeastOneRule()
        {
            Assert.IsTrue(AvatarValidationRules.Rules.Length > 0, "Should have at least one validation rule registered");
        }

        [Test]
        public void Add_AddsRuleToCollection()
        {
            var initialCount = AvatarValidationRules.Rules.Length;
            var rule = new TestValidationRule();
            AvatarValidationRules.Add(rule);
            // After Add, Rules should include the new one
            Assert.AreEqual(initialCount + 1, AvatarValidationRules.Rules.Length);
            Assert.IsTrue(AvatarValidationRules.Rules.Contains(rule));
            // Note: Cannot remove from array-based Rules, but Add only adds to internal list
            // The test rule remains in the internal list but doesn't cause issues
        }

        private class TestValidationRule : IAvatarValidationRule
        {
            public NotificationItem Validate(KRT.VRCQuestTools.Models.VRChat.VRChatAvatar avatar)
            {
                return null;
            }
        }
    }
}
