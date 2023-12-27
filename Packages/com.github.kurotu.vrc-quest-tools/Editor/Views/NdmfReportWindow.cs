#if !VQT_HAS_NDMF_ERROR_REPORT
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Views
{
    /// <summary>
    /// Report window for NDMF.
    /// </summary>
    internal class NdmfReportWindow : EditorWindow
    {
        private ReportItem[] items;
        private Vector2 scrollPosition;

        /// <summary>
        /// Show window.
        /// </summary>
        /// <param name="items">ReportItem array to show.</param>
        internal static void ShowWindow(ReportItem[] items)
        {
            var window = GetWindow<NdmfReportWindow>();
            window.items = items;
            window.Show();
        }

        /// <summary>
        /// Clear report items.
        /// </summary>
        internal static void Clear()
        {
            var window = GetWindow<NdmfReportWindow>();
            window.items = new ReportItem[0];
        }

        private void OnEnable()
        {
            titleContent.text = "VQT NDMF Report";
        }

        private void OnGUI()
        {
            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scrollView.scrollPosition;
                if (items == null || items.Length == 0)
                {
                    EditorGUILayout.LabelField("No reports found.");
                    return;
                }
                foreach (var item in items)
                {
                    EditorGUIUtility.HelpBoxGUI(item.type, () =>
                    {
                        EditorGUILayout.LabelField(item.message, EditorStyles.wordWrappedLabel);
                        if (item.gameObject != null)
                        {
                            using (var disabled = new EditorGUI.DisabledGroupScope(true))
                            {
                                EditorGUILayout.ObjectField(item.gameObject, typeof(GameObject), true);
                            }
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Report item to show.
        /// </summary>
        internal class ReportItem
        {
            /// <summary>
            /// Message type.
            /// </summary>
            internal MessageType type;

            /// <summary>
            /// Message to show.
            /// </summary>
            internal string message;

            /// <summary>
            /// Related GameObject.
            /// </summary>
            internal GameObject gameObject;
        }
    }
}
#endif
