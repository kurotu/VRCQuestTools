using System;
using System.Collections.Generic;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// PropertyDrawer for ToonStandardConvertSetting.
    /// </summary>
    [CustomPropertyDrawer(typeof(ToonStandardConvertSettings))]
    internal class ToonStandardConvertSettingsDrawer : MaterialConvertSettingsDrawerBase
    {
        /// <summary>
        /// List of shadow ramps.
        /// </summary>
        private static Lazy<List<Texture2D>> shadowRamps = new Lazy<List<Texture2D>>(() =>
        {
            var shadowRamps = new List<Texture2D>()
            {
                 ToonStandardMaterialWrapper.RampTexture.Flat,
                 ToonStandardMaterialWrapper.RampTexture.Realistic,
                 ToonStandardMaterialWrapper.RampTexture.RealisticSoft,
                 ToonStandardMaterialWrapper.RampTexture.RealisticVerySoft,
                 ToonStandardMaterialWrapper.RampTexture.Toon2Band,
                 ToonStandardMaterialWrapper.RampTexture.Toon3Band,
                 ToonStandardMaterialWrapper.RampTexture.Toon4Band,
            };
            return shadowRamps;
        });

        /// <summary>
        /// Fallback shadow ramp types.
        /// </summary>
        private enum FallbackShadow
        {
            Flat,
            Realistic,
            RealisticSoft,
            RealisticVerySoft,
            Toon2Band,
            Toon3Band,
            Toon4Band,
            Custom,
        }

        /// <inheritdoc/>
        protected override Type MaterialConvertSettingsType => typeof(ToonStandardConvertSettings);

        /// <inheritdoc/>
        protected override Rect DrawPropertyFields(Rect fieldRect, SerializedProperty property)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;

            EditorGUI.indentLevel++;

            var generateQuestTextures = property.FindPropertyRelative("generateQuestTextures");
            EditorGUI.PropertyField(fieldRect, generateQuestTextures, new GUIContent(i18n.GenerateAndroidTexturesLabel, i18n.GenerateAndroidTexturesTooltip));
            fieldRect.y += EditorGUIUtility.singleLineHeight;
            fieldRect.y += EditorGUIUtility.standardVerticalSpacing;

            using (var disabled = new EditorGUI.DisabledScope(!generateQuestTextures.boolValue))
            {
                EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("maxTextureSize"), new GUIContent(i18n.IMaterialConvertSettingsTexturesSizeLimitLabel));
                fieldRect.y += EditorGUIUtility.singleLineHeight;
                fieldRect.y += EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.PropertyField(fieldRect, property.FindPropertyRelative("mobileTextureFormat"), new GUIContent(i18n.IMaterialConvertSettingsMobileTextureFormatLabel));
                fieldRect.y += EditorGUIUtility.singleLineHeight;
                fieldRect.y += EditorGUIUtility.standardVerticalSpacing;
            }

            var generateShadowRamp = property.FindPropertyRelative("generateShadowRamp");
            EditorGUI.PropertyField(fieldRect, generateShadowRamp, new GUIContent(i18n.ToonStandardConvertSettingsGenerateShadowRampLabel));
            fieldRect.y += EditorGUIUtility.singleLineHeight;
            fieldRect.y += EditorGUIUtility.standardVerticalSpacing;

            var fallbackShadowRamp = property.FindPropertyRelative("fallbackShadowRamp");
            using (var scope = new EditorGUI.PropertyScope(fieldRect, new GUIContent(i18n.ToonStandardConvertSettingsFallbackShadowRampLabel), fallbackShadowRamp))
            {
                var selectedRamp = GetSelectedFallbackShadow(fallbackShadowRamp.objectReferenceValue as Texture2D);

                using (var ccs = new EditorGUI.ChangeCheckScope())
                {
                    selectedRamp = (FallbackShadow)EditorGUI.EnumPopup(fieldRect, scope.content, selectedRamp);
                    if (ccs.changed)
                    {
                        fallbackShadowRamp.objectReferenceValue = selectedRamp == FallbackShadow.Custom
                            ? null
                            : shadowRamps.Value[(int)selectedRamp];
                    }
                }
                fieldRect.y += EditorGUIUtility.singleLineHeight;
                fieldRect.y += EditorGUIUtility.standardVerticalSpacing;

                if (selectedRamp == FallbackShadow.Custom)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.ObjectField(fieldRect, fallbackShadowRamp, typeof(Texture2D), new GUIContent(i18n.ToonStandardConvertSettingsCustomFallbackShadowRampLabel));
                    fieldRect.y += EditorGUIUtility.singleLineHeight;
                    fieldRect.y += EditorGUIUtility.standardVerticalSpacing;
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUI.LabelField(fieldRect, i18n.ToonStandardConvertSettingsFeaturesLabel);
            fieldRect.y += EditorGUIUtility.singleLineHeight;
            fieldRect.y += EditorGUIUtility.standardVerticalSpacing;
            using(new EditorGUI.IndentLevelScope())
            {
                var useNormalMap = property.FindPropertyRelative("useNormalMap");
                EditorGUI.PropertyField(fieldRect, useNormalMap, new GUIContent(i18n.ToonStandardConvertSettingsFeaturesNormalMapLabel));
                fieldRect.y += EditorGUIUtility.singleLineHeight;
                fieldRect.y += EditorGUIUtility.standardVerticalSpacing;

                var useEmission = property.FindPropertyRelative("useEmission");
                EditorGUI.PropertyField(fieldRect, useEmission, new GUIContent(i18n.ToonStandardConvertSettingsFeaturesEmissionLabel));
                fieldRect.y += EditorGUIUtility.singleLineHeight;
                fieldRect.y += EditorGUIUtility.standardVerticalSpacing;

                var useOcclusion = property.FindPropertyRelative("useOcclusion");
                EditorGUI.PropertyField(fieldRect, useOcclusion, new GUIContent(i18n.ToonStandardConvertSettingsFeaturesOcclusionLabel));
                fieldRect.y += EditorGUIUtility.singleLineHeight;
                fieldRect.y += EditorGUIUtility.standardVerticalSpacing;

                var useSpecular = property.FindPropertyRelative("useSpecular");
                EditorGUI.PropertyField(fieldRect, useSpecular, new GUIContent(i18n.ToonStandardConvertSettingsFeaturesSpecularLabel));
                fieldRect.y += EditorGUIUtility.singleLineHeight;
                fieldRect.y += EditorGUIUtility.standardVerticalSpacing;

                var useMatcap = property.FindPropertyRelative("useMatcap");
                EditorGUI.PropertyField(fieldRect, useMatcap, new GUIContent(i18n.ToonStandardConvertSettingsFeaturesMatcapLabel));
                fieldRect.y += EditorGUIUtility.singleLineHeight;
                fieldRect.y += EditorGUIUtility.standardVerticalSpacing;

                var useRim = property.FindPropertyRelative("useRimLighting");
                EditorGUI.PropertyField(fieldRect, useRim, new GUIContent(i18n.ToonStandardConvertSettingsFeaturesRimLightingLabel));
                fieldRect.y += EditorGUIUtility.singleLineHeight;
                fieldRect.y += EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.indentLevel--;

            return fieldRect;
        }

        /// <inheritdoc/>
        protected override float GetPropertyFieldsHeight(SerializedProperty property)
        {
            var height = 0.0f;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("generateQuestTextures"));
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("maxTextureSize"));
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("mobileTextureFormat"));
            height += EditorGUIUtility.standardVerticalSpacing;

            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("generateShadowRamp"));
            height += EditorGUIUtility.standardVerticalSpacing;
            var fallbackShadowRamp = property.FindPropertyRelative("fallbackShadowRamp");
            height += EditorGUIUtility.singleLineHeight;
            height += EditorGUIUtility.standardVerticalSpacing;
            if (GetSelectedFallbackShadow(fallbackShadowRamp.objectReferenceValue as Texture2D) == FallbackShadow.Custom)
            {
                height += EditorGUI.GetPropertyHeight(fallbackShadowRamp);
                height += EditorGUIUtility.standardVerticalSpacing;
            }

            height += EditorGUIUtility.singleLineHeight; // Features label
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("useNormalMap"));
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("useEmission"));
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("useOcclusion"));
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("useSpecular"));
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("useMatcap"));
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("useRimLighting"));
            height += EditorGUIUtility.standardVerticalSpacing;

            return height;
        }

        private FallbackShadow GetSelectedFallbackShadow(Texture2D texture)
        {
            var index = shadowRamps.Value.IndexOf(texture);
            if (index >= 0)
            {
                return (FallbackShadow)Enum.GetValues(typeof(FallbackShadow)).GetValue(index);
            }
            return FallbackShadow.Custom;
        }
    }
}
