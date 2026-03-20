// <copyright file="VRCPhysBoneProviderTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models.VRChat;
using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="VRCPhysBoneProvider"/>.
    /// </summary>
    public class VRCPhysBoneProviderTests
    {
        private GameObject go;
        private VRCPhysBone physBone;
        private VRCPhysBoneProvider provider;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("TestPhysBone");
            physBone = go.AddComponent<VRCPhysBone>();
            provider = new VRCPhysBoneProvider(physBone);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Component_ReturnsPhysBone()
        {
            Assert.AreEqual(physBone, provider.Component);
        }

        [Test]
        public void GameObject_ReturnsGameObject()
        {
            Assert.AreEqual(go, provider.GameObject);
        }

        [Test]
        public void RootTransform_WhenNull_ReturnsSelf()
        {
            physBone.rootTransform = null;
            Assert.AreEqual(go.transform, provider.RootTransform);
        }

        [Test]
        public void RootTransform_WhenSet_ReturnsSetTransform()
        {
            var root = new GameObject("Root");
            physBone.rootTransform = root.transform;
            Assert.AreEqual(root.transform, provider.RootTransform);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void IgnoreTransforms_ReturnsListReference()
        {
            Assert.IsNotNull(provider.IgnoreTransforms);
        }

        [Test]
        public void EndpointPosition_ReturnsValue()
        {
            physBone.endpointPosition = new Vector3(1, 2, 3);
            Assert.AreEqual(new Vector3(1, 2, 3), provider.EndpointPosition);
        }

        [Test]
        public void MultiChildType_ReturnsValue()
        {
            var mt = provider.MultiChildType;
            Assert.IsTrue(System.Enum.IsDefined(typeof(MultiChildType), mt));
        }

        [Test]
        public void Colliders_ReturnsListReference()
        {
            Assert.IsNotNull(provider.Colliders);
        }

        [Test]
        public void Radius_ReturnsValue()
        {
            physBone.radius = 0.5f;
            Assert.AreEqual(0.5f, provider.Radius);
        }

        [Test]
        public void RadiusCurve_SetAndRetrieve()
        {
            physBone.radiusCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
            Assert.IsNotNull(provider.RadiusCurve);
            Assert.AreEqual(2, provider.RadiusCurve.length);
        }

        [Test]
        public void GetPhysBones_ReturnsPhysBone()
        {
            var bones = provider.GetPhysBones();
            Assert.AreEqual(1, bones.Length);
            Assert.AreEqual(physBone, bones[0]);
        }

        [Test]
        public void ClearCollider_ValidIndex_SetsNull()
        {
            var colliderGo = new GameObject("Collider");
            var collider = colliderGo.AddComponent<VRCPhysBoneCollider>();
            physBone.colliders.Add(collider);

            provider.ClearCollider(0);
            Assert.IsNull(physBone.colliders[0]);

            Object.DestroyImmediate(colliderGo);
        }

        [Test]
        public void ClearCollider_InvalidIndex_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => provider.ClearCollider(-1));
            Assert.DoesNotThrow(() => provider.ClearCollider(100));
        }
    }
}
