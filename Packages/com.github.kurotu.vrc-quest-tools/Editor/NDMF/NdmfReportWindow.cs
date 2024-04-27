using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Report window for NDMF.
    /// </summary>
    internal class NdmfReportWindow : EditorWindow
    {
        private ReportItem[] items;
        private Vector2 scrollPosition;

        /// <summary>
        /// Add a report item.
        /// </summary>
        /// <param name="item">ReportItem to add.</param>
        internal static void AddReportItem(ReportItem item)
        {
            var window = GetWindow<NdmfReportWindow>();
            if (window.items == null)
            {
                window.items = new ReportItem[0];
            }
            window.items = window.items.Concat(new[] { item }).ToArray();
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
                    Views.EditorGUIUtility.HelpBoxGUI(item.type, () =>
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
