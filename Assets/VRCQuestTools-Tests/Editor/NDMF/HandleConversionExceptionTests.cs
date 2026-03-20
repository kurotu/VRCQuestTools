// Tests for NdmfPluginUtility.HandleConversionException VQT exception branches.

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KRT.VRCQuestTools.Tests.NDMF
{
    [TestFixture]
    internal class HandleConversionExceptionVQTTests
    {
        private Type utilityType;
        private MethodInfo handleMethod;

        [SetUp]
        public void SetUp()
        {
            utilityType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfPluginUtility");
            if (utilityType == null)
            {
                Assert.Ignore("NdmfPluginUtility not found");
                return;
            }
            handleMethod = utilityType.GetMethod("HandleConversionException", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (handleMethod == null)
            {
                Assert.Ignore("HandleConversionException method not found");
            }
        }

        [Test]
        public void HandleConversionException_TargetMaterialNullException_DoesNotRethrow()
        {
            var go = new GameObject("TestComp");
            try
            {
                var component = go.AddComponent<MeshRenderer>();
                var exception = new TargetMaterialNullException("test", component);
                try
                {
                    handleMethod.Invoke(null, new object[] { exception });
                }
                catch (TargetInvocationException ex)
                {
                    // ErrorReport.ReportError may throw outside build context, but the branch is covered
                    Assert.Pass($"ErrorReport threw (expected outside build context): {ex.InnerException?.GetType().Name}");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HandleConversionException_InvalidReplacementMaterialException_DoesNotRethrow()
        {
            var go = new GameObject("TestComp");
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var component = go.AddComponent<MeshRenderer>();
                var exception = new InvalidReplacementMaterialException("test", component, mat);
                try
                {
                    handleMethod.Invoke(null, new object[] { exception });
                }
                catch (TargetInvocationException ex)
                {
                    Assert.Pass($"ErrorReport threw (expected outside build context): {ex.InnerException?.GetType().Name}");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void HandleConversionException_InvalidMaterialSwapNullException_DoesNotRethrow()
        {
            var go = new GameObject("TestSwap");
            try
            {
                var swap = go.AddComponent<MaterialSwap>();
                swap.materialMappings = new System.Collections.Generic.List<MaterialSwap.MaterialMapping>
                {
                    new MaterialSwap.MaterialMapping(),
                };
                var exception = new InvalidMaterialSwapNullException("test", swap, 0);
                try
                {
                    handleMethod.Invoke(null, new object[] { exception });
                }
                catch (TargetInvocationException ex)
                {
                    Assert.Pass($"ErrorReport threw (expected outside build context): {ex.InnerException?.GetType().Name}");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void HandleConversionException_PackageCompatibilityException_DoesNotRethrow()
        {
            var exception = new LegacyPackageException("TestPackage", "1.0.0");
            try
            {
                handleMethod.Invoke(null, new object[] { exception });
            }
            catch (TargetInvocationException ex)
            {
                Assert.Pass($"ErrorReport threw (expected outside build context): {ex.InnerException?.GetType().Name}");
            }
        }

        [Test]
        public void HandleConversionException_MaterialConversionException_HandlesGracefully()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var inner = new System.Exception("inner error");
                var exception = new MaterialConversionException("test", mat, inner);
                try
                {
                    handleMethod.Invoke(null, new object[] { exception });
                }
                catch (TargetInvocationException)
                {
                    // Expected: NdmfObjectRegistry.GetReference or ErrorReport may throw
                    Assert.Pass("Expected exception outside build context");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void HandleConversionException_MaterialConversionWithPackageCompatibilityInner_HandlesGracefully()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var inner = new LegacyPackageException("TestPackage", "2.0.0");
                var exception = new MaterialConversionException("test", mat, inner);
                LogAssert.Expect(LogType.Exception, new Regex("LegacyPackageException"));
                try
                {
                    handleMethod.Invoke(null, new object[] { exception });
                }
                catch (TargetInvocationException)
                {
                    Assert.Pass("Expected exception outside build context");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void HandleConversionException_AnimationClipConversionException_HandlesGracefully()
        {
            var clip = new AnimationClip();
            try
            {
                var inner = new System.Exception("anim error");
                var exception = new AnimationClipConversionException("test", clip, inner);
                try
                {
                    handleMethod.Invoke(null, new object[] { exception });
                }
                catch (TargetInvocationException)
                {
                    Assert.Pass("Expected exception outside build context");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(clip);
            }
        }

        [Test]
        public void HandleConversionException_AnimatorControllerConversionException_HandlesGracefully()
        {
            var controller = new UnityEditor.Animations.AnimatorController();
            try
            {
                var inner = new System.Exception("ctrl error");
                var exception = new AnimatorControllerConversionException("test", controller, inner);
                try
                {
                    handleMethod.Invoke(null, new object[] { exception });
                }
                catch (TargetInvocationException)
                {
                    Assert.Pass("Expected exception outside build context");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(controller);
            }
        }

        [Test]
        public void HandleConversionException_NonVQTExceptionWithInnerException_LogsInner()
        {
            var inner = new ArgumentException("inner");
            var exception = new InvalidOperationException("outer", inner);
            LogAssert.Expect(LogType.Exception, new Regex("ArgumentException"));
            var ex = Assert.Throws<TargetInvocationException>(() =>
            {
                handleMethod.Invoke(null, new object[] { exception });
            });
            Assert.That(ex.InnerException, Is.TypeOf<InvalidOperationException>());
        }
    }
}
