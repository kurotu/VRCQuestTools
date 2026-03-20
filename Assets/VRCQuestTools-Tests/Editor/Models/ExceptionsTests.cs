// <copyright file="ExceptionsTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for exception classes in Exceptions.cs.
    /// </summary>
    public class ExceptionsTests
    {
        [Test]
        public void MaterialConversionException_StoresSourceAndMessage()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var inner = new System.Exception("inner");
                var ex = new MaterialConversionException("test message", mat, inner);
                Assert.AreEqual("test message", ex.Message);
                Assert.AreEqual(mat, ex.SourceObject);
                Assert.AreEqual(inner, ex.InnerException);
                Assert.IsTrue(ex is IVRCQuestToolsException);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void AnimationClipConversionException_StoresSourceAndMessage()
        {
            var clip = new AnimationClip();
            try
            {
                var inner = new System.Exception("inner");
                var ex = new AnimationClipConversionException("clip error", clip, inner);
                Assert.AreEqual("clip error", ex.Message);
                Assert.AreEqual(clip, ex.SourceObject);
                Assert.AreEqual(inner, ex.InnerException);
            }
            finally
            {
                Object.DestroyImmediate(clip);
            }
        }

        [Test]
        public void AnimatorControllerConversionException_StoresSourceAndMessage()
        {
            var controller = new UnityEditor.Animations.AnimatorController();
            try
            {
                var inner = new System.Exception("inner");
                var ex = new AnimatorControllerConversionException("ctrl error", controller, inner);
                Assert.AreEqual("ctrl error", ex.Message);
                Assert.AreEqual(controller, ex.SourceObject);
                Assert.AreEqual(inner, ex.InnerException);
            }
            finally
            {
                Object.DestroyImmediate(controller);
            }
        }

        [Test]
        public void TargetMaterialNullException_StoresComponentAndMessage()
        {
            var go = new GameObject("TestException");
            try
            {
                var comp = go.transform;
                var ex = new TargetMaterialNullException("null target", comp);
                Assert.AreEqual("null target", ex.Message);
                Assert.AreEqual(comp, ex.Component);
                Assert.IsTrue(ex is IVRCQuestToolsException);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void InvalidMaterialSwapNullException_StoresComponentAndIndex()
        {
            var go = new GameObject("TestException");
            try
            {
                var swap = go.AddComponent<MaterialSwap>();
                swap.materialMappings = new System.Collections.Generic.List<MaterialSwap.MaterialMapping>
                {
                    new MaterialSwap.MaterialMapping(),
                };
                var ex = new InvalidMaterialSwapNullException("swap null", swap, 0);
                Assert.AreEqual("swap null", ex.Message);
                Assert.AreEqual(swap, ex.Component);
                Assert.IsNotNull(ex.MaterialMapping);
                Assert.IsTrue(ex is IVRCQuestToolsException);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void InvalidReplacementMaterialException_StoresComponentAndMaterial()
        {
            var go = new GameObject("TestException");
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var comp = go.transform;
                var ex = new InvalidReplacementMaterialException("bad replacement", comp, mat);
                Assert.AreEqual("bad replacement", ex.Message);
                Assert.AreEqual(comp, ex.Component);
                Assert.AreEqual(mat, ex.ReplacementMaterial);
                Assert.IsTrue(ex is IVRCQuestToolsException);
            }
            finally
            {
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(go);
            }
        }
    }
}
