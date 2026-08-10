using System.Linq;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.I18n;
using KRT.VRCQuestTools.Models.VRChat;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Validators
{
    /// <summary>
    /// Validation rule for missing NDMF package.
    /// </summary>
    internal class MissingNdmfRule : IAvatarValidationRule
    {
        /// <summary>
        /// Gets the components which don't work without NDMF.
        /// <see cref="IManualConversionComponent"/> components are excluded because the manual conversion
        /// applies their settings for Mobile even when NDMF is not installed.
        /// </summary>
        /// <param name="avatarObject">Avatar root object.</param>
        /// <returns>Components which require NDMF.</returns>
        internal static Component[] GetComponentsRequiringNdmf(GameObject avatarObject)
        {
            return avatarObject.GetComponentsInChildren<INdmfComponent>(true)
                .Where(c => !(c is IManualConversionComponent))
                .OfType<Component>()
                .ToArray();
        }

        /// <inheritdoc/>
        public NotificationItem Validate(VRChatAvatar avatar)
        {
#if VQT_HAS_NDMF
            return null;
#else
            var components = GetComponentsRequiringNdmf(avatar.GameObject);
            if (components.Length == 0)
            {
                return null;
            }

            return new NotificationItem(() =>
            {
                if (avatar.AvatarDescriptor == null)
                {
                    return true;
                }

                var c = GetComponentsRequiringNdmf(avatar.GameObject);
                if (c.Length == 0)
                {
                    return true;
                }

                var i18n = VRCQuestToolsSettings.I18nResource;

                GUILayout.Label(i18n.ComponentRequiresNdmf, EditorStyles.wordWrappedLabel);
                GUILayout.Label($"{avatar.GameObject.name}", EditorStyles.wordWrappedLabel);

                using (var disabled = new EditorGUI.DisabledScope(true))
                {
                    foreach (var obj in c)
                    {
                        EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
                    }
                }

                if (GUILayout.Button(i18n.DismissLabel))
                {
                    return true;
                }

                return false;
            });
#endif
        }

        [InitializeOnLoadMethod]
        private static void InitOnLoad()
        {
            AvatarValidationRules.Add(new MissingNdmfRule());
        }
    }
}
