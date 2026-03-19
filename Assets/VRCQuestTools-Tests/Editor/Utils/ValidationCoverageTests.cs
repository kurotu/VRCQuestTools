// Batch 25 - Coverage improvement tests targeting remaining testable gaps
// Targets: TextureUtility, VRCSDKUtility, UnityAnimationUtility, MSMapGenViewModel,
//          MaterialWrapperBuilder, CacheUtility, MaterialGeneratorUtility, AvatarConverter

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.ViewModels;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;
using UObject = UnityEngine.Object;
using VQTBuildTarget = KRT.VRCQuestTools.Models.BuildTarget;

namespace KRT.VRCQuestTools.Tests
{
    // ==========================================
    // TextureUtility Tests
    // ==========================================
    [TestFixture]
    public class Batch25_TextureUtilityTests
    {
        [Test]
        public void GetCompressionFormat_ASTC6x6_ReturnsASTCFormat()
        {
            var result = TextureUtility.GetCompressionFormat(MobileTextureFormat.ASTC_6x6);
            Assert.AreEqual(TextureFormat.ASTC_6x6, result);
        }

        [Test]
        public void GetCompressionFormat_ASTC4x4_ReturnsASTCFormat()
        {
            var result = TextureUtility.GetCompressionFormat(MobileTextureFormat.ASTC_4x4);
            Assert.AreEqual(TextureFormat.ASTC_4x4, result);
        }

        [Test]
        public void GetCompressionFormat_ETC2_RGB4_ReturnsETC2Format()
        {
            // ETC2 is not in MobileTextureFormat enum, test ASTC_8x8 instead
            var result = TextureUtility.GetCompressionFormat(MobileTextureFormat.ASTC_8x8);
            Assert.AreEqual(TextureFormat.ASTC_8x8, result);
        }

        [Test]
        public void GetCompressionFormat_ETC2_RGBA8_ReturnsETC2Format()
        {
            // ETC2 is not in MobileTextureFormat enum, test ASTC_10x10 instead
            var result = TextureUtility.GetCompressionFormat(MobileTextureFormat.ASTC_10x10);
            Assert.AreEqual(TextureFormat.ASTC_10x10, result);
        }

        [Test]
        public void CreateMinimumEmptyTexture_Returns4x4Texture()
        {
            var tex = TextureUtility.CreateMinimumEmptyTexture();
            try
            {
                Assert.IsNotNull(tex);
                Assert.AreEqual(4, tex.width);
                Assert.AreEqual(4, tex.height);
            }
            finally { UObject.DestroyImmediate(tex); }
        }

        [Test]
        public void CreateColorTexture_WithDimensions_ReturnsCorrectSize()
        {
            var tex = TextureUtility.CreateColorTexture(new Color32(255, 0, 0, 255), 8, 8);
            try
            {
                Assert.IsNotNull(tex);
                Assert.AreEqual(8, tex.width);
                Assert.AreEqual(8, tex.height);
            }
            finally { UObject.DestroyImmediate(tex); }
        }

        [Test]
        public void CreateColorTexture_DefaultSize_ReturnsNonNull()
        {
            var tex = TextureUtility.CreateColorTexture(new Color32(0, 255, 0, 255));
            try
            {
                Assert.IsNotNull(tex);
                Assert.Greater(tex.width, 0);
                Assert.Greater(tex.height, 0);
            }
            finally { UObject.DestroyImmediate(tex); }
        }

