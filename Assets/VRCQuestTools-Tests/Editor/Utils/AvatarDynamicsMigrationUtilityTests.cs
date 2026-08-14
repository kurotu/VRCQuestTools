// <copyright file="AvatarDynamicsMigrationUtilityTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#pragma warning disable CS0618

using System;
using System.Linq;
using KRT.VRCQuestTools.Components;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Tests for AvatarDynamicsMigrationUtility: scanning for and migrating legacy Avatar Dynamics
    /// settings on both project prefabs (base prefabs and variants) and scene objects.
    /// </summary>
    public class AvatarDynamicsMigrationUtilityTests : KRT.VRCQuestTools.TestUtilities.IsolatedEditorSceneTest
    {
        private string testFolder;

        /// <summary>
        /// Creates a scratch folder under Assets/ for prefab assets created by each test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            var folderName = $"VQT_MigrationTests_{Guid.NewGuid():N}";
            testFolder = $"Assets/{folderName}";
            AssetDatabase.CreateFolder("Assets", folderName);
        }

        /// <summary>
        /// Deletes the scratch folder and every prefab asset created inside it.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            if (AssetDatabase.IsValidFolder(testFolder))
            {
                AssetDatabase.DeleteAsset(testFolder);
            }
            AssetDatabase.Refresh();
        }

        [Test]
        public void Migrate_BaseOnlyLegacyPrefab_MigratesAndClearsLegacyFields()
        {
            var root = CreateAvatarHierarchy("BaseAvatar");
            var physBone = root.GetComponentInChildren<VRCPhysBone>();
            root.GetComponent<AvatarConverterSettings>().physBonesToKeep = new[] { physBone };
            var path = SaveAsPrefabAndDestroy(root, "BasePrefab");

            var target = AvatarDynamicsMigrationTarget.ForProjectPrefab(path);
            var result = AvatarDynamicsMigrationUtility.Migrate(new[] { target });

            Assert.AreEqual(1, result.MigratedCount);
            Assert.AreEqual(0, result.SkippedCount);

            var migratedAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var converterSettings = migratedAsset.GetComponent<AvatarConverterSettings>();

            // Elements are nulled in place rather than shrinking the array (protects any prefab
            // variant/instance that overrides this same array with different, equal-length content
            // - see AvatarDynamicsSettingsUtility.ClearLegacyArraysInPlace), so "cleared" means
            // all-null, not necessarily zero-length.
            Assert.IsTrue(converterSettings.physBonesToKeep.All(p => p == null), "Legacy field should be cleared on the asset");
            Assert.IsFalse(converterSettings.HasLegacyAvatarDynamicsSettings);

            // The kept PhysBone should have no PlatformComponentRemover (it wasn't marked for removal).
            var boneObject = migratedAsset.GetComponentInChildren<VRCPhysBone>().gameObject;
            Assert.IsNull(boneObject.GetComponent<PlatformComponentRemover>());
        }

        [Test]
        public void Migrate_ProjectPrefab_PartialSelection_RemovesUnkeptComponentsViaPCR()
        {
            var root = CreateAvatarHierarchy("BaseAvatar2");
            var keptBone = root.GetComponentInChildren<VRCPhysBone>();
            var removedBone = new GameObject("RemovedBone");
            removedBone.transform.SetParent(root.transform);
            var removedPhysBone = removedBone.AddComponent<VRCPhysBone>();

            // Only keptBone is listed as kept, so removedPhysBone should be marked for Android removal.
            root.GetComponent<AvatarConverterSettings>().physBonesToKeep = new[] { keptBone };
            var path = SaveAsPrefabAndDestroy(root, "BasePrefabPartialKeep");

            var target = AvatarDynamicsMigrationTarget.ForProjectPrefab(path);
            var result = AvatarDynamicsMigrationUtility.Migrate(new[] { target });

            Assert.AreEqual(1, result.MigratedCount);

            var migratedAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var bones = migratedAsset.GetComponentsInChildren<VRCPhysBone>(true);
            var keptBoneAfter = bones.First(b => b.gameObject.name == "Bone");
            var removedBoneAfter = bones.First(b => b.gameObject.name == "RemovedBone");

            Assert.IsNull(keptBoneAfter.gameObject.GetComponent<PlatformComponentRemover>(), "Kept PhysBone's GameObject should not need a PlatformComponentRemover");

            var remover = removedBoneAfter.gameObject.GetComponent<PlatformComponentRemover>();
            Assert.IsNotNull(remover, "Removed PhysBone's GameObject should have a PlatformComponentRemover");
            var setting = Array.Find(remover.componentSettings, s => s.component == removedBoneAfter);
            Assert.IsNotNull(setting);
            Assert.IsTrue(setting.removeOnAndroid);
        }

        [Test]
        public void Migrate_VariantOverridingLegacyFields_MigratesIndependentlyOfBase()
        {
            // Base prefab has no legacy settings at all.
            var baseRoot = CreateAvatarHierarchy("Base");
            var basePath = SaveAsPrefabAndDestroy(baseRoot, "Base");
            var baseAsset = AssetDatabase.LoadAssetAtPath<GameObject>(basePath);

            // Variant overrides physBonesToKeep by itself.
            var variantInstance = (GameObject)PrefabUtility.InstantiatePrefab(baseAsset);
            var variantPhysBone = variantInstance.GetComponentInChildren<VRCPhysBone>();
            variantInstance.GetComponent<AvatarConverterSettings>().physBonesToKeep = new[] { variantPhysBone };
            var variantPath = SaveAsPrefabAndDestroy(variantInstance, "Variant");

            Assert.IsTrue(PrefabUtility.IsPartOfVariantPrefab(AssetDatabase.LoadAssetAtPath<GameObject>(variantPath)), "Sanity check: Variant should actually be a Prefab Variant of Base");

            var baseTarget = AvatarDynamicsMigrationTarget.ForProjectPrefab(basePath);
            var variantTarget = AvatarDynamicsMigrationTarget.ForProjectPrefab(variantPath);
            var result = AvatarDynamicsMigrationUtility.Migrate(new[] { baseTarget, variantTarget });

            Assert.AreEqual(1, result.MigratedCount, "Only the variant has legacy settings to migrate");
            Assert.AreEqual(1, result.SkippedCount, "The base prefab never had legacy settings");

            var migratedVariant = AssetDatabase.LoadAssetAtPath<GameObject>(variantPath);
            var variantSettings = migratedVariant.GetComponent<AvatarConverterSettings>();
            Assert.IsTrue(variantSettings.physBonesToKeep.All(p => p == null));
            Assert.IsFalse(variantSettings.HasLegacyAvatarDynamicsSettings);

            // The LoadPrefabContents/SaveAsPrefabAsset round trip used for the no-Undo migration
            // path must not flatten the variant into a standalone prefab.
            Assert.IsTrue(PrefabUtility.IsPartOfVariantPrefab(migratedVariant), "Should still be a Prefab Variant after migration");
            Assert.IsNotNull(PrefabUtility.GetCorrespondingObjectFromSource(migratedVariant), "Variant should still be linked to its base prefab after migration");

            var baseAssetAfter = AssetDatabase.LoadAssetAtPath<GameObject>(basePath);
            Assert.IsFalse(baseAssetAfter.GetComponent<AvatarConverterSettings>().HasLegacyAvatarDynamicsSettings, "Base was never legacy and should remain untouched");
        }

        [Test]
        public void Migrate_BaseAndVariantBothHaveOwnLegacySettings_EachMigratesIndependently()
        {
            // Base has two PhysBones and keeps "Bone" (BoneA), so "BoneB" is marked for removal.
            var baseRoot = CreateAvatarHierarchy("MixedBase");
            var boneA = baseRoot.GetComponentInChildren<VRCPhysBone>();
            var boneBObject = new GameObject("BoneB");
            boneBObject.transform.SetParent(baseRoot.transform);
            var boneB = boneBObject.AddComponent<VRCPhysBone>();
            baseRoot.GetComponent<AvatarConverterSettings>().physBonesToKeep = new[] { boneA };
            var basePath = SaveAsPrefabAndDestroy(baseRoot, "MixedBase");
            var baseAsset = AssetDatabase.LoadAssetAtPath<GameObject>(basePath);

            // Variant overrides with the opposite selection: keeps BoneB, removes BoneA - a
            // genuinely different legacy configuration from the base's, not merely inherited.
            var variantInstance = (GameObject)PrefabUtility.InstantiatePrefab(baseAsset);
            var variantBoneB = variantInstance.transform.Find("BoneB").GetComponent<VRCPhysBone>();
            variantInstance.GetComponent<AvatarConverterSettings>().physBonesToKeep = new[] { variantBoneB };
            var variantPath = SaveAsPrefabAndDestroy(variantInstance, "MixedVariant");

            var baseTarget = AvatarDynamicsMigrationTarget.ForProjectPrefab(basePath);
            var variantTarget = AvatarDynamicsMigrationTarget.ForProjectPrefab(variantPath);
            var result = AvatarDynamicsMigrationUtility.Migrate(new[] { baseTarget, variantTarget });

            Assert.AreEqual(2, result.MigratedCount, "Both base and variant have their own distinct legacy settings to migrate");
            Assert.AreEqual(0, result.SkippedCount);

            // Base: BoneA kept (no PCR), BoneB removed (PCR removeOnAndroid=true).
            var migratedBase = AssetDatabase.LoadAssetAtPath<GameObject>(basePath);
            var baseBones = migratedBase.GetComponentsInChildren<VRCPhysBone>(true);
            var baseBoneA = baseBones.First(b => b.gameObject.name == "Bone");
            var baseBoneB = baseBones.First(b => b.gameObject.name == "BoneB");
            Assert.IsNull(baseBoneA.gameObject.GetComponent<PlatformComponentRemover>(), "Base: kept BoneA should have no PCR");
            var baseRemoverB = baseBoneB.gameObject.GetComponent<PlatformComponentRemover>();
            Assert.IsNotNull(baseRemoverB, "Base: removed BoneB should have a PCR");
            Assert.IsTrue(Array.Find(baseRemoverB.componentSettings, s => s.component == baseBoneB).removeOnAndroid);
            Assert.IsFalse(migratedBase.GetComponent<AvatarConverterSettings>().HasLegacyAvatarDynamicsSettings);

            // Variant: independently, BoneB kept (no PCR) and BoneA removed (PCR removeOnAndroid=true)
            // - the exact opposite of base, proving the variant's migration used its OWN override
            // data rather than inheriting/copying base's result.
            var migratedVariant = AssetDatabase.LoadAssetAtPath<GameObject>(variantPath);
            var variantBones = migratedVariant.GetComponentsInChildren<VRCPhysBone>(true);
            var vBoneA = variantBones.First(b => b.gameObject.name == "Bone");
            var vBoneB = variantBones.First(b => b.gameObject.name == "BoneB");
            Assert.IsNull(vBoneB.gameObject.GetComponent<PlatformComponentRemover>(), "Variant: kept BoneB should have no PCR");
            var variantRemoverA = vBoneA.gameObject.GetComponent<PlatformComponentRemover>();
            Assert.IsNotNull(variantRemoverA, "Variant: removed BoneA should have a PCR");
            Assert.IsTrue(Array.Find(variantRemoverA.componentSettings, s => s.component == vBoneA).removeOnAndroid);
            Assert.IsFalse(migratedVariant.GetComponent<AvatarConverterSettings>().HasLegacyAvatarDynamicsSettings);
            Assert.IsTrue(PrefabUtility.IsPartOfVariantPrefab(migratedVariant), "Should still be a Prefab Variant after migration");
        }

        // Regression test for the exact bug the previous test's failure surfaced: migrating ONLY
        // the base (leaving the variant unselected/out of the batch, e.g. the user unchecked it in
        // the review window) must not corrupt the variant's own, still-unmigrated legacy override.
        // Before ClearLegacyArraysInPlace, shrinking the base's physBonesToKeep from length 1 to 0
        // silently dropped the variant's equal-length per-index override on the same array, making
        // the variant's independent (and still-pending) legacy settings vanish even though the
        // variant itself was never touched.
        [Test]
        public void Migrate_OnlyBaseSelected_VariantsIndependentLegacySettingsSurviveUntouched()
        {
            var baseRoot = CreateAvatarHierarchy("SurviveBase");
            var boneA = baseRoot.GetComponentInChildren<VRCPhysBone>();
            var boneBObject = new GameObject("BoneB");
            boneBObject.transform.SetParent(baseRoot.transform);
            var boneB = boneBObject.AddComponent<VRCPhysBone>();
            baseRoot.GetComponent<AvatarConverterSettings>().physBonesToKeep = new[] { boneA };
            var basePath = SaveAsPrefabAndDestroy(baseRoot, "SurviveBase");
            var baseAsset = AssetDatabase.LoadAssetAtPath<GameObject>(basePath);

            var variantInstance = (GameObject)PrefabUtility.InstantiatePrefab(baseAsset);
            var variantBoneB = variantInstance.transform.Find("BoneB").GetComponent<VRCPhysBone>();
            variantInstance.GetComponent<AvatarConverterSettings>().physBonesToKeep = new[] { variantBoneB };
            var variantPath = SaveAsPrefabAndDestroy(variantInstance, "SurviveVariant");

            // Migrate ONLY the base - the variant is deliberately left out of this batch.
            var baseTarget = AvatarDynamicsMigrationTarget.ForProjectPrefab(basePath);
            var result = AvatarDynamicsMigrationUtility.Migrate(new[] { baseTarget });

            Assert.AreEqual(1, result.MigratedCount);
            Assert.IsFalse(AssetDatabase.LoadAssetAtPath<GameObject>(basePath).GetComponent<AvatarConverterSettings>().HasLegacyAvatarDynamicsSettings, "Base should be migrated");

            var untouchedVariant = AssetDatabase.LoadAssetAtPath<GameObject>(variantPath);
            var untouchedVariantSettings = untouchedVariant.GetComponent<AvatarConverterSettings>();
            Assert.IsTrue(untouchedVariantSettings.HasLegacyAvatarDynamicsSettings, "Variant's own legacy settings must survive even though it was never included in the batch");
            var keptBones = untouchedVariantSettings.physBonesToKeep.Where(p => p != null).ToArray();
            Assert.AreEqual(1, keptBones.Length);
            Assert.AreEqual("BoneB", keptBones[0].gameObject.name, "Variant should still intend to keep its own BoneB, not have inherited base's cleared/empty state");
        }

        [Test]
        public void Migrate_BaseFirstOrdering_VariantInheritingLegacyBecomesNoOp()
        {
            // Base prefab has legacy settings; the variant does not override them at all,
            // so it only ever sees the inherited (eventually migrated) value.
            var baseRoot = CreateAvatarHierarchy("InheritBase");
            var basePhysBone = baseRoot.GetComponentInChildren<VRCPhysBone>();
            baseRoot.GetComponent<AvatarConverterSettings>().physBonesToKeep = new[] { basePhysBone };
            var basePath = SaveAsPrefabAndDestroy(baseRoot, "InheritBase");
            var baseAsset = AssetDatabase.LoadAssetAtPath<GameObject>(basePath);

            // Variant with no override of Avatar Dynamics settings at all.
            var variantInstance = (GameObject)PrefabUtility.InstantiatePrefab(baseAsset);
            var variantPath = SaveAsPrefabAndDestroy(variantInstance, "InheritVariant");

            var baseTarget = AvatarDynamicsMigrationTarget.ForProjectPrefab(basePath);
            var variantTarget = AvatarDynamicsMigrationTarget.ForProjectPrefab(variantPath);

            // Pass the variant first to confirm Migrate (not the caller) enforces base-before-variant ordering.
            var result = AvatarDynamicsMigrationUtility.Migrate(new[] { variantTarget, baseTarget });

            Assert.AreEqual(1, result.MigratedCount, "Only the base needed an actual change");
            Assert.AreEqual(1, result.SkippedCount, "The variant should resolve to 'already migrated' once the base is done, not get its own redundant override");

            var migratedBase = AssetDatabase.LoadAssetAtPath<GameObject>(basePath);
            Assert.IsFalse(migratedBase.GetComponent<AvatarConverterSettings>().HasLegacyAvatarDynamicsSettings);
        }

        [Test]
        public void FindMigrationTargets_OrdersProjectPrefabsByAssetPath()
        {
            // "AAA_Variant" sorts before its own base "ZZZ_Base" by path, making the two orderings
            // observably different: the scan lists by asset path for stable display, while
            // Migrate() re-derives the dependency-safe processing order itself (covered by
            // Migrate_BaseFirstOrdering_VariantInheritingLegacyBecomesNoOp).
            var baseRoot = CreateAvatarHierarchy("ZZZ_Base");
            var basePhysBone = baseRoot.GetComponentInChildren<VRCPhysBone>();
            baseRoot.GetComponent<AvatarConverterSettings>().physBonesToKeep = new[] { basePhysBone };
            var basePath = SaveAsPrefabAndDestroy(baseRoot, "ZZZ_Base");
            var baseAsset = AssetDatabase.LoadAssetAtPath<GameObject>(basePath);

            var variantInstance = (GameObject)PrefabUtility.InstantiatePrefab(baseAsset);
            var variantPhysBone = variantInstance.GetComponentInChildren<VRCPhysBone>();
            variantInstance.GetComponent<AvatarConverterSettings>().physBonesToKeep = new[] { variantPhysBone };
            var variantPath = SaveAsPrefabAndDestroy(variantInstance, "AAA_Variant");

            var targets = AvatarDynamicsMigrationUtility.FindMigrationTargets();
            var prefabPaths = targets.Where(t => t.IsProjectPrefab).Select(t => t.ProjectPrefabAssetPath).ToArray();

            var sorted = prefabPaths.OrderBy(p => p, StringComparer.Ordinal).ToArray();
            CollectionAssert.AreEqual(sorted, prefabPaths, "Project prefabs should be listed in ordinal asset-path order");

            var baseIndex = Array.IndexOf(prefabPaths, basePath);
            var variantIndex = Array.IndexOf(prefabPaths, variantPath);
            Assert.GreaterOrEqual(baseIndex, 0, "Base prefab should be discovered");
            Assert.GreaterOrEqual(variantIndex, 0, "Variant prefab should be discovered");
            Assert.Less(variantIndex, baseIndex, "Path order should determine the listing even when it disagrees with dependency order");
        }

        // Prefab dependency ordering must also cover plain nested prefab instances, not just Prefab
        // Variant inheritance: "Container" is NOT a variant of "Nested", it just embeds an instance
        // of Nested as a child with no override of its own. If Container were migrated before Nested,
        // it would read Nested's still-unmigrated legacy settings through inheritance and write an
        // unnecessary (though not corrupting) instance-level override to "freeze" that soon-to-change
        // value, instead of correctly resolving to a clean no-op once Nested is actually migrated.
        [Test]
        public void Migrate_NestedNonVariantPrefabDependency_MigratesDependencyFirstAndAvoidsRedundantOverride()
        {
            var nestedRoot = CreateAvatarHierarchy("Nested");
            var nestedBone = nestedRoot.GetComponentInChildren<VRCPhysBone>();
            nestedRoot.GetComponent<AvatarConverterSettings>().physBonesToKeep = new[] { nestedBone };
            var nestedPath = SaveAsPrefabAndDestroy(nestedRoot, "Nested");
            var nestedAsset = AssetDatabase.LoadAssetAtPath<GameObject>(nestedPath);

            var containerRoot = new GameObject("Container");
            containerRoot.transform.SetParent(TestRoot.transform);
            PrefabUtility.InstantiatePrefab(nestedAsset, containerRoot.transform);
            var containerPath = SaveAsPrefabAndDestroy(containerRoot, "Container");

            Assert.IsFalse(PrefabUtility.IsPartOfVariantPrefab(AssetDatabase.LoadAssetAtPath<GameObject>(containerPath)), "Sanity check: Container is not a Prefab Variant of Nested, just a container for a nested instance of it");

            var nestedTarget = AvatarDynamicsMigrationTarget.ForProjectPrefab(nestedPath);
            var containerTarget = AvatarDynamicsMigrationTarget.ForProjectPrefab(containerPath);

            // Pass Container first to confirm Migrate (not the caller) resolves the dependency order.
            var result = AvatarDynamicsMigrationUtility.Migrate(new[] { containerTarget, nestedTarget });

            Assert.AreEqual(1, result.MigratedCount, "Only Nested needed an actual change");
            Assert.AreEqual(1, result.SkippedCount, "Container should resolve to a clean no-op via inheritance, not get a redundant override");

            var migratedNested = AssetDatabase.LoadAssetAtPath<GameObject>(nestedPath);
            Assert.IsFalse(migratedNested.GetComponent<AvatarConverterSettings>().HasLegacyAvatarDynamicsSettings);

            var migratedContainer = AssetDatabase.LoadAssetAtPath<GameObject>(containerPath);
            Assert.IsFalse(migratedContainer.GetComponentInChildren<AvatarConverterSettings>(true).HasLegacyAvatarDynamicsSettings);
        }

        // Same class of bug as Migrate_BaseAndVariantBothHaveOwnLegacySettings_EachMigratesIndependently,
        // but between a Prefab Variant asset and a scene instance of it, rather than between two
        // prefab assets. Base has no legacy settings; the variant has its own (keeps "Bone"); a
        // scene-file instance of the VARIANT overrides with a different, equal-length selection
        // (keeps "BoneB" instead). Both the variant and the scene instance are migrated in the same
        // batch and must end up with their own, independent, correct PCR state.
        [Test]
        public void Migrate_PrefabVariantAndSceneInstanceBothHaveOwnLegacySettings_EachMigratesIndependently()
        {
            var baseRoot = CreateAvatarHierarchy("PVSI_Base");
            var boneBObject = new GameObject("BoneB");
            boneBObject.transform.SetParent(baseRoot.transform);
            boneBObject.AddComponent<VRCPhysBone>();
            var basePath = SaveAsPrefabAndDestroy(baseRoot, "PVSI_Base");
            var baseAsset = AssetDatabase.LoadAssetAtPath<GameObject>(basePath);

            // Variant keeps "Bone" (BoneA), so "BoneB" is marked for removal.
            var variantInstance = (GameObject)PrefabUtility.InstantiatePrefab(baseAsset);
            var variantBoneA = variantInstance.transform.Find("Bone").GetComponent<VRCPhysBone>();
            variantInstance.GetComponent<AvatarConverterSettings>().physBonesToKeep = new[] { variantBoneA };
            var variantPath = SaveAsPrefabAndDestroy(variantInstance, "PVSI_Variant");
            Assert.IsTrue(PrefabUtility.IsPartOfVariantPrefab(AssetDatabase.LoadAssetAtPath<GameObject>(variantPath)), "Sanity check: should be a Prefab Variant");
            var variantAsset = AssetDatabase.LoadAssetAtPath<GameObject>(variantPath);

            // A scene-file instance of the variant overrides with the opposite selection: keeps
            // BoneB, removes BoneA.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var sceneInstance = (GameObject)PrefabUtility.InstantiatePrefab(variantAsset, scene);
            var instanceBoneB = sceneInstance.transform.Find("BoneB").GetComponent<VRCPhysBone>();
            var instanceConverterSettings = sceneInstance.GetComponent<AvatarConverterSettings>();
            instanceConverterSettings.physBonesToKeep = new[] { instanceBoneB };
            PrefabUtility.RecordPrefabInstancePropertyModifications(instanceConverterSettings);
            var scenePath = SaveActiveSceneAndUnload(scene, "PVSI_Scene");

            var variantTarget = AvatarDynamicsMigrationTarget.ForProjectPrefab(variantPath);
            var sceneTarget = AvatarDynamicsMigrationTarget.ForSceneFile(scenePath);
            var result = AvatarDynamicsMigrationUtility.Migrate(new[] { variantTarget, sceneTarget });

            Assert.AreEqual(2, result.MigratedCount, "Both the variant and the scene instance have their own distinct legacy settings to migrate");
            Assert.AreEqual(0, result.SkippedCount);

            // Variant: BoneA kept (no PCR), BoneB removed (PCR removeOnAndroid=true).
            var migratedVariant = AssetDatabase.LoadAssetAtPath<GameObject>(variantPath);
            var variantBones = migratedVariant.GetComponentsInChildren<VRCPhysBone>(true);
            var vBoneA = variantBones.First(b => b.gameObject.name == "Bone");
            var vBoneB = variantBones.First(b => b.gameObject.name == "BoneB");
            Assert.IsNull(vBoneA.gameObject.GetComponent<PlatformComponentRemover>(), "Variant: kept BoneA should have no PCR");
            var variantRemoverB = vBoneB.gameObject.GetComponent<PlatformComponentRemover>();
            Assert.IsNotNull(variantRemoverB, "Variant: removed BoneB should have a PCR");
            Assert.IsTrue(Array.Find(variantRemoverB.componentSettings, s => s.component == vBoneB).removeOnAndroid);
            Assert.IsFalse(migratedVariant.GetComponent<AvatarConverterSettings>().HasLegacyAvatarDynamicsSettings);

            // Scene instance: independently, BoneB kept and BoneA removed - the exact opposite of
            // the variant it's an instance of, proving the instance's migration used its OWN
            // override data, and that the instance correctly overrides the PCR it inherited from
            // the variant (BoneB's inherited remover must be neutralized/removed at the instance
            // level, and BoneA needs a new instance-level remover the variant never had).
            var reopened = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            try
            {
                var reopenedConverterSettings = reopened.GetRootGameObjects()
                    .SelectMany(r => r.GetComponentsInChildren<AvatarConverterSettings>(true))
                    .First();
                Assert.IsFalse(reopenedConverterSettings.HasLegacyAvatarDynamicsSettings);

                var sceneBones = reopened.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<VRCPhysBone>(true)).ToArray();
                var sBoneA = sceneBones.First(b => b.gameObject.name == "Bone");
                var sBoneB = sceneBones.First(b => b.gameObject.name == "BoneB");

                var removerA = sBoneA.gameObject.GetComponent<PlatformComponentRemover>();
                Assert.IsNotNull(removerA, "Scene instance: removed BoneA should have a PCR (added at the instance level)");
                Assert.IsTrue(Array.Find(removerA.componentSettings, s => s.component == sBoneA).removeOnAndroid);

                var removerB = sBoneB.gameObject.GetComponent<PlatformComponentRemover>();
                var settingB = removerB != null ? Array.Find(removerB.componentSettings, s => s.component == sBoneB) : null;
                Assert.IsTrue(removerB == null || settingB == null || !settingB.removeOnAndroid, "Scene instance: kept BoneB should not be marked for Android removal");
            }
            finally
            {
                EditorSceneManager.CloseScene(reopened, true);
            }
        }

        // Regression test mirroring Migrate_OnlyBaseSelected_VariantsIndependentLegacySettingsSurviveUntouched,
        // for the Prefab/Prefab Variant -> scene-file-instance relationship instead of the
        // base-prefab -> variant-prefab one.
        [Test]
        public void Migrate_OnlyPrefabVariantSelected_SceneInstanceIndependentLegacySettingsSurviveUntouched()
        {
            var baseRoot = CreateAvatarHierarchy("SurvivePVSI_Base");
            var boneBObject = new GameObject("BoneB");
            boneBObject.transform.SetParent(baseRoot.transform);
            boneBObject.AddComponent<VRCPhysBone>();
            var basePath = SaveAsPrefabAndDestroy(baseRoot, "SurvivePVSI_Base");
            var baseAsset = AssetDatabase.LoadAssetAtPath<GameObject>(basePath);

            var variantInstance = (GameObject)PrefabUtility.InstantiatePrefab(baseAsset);
            var variantBoneA = variantInstance.transform.Find("Bone").GetComponent<VRCPhysBone>();
            variantInstance.GetComponent<AvatarConverterSettings>().physBonesToKeep = new[] { variantBoneA };
            var variantPath = SaveAsPrefabAndDestroy(variantInstance, "SurvivePVSI_Variant");
            var variantAsset = AssetDatabase.LoadAssetAtPath<GameObject>(variantPath);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var sceneInstance = (GameObject)PrefabUtility.InstantiatePrefab(variantAsset, scene);
            var instanceBoneB = sceneInstance.transform.Find("BoneB").GetComponent<VRCPhysBone>();
            var instanceConverterSettings = sceneInstance.GetComponent<AvatarConverterSettings>();
            instanceConverterSettings.physBonesToKeep = new[] { instanceBoneB };
            PrefabUtility.RecordPrefabInstancePropertyModifications(instanceConverterSettings);
            var scenePath = SaveActiveSceneAndUnload(scene, "SurvivePVSI_Scene");

            // Migrate ONLY the variant - the scene file is deliberately left out of this batch.
            var variantTarget = AvatarDynamicsMigrationTarget.ForProjectPrefab(variantPath);
            var result = AvatarDynamicsMigrationUtility.Migrate(new[] { variantTarget });

            Assert.AreEqual(1, result.MigratedCount);
            Assert.IsFalse(AssetDatabase.LoadAssetAtPath<GameObject>(variantPath).GetComponent<AvatarConverterSettings>().HasLegacyAvatarDynamicsSettings, "Variant should be migrated");

            var reopened = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            try
            {
                var untouchedConverterSettings = reopened.GetRootGameObjects()
                    .SelectMany(r => r.GetComponentsInChildren<AvatarConverterSettings>(true))
                    .First();
                Assert.IsTrue(untouchedConverterSettings.HasLegacyAvatarDynamicsSettings, "Scene instance's own legacy settings must survive even though the scene was never included in the batch");
                var keptBones = untouchedConverterSettings.physBonesToKeep.Where(p => p != null).ToArray();
                Assert.AreEqual(1, keptBones.Length);
                Assert.AreEqual("BoneB", keptBones[0].gameObject.name, "Scene instance should still intend to keep its own BoneB, not have inherited the variant's migrated state");
            }
            finally
            {
                EditorSceneManager.CloseScene(reopened, true);
            }
        }

        [Test]
        public void Migrate_SceneInstanceOverride_MigratesInstanceWithoutTouchingSourcePrefab()
        {
            // Source prefab never had legacy settings.
            var baseRoot = CreateAvatarHierarchy("InstanceOverrideBase");
            var basePath = SaveAsPrefabAndDestroy(baseRoot, "InstanceOverrideBase");
            var baseAsset = AssetDatabase.LoadAssetAtPath<GameObject>(basePath);

            // A scene instance overrides physBonesToKeep by itself.
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(baseAsset, TestRoot.transform);
            var instancePhysBone = instance.GetComponentInChildren<VRCPhysBone>();
            var instanceConverterSettings = instance.GetComponent<AvatarConverterSettings>();
            instanceConverterSettings.physBonesToKeep = new[] { instancePhysBone };
            PrefabUtility.RecordPrefabInstancePropertyModifications(instanceConverterSettings);

            var targets = AvatarDynamicsMigrationUtility.FindMigrationTargets();
            var sceneTarget = targets.FirstOrDefault(t => !t.IsProjectPrefab && t.SceneConverterSettings == instanceConverterSettings);
            Assert.IsNotNull(sceneTarget, "The scene instance override should be discovered as a scene target");

            var result = AvatarDynamicsMigrationUtility.Migrate(new[] { sceneTarget });
            Assert.AreEqual(1, result.MigratedCount);

            Assert.IsFalse(instanceConverterSettings.HasLegacyAvatarDynamicsSettings, "Instance override should be cleared");

            var sourceSettings = AssetDatabase.LoadAssetAtPath<GameObject>(basePath).GetComponent<AvatarConverterSettings>();
            Assert.IsFalse(sourceSettings.HasLegacyAvatarDynamicsSettings, "Source prefab never had legacy settings and must stay untouched");
        }

        [Test]
        public void Migrate_UnloadedSceneFile_MigratesAndSaves()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = BuildAvatarHierarchy("SceneFileAvatar");
            var physBone = root.GetComponentInChildren<VRCPhysBone>();
            root.GetComponent<AvatarConverterSettings>().physBonesToKeep = new[] { physBone };
            var scenePath = SaveActiveSceneAndUnload(scene, "TargetScene");

            var target = AvatarDynamicsMigrationTarget.ForSceneFile(scenePath);
            var result = AvatarDynamicsMigrationUtility.Migrate(new[] { target });

            Assert.AreEqual(1, result.MigratedCount);

            var reopened = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            try
            {
                var converterSettings = reopened.GetRootGameObjects()
                    .SelectMany(r => r.GetComponentsInChildren<AvatarConverterSettings>(true))
                    .First();
                Assert.IsTrue(converterSettings.physBonesToKeep.All(p => p == null), "Legacy field should be cleared and saved to the scene file");
                Assert.IsFalse(converterSettings.HasLegacyAvatarDynamicsSettings);
            }
            finally
            {
                EditorSceneManager.CloseScene(reopened, true);
            }
        }

        [Test]
        public void Migrate_SceneFileWithMultipleAvatars_MigratesOnlySelectedAvatar()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var avatarA = BuildAvatarHierarchy("AvatarA");
            avatarA.GetComponent<AvatarConverterSettings>().physBonesToKeep = new[] { avatarA.GetComponentInChildren<VRCPhysBone>() };
            var avatarB = BuildAvatarHierarchy("AvatarB");
            avatarB.GetComponent<AvatarConverterSettings>().physBonesToKeep = new[] { avatarB.GetComponentInChildren<VRCPhysBone>() };
            var scenePath = SaveActiveSceneAndUnload(scene, "MultiAvatarScene");

            var targets = AvatarDynamicsMigrationUtility.FindMigrationTargets();
            var fileTargets = targets.Where(t => t.IsSceneFile && t.SceneFileAssetPath == scenePath).ToArray();
            Assert.AreEqual(2, fileTargets.Length, "Each AvatarConverterSettings in the scene file should be listed as its own target");

            var targetA = fileTargets.First(t => t.SceneFileHierarchyPath == "AvatarA");
            var result = AvatarDynamicsMigrationUtility.Migrate(new[] { targetA });

            Assert.AreEqual(1, result.MigratedCount);
            Assert.AreEqual(0, result.SkippedCount);

            var reopened = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            try
            {
                var all = reopened.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<AvatarConverterSettings>(true)).ToArray();
                var a = all.First(c => c.gameObject.name == "AvatarA");
                var b = all.First(c => c.gameObject.name == "AvatarB");
                Assert.IsFalse(a.HasLegacyAvatarDynamicsSettings, "Selected AvatarA should be migrated");
                Assert.IsTrue(b.HasLegacyAvatarDynamicsSettings, "Unselected AvatarB must keep its legacy settings");
            }
            finally
            {
                EditorSceneManager.CloseScene(reopened, true);
            }
        }

        // Regression test: ApplyWithoutUndo must call RecordPrefabInstancePropertyModifications, or an
        // instance-level legacy override inside a scene file silently reverts to the (non-legacy)
        // prefab source's values once the migrated scene is saved and reloaded.
        [Test]
        public void Migrate_UnloadedSceneFile_PrefabInstanceOverride_PersistsAfterSave()
        {
            var baseRoot = CreateAvatarHierarchy("SceneInstanceBase");
            var basePath = SaveAsPrefabAndDestroy(baseRoot, "SceneInstanceBase");
            var baseAsset = AssetDatabase.LoadAssetAtPath<GameObject>(basePath);

            // Replaces the active scene (TestRoot is already empty at this point), which sidesteps
            // this environment's "Cannot create a new scene additively with an untitled scene
            // unsaved" restriction on EditorSceneManager.NewScene(..., Additive).
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(baseAsset, scene);
            var instancePhysBone = instance.GetComponentInChildren<VRCPhysBone>();
            var instanceConverterSettings = instance.GetComponent<AvatarConverterSettings>();
            instanceConverterSettings.physBonesToKeep = new[] { instancePhysBone };
            PrefabUtility.RecordPrefabInstancePropertyModifications(instanceConverterSettings);
            var scenePath = SaveActiveSceneAndUnload(scene, "InstanceOverrideScene");

            var target = AvatarDynamicsMigrationTarget.ForSceneFile(scenePath);
            var result = AvatarDynamicsMigrationUtility.Migrate(new[] { target });

            Assert.AreEqual(1, result.MigratedCount);

            var reopened = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            try
            {
                var converterSettings = reopened.GetRootGameObjects()
                    .SelectMany(r => r.GetComponentsInChildren<AvatarConverterSettings>(true))
                    .First();
                Assert.IsTrue(converterSettings.physBonesToKeep.All(p => p == null), "Instance override should be cleared AND persisted to disk");
                Assert.IsFalse(converterSettings.HasLegacyAvatarDynamicsSettings);
            }
            finally
            {
                EditorSceneManager.CloseScene(reopened, true);
            }

            var sourceSettings = AssetDatabase.LoadAssetAtPath<GameObject>(basePath).GetComponent<AvatarConverterSettings>();
            Assert.IsFalse(sourceSettings.HasLegacyAvatarDynamicsSettings, "Source prefab never had legacy settings and must stay untouched");
        }

        [Test]
        public void FindMigrationTargets_ExcludesAlreadyLoadedScenePath()
        {
            // Replaces the active scene so the resulting scene keeps whatever path we save it to
            // (SaveScene without saveAsCopy) while staying loaded/active - exactly the "already
            // loaded at this exact path" condition this test needs to exercise.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = BuildAvatarHierarchy("LoadedSceneAvatar");
            var physBone = root.GetComponentInChildren<VRCPhysBone>();
            var converterSettings = root.GetComponent<AvatarConverterSettings>();
            converterSettings.physBonesToKeep = new[] { physBone };
            var scenePath = $"{testFolder}/LoadedScene.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            try
            {
                var targets = AvatarDynamicsMigrationUtility.FindMigrationTargets();

                var fileTarget = targets.FirstOrDefault(t => t.IsSceneFile && t.SceneFileAssetPath == scenePath);
                Assert.IsNull(fileTarget, "An already-loaded scene should not also be scanned as a separate scene-file target");

                var liveTarget = targets.FirstOrDefault(t => !t.IsProjectPrefab && !t.IsSceneFile && t.SceneConverterSettings == converterSettings);
                Assert.IsNotNull(liveTarget, "The already-loaded scene's avatar should still be found via the live-scene scan");
            }
            finally
            {
                // Move off the doomed path before TearDown deletes testFolder, so no console error
                // fires for a loaded scene whose backing file just disappeared out from under it.
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }

        private GameObject BuildAvatarHierarchy(string name)
        {
            var root = new GameObject(name);
            root.AddComponent<VRCAvatarDescriptor>();
            root.AddComponent<AvatarConverterSettings>();

            var bone = new GameObject("Bone");
            bone.transform.SetParent(root.transform);
            bone.AddComponent<VRCPhysBone>();

            return root;
        }

        private GameObject CreateAvatarHierarchy(string name)
        {
            var root = BuildAvatarHierarchy(name);
            root.transform.SetParent(TestRoot.transform);
            return root;
        }

        private string SaveAsPrefabAndDestroy(GameObject instance, string assetName)
        {
            var path = $"{testFolder}/{assetName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            UnityEngine.Object.DestroyImmediate(instance);
            return path;
        }

        // Saves the given (currently active, Single-mode) scene to a path under testFolder, then
        // replaces the active scene so the saved file is left unloaded - i.e. an ordinary project
        // scene file on disk, matching what AvatarDynamicsMigrationUtility scans for. CloseScene
        // isn't used here because this environment's EditorSceneManager.NewScene(..., Additive)
        // (needed to have more than one loaded scene to close down to) refuses to run while the
        // active scene is untitled/unsaved, which is the state every test starts from.
        private string SaveActiveSceneAndUnload(Scene scene, string sceneName)
        {
            var path = $"{testFolder}/{sceneName}.unity";
            EditorSceneManager.SaveScene(scene, path);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            return path;
        }
    }
}
