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
            if (onSaved == null)
            {
                throw new ArgumentNullException(nameof(onSaved));
            }

            var framesLeft = FramesToWait;
            EditorApplication.CallbackFunction tick = null;
            tick = () =>
            {
                if (window == null)
                {
                    // The window was closed before the wait completed. Callers rely on onSaved to
                    // close windows, destroy temp objects, and restore editor settings (e.g.
                    // display language), so it must still run even though there is nothing to
                    // capture; otherwise those temp objects/settings leak and a "Capture All" chain
                    // waiting on this callback would stall forever.
                    EditorApplication.update -= tick;
                    onSaved();
                    return;
                }

                window.Repaint();
                framesLeft--;
                if (framesLeft > 0)
                {
                    return;
                }

                EditorApplication.update -= tick;
                try
                {
                    WindowPixelCapture.SaveScreenshot(window, path);
                    Logger.Log($"Saved screenshot: {path}");
                }
                finally
                {
                    // Guarantee onSaved runs even if the capture itself throws, for the same reason
                    // as the window == null case above.
                    onSaved();
                }
            };
            EditorApplication.update += tick;
        }
    }
}
