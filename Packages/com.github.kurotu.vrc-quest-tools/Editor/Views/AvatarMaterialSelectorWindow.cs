using System;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Views
{
    /// <summary>
    /// EditorWindow for selecting material.
    /// </summary>
    internal class AvatarMaterialSelectorWindow : EditorWindow
    {
        private Material selectedMaterial;
        private string materialPath;
        private Material[] materials;
        private Action<Material> onMaterialSelected;

        private Vector2 scrollPosition;

        /// <summary>
        /// Show AvatarMaterialSelectorWindow as AuxWindow.
        /// </summary>
        /// <param name="material">Current selected material.</param>
        /// <param name="materials">Materials to show.</param>
        /// <param name="onMaterialSelected">Callback on selection.</param>
        /// <returns>Opened window.</returns>
        public static AvatarMaterialSelectorWindow Open(Material material, Material[] materials, Action<Material> onMaterialSelected)
        {
            var window = CreateInstance<AvatarMaterialSelectorWindow>();
            window.selectedMaterial = material;
            window.materialPath = AssetDatabase.GetAssetPath(material);
            window.materials = materials;
            window.onMaterialSelected = onMaterialSelected;
            window.titleContent = new GUIContent("Select Material");
            window.ShowAuxWindow();
            return window;
        }

        private void OnGUI()
        {
            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scrollView.scrollPosition;

                foreach (var material in materials)
                {
                    using (var ccs = new EditorGUI.ChangeCheckScope())
                    {
                        var isSelected = MaterialItem(material, position.width, material == selectedMaterial);
                        if (ccs.changed && isSelected)
                        {
                            selectedMaterial = material;
                            materialPath = AssetDatabase.GetAssetPath(material);
                            onMaterialSelected?.Invoke(material);
                        }
                    }
                }
            }

            if (selectedMaterial != null)
            {
                EditorGUILayout.LabelField($"{selectedMaterial.name} (Material)    {materialPath}");
            }
        }

        private bool MaterialItem(Material material, float width, bool isSelected)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var selected = EditorGUILayout.ToggleLeft(string.Empty, isSelected, GUILayout.Width(16));

                var thumbnail = AssetPreview.GetAssetPreview(material);
                EditorGUILayout.LabelField(new GUIContent(thumbnail), GUILayout.Width(48), GUILayout.Height(48));

                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField(material.name);

                    var path = AssetDatabase.GetAssetPath(material);
                    EditorGUILayout.LabelField(path, EditorStyles.miniLabel);
                }

                return selected;
            }
        }
    }
}
