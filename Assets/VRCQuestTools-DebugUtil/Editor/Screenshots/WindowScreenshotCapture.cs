using System;
using System.IO;
using EditorDriver;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Debug.Screenshots
{
    /// <summary>
    /// Captures fixed-size screenshots of VRCQuestTools EditorWindows for documentation.
    /// </summary>
    internal static class WindowScreenshotCapture
    {
        /// <summary>
        /// Opens a fresh floating instance of <typeparamref name="TWindow"/>, seeds its state via
        /// <paramref name="configure"/>, forces it to a fixed size/position, and saves a screenshot
        /// PNG captured from the window's own internal rendering (see <see cref="WindowPixelCapture"/>).
        /// </summary>
        /// <typeparam name="TWindow">Window type to capture.</typeparam>
        /// <param name="fileName">Output PNG file name, saved under <see cref="ScreenshotSettings.OutputDirectory"/>.</param>
        /// <param name="size">Fixed window size.</param>
        /// <param name="configure">Optional callback to seed window state via reflection before capture.</param>
        /// <param name="onDone">Optional callback invoked after the window is captured and closed.</param>
        internal static void Capture<TWindow>(string fileName, Vector2 size, Action<WindowHandle> configure = null, Action onDone = null)
            where TWindow : EditorWindow
        {
            var driver = new Driver();
            var handle = driver.OpenWindow<TWindow>();
            configure?.Invoke(handle);

            var window = handle.Window;
            window.SetPosition(new Rect(ScreenshotSettings.CaptureOrigin, size));
            window.Focus();

            var path = Path.Combine(ScreenshotSettings.OutputDirectory, fileName);
            CaptureScheduler.CaptureAfterRepaint(window, path, () =>
            {
                driver.Dispose();
                onDone?.Invoke();
            });
        }
    }
}
