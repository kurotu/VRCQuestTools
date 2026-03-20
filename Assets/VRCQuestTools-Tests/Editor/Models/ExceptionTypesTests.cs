// Tests for additional exception types and edge cases
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    internal class ExceptionTypesTests
    {
        [Test]
        public void BreakingPackageException_HasMessage()
        {
            var ex = new BreakingPackageException("TestPackage", "2.0.0");
            Assert.IsNotNull(ex.Message);
            Assert.IsNotEmpty(ex.Message);
            Assert.AreEqual("TestPackage", ex.PackageDisplayName);
            Assert.AreEqual("2.0.0", ex.BreakingVersion);
            Assert.IsNotNull(ex.LocalizedMessage);
        }

        [Test]
        public void BreakingPackageException_IsPackageCompatibilityException()
        {
            var ex = new BreakingPackageException("test", "localized");
            Assert.IsInstanceOf<PackageCompatibilityException>(ex);
        }

        [Test]
        public void AnimationClipConversionException_HasInnerException()
        {
            var clip = new AnimationClip();
            try
            {
                var inner = new System.Exception("inner");
                var ex = new AnimationClipConversionException("msg", clip, inner);
                Assert.AreEqual(inner, ex.InnerException);
                Assert.AreEqual(clip, ex.SourceObject);
            }
            finally
            {
                Object.DestroyImmediate(clip);
            }
        }

        [Test]
        public void AnimatorControllerConversionException_HasInnerException()
        {
            var controller = new AnimatorController();
            try
            {
                var inner = new System.Exception("inner");
                var ex = new AnimatorControllerConversionException("msg", controller, inner);
                Assert.AreEqual(inner, ex.InnerException);
                Assert.AreEqual(controller, ex.SourceObject);
            }
            finally
            {
                Object.DestroyImmediate(controller);
            }
        }

        [Test]
        public void MaterialConversionException_HasInnerException()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var inner = new System.Exception("inner");
                var ex = new MaterialConversionException("msg", mat, inner);
                Assert.AreEqual(inner, ex.InnerException);
                Assert.AreEqual(mat, ex.SourceObject);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }
    }
}
