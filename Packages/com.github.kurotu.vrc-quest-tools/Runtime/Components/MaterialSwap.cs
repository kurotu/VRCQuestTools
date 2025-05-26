using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KRT.VRCQuestTools.Components
{
    /// <summary>
    /// Component to swap materials on specific game objects for platform optimization.
    /// </summary>
    [AddComponentMenu("VRCQuestTools/VQT Material Swap")]
    [HelpURL("https://kurotu.github.io/VRCQuestTools/docs/references/components/material-swap?lang=auto")]
    [DisallowMultipleComponent]
    public class MaterialSwap : VRCQuestToolsEditorOnly, IMaterialOperatorComponent, INdmfComponent, IPlatformDependentComponent
    {
        /// <summary>
        /// List of material mappings.
        /// </summary>
        public List<MaterialMapping> materialMappings = new List<MaterialMapping>();

        /// <summary>
        /// Apply material swaps based on the mapping.
        /// </summary>
        public void ApplyMaterialSwaps()
        {
            var renderers = GetComponentsInChildren<Renderer>(true);

            foreach (var renderer in renderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                var materials = renderer.sharedMaterials;
                bool materialChanged = false;

                // Check each material slot against our mappings
                for (int i = 0; i < materials.Length; i++)
                {
                    var currentMaterial = materials[i];
                    if (currentMaterial == null)
                    {
                        continue;
                    }

                    // Find matching mapping
                    var mapping = materialMappings.FirstOrDefault(m =>
                        m.originalMaterial == currentMaterial &&
                        m.replacementMaterial != null);

                    if (mapping != null)
                    {
                        materials[i] = mapping.replacementMaterial;
                        materialChanged = true;
                    }
                }

                if (materialChanged)
                {
                    renderer.sharedMaterials = materials;
                }
            }
        }

        /// <summary>
        /// Material mapping pairs for replacement.
        /// </summary>
        [System.Serializable]
        public class MaterialMapping
        {
            /// <summary>
            /// Original material to replace.
            /// </summary>
            public Material originalMaterial;

            /// <summary>
            /// Replacement material.
            /// </summary>
            public Material replacementMaterial;
        }
    }
}
