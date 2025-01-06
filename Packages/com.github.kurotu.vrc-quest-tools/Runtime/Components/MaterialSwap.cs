using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace KRT.VRCQuestTools.Components
{
    /// <summary>
    /// Component to swap materials on specific game objects for platform optimization.
    /// </summary>
    [AddComponentMenu("VRCQuestTools/VQT Material Swap")]
    [HelpURL("https://kurotu.github.io/VRCQuestTools/docs/references/components/material-swap?lang=auto")]
    [DisallowMultipleComponent]
    public class MaterialSwap : VRCQuestToolsEditorOnly, INdmfComponent, IPlatformDependentComponent, IExperimentalComponent
    {
        /// <summary>
        /// Material mapping pairs for replacement
        /// </summary>
        [System.Serializable]
        public class MaterialMapping
        {
            public Material originalMaterial;
            public Material replacementMaterial;
        }

        /// <summary>
        /// List of material mappings
        /// </summary>
        public List<MaterialMapping> materialMappings = new List<MaterialMapping>();

        /// <summary>
        /// Enable swapping on Android target
        /// </summary> 
        public bool enabledOnAndroid = true;

        /// <summary>
        /// Include child renderers in material swap
        /// </summary>
        public bool includeChildren = true;

        /// <summary>
        /// Gets all current materials from the renderer
        /// </summary>
        public Material[] GetCurrentMaterials()
        {
            var renderer = GetComponent<Renderer>();
            return renderer != null ? renderer.sharedMaterials : new Material[0];
        }

        /// <summary>
        /// Apply material swaps based on the mapping
        /// </summary>
        public void ApplyMaterialSwaps()
        {
            var renderers = includeChildren ?
                GetComponentsInChildren<Renderer>(true) :
                new[] { GetComponent<Renderer>() };

            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;

                var materials = renderer.sharedMaterials;
                bool materialChanged = false;

                // Check each material slot against our mappings
                for (int i = 0; i < materials.Length; i++)
                {
                    var currentMaterial = materials[i];
                    if (currentMaterial == null) continue;

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

#if UNITY_EDITOR
    private void Reset()
    {
        // Initialize mappings with current materials
        var currentMaterials = GetCurrentMaterials();
        materialMappings.Clear();
        
        foreach (var material in currentMaterials.Where(m => m != null))
        {
            materialMappings.Add(new MaterialMapping {
                originalMaterial = material,
                replacementMaterial = null
            });
        }
    }
#endif
    }
}