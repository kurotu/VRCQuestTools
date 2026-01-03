// <copyright file="AAOMergePhysBoneProviderMockTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using KRT.VRCQuestTools.Models.VRChat;
using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace KRT.VRCQuestTools.Models.VRChat
{
    /// <summary>
    /// Tests for AAOMergePhysBoneProvider class.
    /// </summary>
    public class AAOMergePhysBoneProviderMockTests
    {
        private GameObject testGameObject;
        private VRCPhysBone testPhysBone1;
        private VRCPhysBone testPhysBone2;
        private VRCPhysBoneCollider testCollider1;
        private VRCPhysBoneCollider testCollider2;
        private AAOMergePhysBoneProvider.AAOMergePhysBoneReflectionInfo mockReflectionInfo;

        [SetUp]
        public void SetUp()
        {
            testGameObject = new GameObject("TestAAOMergePhysBone");

            mockReflectionInfo = new AAOMergePhysBoneProvider.AAOMergePhysBoneReflectionInfo(
                typeof(MockAaoMergePhysBone).FullName,
                nameof(MockAaoMergePhysBone.componentsSet),
                nameof(MockPrefabSafeSet.GetAsList));

            // Create test PhysBones
            var pb1Object = new GameObject("PhysBone1");
            pb1Object.transform.SetParent(testGameObject.transform);
            testPhysBone1 = pb1Object.AddComponent<VRCPhysBone>();

            var pb2Object = new GameObject("PhysBone2");
            pb2Object.transform.SetParent(testGameObject.transform);
            testPhysBone2 = pb2Object.AddComponent<VRCPhysBone>();

            // Create test colliders
            var collider1Object = new GameObject("Collider1");
            collider1Object.transform.SetParent(testGameObject.transform);
            testCollider1 = collider1Object.AddComponent<VRCPhysBoneCollider>();

            var collider2Object = new GameObject("Collider2");
            collider2Object.transform.SetParent(testGameObject.transform);
            testCollider2 = collider2Object.AddComponent<VRCPhysBoneCollider>();
        }

        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(testGameObject);
            }
        }

        /// <summary>
        /// Test that AAOMergePhysBoneProvider throws exception for null component.
        /// </summary>
        [Test]
        public void TestAAOMergePhysBoneProvider_ThrowsOnNullComponent()
        {
            Assert.Throws<ArgumentNullException>(() => new AAOMergePhysBoneProvider(null));
        }

        /// <summary>
        /// Test AAOMergePhysBoneProvider with mock AAO MergePhysBone component.
        /// </summary>
        [Test]
        public void TestAAOMergePhysBoneProvider_WithMockComponent()
        {
            var mockComponent = testGameObject.AddComponent<MockAaoMergePhysBone>();
            mockComponent.componentsSet.Add(testPhysBone1);
            mockComponent.componentsSet.Add(testPhysBone2);

            var provider = new AAOMergePhysBoneProvider(mockComponent, mockReflectionInfo);

            Assert.AreEqual(mockComponent, provider.Component);
            Assert.AreEqual(testGameObject, provider.GameObject);
            Assert.IsNotNull(provider.GetPhysBones());
            Assert.AreEqual(2, provider.GetPhysBones().Length);
        }

        /// <summary>
        /// Test AAOMergePhysBoneProvider returns merged colliders without duplicates.
        /// </summary>
        [Test]
        public void TestAAOMergePhysBoneProvider_MergesCollidersWithoutDuplicates()
        {
            var mockComponent = testGameObject.AddComponent<MockAaoMergePhysBone>();

            // Set up PhysBones with overlapping colliders
            testPhysBone1.colliders.Add(testCollider1);
            testPhysBone1.colliders.Add(testCollider2);
            testPhysBone2.colliders.Add(testCollider1); // Duplicate collider

            mockComponent.componentsSet.Add(testPhysBone1);
            mockComponent.componentsSet.Add(testPhysBone2);

            var provider = new AAOMergePhysBoneProvider(mockComponent, mockReflectionInfo);
            var colliders = provider.Colliders;

            Assert.AreEqual(2, colliders.Count);
            Assert.IsTrue(colliders.Contains(testCollider1));
            Assert.IsTrue(colliders.Contains(testCollider2));
        }

        /// <summary>
        /// Test AAOMergePhysBoneProvider returns merged ignore transforms without duplicates.
        /// </summary>
        [Test]
        public void TestAAOMergePhysBoneProvider_MergesIgnoreTransformsWithoutDuplicates()
        {
            var mockComponent = testGameObject.AddComponent<MockAaoMergePhysBone>();

            var ignoreTransform1 = new GameObject("Ignore1").transform;
            var ignoreTransform2 = new GameObject("Ignore2").transform;
            ignoreTransform1.SetParent(testGameObject.transform);
            ignoreTransform2.SetParent(testGameObject.transform);

            // Set up PhysBones with overlapping ignore transforms
            testPhysBone1.ignoreTransforms.Add(ignoreTransform1);
            testPhysBone1.ignoreTransforms.Add(ignoreTransform2);
            testPhysBone2.ignoreTransforms.Add(ignoreTransform1); // Duplicate

            mockComponent.componentsSet.Add(testPhysBone1);
            mockComponent.componentsSet.Add(testPhysBone2);

            var provider = new AAOMergePhysBoneProvider(mockComponent, mockReflectionInfo);
            var ignoreTransforms = provider.IgnoreTransforms;

            Assert.AreEqual(2, ignoreTransforms.Count);
            Assert.IsTrue(ignoreTransforms.Contains(ignoreTransform1));
            Assert.IsTrue(ignoreTransforms.Contains(ignoreTransform2));

            UnityEngine.Object.DestroyImmediate(ignoreTransform1.gameObject);
            UnityEngine.Object.DestroyImmediate(ignoreTransform2.gameObject);
        }

        /// <summary>
        /// Test AAOMergePhysBoneProvider properties with empty PhysBones array.
        /// </summary>
        [Test]
        public void TestAAOMergePhysBoneProvider_EmptyPhysBonesArray()
        {
            var mockComponent = testGameObject.AddComponent<MockAaoMergePhysBone>();

            var provider = new AAOMergePhysBoneProvider(mockComponent, mockReflectionInfo);

            Assert.AreEqual(0, provider.GetPhysBones().Length);
            Assert.AreEqual(Vector3.zero, provider.EndpointPosition);
            Assert.AreEqual(MultiChildType.Ignore, provider.MultiChildType);
            Assert.AreEqual(0f, provider.Radius);
            Assert.IsNull(provider.RadiusCurve);
            Assert.AreEqual(0, provider.Colliders.Count);
            Assert.AreEqual(0, provider.IgnoreTransforms.Count);
        }

        /// <summary>
        /// Test AAOMergePhysBoneProvider filters out null PhysBones.
        /// </summary>
        [Test]
        public void TestAAOMergePhysBoneProvider_FiltersNullPhysBones()
        {
            var mockComponent = testGameObject.AddComponent<MockAaoMergePhysBone>();
            mockComponent.componentsSet.Add(testPhysBone1);
            mockComponent.componentsSet.Add(null);
            mockComponent.componentsSet.Add(testPhysBone2);

            var provider = new AAOMergePhysBoneProvider(mockComponent, mockReflectionInfo);
            var physBones = provider.GetPhysBones();

            Assert.AreEqual(2, physBones.Length);
            Assert.IsTrue(Array.Exists(physBones, pb => pb == testPhysBone1));
            Assert.IsTrue(Array.Exists(physBones, pb => pb == testPhysBone2));
        }

        /// <summary>
        /// Test AAOMergePhysBoneProvider ClearCollider does not throw.
        /// </summary>
        [Test]
        public void TestAAOMergePhysBoneProvider_ClearColliderDoesNotThrow()
        {
            var mockComponent = testGameObject.AddComponent<MockAaoMergePhysBone>();
            mockComponent.componentsSet.Add(testPhysBone1);

            var provider = new AAOMergePhysBoneProvider(mockComponent, mockReflectionInfo);

            Assert.DoesNotThrow(() => provider.ClearCollider(0));
        }

        /// <summary>
        /// Test AAOMergePhysBoneProvider uses first PhysBone properties.
        /// </summary>
        [Test]
        public void TestAAOMergePhysBoneProvider_UsesFirstPhysBoneProperties()
        {
            var mockComponent = testGameObject.AddComponent<MockAaoMergePhysBone>();

            testPhysBone1.endpointPosition = new Vector3(1, 2, 3);
            testPhysBone1.radius = 0.5f;
            testPhysBone1.radiusCurve = AnimationCurve.Linear(0, 0, 1, 1);

            mockComponent.componentsSet.Add(testPhysBone1);
            mockComponent.componentsSet.Add(testPhysBone2);

            var provider = new AAOMergePhysBoneProvider(mockComponent, mockReflectionInfo);

            Assert.AreEqual(new Vector3(1, 2, 3), provider.EndpointPosition);
            Assert.AreEqual(0.5f, provider.Radius);
            Assert.IsNotNull(provider.RadiusCurve);
        }

        /// <summary>
        /// Mock AAO MergePhysBone component for testing.
        /// This simulates the structure of AAO's internal MergePhysBone component.
        /// </summary>
        private class MockAaoMergePhysBone : MonoBehaviour
        {
            public readonly MockPrefabSafeSet componentsSet = new MockPrefabSafeSet();
        }

        /// <summary>
        /// Mock PrefabSafeSet to simulate AAO's PrefabSafeSet structure.
        /// </summary>
        private class MockPrefabSafeSet : System.Collections.IEnumerable
        {
            private readonly List<VRCPhysBone> items = new List<VRCPhysBone>();

            public void Add(VRCPhysBone item)
            {
                items.Add(item);
            }

            public List<VRCPhysBone> GetAsList()
            {
                return items;
            }

            public System.Collections.IEnumerator GetEnumerator()
            {
                return items.GetEnumerator();
            }
        }
    }
}
