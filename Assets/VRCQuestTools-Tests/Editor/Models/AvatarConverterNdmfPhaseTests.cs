// <copyright file="AvatarConverterNdmfPhaseTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="AvatarConverterNdmfPhase"/> and <see cref="AvatarConverterNdmfPhaseExtension"/>.
    /// </summary>
    public class AvatarConverterNdmfPhaseTests
    {
        [Test]
        public void Resolve_Transforming_ReturnsTransforming()
        {
            var result = AvatarConverterNdmfPhase.Transforming.Resolve(null);
            Assert.AreEqual(AvatarConverterNdmfPhase.Transforming, result);
        }

        [Test]
        public void Resolve_Optimizing_ReturnsOptimizing()
        {
            var result = AvatarConverterNdmfPhase.Optimizing.Resolve(null);
            Assert.AreEqual(AvatarConverterNdmfPhase.Optimizing, result);
        }

        [Test]
        public void Resolve_Auto_NullAvatar_ReturnsOptimizing()
        {
            var result = AvatarConverterNdmfPhase.Auto.Resolve(null);
            Assert.AreEqual(AvatarConverterNdmfPhase.Optimizing, result);
        }

        [Test]
        public void Resolve_Auto_WithGameObject_ReturnsOptimizing()
        {
            // Without VRCFury, Auto resolves to Optimizing
            var go = new GameObject("TestAvatar");
            try
            {
                var result = AvatarConverterNdmfPhase.Auto.Resolve(go);
                Assert.AreEqual(AvatarConverterNdmfPhase.Optimizing, result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Resolve_Transforming_WithGameObject_StillReturnsTransforming()
        {
            var go = new GameObject("TestAvatar");
            try
            {
                var result = AvatarConverterNdmfPhase.Transforming.Resolve(go);
                Assert.AreEqual(AvatarConverterNdmfPhase.Transforming, result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
