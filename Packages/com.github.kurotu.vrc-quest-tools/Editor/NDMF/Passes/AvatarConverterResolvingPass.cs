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
            var targetSettings = context.AvatarRootObject.GetComponent<PlatformTargetSettings>();
            if (targetSettings == null)
            {
                return;
            }

            var converterSettings = context.AvatarRootObject.GetComponent<AvatarConverterSettings>();
            if (converterSettings == null)
            {
                return;
            }

            if (targetSettings.buildTarget != Models.BuildTarget.Android)
            {
                Object.DestroyImmediate(converterSettings);
                return;
            }

            VRCQuestTools.AvatarConverter.PrepareConvertForQuestInPlace(converterSettings);
        }
    }
}
