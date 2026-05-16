// <copyright file="AvatarConverterTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Reflection;
using KRT.VRCQuestTools.Components;
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
    }
}
