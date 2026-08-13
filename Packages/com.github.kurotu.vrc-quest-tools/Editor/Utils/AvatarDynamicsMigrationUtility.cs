// <copyright file="AvatarDynamicsMigrationUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#pragma warning disable CS0618

using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models.VRChat;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// A single migration target: a project prefab asset, an unloaded project scene file, or a live
    /// AvatarConverterSettings instance (in a loaded scene, or in the currently open Prefab Stage).
    /// </summary>
    internal class AvatarDynamicsMigrationTarget
    {
        /// <summary>
        /// Gets a display label for the target.
        /// </summary>
        internal string Label { get; }

        /// <summary>
        /// Gets the asset path when this target is a project prefab; otherwise null.
        /// </summary>
        internal string ProjectPrefabAssetPath { get; }

        /// <summary>
        /// Gets the scene asset path when this target is an unloaded project scene file; otherwise null.
        /// </summary>
        internal string SceneFileAssetPath { get; }

        /// <summary>
        /// Gets the hierarchy path of the target AvatarConverterSettings inside the scene file, or
        /// null when the target stands for the whole scene file.
        /// </summary>
        internal string SceneFileHierarchyPath { get; }

        /// <summary>
        /// Gets the scene this target belongs to (asset path, or scene name for an unsaved scene),
        /// for grouping scene targets per scene in the UI. Null for project prefab targets.
        /// </summary>
        internal string SceneGroupLabel { get; }

        /// <summary>
        /// Gets the label of the target object within its scene (hierarchy path). Null for project
        /// prefab targets.
        /// </summary>
        internal string ObjectLabel { get; }

        /// <summary>
        /// Gets the live AvatarConverterSettings when this target is a scene (or Prefab Stage) object; otherwise null.
        /// </summary>
        internal AvatarConverterSettings SceneConverterSettings { get; }

        /// <summary>
        /// Gets a value indicating whether this target is a project prefab asset.
        /// </summary>
        internal bool IsProjectPrefab => ProjectPrefabAssetPath != null;

        /// <summary>
        /// Gets a value indicating whether this target is an unloaded project scene file.
        /// </summary>
        internal bool IsSceneFile => SceneFileAssetPath != null;

        private AvatarDynamicsMigrationTarget(string label, string projectPrefabAssetPath, string sceneFileAssetPath, string sceneFileHierarchyPath, string sceneGroupLabel, string objectLabel, AvatarConverterSettings sceneConverterSettings)
        {
            Label = label;
            ProjectPrefabAssetPath = projectPrefabAssetPath;
            SceneFileAssetPath = sceneFileAssetPath;
            SceneFileHierarchyPath = sceneFileHierarchyPath;
            SceneGroupLabel = sceneGroupLabel;
            ObjectLabel = objectLabel;
            SceneConverterSettings = sceneConverterSettings;
        }

        internal static AvatarDynamicsMigrationTarget ForProjectPrefab(string assetPath)
        {
            return new AvatarDynamicsMigrationTarget(assetPath, assetPath, null, null, null, null, null);
        }

        internal static AvatarDynamicsMigrationTarget ForSceneFile(string assetPath)
        {
            return new AvatarDynamicsMigrationTarget(assetPath, null, assetPath, null, assetPath, null, null);
        }

        internal static AvatarDynamicsMigrationTarget ForSceneFileObject(string assetPath, string hierarchyPath)
        {
            return new AvatarDynamicsMigrationTarget($"{assetPath} [{hierarchyPath}]", null, assetPath, hierarchyPath, assetPath, hierarchyPath, null);
        }

        internal static AvatarDynamicsMigrationTarget ForSceneObject(string sceneGroupLabel, string hierarchyPath, AvatarConverterSettings converterSettings)
        {
            return new AvatarDynamicsMigrationTarget($"{sceneGroupLabel} [{hierarchyPath}]", null, null, null, sceneGroupLabel, hierarchyPath, converterSettings);
        }
    }

    /// <summary>
    /// Result of running <see cref="AvatarDynamicsMigrationUtility.Migrate(IEnumerable{AvatarDynamicsMigrationTarget})"/>.
    /// </summary>
    internal readonly struct AvatarDynamicsMigrationResult
    {
        /// <summary>
        /// Gets the number of targets that were actually migrated.
        /// </summary>
        internal int MigratedCount { get; }

        /// <summary>
        /// Gets the number of selected targets that turned out to have nothing left to migrate
        /// (e.g. a prefab variant resolved via an already-migrated base prefab).
        /// </summary>
        internal int SkippedCount { get; }

        internal AvatarDynamicsMigrationResult(int migratedCount, int skippedCount)
        {
            MigratedCount = migratedCount;
            SkippedCount = skippedCount;
        }
    }

    /// <summary>
    /// Finds and migrates <see cref="AvatarConverterSettings"/> components which still store their
    /// Avatar Dynamics settings in the legacy physBonesToKeep/physBoneCollidersToKeep/contactsToKeep
    /// fields, converting them to <see cref="PlatformComponentRemover"/> components.
    /// </summary>
    internal static class AvatarDynamicsMigrationUtility
    {
        /// <summary>
        /// Scans project prefabs and scene files under Assets/, and AvatarConverterSettings in loaded
        /// scenes (and the currently open Prefab Stage), for legacy Avatar Dynamics settings.
        /// Scene files are listed per contained AvatarConverterSettings, not per file.
        /// Prefabs and scene files are ordered by asset path for stable, predictable display;
        /// <see cref="Migrate"/> re-derives the dependency-safe processing order itself, so the
        /// scan order carries no execution-order meaning.
        /// Scanning unloaded scene files requires briefly opening each one, so this is noticeably
        /// more expensive than the prefab/loaded-scene scan alone.
        /// </summary>
        /// <returns>Migration targets ordered by asset path.</returns>
        internal static AvatarDynamicsMigrationTarget[] FindMigrationTargets()
        {
            return FindProjectPrefabTargets().Concat(FindSceneFileTargets()).Concat(FindLoadedSceneTargets()).ToArray();
        }

        /// <summary>
        /// Migrates the given targets. Project prefab assets are processed base-before-variant and
        /// saved directly to disk (not covered by Undo); unloaded scene files are opened additively,
        /// migrated, saved, and closed as a batch (also not covered by Undo); already-loaded scene
        /// and Prefab Stage objects are migrated in place using the existing Undo-tracked path and
        /// left for the user to save. Each target is re-checked for legacy settings immediately
        /// before migrating, since migrating a base prefab can resolve a variant's (or a scene file's
        /// prefab instance's) legacy settings without any change being needed there.
        /// </summary>
        /// <param name="targets">Targets to migrate.</param>
        /// <returns>Migration result summary.</returns>
        internal static AvatarDynamicsMigrationResult Migrate(IEnumerable<AvatarDynamicsMigrationTarget> targets)
        {
            var migratedCount = 0;
            var skippedCount = 0;

            var prefabTargets = targets.Where(t => t.IsProjectPrefab).ToArray();
            var orderedPrefabPaths = OrderPrefabPathsByDependencyDepth(prefabTargets.Select(t => t.ProjectPrefabAssetPath));
            var prefabTargetsByPath = prefabTargets.ToLookup(t => t.ProjectPrefabAssetPath);
            foreach (var path in orderedPrefabPaths)
            {
                foreach (var target in prefabTargetsByPath[path])
                {
                    if (MigrateProjectPrefab(target.ProjectPrefabAssetPath))
                    {
                        migratedCount++;
                    }
                    else
                    {
                        skippedCount++;
                    }
                }
            }

            // Grouped per scene file so each file is opened at most once even when several of its
            // AvatarConverterSettings are listed (and selected) as individual targets.
            foreach (var group in targets.Where(t => t.IsSceneFile).GroupBy(t => t.SceneFileAssetPath))
            {
                // A whole-file target (null hierarchy path) subsumes any per-object selection.
                var hierarchyPaths = group.Any(t => t.SceneFileHierarchyPath == null)
                    ? null
                    : new HashSet<string>(group.Select(t => t.SceneFileHierarchyPath));
                var sceneResult = MigrateSceneFile(group.Key, hierarchyPaths);
                migratedCount += sceneResult.MigratedCount;
                skippedCount += sceneResult.SkippedCount;
            }

            foreach (var target in targets.Where(t => !t.IsProjectPrefab && !t.IsSceneFile))
            {
                if (MigrateSceneObject(target.SceneConverterSettings))
                {
                    migratedCount++;
                }
                else
                {
                    skippedCount++;
                }
            }

            return new AvatarDynamicsMigrationResult(migratedCount, skippedCount);
        }

        private static IEnumerable<AvatarDynamicsMigrationTarget> FindProjectPrefabTargets()
        {
            var currentStagePath = PrefabStageUtility.GetCurrentPrefabStage()?.assetPath;

            var candidatePaths = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Distinct()
                .Where(path => path != currentStagePath)
                .Select(path => (path, root: AssetDatabase.LoadAssetAtPath<GameObject>(path)))
                .Where(x => x.root != null && !PrefabUtility.IsPartOfImmutablePrefab(x.root))
                .Where(x => x.root.GetComponentsInChildren<AvatarConverterSettings>(true).Any(HasLegacySettings))
                .Select(x => x.path)
                .OrderBy(path => path, System.StringComparer.Ordinal)
                .ToArray();

            return candidatePaths.Select(AvatarDynamicsMigrationTarget.ForProjectPrefab);
        }

        private static IEnumerable<AvatarDynamicsMigrationTarget> FindSceneFileTargets()
        {
            var loadedPaths = new HashSet<string>(
                Enumerable.Range(0, SceneManager.sceneCount)
                    .Select(SceneManager.GetSceneAt)
                    .Select(s => s.path)
                    .Where(p => !string.IsNullOrEmpty(p)));

            var scenePaths = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Distinct()
                .Where(path => !loadedPaths.Contains(path))
                .OrderBy(path => path, System.StringComparer.Ordinal);

            var targets = new List<AvatarDynamicsMigrationTarget>();
            foreach (var scenePath in scenePaths)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                try
                {
                    var sceneTargets = scene.GetRootGameObjects()
                        .SelectMany(root => root.GetComponentsInChildren<AvatarConverterSettings>(true))
                        .Where(HasLegacySettings)
                        .Select(c => AvatarDynamicsMigrationTarget.ForSceneFileObject(scenePath, GetHierarchyPath(c.gameObject)));
                    targets.AddRange(sceneTargets);
                }
                finally
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            return targets;
        }

        private static IEnumerable<AvatarDynamicsMigrationTarget> FindLoadedSceneTargets()
        {
            var targets = new List<AvatarDynamicsMigrationTarget>();

            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null)
            {
                var stageTargets = stage.prefabContentsRoot.GetComponentsInChildren<AvatarConverterSettings>(true)
                    .Where(HasLegacySettings)
                    .Select(c => AvatarDynamicsMigrationTarget.ForSceneObject($"{stage.assetPath} (Prefab Stage)", GetHierarchyPath(c.gameObject), c));
                targets.AddRange(stageTargets);
            }

            var scenes = Enumerable.Range(0, SceneManager.sceneCount)
                .Select(SceneManager.GetSceneAt)
                .Where(s => s.isLoaded);
            foreach (var scene in scenes)
            {
                var sceneLabel = string.IsNullOrEmpty(scene.path) ? scene.name : scene.path;
                var sceneTargets = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<AvatarConverterSettings>(true))
                    .Where(HasLegacySettings)
                    .Select(c => AvatarDynamicsMigrationTarget.ForSceneObject(sceneLabel, GetHierarchyPath(c.gameObject), c));
                targets.AddRange(sceneTargets);
            }

            return targets;
        }

        private static bool MigrateProjectPrefab(string assetPath)
        {
            // Guard against stale scan data: if the user opened this exact prefab in Prefab Mode
            // after it was scanned, LoadPrefabContents/SaveAsPrefabAsset here would silently
            // overwrite their in-stage edits on disk. Migrate the live stage contents instead (Undo-
            // tracked, like any other already-open scene/stage object) and leave saving to the user.
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && stage.assetPath == assetPath)
            {
                var didMigrateStage = false;
                foreach (var converterSettings in stage.prefabContentsRoot.GetComponentsInChildren<AvatarConverterSettings>(true))
                {
                    if (MigrateSceneObject(converterSettings))
                    {
                        didMigrateStage = true;
                    }
                }

                return didMigrateStage;
            }

            var root = PrefabUtility.LoadPrefabContents(assetPath);
            try
            {
                var didMigrate = false;
                foreach (var converterSettings in root.GetComponentsInChildren<AvatarConverterSettings>(true))
                {
                    // Re-check: a base prefab migrated earlier in this run may already have resolved
                    // this component's (inherited) legacy settings, leaving nothing to change here.
                    if (!HasLegacySettings(converterSettings))
                    {
                        continue;
                    }

                    MigrateOne(converterSettings, false);
                    didMigrate = true;
                }

                if (didMigrate)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, assetPath);
                }

                return didMigrate;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static AvatarDynamicsMigrationResult MigrateSceneFile(string scenePath, HashSet<string> hierarchyPaths)
        {
            // Guard against stale scan data: if the user opened this exact scene themselves after
            // it was scanned, OpenScene/SaveScene/CloseScene here would silently commit their
            // unrelated edits and yank the scene out of the Hierarchy. Migrate it in place instead,
            // like any other already-loaded scene, and leave saving to the user.
            var loadedScene = SceneManager.GetSceneByPath(scenePath);
            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                var loadedMigrated = 0;
                var loadedSkipped = 0;
                foreach (var converterSettings in SelectConverterSettings(loadedScene, hierarchyPaths))
                {
                    if (MigrateSceneObject(converterSettings))
                    {
                        loadedMigrated++;
                    }
                    else
                    {
                        loadedSkipped++;
                    }
                }

                return new AvatarDynamicsMigrationResult(loadedMigrated, loadedSkipped);
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            try
            {
                var migrated = 0;
                var skipped = 0;
                foreach (var converterSettings in SelectConverterSettings(scene, hierarchyPaths))
                {
                    // Re-check: a base prefab migrated earlier in this run may already have resolved
                    // a prefab instance's (inherited) legacy settings, leaving nothing to change here.
                    if (!HasLegacySettings(converterSettings))
                    {
                        skipped++;
                        continue;
                    }

                    MigrateOne(converterSettings, false);
                    migrated++;
                }

                if (migrated > 0)
                {
                    EditorSceneManager.SaveScene(scene, scenePath);
                }

                return new AvatarDynamicsMigrationResult(migrated, skipped);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        // With a null filter (whole-file target), only components that still have legacy settings
        // count - the others were never listed as targets, so they are neither migrated nor
        // "skipped". With an explicit selection, every selected component is returned so a
        // since-resolved one is reported as skipped rather than silently dropped.
        private static IEnumerable<AvatarConverterSettings> SelectConverterSettings(Scene scene, HashSet<string> hierarchyPaths)
        {
            var all = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<AvatarConverterSettings>(true));
            return hierarchyPaths == null
                ? all.Where(HasLegacySettings)
                : all.Where(c => hierarchyPaths.Contains(GetHierarchyPath(c.gameObject)));
        }

        private static bool MigrateSceneObject(AvatarConverterSettings converterSettings)
        {
            if (!HasLegacySettings(converterSettings))
            {
                return false;
            }

            MigrateOne(converterSettings, true);

            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && converterSettings.gameObject.scene == stage.scene)
            {
                EditorSceneManager.MarkSceneDirty(stage.scene);
            }
            else
            {
                EditorSceneManager.MarkSceneDirty(converterSettings.gameObject.scene);
            }

            return true;
        }

        private static void MigrateOne(AvatarConverterSettings converterSettings, bool recordUndo)
        {
            var providers = converterSettings.physBonesToKeep
                .Where(p => p != null)
                .Select(p => (VRCPhysBoneProviderBase)new VRCPhysBoneProvider(p))
                .ToArray();
            var colliders = converterSettings.physBoneCollidersToKeep.Where(c => c != null).ToArray();
            var contacts = converterSettings.contactsToKeep.Where(c => c != null).ToArray();

            if (recordUndo)
            {
                AvatarDynamicsSettingsUtility.Apply(converterSettings, providers, colliders, contacts);
            }
            else
            {
                AvatarDynamicsSettingsUtility.ApplyWithoutUndo(converterSettings, providers, colliders, contacts);
            }
        }

        private static bool HasLegacySettings(AvatarConverterSettings converterSettings)
        {
            return converterSettings != null
                && converterSettings.AvatarDescriptor != null
                && converterSettings.HasLegacyAvatarDynamicsSettings;
        }

        // Orders prefab asset paths so that, within this set, anything another prefab in the set
        // depends on comes first - covering both Prefab Variant inheritance (a variant references
        // its base) and plain nested prefab instances (a prefab containing a child instance of
        // another prefab in the set), not just variant lineage. This matters because migrating a
        // dependency after its dependent has already been read/migrated can make the dependent
        // either race a stale (pre-migration) copy of the dependency's legacy settings (picking up
        // an inherited value that's about to change, and writing an unnecessary/redundant override
        // of its own to "freeze" that soon-to-be-wrong value) or, for the specific legacy-array
        // fields this tool clears, silently drop an unrelated equal-length override (see
        // AvatarDynamicsSettingsUtility.ClearLegacyArraysInPlace for that mechanism - ordering
        // doesn't strictly need it there since arrays are never shrunk, but ordering still avoids
        // the redundant-override case).
        private static string[] OrderPrefabPathsByDependencyDepth(IEnumerable<string> paths)
        {
            var pathArray = paths.Distinct().ToArray();
            var pathSet = new HashSet<string>(pathArray);
            var depthCache = new Dictionary<string, int>();
            return pathArray.OrderBy(path => GetDependencyDepth(path, pathSet, depthCache)).ToArray();
        }

        // Depth relative to the OTHER paths in pathSet only: 0 if this asset doesn't depend (directly
        // or transitively) on any other path in the set, otherwise one more than the deepest such
        // dependency. AssetDatabase.GetDependencies naturally covers both a Prefab Variant's
        // reference to its base and a nested prefab instance's reference to its source prefab, so
        // this generalizes the earlier variant-only GetCorrespondingObjectFromSource-based depth.
        private static int GetDependencyDepth(string path, HashSet<string> pathSet, Dictionary<string, int> depthCache)
        {
            if (depthCache.TryGetValue(path, out var cached))
            {
                return cached;
            }

            var directDependencies = AssetDatabase.GetDependencies(path, false)
                .Where(dependency => dependency != path && pathSet.Contains(dependency));

            var depth = 0;
            foreach (var dependency in directDependencies)
            {
                depth = System.Math.Max(depth, GetDependencyDepth(dependency, pathSet, depthCache) + 1);
            }

            depthCache[path] = depth;
            return depth;
        }

        private static string GetHierarchyPath(GameObject gameObject)
        {
            var path = gameObject.name;
            var parent = gameObject.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}
