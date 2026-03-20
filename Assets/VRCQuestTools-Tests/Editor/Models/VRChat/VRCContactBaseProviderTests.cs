// <copyright file="VRCContactBaseProviderTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models.VRChat;
using NUnit.Framework;
using UnityEngine;
using VRC.Dynamics;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="VRCContactBaseProvider"/>.
    /// </summary>
    public class VRCContactBaseProviderTests
    {
        private GameObject go;
        private ContactReceiver contact;
        private VRCContactBaseProvider provider;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("TestContact");
            contact = go.AddComponent<ContactReceiver>();
            provider = new VRCContactBaseProvider(contact);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Component_ReturnsContactBase()
        {
            Assert.AreEqual(contact, provider.Component);
        }

        [Test]
        public void GameObject_ReturnsComponentGameObject()
        {
            Assert.AreEqual(go, provider.GameObject);
        }

        [Test]
        public void ComponentType_IsContact()
        {
            Assert.AreEqual(AvatarDynamicsComponentType.Contact, provider.ComponentType);
        }

        [Test]
        public void Radius_ReturnsContactRadius()
        {
            contact.radius = 0.75f;
            Assert.AreEqual(0.75f, provider.Radius);
        }
    }
}
