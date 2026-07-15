using System;
using System.IO;
using EditorDriver;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Debug.Screenshots
{
    /// <summary>
    /// Captures isolated inspector screenshots for VRCQuestTools components for documentation.
    /// </summary>
    internal static class ComponentScreenshotCapture
    {
        /// <summary>
        /// Captures an isolated inspector screenshot for a component of type <typeparamref name="TComponent"/>.
        /// Uses the matching component on the currently selected GameObject if present, so a developer can
        /// hand-curate example data beforehand; otherwise creates a temporary GameObject with a fresh component
        /// and seeds it via <paramref name="populate"/> so the screenshot shows plausible sample data instead
        /// of empty defaults. The screenshot is captured from the window's own internal rendering (see
        /// <see cref="WindowPixelCapture"/>).
        /// </summary>
        /// <typeparam name="TComponent">Component type to capture.</typeparam>
        /// <param name="fileName">Output PNG file name, saved under <see cref="ScreenshotSettings.OutputDirectory"/>.</param>
        /// <param name="size">Fixed window size.</param>
        /// <param name="populate">
        /// Optional callback to seed sample data on a freshly created component. Only invoked on the
        /// temp-GameObject fallback path, never when using a developer's own selected GameObject.
        /// </param>
        /// <param name="onDone">Optional callback invoked after the window is captured and closed.</param>
        internal static void Capture<TComponent>(string fileName, Vector2 size, Action<TComponent> populate = null, Action onDone = null)
            where TComponent : Component
        {
            var component = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponent<TComponent>()
                : null;

            GameObject tempGameObject = null;
            if (component == null)
            {
                tempGameObject = new GameObject($"VRCQuestTools_ScreenshotTemp_{typeof(TComponent).Name}");

                // HideInHierarchy | DontSave (not HideAndDontSave) keeps the GameObject editable.
                // HideAndDontSave also sets NotEditable, which some inspector code paths treat specially.
                tempGameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
                component = tempGameObject.AddComponent<TComponent>();
                populate?.Invoke(component);
            }

            var window = IsolatedInspectorWindow.Create(component);
            window.SetPosition(new Rect(ScreenshotSettings.CaptureOrigin, size));
            window.Focus();

            var tempGameObjectToDestroy = tempGameObject;
            var path = Path.Combine(ScreenshotSettings.OutputDirectory, fileName);
            CaptureScheduler.CaptureAfterRepaint(window, path, () =>
            {
                if (window != null)
                {
                    window.Close();
                }

                if (tempGameObjectToDestroy != null)
                {
                    UnityEngine.Object.DestroyImmediate(tempGameObjectToDestroy);
                }

                onDone?.Invoke();
            });
        }
    }
}
