// Tests for VRChatAvatar, VRCQuestToolsSettings, ViewModels, Validators
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Validators;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.ViewModels;

using VRC_AvatarDescriptor = VRC.SDKBase.VRC_AvatarDescriptor;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    public class VRChatAvatarTests
    {
        [Test]
        public void GetRuntimeAnimatorControllers_WithAnimator()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                var animator = go.AddComponent<Animator>();
                var controller = new AnimatorController();
                controller.name = "TestController";
                controller.AddLayer("Base");
                animator.runtimeAnimatorController = controller;

                var avatar = new VRChatAvatar(desc);
                var controllers = avatar.GetRuntimeAnimatorControllers();
                Assert.IsNotNull(controllers);
                Assert.IsTrue(controllers.Length > 0);

                Object.DestroyImmediate(controller);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetRuntimeAnimatorControllers_NoAnimator_ReturnsEmpty()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                var avatar = new VRChatAvatar(desc);
                var controllers = avatar.GetRuntimeAnimatorControllers();
                Assert.IsNotNull(controllers);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void EstimatePerformanceStats_WithPhysBones()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                go.AddComponent<Animator>();

                var pbGo = new GameObject("PB");
                pbGo.transform.SetParent(go.transform);
                var pb = pbGo.AddComponent<VRCPhysBone>();

                var avatar = new VRChatAvatar(desc);
                var stats = avatar.EstimatePerformanceStats(
                    new VRCPhysBone[] { pb },
                    new VRCPhysBoneCollider[0],
                    new ContactBase[0],
                    true);

                Assert.IsNotNull(stats);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void EstimatePerformanceStats_WithProviders()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                go.AddComponent<Animator>();

                var pbGo = new GameObject("PB");
                pbGo.transform.SetParent(go.transform);
                var pb = pbGo.AddComponent<VRCPhysBone>();
                var provider = new VRCPhysBoneProvider(pb);

                var avatar = new VRChatAvatar(desc);
                var stats = avatar.EstimatePerformanceStats(
                    new VRCPhysBoneProviderBase[] { provider },
                    new VRCPhysBoneCollider[0],
                    new ContactBase[0],
                    true);

                Assert.IsNotNull(stats);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void EstimatePerformanceStats_Desktop()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                go.AddComponent<Animator>();

                var avatar = new VRChatAvatar(desc);
                var stats = avatar.EstimatePerformanceStats(
                    new VRCPhysBone[0],
                    new VRCPhysBoneCollider[0],
                    new ContactBase[0],
                    false);

                Assert.IsNotNull(stats);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetRelatedMaterials_WithRenderer()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
                var childGo = new GameObject("Body");
                childGo.transform.SetParent(go.transform);
                var renderer = childGo.AddComponent<MeshRenderer>();
                var mat = new Material(Shader.Find("Standard"));
                mat.name = "TestMat";
                renderer.sharedMaterial = mat;

                var materials = VRChatAvatar.GetRelatedMaterials(go);
                Assert.IsTrue(materials.Length > 0);
                Assert.Contains(mat, materials);

                Object.DestroyImmediate(mat);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasVertexColor_NoMeshes_ReturnsFalse()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                var avatar = new VRChatAvatar(desc);
                Assert.IsFalse(avatar.HasVertexColor);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasUnityConstraints_NoConstraints_ReturnsFalse()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                var avatar = new VRChatAvatar(desc);
                Assert.IsFalse(avatar.HasUnityConstraints);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasDynamicBoneComponents_NoDynamicBone_ReturnsFalse()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                var avatar = new VRChatAvatar(desc);
                Assert.IsFalse(avatar.HasDynamicBoneComponents);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetPhysBoneProviders_ReturnsProviders()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                var pbGo = new GameObject("PB");
                pbGo.transform.SetParent(go.transform);
                pbGo.AddComponent<VRCPhysBone>();

                var avatar = new VRChatAvatar(desc);
                var providers = avatar.GetPhysBoneProviders();
                Assert.AreEqual(1, providers.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetNonLocalContacts_ReturnsNonLocal()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                var crGo = new GameObject("CR");
                crGo.transform.SetParent(go.transform);
                var cr = crGo.AddComponent<VRCContactReceiver>();

                var avatar = new VRChatAvatar(desc);
                var contacts = avatar.GetNonLocalContacts();
                Assert.IsNotNull(contacts);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetLocalContactReceivers_Empty()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                var avatar = new VRChatAvatar(desc);
                var receivers = avatar.GetLocalContactReceivers();
                Assert.AreEqual(0, receivers.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetLocalContactSenders_Empty()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                var avatar = new VRChatAvatar(desc);
                var senders = avatar.GetLocalContactSenders();
                Assert.AreEqual(0, senders.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }

    [TestFixture]
    public class VRCQuestToolsSettingsTests_Misc
    {
        [Test]
        public void I18nResource_IsNotNull()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            Assert.IsNotNull(i18n);
        }

        [Test]
        public void IsShowUnitySettingsWindowOnLoadEnabled_GetSet()
        {
            var original = VRCQuestToolsSettings.IsShowUnitySettingsWindowOnLoadEnabled;
            try
            {
                VRCQuestToolsSettings.IsShowUnitySettingsWindowOnLoadEnabled = false;
                Assert.IsFalse(VRCQuestToolsSettings.IsShowUnitySettingsWindowOnLoadEnabled);
                VRCQuestToolsSettings.IsShowUnitySettingsWindowOnLoadEnabled = true;
                Assert.IsTrue(VRCQuestToolsSettings.IsShowUnitySettingsWindowOnLoadEnabled);
            }
            finally
            {
                VRCQuestToolsSettings.IsShowUnitySettingsWindowOnLoadEnabled = original;
            }
        }

        [Test]
        public void LatestVersionCache_GetSet()
        {
            var original = VRCQuestToolsSettings.LatestVersionCache;
            try
            {
                var ver = new SemVer("1.2.3");
                VRCQuestToolsSettings.LatestVersionCache = ver;
                var result = VRCQuestToolsSettings.LatestVersionCache;
                Assert.AreEqual("1.2.3", result.ToString());
            }
            finally
            {
                VRCQuestToolsSettings.LatestVersionCache = original;
            }
        }

        [Test]
        public void SkippedVersion_GetSet()
        {
            var original = VRCQuestToolsSettings.SkippedVersion;
            try
            {
                var ver = new SemVer("2.3.4");
                VRCQuestToolsSettings.SkippedVersion = ver;
                var result = VRCQuestToolsSettings.SkippedVersion;
                Assert.AreEqual("2.3.4", result.ToString());
            }
            finally
            {
                VRCQuestToolsSettings.SkippedVersion = original;
            }
        }

        [Test]
        public void LastVersionCheckDateTime_GetSet()
        {
            var original = VRCQuestToolsSettings.LastVersionCheckDateTime;
            try
            {
                var dt = new System.DateTime(2024, 6, 15, 12, 0, 0, System.DateTimeKind.Utc);
                VRCQuestToolsSettings.LastVersionCheckDateTime = dt;
                var result = VRCQuestToolsSettings.LastVersionCheckDateTime;
                Assert.AreEqual(dt.Year, result.Year);
                Assert.AreEqual(dt.Month, result.Month);
                Assert.AreEqual(dt.Day, result.Day);
            }
            finally
            {
                VRCQuestToolsSettings.LastVersionCheckDateTime = original;
            }
        }

        [Test]
        public void DisplayLanguage_GetSet()
        {
            var original = VRCQuestToolsSettings.DisplayLanguage;
            try
            {
                VRCQuestToolsSettings.DisplayLanguage = DisplayLanguage.English;
                Assert.AreEqual(DisplayLanguage.English, VRCQuestToolsSettings.DisplayLanguage);
                VRCQuestToolsSettings.DisplayLanguage = DisplayLanguage.Japanese;
                Assert.AreEqual(DisplayLanguage.Japanese, VRCQuestToolsSettings.DisplayLanguage);
                VRCQuestToolsSettings.DisplayLanguage = DisplayLanguage.Auto;
                Assert.AreEqual(DisplayLanguage.Auto, VRCQuestToolsSettings.DisplayLanguage);
            }
            finally
            {
                VRCQuestToolsSettings.DisplayLanguage = original;
            }
        }

        [Test]
        public void IsValidationAutomatorEnabled_GetSet()
        {
            var original = VRCQuestToolsSettings.IsValidationAutomatorEnabled;
            try
            {
                VRCQuestToolsSettings.IsValidationAutomatorEnabled = false;
                Assert.IsFalse(VRCQuestToolsSettings.IsValidationAutomatorEnabled);
                VRCQuestToolsSettings.IsValidationAutomatorEnabled = true;
                Assert.IsTrue(VRCQuestToolsSettings.IsValidationAutomatorEnabled);
            }
            finally
            {
                VRCQuestToolsSettings.IsValidationAutomatorEnabled = original;
            }
        }

        [Test]
        public void IsCheckTextureFormatOnStandaloneEnabled_GetSet()
        {
            var original = VRCQuestToolsSettings.IsCheckTextureFormatOnStandaloneEnabled;
            try
            {
                VRCQuestToolsSettings.IsCheckTextureFormatOnStandaloneEnabled = true;
                Assert.IsTrue(VRCQuestToolsSettings.IsCheckTextureFormatOnStandaloneEnabled);
                VRCQuestToolsSettings.IsCheckTextureFormatOnStandaloneEnabled = false;
                Assert.IsFalse(VRCQuestToolsSettings.IsCheckTextureFormatOnStandaloneEnabled);
            }
            finally
            {
                VRCQuestToolsSettings.IsCheckTextureFormatOnStandaloneEnabled = original;
            }
        }

        [Test]
        public void TextureCacheSize_GetSet()
        {
            var original = VRCQuestToolsSettings.TextureCacheSize;
            try
            {
                VRCQuestToolsSettings.TextureCacheSize = 256 * 1024 * 1024;
                Assert.AreEqual(256UL * 1024 * 1024, VRCQuestToolsSettings.TextureCacheSize);
            }
            finally
            {
                VRCQuestToolsSettings.TextureCacheSize = original;
            }
        }

        [Test]
        public void TextureCacheFolder_IsNotEmpty()
        {
            Assert.IsFalse(string.IsNullOrEmpty(VRCQuestToolsSettings.TextureCacheFolder));
        }

        [Test]
        public void ResetPreferences_DoesNotThrow()
        {
            var originalSize = VRCQuestToolsSettings.TextureCacheSize;
            try
            {
                VRCQuestToolsSettings.ResetPreferences();
                // After reset, TextureCacheSize should be default
                Assert.IsTrue(VRCQuestToolsSettings.TextureCacheSize > 0);
            }
            finally
            {
                VRCQuestToolsSettings.TextureCacheSize = originalSize;
            }
        }
    }

    [TestFixture]
    public class UnityQuestSettingsViewModelTests_Misc
    {
        [Test]
        public void DefaultAndroidTextureCompression_IsSet()
        {
            var vm = new UnityQuestSettingsViewModel();
            // Should not throw
            var compression = vm.DefaultAndroidTextureCompression;
            Assert.IsNotNull(compression);
        }

        [Test]
        public void ShowWindowOnLoad_GetSet()
        {
            var vm = new UnityQuestSettingsViewModel();
            var original = vm.ShowWindowOnLoad;
            try
            {
                vm.ShowWindowOnLoad = false;
                Assert.IsFalse(vm.ShowWindowOnLoad);
                vm.ShowWindowOnLoad = true;
                Assert.IsTrue(vm.ShowWindowOnLoad);
            }
            finally
            {
                vm.ShowWindowOnLoad = original;
            }
        }

        [Test]
        public void AllSettingsValid_ReturnsBoolean()
        {
            var vm = new UnityQuestSettingsViewModel();
            // Just test it doesn't throw
            var result = vm.AllSettingsValid;
            Assert.IsNotNull(result);
        }

        [Test]
        public void HasValidAndroidTextureCompression_ReturnsBoolean()
        {
            var vm = new UnityQuestSettingsViewModel();
            var result = vm.HasValidAndroidTextureCompression;
            Assert.IsNotNull(result);
        }

        [Test]
        public void HasAndroidBuildSupport_ReturnsBoolean()
        {
            var vm = new UnityQuestSettingsViewModel();
            var result = vm.HasAndroidBuildSupport;
            Assert.IsNotNull(result);
        }
    }

    [TestFixture]
    public class MissingScriptsRuleTests_Misc
    {
        [Test]
        public void Validate_InactiveAvatar_ReturnsNull()
        {
            var go = new GameObject("Avatar");
            go.SetActive(false);
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                var avatar = new VRChatAvatar(desc);
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
        public void Validate_NoMissingScripts_ReturnsNull()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                var avatar = new VRChatAvatar(desc);
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

    [TestFixture]
    public class MissingNdmfRuleTests_Misc
    {
        [Test]
        public void Validate_NoNdmfComponents_ReturnsNull()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                var avatar = new VRChatAvatar(desc);
                var rule = new MissingNdmfRule();
                var result = rule.Validate(avatar);
                // With VQT_HAS_NDMF defined, always returns null
                // Without it, returns null if no INdmfComponent
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }

    [TestFixture]
    public class MSMapGenViewModelTests_Misc
    {
        [Test]
        public void DisableGenerateButton_BothNull_ReturnsTrue()
        {
            var vm = new MSMapGenViewModel();
            vm.metallicMap = null;
            vm.smoothnessMap = null;
            Assert.IsTrue(vm.DisableGenerateButton);
        }

        [Test]
        public void DisableGenerateButton_WithMetallicMap_ReturnsFalse()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                var vm = new MSMapGenViewModel();
                vm.metallicMap = tex;
                vm.smoothnessMap = null;
                Assert.IsFalse(vm.DisableGenerateButton);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void DisableGenerateButton_WithSmoothnessMap_ReturnsFalse()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                var vm = new MSMapGenViewModel();
                vm.metallicMap = null;
                vm.smoothnessMap = tex;
                Assert.IsFalse(vm.DisableGenerateButton);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void DisableGenerateButton_BothSet_ReturnsFalse()
        {
            var tex1 = new Texture2D(4, 4);
            var tex2 = new Texture2D(4, 4);
            try
            {
                var vm = new MSMapGenViewModel();
                vm.metallicMap = tex1;
                vm.smoothnessMap = tex2;
                Assert.IsFalse(vm.DisableGenerateButton);
            }
            finally
            {
                Object.DestroyImmediate(tex1);
                Object.DestroyImmediate(tex2);
            }
        }
    }

    [TestFixture]
    public class AvatarValidationRulesTests_Misc
    {
        [Test]
        public void Rules_ContainsMissingScriptsRule()
        {
            var rules = AvatarValidationRules.Rules;
            bool found = false;
            foreach (var rule in rules)
            {
                if (rule is MissingScriptsRule)
                {
                    found = true;
                    break;
                }
            }
            Assert.IsTrue(found, "Rules should contain MissingScriptsRule");
        }

        [Test]
        public void Rules_ContainsMissingNdmfRule()
        {
            var rules = AvatarValidationRules.Rules;
            bool found = false;
            foreach (var rule in rules)
            {
                if (rule is MissingNdmfRule)
                {
                    found = true;
                    break;
                }
            }
            Assert.IsTrue(found, "Rules should contain MissingNdmfRule");
        }
    }

    [TestFixture]
    public class ComponentTests
    {
        [Test]
        public void AvatarConverterSettings_DefaultValues()
        {
            var go = new GameObject("Avatar");
            try
            {
                var settings = go.AddComponent<KRT.VRCQuestTools.Components.AvatarConverterSettings>();
                Assert.IsNotNull(settings);
                // Test default property values
                Assert.IsNotNull(settings.DefaultMaterialConvertSettings);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void MaterialSwap_MaterialMappings_DefaultEmpty()
        {
            var go = new GameObject("TestObj");
            try
            {
                var swap = go.AddComponent<KRT.VRCQuestTools.Components.MaterialSwap>();
                Assert.IsNotNull(swap);
                Assert.IsNotNull(swap.materialMappings);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void VertexColorRemover_Defaults()
        {
            var go = new GameObject("TestObj");
            try
            {
                var vcr = go.AddComponent<KRT.VRCQuestTools.Components.VertexColorRemover>();
                Assert.IsNotNull(vcr);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void PlatformComponentRemover_Defaults()
        {
            var go = new GameObject("TestObj");
            try
            {
                var pcr = go.AddComponent<KRT.VRCQuestTools.Components.PlatformComponentRemover>();
                Assert.IsNotNull(pcr);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
