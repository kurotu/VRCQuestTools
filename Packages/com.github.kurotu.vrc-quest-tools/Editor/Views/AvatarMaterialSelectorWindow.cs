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

                var itemWidth = 64;
                var cols = (int)(position.width / itemWidth);
                var blankItems = cols - materials.Length % cols;
                var allItemsCount = materials.Length + blankItems;

                using (var vertical = new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();

                    var width = 64;
                    var margin = UnityEditor.EditorGUIUtility.standardVerticalSpacing;
                    float currentWidth = 0;
                    for (int i = 0; i < allItemsCount; i++)
                    {
                        if (i < materials.Length)
                        {
                            var material = materials[i];
                            var isSelected = selectedMaterial == material;
                            if (MaterialItem(material, width, isSelected) && !isSelected)
                            {
                                selectedMaterial = material;
                                materialPath = AssetDatabase.GetAssetPath(material);
                                onMaterialSelected?.Invoke(selectedMaterial);
                            }
                        }

                        currentWidth += width + margin * 2;
                        if (currentWidth + width + margin * 2 > position.width - 16)
                        {
                            currentWidth = 0;
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.Space();
                            EditorGUILayout.BeginHorizontal();
                        }
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.LabelField($"{selectedMaterial.name} (Material)    {materialPath}");
        }

        private bool MaterialItem(Material material, float width, bool isSelected)
        {
            using (var vertical = new EditorGUILayout.VerticalScope(GUILayout.Width(width)))
            {
                var thumbnail = AssetPreview.GetAssetPreview(material);
                using (new EditorGUILayout.HorizontalScope(GUILayout.Width(width)))
                {
                    EditorGUILayout.LabelField(new GUIContent(thumbnail), GUILayout.Width(54), GUILayout.Height(54));
                }
                EditorGUILayout.LabelField(material.name, EditorStyles.miniLabel, GUILayout.Width(width));

                if (isSelected)
                {
                    var color = new Color(0, 0, 1, 0.3f);
                    EditorGUI.DrawRect(vertical.rect, color);
                }

                var mousePosition = Event.current.mousePosition;
                if (vertical.rect.Contains(mousePosition))
                {
                    if (Event.current.type == EventType.MouseDown)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
