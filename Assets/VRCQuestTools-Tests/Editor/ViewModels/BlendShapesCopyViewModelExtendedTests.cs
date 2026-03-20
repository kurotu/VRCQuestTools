// <copyright file="BlendShapesCopyViewModelExtendedTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.ViewModels;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests.ViewModels
{
    [TestFixture]
    internal class BlendShapesCopyViewModelExtendedTests
    {
        [Test]
        public void CopyBlendShapesCopy_CopiesWeights()
        {
            // Create source mesh with blendshapes
            var sourceGo = new GameObject("Source");
            var sourceSmr = sourceGo.AddComponent<SkinnedMeshRenderer>();
            var sourceMesh = new Mesh();
            sourceMesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            sourceMesh.triangles = new int[] { 0, 1, 2 };
            var deltas = new Vector3[] { Vector3.right, Vector3.right, Vector3.right };
            var normals = new Vector3[3];
            var tangents = new Vector3[3];
            sourceMesh.AddBlendShapeFrame("Shape1", 100f, deltas, normals, tangents);
            sourceSmr.sharedMesh = sourceMesh;
            sourceSmr.SetBlendShapeWeight(0, 75f);

            // Create target mesh with same blendshapes
            var targetGo = new GameObject("Target");
            var targetSmr = targetGo.AddComponent<SkinnedMeshRenderer>();
            var targetMesh = new Mesh();
            targetMesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            targetMesh.triangles = new int[] { 0, 1, 2 };
            targetMesh.AddBlendShapeFrame("Shape1", 100f, deltas, normals, tangents);
            targetSmr.sharedMesh = targetMesh;
            targetSmr.SetBlendShapeWeight(0, 0f);

            try
            {
                var vm = new BlendShapesCopyViewModel();
                vm.sourceMesh = sourceSmr;
                vm.targetMesh = targetSmr;

                vm.CopyBlendShapesCopy();

                Assert.AreEqual(75f, targetSmr.GetBlendShapeWeight(0), 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(sourceGo);
                Object.DestroyImmediate(targetGo);
                Object.DestroyImmediate(sourceMesh);
                Object.DestroyImmediate(targetMesh);
            }
        }

        [Test]
        public void SwitchMeshes_SwapsSourceAndTarget()
        {
            var sourceGo = new GameObject("Source");
            var targetGo = new GameObject("Target");
            var sourceSmr = sourceGo.AddComponent<SkinnedMeshRenderer>();
            var targetSmr = targetGo.AddComponent<SkinnedMeshRenderer>();

            try
            {
                var vm = new BlendShapesCopyViewModel();
                vm.sourceMesh = sourceSmr;
                vm.targetMesh = targetSmr;

                vm.SwitchMeshes();

                Assert.AreEqual(targetSmr, vm.sourceMesh);
                Assert.AreEqual(sourceSmr, vm.targetMesh);
            }
            finally
            {
                Object.DestroyImmediate(sourceGo);
                Object.DestroyImmediate(targetGo);
            }
        }

        [Test]
        public void ShouldDisableCopyButton_BothNull_True()
        {
            var vm = new BlendShapesCopyViewModel();
            Assert.IsTrue(vm.ShouldDisableCopyButton);
        }

        [Test]
        public void ShouldDisableCopyButton_SourceNull_True()
        {
            var go = new GameObject("Target");
            var smr = go.AddComponent<SkinnedMeshRenderer>();
            try
            {
                var vm = new BlendShapesCopyViewModel();
                vm.targetMesh = smr;
                Assert.IsTrue(vm.ShouldDisableCopyButton);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ShouldDisableCopyButton_TargetNull_True()
        {
            var go = new GameObject("Source");
            var smr = go.AddComponent<SkinnedMeshRenderer>();
            try
            {
                var vm = new BlendShapesCopyViewModel();
                vm.sourceMesh = smr;
                Assert.IsTrue(vm.ShouldDisableCopyButton);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ShouldDisableCopyButton_BothSet_False()
        {
            var sourceGo = new GameObject("Source");
            var targetGo = new GameObject("Target");
            var sourceSmr = sourceGo.AddComponent<SkinnedMeshRenderer>();
            var targetSmr = targetGo.AddComponent<SkinnedMeshRenderer>();
            try
            {
                var vm = new BlendShapesCopyViewModel();
                vm.sourceMesh = sourceSmr;
                vm.targetMesh = targetSmr;
                Assert.IsFalse(vm.ShouldDisableCopyButton);
            }
            finally
            {
                Object.DestroyImmediate(sourceGo);
                Object.DestroyImmediate(targetGo);
            }
        }
    }
}
