using System;
using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.I18n;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Services;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;
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
        /// <param name="componentFieldIndent">Left offset (px) of the component field, so it aligns under the group header label.</param>
        /// <returns>New selected components.</returns>
        internal static T[] AvatarDynamicsComponentSelectorList<T>(T[] objects, T[] selectedObjects, float componentFieldIndent = 0f)
            where T : IVRCAvatarDynamicsProvider
        {
            var afterSelected = new List<T>();
            IVRCAvatarDynamicsProvider hoveredProvider = null;

            foreach (var obj in objects)
            {
                var isSelected = ToggleAvatarDynamicsComponentField(selectedObjects.Select(o => o.Component).Contains(obj.Component), obj, componentFieldIndent);

                // Check for hover on the last drawn control
                var currentEvent = Event.current;
                if ((currentEvent.type == EventType.Repaint || currentEvent.type == EventType.MouseMove))
                {
                    var lastRect = GUILayoutUtility.GetLastRect();
                    if (lastRect.Contains(currentEvent.mousePosition))
                    {
                        hoveredProvider = obj;
                    }
                }

                if (isSelected)
                {
                    afterSelected.Add(obj);
                }
            }

            // Update preview component after all controls are processed
            if (hoveredProvider != null)
            {
                AvatarDynamicsPreviewService.SetPreviewComponent(hoveredProvider);
            }
            return afterSelected.ToArray();
        }

        /// <summary>
        /// Show a check list to select components, grouped by the nearest prefab instance root.
        /// Each group has its own foldout, indentation by prefab nesting depth, and select/deselect buttons.
        /// </summary>
        /// <typeparam name="T">Component provider type.</typeparam>
        /// <param name="objects">Components to list.</param>
        /// <param name="selectedObjects">Already selected components.</param>
        /// <param name="avatarRoot">Avatar root GameObject for relative path labels.</param>
        /// <param name="groupFoldouts">Foldout states keyed by group label. Modified in place.</param>
        /// <returns>New selected components, in the original order of <paramref name="objects"/>.</returns>
        internal static T[] GroupedAvatarDynamicsComponentSelectorList<T>(T[] objects, T[] selectedObjects, GameObject avatarRoot, Dictionary<string, bool> groupFoldouts)
            where T : IVRCAvatarDynamicsProvider
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            var selectedComponents = new HashSet<Component>(selectedObjects.Select(o => o.Component));
            var groups = objects
                .GroupBy(o => PrefabUtility.GetNearestPrefabInstanceRoot(o.GameObject))
                .OrderBy(g => (g.Key == null || g.Key == avatarRoot) ? 0 : 1);
            var baseIndent = EditorGUI.indentLevel;
            foreach (var group in groups)
            {
                var groupArray = group.ToArray();
                var isPrefab = group.Key != null && group.Key != avatarRoot;
                var iconTarget = isPrefab ? group.Key : avatarRoot;
                var label = iconTarget.name;
                var key = GetGroupFoldoutKey(avatarRoot, group.Key);
                var depth = GetGroupNestingDepth(avatarRoot.transform, group.Key != null ? group.Key.transform : null);
                if (!groupFoldouts.TryGetValue(key, out var groupOpen))
                {
                    groupOpen = true;
                }

                // Draw the header manually so the whole row toggles the foldout (like the Hierarchy) and
                // highlights on hover. EditorGUILayout.Foldout cannot render the icon reliably when indented,
                // so the arrow glyph, icon and label are drawn on explicit rects within the row.
                var lineHeight = UnityEditor.EditorGUIUtility.singleLineHeight;
                const float arrowWidth = 13f;
                const float arrowIconGap = 4f;
                const float iconLabelGap = 2f;
                var icon = GetHierarchyIcon(iconTarget);
                var selectContent = new GUIContent(i18n.SelectAllButtonLabel);
                var deselectContent = new GUIContent(i18n.DeselectAllButtonLabel);
                var selectWidth = EditorStyles.miniButton.CalcSize(selectContent).x;
                var deselectWidth = EditorStyles.miniButton.CalcSize(deselectContent).x;

                EditorGUI.indentLevel = 0;
                var rowRect = EditorGUILayout.GetControlRect(false, lineHeight);
                var indent = (baseIndent + depth) * 15f;

                // Left offset of the header label; item component fields align to this so they sit under the label.
                var labelOffset = indent + arrowWidth + arrowIconGap + lineHeight + iconLabelGap;
                var deselectRect = new Rect(rowRect.xMax - deselectWidth, rowRect.y, deselectWidth, lineHeight);
                var selectRect = new Rect(deselectRect.x - selectWidth, rowRect.y, selectWidth, lineHeight);
                var toggleRect = new Rect(rowRect.x, rowRect.y, selectRect.x - rowRect.x, lineHeight);
                var hovered = toggleRect.Contains(Event.current.mousePosition);

                if (Event.current.type == EventType.Repaint)
                {
                    if (hovered)
                    {
                        var hoverColor = UnityEditor.EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.08f) : new Color(0f, 0f, 0f, 0.06f);
                        EditorGUI.DrawRect(toggleRect, hoverColor);
                    }
                    var arrowRect = new Rect(rowRect.x + indent, rowRect.y, arrowWidth, lineHeight);
                    EditorStyles.foldout.Draw(arrowRect, false, false, groupOpen, false);
                    var iconRect = new Rect(arrowRect.xMax + arrowIconGap, rowRect.y, lineHeight, lineHeight);
                    if (icon != null)
                    {
                        GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                    }
                    var labelRect = new Rect(iconRect.xMax + iconLabelGap, rowRect.y, toggleRect.xMax - iconRect.xMax - iconLabelGap, lineHeight);
                    GUI.Label(labelRect, label, EditorStyles.boldLabel);
                }

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && toggleRect.Contains(Event.current.mousePosition))
                {
                    groupFoldouts[key] = !groupOpen;
                    Event.current.Use();
                }
                else
                {
                    groupFoldouts[key] = groupOpen;
                }

                if (GUI.Button(selectRect, selectContent, EditorStyles.miniButton))
                {
                    foreach (var o in groupArray)
                    {
                        selectedComponents.Add(o.Component);
                    }
                }
                if (GUI.Button(deselectRect, deselectContent, EditorStyles.miniButton))
                {
                    foreach (var o in groupArray)
                    {
                        selectedComponents.Remove(o.Component);
                    }
                }

                // Use the value captured at the start of this frame for item visibility so the
                // EditorGUILayout control count stays consistent between Layout and other events.
                if (groupOpen)
                {
                    // Align each item's component field under the group header label. The right (RootTransform)
                    // column stays fixed, so the component field width shrinks as the group is nested deeper.
                    var currentGroupSelected = groupArray.Where(o => selectedComponents.Contains(o.Component)).ToArray();
                    var newGroupSelected = AvatarDynamicsComponentSelectorList(groupArray, currentGroupSelected, labelOffset);
                    var newSet = new HashSet<Component>(newGroupSelected.Select(o => o.Component));
                    foreach (var o in groupArray)
                    {
                        if (newSet.Contains(o.Component))
                        {
                            selectedComponents.Add(o.Component);
                        }
                        else
                        {
                            selectedComponents.Remove(o.Component);
                        }
                    }
                }
                EditorGUI.indentLevel = baseIndent;
            }
            return objects.Where(o => selectedComponents.Contains(o.Component)).ToArray();
        }

        /// <summary>
        /// Show a toggle field for Avatar Dynamics component.
        /// </summary>
        /// <param name="value">Current state.</param>
        /// <param name="provider">Provider to show.</param>
        /// <param name="componentFieldIndent">Left offset (px) of the component field so it aligns under the group header label.</param>
        /// <returns>true for selected.</returns>
        internal static bool ToggleAvatarDynamicsComponentField(bool value, IVRCAvatarDynamicsProvider provider, float componentFieldIndent = 0f)
        {
            const float checkBoxWidth = 16f;
            const float gap = 2f;
            const float minComponentWidth = 80f;
            var lineHeight = UnityEditor.EditorGUIUtility.singleLineHeight;

            var prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            var rowRect = EditorGUILayout.GetControlRect(false, lineHeight);
            EditorGUI.indentLevel = prevIndent;

            // The right (RootTransform) column is fixed at the midpoint; the component field's left edge is
            // aligned under the group header label, so its width shrinks as the group is nested deeper.
            var half = rowRect.width * 0.5f;
            var componentX = Mathf.Clamp(componentFieldIndent, checkBoxWidth + gap, half - gap - minComponentWidth);
            var checkRect = new Rect(rowRect.x + componentX - gap - checkBoxWidth, rowRect.y, checkBoxWidth, lineHeight);
            var componentRect = new Rect(rowRect.x + componentX, rowRect.y, rowRect.x + half - gap - (rowRect.x + componentX), lineHeight);
            var rootRect = new Rect(rowRect.x + half + gap, rowRect.y, rowRect.xMax - (rowRect.x + half + gap), lineHeight);

            var selected = EditorGUI.Toggle(checkRect, value);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.ObjectField(componentRect, provider.Component, typeof(Component), true);
                EditorGUI.ObjectField(rootRect, VRCSDKUtility.GetRootTransform(provider.Component), typeof(Transform), true);
            }
            return selected;
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

            var currentValue = VRCSDKUtility.GetAvatarDynamicsCurrentPerformanceValue(stats, category);
            var poorRankLimit = VRCSDKUtility.GetAvatarDynamicsPoorRankLimit(statsLevelSet, category);
            label = $"{categoryDisplayName}: {currentValue} ({i18n.Maximum}: {poorRankLimit})";

            veryPoorViolation = VRCSDKUtility.GetAvatarDynamicsVeryPoorViolationMessage(category, i18n);
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

        // Unique foldout key per group. Uses the relative path so prefabs sharing a name keep independent foldout states.
        private static string GetGroupFoldoutKey(GameObject avatarRoot, GameObject prefabRoot)
        {
            if (prefabRoot == null || prefabRoot == avatarRoot)
            {
                return avatarRoot.name;
            }
            return GetRelativeTransformPath(avatarRoot.transform, prefabRoot.transform);
        }

        // Returns the icon Unity uses for the GameObject in the Hierarchy (plain GameObject, prefab, or variant).
        private static Texture GetHierarchyIcon(GameObject go)
        {
            var icon = AssetPreview.GetMiniThumbnail(go);
            if (icon == null)
            {
                icon = AssetPreview.GetMiniTypeThumbnail(typeof(GameObject));
            }
            return icon;
        }

        private static string GetRelativeTransformPath(Transform root, Transform target)
        {
            var parts = new List<string>();
            var t = target;
            while (t != null && t != root)
            {
                parts.Insert(0, t.name);
                t = t.parent;
            }
            return string.Join(" / ", parts);
        }

        // Returns how many intermediate prefab instance roots exist between avatarRoot and prefabRoot.
        // null or avatarRoot → 0; direct child prefab → 1; nested inside another prefab → 2; etc.
        private static int GetGroupNestingDepth(Transform avatarRoot, Transform prefabRoot)
        {
            if (prefabRoot == null || prefabRoot == avatarRoot)
            {
                return 0;
            }
            int depth = 0;
            var t = prefabRoot.parent;
            while (t != null && t != avatarRoot)
            {
                var root = PrefabUtility.GetNearestPrefabInstanceRoot(t.gameObject);
                if (root != null && root == t.gameObject)
                {
                    depth++;
                }
                t = t.parent;
            }
            return depth + 1;
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
