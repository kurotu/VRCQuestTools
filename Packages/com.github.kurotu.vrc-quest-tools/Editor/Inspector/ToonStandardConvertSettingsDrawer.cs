using System;
using System.Collections.Generic;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
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
            EditorGUI.PropertyField(fieldRect, generateQuestTextures, new GUIContent(i18n.GenerateMobileTexturesLabel, i18n.GenerateMobileTexturesTooltip));
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

            // Row 1: Features label + featureMode dropdown
            var featureModeProperty = property.FindPropertyRelative("featureMode");
            var modeOptions = new GUIContent[]
            {
                new GUIContent(i18n.ToonStandardConvertSettingsFeaturesModeOptIn),
                new GUIContent(i18n.ToonStandardConvertSettingsFeaturesModeOptOut),
            };
            using (var ccs = new EditorGUI.ChangeCheckScope())
            {
                var newModeIndex = EditorGUI.Popup(fieldRect, new GUIContent(i18n.ToonStandardConvertSettingsFeaturesLabel), featureModeProperty.intValue, modeOptions);
                if (ccs.changed)
                {
                    featureModeProperty.intValue = newModeIndex;
                    var newMode = (ToonStandardFeaturesMode)newModeIndex;
                    SetAllFeaturesSerializedProperty(property, newMode == ToonStandardFeaturesMode.OptOut);
                    property.serializedObject.ApplyModifiedProperties();
                    NdmfUtility.NotifyObjectUpdate(property.serializedObject.targetObject);
                }
            }

            fieldRect.y += EditorGUIUtility.singleLineHeight;
            fieldRect.y += EditorGUIUtility.standardVerticalSpacing;

            // Row 2: Select All / Deselect All buttons
            var buttonRect = fieldRect;
            buttonRect.x = fieldRect.x + EditorGUIUtility.labelWidth;
            buttonRect.width = (fieldRect.width - EditorGUIUtility.labelWidth) / 2;
            if (GUI.Button(buttonRect, new GUIContent(i18n.SelectAllButtonLabel)))
            {
                SetAllFeaturesSerializedProperty(property, true);
                property.serializedObject.ApplyModifiedProperties();
                NdmfUtility.NotifyObjectUpdate(property.serializedObject.targetObject);
            }

            buttonRect.x += buttonRect.width;
            if (GUI.Button(buttonRect, new GUIContent(i18n.DeselectAllButtonLabel)))
            {
                SetAllFeaturesSerializedProperty(property, false);
                property.serializedObject.ApplyModifiedProperties();
                NdmfUtility.NotifyObjectUpdate(property.serializedObject.targetObject);
            }

            fieldRect.y += EditorGUIUtility.singleLineHeight;
            fieldRect.y += EditorGUIUtility.standardVerticalSpacing;

            // Feature checkboxes driven by FeaturePropertyNames
            using (new EditorGUI.IndentLevelScope())
            {
                foreach (var propName in ToonStandardConvertSettings.FeaturePropertyNames)
                {
                    var featureProp = property.FindPropertyRelative(propName);
                    var labelKey = GetFeatureLabelKey(propName);
                    var label = i18n.GetText(labelKey);
                    EditorGUI.PropertyField(fieldRect, featureProp, new GUIContent(label));
                    fieldRect.y += EditorGUIUtility.singleLineHeight;
                    fieldRect.y += EditorGUIUtility.standardVerticalSpacing;
                }
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

            height += EditorGUIUtility.singleLineHeight; // Features label + featureMode dropdown
            height += EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUIUtility.singleLineHeight; // Select All / Deselect All buttons
            height += EditorGUIUtility.standardVerticalSpacing;
            foreach (var propName in ToonStandardConvertSettings.FeaturePropertyNames)
            {
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative(propName));
                height += EditorGUIUtility.standardVerticalSpacing;
            }
            height += EditorGUIUtility.standardVerticalSpacing;

            return height;
        }

        private static string GetFeatureLabelKey(string fieldName)
        {
            // "useNormalMap" -> "ToonStandardConvertSettingsFeaturesNormalMapLabel"
            var featureName = fieldName.Substring("use".Length);
            return "ToonStandardConvertSettingsFeatures" + featureName + "Label";
        }

        private static void SetAllFeaturesSerializedProperty(SerializedProperty property, bool value)
        {
            foreach (var propName in ToonStandardConvertSettings.FeaturePropertyNames)
            {
                property.FindPropertyRelative(propName).boolValue = value;
            }
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
