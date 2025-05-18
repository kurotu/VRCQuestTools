using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Represents additional material convert settings.
    /// </summary>
    [Serializable]
    public class AdditionalMaterialConvertSettings
    {
        /// <summary>
        /// Target material to convert.
        /// </summary>
        [SerializeField]
        public Material targetMaterial = null;

        /// <summary>
        /// Material convert settings.
        /// </summary>
        [SerializeReference]
        public IMaterialConvertSettings materialConvertSettings = new ToonLitConvertSettings();

        /// <summary>
        /// Load default assets for the material convert settings.
        /// </summary>
        public void LoadDefaultAssets()
        {
            materialConvertSettings.LoadDefaultAssets();
        }
    }
}
