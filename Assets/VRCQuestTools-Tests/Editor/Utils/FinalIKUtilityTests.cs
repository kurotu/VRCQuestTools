// Tests for FinalIKUtility
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    internal class FinalIKUtilityTests
    {
        [Test]
        public void ComponentTypes_ReturnsCollection()
        {
            var types = FinalIKUtility.ComponentTypes;
            Assert.IsNotNull(types);
            // FinalIK may or may not be installed
        }

        [Test]
        public void IsFinalIKComponent_NonFinalIKComponent_ReturnsFalse()
        {
            var go = new GameObject("TestObj");
            try
            {
                var transform = go.GetComponent<Transform>();
                Assert.IsFalse(FinalIKUtility.IsFinalIKComponent(transform));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void IsFinalIKComponent_ByType_TransformReturnsFalse()
        {
            Assert.IsFalse(FinalIKUtility.IsFinalIKComponent(typeof(Transform)));
        }

        [Test]
        public void IsFinalIKComponent_ByType_MonoBehaviourReturnsFalse()
        {
            Assert.IsFalse(FinalIKUtility.IsFinalIKComponent(typeof(MonoBehaviour)));
        }
    }
}
