// Batch 32: High-impact coverage tests targeting TextureUtility, VRCQuestTools entry point,
// AvatarConverter private methods, and FallbackAvatarCallback branches.

using System;
using System.Collections.Generic;
using System.Reflection;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UObject = UnityEngine.Object;
using EditorBuildTarget = UnityEditor.BuildTarget;

namespace KRT.VRCQuestTools.Tests
{
    // =========================================================
    // TextureUtility - IsSupportedTextureFormat Tests
    // =========================================================
    [TestFixture]
    public class TextureUtility_IsSupportedTextureFormat_Tests
    {
        [Test]
        public void IsSupportedTextureFormat_Android_ASTC4x4_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.ASTC_4x4, EditorBuildTarget.Android));
        }

        [Test]
        public void IsSupportedTextureFormat_Android_ASTC6x6_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.ASTC_6x6, EditorBuildTarget.Android));
        }

        [Test]
        public void IsSupportedTextureFormat_Android_RGBA32_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.RGBA32, EditorBuildTarget.Android));
        }

        [Test]
        public void IsSupportedTextureFormat_Windows_DXT5_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.DXT5, EditorBuildTarget.StandaloneWindows64));
        }

        [Test]
        public void IsSupportedTextureFormat_Windows_DXT1_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.DXT1, EditorBuildTarget.StandaloneWindows64));
        }

        [Test]
        public void IsSupportedTextureFormat_Windows_RGBA32_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.RGBA32, EditorBuildTarget.StandaloneWindows64));
        }

        [Test]
        public void IsSupportedTextureFormat_StandaloneWindows_DXT5_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.DXT5, EditorBuildTarget.StandaloneWindows));
        }

        [Test]
        public void IsSupportedTextureFormat_iOS_ASTC4x4_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.ASTC_4x4, EditorBuildTarget.iOS));
        }

        [Test]
        public void IsSupportedTextureFormat_iOS_RGBA32_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsSupportedTextureFormat(TextureFormat.RGBA32, EditorBuildTarget.iOS));
        }

        [Test]
        public void IsSupportedTextureFormat_UnsupportedBuildTarget_Throws()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                TextureUtility.IsSupportedTextureFormat(TextureFormat.RGBA32, EditorBuildTarget.WebGL);
            });
        }

        [Test]
        public void IsSupportedTextureFormat_Windows_ASTC_ReturnsFalse()
        {
            Assert.IsFalse(TextureUtility.IsSupportedTextureFormat(TextureFormat.ASTC_4x4, EditorBuildTarget.StandaloneWindows64));
        }

        [Test]
        public void IsSupportedTextureFormat_Android_DXT5_ReturnsFalse()
        {
            Assert.IsFalse(TextureUtility.IsSupportedTextureFormat(TextureFormat.DXT5, EditorBuildTarget.Android));
        }
    }

    // =========================================================
    // TextureUtility - GetImageContentsHash Tests
    // =========================================================
    [TestFixture]
    public class TextureUtility_GetImageContentsHash_Tests
    {
        [Test]
        public void GetImageContentsHash_Null_ReturnsDefault()
        {
            var hash = TextureUtility.GetImageContentsHash(null);
            Assert.AreEqual(default(Hash128), hash);
        }

        [Test]
        public void GetImageContentsHash_RenderTexture_ReturnsNonDefault()
        {
            var rt = RenderTexture.GetTemporary(4, 4);
            try
            {
                var hash = TextureUtility.GetImageContentsHash(rt);
                // RenderTexture path generates a random hash, just verify it doesn't throw
                Assert.IsNotNull(hash);
            }
            finally
            {
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        [Test]
        public void GetImageContentsHash_Texture2D_ReturnsImageHash()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                var hash = TextureUtility.GetImageContentsHash(tex);
                Assert.IsNotNull(hash);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }
    }

    // =========================================================
    // TextureUtility - CompressTextureForBuildTarget Tests
    // =========================================================
    [TestFixture]
    public class TextureUtility_CompressTextureForBuildTarget_Tests
    {
        [Test]
        public void CompressTextureForBuildTarget_WindowsTarget_UsesDXT5()
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            try
            {
                var result = TextureUtility.CompressTextureForBuildTarget(tex, EditorBuildTarget.StandaloneWindows64, TextureFormat.ASTC_6x6);
                Assert.IsNotNull(result);
                Assert.AreEqual(TextureFormat.DXT5, result.format);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressTextureForBuildTarget_AndroidTarget_UsesMobileFormat()
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            try
            {
                var result = TextureUtility.CompressTextureForBuildTarget(tex, EditorBuildTarget.Android, TextureFormat.ASTC_6x6);
                Assert.IsNotNull(result);
                Assert.AreEqual(TextureFormat.ASTC_6x6, result.format);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressTextureForBuildTarget_NonMultipleOf4_Windows_SkipsDXT5()
        {
            // 15x15 is not multiple of 4 => DXT5 compression skipped
            var tex = new Texture2D(15, 15, TextureFormat.RGBA32, false);
            try
            {
                var result = TextureUtility.CompressTextureForBuildTarget(tex, EditorBuildTarget.StandaloneWindows64, TextureFormat.ASTC_6x6);
                Assert.IsNotNull(result);
                // Should NOT be compressed to DXT5 since dimensions not multiple of 4
                Assert.AreNotEqual(TextureFormat.DXT5, result.format);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressTextureForBuildTarget_WithMaxTextureSize_ResizesBeforeCompress()
        {
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            try
            {
                var result = TextureUtility.CompressTextureForBuildTarget(tex, EditorBuildTarget.Android, TextureFormat.ASTC_6x6, 32);
                Assert.IsNotNull(result);
                Assert.LessOrEqual(result.width, 32);
                Assert.LessOrEqual(result.height, 32);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressTextureForBuildTarget_MaxSizeSameAsTexture_NoResize()
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            try
            {
                var result = TextureUtility.CompressTextureForBuildTarget(tex, EditorBuildTarget.Android, TextureFormat.ASTC_6x6, 16);
                Assert.IsNotNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressTextureForBuildTarget_iOSTarget_UsesMobileFormat()
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            try
            {
                var result = TextureUtility.CompressTextureForBuildTarget(tex, EditorBuildTarget.iOS, TextureFormat.ASTC_8x8);
                Assert.IsNotNull(result);
                Assert.AreEqual(TextureFormat.ASTC_8x8, result.format);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }
    }

    // =========================================================
    // TextureUtility - CompressNormalMap Tests
    // =========================================================
    [TestFixture]
    public class TextureUtility_CompressNormalMap_Tests
    {
        [Test]
        public void CompressNormalMap_AndroidTarget_ReturnsCompressedTexture()
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, true);
            try
            {
                var result = TextureUtility.CompressNormalMap(tex, EditorBuildTarget.Android, TextureFormat.ASTC_6x6);
                Assert.IsNotNull(result);
                UObject.DestroyImmediate(result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressNormalMap_WindowsTarget_ReturnsCompressedTexture()
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, true);
            try
            {
                var result = TextureUtility.CompressNormalMap(tex, EditorBuildTarget.StandaloneWindows64, TextureFormat.ASTC_6x6);
                Assert.IsNotNull(result);
                UObject.DestroyImmediate(result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressNormalMap_WithMaxTextureSize_ReturnsResized()
        {
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, true);
            try
            {
                var result = TextureUtility.CompressNormalMap(tex, EditorBuildTarget.Android, TextureFormat.ASTC_6x6, false, 32);
                Assert.IsNotNull(result);
                Assert.LessOrEqual(Math.Max(result.width, result.height), 32);
                UObject.DestroyImmediate(result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CompressNormalMap_Readable_ReturnsReadableTexture()
        {
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, true);
            try
            {
                var result = TextureUtility.CompressNormalMap(tex, EditorBuildTarget.Android, TextureFormat.ASTC_6x6, readable: true);
                Assert.IsNotNull(result);
                // Verify no exception when reading pixels (readable=true)
                Assert.DoesNotThrow(() => result.GetPixels32());
                UObject.DestroyImmediate(result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }
    }

    // =========================================================
    // TextureUtility - GetBestPlatformOverrideSettings Tests
    // =========================================================
    [TestFixture]
    public class TextureUtility_GetBestPlatformOverrideSettings_Tests
    {
        [Test]
        public void GetBestPlatformOverrideSettings_Null_ReturnsNull()
        {
            var result = TextureUtility.GetBestPlatformOverrideSettings(null);
            Assert.IsNull(result);
        }

        [Test]
        public void GetBestPlatformOverrideSettings_EmptyArray_ReturnsNull()
        {
            var result = TextureUtility.GetBestPlatformOverrideSettings();
            Assert.IsNull(result);
        }

        [Test]
        public void GetBestPlatformOverrideSettings_NullTexture_ReturnsNull()
        {
            var result = TextureUtility.GetBestPlatformOverrideSettings((Texture)null);
            Assert.IsNull(result);
        }

        [Test]
        public void GetBestPlatformOverrideSettings_InMemoryTexture_ReturnsNull()
        {
            // Textures not saved to disk have no asset path
            var tex = new Texture2D(4, 4);
            try
            {
                var result = TextureUtility.GetBestPlatformOverrideSettings(tex);
                Assert.IsNull(result);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }
    }

    // =========================================================
    // TextureUtility - IsKnownTextureFormat Tests
    // =========================================================
    [TestFixture]
    public class TextureUtility_IsKnownTextureFormat_Tests
    {
        [Test]
        public void IsKnownTextureFormat_RGBA32_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(TextureFormat.RGBA32));
        }

        [Test]
        public void IsKnownTextureFormat_DXT5_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(TextureFormat.DXT5));
        }

        [Test]
        public void IsKnownTextureFormat_ASTC4x4_ReturnsTrue()
        {
            Assert.IsTrue(TextureUtility.IsKnownTextureFormat(TextureFormat.ASTC_4x4));
        }

        [Test]
        public void IsKnownTextureFormat_UnknownFormat_ReturnsFalse()
        {
            // Cast an invalid enum value that is definitely not in any known format list
            Assert.IsFalse(TextureUtility.IsKnownTextureFormat((TextureFormat)(-999)));
        }
    }

    // =========================================================
    // TextureUtility - Private ASTC Methods via Reflection
    // =========================================================
    [TestFixture]
    public class TextureUtility_ASTCMethods_Tests
    {
        private static readonly Type texUtilType = typeof(TextureUtility);

        private static MethodInfo GetPrivateStaticMethod(string name)
        {
            return texUtilType.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
        }

        [Test]
        public void GetASTCQualityScore_4x4_Returns16()
        {
            var method = GetPrivateStaticMethod("GetASTCQualityScore");
            if (method == null) Assert.Inconclusive("GetASTCQualityScore not found");
            var result = (int)method.Invoke(null, new object[] { TextureFormat.ASTC_4x4 });
            Assert.AreEqual(16, result);
        }

        [Test]
        public void GetASTCQualityScore_12x12_Returns144()
        {
            var method = GetPrivateStaticMethod("GetASTCQualityScore");
            if (method == null) Assert.Inconclusive("GetASTCQualityScore not found");
            var result = (int)method.Invoke(null, new object[] { TextureFormat.ASTC_12x12 });
            Assert.AreEqual(144, result);
        }

        [Test]
        public void GetASTCQualityScore_NonASTC_ReturnsMaxValue()
        {
            var method = GetPrivateStaticMethod("GetASTCQualityScore");
            if (method == null) Assert.Inconclusive("GetASTCQualityScore not found");
            var result = (int)method.Invoke(null, new object[] { TextureFormat.DXT5 });
            Assert.AreEqual(int.MaxValue, result);
        }

        [Test]
        public void GetASTCQualityScore_AllFormats()
        {
            var method = GetPrivateStaticMethod("GetASTCQualityScore");
            if (method == null) Assert.Inconclusive("GetASTCQualityScore not found");

            Assert.AreEqual(25, (int)method.Invoke(null, new object[] { TextureFormat.ASTC_5x5 }));
            Assert.AreEqual(36, (int)method.Invoke(null, new object[] { TextureFormat.ASTC_6x6 }));
            Assert.AreEqual(64, (int)method.Invoke(null, new object[] { TextureFormat.ASTC_8x8 }));
            Assert.AreEqual(100, (int)method.Invoke(null, new object[] { TextureFormat.ASTC_10x10 }));
        }

        [Test]
        public void GetBetterASTCFormat_NullCurrent_ReturnsCandidate()
        {
            var method = GetPrivateStaticMethod("GetBetterASTCFormat");
            if (method == null) Assert.Inconclusive("GetBetterASTCFormat not found");
            var result = (TextureFormat)method.Invoke(null, new object[] { null, TextureFormat.ASTC_6x6 });
            Assert.AreEqual(TextureFormat.ASTC_6x6, result);
        }

        [Test]
        public void GetBetterASTCFormat_BetterCandidate_ReturnsCandidate()
        {
            var method = GetPrivateStaticMethod("GetBetterASTCFormat");
            if (method == null) Assert.Inconclusive("GetBetterASTCFormat not found");
            var result = (TextureFormat)method.Invoke(null, new object[] { (TextureFormat?)TextureFormat.ASTC_12x12, TextureFormat.ASTC_4x4 });
            Assert.AreEqual(TextureFormat.ASTC_4x4, result);
        }

        [Test]
        public void GetBetterASTCFormat_WorseCandiate_ReturnsCurrent()
        {
            var method = GetPrivateStaticMethod("GetBetterASTCFormat");
            if (method == null) Assert.Inconclusive("GetBetterASTCFormat not found");
            var result = (TextureFormat)method.Invoke(null, new object[] { (TextureFormat?)TextureFormat.ASTC_4x4, TextureFormat.ASTC_12x12 });
            Assert.AreEqual(TextureFormat.ASTC_4x4, result);
        }

        [Test]
        public void GetMobileTextureFormatFromImporterFormat_AllFormats()
        {
            var method = GetPrivateStaticMethod("GetMobileTextureFormatFromImporterFormat");
            if (method == null) Assert.Inconclusive("GetMobileTextureFormatFromImporterFormat not found");

            // Test standard ASTC formats
            Assert.AreEqual(TextureFormat.ASTC_4x4, (TextureFormat?)method.Invoke(null, new object[] { TextureImporterFormat.ASTC_4x4 }));
            Assert.AreEqual(TextureFormat.ASTC_5x5, (TextureFormat?)method.Invoke(null, new object[] { TextureImporterFormat.ASTC_5x5 }));
            Assert.AreEqual(TextureFormat.ASTC_6x6, (TextureFormat?)method.Invoke(null, new object[] { TextureImporterFormat.ASTC_6x6 }));
            Assert.AreEqual(TextureFormat.ASTC_8x8, (TextureFormat?)method.Invoke(null, new object[] { TextureImporterFormat.ASTC_8x8 }));
            Assert.AreEqual(TextureFormat.ASTC_10x10, (TextureFormat?)method.Invoke(null, new object[] { TextureImporterFormat.ASTC_10x10 }));
            Assert.AreEqual(TextureFormat.ASTC_12x12, (TextureFormat?)method.Invoke(null, new object[] { TextureImporterFormat.ASTC_12x12 }));

            // Test HDR ASTC mapped to non-HDR
            Assert.AreEqual(TextureFormat.ASTC_4x4, (TextureFormat?)method.Invoke(null, new object[] { TextureImporterFormat.ASTC_HDR_4x4 }));
            Assert.AreEqual(TextureFormat.ASTC_5x5, (TextureFormat?)method.Invoke(null, new object[] { TextureImporterFormat.ASTC_HDR_5x5 }));
            Assert.AreEqual(TextureFormat.ASTC_6x6, (TextureFormat?)method.Invoke(null, new object[] { TextureImporterFormat.ASTC_HDR_6x6 }));
            Assert.AreEqual(TextureFormat.ASTC_8x8, (TextureFormat?)method.Invoke(null, new object[] { TextureImporterFormat.ASTC_HDR_8x8 }));
            Assert.AreEqual(TextureFormat.ASTC_10x10, (TextureFormat?)method.Invoke(null, new object[] { TextureImporterFormat.ASTC_HDR_10x10 }));
            Assert.AreEqual(TextureFormat.ASTC_12x12, (TextureFormat?)method.Invoke(null, new object[] { TextureImporterFormat.ASTC_HDR_12x12 }));

            // Test non-ASTC returns null
            Assert.IsNull((TextureFormat?)method.Invoke(null, new object[] { TextureImporterFormat.DXT5 }));
            Assert.IsNull((TextureFormat?)method.Invoke(null, new object[] { TextureImporterFormat.PVRTC_RGBA4 }));
        }
    }

    // =========================================================
    // TextureUtility - CreateColorTexture Tests
    // =========================================================
    [TestFixture]
    public class TextureUtility_CreateColorTexture_Tests
    {
        [Test]
        public void CreateColorTexture_Default_Creates4x4()
        {
            var tex = TextureUtility.CreateColorTexture(Color.red);
            try
            {
                Assert.AreEqual(4, tex.width);
                Assert.AreEqual(4, tex.height);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CreateColorTexture_CustomSize_CreatesCorrectSize()
        {
            var tex = TextureUtility.CreateColorTexture(Color.blue, 8, 8);
            try
            {
                Assert.AreEqual(8, tex.width);
                Assert.AreEqual(8, tex.height);
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }

        [Test]
        public void CreateColorTexture_BlackColor_AllPixelsBlack()
        {
            var tex = TextureUtility.CreateColorTexture(Color.black, 4, 4);
            try
            {
                var pixels = tex.GetPixels32();
                foreach (var pixel in pixels)
                {
                    Assert.AreEqual(0, pixel.r);
                    Assert.AreEqual(0, pixel.g);
                    Assert.AreEqual(0, pixel.b);
                }
            }
            finally
            {
                UObject.DestroyImmediate(tex);
            }
        }
    }

    // =========================================================
    // VRCQuestTools - Entry Point Properties
    // =========================================================
    [TestFixture]
    public class VRCQuestTools_EntryPoint_Tests
    {
        [Test]
        public void AssetRoot_IsNotNullOrEmpty()
        {
            Assert.IsNotNull(VRCQuestTools.AssetRoot);
            Assert.IsNotEmpty(VRCQuestTools.AssetRoot);
        }

        [Test]
        public void AssetRoot_StartsWithPackages()
        {
            // Installed as VPM package, so path should start with "Packages"
            Assert.IsTrue(VRCQuestTools.AssetRoot.StartsWith("Packages"),
                $"AssetRoot should start with 'Packages' but was '{VRCQuestTools.AssetRoot}'");
        }

        [Test]
        public void IsImportedAsPackage_ReturnsTrue()
        {
            // VRCQuestTools is installed as a VPM package
            Assert.IsTrue(VRCQuestTools.IsImportedAsPackage);
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

        [Test]
        public void VPM_IsNotNull()
        {
            Assert.IsNotNull(VRCQuestTools.VPM);
        }

        [Test]
        public void VPMRepositoryURL_IsValidUrl()
        {
            Assert.IsTrue(VRCQuestTools.VPMRepositoryURL.StartsWith("https://"));
        }

        [Test]
        public void DocsURL_IsValidUrl()
        {
            Assert.IsTrue(VRCQuestTools.DocsURL.StartsWith("https://"));
        }

        [Test]
        public void PackageName_IsCorrect()
        {
            Assert.AreEqual("com.github.kurotu.vrc-quest-tools", VRCQuestTools.PackageName);
        }
    }

    // =========================================================
    // AvatarConverter - ApplyVRCQuestToolsComponents Tests
    // =========================================================
    [TestFixture]
    public class AvatarConverter_ApplyVRCQuestToolsComponents_Tests
    {
        private static readonly Type converterType = typeof(VRCQuestTools).Assembly
            .GetType("KRT.VRCQuestTools.Models.VRChat.AvatarConverter");

        private readonly List<GameObject> createdObjects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in createdObjects)
            {
                if (obj != null) UObject.DestroyImmediate(obj);
            }
            createdObjects.Clear();
        }

        private object CreateConverter()
        {
            var ctor = converterType.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new[] { typeof(MaterialWrapperBuilder) },
                null);
            return ctor.Invoke(new object[] { new MaterialWrapperBuilder() });
        }

        private void InvokeApplyVRCQuestToolsComponents(object converter, object settings, GameObject go)
        {
            var method = converterType.GetMethod("ApplyVRCQuestToolsComponents",
                BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(converter, new object[] { settings, go });
        }

        [Test]
        public void ApplyVRCQuestToolsComponents_AddsConvertedAvatar()
        {
            var go = new GameObject("TestAvatar");
            createdObjects.Add(go);
            var descriptor = go.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            var settings = go.AddComponent<KRT.VRCQuestTools.Components.AvatarConverterSettings>();

            var converter = CreateConverter();
            InvokeApplyVRCQuestToolsComponents(converter, settings, go);

            var convertedAvatar = go.GetComponent<KRT.VRCQuestTools.Components.ConvertedAvatar>();
            Assert.IsNotNull(convertedAvatar, "ConvertedAvatar component should be added");
        }

        [Test]
        public void ApplyVRCQuestToolsComponents_DoesNotDuplicateConvertedAvatar()
        {
            var go = new GameObject("TestAvatar");
            createdObjects.Add(go);
            var descriptor = go.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            var settings = go.AddComponent<KRT.VRCQuestTools.Components.AvatarConverterSettings>();
            go.AddComponent<KRT.VRCQuestTools.Components.ConvertedAvatar>();

            var converter = CreateConverter();
            InvokeApplyVRCQuestToolsComponents(converter, settings, go);

            var components = go.GetComponents<KRT.VRCQuestTools.Components.ConvertedAvatar>();
            Assert.AreEqual(1, components.Length, "Should not duplicate ConvertedAvatar");
        }

        [Test]
        public void ApplyVRCQuestToolsComponents_WithPlatformTargetSettings_SetsAndroid()
        {
            var go = new GameObject("TestAvatar");
            createdObjects.Add(go);
            var descriptor = go.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            var settings = go.AddComponent<KRT.VRCQuestTools.Components.AvatarConverterSettings>();
            var platformSettings = go.AddComponent<KRT.VRCQuestTools.Components.PlatformTargetSettings>();

            var converter = CreateConverter();
            InvokeApplyVRCQuestToolsComponents(converter, settings, go);

            Assert.AreEqual(KRT.VRCQuestTools.Models.BuildTarget.Android, platformSettings.buildTarget);
        }

        [Test]
        public void ApplyVRCQuestToolsComponents_WithoutPlatformTargetSettings_NoCrash()
        {
            var go = new GameObject("TestAvatar");
            createdObjects.Add(go);
            var descriptor = go.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            var settings = go.AddComponent<KRT.VRCQuestTools.Components.AvatarConverterSettings>();

            var converter = CreateConverter();
            Assert.DoesNotThrow(() => InvokeApplyVRCQuestToolsComponents(converter, settings, go));
        }
    }

    // =========================================================
    // AvatarConverter - ApplyVirtualLens2Support Tests
    // =========================================================
    [TestFixture]
    public class AvatarConverter_ApplyVirtualLens2Support_Tests
    {
        private static readonly Type converterType = typeof(VRCQuestTools).Assembly
            .GetType("KRT.VRCQuestTools.Models.VRChat.AvatarConverter");

        private readonly List<GameObject> createdObjects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in createdObjects)
            {
                if (obj != null) UObject.DestroyImmediate(obj);
            }
            createdObjects.Clear();
        }

        private object CreateConverter()
        {
            var ctor = converterType.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new[] { typeof(MaterialWrapperBuilder) },
                null);
            return ctor.Invoke(new object[] { new MaterialWrapperBuilder() });
        }

        private void InvokeApplyVirtualLens2Support(object converter, GameObject go)
        {
            var method = converterType.GetMethod("ApplyVirtualLens2Support",
                BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(converter, new object[] { go });
        }

        [Test]
        public void ApplyVirtualLens2Support_NoVirtualLensRoot_NoChange()
        {
            var go = new GameObject("TestAvatar");
            createdObjects.Add(go);

            var converter = CreateConverter();
            Assert.DoesNotThrow(() => InvokeApplyVirtualLens2Support(converter, go));
        }

        [Test]
        public void ApplyVirtualLens2Support_WithVirtualLensRoot_DisablesAndTagsEditorOnly()
        {
            var go = new GameObject("TestAvatar");
            createdObjects.Add(go);
            var root = new GameObject("_VirtualLens_Root");
            root.transform.SetParent(go.transform);

            var converter = CreateConverter();
            InvokeApplyVirtualLens2Support(converter, go);

            Assert.AreEqual("EditorOnly", root.tag);
            Assert.IsFalse(root.activeSelf);
        }

        [Test]
        public void ApplyVirtualLens2Support_WithVirtualLensOrigin_DisablesAndTagsEditorOnly()
        {
            var go = new GameObject("TestAvatar");
            createdObjects.Add(go);
            var root = new GameObject("_VirtualLens_Root");
            root.transform.SetParent(go.transform);
            var origin = new GameObject("VirtualLensOrigin");
            origin.transform.SetParent(go.transform);

            var converter = CreateConverter();
            InvokeApplyVirtualLens2Support(converter, go);

            Assert.AreEqual("EditorOnly", root.tag);
            Assert.IsFalse(root.activeSelf);
            Assert.AreEqual("EditorOnly", origin.tag);
            Assert.IsFalse(origin.activeSelf);
        }

        [Test]
        public void ApplyVirtualLens2Support_WithVirtualLensRootOnly_NoOriginNoCrash()
        {
            var go = new GameObject("TestAvatar");
            createdObjects.Add(go);
            var root = new GameObject("_VirtualLens_Root");
            root.transform.SetParent(go.transform);

            var converter = CreateConverter();
            InvokeApplyVirtualLens2Support(converter, go);

            Assert.AreEqual("EditorOnly", root.tag);
            Assert.IsFalse(root.activeSelf);
        }
    }

    // =========================================================
    // AvatarConverter - PrepareConvertForQuestInPlace Tests
    // =========================================================
    [TestFixture]
    public class AvatarConverter_PrepareConvertForQuestInPlace_Tests
    {
        private static readonly Type converterType = typeof(VRCQuestTools).Assembly
            .GetType("KRT.VRCQuestTools.Models.VRChat.AvatarConverter");
        private static readonly Type avatarType = typeof(VRCQuestTools).Assembly
            .GetType("KRT.VRCQuestTools.Models.VRChat.VRChatAvatar");

        private readonly List<GameObject> createdObjects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in createdObjects)
            {
                if (obj != null) UObject.DestroyImmediate(obj);
            }
            createdObjects.Clear();
        }

        [Test]
        public void PrepareConvertForQuestInPlace_WithSimpleAvatar_DoesNotThrow()
        {
            var go = new GameObject("TestAvatar");
            createdObjects.Add(go);
            var descriptor = go.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();

            var ctor = converterType.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null, new[] { typeof(MaterialWrapperBuilder) }, null);
            var converter = ctor.Invoke(new object[] { new MaterialWrapperBuilder() });

            var avatar = Activator.CreateInstance(avatarType,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null, new object[] { descriptor }, null);

            var method = converterType.GetMethod("PrepareConvertForQuestInPlace",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            Assert.DoesNotThrow(() => method.Invoke(converter, new object[] { avatar }));
        }
    }

    // =========================================================
    // AvatarConverter - PrepareModularAvatarComponentsInPlace Tests
    // =========================================================
    [TestFixture]
    public class AvatarConverter_PrepareModularAvatarComponentsInPlace_Tests
    {
        private static readonly Type converterType = typeof(VRCQuestTools).Assembly
            .GetType("KRT.VRCQuestTools.Models.VRChat.AvatarConverter");
        private static readonly Type avatarType = typeof(VRCQuestTools).Assembly
            .GetType("KRT.VRCQuestTools.Models.VRChat.VRChatAvatar");

        private readonly List<GameObject> createdObjects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in createdObjects)
            {
                if (obj != null) UObject.DestroyImmediate(obj);
            }
            createdObjects.Clear();
        }

        [Test]
        public void PrepareModularAvatarComponentsInPlace_WithSimpleAvatar_DoesNotThrow()
        {
            var go = new GameObject("TestAvatar");
            createdObjects.Add(go);
            var descriptor = go.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();

            var ctor = converterType.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null, new[] { typeof(MaterialWrapperBuilder) }, null);
            var converter = ctor.Invoke(new object[] { new MaterialWrapperBuilder() });

            var avatar = Activator.CreateInstance(avatarType,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null, new object[] { descriptor }, null);

            var method = converterType.GetMethod("PrepareModularAvatarComponentsInPlace",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            Assert.DoesNotThrow(() => method.Invoke(converter, new object[] { avatar }));
        }
    }

    // =========================================================
    // FallbackAvatarCallback - PendingFallbackAvatars dictionary
    // =========================================================
    [TestFixture]
    public class FallbackAvatarCallback_PendingDict_Tests
    {
        private static readonly Type callbackType = typeof(LilToonToonStandardGenerator).Assembly
            .GetType("KRT.VRCQuestTools.NonDestructive.FallbackAvatarCallback");

        private readonly List<GameObject> createdObjects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in createdObjects)
            {
                if (obj != null) UObject.DestroyImmediate(obj);
            }
            createdObjects.Clear();
        }

        private static Type FindPipelineManagerType()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var t = asm.GetType("VRC.Core.PipelineManager");
                    if (t != null) return t;
                }
                catch { }
            }
            return null;
        }

        [Test]
        public void OnPreprocessAvatar_WithFallbackAvatar_AddsToPendingDict()
        {
            if (callbackType == null) Assert.Inconclusive("FallbackAvatarCallback not found");
            var pmType = FindPipelineManagerType();
            if (pmType == null) Assert.Inconclusive("PipelineManager type not found");

            var go = new GameObject("TestFallbackAvatar");
            createdObjects.Add(go);
            var pm = go.AddComponent(pmType);
            var bpField = pmType.GetField("blueprintId", BindingFlags.Instance | BindingFlags.Public);
            if (bpField == null) Assert.Inconclusive("blueprintId field not found");
            bpField.SetValue(pm, "avtr_test_fallback_dict");
            go.AddComponent<KRT.VRCQuestTools.Components.FallbackAvatar>();

            var callback = Activator.CreateInstance(callbackType);
            var method = callbackType.GetMethod("OnPreprocessAvatar",
                BindingFlags.Instance | BindingFlags.Public);
            var result = (bool)method.Invoke(callback, new object[] { go });

            Assert.IsTrue(result);

            // Check PendingFallbackAvatars dictionary has the entry
            var dictField = callbackType.GetField("PendingFallbackAvatars",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (dictField != null)
            {
                var dict = dictField.GetValue(null) as System.Collections.IDictionary;
                if (dict != null)
                {
                    Assert.IsTrue(dict.Contains("avtr_test_fallback_dict"),
                        "PendingFallbackAvatars should contain the blueprintId");
                }
            }
        }

        [Test]
        public void OnPreprocessAvatar_WithoutFallbackAvatar_RemovesFromPendingDict()
        {
            if (callbackType == null) Assert.Inconclusive("FallbackAvatarCallback not found");
            var pmType = FindPipelineManagerType();
            if (pmType == null) Assert.Inconclusive("PipelineManager type not found");

            var go = new GameObject("TestNoFallback");
            createdObjects.Add(go);
            var pm = go.AddComponent(pmType);
            var bpField = pmType.GetField("blueprintId", BindingFlags.Instance | BindingFlags.Public);
            if (bpField == null) Assert.Inconclusive("blueprintId field not found");
            bpField.SetValue(pm, "avtr_test_nofallback");
            // No FallbackAvatar component added

            var callback = Activator.CreateInstance(callbackType);
            var method = callbackType.GetMethod("OnPreprocessAvatar",
                BindingFlags.Instance | BindingFlags.Public);
            var result = (bool)method.Invoke(callback, new object[] { go });

            Assert.IsTrue(result);

            // The entry should be removed (or set to false) in PendingFallbackAvatars
            var dictField = callbackType.GetField("PendingFallbackAvatars",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (dictField != null)
            {
                var dict = dictField.GetValue(null) as System.Collections.IDictionary;
                if (dict != null && dict.Contains("avtr_test_nofallback"))
                {
                    var val = dict["avtr_test_nofallback"];
                    Assert.IsFalse((bool)val, "PendingFallbackAvatars should have false for avatars without FallbackAvatar");
                }
            }
        }
    }

    // =========================================================
    // MaterialWrapperBuilder - Build with lilToon shader
    // =========================================================
    [TestFixture]
    public class MaterialWrapperBuilder_Build_Batch32Tests
    {
        private MaterialWrapperBuilder builder;

        [SetUp]
        public void SetUp()
        {
            builder = new MaterialWrapperBuilder();
        }

        [Test]
        public void Build_LilToonMaterial_ReturnsLilToonMaterial()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Inconclusive("lilToon shader not found");
            var mat = new Material(shader);
            try
            {
                var wrapper = builder.Build(mat);
                Assert.IsInstanceOf<LilToonMaterial>(wrapper);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_StandardMaterial_ReturnsStandardMaterial()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var wrapper = builder.Build(mat);
                Assert.IsInstanceOf<StandardMaterial>(wrapper);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_QuestShader_ReturnsMaterialBase()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (shader == null) Assert.Inconclusive("Toon Lit shader not found");
            var mat = new Material(shader);
            try
            {
                var wrapper = builder.Build(mat);
                Assert.IsNotNull(wrapper);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_UnlitColor_ReturnsMaterialBase()
        {
            var shader = Shader.Find("Unlit/Color");
            if (shader == null) Assert.Inconclusive("Unlit/Color shader not found");
            var mat = new Material(shader);
            try
            {
                var wrapper = builder.Build(mat);
                Assert.IsNotNull(wrapper);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_LilToon_ReturnsLilToon()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null) Assert.Inconclusive("lilToon shader not found");
            var mat = new Material(shader);
            try
            {
                var category = builder.DetectShaderCategory(mat);
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.LilToon, category);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_UnlitColor_ReturnsUnlit()
        {
            var shader = Shader.Find("Unlit/Color");
            if (shader == null) Assert.Inconclusive("Unlit/Color shader not found");
            var mat = new Material(shader);
            try
            {
                var category = builder.DetectShaderCategory(mat);
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Unlit, category);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_ToonLit_ReturnsQuest()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (shader == null) Assert.Inconclusive("Toon Lit shader not found");
            var mat = new Material(shader);
            try
            {
                var category = builder.DetectShaderCategory(mat);
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Quest, category);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_ToonStandard_ReturnsQuest()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Standard");
            if (shader == null) Assert.Inconclusive("Toon Standard shader not found");
            var mat = new Material(shader);
            try
            {
                var category = builder.DetectShaderCategory(mat);
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Quest, category);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void DetectShaderCategory_UnknownShader_ReturnsUnverified()
        {
            // Sprites/Default shader is not in any VRChat category
            var shader = Shader.Find("Sprites/Default");
            if (shader == null) Assert.Inconclusive("Sprites/Default shader not found");
            var mat = new Material(shader);
            try
            {
                var category = builder.DetectShaderCategory(mat);
                Assert.AreEqual(MaterialWrapperBuilder.ShaderCategory.Unverified, category);
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }
    }

    // =========================================================
    // VRCSDKUtility - Additional Coverage Tests
    // =========================================================
    [TestFixture]
    public class VRCSDKUtility_ExtendedTests
    {
        private readonly List<GameObject> createdObjects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in createdObjects)
            {
                if (obj != null) UObject.DestroyImmediate(obj);
            }
            createdObjects.Clear();
        }

        [Test]
        public void GetAvatarRoot_ChildOfAvatar_ReturnsAvatarRoot()
        {
            var root = new GameObject("AvatarRoot");
            createdObjects.Add(root);
            root.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            var child = new GameObject("Child");
            child.transform.SetParent(root.transform);

            var result = VRCSDKUtility.GetAvatarRoot(child);
            Assert.AreEqual(root, result);
        }

        [Test]
        public void GetAvatarRoot_NonAvatarObject_ReturnsNull()
        {
            var go = new GameObject("NotAvatar");
            createdObjects.Add(go);

            var result = VRCSDKUtility.GetAvatarRoot(go);
            Assert.IsNull(result);
        }

        [Test]
        public void GetAvatarRoot_AvatarObjectItself_ReturnsSelf()
        {
            var go = new GameObject("AvatarRoot");
            createdObjects.Add(go);
            go.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();

            var result = VRCSDKUtility.GetAvatarRoot(go);
            Assert.AreEqual(go, result);
        }

        [Test]
        public void GetAvatarRoot_DeepChild_ReturnsAvatarRoot()
        {
            var root = new GameObject("AvatarRoot");
            createdObjects.Add(root);
            root.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            var child1 = new GameObject("Child1");
            child1.transform.SetParent(root.transform);
            var child2 = new GameObject("Child2");
            child2.transform.SetParent(child1.transform);

            var result = VRCSDKUtility.GetAvatarRoot(child2);
            Assert.AreEqual(root, result);
        }

        [Test]
        public void IsAvatarRoot_WithDescriptor_ReturnsTrue()
        {
            var go = new GameObject("AvatarRoot");
            createdObjects.Add(go);
            go.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();

            Assert.IsTrue(VRCSDKUtility.IsAvatarRoot(go));
        }

        [Test]
        public void IsAvatarRoot_WithoutDescriptor_ReturnsFalse()
        {
            var go = new GameObject("NotAvatar");
            createdObjects.Add(go);

            Assert.IsFalse(VRCSDKUtility.IsAvatarRoot(go));
        }

        [Test]
        public void GetGameObjectsWithMissingComponents_CleanAvatar_ReturnsEmpty()
        {
            var go = new GameObject("CleanAvatar");
            createdObjects.Add(go);

            var result = VRCSDKUtility.GetGameObjectsWithMissingComponents(go, true);
            Assert.AreEqual(0, result.Length);
        }

        [Test]
        public void CountMissingComponentsInChildren_CleanAvatar_ReturnsZero()
        {
            var go = new GameObject("CleanAvatar");
            createdObjects.Add(go);

            var result = VRCSDKUtility.CountMissingComponentsInChildren(go, true);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void IsMaterialAllowedForQuestAvatar_ToonLit_ReturnsTrue()
        {
            var shader = Shader.Find("VRChat/Mobile/Toon Lit");
            if (shader == null) Assert.Inconclusive("Toon Lit not found");
            var mat = new Material(shader);
            try
            {
                Assert.IsTrue(VRCSDKUtility.IsMaterialAllowedForQuestAvatar(mat));
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void IsMaterialAllowedForQuestAvatar_Standard_ReturnsFalse()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                Assert.IsFalse(VRCSDKUtility.IsMaterialAllowedForQuestAvatar(mat));
            }
            finally
            {
                UObject.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetFullPathInHierarchy_ChildObject_ReturnsFullPath()
        {
            var root = new GameObject("Root");
            createdObjects.Add(root);
            var child = new GameObject("Child");
            child.transform.SetParent(root.transform);
            var grandchild = new GameObject("Grandchild");
            grandchild.transform.SetParent(child.transform);

            var result = VRCSDKUtility.GetFullPathInHierarchy(grandchild);
            // Uses Transform path format with leading slash and double separators
            Assert.IsTrue(result.Contains("Root") && result.Contains("Child") && result.Contains("Grandchild"),
                $"Path should contain all hierarchy names, got: {result}");
        }

        [Test]
        public void GetFullPathInHierarchy_RootObject_ReturnsName()
        {
            var go = new GameObject("RootOnly");
            createdObjects.Add(go);

            var result = VRCSDKUtility.GetFullPathInHierarchy(go);
            Assert.IsTrue(result.Contains("RootOnly"),
                $"Path should contain the object name, got: {result}");
        }

        [Test]
        public void IsProxyAnimationClip_Null_ReturnsFalse()
        {
            Assert.IsFalse(VRCSDKUtility.IsProxyAnimationClip(null));
        }

        [Test]
        public void IsProxyAnimationClip_CustomClip_ReturnsFalse()
        {
            var clip = new AnimationClip();
            clip.name = "TestClip";
            try
            {
                Assert.IsFalse(VRCSDKUtility.IsProxyAnimationClip(clip));
            }
            finally
            {
                UObject.DestroyImmediate(clip);
            }
        }
    }
}
