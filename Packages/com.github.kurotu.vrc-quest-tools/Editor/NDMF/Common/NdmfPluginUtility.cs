using System.Runtime.ExceptionServices;
using KRT.VRCQuestTools.Models;
using nadena.dev.ndmf;
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
        /// Handle conversion exception.
        /// </summary>
        /// <param name="exception">Exception thrown in a pass.</param>
        internal static void HandleConversionException(System.Exception exception)
        {
            SimpleError ndmfError;
            bool shouldRethrow = false;
            if (exception is IVRCQuestToolsException vqte)
            {
                switch (vqte)
                {
                    case MaterialConversionException e:
                        {
                            var matRef = NdmfObjectRegistry.GetReference(e.source);
                            ndmfError = new MaterialConversionError(matRef, e);
                        }
                        break;
                    case AnimationClipConversionException e:
                        {
                            var animRef = NdmfObjectRegistry.GetReference(e.source);
                            ndmfError = new ObjectConversionError(animRef, e);
                        }
                        break;
                    case AnimatorControllerConversionException e:
                        {
                            var animRef = NdmfObjectRegistry.GetReference(e.source);
                            ndmfError = new ObjectConversionError(animRef, e);
                        }
                        break;
                    case InvalidMaterialSwapNullException e:
                        ndmfError = new MaterialSwapNullError(e.component, e.MaterialMapping);
                        break;
                    case InvalidReplacementMaterialException e:
                        ndmfError = new ReplacementMaterialError(e.component, e.replacementMaterial);
                        break;
                    case TargetMaterialNullException e:
                        ndmfError = new TargetMaterialNullError(e.component);
                        break;
                    default:
                        ndmfError = new SimpleStringError(
                            $"Unhandled {exception.GetType().Name}",
                            exception.Message,
                            "Report to the developer to show detailed error report.",
                            ErrorSeverity.NonFatal);
                        shouldRethrow = true;
                        Debug.LogError($"Unhandled exception type: {exception.GetType()}");
                        break;
                }
            }
            else
            {
                ndmfError = null;
                shouldRethrow = true;
            }
            if (ndmfError != null)
            {
                ErrorReport.ReportError(ndmfError);
            }

            if (exception.InnerException != null)
            {
                Debug.LogException(exception.InnerException);
            }
            if (shouldRethrow)
            {
                ExceptionDispatchInfo.Capture(exception).Throw();
            }
        }

        private static void SetBuildTarget(Models.BuildTarget target)
        {
            NdmfSessionState.BuildTarget = target;
        }
    }
}
