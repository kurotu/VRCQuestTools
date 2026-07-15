using System;
using UnityEditor;

namespace KRT.VRCQuestTools.Debug.Screenshots
{
    /// <summary>
    /// Helper to defer screenshot capture until an EditorWindow has run at least a couple of GUI
    /// frames, then captures and saves it via <see cref="WindowPixelCapture"/>. Because that capture
    /// reads Unity's own internal rendering (not real OS screen pixels), a short frame wait is only
    /// needed so the window's first OnGUI/layout pass has actually happened, not to wait for OS-level
    /// paint/composite.
    /// </summary>
    internal static class CaptureScheduler
    {
        private const int FramesToWait = 2;

        /// <summary>
        /// Waits a couple of Editor update ticks, repainting <paramref name="window"/> each tick,
        /// then captures and saves it to <paramref name="path"/>.
        /// </summary>
        /// <param name="window">Window to capture.</param>
        /// <param name="path">Output PNG path.</param>
        /// <param name="onSaved">Callback invoked once the PNG has been written.</param>
        internal static void CaptureAfterRepaint(EditorWindow window, string path, Action onSaved)
        {
            var framesLeft = FramesToWait;
            EditorApplication.CallbackFunction tick = null;
            tick = () =>
            {
                if (window == null)
                {
                    EditorApplication.update -= tick;
                    return;
                }

                window.Repaint();
                framesLeft--;
                if (framesLeft > 0)
                {
                    return;
                }

                EditorApplication.update -= tick;
                WindowPixelCapture.SaveScreenshot(window, path);
                Logger.Log($"Saved screenshot: {path}");
                onSaved();
            };
            EditorApplication.update += tick;
        }
    }
}
