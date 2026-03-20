// Tests for BlendShapesCopyViewModel

using NUnit.Framework;
using UnityEngine;
using KRT.VRCQuestTools.ViewModels;

namespace KRT.VRCQuestTools.ViewModels.Tests
{
    [TestFixture]
    public class BlendShapesCopyViewModelTests
    {
        [Test]
        public void ShouldDisableCopyButton_BothNull_ReturnsTrue()
        {
            var vm = new BlendShapesCopyViewModel();
            Assert.IsTrue(vm.ShouldDisableCopyButton);
        }

        [Test]
        public void ShouldDisableCopyButton_SourceOnly_ReturnsTrue()
        {
            var go = new GameObject("Test");
            var renderer = go.AddComponent<SkinnedMeshRenderer>();
            try
            {
                var vm = new BlendShapesCopyViewModel();
                vm.sourceMesh = renderer;
                Assert.IsTrue(vm.ShouldDisableCopyButton);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ShouldDisableCopyButton_TargetOnly_ReturnsTrue()
        {
            var go = new GameObject("Test");
            var renderer = go.AddComponent<SkinnedMeshRenderer>();
            try
            {
                var vm = new BlendShapesCopyViewModel();
                vm.targetMesh = renderer;
                Assert.IsTrue(vm.ShouldDisableCopyButton);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ShouldDisableCopyButton_BothSet_ReturnsFalse()
        {
            var go1 = new GameObject("Source");
            var go2 = new GameObject("Target");
            var source = go1.AddComponent<SkinnedMeshRenderer>();
            var target = go2.AddComponent<SkinnedMeshRenderer>();
            try
            {
                var vm = new BlendShapesCopyViewModel();
                vm.sourceMesh = source;
                vm.targetMesh = target;
                Assert.IsFalse(vm.ShouldDisableCopyButton);
            }
            finally
            {
                Object.DestroyImmediate(go1);
                Object.DestroyImmediate(go2);
            }
        }

        [Test]
        public void SwitchMeshes_SwapsSourceAndTarget()
        {
            var go1 = new GameObject("Source");
            var go2 = new GameObject("Target");
            var source = go1.AddComponent<SkinnedMeshRenderer>();
            var target = go2.AddComponent<SkinnedMeshRenderer>();
            try
            {
                var vm = new BlendShapesCopyViewModel();
                vm.sourceMesh = source;
                vm.targetMesh = target;
                vm.SwitchMeshes();
                Assert.AreEqual(target, vm.sourceMesh);
                Assert.AreEqual(source, vm.targetMesh);
            }
            finally
            {
                Object.DestroyImmediate(go1);
                Object.DestroyImmediate(go2);
            }
        }

        [Test]
        public void SwitchMeshes_NullSource_Works()
        {
            var go = new GameObject("Target");
            var target = go.AddComponent<SkinnedMeshRenderer>();
            try
            {
                var vm = new BlendShapesCopyViewModel();
                vm.targetMesh = target;
                vm.SwitchMeshes();
                Assert.AreEqual(target, vm.sourceMesh);
                Assert.IsNull(vm.targetMesh);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
