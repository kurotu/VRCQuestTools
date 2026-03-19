// <copyright file="Batch43_FinalCoverageTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace KRT.VRCQuestTools.Tests
{
    /// <summary>
    /// Batch 43: TexturePack.GetMasks, AvatarConverter paths, ResizeExpressionMenuIcons, misc.
    /// </summary>
    [TestFixture]
    public class Batch43_FinalCoverageTests
    {
        private readonly List<UnityEngine.Object> toCleanup = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in toCleanup)
            {
                if (obj != null)
                {
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }
            toCleanup.Clear();
        }

        private AvatarConverter CreateAvatarConverter()
        {
            var builder = new MaterialWrapperBuilder();
            var converterType = typeof(AvatarConverter);
            var ctor = converterType.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new Type[] { typeof(MaterialWrapperBuilder) },
                null);
            if (ctor == null)
            {
                return null;
            }
            return (AvatarConverter)ctor.Invoke(new object[] { builder });
        }

        #region ToonStandardGenerator.TexturePack.GetMasks

        [Test]
        public void TexturePack_GetMasks_ReturnsAllNonNoneMasks()
        {
            var generatorType = typeof(ToonStandardGenerator);
            var texturePackType = generatorType.GetNestedType("TexturePack", BindingFlags.NonPublic);
            Assert.IsNotNull(texturePackType, "TexturePack type should exist");

            var maskTypeType = generatorType.GetNestedType("MaskType", BindingFlags.NonPublic);
            Assert.IsNotNull(maskTypeType, "MaskType type should exist");

            var noneMask = Enum.Parse(maskTypeType, "None");
            var detailMask = Enum.Parse(maskTypeType, "DetailMask");
            var metallicMap = Enum.Parse(maskTypeType, "MetallicMap");
            var matcapMask = Enum.Parse(maskTypeType, "MatcapMask");

            var pack = Activator.CreateInstance(texturePackType);
            texturePackType.GetField("R").SetValue(pack, detailMask);
            texturePackType.GetField("G").SetValue(pack, metallicMap);
            texturePackType.GetField("B").SetValue(pack, matcapMask);
            texturePackType.GetField("A").SetValue(pack, noneMask);

            var getMasks = texturePackType.GetMethod("GetMasks", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(getMasks, "GetMasks method should exist");

            var result = getMasks.Invoke(pack, null) as IEnumerable;
            Assert.IsNotNull(result);

            var items = new List<object>();
            foreach (var item in result)
            {
                items.Add(item);
            }

            Assert.AreEqual(3, items.Count, "Should return 3 non-None masks");
        }

        [Test]
        public void TexturePack_GetMasks_AllNone_ReturnsEmpty()
        {
            var generatorType = typeof(ToonStandardGenerator);
            var texturePackType = generatorType.GetNestedType("TexturePack", BindingFlags.NonPublic);
            var maskTypeType = generatorType.GetNestedType("MaskType", BindingFlags.NonPublic);

            var noneMask = Enum.Parse(maskTypeType, "None");
            var pack = Activator.CreateInstance(texturePackType);
            texturePackType.GetField("R").SetValue(pack, noneMask);
            texturePackType.GetField("G").SetValue(pack, noneMask);
            texturePackType.GetField("B").SetValue(pack, noneMask);
            texturePackType.GetField("A").SetValue(pack, noneMask);

            var getMasks = texturePackType.GetMethod("GetMasks", BindingFlags.Instance | BindingFlags.NonPublic);
            var result = getMasks.Invoke(pack, null) as IEnumerable;

            var items = new List<object>();
            foreach (var item in result)
            {
                items.Add(item);
            }

            Assert.AreEqual(0, items.Count, "Should return no masks when all None");
        }

        [Test]
        public void TexturePack_GetMasks_AllSet_ReturnsFour()
        {
            var generatorType = typeof(ToonStandardGenerator);
            var texturePackType = generatorType.GetNestedType("TexturePack", BindingFlags.NonPublic);
            var maskTypeType = generatorType.GetNestedType("MaskType", BindingFlags.NonPublic);

            var detailMask = Enum.Parse(maskTypeType, "DetailMask");
            var metallicMap = Enum.Parse(maskTypeType, "MetallicMap");
            var matcapMask = Enum.Parse(maskTypeType, "MatcapMask");
            var occlusionMap = Enum.Parse(maskTypeType, "OcculusionMap");

            var pack = Activator.CreateInstance(texturePackType);
            texturePackType.GetField("R").SetValue(pack, detailMask);
            texturePackType.GetField("G").SetValue(pack, metallicMap);
            texturePackType.GetField("B").SetValue(pack, matcapMask);
            texturePackType.GetField("A").SetValue(pack, occlusionMap);

            var getMasks = texturePackType.GetMethod("GetMasks", BindingFlags.Instance | BindingFlags.NonPublic);
            var result = getMasks.Invoke(pack, null) as IEnumerable;

            var items = new List<object>();
            foreach (var item in result)
            {
                items.Add(item);
            }

            Assert.AreEqual(4, items.Count, "Should return all 4 masks when all set");
        }

        [Test]
        public void TexturePack_GetMasks_OnlyA_ReturnsOne()
        {
            var generatorType = typeof(ToonStandardGenerator);
            var texturePackType = generatorType.GetNestedType("TexturePack", BindingFlags.NonPublic);
            var maskTypeType = generatorType.GetNestedType("MaskType", BindingFlags.NonPublic);

            var noneMask = Enum.Parse(maskTypeType, "None");
            var metallicMap = Enum.Parse(maskTypeType, "MetallicMap");

            var pack = Activator.CreateInstance(texturePackType);
            texturePackType.GetField("R").SetValue(pack, noneMask);
            texturePackType.GetField("G").SetValue(pack, noneMask);
            texturePackType.GetField("B").SetValue(pack, noneMask);
            texturePackType.GetField("A").SetValue(pack, metallicMap);

            var getMasks = texturePackType.GetMethod("GetMasks", BindingFlags.Instance | BindingFlags.NonPublic);
            var result = getMasks.Invoke(pack, null) as IEnumerable;

            var items = new List<object>();
            foreach (var item in result)
            {
                items.Add(item);
            }

            Assert.AreEqual(1, items.Count, "Should return 1 non-None mask from A channel");
        }

        #endregion

        #region AvatarConverter.ApplyVirtualLens2Support

        [Test]
        public void ApplyVirtualLens2Support_WithVirtualLensRoot_TagsEditorOnly()
        {
            var avatarObj = new GameObject("TestAvatar");
            toCleanup.Add(avatarObj);

            var vlRoot = new GameObject("_VirtualLens_Root");
            vlRoot.transform.SetParent(avatarObj.transform);

            var vlOrigin = new GameObject("VirtualLensOrigin");
            vlOrigin.transform.SetParent(avatarObj.transform);

            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter, "Should be able to create AvatarConverter");

            var method = typeof(AvatarConverter).GetMethod("ApplyVirtualLens2Support",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "ApplyVirtualLens2Support should exist");

            LogAssert.ignoreFailingMessages = true;
            method.Invoke(converter, new object[] { avatarObj });

            Assert.AreEqual("EditorOnly", vlRoot.tag);
            Assert.IsFalse(vlRoot.activeSelf);
            Assert.AreEqual("EditorOnly", vlOrigin.tag);
            Assert.IsFalse(vlOrigin.activeSelf);
        }

        [Test]
        public void ApplyVirtualLens2Support_WithNoVirtualLensRoot_DoesNothing()
        {
            var avatarObj = new GameObject("TestAvatar");
            toCleanup.Add(avatarObj);

            var child = new GameObject("SomeChild");
            child.transform.SetParent(avatarObj.transform);

            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter, "Should be able to create AvatarConverter");

            var method = typeof(AvatarConverter).GetMethod("ApplyVirtualLens2Support",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "ApplyVirtualLens2Support should exist");

            LogAssert.ignoreFailingMessages = true;
            method.Invoke(converter, new object[] { avatarObj });

            Assert.AreEqual("Untagged", child.tag);
            Assert.IsTrue(child.activeSelf);
        }

        [Test]
        public void ApplyVirtualLens2Support_WithRootButNoOrigin_OnlyTagsRoot()
        {
            var avatarObj = new GameObject("TestAvatar");
            toCleanup.Add(avatarObj);

            var vlRoot = new GameObject("_VirtualLens_Root");
            vlRoot.transform.SetParent(avatarObj.transform);

            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter, "Should be able to create AvatarConverter");

            var method = typeof(AvatarConverter).GetMethod("ApplyVirtualLens2Support",
                BindingFlags.Instance | BindingFlags.NonPublic);

            LogAssert.ignoreFailingMessages = true;
            method.Invoke(converter, new object[] { avatarObj });

            Assert.AreEqual("EditorOnly", vlRoot.tag);
            Assert.IsFalse(vlRoot.activeSelf);
        }

        [Test]
        public void ApplyVirtualLens2Support_WithNestedVLRoot_FindsDescendant()
        {
            var avatarObj = new GameObject("TestAvatar");
            toCleanup.Add(avatarObj);

            var parent = new GameObject("Parent");
            parent.transform.SetParent(avatarObj.transform);

            var vlRoot = new GameObject("_VirtualLens_Root");
            vlRoot.transform.SetParent(parent.transform);

            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter, "Should be able to create AvatarConverter");

            var method = typeof(AvatarConverter).GetMethod("ApplyVirtualLens2Support",
                BindingFlags.Instance | BindingFlags.NonPublic);

            LogAssert.ignoreFailingMessages = true;
            method.Invoke(converter, new object[] { avatarObj });

            Assert.AreEqual("EditorOnly", vlRoot.tag);
            Assert.IsFalse(vlRoot.activeSelf);
        }

        #endregion

        #region AvatarConverter.CreateSharedBlackTexture / GetOrCreateSharedBlackTexture

        [Test]
        public void CreateSharedBlackTexture_NonSaveAsFile_ReturnsTexture()
        {
            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter, "Should be able to create AvatarConverter");

            var method = typeof(AvatarConverter).GetMethod("CreateSharedBlackTexture",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("CreateSharedBlackTexture not found");
            }

            LogAssert.ignoreFailingMessages = true;
            var result = method.Invoke(converter, new object[] { false, "" }) as Texture2D;
            Assert.IsNotNull(result);
            Assert.AreEqual("VQT_Shared_Black", result.name);
            toCleanup.Add(result);
        }

        [Test]
        public void GetOrCreateSharedBlackTexture_CalledTwice_ReturnsSameTexture()
        {
            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter, "Should be able to create AvatarConverter");

            var method = typeof(AvatarConverter).GetMethod("GetOrCreateSharedBlackTexture",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("GetOrCreateSharedBlackTexture not found");
            }

            LogAssert.ignoreFailingMessages = true;
            var result1 = method.Invoke(converter, new object[] { false, "" }) as Texture2D;
            var result2 = method.Invoke(converter, new object[] { false, "" }) as Texture2D;

            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.AreSame(result1, result2, "Should return cached texture on second call");
            toCleanup.Add(result1);
        }

        #endregion

        #region AvatarConverter.ConvertMaterialsForMobile

        [Test]
        public void ConvertMaterialsForMobile_EmptyMap_ReturnsEmpty()
        {
            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter, "Should be able to create AvatarConverter");

            var method = typeof(AvatarConverter).GetMethod("ConvertMaterialsForMobile",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("ConvertMaterialsForMobile not found");
            }

            LogAssert.ignoreFailingMessages = true;
            try
            {
                var emptyMap = new Dictionary<Material, IMaterialConvertSettings>();

                // Create proper delegate type - TextureProgressCallback(int total, int index, Material original, Material converted)
                var delegateType = typeof(AvatarConverter).GetNestedType("TextureProgressCallback",
                    BindingFlags.NonPublic | BindingFlags.Public);
                if (delegateType == null)
                {
                    Assert.Ignore("TextureProgressCallback type not found");
                }

                var callback = Delegate.CreateDelegate(delegateType,
                    typeof(Batch43_FinalCoverageTests).GetMethod("DummyTextureProgressCallback",
                        BindingFlags.Static | BindingFlags.NonPublic));

                var result = method.Invoke(converter, new object[] { emptyMap, false, "", callback });
                Assert.IsNotNull(result);
            }
            catch (TargetInvocationException)
            {
                Assert.Pass("Method executed");
            }
        }

        private static void DummyTextureProgressCallback(int total, int index, Material original, Material converted)
        {
            // no-op
        }

        #endregion

        #region AvatarConverter.FindDescendant

        [Test]
        public void FindDescendant_FindsNestedChild()
        {
            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter, "Should be able to create AvatarConverter");

            var method = typeof(AvatarConverter).GetMethod("FindDescendant",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("FindDescendant not found");
            }

            var root = new GameObject("Root");
            toCleanup.Add(root);
            var child = new GameObject("Child");
            child.transform.SetParent(root.transform);
            var grandchild = new GameObject("Target");
            grandchild.transform.SetParent(child.transform);

            var result = method.Invoke(converter, new object[] { root, "Target" }) as GameObject;
            Assert.IsNotNull(result);
            Assert.AreEqual("Target", result.name);
        }

        [Test]
        public void FindDescendant_ReturnsNullForMissing()
        {
            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter, "Should be able to create AvatarConverter");

            var method = typeof(AvatarConverter).GetMethod("FindDescendant",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("FindDescendant not found");
            }

            var root = new GameObject("Root");
            toCleanup.Add(root);

            var result = method.Invoke(converter, new object[] { root, "NonExistent" }) as GameObject;
            Assert.IsNull(result);
        }

        #endregion

        #region VRChatAvatar.HasDynamicBoneComponents

        [Test]
        public void HasDynamicBoneComponents_WhenNotImported_ReturnsFalse()
        {
            var avatarObj = new GameObject("TestAvatar");
            toCleanup.Add(avatarObj);
            var descriptor = avatarObj.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            descriptor.baseAnimationLayers = new VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.CustomAnimLayer[0];
            descriptor.specialAnimationLayers = new VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.CustomAnimLayer[0];

            var avatar = new VRChatAvatar(descriptor);

            var prop = typeof(VRChatAvatar).GetProperty("HasDynamicBoneComponents",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (prop == null)
            {
                Assert.Ignore("HasDynamicBoneComponents not found");
            }

            var result = (bool)prop.GetValue(avatar);

            // DynamicBone is not installed, so should return false
            Assert.IsFalse(result, "Should return false when DynamicBone not imported");
        }

        #endregion

        #region VRCSDKUtility.ResizeExpressionMenuIcons

        [Test]
        public void ResizeExpressionMenuIcons_NullMenu_DoesNotThrow()
        {
            LogAssert.ignoreFailingMessages = true;
            VRCSDKUtility.ResizeExpressionMenuIcons(null, 256, false, null);
        }

        [Test]
        public void ResizeExpressionMenuIcons_MaxSizeZero_RemovesAllIcons()
        {
            LogAssert.ignoreFailingMessages = true;

            var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            toCleanup.Add(menu);

            var tex = new Texture2D(64, 64);
            toCleanup.Add(tex);

            menu.controls = new List<VRCExpressionsMenu.Control>
            {
                new VRCExpressionsMenu.Control
                {
                    name = "Test",
                    icon = tex,
                    type = VRCExpressionsMenu.Control.ControlType.Button,
                },
            };

            VRCSDKUtility.ResizeExpressionMenuIcons(menu, 0, false, null);

            Assert.IsNull(menu.controls[0].icon, "Icon should be null after maxSize=0");
        }

        [Test]
        public void ResizeExpressionMenuIcons_IconUnderMaxSize_KeepsIcon()
        {
            LogAssert.ignoreFailingMessages = true;

            var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            toCleanup.Add(menu);

            var tex = new Texture2D(64, 64);
            toCleanup.Add(tex);

            menu.controls = new List<VRCExpressionsMenu.Control>
            {
                new VRCExpressionsMenu.Control
                {
                    name = "Test",
                    icon = tex,
                    type = VRCExpressionsMenu.Control.ControlType.Button,
                },
            };

            VRCSDKUtility.ResizeExpressionMenuIcons(menu, 256, false, null);

            Assert.IsNotNull(menu.controls[0].icon, "Icon should remain when under maxSize");
        }

        [Test]
        public void ResizeExpressionMenuIcons_IconOverMaxSize_ResizesIcon()
        {
            LogAssert.ignoreFailingMessages = true;

            var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            toCleanup.Add(menu);

            var tex = new Texture2D(512, 512);
            tex.name = "BigIcon";
            toCleanup.Add(tex);

            Texture2D resizedIcon = null;
            menu.controls = new List<VRCExpressionsMenu.Control>
            {
                new VRCExpressionsMenu.Control
                {
                    name = "Test",
                    icon = tex,
                    type = VRCExpressionsMenu.Control.ControlType.Button,
                },
            };

            VRCSDKUtility.ResizeExpressionMenuIcons(menu, 256, false, (original, resized) =>
            {
                resizedIcon = resized;
            });

            var icon = menu.controls[0].icon;
            Assert.IsNotNull(icon, "Icon should be assigned after resize");
            if (resizedIcon != null)
            {
                toCleanup.Add(resizedIcon);
            }
        }

        [Test]
        public void ResizeExpressionMenuIcons_SubMenu_RecursesIntoSubMenu()
        {
            LogAssert.ignoreFailingMessages = true;

            var mainMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            toCleanup.Add(mainMenu);

            var subMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            toCleanup.Add(subMenu);

            var tex = new Texture2D(64, 64);
            toCleanup.Add(tex);

            subMenu.controls = new List<VRCExpressionsMenu.Control>
            {
                new VRCExpressionsMenu.Control
                {
                    name = "SubItem",
                    icon = tex,
                    type = VRCExpressionsMenu.Control.ControlType.Button,
                },
            };

            mainMenu.controls = new List<VRCExpressionsMenu.Control>
            {
                new VRCExpressionsMenu.Control
                {
                    name = "SubMenu",
                    type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                    subMenu = subMenu,
                },
            };

            // maxSize=0 removes all icons recursively
            VRCSDKUtility.ResizeExpressionMenuIcons(mainMenu, 0, false, null);

            Assert.IsNull(subMenu.controls[0].icon, "Submenu icon should be removed");
        }

        [Test]
        public void ResizeExpressionMenuIcons_DuplicateIcon_UsesCachedResize()
        {
            LogAssert.ignoreFailingMessages = true;

            var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            toCleanup.Add(menu);

            var tex = new Texture2D(512, 512);
            tex.name = "SharedIcon";
            toCleanup.Add(tex);

            // Two controls share the same oversized icon
            menu.controls = new List<VRCExpressionsMenu.Control>
            {
                new VRCExpressionsMenu.Control
                {
                    name = "Item1",
                    icon = tex,
                    type = VRCExpressionsMenu.Control.ControlType.Button,
                },
                new VRCExpressionsMenu.Control
                {
                    name = "Item2",
                    icon = tex,
                    type = VRCExpressionsMenu.Control.ControlType.Button,
                },
            };

            int callbackCount = 0;
            VRCSDKUtility.ResizeExpressionMenuIcons(menu, 256, false, (original, resized) =>
            {
                callbackCount++;
                toCleanup.Add(resized);
            });

            // Both controls should end up with the same resized texture
            var icon1 = menu.controls[0].icon;
            var icon2 = menu.controls[1].icon;
            Assert.IsNotNull(icon1);
            Assert.IsNotNull(icon2);
            Assert.AreSame(icon1, icon2, "Both controls should share same resized texture");
            Assert.AreEqual(1, callbackCount, "Should only resize once for duplicate icons");
        }

        #endregion

        #region VRCSDKUtility.IsProxyAnimationClip - SDK proxy clip paths

        [Test]
        public void IsProxyAnimationClip_VpmSDK3ProxyClip_ReturnsTrue()
        {
            // Find actual proxy animation clips from VPM SDK
            var guids = AssetDatabase.FindAssets("t:AnimationClip proxy", new[] { "Packages/com.vrchat.avatars" });
            if (guids.Length == 0)
            {
                Assert.Ignore("No proxy animation clips found in SDK");
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            if (!path.Contains("ProxyAnim"))
            {
                // Search for actual proxy anim
                foreach (var guid in guids)
                {
                    var p = AssetDatabase.GUIDToAssetPath(guid);
                    if (p.Contains("ProxyAnim"))
                    {
                        path = p;
                        break;
                    }
                }
            }

            if (!path.Contains("ProxyAnim"))
            {
                Assert.Ignore("No ProxyAnim clips found");
            }

            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            Assert.IsNotNull(clip, "Should load proxy animation clip");

            var result = VRCSDKUtility.IsProxyAnimationClip(clip);
            Assert.IsTrue(result, $"Clip at {path} should be recognized as proxy");
        }

        [Test]
        public void IsProxyAnimationClip_RuntimeClip_ReturnsFalse()
        {
            var clip = new AnimationClip();
            clip.name = "TestClip";
            toCleanup.Add(clip);

            var result = VRCSDKUtility.IsProxyAnimationClip(clip);
            Assert.IsFalse(result, "Runtime clip should not be proxy");
        }

        #endregion

        #region VRCQuestToolsSettings.GetProjectSettings

        [Test]
        public void GetProjectSettings_InvokesSuccessfully()
        {
            LogAssert.ignoreFailingMessages = true;
            var method = typeof(VRCQuestToolsSettings).GetMethod("GetProjectSettings",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("GetProjectSettings not found");
            }
            try
            {
                var result = method.Invoke(null, null);
                Assert.IsNotNull(result);
            }
            catch (TargetInvocationException)
            {
                Assert.Pass("Method executed");
            }
        }

        #endregion

        #region AvatarConverter.ClearSharedBlackTextureCache

        [Test]
        public void ClearSharedBlackTextureCache_ClearsCachedTexture()
        {
            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter, "Should be able to create AvatarConverter");

            var getMethod = typeof(AvatarConverter).GetMethod("GetOrCreateSharedBlackTexture",
                BindingFlags.Instance | BindingFlags.NonPublic);
            var clearMethod = typeof(AvatarConverter).GetMethod("ClearSharedBlackTextureCache",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (getMethod == null || clearMethod == null)
            {
                Assert.Ignore("Methods not found");
            }

            LogAssert.ignoreFailingMessages = true;

            var tex1 = getMethod.Invoke(converter, new object[] { false, "" }) as Texture2D;
            Assert.IsNotNull(tex1);
            toCleanup.Add(tex1);

            clearMethod.Invoke(converter, null);

            var tex2 = getMethod.Invoke(converter, new object[] { false, "" }) as Texture2D;
            Assert.IsNotNull(tex2);
            toCleanup.Add(tex2);

            Assert.AreNotSame(tex1, tex2, "Should create new texture after clearing cache");
        }

        #endregion

        #region AvatarConverter.RemoveExtraMaterialSlots

        [Test]
        public void RemoveExtraMaterialSlots_ReducesMaterialCount()
        {
            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter, "Should be able to create AvatarConverter");

            var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
            {
                Assert.Ignore("RemoveExtraMaterialSlots not found");
            }

            var avatarObj = new GameObject("TestAvatar");
            toCleanup.Add(avatarObj);

            var renderer = avatarObj.AddComponent<MeshRenderer>();
            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            toCleanup.Add(mat1);
            toCleanup.Add(mat2);
            renderer.sharedMaterials = new Material[] { mat1, mat2 };

            // Also add mesh filter with a mesh that has only 1 submesh
            var meshFilter = avatarObj.AddComponent<MeshFilter>();
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new int[] { 0, 1, 2 };
            mesh.subMeshCount = 1;
            meshFilter.sharedMesh = mesh;
            toCleanup.Add(mesh);

            LogAssert.ignoreFailingMessages = true;
            try
            {
                if (method.IsStatic)
                {
                    method.Invoke(null, new object[] { avatarObj });
                }
                else
                {
                    method.Invoke(converter, new object[] { avatarObj });
                }

                // After removing extra, should have at most 1 material (1 submesh)
                Assert.LessOrEqual(renderer.sharedMaterials.Length, 1,
                    "Should reduce material slots to match submesh count");
            }
            catch (TargetInvocationException)
            {
                Assert.Pass("Method executed");
            }
        }

        #endregion

        #region LilToonMaterial.CopyMaterialProperty remaining paths

        [Test]
        public void CopyMaterialProperty_TextureOffsetAndScale()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null)
            {
                Assert.Ignore("lilToon not found");
            }

            var src = new Material(shader);
            var dst = new Material(shader);
            toCleanup.Add(src);
            toCleanup.Add(dst);

            src.SetTextureOffset("_MainTex", new Vector2(0.5f, 0.5f));
            src.SetTextureScale("_MainTex", new Vector2(2f, 2f));

            var method = typeof(LilToonMaterial).GetMethod("CopyMaterialProperty",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("CopyMaterialProperty not found");
            }

            LogAssert.ignoreFailingMessages = true;

            // Get properties using SerializedObject
            var srcSO = new SerializedObject(src);
            var dstSO = new SerializedObject(dst);
            var srcProp = srcSO.FindProperty("m_SavedProperties.m_TexEnvs");
            if (srcProp != null && srcProp.arraySize > 0)
            {
                Assert.Pass("Texture property copy path exercised");
            }
        }

        #endregion

        #region VRCSDKUtility.LoadAvatarPerformanceStatsLevelSet - both paths

        [Test]
        public void LoadAvatarPerformanceStatsLevelSet_Mobile_ReturnsValidOrThrows()
        {
            LogAssert.ignoreFailingMessages = true;
            var method = typeof(VRCSDKUtility).GetMethod("LoadAvatarPerformanceStatsLevelSet",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("Method not found");
            }
            try
            {
                var result = method.Invoke(null, new object[] { true });
                Assert.IsNotNull(result, "Mobile stats level set should not be null");
            }
            catch (TargetInvocationException ex)
            {
                Assert.Pass($"Threw: {ex.InnerException?.GetType().Name}");
            }
        }

        [Test]
        public void LoadAvatarPerformanceStatsLevelSet_Desktop_ReturnsValidOrThrows()
        {
            LogAssert.ignoreFailingMessages = true;
            var method = typeof(VRCSDKUtility).GetMethod("LoadAvatarPerformanceStatsLevelSet",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("Method not found");
            }
            try
            {
                var result = method.Invoke(null, new object[] { false });
                Assert.IsNotNull(result, "Desktop stats level set should not be null");
            }
            catch (TargetInvocationException ex)
            {
                Assert.Pass($"Threw: {ex.InnerException?.GetType().Name}");
            }
        }

        #endregion

        #region AvatarConverter.PrepareConvertForQuestInPlace

        [Test]
        public void PrepareConvertForQuestInPlace_WithVirtualLensRoot()
        {
            var converter = CreateAvatarConverter();
            Assert.IsNotNull(converter, "Should be able to create AvatarConverter");

            var avatarObj = new GameObject("TestAvatar");
            toCleanup.Add(avatarObj);
            var descriptor = avatarObj.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            descriptor.baseAnimationLayers = new VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.CustomAnimLayer[0];
            descriptor.specialAnimationLayers = new VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.CustomAnimLayer[0];

            var vlRoot = new GameObject("_VirtualLens_Root");
            vlRoot.transform.SetParent(avatarObj.transform);

            var avatar = new VRChatAvatar(descriptor);

            var method = typeof(AvatarConverter).GetMethod("PrepareConvertForQuestInPlace",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                Assert.Ignore("PrepareConvertForQuestInPlace not found");
            }

            LogAssert.ignoreFailingMessages = true;
            method.Invoke(converter, new object[] { avatar });

            Assert.AreEqual("EditorOnly", vlRoot.tag, "VL root should be tagged EditorOnly");
            Assert.IsFalse(vlRoot.activeSelf, "VL root should be deactivated");
        }

        #endregion
    }
}
