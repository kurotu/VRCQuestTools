using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using nadena.dev.ndmf;
using nadena.dev.ndmf.util;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

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
            if ((EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneWindows
                || EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneWindows64)
                && !VRCQuestToolsSettings.IsCheckTextureFormatOnStandaloneEnabled)
            {
                return;
            }

            var unsupportedTextures = new List<Texture2D>();
            var unknownTextures = new List<Texture2D>();

            var avatar = new VRChatAvatar(context.AvatarDescriptor);
            var materialTextures = avatar.Materials.SelectMany(m =>
            {
                var props = m.GetTexturePropertyNames();
                return props.Select(p => m.GetTexture(p));
            })
            .Where(t => t != null)
            .Where(t => t is Texture2D)
            .Cast<Texture2D>();

            var menuTextures = VRCSDKUtility.GetTexturesFromMenu(context.AvatarDescriptor.expressionsMenu);

            var allTextures = materialTextures.Concat(menuTextures).Distinct().ToArray();
            foreach (var texture in allTextures)
            {
                try
                {
                    if (!TextureUtility.IsSupportedTextureFormat(texture.format, EditorUserBuildSettings.activeBuildTarget))
                    {
                        if (TextureUtility.IsKnownTextureFormat(texture.format))
                        {
                            unsupportedTextures.Add(texture);
                        }
                        else
                        {
                            unknownTextures.Add(texture);
                        }
                    }
                }
                catch (System.NotSupportedException e)
                {
                    Debug.LogWarning($"[{VRCQuestTools.Name}] Texture format check is skipped: {e.Message}");
                    return;
                }
            }

            if (unsupportedTextures.Count > 0)
            {
                var formats = unsupportedTextures.Select(t => t.format).Distinct();
                foreach (var format in formats)
                {
                    var textures = unsupportedTextures.Where(t => t.format == format).OrderBy(t => t.name).ToArray();
                    NdmfErrorReport.ReportError(new UnsupportedTextureFormatError(format, EditorUserBuildSettings.activeBuildTarget, textures));
                }
            }

            if (unknownTextures.Count > 0)
            {
                var formats = unknownTextures.Select(t => t.format).Distinct();
                foreach (var format in formats)
                {
                    var textures = unknownTextures.Where(t => t.format == format).OrderBy(t => t.name).ToArray();
                    NdmfErrorReport.ReportError(new UnknownTextureFormatError(format, EditorUserBuildSettings.activeBuildTarget, textures));
                }
            }
        }
    }
}
