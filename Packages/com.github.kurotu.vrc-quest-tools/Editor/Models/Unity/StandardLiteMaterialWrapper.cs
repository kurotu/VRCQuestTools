using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Wrapper for Standard Lite material.
    /// </summary>
    internal class StandardLiteMaterialWrapper
    {
        private readonly Material material;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardLiteMaterialWrapper"/> class.
        /// </summary>
        /// <param name="material">Material to wrap.</param>
        internal StandardLiteMaterialWrapper(Material material)
        {
            this.material = material;
        }

        /// <summary>
        /// Gets or sets the albedo texture.
        /// </summary>
        internal Texture Albedo
        {
            get => material.GetTexture("_MainTex");
            set => material.SetTexture("_MainTex", value);
        }

        /// <summary>
        /// Gets or sets the albedo color.
        /// </summary>
        internal Color AlbedoColor
        {
            get => material.GetColor("_Color");
            set => material.SetColor("_Color", value);
        }

        /// <summary>
        /// Gets or sets the metallic smoothness map.
        /// </summary>
        internal Texture MetallicSmoothnessMap
        {
            get => material.GetTexture("_MetallicGlossMap");
            set => material.SetTexture("_MetallicGlossMap", value);
        }

        /// <summary>
        /// Gets or sets the metallic value.
        /// </summary>
        internal float Metallic
        {
            get => material.GetFloat("_Metallic");
            set => material.SetFloat("_Metallic", value);
        }

        /// <summary>
        /// Gets or sets the smoothness value.
        /// </summary>
        internal float Smoothness
        {
            get => material.GetFloat("_Glossiness");
            set => material.SetFloat("_Glossiness", value);
        }

        /// <summary>
        /// Gets or sets the normal map.
        /// </summary>
        internal Texture NormalMap
        {
            get => material.GetTexture("_BumpMap");
            set => material.SetTexture("_BumpMap", value);
        }

        // Not implemented: Occlusion Map.
        // Not implemented: Detail Mask.

        /// <summary>
        /// Sets a value indicating whether the material has emission enabled.
        /// </summary>
        internal bool Emission
        {
            set
            {
                if (value)
                {
                    material.EnableKeyword("_EMISSION");
                    var so = new SerializedObject(material);
                    so.Update();
                    so.FindProperty("m_LightmapFlags").intValue = 6;
                    so.ApplyModifiedProperties();
                }
                else
                {
                    material.DisableKeyword("_EMISSION");
                }
            }
        }

        /// <summary>
        /// Gets or sets the emission map.
        /// </summary>
        internal Texture EmissionMap
        {
            get => material.GetTexture("_EmissionMap");
            set => material.SetTexture("_EmissionMap", value);
        }

        /// <summary>
        /// Gets or sets the emission color.
        /// </summary>
        internal Color EmissionColor
        {
            get => material.GetColor("_EmissionColor");
            set => material.SetColor("_EmissionColor", value);
        }

        // Not implemented: Global Illumination.
        // Not implemented: Detail Albedo.
        // Not implemented: Detail Normal Map.

        /// <summary>
        /// Sets a value indicating whether the material uses specular light probe hack.
        /// </summary>
        internal bool SpecularLightprobeHack
        {
            set
            {
                if (value)
                {
                    material.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
                    material.SetFloat("_SpecularHighlights", 1);
                }
                else
                {
                    material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
                    material.SetFloat("_SpecularHighlights", 0);
                }
            }
        }

        /// <summary>
        /// Sets a value indicating whether the material uses glossy reflections.
        /// </summary>
        internal bool Reflections
        {
            set
            {
                if (value)
                {
                    material.DisableKeyword("_GLOSSYREFLECTIONS_OFF");
                    material.SetFloat("_GlossyReflections", 1);
                }
                else
                {
                    material.EnableKeyword("_GLOSSYREFLECTIONS_OFF");
                    material.SetFloat("_GlossyReflections", 0);
                }
            }
        }

        /// <summary>
        /// Implicitly converts a StandardLiteMaterialWrapper to a Material.
        /// </summary>
        /// <param name="m">This material wrapper.</param>
        public static implicit operator Material(StandardLiteMaterialWrapper m) => m.material;
    }
}
