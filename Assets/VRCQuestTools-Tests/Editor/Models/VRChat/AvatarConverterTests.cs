// <copyright file="AvatarConverterTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Reflection;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.VRChat
{
    /// <summary>
    /// Tests for AvatarConverter.
    /// </summary>
    public class AvatarConverterTests
    {
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
