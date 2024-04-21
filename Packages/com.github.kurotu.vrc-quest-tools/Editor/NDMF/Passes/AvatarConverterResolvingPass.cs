using KRT.VRCQuestTools.Components;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Ndmf
{
    /// <summary>
    /// Prepare the avatar with AvatarConverterSettings component in NDMF.
    /// </summary>
    internal class AvatarConverterResolvingPass : Pass<AvatarConverterResolvingPass>
    {
        /// <inheritdoc/>
        public override string DisplayName => "Prepare converter and build target";

        /// <inheritdoc/>
        protected override void Execute(BuildContext context)
        {
            var converterSettings = context.AvatarRootObject.GetComponent<AvatarConverterSettings>();
            if (converterSettings == null)
            {
                return;
            }

            var buildTarget = NdmfHelper.ResolveBuildTarget(context.AvatarRootObject);
            switch (buildTarget)
            {
                case Models.BuildTarget.PC:
                    Object.DestroyImmediate(converterSettings);
                    break;
                case Models.BuildTarget.Android:
                    VRCQuestTools.AvatarConverter.PrepareConvertForQuestInPlace(converterSettings);
                    break;
                default:
                    throw new System.InvalidProgramException($"Unsupported build target: {buildTarget}");
            }
        }
    }
}
