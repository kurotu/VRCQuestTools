using System.IO;
using UnityEngine;

namespace KRT.VRCQuestTools.Debug.Screenshots
{
    /// <summary>
    /// Shared configuration for the documentation screenshot capture tool.
    /// </summary>
    internal static class ScreenshotSettings
    {
        /// <summary>
        /// Fixed on-screen position used for every capture, so window framing is reproducible.
        /// </summary>
        internal static readonly Vector2 CaptureOrigin = new Vector2(100, 100);

        internal static readonly Vector2 AvatarConverterWindowSize = new Vector2(480, 210);
        internal static readonly Vector2 BlendShapesCopyWindowSize = new Vector2(480, 150);
        internal static readonly Vector2 MSMapGenWindowSize = new Vector2(420, 270);
        internal static readonly Vector2 UnityQuestSettingsWindowSize = new Vector2(480, 230);
        // PhysBonesRemoveWindow reserves a fixed-height scroll area for its PhysBone/Collider/Contact
        // lists independent of window size, so with no data it leaves a gap no matter the height;
        // shrinking further only clips content into a scrollbar instead of removing that gap.
        internal static readonly Vector2 PhysBonesRemoveWindowSize = new Vector2(480, 700);

        // Isolated component inspector sizes: sized per component since content height varies widely.
        internal static readonly Vector2 AvatarConverterSettingsSize = new Vector2(480, 480);
        internal static readonly Vector2 MaterialSwapSize = new Vector2(560, 260);
        internal static readonly Vector2 MaterialConversionSettingsSize = new Vector2(480, 580);
        internal static readonly Vector2 MenuIconResizerSize = new Vector2(480, 230);
        internal static readonly Vector2 MeshFlipperSize = new Vector2(480, 290);
        internal static readonly Vector2 PlatformComponentRemoverSize = new Vector2(480, 175);
        internal static readonly Vector2 PlatformGameObjectRemoverSize = new Vector2(480, 130);
        internal static readonly Vector2 PlatformTargetSettingsSize = new Vector2(480, 145);
        internal static readonly Vector2 VertexColorRemoverSize = new Vector2(480, 150);

        /// <summary>
        /// Gets the output directory for captured PNGs: Website/static/img, shared by both docs locales.
        /// </summary>
        internal static string OutputDirectory => Path.Combine(RepoRoot, "Website", "static", "img");

        private static string RepoRoot => Path.GetDirectoryName(Application.dataPath);
    }
}
