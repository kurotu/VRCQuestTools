using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.I18n;
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
        /// <summary>
        /// Delegate for GUI function.
        /// </summary>
        internal delegate void GUICallback();

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
        internal static void PerformanceRatingPanel(Models.VRChat.AvatarDynamics.PerformanceStats stats, AvatarPerformanceStatsLevelSet statsLevelSet, AvatarPerformanceCategory category, I18nBase i18n)
        {
            var rating = Models.VRChat.AvatarPerformanceCalculator.GetPerformanceRating(stats, statsLevelSet, category);
            string categoryName;
            string veryPoorViolation;
            int value;
            int maximum;
            switch (category)
            {
                case AvatarPerformanceCategory.PhysBoneComponentCount:
                    categoryName = "PhysBones Components";
                    veryPoorViolation = i18n.PhysBonesWillBeRemovedAtRunTime;
                    value = stats.PhysBonesCount;
                    maximum = statsLevelSet.poor.physBone.componentCount;
                    break;
                case AvatarPerformanceCategory.PhysBoneTransformCount:
                    categoryName = "PhysBones Affected Transforms";
                    veryPoorViolation = i18n.PhysBonesTransformsShouldBeReduced;
                    value = stats.PhysBonesTransformCount;
                    maximum = statsLevelSet.poor.physBone.transformCount;
                    break;
                case AvatarPerformanceCategory.PhysBoneColliderCount:
                    categoryName = "PhysBones Colliders";
                    veryPoorViolation = i18n.PhysBoneCollidersWillBeRemovedAtRunTime;
                    value = stats.PhysBonesColliderCount;
                    maximum = statsLevelSet.poor.physBone.colliderCount;
                    break;
                case AvatarPerformanceCategory.PhysBoneCollisionCheckCount:
                    categoryName = "PhysBones Collision Check Count";
                    veryPoorViolation = i18n.PhysBonesCollisionCheckCountShouldBeReduced;
                    value = stats.PhysBonesCollisionCheckCount;
                    maximum = statsLevelSet.poor.physBone.collisionCheckCount;
                    break;
                case AvatarPerformanceCategory.ContactCount:
                    categoryName = "Avatar Dynamics Contacts";
                    veryPoorViolation = i18n.ContactsWillBeRemovedAtRunTime;
                    value = stats.ContactsCount;
                    maximum = statsLevelSet.poor.contactCount;
                    break;
                default: throw new System.InvalidOperationException();
            }
            var label = $"{categoryName}: {value} ({i18n.Maximum}: {maximum})";
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
                using (var vertical = new EditorGUILayout.VerticalScope(GUILayout.MaxWidth(32)))
                {
                    GUILayout.FlexibleSpace();
                    var icon = MessageTypeIconContent(type);
                    GUILayout.Label(icon, new GUIStyle()
                    {
                        padding = new RectOffset(0, 0, 0, 0),
                    });
                    GUILayout.FlexibleSpace();
                }

                using (var vertical = new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
                {
                    GUILayout.FlexibleSpace();
                    gui();
                    GUILayout.FlexibleSpace();
                }
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

        private static GUIContent MessageTypeIconContent(MessageType type)
        {
            switch (type)
            {
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
            /// <param name="label">Label string to show.</param>
            internal FoldoutHeaderGroupScope(bool foldout, string label)
            {
                this.Foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, label);
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
