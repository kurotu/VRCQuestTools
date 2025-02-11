using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;
#if !VQT_HAS_NDMF_ERROR_REPORT
using KRT.VRCQuestTools.Ndmf.Dummy;
#endif

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Utility for NDMF plugin.
    /// </summary>
    internal static class NdmfPluginUtility
    {
        /// <summary>
        /// Manual bake with Android buold target settings.
        /// </summary>
        /// <param name="avatar">Target avatar.</param>
        /// <returns>Baked avatar.</returns>
        internal static GameObject ManualBakeWithAndroidSettings(GameObject avatar)
        {
            SetBuildTarget(Models.BuildTarget.Android);
            try
            {
                return AvatarProcessor.ProcessAvatarUI(avatar);
            }
            finally
            {
                SetBuildTarget(Models.BuildTarget.Auto);
            }
        }

        /// <summary>
        /// Convert the avatar with AvatarConverterSettings component in NDMF.
        /// </summary>
        /// <param name="context">BuildContext.</param>
        /// <param name="settings">settings component.</param>
        internal static void ConvertAvatarInPass(BuildContext context, AvatarConverterSettings settings)
        {
            var buildTarget = NdmfHelper.ResolveBuildTarget(context.AvatarRootObject);
            if (buildTarget != Models.BuildTarget.Android)
            {
                return;
            }

            try
            {
                RegisterMaterialSwapsToObjectRegistry(context.AvatarRootObject);

                VRCQuestTools.AvatarConverter.ConvertForQuestInPlace(settings, VRCQuestTools.ComponentRemover, false, null, new Models.VRChat.AvatarConverter.ProgressCallback()
                {
                    onTextureProgress = (_, __, original, converted) =>
                    {
                        // Register converted material to ObjectRegistry when it is not an asset..
                        if (converted != null && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(converted)))
                        {
                            ObjectRegistry.RegisterReplacedObject(original, converted);
                        }
                    },
                    onAnimationClipProgress = (_, __, original, converted) =>
                    {
                        if (converted != null)
                        {
                            ObjectRegistry.RegisterReplacedObject(original, converted);
                        }
                    },
                    onRuntimeAnimatorProgress = (_, __, original, converted) =>
                    {
                        if (converted != null)
                        {
                            ObjectRegistry.RegisterReplacedObject(original, converted);
                        }
                    },
                });
            }
            catch (System.Exception exception)
            {
                HandleConversionException(exception);
            }
        }

        private static void SetBuildTarget(Models.BuildTarget target)
        {
            NdmfSessionState.BuildTarget = target;
        }

        private static void RegisterMaterialSwapsToObjectRegistry(GameObject avatarRoot)
        {
            var swaps = avatarRoot.GetComponentsInChildren<MaterialSwap>();
            foreach (var swap in swaps)
            {
                foreach (var mapping in swap.materialMappings)
                {
                    if (mapping.originalMaterial != null && mapping.replacementMaterial != null)
                    {
                        ObjectRegistry.RegisterReplacedObject(mapping.originalMaterial, mapping.replacementMaterial);
                    }
                }
            }
        }

        private static void HandleConversionException(System.Exception exception)
        {
            var message = exception.Message;
            var dialogException = exception;
            switch (exception)
            {
                case MaterialConversionException e:
                    {
                        var matRef = ObjectRegistry.GetReference(e.source);
                        var error = new MaterialConversionError(matRef, e);
                        ErrorReport.ReportError(error);
                    }
                    break;
                case AnimationClipConversionException e:
                    {
                        var animRef = ObjectRegistry.GetReference(e.source);
                        var error = new ObjectConversionError(animRef, e);
                    }
                    break;
                case AnimatorControllerConversionException e:
                    {
                        var animRef = ObjectRegistry.GetReference(e.source);
                        var error = new ObjectConversionError(animRef, e);
                    }
                    break;
            }
            if (exception.InnerException != null)
            {
                Debug.LogException(exception.InnerException);
            }
            Debug.LogException(exception);
        }
    }
}
