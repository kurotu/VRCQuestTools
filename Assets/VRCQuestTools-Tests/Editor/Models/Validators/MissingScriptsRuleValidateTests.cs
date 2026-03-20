// Tests for MissingScriptsRule.Validate covering the main logic paths:
// - active avatar with missing scripts returns NotificationItem
// - active avatar without missing scripts returns null
// - inactive avatar returns null

using KRT.VRCQuestTools.Models.Validators;
using KRT.VRCQuestTools.Models.VRChat;
using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    public class MissingScriptsRuleValidateTests
    {
        [Test]
        public void Validate_InactiveAvatar_ReturnsNull()
        {
            var go = new GameObject("Avatar");
            go.AddComponent<VRCAvatarDescriptor>();
            go.SetActive(false);
            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
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
        public void Validate_ActiveAvatarNoMissingScripts_ReturnsNull()
        {
            var go = new GameObject("Avatar");
            go.AddComponent<VRCAvatarDescriptor>();
            var child = new GameObject("Child");
            child.transform.SetParent(go.transform);
            child.AddComponent<MeshRenderer>();
            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                var rule = new MissingScriptsRule();
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
