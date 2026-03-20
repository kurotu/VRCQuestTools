// <copyright file="PlatformGameObjectRemoverTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Components;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="PlatformGameObjectRemover"/>.
    /// </summary>
    public class PlatformGameObjectRemoverTests
    {
        [Test]
        public void DefaultValues_AreFalse()
        {
            var go = new GameObject("Test");
            try
            {
                var remover = go.AddComponent<PlatformGameObjectRemover>();
                Assert.IsFalse(remover.removeOnPC);
                Assert.IsFalse(remover.removeOnAndroid);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void RemoveOnPC_CanBeSet()
        {
            var go = new GameObject("Test");
            try
            {
                var remover = go.AddComponent<PlatformGameObjectRemover>();
                remover.removeOnPC = true;
                Assert.IsTrue(remover.removeOnPC);
                Assert.IsFalse(remover.removeOnAndroid);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void RemoveOnAndroid_CanBeSet()
        {
            var go = new GameObject("Test");
            try
            {
                var remover = go.AddComponent<PlatformGameObjectRemover>();
                remover.removeOnAndroid = true;
                Assert.IsFalse(remover.removeOnPC);
                Assert.IsTrue(remover.removeOnAndroid);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void BothFlags_CanBeSetToTrue()
        {
            var go = new GameObject("Test");
            try
            {
                var remover = go.AddComponent<PlatformGameObjectRemover>();
                remover.removeOnPC = true;
                remover.removeOnAndroid = true;
                Assert.IsTrue(remover.removeOnPC);
                Assert.IsTrue(remover.removeOnAndroid);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
