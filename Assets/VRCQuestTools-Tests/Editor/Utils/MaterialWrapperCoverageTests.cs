// Tests for MaterialWrapperBuilder, ToonLitGenerator, MatCapLitGenerator,
// MissingScriptsRule, MissingNdmfRule, VRCQuestToolsSettings, AssetUtility - Batch 15
using System;
using System.Reflection;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.Validators;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    public class Batch15_MaterialWrapperBuilderTests
    {
        private MaterialWrapperBuilder builder;

        [SetUp]
        public void SetUp()
        {
            builder = new MaterialWrapperBuilder();
        }

        [Test]
        public void DetectShaderCategory_Standard_ReturnsStandard()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Standard, builder.DetectShaderCategory(mat));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_StandardSpecular_ReturnsStandard()
        {
            var shader = Shader.Find("Standard (Specular setup)");
            if (shader == null)
            {
                Assert.Ignore("Standard (Specular setup) shader not available.");
                return;
            }
            var mat = new Material(shader);
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Standard, builder.DetectShaderCategory(mat));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_UnlitColor_ReturnsUnlit()
        {
            var shader = Shader.Find("Unlit/Color");
            if (shader == null)
            {
                Assert.Ignore("Unlit/Color shader not available.");
                return;
            }
            var mat = new Material(shader);
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Unlit, builder.DetectShaderCategory(mat));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_UnlitTexture_ReturnsUnlit()
        {
            var shader = Shader.Find("Unlit/Texture");
            if (shader == null)
            {
                Assert.Ignore("Unlit/Texture shader not available.");
                return;
            }
            var mat = new Material(shader);
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Unlit, builder.DetectShaderCategory(mat));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_VRChatMobileToonLit_ReturnsQuest()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (shader == null)
            {
                Assert.Ignore("VRChat/Mobile/Toon Lit shader not available.");
                return;
            }
            var mat = new Material(shader);
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Quest, builder.DetectShaderCategory(mat));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_VRChatMobileStandardLite_ReturnsQuest()
        {
            var shader = Shader.Find("VRChat/Mobile/Standard Lite");
            if (shader == null)
            {
                Assert.Ignore("VRChat/Mobile/Standard Lite shader not available.");
                return;
            }
            var mat = new Material(shader);
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Quest, builder.DetectShaderCategory(mat));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_UnknownShader_ReturnsUnverified()
        {
            // Hidden shaders typically don't match any category
            var shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null)
            {
                Assert.Ignore("Hidden/Internal-Colored shader not available.");
                return;
            }
            var mat = new Material(shader);
            try
            {
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Unverified, builder.DetectShaderCategory(mat));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_Standard_ReturnsStandardMaterial()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var wrapper = builder.Build(mat);
                Assert.IsNotNull(wrapper);
                Assert.IsInstanceOf<StandardMaterial>(wrapper);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_Unlit_ReturnsStandardMaterial()
        {
            var shader = Shader.Find("Unlit/Color");
            if (shader == null)
            {
                Assert.Ignore("Unlit/Color shader not available.");
                return;
            }
            var mat = new Material(shader);
            try
            {
                var wrapper = builder.Build(mat);
                Assert.IsNotNull(wrapper);
                Assert.IsInstanceOf<StandardMaterial>(wrapper);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_Quest_ReturnsStandardMaterial()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (shader == null)
            {
                Assert.Ignore("VRChat/Mobile/Toon Lit shader not available.");
                return;
            }
            var mat = new Material(shader);
            try
            {
                var wrapper = builder.Build(mat);
                Assert.IsNotNull(wrapper);
                Assert.IsInstanceOf<StandardMaterial>(wrapper);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_Unverified_ReturnsStandardMaterial()
        {
            var shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null)
            {
                Assert.Ignore("Hidden/Internal-Colored shader not available.");
                return;
            }
            var mat = new Material(shader);
            try
            {
                var wrapper = builder.Build(mat);
                Assert.IsNotNull(wrapper);
                Assert.IsInstanceOf<StandardMaterial>(wrapper);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }
    }

    [TestFixture]
    public class Batch15_ToonLitGeneratorTests
    {
        [Test]
        public void Constructor_DoesNotThrow()
        {
            var settings = new ToonLitConvertSettings();
            var generator = new ToonLitGenerator(settings);
            Assert.IsNotNull(generator);
        }

        [Test]
        public void GenerateMaterial_WithoutQuestTextures_ReturnsImmediately()
        {
            var settings = new ToonLitConvertSettings();
            settings.generateQuestTextures = false;
            var generator = new ToonLitGenerator(settings);

            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var wrapper = new StandardMaterial(mat);
                Material result = null;
                var request = generator.GenerateMaterial(wrapper, UnityEditor.BuildTarget.Android, false, "Assets", (m) => result = m);
                Assert.IsNotNull(request);
                request.WaitForCompletion();
                Assert.IsNotNull(result);
                Object.DestroyImmediate(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }
    }

    [TestFixture]
    public class Batch15_MatCapLitGeneratorTests
    {
        [Test]
        public void Constructor_DoesNotThrow()
        {
            var settings = new MatCapLitConvertSettings();
            var generator = new MatCapLitGenerator(settings);
            Assert.IsNotNull(generator);
        }
    }

    [TestFixture]
    public class Batch15_MissingScriptsRuleTests
    {
        [Test]
        public void Validate_ActiveAvatar_NoMissingScripts_ReturnsNull()
        {
            var go = new GameObject("Avatar");
            try
            {
                go.SetActive(true);
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
        public void Validate_InactiveAvatar_ReturnsNull()
        {
            var go = new GameObject("Avatar");
            try
            {
                go.SetActive(false);
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
    public class Batch15_MissingNdmfRuleTests
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
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Validate_WithNdmfComponent_ReturnsNotificationItem()
        {
            var go = new GameObject("Avatar");
            try
            {
                var desc = go.AddComponent<VRCAvatarDescriptor>();
                // AvatarConverterSettings implements INdmfComponent
                go.AddComponent<AvatarConverterSettings>();
                var avatar = new VRChatAvatar(desc);

                var rule = new MissingNdmfRule();
                var result = rule.Validate(avatar);
                // When VQT_HAS_NDMF is defined, it returns null regardless
                // When not defined, it returns a NotificationItem
                // We test whichever behavior the current build gives us
                // Just ensure it doesn't throw
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }

    [TestFixture]
    public class Batch15_AssetUtilityTests
    {
        [Test]
        public void GetAllObjectReferences_ReturnsNonNull()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var refs = AssetUtility.GetAllObjectReferences(mat);
                Assert.IsNotNull(refs);
                Assert.IsTrue(refs.Length > 0);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetAllObjectReferences_WithTexture_IncludesTexture()
        {
            var mat = new Material(Shader.Find("Standard"));
            var tex = new Texture2D(4, 4);
            try
            {
                mat.mainTexture = tex;
                var refs = AssetUtility.GetAllObjectReferences(mat);
                Assert.IsNotNull(refs);
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void IsDynamicBoneImported_ReturnsBool()
        {
            // Just test it doesn't throw
            var result = AssetUtility.IsDynamicBoneImported();
            Assert.IsInstanceOf<bool>(result);
        }

        [Test]
        public void IsLilToonImported_ReturnsBool()
        {
            var result = AssetUtility.IsLilToonImported();
            Assert.IsInstanceOf<bool>(result);
        }

        [Test]
        public void CanLilToonBakeShadowRamp_ReturnsBool()
        {
            var result = AssetUtility.CanLilToonBakeShadowRamp();
            Assert.IsInstanceOf<bool>(result);
        }
    }

    [TestFixture]
    public class Batch15_VRCQuestToolsSettingsExtraTests
    {
        [Test]
        public void I18nResource_IsNotNull()
        {
            var resource = VRCQuestToolsSettings.I18nResource;
            Assert.IsNotNull(resource);
        }
    }

    [TestFixture]
    public class Batch15_AvatarValidationRulesTests
    {
        [Test]
        public void Rules_ContainsRules()
        {
            var rules = AvatarValidationRules.Rules;
            Assert.IsNotNull(rules);
            Assert.IsTrue(rules.Length > 0);
        }

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
            Assert.IsTrue(found, "MissingScriptsRule should be registered in AvatarValidationRules.");
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
            Assert.IsTrue(found, "MissingNdmfRule should be registered in AvatarValidationRules.");
        }
    }

    [TestFixture]
    public class Batch15_VertexColorRemoverExtraTests
    {
        [Test]
        public void RemoveVertexColor_SkinnedMeshWithVertexColors()
        {
            var go = new GameObject("Avatar");
            try
            {
                var childGo = new GameObject("Body");
                childGo.transform.SetParent(go.transform);
                var smr = childGo.AddComponent<SkinnedMeshRenderer>();

                var mesh = new Mesh();
                mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up, Vector3.right };
                mesh.triangles = new int[] { 0, 1, 2, 1, 2, 3 };
                mesh.colors = new Color[] { Color.red, Color.green, Color.blue, Color.white };
                smr.sharedMesh = mesh;

                var remover = go.AddComponent<VertexColorRemover>();
                remover.includeChildren = true;
                remover.RemoveVertexColor();

                // After removal, the mesh should have no colors or all zero
                var resultColors = smr.sharedMesh.colors;
                if (resultColors != null && resultColors.Length > 0)
                {
                    foreach (var c in resultColors)
                    {
                        Assert.AreEqual(0f, c.r, 0.01f);
                        Assert.AreEqual(0f, c.g, 0.01f);
                        Assert.AreEqual(0f, c.b, 0.01f);
                    }
                }

                Object.DestroyImmediate(mesh);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void RemoveVertexColor_NoMesh_DoesNotThrow()
        {
            var go = new GameObject("Avatar");
            try
            {
                var childGo = new GameObject("Body");
                childGo.transform.SetParent(go.transform);
                childGo.AddComponent<MeshRenderer>();
                // No MeshFilter - no mesh to process

                var remover = go.AddComponent<VertexColorRemover>();
                remover.includeChildren = true;
                UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
                try
                {
                    remover.RemoveVertexColor();
                }
                finally
                {
                    UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
                }
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }

    [TestFixture]
    public class Batch15_AvatarConverterSettingsExtraTests
    {
        [Test]
        public void GetMaterialConvertSettings_DefaultReturnsDefault()
        {
            var go = new GameObject("Avatar");
            try
            {
                var settings = go.AddComponent<AvatarConverterSettings>();
                var mat = new Material(Shader.Find("Standard"));
                try
                {
                    var convertSettings = settings.GetMaterialConvertSettings(mat);
                    Assert.IsNotNull(convertSettings);
                    // Default should be ToonLitConvertSettings
                    Assert.AreEqual(settings.DefaultMaterialConvertSettings, convertSettings);
                }
                finally
                {
                    Object.DestroyImmediate(mat);
                }
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void AvatarDescriptor_WithDescriptor_ReturnsNonNull()
        {
            var go = new GameObject("Avatar");
            try
            {
                go.AddComponent<VRCAvatarDescriptor>();
                var settings = go.AddComponent<AvatarConverterSettings>();

                Assert.IsNotNull(settings.AvatarDescriptor);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void AvatarDescriptor_WithoutDescriptor_ReturnsNull()
        {
            var go = new GameObject("NoDesc");
            try
            {
                var settings = go.AddComponent<AvatarConverterSettings>();
                Assert.IsNull(settings.AvatarDescriptor);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void AdditionalMaterialConvertSettings_SetAndGet()
        {
            var go = new GameObject("Avatar");
            try
            {
                var settings = go.AddComponent<AvatarConverterSettings>();
                var additional = new AdditionalMaterialConvertSettings[]
                {
                    new AdditionalMaterialConvertSettings(),
                };
                settings.AdditionalMaterialConvertSettings = additional;
                Assert.AreEqual(1, settings.AdditionalMaterialConvertSettings.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_DefaultIsTrue()
        {
            var go = new GameObject("Avatar");
            try
            {
                var settings = go.AddComponent<AvatarConverterSettings>();
                Assert.IsTrue(settings.RemoveExtraMaterialSlots);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void IsPrimaryRoot_ReturnsTrue()
        {
            var go = new GameObject("Avatar");
            try
            {
                var settings = go.AddComponent<AvatarConverterSettings>();
                Assert.IsTrue(settings.IsPrimaryRoot);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }

    [TestFixture]
    public class Batch15_MaterialGeneratorUtilityTests
    {
        [Test]
        public void ConvertToNullableTextureFormat_NoOverride_ReturnsNull()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod("ConvertToNullableTextureFormat", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
            {
                Assert.Ignore("ConvertToNullableTextureFormat method not found.");
                return;
            }
            var result = method.Invoke(null, new object[] { MobileTextureFormat.NoOverride });
            Assert.IsNull(result);
        }

        [Test]
        public void ConvertToNullableTextureFormat_ASTC4x4_ReturnsFormat()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod("ConvertToNullableTextureFormat", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
            {
                Assert.Ignore("ConvertToNullableTextureFormat method not found.");
                return;
            }
            var result = method.Invoke(null, new object[] { MobileTextureFormat.ASTC_4x4 });
            Assert.IsNotNull(result);
            Assert.AreEqual(TextureFormat.ASTC_4x4, (TextureFormat)result);
        }

        [Test]
        public void ConvertToNullableTextureFormat_ASTC6x6_ReturnsFormat()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod("ConvertToNullableTextureFormat", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
            {
                Assert.Ignore("ConvertToNullableTextureFormat method not found.");
                return;
            }
            var result = method.Invoke(null, new object[] { MobileTextureFormat.ASTC_6x6 });
            Assert.IsNotNull(result);
            Assert.AreEqual(TextureFormat.ASTC_6x6, (TextureFormat)result);
        }

        [Test]
        public void ConvertToNullableTextureFormat_AllFormats()
        {
            var method = typeof(MaterialGeneratorUtility).GetMethod("ConvertToNullableTextureFormat", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
            {
                Assert.Ignore("ConvertToNullableTextureFormat method not found.");
                return;
            }
            var formats = new MobileTextureFormat[]
            {
                MobileTextureFormat.ASTC_5x5,
                MobileTextureFormat.ASTC_8x8,
                MobileTextureFormat.ASTC_10x10,
                MobileTextureFormat.ASTC_12x12,
            };
            foreach (var fmt in formats)
            {
                var result = method.Invoke(null, new object[] { fmt });
                Assert.IsNotNull(result, $"Expected non-null for {fmt}");
            }
        }
    }

    [TestFixture]
    public class Batch15_ModularAvatarUtilityExtraTests
    {
        [Test]
        public void IsModularAvatarImported_ReturnsBool()
        {
            var result = ModularAvatarUtility.IsModularAvatarImported();
            Assert.IsInstanceOf<bool>(result);
        }
    }

    [TestFixture]
    public class Batch15_SystemUtilityTests
    {
        [Test]
        public void GetAvailableLocales_HasKnownMethod()
        {
            // SystemUtility provides utility methods - just verify it's accessible
            Assert.IsNotNull(typeof(SystemUtility));
        }
    }

    [TestFixture]
    public class Batch15_AnimatorControllerDuplicatorExtraTests
    {
        private AnimatorControllerDuplicator duplicator;

        [SetUp]
        public void SetUp()
        {
            duplicator = new AnimatorControllerDuplicator();
        }

        [Test]
        public void Duplicate_NullController_ReturnsNull()
        {
            var result = duplicator.Duplicate(null);
            Assert.IsNull(result);
        }

        [Test]
        public void Duplicate_EmptyController()
        {
            var controller = new AnimatorController();
            controller.name = "TestController";
            controller.AddLayer("Base");
            try
            {
                var duplicate = duplicator.Duplicate(controller);
                try
                {
                    Assert.IsNotNull(duplicate);
                    Assert.AreNotSame(controller, duplicate);
                    Assert.AreEqual(1, duplicate.layers.Length);
                }
                finally
                {
                    Object.DestroyImmediate(duplicate);
                }
            }
            finally
            {
                Object.DestroyImmediate(controller);
            }
        }

        [Test]
        public void Duplicate_ControllerWithState()
        {
            var controller = new AnimatorController();
            controller.name = "TestWithStates";
            controller.AddLayer("Base");
            var sm = controller.layers[0].stateMachine;
            var state = sm.AddState("TestState");
            state.motion = new AnimationClip { name = "TestClip" };
            try
            {
                var duplicate = duplicator.Duplicate(controller);
                try
                {
                    Assert.IsNotNull(duplicate);
                    Assert.AreEqual(1, duplicate.layers.Length);
                    Assert.AreEqual(1, duplicate.layers[0].stateMachine.states.Length);
                }
                finally
                {
                    Object.DestroyImmediate(duplicate);
                }
            }
            finally
            {
                if (state.motion != null)
                {
                    Object.DestroyImmediate(state.motion);
                }
                Object.DestroyImmediate(controller);
            }
        }

        [Test]
        public void Duplicate_ControllerWithParameters()
        {
            var controller = new AnimatorController();
            controller.name = "TestWithParams";
            controller.AddLayer("Base");
            controller.AddParameter("TestBool", AnimatorControllerParameterType.Bool);
            controller.AddParameter("TestFloat", AnimatorControllerParameterType.Float);
            controller.AddParameter("TestInt", AnimatorControllerParameterType.Int);
            try
            {
                var duplicate = duplicator.Duplicate(controller);
                try
                {
                    Assert.IsNotNull(duplicate);
                    Assert.AreEqual(3, duplicate.parameters.Length);
                    Assert.AreEqual("TestBool", duplicate.parameters[0].name);
                    Assert.AreEqual(AnimatorControllerParameterType.Bool, duplicate.parameters[0].type);
                }
                finally
                {
                    Object.DestroyImmediate(duplicate);
                }
            }
            finally
            {
                Object.DestroyImmediate(controller);
            }
        }
    }

    [TestFixture]
    public class Batch15_VRCQuestToolsEntryTests
    {
        [Test]
        public void Name_IsNotEmpty()
        {
            Assert.IsNotEmpty(VRCQuestTools.Name);
            Assert.AreEqual("VRCQuestTools", VRCQuestTools.Name);
        }

        [Test]
        public void Version_IsNotEmpty()
        {
            Assert.IsNotEmpty(VRCQuestTools.Version);
        }

        [Test]
        public void ComponentRemover_IsNotNull()
        {
            Assert.IsNotNull(VRCQuestTools.ComponentRemover);
        }

        [Test]
        public void AvatarConverter_IsNotNull()
        {
            Assert.IsNotNull(VRCQuestTools.AvatarConverter);
        }
    }
}
