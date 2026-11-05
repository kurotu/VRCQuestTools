// <copyright file="AvatarConverterTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.VRChat
{
    /// <summary>
    /// Tests for AvatarConverter.
    /// </summary>
    public class AvatarConverterTests
    {
        private const string MobileToonLitShaderName = "VRChat/Mobile/Toon Lit";
        private const string TempRoot = "Assets/VRCQuestTools-Tests/Temp";
        private static readonly char[] WindowsInvalidFileNameChars = { '"', '<', '>', '|', ':', '*', '?', '\\', '/' };

        /// <summary>
        /// Test that Platform GameObject Remover deletes marked GameObjects on Android conversion.
        /// </summary>
        [Test]
        public void ApplyPlatformGameObjectRemoversForAndroid_RemoveOnAndroid_DeletesGameObject()
        {
            var root = new GameObject("AvatarRoot");
            var child = new GameObject("ChildToDelete");
            child.transform.SetParent(root.transform);
            var remover = child.AddComponent<PlatformGameObjectRemover>();
            remover.removeOnAndroid = true;
            remover.removeOnPC = false;

            InvokeApplyPlatformGameObjectRemoversForAndroid(root);

            Assert.IsTrue(child == null, "GameObject marked removeOnAndroid should be deleted.");

            Object.DestroyImmediate(root);
        }

        /// <summary>
        /// Test that Platform GameObject Remover keeps GameObjects when Android removal is not enabled.
        /// </summary>
        [Test]
        public void ApplyPlatformGameObjectRemoversForAndroid_RemoveOnPCOnly_KeepsGameObject()
        {
            var root = new GameObject("AvatarRoot");
            var child = new GameObject("ChildToKeep");
            child.transform.SetParent(root.transform);
            var remover = child.AddComponent<PlatformGameObjectRemover>();
            remover.removeOnAndroid = false;
            remover.removeOnPC = true;

            InvokeApplyPlatformGameObjectRemoversForAndroid(root);

            Assert.IsFalse(child == null, "GameObject should remain when removeOnAndroid is false.");

            Object.DestroyImmediate(root);
        }

        /// <summary>
        /// Test that characters invalid on Windows are replaced while "/" is kept as a directory separator.
        /// </summary>
        /// <param name="meshName">Mesh name.</param>
        /// <param name="expected">Expected file name.</param>
        [TestCase("Body", "Body")]
        [TestCase("HairPin_???", "HairPin____")]
        [TestCase("a:b*c", "a_b_c")]
        [TestCase("Parent/Child?", "Parent/Child_")]
        public void ReplaceInvalidFileNameChars_WindowsInvalidChars_ReplacesWithUnderscore(string meshName, string expected)
        {
            var method = typeof(AvatarConverter).GetMethod("ReplaceInvalidFileNameChars", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, "ReplaceInvalidFileNameChars method should exist.");

            var actual = method.Invoke(null, new object[] { meshName, WindowsInvalidFileNameChars });

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test that meshes in one source file get separate outputs when their file names would collide.
        /// </summary>
        /// <param name="firstName">Name of the first mesh.</param>
        /// <param name="secondName">Name of the second mesh.</param>
        [TestCase("Hair", "Hair")]
        [TestCase("Hair?", "Hair_")]
        [TestCase("Parent/Child?", "Parent/Child_")]
        public void RemoveVertexColor_MeshesInOneSourceFile_KeepsEachMesh(string firstName, string secondName)
        {
            var tempFolder = $"{TempRoot}/{System.Guid.NewGuid():N}";
            Directory.CreateDirectory(tempFolder);
            AssetDatabase.Refresh();
            var root = new GameObject("AvatarRoot");
            try
            {
                var sourcePath = $"{tempFolder}/source.asset";
                var firstSource = CreateMeshWithVertexColors(firstName, 3);
                var secondSource = CreateMeshWithVertexColors(secondName, 4);

                // CreateAsset renames the main object to the file name, so both meshes must be sub-assets.
                AssetDatabase.CreateAsset(new Mesh(), sourcePath);
                AssetDatabase.AddObjectToAsset(firstSource, sourcePath);
                AssetDatabase.AddObjectToAsset(secondSource, sourcePath);
                AssetDatabase.SaveAssets();
                Assert.AreEqual(firstName, firstSource.name);
                Assert.AreEqual(secondName, secondSource.name);
                var firstFilter = CreateMeshObject(root, "First", firstSource);
                var secondFilter = CreateMeshObject(root, "Second", secondSource);

                InvokeRemoveVertexColor(new AvatarConverter(null), root, tempFolder);

                Assert.AreNotEqual(AssetDatabase.GetAssetPath(firstFilter.sharedMesh), AssetDatabase.GetAssetPath(secondFilter.sharedMesh));
                Assert.AreEqual(3, firstFilter.sharedMesh.vertexCount);
                Assert.AreEqual(4, secondFilter.sharedMesh.vertexCount);
                Assert.Zero(firstFilter.sharedMesh.colors32.Length);
                Assert.Zero(secondFilter.sharedMesh.colors32.Length);
            }
            finally
            {
                Object.DestroyImmediate(root);
                AssetDatabase.DeleteAsset(tempFolder);
            }
        }

        /// <summary>
        /// Test default menu icon settings in AvatarConverterSettings.
        /// </summary>
        [Test]
        public void AvatarConverterSettings_DefaultMenuIconSettings_AreExpectedValues()
        {
            var root = new GameObject("AvatarRoot");
            var settings = root.AddComponent<AvatarConverterSettings>();

            Assert.IsTrue(settings.resizeExpressionsMenuIcons);
            Assert.AreEqual(MenuIconResizer.TextureResizeMode.Max128x128, settings.expressionsMenuIconResizeMode);
            Assert.IsTrue(settings.compressExpressionsMenuIcons);
            Assert.AreEqual(MobileTextureFormat.ASTC_8x8, settings.expressionsMenuIconMobileTextureFormat);

            Object.DestroyImmediate(root);
        }

        /// <summary>
        /// Test that NetworkIDAssigner is added on manual conversion when auto assignment is enabled.
        /// </summary>
        [Test]
        public void ApplyVRCQuestToolsComponents_ManualConversionWithAutoAssignEnabled_AddsNetworkIDAssigner()
        {
            var root = new GameObject("AvatarRoot");
            var settings = root.AddComponent<AvatarConverterSettings>();
            settings.assignNetworkIds = true;
            var converter = new AvatarConverter(null);

            InvokeApplyVRCQuestToolsComponents(converter, settings, root, true);

            Assert.IsNotNull(root.GetComponent<NetworkIDAssigner>(), "NetworkIDAssigner should be added when assignNetworkIds is enabled in manual conversion.");
            Object.DestroyImmediate(root);
        }

        /// <summary>
        /// Test that NetworkIDAssigner is not added on manual conversion when auto assignment is disabled.
        /// </summary>
        [Test]
        public void ApplyVRCQuestToolsComponents_ManualConversionWithAutoAssignDisabled_DoesNotAddNetworkIDAssigner()
        {
            var root = new GameObject("AvatarRoot");
            var settings = root.AddComponent<AvatarConverterSettings>();
            settings.assignNetworkIds = false;
            var converter = new AvatarConverter(null);

            InvokeApplyVRCQuestToolsComponents(converter, settings, root, true);

            Assert.IsNull(root.GetComponent<NetworkIDAssigner>(), "NetworkIDAssigner should not be added when assignNetworkIds is disabled.");
            Object.DestroyImmediate(root);
        }

        /// <summary>
        /// Test that NetworkIDAssigner is not added by NDMF conversion path.
        /// </summary>
        [Test]
        public void ApplyVRCQuestToolsComponents_NdmfConversion_DoesNotAddNetworkIDAssigner()
        {
            var root = new GameObject("AvatarRoot");
            var settings = root.AddComponent<AvatarConverterSettings>();
            settings.assignNetworkIds = true;
            var converter = new AvatarConverter(null);

            InvokeApplyVRCQuestToolsComponents(converter, settings, root, false);

            Assert.IsNull(root.GetComponent<NetworkIDAssigner>(), "NDMF conversion path should not add NetworkIDAssigner.");
            Object.DestroyImmediate(root);
        }

#if VQT_HAS_NDMF
        /// <summary>
        /// Test that menu icon settings in AvatarConverterSettings are reflected to MenuIconResizer on manual conversion.
        /// </summary>
        [Test]
        public void ApplyVRCQuestToolsComponents_ManualConversionWithMenuIconSettings_AddsAndConfiguresMenuIconResizer()
        {
            var root = new GameObject("AvatarRoot");
            var settings = root.AddComponent<AvatarConverterSettings>();
            settings.resizeExpressionsMenuIcons = true;
            settings.expressionsMenuIconResizeMode = MenuIconResizer.TextureResizeMode.Max128x128;
            settings.compressExpressionsMenuIcons = true;
            settings.expressionsMenuIconMobileTextureFormat = MobileTextureFormat.ASTC_8x8;
            var converter = new AvatarConverter(null);

            InvokeApplyVRCQuestToolsComponents(converter, settings, root, true);

            var resizer = root.GetComponent<MenuIconResizer>();
            Assert.IsNotNull(resizer, "MenuIconResizer should be added when menu icon resize or compression is enabled.");
            Assert.AreEqual(MenuIconResizer.TextureResizeMode.Max128x128, resizer.resizeModeAndroid);
            Assert.IsTrue(resizer.compressTextures);
            Assert.AreEqual(MobileTextureFormat.ASTC_8x8, resizer.mobileTextureFormat);

            Object.DestroyImmediate(root);
        }

        /// <summary>
        /// Test that menu icon settings can compress without resizing.
        /// </summary>
        [Test]
        public void ApplyVRCQuestToolsComponents_ManualConversionWithMenuIconCompressionOnly_ConfiguresDoNotResize()
        {
            var root = new GameObject("AvatarRoot");
            var settings = root.AddComponent<AvatarConverterSettings>();
            settings.resizeExpressionsMenuIcons = false;
            settings.compressExpressionsMenuIcons = true;
            settings.expressionsMenuIconMobileTextureFormat = MobileTextureFormat.ASTC_8x8;
            var converter = new AvatarConverter(null);

            InvokeApplyVRCQuestToolsComponents(converter, settings, root, true);

            var resizer = root.GetComponent<MenuIconResizer>();
            Assert.IsNotNull(resizer, "MenuIconResizer should be added when menu icon compression is enabled.");
            Assert.AreEqual(MenuIconResizer.TextureResizeMode.DoNotResize, resizer.resizeModeAndroid);
            Assert.IsTrue(resizer.compressTextures);
            Assert.AreEqual(MobileTextureFormat.ASTC_8x8, resizer.mobileTextureFormat);

            Object.DestroyImmediate(root);
        }

        /// <summary>
        /// Test that MenuIconResizer is not added when both resize and compression are disabled.
        /// </summary>
        [Test]
        public void ApplyVRCQuestToolsComponents_ManualConversionWithMenuIconSettingsDisabled_DoesNotAddMenuIconResizer()
        {
            var root = new GameObject("AvatarRoot");
            var settings = root.AddComponent<AvatarConverterSettings>();
            settings.resizeExpressionsMenuIcons = false;
            settings.compressExpressionsMenuIcons = false;
            var converter = new AvatarConverter(null);

            InvokeApplyVRCQuestToolsComponents(converter, settings, root, true);

            Assert.IsNull(root.GetComponent<MenuIconResizer>(), "MenuIconResizer should not be added when both resize and compression are disabled.");
            Object.DestroyImmediate(root);
        }
#endif

        /// <summary>
        /// Test that a material which is already allowed for mobile avatars is not converted by default.
        /// </summary>
        [Test]
        public void RequiresConversion_MobileMaterialWithoutReplacement_ReturnsFalse()
        {
            var shader = Shader.Find(MobileToonLitShaderName);
            if (shader == null)
            {
                Assert.Ignore($"{MobileToonLitShaderName} shader not found.");
            }
            var material = new Material(shader);

            Assert.IsFalse(AvatarConverter.RequiresConversion(material, new ToonLitConvertSettings()));

            Object.DestroyImmediate(material);
        }

        /// <summary>
        /// Test that a material which is already allowed for mobile avatars is still replaced when the user
        /// explicitly configured a material replacement for it. See https://github.com/kurotu/VRCQuestTools/issues/293.
        /// </summary>
        [Test]
        public void RequiresConversion_MobileMaterialWithReplacement_ReturnsTrue()
        {
            var shader = Shader.Find(MobileToonLitShaderName);
            if (shader == null)
            {
                Assert.Ignore($"{MobileToonLitShaderName} shader not found.");
            }
            var material = new Material(shader);
            var replacement = new Material(shader);

            Assert.IsTrue(AvatarConverter.RequiresConversion(material, new MaterialReplaceSettings { material = replacement }));

            Object.DestroyImmediate(replacement);
            Object.DestroyImmediate(material);
        }

        /// <summary>
        /// Test that a material which is not allowed for mobile avatars is always converted.
        /// </summary>
        [Test]
        public void RequiresConversion_NonMobileMaterial_ReturnsTrue()
        {
            var material = new Material(Shader.Find("Standard"));

            Assert.IsTrue(AvatarConverter.RequiresConversion(material, new ToonLitConvertSettings()));

            Object.DestroyImmediate(material);
        }

        /// <summary>
        /// Test that VQT Material Swap replaces a material which is already allowed for mobile avatars.
        /// </summary>
        [Test]
        public void ConvertMaterialsForMobile_MaterialSwapOfMobileMaterial_ReplacesMaterial()
        {
            var shader = Shader.Find(MobileToonLitShaderName);
            if (shader == null)
            {
                Assert.Ignore($"{MobileToonLitShaderName} shader not found.");
            }
            var original = new Material(shader) { name = "Original" };
            var replacement = new Material(shader) { name = "Replacement" };

            var root = new GameObject("AvatarRoot");
            var swap = root.AddComponent<MaterialSwap>();
            swap.materialMappings = new List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping
                {
                    originalMaterial = original,
                    replacementMaterial = replacement,
                },
            };

            var converter = new AvatarConverter(new MaterialWrapperBuilder());
            var settingsMap = converter.CreateMaterialConvertSettingsMap(root, new Material[] { original });
            var converted = converter.ConvertMaterialsForMobile(settingsMap, false, string.Empty, null, false, root);

            Assert.IsTrue(converted.ContainsKey(original), "Mobile material specified as a swap target should be converted.");
            Assert.AreEqual(replacement, converted[original]);

            Object.DestroyImmediate(root);
            Object.DestroyImmediate(replacement);
            Object.DestroyImmediate(original);
        }

        /// <summary>
        /// Invokes AvatarConverter.ApplyPlatformGameObjectRemoversForAndroid by reflection.
        /// </summary>
        /// <param name="avatarObject">Avatar root object.</param>
        private static void InvokeApplyPlatformGameObjectRemoversForAndroid(GameObject avatarObject)
        {
            var type = typeof(AvatarConverter);
            var method = type.GetMethod("ApplyPlatformGameObjectRemoversForAndroid", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, "ApplyPlatformGameObjectRemoversForAndroid method should exist.");
            method.Invoke(null, new object[] { avatarObject });
        }

        /// <summary>
        /// Creates a mesh with vertex colors.
        /// </summary>
        /// <param name="name">Mesh name.</param>
        /// <param name="vertexCount">Vertex count, which identifies the mesh in assertions.</param>
        /// <returns>Created mesh.</returns>
        private static Mesh CreateMeshWithVertexColors(string name, int vertexCount)
        {
            var mesh = new Mesh { name = name };
            mesh.vertices = Enumerable.Range(0, vertexCount)
                .Select(i => Quaternion.Euler(0f, 0f, 360f * i / vertexCount) * Vector3.right)
                .ToArray();
            mesh.triangles = Enumerable.Range(1, vertexCount - 2)
                .SelectMany(i => new[] { 0, i, i + 1 })
                .ToArray();
            mesh.colors32 = Enumerable.Repeat(new Color32(255, 0, 0, 255), vertexCount).ToArray();
            return mesh;
        }

        /// <summary>
        /// Creates a child object which renders the mesh.
        /// </summary>
        /// <param name="root">Parent object.</param>
        /// <param name="name">Object name.</param>
        /// <param name="mesh">Mesh to render.</param>
        /// <returns>Created mesh filter.</returns>
        private static MeshFilter CreateMeshObject(GameObject root, string name, Mesh mesh)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(root.transform);
            obj.AddComponent<MeshRenderer>();
            var filter = obj.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            return filter;
        }

        /// <summary>
        /// Invokes AvatarConverter.RemoveVertexColor by reflection.
        /// </summary>
        /// <param name="converter">AvatarConverter instance.</param>
        /// <param name="avatarObject">Avatar root object.</param>
        /// <param name="assetsDirectory">Output directory.</param>
        private static void InvokeRemoveVertexColor(AvatarConverter converter, GameObject avatarObject, string assetsDirectory)
        {
            var method = typeof(AvatarConverter).GetMethod("RemoveVertexColor", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "RemoveVertexColor method should exist.");
            method.Invoke(converter, new object[] { avatarObject, assetsDirectory });
        }

        /// <summary>
        /// Invokes AvatarConverter.ApplyVRCQuestToolsComponents by reflection.
        /// </summary>
        /// <param name="converter">AvatarConverter instance.</param>
        /// <param name="settings">AvatarConverterSettings instance.</param>
        /// <param name="avatarObject">Avatar root object.</param>
        /// <param name="saveAssetsAsFile">Whether manual conversion mode is used.</param>
        private static void InvokeApplyVRCQuestToolsComponents(AvatarConverter converter, AvatarConverterSettings settings, GameObject avatarObject, bool saveAssetsAsFile)
        {
            var type = typeof(AvatarConverter);
            var method = type.GetMethod("ApplyVRCQuestToolsComponents", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, "ApplyVRCQuestToolsComponents method should exist.");
            method.Invoke(converter, new object[] { settings, avatarObject, saveAssetsAsFile });
        }
    }
}
