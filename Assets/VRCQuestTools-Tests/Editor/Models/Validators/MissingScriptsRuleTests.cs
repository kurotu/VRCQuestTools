// <copyright file="MissingScriptsRuleTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models.Validators;
using KRT.VRCQuestTools.Models.VRChat;
using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC_AvatarDescriptor = VRC.SDKBase.VRC_AvatarDescriptor;

namespace KRT.VRCQuestTools.Tests.Models.Validators
{
    [TestFixture]
    internal class MissingScriptsRuleTests
    {
        [Test]
        public void Validate_CleanAvatar_ReturnsNull()
        {
            var go = new GameObject("CleanAvatar");
            go.SetActive(true);
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                var rule = new MissingScriptsRule();
                var avatar = new VRChatAvatar(descriptor);
                var result = rule.Validate(avatar);
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Validate_InactiveAvatar_ReturnsNull()
        {
            var go = new GameObject("InactiveAvatar");
            go.SetActive(false);
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                var rule = new MissingScriptsRule();
                var avatar = new VRChatAvatar(descriptor);
                var result = rule.Validate(avatar);
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
