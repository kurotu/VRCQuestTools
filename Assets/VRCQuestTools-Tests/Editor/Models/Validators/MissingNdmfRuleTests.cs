// <copyright file="MissingNdmfRuleTests.cs" company="kurotu">
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
    internal class MissingNdmfRuleTests
    {
        [Test]
        public void Validate_AvatarWithoutNdmfComponents_ReturnsNull()
        {
            var go = new GameObject("Avatar");
            var descriptor = go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                var rule = new MissingNdmfRule();
                var avatar = new VRChatAvatar(descriptor);
                var result = rule.Validate(avatar);
                // When VQT_HAS_NDMF is defined, always returns null
                // When VQT_HAS_NDMF is not defined and no INdmfComponent, returns null
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
