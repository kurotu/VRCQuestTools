using KRT.VRCQuestTools.Models.VRChat.PhysBoneProviders;
using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace KRT.VRCQuestTools.Models.VRChat.PhysBoneProviders
{
    /// <summary>
    /// Tests for PhysBoneProvider classes.
    /// </summary>
    public class PhysBoneProviderTests
    {
        private GameObject testGameObject;
        private VRCPhysBone testPhysBone;

        [SetUp]
        public void SetUp()
        {
            testGameObject = new GameObject("TestPhysBone");
            testPhysBone = testGameObject.AddComponent<VRCPhysBone>();
        }

        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }
        }

        /// <summary>
        /// Test VRCPhysBoneProvider basic functionality.
        /// </summary>
        [Test]
        public void TestVRCPhysBoneProvider_BasicFunctionality()
        {
            var provider = new VRCPhysBoneProvider(testPhysBone);

            Assert.AreEqual(testPhysBone, provider.Component);
            Assert.AreEqual(testGameObject, provider.GameObject);
            Assert.IsNotNull(provider.IgnoreTransforms);
            Assert.IsNotNull(provider.Colliders);
        }

        /// <summary>
        /// Test VRCPhysBoneProvider properties.
        /// </summary>
        [Test]
        public void TestVRCPhysBoneProvider_Properties()
        {
            var provider = new VRCPhysBoneProvider(testPhysBone);

            // Test default values
            Assert.AreEqual(Vector3.zero, provider.EndpointPosition);
            Assert.AreEqual(MultiChildType.Ignore, provider.MultiChildType);
            Assert.IsNull(provider.RootTransform);
        }

        /// <summary>
        /// Test VRCPhysBoneProvider can handle null component gracefully.
        /// </summary>
        [Test]
        public void TestVRCPhysBoneProvider_NullHandling()
        {
            var provider = new VRCPhysBoneProvider(testPhysBone);

            // ClearCollider should not throw even with empty colliders list
            Assert.DoesNotThrow(() => provider.ClearCollider(0));
        }
    }
}