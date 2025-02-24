using UnityEngine;

namespace KRT.VRCQuestTools.Components
{
    /// <summary>
    /// Resize menu icons in expressions menu.
    /// </summary>
    [AddComponentMenu("VRCQuestTools/VQT Menu Icons Resizer")]
    [HelpURL("https://kurotu.github.io/VRCQuestTools/docs/references/components/menu-icons-resizer?lang=auto")]
    [DisallowMultipleComponent]
    public class MenuIconsResizer : VRCQuestToolsEditorOnly, IPlatformDependentComponent, INdmfComponent
    {
        /// <summary>
        /// Maximum texture size for PC target.
        /// </summary>
        public TextureResizeMode resizeModePC = TextureResizeMode.DoNotResize;

        /// <summary>
        /// Maximum texture size for Android target.
        /// </summary>
        public TextureResizeMode resizeModeAndroid = TextureResizeMode.DoNotResize;

        /// <summary>
        /// Compress existing textures if needed.
        /// </summary>
        public bool compressTextures = true;

        /// <summary>
        /// Texture size limit for quest.
        /// 256px is the maximum size for VRChat expressions menu icons.
        /// </summary>
        public enum TextureResizeMode
        {
            /// <summary>
            /// Keep textures.
            /// </summary>
            DoNotResize = -1,

            /// <summary>
            /// Remove textures..
            /// </summary>
            Remove = 0,

            /// <summary>
            /// Max 64x64.
            /// </summary>
            Max64x64 = 64,

            /// <summary>
            /// Max 128x128..
            /// </summary>
            Max128x128 = 128,
        }
    }
}