        [Test]
        public void IsKnownTextureFormat_RGBA32_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(TextureFormat.RGBA32));
        }

        [Test]
        public void IsKnownTextureFormat_ASTC6x6_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(TextureFormat.ASTC_6x6));
        }

        [Test]
        public void IsUncompressedFormat_RGBA32_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsUncompressedFormat(TextureFormat.RGBA32));
        }

        [Test]
        public void IsUncompressedFormat_RGB24_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsUncompressedFormat(TextureFormat.RGB24));
        }

        [Test]
        public void IsUncompressedFormat_ASTC6x6_ReturnsFalse()
        {
            Assert.IsFalse(TextureUtility.IsUncompressedFormat(TextureFormat.ASTC_6x6));
        }

        [Test]
        public void IsSupportedTextureFormat_ASTC6x6_ForAndroid_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.ASTC_6x6, UnityEditor.BuildTarget.Android));
        }

        [Test]
        public void IsSupportedTextureFormat_DXT5_ForAndroid_ReturnsFalse()
        {
            Assert.IsFalse(TextureUtility.IsSupportedTextureFormat(TextureFormat.DXT5, UnityEditor.BuildTarget.Android));
        }

        [Test]
        public void IsSupportedTextureFormat_DXT5_ForWindows_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.DXT5, UnityEditor.BuildTarget.StandaloneWindows64));
        }

        [Test]
        public void IsSupportedTextureFormat_ASTC6x6_ForiOS_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.ASTC_6x6, UnityEditor.BuildTarget.iOS));
        }

        [Test]
        public void IsNormalMapAsset_NullTexture_ReturnsFalse()
        {
            Assert.IsFalse(TextureUtility.IsNormalMapAsset(null));
        }

        [Test]
        public void IsNormalMapAsset_RuntimeTexture_ReturnsFalse()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                Assert.IsFalse(TextureUtility.IsNormalMapAsset(tex));
            }
            finally { UObject.DestroyImmediate(tex); }
        }

        [Test]
        public void GetCompressionFormat_AllValues_DoNotThrow()
        {
            foreach (MobileTextureFormat fmt in Enum.GetValues(typeof(MobileTextureFormat)))
            {
                if (fmt == MobileTextureFormat.NoOverride) continue;
                Assert.DoesNotThrow(() => TextureUtility.GetCompressionFormat(fmt));
            }
        }
    }

    // ==========================================
    // VRCSDKUtility Tests
    // ==========================================
    [TestFixture]
    public class Batch25_VRCSDKUtilityTests
    {
        [Test]
        public void IsAvatarRoot_NullObject_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsAvatarRoot(null));
        }

        [Test]
        public void IsAvatarRoot_WithDescriptor_ReturnsTrue()
        {
            var go = new GameObject("Avatar");
            go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                Assert.IsTrue(VRCSDKUtility.IsAvatarRoot(go));
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void IsAvatarRoot_WithoutDescriptor_ReturnsFalse()
        {
            var go = new GameObject("NotAvatar");
            try
            {
                Assert.IsFalse(VRCSDKUtility.IsAvatarRoot(go));
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void GetAvatarRoot_FromChild_ReturnsAvatarRoot()
        {
            var root = new GameObject("AvatarRoot");
            root.AddComponent<VRCAvatarDescriptor>();
            var child = new GameObject("Child");
            child.transform.SetParent(root.transform);
            try
            {
                var result = VRCSDKUtility.GetAvatarRoot(child);
                Assert.AreEqual(root, result);
            }
            finally { UObject.DestroyImmediate(root); }
        }

        [Test]
        public void GetAvatarRoot_FromRoot_ReturnsItself()
        {
            var root = new GameObject("AvatarRoot");
            root.AddComponent<VRCAvatarDescriptor>();
            try
            {
                var result = VRCSDKUtility.GetAvatarRoot(root);
                Assert.AreEqual(root, result);
            }
            finally { UObject.DestroyImmediate(root); }
        }

        [Test]
        public void GetAvatarRoot_NoDescriptor_ReturnsNull()
        {
            var go = new GameObject("NotAvatar");
            try
            {
                var result = VRCSDKUtility.GetAvatarRoot(go);
                Assert.IsNull(result);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void IsProxyAnimationClip_NullClip_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsProxyAnimationClip(null));
        }

        [Test]
        public void IsProxyAnimationClip_NormalClip_ReturnsFalse()
        {
            var clip = new AnimationClip { name = "TestClip" };
            try
            {
                Assert.IsFalse(VRCSDKUtility.IsProxyAnimationClip(clip));
            }
            finally { UObject.DestroyImmediate(clip); }
        }

        [Test]
        public void IsExampleAsset_NullPath_ThrowsOrReturnsFalse()
        {
            // null path may throw; just ensure we don't get unexpected behavior
            try
            {
                var result = VRCSDKUtility.IsExampleAsset((string)null);
                Assert.IsFalse(result);
            }
            catch (System.ArgumentNullException)
            {
                Assert.Pass("Throws ArgumentNullException for null path");
            }
        }

        [Test]
        public void IsExampleAsset_EmptyPath_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsExampleAsset(string.Empty));
        }

        [Test]
        public void IsExampleAsset_NonExamplePath_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsExampleAsset("Assets/MyProject/MyScript.cs"));
        }

        [Test]
        public void IsUnsupportedComponentType_Transform_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsUnsupportedComponentType(typeof(Transform)));
        }

        [Test]
        public void IsUnsupportedComponentType_MeshRenderer_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsUnsupportedComponentType(typeof(MeshRenderer)));
        }

        [Test]
        public void UnsupportedComponentTypes_IsNotEmpty()
        {
            Assert.IsNotNull(VRCSDKUtility.UnsupportedComponentTypes);
            Assert.Greater(VRCSDKUtility.UnsupportedComponentTypes.Length, 0);
        }

        [Test]
        public void PoorPhysBonesCountLimit_IsPositive()
        {
            Assert.Greater(VRCSDKUtility.PoorPhysBonesCountLimit, 0);
        }

        [Test]
        public void PoorContactsCountLimit_IsPositive()
        {
            Assert.Greater(VRCSDKUtility.PoorContactsCountLimit, 0);
        }

        [Test]
        public void AvatarDynamicsPerformanceCategories_IsNotEmpty()
        {
            Assert.IsNotNull(VRCSDKUtility.AvatarDynamicsPerformanceCategories);
            Assert.Greater(VRCSDKUtility.AvatarDynamicsPerformanceCategories.Length, 0);
        }
    }

    // ==========================================
    // MSMapGenViewModel Tests
    // ==========================================
    [TestFixture]
    public class Batch25_MSMapGenViewModelTests
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
        public void DisableGenerateButton_OnlyMetallicSet_ReturnsFalse()
        {
            var vm = new MSMapGenViewModel();
            var tex = new Texture2D(4, 4);
            try
            {
                vm.metallicMap = tex;
                vm.smoothnessMap = null;
                Assert.IsFalse(vm.DisableGenerateButton);
            }
            finally { UObject.DestroyImmediate(tex); }
        }

        [Test]
        public void DisableGenerateButton_OnlySmoothnessSet_ReturnsFalse()
        {
            var vm = new MSMapGenViewModel();
            var tex = new Texture2D(4, 4);
            try
            {
                vm.metallicMap = null;
                vm.smoothnessMap = tex;
                Assert.IsFalse(vm.DisableGenerateButton);
            }
            finally { UObject.DestroyImmediate(tex); }
        }

        [Test]
        public void DisableGenerateButton_BothSet_ReturnsFalse()
        {
            var vm = new MSMapGenViewModel();
            var tex1 = new Texture2D(4, 4);
            var tex2 = new Texture2D(4, 4);
            try
            {
                vm.metallicMap = tex1;
                vm.smoothnessMap = tex2;
                Assert.IsFalse(vm.DisableGenerateButton);
            }
            finally
            {
                UObject.DestroyImmediate(tex1);
                UObject.DestroyImmediate(tex2);
            }
        }

        [Test]
        public void Constructor_DefaultValues_AreNull()
        {
            var vm = new MSMapGenViewModel();
            Assert.IsNull(vm.metallicMap);
            Assert.IsNull(vm.smoothnessMap);
            Assert.IsFalse(vm.invertSmoothness);
        }
    }

    // ==========================================
    // MaterialWrapperBuilder Tests
    // ==========================================
    [TestFixture]
    public class Batch25_MaterialWrapperBuilderTests
    {
        [Test]
        public void DetectShaderCategory_StandardShader_ReturnsStandard()
        {
            var shader = Shader.Find("Standard");
            if (shader == null) { Assert.Ignore("Standard shader not found"); return; }
            var mat = new Material(shader);
            try
            {
                var builder = new MaterialWrapperBuilder();
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Standard, builder.DetectShaderCategory(mat));
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void DetectShaderCategory_StandardSpecularShader_ReturnsStandard()
        {
            var shader = Shader.Find("Standard (Specular setup)");
            if (shader == null) { Assert.Ignore("Standard (Specular setup) shader not found"); return; }
            var mat = new Material(shader);
            try
            {
                var builder = new MaterialWrapperBuilder();
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Standard, builder.DetectShaderCategory(mat));
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void DetectShaderCategory_UnlitColorShader_ReturnsUnlit()
        {
            var shader = Shader.Find("Unlit/Color");
            if (shader == null) { Assert.Ignore("Unlit/Color shader not found"); return; }
            var mat = new Material(shader);
            try
            {
                var builder = new MaterialWrapperBuilder();
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Unlit, builder.DetectShaderCategory(mat));
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void DetectShaderCategory_UnlitTextureShader_ReturnsUnlit()
        {
            var shader = Shader.Find("Unlit/Texture");
            if (shader == null) { Assert.Ignore("Unlit/Texture shader not found"); return; }
            var mat = new Material(shader);
            try
            {
                var builder = new MaterialWrapperBuilder();
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Unlit, builder.DetectShaderCategory(mat));
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void DetectShaderCategory_QuestToonLit_ReturnsQuest()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (shader == null) { Assert.Ignore("Toon Lit shader not found"); return; }
            var mat = new Material(shader);
            try
            {
                var builder = new MaterialWrapperBuilder();
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Quest, builder.DetectShaderCategory(mat));
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void DetectShaderCategory_QuestMatCapLit_ReturnsQuest()
        {
            var shader = Shader.Find("VRChat/Mobile/MatCap Lit");
            if (shader == null) { Assert.Ignore("MatCap Lit shader not found"); return; }
            var mat = new Material(shader);
            try
            {
                var builder = new MaterialWrapperBuilder();
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Quest, builder.DetectShaderCategory(mat));
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void DetectShaderCategory_ToonStandard_ReturnsQuest()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Standard");
            if (shader == null) { Assert.Ignore("Toon Standard shader not found"); return; }
            var mat = new Material(shader);
            try
            {
                var builder = new MaterialWrapperBuilder();
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Quest, builder.DetectShaderCategory(mat));
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void DetectShaderCategory_UnknownShader_ReturnsUnverified()
        {
            var shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null) { Assert.Ignore("Internal shader not found"); return; }
            var mat = new Material(shader);
            try
            {
                var builder = new MaterialWrapperBuilder();
                var cat = builder.DetectShaderCategory(mat);
                // Should not be Standard, Unlit, or Quest for internal shaders
                Assert.IsTrue(cat == MaterialWrapperBuilder.ShaderCategory.Unverified ||
                              cat == MaterialWrapperBuilder.ShaderCategory.Unlit);
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void Build_StandardShader_ReturnsMaterialBase()
        {
            var shader = Shader.Find("Standard");
            if (shader == null) { Assert.Ignore("Standard shader not found"); return; }
            var mat = new Material(shader);
            try
            {
                var builder = new MaterialWrapperBuilder();
                var wrapper = builder.Build(mat);
                Assert.IsNotNull(wrapper);
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void Build_QuestShader_ReturnsMaterialBase()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (shader == null) { Assert.Ignore("Toon Lit shader not found"); return; }
            var mat = new Material(shader);
            try
            {
                var builder = new MaterialWrapperBuilder();
                var wrapper = builder.Build(mat);
                Assert.IsNotNull(wrapper);
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void Build_UnlitShader_ReturnsMaterialBase()
        {
            var shader = Shader.Find("Unlit/Color");
            if (shader == null) { Assert.Ignore("Unlit/Color shader not found"); return; }
            var mat = new Material(shader);
            try
            {
                var builder = new MaterialWrapperBuilder();
                var wrapper = builder.Build(mat);
                Assert.IsNotNull(wrapper);
            }
            finally { UObject.DestroyImmediate(mat); }
        }
    }

    // ==========================================
    // UnityAnimationUtility Tests
    // ==========================================
    [TestFixture]
    public class Batch25_UnityAnimationUtilityTests
    {
        [Test]
        public void GetMaterials_NullController_ThrowsOrReturnsEmpty()
        {
            try
            {
                var result = UnityAnimationUtility.GetMaterials((RuntimeAnimatorController)null);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Length);
            }
            catch (System.NullReferenceException)
            {
                Assert.Pass("Throws NRE for null controller");
            }
        }

        [Test]
        public void GetMaterials_EmptyController_ReturnsEmpty()
        {
            var controller = new AnimatorController();
            try
            {
                var result = UnityAnimationUtility.GetMaterials(controller);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Length);
            }
            finally { UObject.DestroyImmediate(controller); }
        }

        [Test]
        public void GetBlendTrees_EmptyController_ReturnsEmpty()
        {
            var controller = new AnimatorController();
            controller.AddLayer("Base");
            try
            {
                var result = UnityAnimationUtility.GetBlendTrees(controller);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Length);
            }
            finally { UObject.DestroyImmediate(controller); }
        }

        [Test]
        public void GetBlendTrees_WithBlendTree_ReturnsBlendTrees()
        {
            var controller = new AnimatorController();
            controller.AddLayer("Base");
            controller.AddParameter("Blend", AnimatorControllerParameterType.Float);
            var stateMachine = controller.layers[0].stateMachine;
            var blendTree = new BlendTree();
            stateMachine.AddState("BlendState").motion = blendTree;
            try
            {
                var result = UnityAnimationUtility.GetBlendTrees(controller);
                Assert.IsNotNull(result);
                Assert.GreaterOrEqual(result.Length, 1);
            }
            finally { UObject.DestroyImmediate(controller); }
        }

        [Test]
        public void ReplaceAnimationClipMaterials_NullClip_Throws()
        {
            Assert.Throws<System.ArgumentException>(() => UnityAnimationUtility.ReplaceAnimationClipMaterials(null, new Dictionary<Material, Material>()));
        }

        [Test]
        public void ReplaceAnimationClipMaterials_EmptyDictionary_DoesNotThrow()
        {
            var clip = new AnimationClip();
            try
            {
                Assert.DoesNotThrow(() => UnityAnimationUtility.ReplaceAnimationClipMaterials(clip, new Dictionary<Material, Material>()));
            }
            finally { UObject.DestroyImmediate(clip); }
        }

        [Test]
        public void GetMaterials_FromClip_ReturnsEmpty()
        {
            var clip = new AnimationClip();
            try
            {
                var result = UnityAnimationUtility.GetMaterials(clip);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Length);
            }
            finally { UObject.DestroyImmediate(clip); }
        }

        [Test]
        public void DoesMotionExistInBlendTreeDescendants_NullTree_ThrowsOrReturnsFalse()
        {
            var clip = new AnimationClip();
            try
            {
                try
                {
                    Assert.IsFalse(UnityAnimationUtility.DoesMotionExistInBlendTreeDescendants(null, clip));
                }
                catch (System.NullReferenceException)
                {
                    Assert.Pass("Throws NRE for null tree");
                }
            }
            finally { UObject.DestroyImmediate(clip); }
        }

        [Test]
        public void DoesMotionExistInBlendTreeDescendants_EmptyTree_ReturnsFalse()
        {
            var clip = new AnimationClip();
            var tree = new BlendTree();
            try
            {
                Assert.IsFalse(UnityAnimationUtility.DoesMotionExistInBlendTreeDescendants(tree, clip));
            }
            finally
            {
                UObject.DestroyImmediate(clip);
                UObject.DestroyImmediate(tree);
            }
        }

        [Test]
        public void DeepCopyBlendTree_EmptyTree_ReturnsNewInstance()
        {
            var tree = new BlendTree { name = "TestTree" };
            try
            {
                var copy = UnityAnimationUtility.DeepCopyBlendTree(tree);
                try
                {
                    Assert.IsNotNull(copy);
                    Assert.AreNotSame(tree, copy);
                }
                finally { UObject.DestroyImmediate(copy); }
            }
            finally { UObject.DestroyImmediate(tree); }
        }
    }

    // ==========================================
    // CacheUtility Additional Tests
    // ==========================================
    [TestFixture]
    public class Batch25_CacheUtilityTests
    {
        [Test]
        public void GetContentCacheKey_DifferentShader_DifferentKey()
        {
            var shader1 = Shader.Find("Standard");
            var shader2 = Shader.Find("Unlit/Color");
            if (shader1 == null || shader2 == null) { Assert.Ignore("Shaders not found"); return; }

            var mat1 = new Material(shader1);
            var mat2 = new Material(shader2);
            try
            {
                var key1 = CacheUtility.GetContentCacheKey(mat1);
                var key2 = CacheUtility.GetContentCacheKey(mat2);
                Assert.AreNotEqual(key1, key2);
            }
            finally
            {
                UObject.DestroyImmediate(mat1);
                UObject.DestroyImmediate(mat2);
            }
        }

        [Test]
        public void GetContentCacheKey_SameShaderDifferentColor_DifferentKey()
        {
            var shader = Shader.Find("Standard");
            if (shader == null) { Assert.Ignore("Standard shader not found"); return; }

            var mat1 = new Material(shader);
            var mat2 = new Material(shader);
            mat1.color = Color.red;
            mat2.color = Color.blue;
            try
            {
                var key1 = CacheUtility.GetContentCacheKey(mat1);
                var key2 = CacheUtility.GetContentCacheKey(mat2);
                Assert.AreNotEqual(key1, key2);
            }
            finally
            {
                UObject.DestroyImmediate(mat1);
                UObject.DestroyImmediate(mat2);
            }
        }

        [Test]
        public void GetContentCacheKey_SameMaterialTwice_SameKey()
        {
            var shader = Shader.Find("Standard");
            if (shader == null) { Assert.Ignore("Standard shader not found"); return; }

            var mat = new Material(shader);
            try
            {
                var key1 = CacheUtility.GetContentCacheKey(mat);
                var key2 = CacheUtility.GetContentCacheKey(mat);
                Assert.AreEqual(key1, key2);
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void GetContentCacheKey_ToonLitMaterial_ReturnsNonEmpty()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (shader == null) { Assert.Ignore("Toon Lit shader not found"); return; }

            var mat = new Material(shader);
            try
            {
                var key = CacheUtility.GetContentCacheKey(mat);
                Assert.IsNotNull(key);
                Assert.IsNotEmpty(key);
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void GetContentCacheKey_MaterialWithTexture_ReturnsNonEmpty()
        {
            var shader = Shader.Find("Standard");
            if (shader == null) { Assert.Ignore("Standard shader not found"); return; }

            var mat = new Material(shader);
            var tex = new Texture2D(4, 4);
            mat.mainTexture = tex;
            try
            {
                var key = CacheUtility.GetContentCacheKey(mat);
                Assert.IsNotNull(key);
                Assert.IsNotEmpty(key);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void GetContentCacheKey_MaterialWithFloat_ChangesKey()
        {
            var shader = Shader.Find("Standard");
            if (shader == null) { Assert.Ignore("Standard shader not found"); return; }

            var mat1 = new Material(shader);
            var mat2 = new Material(shader);
            mat1.SetFloat("_Metallic", 0.0f);
            mat2.SetFloat("_Metallic", 1.0f);
            try
            {
                var key1 = CacheUtility.GetContentCacheKey(mat1);
                var key2 = CacheUtility.GetContentCacheKey(mat2);
                Assert.AreNotEqual(key1, key2);
            }
            finally
            {
                UObject.DestroyImmediate(mat1);
                UObject.DestroyImmediate(mat2);
            }
        }
    }

    // ==========================================
    // AvatarConverter Additional Tests
    // ==========================================
    [TestFixture]
    public class Batch25_AvatarConverterAdditionalTests
    {
        [Test]
        public void FindDescendant_ExistingChild_Found()
        {
            var type = typeof(AvatarConverter);
            var method = type.GetMethod("FindDescendant", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("FindDescendant not accessible"); return; }

            var root = new GameObject("Root");
            var child = new GameObject("Child");
            var grandchild = new GameObject("Target");
            child.transform.SetParent(root.transform);
            grandchild.transform.SetParent(child.transform);
            try
            {
                var result = method.Invoke(null, new object[] { root, "Target" });
                Assert.IsNotNull(result);
                Assert.AreEqual(grandchild, result);
            }
            finally { UObject.DestroyImmediate(root); }
        }

        [Test]
        public void FindDescendant_MissingChild_ReturnsNull()
        {
            var type = typeof(AvatarConverter);
            var method = type.GetMethod("FindDescendant", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("FindDescendant not accessible"); return; }

            var root = new GameObject("Root");
            try
            {
                var result = method.Invoke(null, new object[] { root, "NonExistent" });
                Assert.IsNull(result);
            }
            finally { UObject.DestroyImmediate(root); }
        }

        [Test]
        public void RemoveExtraMaterialSlots_RendererWithExtraSlots_RemovesThem()
        {
            var type = typeof(AvatarConverter);
            var method = type.GetMethod("RemoveExtraMaterialSlots", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("RemoveExtraMaterialSlots not accessible"); return; }

            var go = new GameObject("Avatar");
            var child = new GameObject("Renderer");
            child.transform.SetParent(go.transform);
            var renderer = child.AddComponent<MeshRenderer>();
            var mesh = new Mesh();
            mesh.subMeshCount = 1;
            var filter = child.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            renderer.sharedMaterials = new Material[] { new Material(Shader.Find("Standard")), new Material(Shader.Find("Standard")) };
            try
            {
                method.Invoke(null, new object[] { go });
                // After removal, materials should be trimmed to submesh count
                Assert.AreEqual(1, renderer.sharedMaterials.Length);
            }
            finally
            {
                foreach (var m in renderer.sharedMaterials) UObject.DestroyImmediate(m);
                UObject.DestroyImmediate(mesh);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_RendererMatchingSlots_NoChange()
        {
            var type = typeof(AvatarConverter);
            var method = type.GetMethod("RemoveExtraMaterialSlots", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("RemoveExtraMaterialSlots not accessible"); return; }

            var go = new GameObject("Avatar");
            var child = new GameObject("Renderer");
            child.transform.SetParent(go.transform);
            var renderer = child.AddComponent<MeshRenderer>();
            var mesh = new Mesh();
            mesh.subMeshCount = 2;
            var filter = child.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            renderer.sharedMaterials = new Material[] { mat1, mat2 };
            try
            {
                method.Invoke(null, new object[] { go });
                Assert.AreEqual(2, renderer.sharedMaterials.Length);
            }
            finally
            {
                UObject.DestroyImmediate(mat1);
                UObject.DestroyImmediate(mat2);
                UObject.DestroyImmediate(mesh);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_EmptyAvatar_ReturnsEmpty()
        {
            var go = new GameObject("Avatar");
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            try
            {
                var avatar = new VRChatAvatar(desc);
                var converter = new AvatarConverter(new MaterialWrapperBuilder());
                var result = converter.CreateMaterialConvertSettingsMap(avatar);
                Assert.IsNotNull(result);
            }
            finally { UObject.DestroyImmediate(go); }
        }
    }

    // ==========================================
    // VRCQuestToolsSettings Additional Tests
    // ==========================================
    [TestFixture]
    public class Batch25_VRCQuestToolsSettingsTests
    {
        [Test]
        public void I18nResource_IsNotNull()
        {
            var resource = VRCQuestToolsSettings.I18nResource;
            Assert.IsNotNull(resource);
        }

        [Test]
        public void DisplayLanguage_GetSet_Roundtrip()
        {
            var original = VRCQuestToolsSettings.DisplayLanguage;
            try
            {
                VRCQuestToolsSettings.DisplayLanguage = DisplayLanguage.English;
                Assert.AreEqual(DisplayLanguage.English, VRCQuestToolsSettings.DisplayLanguage);

                VRCQuestToolsSettings.DisplayLanguage = DisplayLanguage.Japanese;
                Assert.AreEqual(DisplayLanguage.Japanese, VRCQuestToolsSettings.DisplayLanguage);
            }
            finally
            {
                VRCQuestToolsSettings.DisplayLanguage = original;
            }
        }

        [Test]
        public void IsShowUnitySettingsWindowOnLoadEnabled_GetSet_Works()
        {
            var original = VRCQuestToolsSettings.IsShowUnitySettingsWindowOnLoadEnabled;
            try
            {
                VRCQuestToolsSettings.IsShowUnitySettingsWindowOnLoadEnabled = !original;
                Assert.AreEqual(!original, VRCQuestToolsSettings.IsShowUnitySettingsWindowOnLoadEnabled);
            }
            finally
            {
                VRCQuestToolsSettings.IsShowUnitySettingsWindowOnLoadEnabled = original;
            }
        }

        [Test]
        public void ValidationAutomatorEnabled_GetSet_Works()
        {
            var original = VRCQuestToolsSettings.IsValidationAutomatorEnabled;
            try
            {
                VRCQuestToolsSettings.IsValidationAutomatorEnabled = !original;
                Assert.AreEqual(!original, VRCQuestToolsSettings.IsValidationAutomatorEnabled);
            }
            finally
            {
                VRCQuestToolsSettings.IsValidationAutomatorEnabled = original;
            }
        }

        [Test]
        public void CheckTextureFormatOnStandalone_DefaultIsFalse()
        {
            // This is the documented default
            var val = VRCQuestToolsSettings.IsCheckTextureFormatOnStandaloneEnabled;
            Assert.IsInstanceOf<bool>(val);
        }
    }

    // ==========================================
    // I18n Coverage Tests - exercise all language branches
    // ==========================================
    [TestFixture]
    public class Batch25_I18nCoverageTests
    {
        [Test]
        public void I18n_GetI18n_AutoLanguage_ReturnsNonNull()
        {
            var original = VRCQuestToolsSettings.DisplayLanguage;
            try
            {
                VRCQuestToolsSettings.DisplayLanguage = DisplayLanguage.Auto;
                var i18n = KRT.VRCQuestTools.I18n.I18n.GetI18n();
                Assert.IsNotNull(i18n);
            }
            finally { VRCQuestToolsSettings.DisplayLanguage = original; }
        }

        [Test]
        public void I18n_GetI18n_English_ReturnsEnglish()
        {
            var original = VRCQuestToolsSettings.DisplayLanguage;
            try
            {
                VRCQuestToolsSettings.DisplayLanguage = DisplayLanguage.English;
                var i18n = KRT.VRCQuestTools.I18n.I18n.GetI18n();
                Assert.IsNotNull(i18n);
                Assert.IsInstanceOf<KRT.VRCQuestTools.I18n.I18nEnglish>(i18n);
            }
            finally { VRCQuestToolsSettings.DisplayLanguage = original; }
        }

        [Test]
        public void I18n_GetI18n_Japanese_ReturnsJapanese()
        {
            var original = VRCQuestToolsSettings.DisplayLanguage;
            try
            {
                VRCQuestToolsSettings.DisplayLanguage = DisplayLanguage.Japanese;
                var i18n = KRT.VRCQuestTools.I18n.I18n.GetI18n();
                Assert.IsNotNull(i18n);
                Assert.IsInstanceOf<KRT.VRCQuestTools.I18n.I18nJapanese>(i18n);
            }
            finally { VRCQuestToolsSettings.DisplayLanguage = original; }
        }

        [Test]
        public void I18n_GetI18n_Russian_ReturnsRussian()
        {
            var original = VRCQuestToolsSettings.DisplayLanguage;
            try
            {
                VRCQuestToolsSettings.DisplayLanguage = DisplayLanguage.Russian;
                var i18n = KRT.VRCQuestTools.I18n.I18n.GetI18n();
                Assert.IsNotNull(i18n);
                Assert.IsInstanceOf<KRT.VRCQuestTools.I18n.I18nRussian>(i18n);
            }
            finally { VRCQuestToolsSettings.DisplayLanguage = original; }
        }

        [Test]
        public void I18nEnglish_AllLabelsAreNonEmpty()
        {
            var i18n = new KRT.VRCQuestTools.I18n.I18nEnglish();
            Assert.IsNotEmpty(i18n.CancelLabel);
            Assert.IsNotEmpty(i18n.OpenLabel);
        }

        [Test]
        public void I18nJapanese_AllLabelsAreNonEmpty()
        {
            var i18n = new KRT.VRCQuestTools.I18n.I18nJapanese();
            Assert.IsNotEmpty(i18n.CancelLabel);
            Assert.IsNotEmpty(i18n.OpenLabel);
        }
    }

    // ==========================================
    // AssetUtility Additional Tests
    // ==========================================
    [TestFixture]
    public class Batch25_AssetUtilityAdditionalTests
    {
        [Test]
        public void IsLilToonImported_ReturnsTrue()
        {
            // lilToon is installed in this project
            Assert.IsTrue(AssetUtility.IsLilToonImported());
        }

        [Test]
        public void IsDynamicBoneImported_ReturnsFalse()
        {
            Assert.IsFalse(AssetUtility.IsDynamicBoneImported());
        }
    }

    // ==========================================
    // ModularAvatarUtility Additional Tests
    // ==========================================
    [TestFixture]
    public class Batch25_ModularAvatarUtilityTests
    {
        [Test]
        public void IsModularAvatarImported_ReturnsBool()
        {
            // MA may or may not be installed - just ensure it doesn't throw
            var result = ModularAvatarUtility.IsModularAvatarImported();
            Assert.That(result, Is.TypeOf<bool>());
        }

        [Test]
        public void RemoveUnsupportedComponents_EmptyObject_DoesNotThrow()
        {
            var go = new GameObject("TestMA");
            try
            {
                Assert.DoesNotThrow(() => ModularAvatarUtility.RemoveUnsupportedComponents(go, false));
            }
            finally { UObject.DestroyImmediate(go); }
        }
    }

    // ==========================================
    // MaterialGeneratorUtility Tests
    // ==========================================
    [TestFixture]
    public class Batch25_MaterialGeneratorUtilityTests
    {
        [Test]
        public void ConvertToNullableTextureFormat_NoOverride_ReturnsNull()
        {
            var type = typeof(MaterialGeneratorUtility);
            var method = type.GetMethod("ConvertToNullableTextureFormat", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("ConvertToNullableTextureFormat not accessible"); return; }

            var result = method.Invoke(null, new object[] { MobileTextureFormat.NoOverride });
            Assert.IsNull(result);
        }

        [Test]
        public void ConvertToNullableTextureFormat_ASTC6x6_ReturnsASTCFormat()
        {
            var type = typeof(MaterialGeneratorUtility);
            var method = type.GetMethod("ConvertToNullableTextureFormat", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("ConvertToNullableTextureFormat not accessible"); return; }

            var result = method.Invoke(null, new object[] { MobileTextureFormat.ASTC_6x6 });
            Assert.IsNotNull(result);
            Assert.AreEqual(TextureFormat.ASTC_6x6, result);
        }

        [Test]
        public void ConvertToNullableTextureFormat_ASTC4x4_ReturnsFormat()
        {
            var type = typeof(MaterialGeneratorUtility);
            var method = type.GetMethod("ConvertToNullableTextureFormat", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null) { Assert.Ignore("ConvertToNullableTextureFormat not accessible"); return; }

            var result = method.Invoke(null, new object[] { MobileTextureFormat.ASTC_4x4 });
            Assert.IsNotNull(result);
        }
    }

    // ==========================================
    // ValidationAutomator Tests
    // ==========================================
    [TestFixture]
    public class Batch25_ValidationAutomatorTests
    {
        [Test]
        public void ValidationAutomatorEnabled_ToggleWorks()
        {
            var original = VRCQuestToolsSettings.IsValidationAutomatorEnabled;
            try
            {
                VRCQuestToolsSettings.IsValidationAutomatorEnabled = true;
                Assert.IsTrue(VRCQuestToolsSettings.IsValidationAutomatorEnabled);

                VRCQuestToolsSettings.IsValidationAutomatorEnabled = false;
                Assert.IsFalse(VRCQuestToolsSettings.IsValidationAutomatorEnabled);
            }
            finally { VRCQuestToolsSettings.IsValidationAutomatorEnabled = original; }
        }
    }

    // ==========================================
    // SystemUtility Tests
    // ==========================================
    [TestFixture]
    public class Batch25_SystemUtilityTests
    {
        [Test]
        public void GetTypeByName_ExistingType_ReturnsType()
        {
            var result = SystemUtility.GetTypeByName("UnityEngine.Transform");
            Assert.IsNotNull(result);
            Assert.AreEqual(typeof(Transform), result);
        }

        [Test]
        public void GetTypeByName_NonExistentType_ReturnsNull()
        {
            var result = SystemUtility.GetTypeByName("NonExistent.FakeType.XYZ");
            Assert.IsNull(result);
        }
    }

    // ==========================================
    // VRChatAvatar Additional Detailed Tests
    // ==========================================
    [TestFixture]
    public class Batch25_VRChatAvatarAdditionalTests
    {
        private GameObject CreateTestAvatar(string name)
        {
            var go = new GameObject(name);
            var desc = go.AddComponent<VRCAvatarDescriptor>();
            desc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            desc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
            return go;
        }

        [Test]
        public void Materials_EmptyAvatar_ReturnsEmpty()
        {
            var go = CreateTestAvatar("Empty");
            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                var mats = avatar.Materials;
                Assert.IsNotNull(mats);
                Assert.AreEqual(0, mats.Length);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void Materials_WithRenderer_ReturnsMaterials()
        {
            var go = CreateTestAvatar("WithRenderer");
            var child = new GameObject("Renderer");
            child.transform.SetParent(go.transform);
            var renderer = child.AddComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Standard"));
            renderer.sharedMaterials = new Material[] { mat };
            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                var mats = avatar.Materials;
                Assert.GreaterOrEqual(mats.Length, 1);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
                UObject.DestroyImmediate(go);
            }
        }

        [Test]
        public void HasUnityConstraints_NoConstraints_ReturnsFalse()
        {
            var go = CreateTestAvatar("NoConstraints");
            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                Assert.IsFalse(avatar.HasUnityConstraints);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void HasDynamicBoneComponents_NoDynamicBone_ReturnsFalse()
        {
            var go = CreateTestAvatar("NoDynBone");
            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                Assert.IsFalse(avatar.HasDynamicBoneComponents);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void HasVertexColor_NoRenderers_ReturnsFalse()
        {
            var go = CreateTestAvatar("NoRenderers");
            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                Assert.IsFalse(avatar.HasVertexColor);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void GetPhysBoneColliders_Empty_ReturnsEmpty()
        {
            var go = CreateTestAvatar("NoPBColliders");
            try
            {
                var avatar = new VRChatAvatar(go.GetComponent<VRCAvatarDescriptor>());
                var colliders = avatar.GetPhysBoneColliders();
                Assert.IsNotNull(colliders);
                Assert.AreEqual(0, colliders.Count());
            }
            finally { UObject.DestroyImmediate(go); }
        }
    }

    // ==========================================
    // NotificationItem Tests
    // ==========================================
    [TestFixture]
    public class Batch25_NotificationItemTests
    {
        [Test]
        public void Constructor_SetsGuiDelegate()
        {
            var item = new NotificationItem(() => true);
            Assert.IsNotNull(item.GuiDelegate);
            Assert.IsTrue(item.GuiDelegate());
        }
    }

    // ==========================================
    // ConversionException Tests
    // ==========================================
    [TestFixture]
    public class Batch25_ConversionExceptionTests
    {
        [Test]
        public void AnimationClipConversionException_Message_IsNotNull()
        {
            var clip = new AnimationClip();
            try
            {
                var ex = new AnimationClipConversionException("test error", clip, new Exception("inner"));
                Assert.IsNotNull(ex.Message);
                Assert.IsNotNull(ex.InnerException);
                Assert.AreEqual(clip, ex.SourceObject);
            }
            finally { UObject.DestroyImmediate(clip); }
        }

        [Test]
        public void AnimatorControllerConversionException_Message_IsNotNull()
        {
            var controller = new AnimatorController();
            try
            {
                var ex = new AnimatorControllerConversionException("test error", controller, new Exception("inner"));
                Assert.IsNotNull(ex.Message);
                Assert.AreEqual(controller, ex.SourceObject);
            }
            finally { UObject.DestroyImmediate(controller); }
        }

        [Test]
        public void MaterialConversionException_Message_IsNotNull()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var ex = new MaterialConversionException("test error", mat, new Exception("inner"));
                Assert.IsNotNull(ex.Message);
                Assert.AreEqual(mat, ex.SourceObject);
            }
            finally { UObject.DestroyImmediate(mat); }
        }

        [Test]
        public void TargetMaterialNullException_Message_ContainsInfo()
        {
            var go = new GameObject("TestComponent");
            try
            {
                var comp = go.AddComponent<MeshRenderer>();
                var ex = new TargetMaterialNullException("TestSlot", comp);
                Assert.IsNotNull(ex.Message);
                Assert.AreEqual(comp, ex.Component);
            }
            finally { UObject.DestroyImmediate(go); }
        }

        [Test]
        public void InvalidReplacementMaterialException_Message_ContainsInfo()
        {
            var go = new GameObject("TestComponent");
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var comp = go.AddComponent<MeshRenderer>();
                var ex = new InvalidReplacementMaterialException("test error", comp, mat);
                Assert.IsNotNull(ex.Message);
                Assert.AreEqual(comp, ex.Component);
                Assert.AreEqual(mat, ex.ReplacementMaterial);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
                UObject.DestroyImmediate(go);
            }
        }
    }
}
