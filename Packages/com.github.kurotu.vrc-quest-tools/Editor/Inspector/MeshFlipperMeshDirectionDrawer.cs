using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// Custom property drawer for MeshFlipperMeshDirection.
    /// </summary>
    [CustomPropertyDrawer(typeof(MeshFlipperMeshDirection))]
    internal class MeshFlipperMeshDirectionDrawer : PropertyDrawer
    {
        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                var selectedIndex = 0;
                using (var css = new EditorGUI.ChangeCheckScope())
                {
                    selectedIndex = EditorGUI.Popup(position, label.text, property.enumValueIndex, GetMeshFlipperMeshDirectionLabels());
                    if (css.changed)
                    {
                        property.enumValueIndex = selectedIndex;
                    }
                }
            }
        }

        private static string[] GetMeshFlipperMeshDirectionLabels()
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            return new string[]
            {
                i18n.MeshFlipperMeshDirectionFlip,
                i18n.MeshFlipperMeshDirectionDoubleSide,
            };
        }
    }
}
