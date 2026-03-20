// <copyright file="VRCPhysBoneColliderProviderTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models.VRChat;
using NUnit.Framework;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="VRCPhysBoneColliderProvider"/>.
    /// </summary>
    public class VRCPhysBoneColliderProviderTests
    {
        private GameObject go;
        private VRCPhysBoneCollider collider;
        private VRCPhysBoneColliderProvider provider;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("TestCollider");
            collider = go.AddComponent<VRCPhysBoneCollider>();
            provider = new VRCPhysBoneColliderProvider(collider);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Component_ReturnsCollider()
        {
            Assert.AreEqual(collider, provider.Component);
        }

        [Test]
        public void GameObject_ReturnsComponentGameObject()
        {
            Assert.AreEqual(go, provider.GameObject);
        }

        [Test]
        public void ComponentType_IsPhysBoneCollider()
        {
            Assert.AreEqual(AvatarDynamicsComponentType.PhysBoneCollider, provider.ComponentType);
        }

        [Test]
        public void ShapeType_ReturnsColliderShapeType()
        {
            collider.shapeType = VRCPhysBoneColliderBase.ShapeType.Capsule;
            Assert.AreEqual(VRCPhysBoneColliderBase.ShapeType.Capsule, provider.ShapeType);
        }

        [Test]
        public void Radius_ReturnsColliderRadius()
        {
            collider.radius = 0.3f;
            Assert.AreEqual(0.3f, provider.Radius);
        }

        [Test]
        public void Height_ReturnsColliderHeight()
        {
            collider.height = 1.5f;
            Assert.AreEqual(1.5f, provider.Height);
        }
    }
}
