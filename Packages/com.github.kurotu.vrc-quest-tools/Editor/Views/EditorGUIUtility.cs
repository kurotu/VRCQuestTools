using System;
using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.I18n;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Validation.Performance;
using VRC.SDKBase.Validation.Performance.Stats;

namespace KRT.VRCQuestTools.Views
{
    /// <summary>
    /// Utility class for Editor GUI.
    /// </summary>
    internal static class EditorGUIUtility
    {
        private static Dictionary<DisplayLanguage, string> displayLanguageNames = System.Enum.GetValues(typeof(DisplayLanguage))
            .Cast<DisplayLanguage>()
            .ToDictionary(x => x, x =>
            {
                switch (x)
                {
                    case DisplayLanguage.Auto:
                        return "Auto";
                    case DisplayLanguage.English:
                        return "English";
                    case DisplayLanguage.Japanese:
                        return "日本語";
                    case DisplayLanguage.Russian:
                        return "Русский";
                    default:
                        throw new System.InvalidProgramException();
                }
            });

        /// <summary>
        /// Delegate for GUI function.
        /// </summary>
        internal delegate void GUICallback();

        /// <summary>
        /// Show language selector.
        /// </summary>
        internal static void LanguageSelector()
        {
            using (var ccs = new EditorGUI.ChangeCheckScope())
            {
                var language = (DisplayLanguage)EditorGUILayout.Popup("Language", (int)VRCQuestToolsSettings.DisplayLanguage, displayLanguageNames.Values.ToArray());
                if (ccs.changed)
                {
                    VRCQuestToolsSettings.DisplayLanguage = language;
                }
            }
        }

