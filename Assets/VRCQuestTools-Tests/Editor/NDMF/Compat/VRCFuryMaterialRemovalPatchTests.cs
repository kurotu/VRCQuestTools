// <copyright file="VRCFuryMaterialRemovalPatchTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Tests for <see cref="VRCFuryMaterialRemovalPatch"/>. The gating-logic tests use fake,
    /// duck-typed VRCFury-shaped objects so they run without VRCFury being installed; the
    /// integration test at the bottom only exercises the real VRCFury type when it's present.
    /// </summary>
    public class VRCFuryMaterialRemovalPatchTests
    {
        private NdmfTestAvatarBuilder builder;

        /// <summary>
        /// Cleans up objects created during the test.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            builder?.Destroy();
            builder = null;
        }

        /// <summary>
        /// When the avatar root can't be resolved from the service instance, the original VRCFury
        /// behavior must not be suppressed.
        /// </summary>
        [Test]
        public void ApplyPrefix_ReturnsTrue_WhenAvatarRootCannotBeResolved()
        {
            var fakeService = new FakeRemoveNonQuestMaterialsService { avatarObject = null };

            Assert.IsTrue(VRCFuryMaterialRemovalPatch.ApplyPrefix(fakeService));
        }

        /// <summary>
        /// Without any IMaterialOperatorComponent, VRCQuestTools has nothing to convert, so VRCFury's
        /// removal must proceed as usual.
        /// </summary>
        [Test]
        public void ApplyPrefix_ReturnsTrue_WhenNoMaterialOperatorComponent()
        {
            builder = new NdmfTestAvatarBuilder();
            var fakeService = new FakeRemoveNonQuestMaterialsService { avatarObject = builder.Root };

            Assert.IsTrue(VRCFuryMaterialRemovalPatch.ApplyPrefix(fakeService));
        }

        /// <summary>
        /// When the resolved NDMF phase is Transforming, VRCQuestTools converts before VRCFury runs,
        /// so VRCFury's removal must proceed as usual.
        /// </summary>
        [Test]
        public void ApplyPrefix_ReturnsTrue_WhenNdmfPhaseResolvesToTransforming()
        {
            builder = new NdmfTestAvatarBuilder();
            var settings = builder.Root.AddComponent<AvatarConverterSettings>();
            settings.ndmfPhase = AvatarConverterNdmfPhase.Transforming;
            var fakeService = new FakeRemoveNonQuestMaterialsService { avatarObject = builder.Root };

            Assert.IsTrue(VRCFuryMaterialRemovalPatch.ApplyPrefix(fakeService));
        }

        /// <summary>
        /// When the resolved NDMF phase is Optimizing, VRCFury's removal must be suppressed so
        /// VRCQuestTools can still convert the original materials later.
        /// </summary>
        [Test]
        public void ApplyPrefix_ReturnsFalse_WhenNdmfPhaseResolvesToOptimizing()
        {
            builder = new NdmfTestAvatarBuilder();
            var settings = builder.Root.AddComponent<AvatarConverterSettings>();
            settings.ndmfPhase = AvatarConverterNdmfPhase.Optimizing;
            var fakeService = new FakeRemoveNonQuestMaterialsService { avatarObject = builder.Root };

            Assert.IsFalse(VRCFuryMaterialRemovalPatch.ApplyPrefix(fakeService));
        }

        /// <summary>
        /// GetAvatarRoot must resolve the avatar GameObject through a wrapper type's implicit
        /// conversion operator, the same way VRCFury's internal VFGameObject wrapper works.
        /// </summary>
        [Test]
        public void GetAvatarRoot_ResolvesThroughImplicitConversionOperator()
        {
            builder = new NdmfTestAvatarBuilder();
            var wrapper = new FakeVFGameObject(builder.Root);
            var fakeService = new FakeWrappedRemoveNonQuestMaterialsService { avatarObject = wrapper };

            Assert.AreSame(builder.Root, VRCFuryMaterialRemovalPatch.GetAvatarRoot(fakeService));
        }

        /// <summary>
        /// When VRCFury is actually installed, verify the real RemoveNonQuestMaterialsService.Apply()
        /// gets patched and is skipped (instead of throwing on its un-initialized dependencies) when
        /// the resolved NDMF phase is Optimizing. Skips when VRCFury isn't installed.
        /// </summary>
        [Test]
        public void Apply_IsSkipped_ForRealVRCFuryService_WhenNdmfPhaseResolvesToOptimizing()
        {
            var serviceType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("VF.Service.RemoveNonQuestMaterialsService"))
                .FirstOrDefault(t => t != null);
            if (serviceType == null)
            {
                Assert.Ignore("VRCFury is not installed.");
                return;
            }

            var applyMethod = serviceType.GetMethod("Apply", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(applyMethod, "VRCFury may have made breaking changes: Apply() not found.");

            var patchInfo = Harmony.GetPatchInfo(applyMethod);
            Assert.NotNull(patchInfo, "RemoveNonQuestMaterialsService.Apply() should be patched by VRCFuryMaterialRemovalPatch.");
            Assert.IsTrue(patchInfo.Prefixes.Any(p => p.owner == "com.github.kurotu.vrc-quest-tools.vrcfury-compat"));

            builder = new NdmfTestAvatarBuilder();
            var settings = builder.Root.AddComponent<AvatarConverterSettings>();
            settings.ndmfPhase = AvatarConverterNdmfPhase.Optimizing;

            var avatarObjectField = serviceType.GetField("avatarObject", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(avatarObjectField, "VRCFury may have made breaking changes: avatarObject field not found.");

            var vfGameObjectType = avatarObjectField.FieldType;
            var fromGameObject = vfGameObjectType
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "op_Implicit" && m.GetParameters().FirstOrDefault()?.ParameterType == typeof(GameObject));
            Assert.NotNull(fromGameObject, "VRCFury may have made breaking changes: no GameObject -> VFGameObject conversion found.");

            var serviceInstance = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(serviceType);
            avatarObjectField.SetValue(serviceInstance, fromGameObject.Invoke(null, new object[] { builder.Root }));

            // The "controllers" field is intentionally left unset. If the patch fails to suppress the
            // original method, it will throw (e.g. a NullReferenceException) when it dereferences it.
            Assert.DoesNotThrow(() => applyMethod.Invoke(serviceInstance, null));
        }

        private class FakeRemoveNonQuestMaterialsService
        {
            public GameObject avatarObject;
        }

        private class FakeWrappedRemoveNonQuestMaterialsService
        {
            public FakeVFGameObject avatarObject;
        }

        private class FakeVFGameObject
        {
            private readonly GameObject gameObject;

            public FakeVFGameObject(GameObject gameObject)
            {
                this.gameObject = gameObject;
            }

            public static implicit operator GameObject(FakeVFGameObject d) => d?.gameObject;
        }
    }
}
