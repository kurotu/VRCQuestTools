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
                    GUILayout.FlexibleSpace();
                    var tex = VRCSDKUtility.LoadPerformanceIcon(rating);
                    EditorGUILayout.LabelField(new GUIContent(tex, $"Quest {rating}"), GUILayout.Width(32), GUILayout.Height(32));
                    GUILayout.FlexibleSpace();
                }

                var style = EditorStyles.wordWrappedLabel;
                using (var vertical = new EditorGUILayout.VerticalScope())
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(label, style);
                    if (subLabel != null)
                    {
                        EditorGUILayout.LabelField(subLabel, style);
                    }
                    GUILayout.FlexibleSpace();
                }
            }
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
    }
}
