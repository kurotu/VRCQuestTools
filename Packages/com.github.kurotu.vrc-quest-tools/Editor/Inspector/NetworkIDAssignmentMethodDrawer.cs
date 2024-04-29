using System.Linq;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// PrpertyDrawer for NetworkIDAssignmentMethod.
    /// </summary>
    [CustomPropertyDrawer(typeof(NetworkIDAssignmentMethod))]
    internal class NetworkIDAssignmentMethodDrawer : PropertyDrawer
    {
        /// <inheritdoc />
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            var names = System.Enum.GetValues(typeof(NetworkIDAssignmentMethod))
                .Cast<NetworkIDAssignmentMethod>()
                .Select(s =>
                {
                    var l = s.ToString();
                    var tooltip = string.Empty;
                    switch (s)
                    {
                        case NetworkIDAssignmentMethod.HierarchyHash:
                            l = i18n.NetworkIDAssignerEditorAssignmentMethodHierachyHashLabel;
                            tooltip = i18n.NetworkIDAssignerEditorAssignmentMethodHierachyHashTooltip;
                            break;
                        case NetworkIDAssignmentMethod.VRChatSDK:
                            l = i18n.NetworkIDAssignerEditorAssignmentMethodVRChatSDKLabel;
                            tooltip = i18n.NetworkIDAssignerEditorAssignmentMethodVRChatSDKTooltip;
                            break;
                    }
                    return new GUIContent(l, tooltip);
                })
                .ToArray();
            property.enumValueIndex = EditorGUI.Popup(position, label, property.enumValueIndex, names);
        }
    }
}
