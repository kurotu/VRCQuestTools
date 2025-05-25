using System.Linq;
using KRT.VRCQuestTools.Models;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;

namespace KRT.VRCQuestTools.Inspector
{
    /// <summary>
    /// PropertyDrawer for AvatarConverterNdmfPhase.
    /// </summary>
    [CustomPropertyDrawer(typeof(AvatarConverterNdmfPhase))]
    internal class AvatarConverterNdmfPhaseDrawer : PropertyDrawer
    {
        /// <inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var phaseIndex = (AvatarConverterNdmfPhase)property.enumValueIndex;
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                using (var ccs = new EditorGUI.ChangeCheckScope())
                {
                    var component = property.serializedObject.targetObject as Component;
                    var avatarDescriptor = GetComponentInParent<VRC_AvatarDescriptor>(component.gameObject);
                    var labels = GetPopupLabels(avatarDescriptor ? avatarDescriptor.gameObject : null);
                    var newIndex = EditorGUI.Popup(position, label, property.enumValueIndex, labels);
                    if (ccs.changed)
                    {
                        property.enumValueIndex = newIndex;
                    }
                }
            }
        }

        private GUIContent[] GetPopupLabels(GameObject avatarRoot)
        {
            return GetPhaseNames(avatarRoot).Select(label => new GUIContent(label)).ToArray();
        }

        private string[] GetPhaseNames(GameObject avatarRoot)
        {
            return System.Enum.GetValues(typeof(AvatarConverterNdmfPhase))
                .Cast<AvatarConverterNdmfPhase>()
                .Select(n =>
                {
                    if (n == AvatarConverterNdmfPhase.Auto)
                    {
                        return $"{n} ({n.Resolve(avatarRoot)})";
                    }
                    return n.ToString();
                })
                .ToArray();
        }

        private T GetComponentInParent<T>(GameObject gameObject)
            where T : Component
        {
#if UNITY_2022_1_OR_NEWER
            return gameObject.GetComponentInParent<T>(true);
#else
            var parent = gameObject.transform;
            while (parent != null)
            {
                var component = parent.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
                parent = parent.parent;
            }
            return null;
#endif
        }
    }
}
