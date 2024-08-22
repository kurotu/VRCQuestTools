using System.IO;
using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;
#if !VQT_HAS_NDMF_ERROR_REPORT
using KRT.VRCQuestTools.Ndmf.Dummy;
#endif

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Convert the avatar with AvatarConverterSettings component in NDMF.
    /// </summary>
    internal class AvatarConverterTransformingPass : Pass<AvatarConverterTransformingPass>
    {
        /// <inheritdoc/>
        public override string DisplayName => "Convert avatar for Android";

        /// <inheritdoc/>
        protected override void Execute(BuildContext context)
        {
            var settings = context.AvatarRootObject.GetComponent<AvatarConverterSettings>();
            if (settings == null)
            {
                return;
            }

            var buildTarget = NdmfHelper.ResolveBuildTarget(context.AvatarRootObject);
            if (buildTarget != Models.BuildTarget.Android)
            {
                return;
            }

            VRCQuestTools.AvatarConverter.ConvertForQuestInPlace(settings, VRCQuestTools.ComponentRemover, false, null, new Models.VRChat.AvatarConverter.ProgressCallback()
            {
                onTextureProgress = (_, __, e, original, converted) =>
                {
                    if (e != null)
                    {
                        Debug.LogError("Failed to convert material: " + original.name, original);
                        Debug.LogException(e, original);
                        var matRef = ObjectRegistry.GetReference(original);
                        var error = new MaterialConversionError(matRef, e);
                        ErrorReport.ReportError(error);
                        return;
                    }

                    // Register converted material to ObjectRegistry when it is not an asset..
                    if (converted != null && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(converted)))
                    {
                        ObjectRegistry.RegisterReplacedObject(original, converted);
                    }
                },
                onAnimationClipProgress = (_, __, e, original, converted) =>
                {
                    if (e != null)
                    {
                        Debug.LogError("Failed to convert animation clip: " + original.name, original);
                        Debug.LogException(e, original);
                        var animRef = ObjectRegistry.GetReference(original);
                        var error = new ObjectConversionError(animRef, e);
                        ErrorReport.ReportError(error);
                        return;
                    }
                    if (converted != null)
                    {
                        ObjectRegistry.RegisterReplacedObject(original, converted);
                    }
                },
                onRuntimeAnimatorProgress = (_, __, e, original, converted) =>
                {
                    if (e != null)
                    {
                        Debug.LogError("Failed to convert runtime animator controller: " + original.name, original);
                        Debug.LogException(e, original);
                        var animRef = ObjectRegistry.GetReference(original);
                        var error = new ObjectConversionError(animRef, e);
                        ErrorReport.ReportError(error);
                        return;
                    }
                    if (converted != null)
                    {
                        ObjectRegistry.RegisterReplacedObject(original, converted);
                    }
                },
            });
        }
    }
}
