// Tests for MissingNdmfRule and MissingScriptsRule
using KRT.VRCQuestTools.Models.Validators;
using KRT.VRCQuestTools.Models.VRChat;
using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    internal class ValidationRulesTests
    {
        private VRChatAvatar CreateAvatar(GameObject go)
        {
            return new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
        }

        [Test]
        public void MissingScriptsRule_ValidAvatar_ReturnsNull()
        {
            var go = new GameObject("AvatarRoot");
            go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                var avatar = CreateAvatar(go);
                var rule = new MissingScriptsRule();
                var result = rule.Validate(avatar);
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void MissingScriptsRule_InactiveAvatar_ReturnsNull()
        {
            var go = new GameObject("AvatarRoot");
            go.AddComponent<VRCAvatarDescriptor>();
            go.SetActive(false);
            try
            {
                var avatar = CreateAvatar(go);
                var rule = new MissingScriptsRule();
                var result = rule.Validate(avatar);
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void MissingNdmfRule_ValidAvatar_ReturnsNull()
        {
            var go = new GameObject("AvatarRoot");
            go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                var avatar = CreateAvatar(go);
                var rule = new MissingNdmfRule();
                var result = rule.Validate(avatar);
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void MissingNdmfRule_InactiveAvatar_ReturnsNull()
        {
            var go = new GameObject("AvatarRoot");
            go.AddComponent<VRCAvatarDescriptor>();
            go.SetActive(false);
            try
            {
                var avatar = CreateAvatar(go);
                var rule = new MissingNdmfRule();
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
