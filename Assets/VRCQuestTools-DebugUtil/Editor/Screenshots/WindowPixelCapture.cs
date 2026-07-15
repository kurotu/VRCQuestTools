using System.IO;
using io.github.hatayama.uLoopMCP;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Debug.Screenshots
{
    /// <summary>
    /// Saves a screenshot of an EditorWindow by rendering it directly into a RenderTexture via
    /// Unity's internal GrabPixels API (through uLoopMCP's <see cref="InternalEditorUtilityBridge"/>),
    /// instead of reading real OS screen pixels. This capture happens entirely inside Unity's own
    /// GPU rendering, so it does not depend on the window actually being visible/unobscured on the
    /// OS desktop, and keeps working even when the Editor's Remote Desktop session is disconnected.
    /// </summary>
    internal static class WindowPixelCapture
    {
        /// <summary>
        /// Renders <paramref name="window"/> into a texture and writes it as a PNG to <paramref name="path"/>.
        /// </summary>
        /// <param name="window">Window to capture.</param>
        /// <param name="path">Output PNG file path. Parent directories are created if needed.</param>
        internal static void SaveScreenshot(EditorWindow window, string path)
        {
            var scale = EditorGUIUtility.pixelsPerPoint;
            var width = Mathf.RoundToInt(window.position.width * scale);
            var height = Mathf.RoundToInt(window.position.height * scale);

            var descriptor = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32, 24);
            if (QualitySettings.activeColorSpace == ColorSpace.Linear)
            {
                // Avoid double gamma correction from BlitSceneViewCapture.mat in Linear color space.
                descriptor.sRGB = false;
            }

            var renderTexture = RenderTexture.GetTemporary(descriptor);
            InternalEditorUtilityBridge.CaptureEditorWindow(window, renderTexture);

            var previousActive = RenderTexture.active;
            RenderTexture.active = renderTexture;
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();
            RenderTexture.active = previousActive;
            RenderTexture.ReleaseTemporary(renderTexture);

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }
    }
}