        /// <summary>
        /// Show update notification panel.
        /// </summary>
        internal static void UpdateNotificationPanel()
        {
            var latestVersion = VRCQuestToolsSettings.LatestVersionCache;
            var currentVersion = new SemVer(VRCQuestTools.Version);
            var skippedVersion = VRCQuestToolsSettings.SkippedVersion;
            var hasUpdate = latestVersion > currentVersion;
            if (latestVersion <= skippedVersion)
            {
                return;
            }

            if (hasUpdate)
            {
                var i18n = VRCQuestToolsSettings.I18nResource;

                var color = GUI.contentColor;
                GUI.contentColor = Color.red;
                GUILayout.Label($"Update: {currentVersion} -> {latestVersion}", EditorStyles.boldLabel);
                GUI.contentColor = color;
                if (latestVersion.IsMajorUpdate(currentVersion))
                {
                    EditorGUILayout.HelpBox(i18n.NewVersionHasBreakingChanges, MessageType.Warning);
                }
                GUILayout.BeginHorizontal();
                if (!VRCQuestTools.IsImportedAsPackage && GUILayout.Button(i18n.GetUpdate))
                {
                    Application.OpenURL(VRCQuestTools.BoothURL);
                }
                if (GUILayout.Button(i18n.SeeChangelog))
                {
                    var path = "docs/changelog/";
                    if (i18n is I18nJapanese)
                    {
                        path = $"ja/{path}";
                    }
                    var changelogURL = new Uri(new Uri(VRCQuestTools.DocsURL), path);
                    Application.OpenURL(changelogURL.AbsoluteUri);
                }
                if (GUILayout.Button(i18n.SkipThisVersion))
                {
                    VRCQuestToolsSettings.SkippedVersion = latestVersion;
                }
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Show a check list to select components.
        /// </summary>
        /// <typeparam name="T">Component type.</typeparam>
        /// <param name="objects">Components to list.</param>
        /// <param name="selectedObjects">Already selected components.</param>
        /// <returns>New selected components.</returns>
        internal static T[] AvatarDynamicsComponentSelectorList<T>(T[] objects, T[] selectedObjects)
            where T : UnityEngine.Component
        {
            var afterSelected = new List<T>();
            foreach (var obj in objects)
            {
                using (var horizontal = new EditorGUILayout.HorizontalScope())
                {
                    var isSelected = ToggleAvatarDynamicsComponentField(selectedObjects.Contains(obj), obj);
                    if (isSelected)
                    {
                        afterSelected.Add(obj);
                    }
                }
            }
            return afterSelected.ToArray();
        }

        /// <summary>
        /// Show a toggle field for Avatar Dynamics component.
        /// </summary>
        /// <param name="value">Current state.</param>
        /// <param name="component">Component to show.</param>
        /// <returns>true for selected.</returns>
        internal static bool ToggleAvatarDynamicsComponentField(bool value, Component component)
        {
            const int CheckBoxWidth = 16;
            using (var horizontal = new EditorGUILayout.HorizontalScope())
            {
                var selected = EditorGUILayout.Toggle(value, GUILayout.Width(CheckBoxWidth));
                GUILayout.Space(2);
                EditorGUILayout.ObjectField(component, component.GetType(), true);
                GUILayout.Space(2);
                EditorGUILayout.ObjectField(VRCSDKUtility.GetRootTransform(component), typeof(Transform), true);
                return selected;
            }
        }

        /// <summary>
        /// Show performance rating panel.
        /// </summary>
        /// <param name="rating">Performance rating.</param>
        /// <param name="label">Primary label text.</param>
        /// <param name="subLabel">Secondary label text.</param>
        internal static void PerformanceRatingPanel(PerformanceRating rating, string label, string subLabel = null)
        {
            using (var horizontal = new EditorGUILayout.HorizontalScope(GUI.skin.box))
            {
                using (var vertical = new EditorGUILayout.VerticalScope(GUILayout.MaxWidth(1)))
                {
                    vertical.rect.Set(vertical.rect.x, vertical.rect.y, 1, vertical.rect.height);
                    var tex = VRCSDKUtility.LoadPerformanceIcon(rating);
                    EditorGUILayout.LabelField(new GUIContent(tex, $"Quest {rating}"), GUILayout.Width(32), GUILayout.Height(32));
                }

                using (var vertical = new EditorGUILayout.VerticalScope())
                {
                    var style = EditorStyles.wordWrappedLabel;
                    EditorGUILayout.LabelField(label, style);
                    if (subLabel != null)
                    {
                        EditorGUILayout.LabelField(subLabel, style);
                    }
                }
            }
        }

        /// <summary>
        /// Show performance rating panel for calculated stats.
        /// </summary>
        /// <param name="stats">Calculated stats.</param>
        /// <param name="statsLevelSet">Stats level set.</param>
        /// <param name="category">Stats category.</param>
        /// <param name="i18n">I18n resource.</param>
        internal static void PerformanceRatingPanel(AvatarPerformanceStats stats, AvatarPerformanceStatsLevelSet statsLevelSet, AvatarPerformanceCategory category, I18nBase i18n)
        {
            var rating = stats.GetPerformanceRatingForCategory(category);
            string categoryDisplayName;
            try
            {
                categoryDisplayName = AvatarPerformanceStats.GetPerformanceCategoryDisplayName(category);
            }
            catch
            {
                categoryDisplayName = category.ToString();
            }
            string ratingDisplayName = AvatarPerformanceStats.GetPerformanceRatingDisplayName(rating);
            string label = $"{categoryDisplayName}: {ratingDisplayName}";
            string veryPoorViolation = null;

            switch (category)
            {
                case AvatarPerformanceCategory.PhysBoneComponentCount:
                    label = $"{categoryDisplayName}: {stats.physBone.Value.componentCount} ({i18n.Maximum}: {statsLevelSet.poor.physBone.componentCount})";
                    veryPoorViolation = i18n.PhysBonesWillBeRemovedAtRunTime;
                    break;
                case AvatarPerformanceCategory.PhysBoneTransformCount:
                    label = $"{categoryDisplayName}: {stats.physBone.Value.transformCount} ({i18n.Maximum}: {statsLevelSet.poor.physBone.transformCount})";
                    veryPoorViolation = i18n.PhysBonesTransformsShouldBeReduced;
                    break;
                case AvatarPerformanceCategory.PhysBoneColliderCount:
                    label = $"{categoryDisplayName}: {stats.physBone.Value.colliderCount} ({i18n.Maximum}: {statsLevelSet.poor.physBone.colliderCount})";
                    veryPoorViolation = i18n.PhysBoneCollidersWillBeRemovedAtRunTime;
                    break;
                case AvatarPerformanceCategory.PhysBoneCollisionCheckCount:
                    label = $"{categoryDisplayName}: {stats.physBone.Value.collisionCheckCount} ({i18n.Maximum}: {statsLevelSet.poor.physBone.collisionCheckCount})";
                    veryPoorViolation = i18n.PhysBonesCollisionCheckCountShouldBeReduced;
                    break;
                case AvatarPerformanceCategory.ContactCount:
                    label = $"{categoryDisplayName}: {stats.contactCount.Value} ({i18n.Maximum}: {statsLevelSet.poor.contactCount})";
                    veryPoorViolation = i18n.ContactsWillBeRemovedAtRunTime;
                    break;
                default:
                    throw new System.InvalidProgramException();
            }
            PerformanceRatingPanel(rating, label, rating >= PerformanceRating.VeryPoor ? veryPoorViolation : null);
        }

        /// <summary>
        /// Draw Editor GUI with help box style.
        /// </summary>
        /// <param name="type">Message type to show icon.</param>
        /// <param name="gui">GUI function to show as contents.</param>
        internal static void HelpBoxGUI(MessageType type, GUICallback gui)
        {
            using (var horizontal = new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                if (type != MessageType.None)
                {
                    using (var vertical = new EditorGUILayout.VerticalScope(GUILayout.MaxWidth(32)))
                    {
                        var icon = MessageTypeIconContent(type);
                        GUILayout.Label(icon, new GUIStyle()
                        {
                            padding = new RectOffset(0, 0, 0, 0),
                        });
                    }
                }

                using (var vertical = new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
                {
                    gui();
                }
            }
        }

        /// <summary>
        /// Draw inverted bool property field.
        /// </summary>
        /// <param name="boolProperty">Bool property.</param>
        /// <param name="label">Label to show.</param>
        /// <param name="options">Layout options.</param>
        internal static void InvertedBoolPropertyField(SerializedProperty boolProperty, GUIContent label, params GUILayoutOption[] options)
        {
            var rect = EditorGUILayout.GetControlRect(options);
            using (var prop = new EditorGUI.PropertyScope(rect, label, boolProperty))
            {
                boolProperty.boolValue = !EditorGUI.Toggle(rect, prop.content, !boolProperty.boolValue);
            }
        }

        /// <summary>
        /// List to select objects.
        /// </summary>
        /// <typeparam name="T">Component type.</typeparam>
        /// <param name="objects">All objects.</param>
        /// <param name="selectedObjects">Selected objects.</param>
        /// <returns>New array of selected objects.</returns>
        internal static T[] ObjectSelectorList<T>(T[] objects, T[] selectedObjects)
            where T : UnityEngine.Object
        {
            var afterSelected = new List<T>();
            foreach (var obj in objects)
            {
                using (var horizontal = new EditorGUILayout.HorizontalScope())
                {
                    var isSelected = EditorGUILayout.Toggle(selectedObjects.Contains(obj), GUILayout.Width(16));
                    GUILayout.Space(2);
                    EditorGUILayout.ObjectField(obj, typeof(T), true);
                    if (isSelected)
                    {
                        afterSelected.Add(obj);
                    }
                }
            }
            return afterSelected.ToArray();
        }

        /// <summary>
        /// Show horizontal divider.
        /// </summary>
        /// <param name="lineHeight">Line height.</param>
        internal static void HorizontalDivider(float lineHeight)
        {
            EditorGUILayout.Space();
            GUILayout.Box(string.Empty, GUILayout.ExpandWidth(true), GUILayout.Height(lineHeight));
            EditorGUILayout.Space();
        }

        /// <summary>
        /// Show boxed foldout.
        /// </summary>
        /// <param name="title">Title to show.</param>
        /// <param name="display">Whether to show.</param>
        /// <returns>Foldout result.</returns>
        internal static bool Foldout(string title, bool display)
        {
            var style = new GUIStyle("ShurikenModuleTitle");
            style.font = EditorStyles.label.font;
            style.fontSize = EditorStyles.label.fontSize;
            style.border = new RectOffset(15, 7, 4, 4);
            style.fixedHeight = 22;
            style.contentOffset = new Vector2(20f, -2f);

            var rect = GUILayoutUtility.GetRect(16f, 22f, style);
            GUI.Box(rect, title, style);

            var e = Event.current;

            var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
            }

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                display = !display;
                e.Use();
            }

            return display;
        }

        private static GUIContent MessageTypeIconContent(MessageType type)
        {
            switch (type)
            {
                case MessageType.None:
                    return new GUIContent();
                case MessageType.Info:
                    return UnityEditor.EditorGUIUtility.IconContent("console.infoicon");
                case MessageType.Warning:
                    return UnityEditor.EditorGUIUtility.IconContent("console.warnicon");
                case MessageType.Error:
                    return UnityEditor.EditorGUIUtility.IconContent("console.erroricon");
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// Disposable scope for <see cref="EditorGUILayout.BeginFoldoutHeaderGroup(bool, string)"/>.
        /// </summary>
        internal class FoldoutHeaderGroupScope : System.IDisposable
        {
            /// <summary>
            /// Gets foldout value.
            /// </summary>
            internal readonly bool Foldout;

            /// <summary>
            /// Initializes a new instance of the <see cref="FoldoutHeaderGroupScope"/> class.
            /// </summary>
            /// <param name="foldout">Is foldout opened.</param>
            /// <param name="content">Content to show.</param>
            internal FoldoutHeaderGroupScope(bool foldout, GUIContent content)
            {
                this.Foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, content);
            }

            /// <summary>
            /// Call <see cref="EditorGUILayout.EndFoldoutHeaderGroup"/> to end foldout header group.
            /// </summary>
            public void Dispose()
            {
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }
    }
}
