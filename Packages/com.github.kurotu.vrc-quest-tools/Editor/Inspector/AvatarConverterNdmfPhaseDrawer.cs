using System.Linq;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// PropertyDrawer for AvatarConverterNdmfPhase.
    /// </summary>
    [CustomPropertyDrawer(typeof(AvatarConverterNdmfPhase))]
    internal class AvatarConverterNdmfPhaseDrawer : PropertyDrawer
    {
        private readonly string[] phaseNames = System.Enum.GetValues(typeof(AvatarConverterNdmfPhase))
            .Cast<AvatarConverterNdmfPhase>()
            .Select(n =>
            {
                if (n == AvatarConverterNdmfPhase.Auto)
                {
                    return $"{n} ({n.Resolve()})";
                }
                return n.ToString();
            })
            .ToArray();

        /// <inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var phaseIndex = (AvatarConverterNdmfPhase)property.enumValueIndex;
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                using (var ccs = new EditorGUI.ChangeCheckScope())
                {
                    var labels = GetPopupLabels();
                    var newIndex = EditorGUI.Popup(position, label, property.enumValueIndex, labels);
                    if (ccs.changed)
                    {
                        property.enumValueIndex = newIndex;
                    }
                }
            }
        }

        private GUIContent[] GetPopupLabels()
        {
            return phaseNames.Select(label => new GUIContent(label)).ToArray();
        }
    }
}
