using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Validation.Performance;

namespace KRT.VRCQuestTools.Views
{
    /// <summary>
    /// Utility class for Editor GUI.
    /// </summary>
    internal static class EditorGUIUtility
    {
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
                var tex = VRCSDKUtility.LoadPerformanceIcon(rating);
                EditorGUILayout.LabelField(new GUIContent(tex, $"Quest {rating}"), GUILayout.Width(32), GUILayout.Height(32));

                var style = EditorStyles.wordWrappedLabel;
                using (var vertical = new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField(label, style);
                    if (subLabel != null)
                    {
                        EditorGUILayout.LabelField(subLabel, style);
                    }
                }
            }
        }
    }
}
