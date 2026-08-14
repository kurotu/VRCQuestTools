// <copyright file="AvatarDynamicsMigrationWindow.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Linq;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Views
{
    /// <summary>
    /// Review window for migrating legacy Avatar Dynamics settings (AvatarConverterSettings'
    /// physBonesToKeep/physBoneCollidersToKeep/contactsToKeep) to PlatformComponentRemover.
    /// Lists every affected project prefab and scene object so the user can exclude targets,
    /// or migrate nothing at all.
    /// </summary>
    internal class AvatarDynamicsMigrationWindow : EditorWindow
    {
        private AvatarDynamicsMigrationTarget[] targets = new AvatarDynamicsMigrationTarget[0];
        private bool[] selected = new bool[0];
        private Vector2 scrollPosition;
        private bool foldoutPrefabs = true;
        private bool foldoutScenes = true;

        /// <summary>
        /// Shows the window, scanning for migration targets.
        /// </summary>
        internal static void ShowWindow()
        {
            // Creating the window runs OnEnable, which already scans; an unconditional Rescan here
            // would repeat the expensive scan (it opens every unloaded scene under Assets/) on
            // first open. Rescan explicitly only when reusing an already-open window, so invoking
            // the menu again still refreshes the list.
            var isOpen = HasOpenInstances<AvatarDynamicsMigrationWindow>();
            var window = GetWindow<AvatarDynamicsMigrationWindow>();
            if (isOpen)
            {
                window.Rescan();
            }

            window.Show();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent(VRCQuestToolsSettings.I18nResource.MigrateAvatarDynamicsSettingsWindowTitle);
            Rescan();
        }

        private void Rescan()
        {
            targets = AvatarDynamicsMigrationUtility.FindMigrationTargets();
            selected = targets.Select(t => true).ToArray();
        }

        private void OnGUI()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;

            EditorGUILayout.LabelField(i18n.MigrateAvatarDynamicsSettingsDescription, EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();

            if (targets.Length == 0)
            {
                EditorGUILayout.HelpBox(i18n.MigrateAvatarDynamicsSettingsNoTargetsMessage, MessageType.Info);
                EditorGUILayout.Space();
                if (GUILayout.Button(i18n.CloseLabel))
                {
                    Close();
                }
                return;
            }

            EditorGUILayout.HelpBox(i18n.MigrateAvatarDynamicsSettingsAssetWarning, MessageType.Warning);
            EditorGUILayout.Space();

            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scrollView.scrollPosition;

                var prefabIndices = IndicesWhere(t => t.IsProjectPrefab);
                if (prefabIndices.Length > 0)
                {
                    if (foldoutPrefabs = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutPrefabs, $"{i18n.MigrateAvatarDynamicsSettingsProjectPrefabsLabel} ({prefabIndices.Length})"))
                    {
                        DrawTargetGroup(prefabIndices);
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    EditorGUILayout.Space();
                }

                var sceneIndices = IndicesWhere(t => !t.IsProjectPrefab);
                if (sceneIndices.Length > 0)
                {
                    if (foldoutScenes = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutScenes, $"{i18n.MigrateAvatarDynamicsSettingsSceneObjectsLabel} ({sceneIndices.Length})"))
                    {
                        DrawSceneTargetGroup(sceneIndices);
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
            }

            EditorGUILayout.Space(8);

            var selectedCount = selected.Count(s => s);
            using (new EditorGUI.DisabledScope(selectedCount == 0))
            {
                if (EditorGUIUtility.LargeButton(i18n.MigrateAvatarDynamicsSettingsButtonLabel(selectedCount)))
                {
                    OnClickMigrate();
                }
            }

            EditorGUILayout.Space(4);

            if (GUILayout.Button(i18n.CloseLabel))
            {
                Close();
            }
        }

        private int[] IndicesWhere(System.Func<AvatarDynamicsMigrationTarget, bool> predicate)
        {
            return Enumerable.Range(0, targets.Length).Where(i => predicate(targets[i])).ToArray();
        }

        private void DrawTargetGroup(int[] indices)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            using (new EditorGUI.IndentLevelScope())
            {
                using (var horizontal = new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(i18n.SelectAllButtonLabel))
                    {
                        foreach (var i in indices)
                        {
                            selected[i] = true;
                        }
                    }
                    if (GUILayout.Button(i18n.DeselectAllButtonLabel))
                    {
                        foreach (var i in indices)
                        {
                            selected[i] = false;
                        }
                    }
                }
                foreach (var i in indices)
                {
                    selected[i] = EditorGUILayout.ToggleLeft(targets[i].Label, selected[i]);
                }
            }
        }

        // Scene targets are grouped per scene: the scene appears once as a plain header row, and
        // each of its avatars gets an indented checkbox beneath it, so a scene containing several
        // avatars is visually one unit rather than repeated path-prefixed rows.
        private void DrawSceneTargetGroup(int[] indices)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            using (new EditorGUI.IndentLevelScope())
            {
                using (var horizontal = new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(i18n.SelectAllButtonLabel))
                    {
                        foreach (var i in indices)
                        {
                            selected[i] = true;
                        }
                    }
                    if (GUILayout.Button(i18n.DeselectAllButtonLabel))
                    {
                        foreach (var i in indices)
                        {
                            selected[i] = false;
                        }
                    }
                }
                foreach (var group in indices.GroupBy(i => targets[i].SceneGroupLabel))
                {
                    EditorGUILayout.LabelField(group.Key, EditorStyles.boldLabel);
                    using (new EditorGUI.IndentLevelScope())
                    {
                        foreach (var i in group)
                        {
                            selected[i] = EditorGUILayout.ToggleLeft(targets[i].ObjectLabel ?? targets[i].Label, selected[i]);
                        }
                    }
                }
            }
        }

        private void OnClickMigrate()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            var selectedTargets = Enumerable.Range(0, targets.Length).Where(i => selected[i]).Select(i => targets[i]).ToArray();
            var directWriteCount = selectedTargets.Count(t => t.IsProjectPrefab || t.IsSceneFile);

            if (directWriteCount > 0 && !EditorUtility.DisplayDialog(VRCQuestTools.Name, i18n.MigrateAvatarDynamicsSettingsConfirmation(directWriteCount), i18n.YesLabel, i18n.CancelLabel))
            {
                return;
            }

            var result = AvatarDynamicsMigrationUtility.Migrate(selectedTargets);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog(VRCQuestTools.Name, i18n.MigrateAvatarDynamicsSettingsResultMessage(result.MigratedCount, result.SkippedCount), i18n.CloseLabel);

            Rescan();
            Repaint();
        }
    }
}
