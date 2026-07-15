using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Debug.Screenshots
{
    /// <summary>
    /// Minimal EditorWindow that hosts a single component's default Editor in isolation,
    /// for capturing "just the component" documentation screenshots without Transform,
    /// other components, or the surrounding Inspector chrome.
    /// </summary>
    internal sealed class IsolatedInspectorWindow : EditorWindow
    {
        private Editor hostedEditor;
        private Vector2 scrollPosition;

        /// <summary>
        /// Creates and shows a fresh floating instance hosting <paramref name="target"/>'s default editor.
        /// </summary>
        /// <param name="target">Component to draw the inspector for.</param>
        /// <returns>The newly created window.</returns>
        internal static IsolatedInspectorWindow Create(Object target)
        {
            var window = CreateInstance<IsolatedInspectorWindow>();
            window.hostedEditor = Editor.CreateEditor(target);
            window.titleContent = new GUIContent(target.GetType().Name);
            window.ShowUtility();
            return window;
        }

        private void OnGUI()
        {
            if (hostedEditor == null || hostedEditor.target == null)
            {
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            hostedEditor.OnInspectorGUI();
            EditorGUILayout.EndScrollView();
        }

        private void OnDestroy()
        {
            if (hostedEditor != null)
            {
                DestroyImmediate(hostedEditor);
                hostedEditor = null;
            }
        }
    }
}
