using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.Utils;
using nadena.dev.ndmf;
using nadena.dev.ndmf.util;
using UnityEditor;
using UnityEngine;
#if !VQT_HAS_NDMF_ERROR_REPORT
using KRT.VRCQuestTools.Ndmf.Dummy;
#endif

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Check referenced textures format.
    /// </summary>
    internal class CheckTextureFormatPass : Pass<CheckTextureFormatPass>
    {
        /// <inheritdoc />
        public override string DisplayName => "Check texture format";

        /// <inheritdoc />
        protected override void Execute(BuildContext context)
        {
            var unsupportedTextures = new List<Texture2D>();
            foreach (var asset in context.AvatarRootObject.ReferencedAssets())
            {
                if (asset is Texture2D texture)
                {
                    if (!AssetUtility.IsSupportedTextureFormat(texture.format, EditorUserBuildSettings.activeBuildTarget))
                    {
                        unsupportedTextures.Add(texture);
                    }
                }
            }

            if (unsupportedTextures.Count > 0)
            {
                var formats = unsupportedTextures.Select(t => t.format).Distinct();
                foreach (var format in formats)
                {
                    var textures = unsupportedTextures.Where(t => t.format == format).OrderBy(t => t.name).ToArray();
                    ErrorReport.ReportError(new UnsupportedTextureFormatError(format, EditorUserBuildSettings.activeBuildTarget, textures));
                }
            }
        }
    }
}
