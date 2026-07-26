using System;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Class for Toon Lit material generator.
    /// </summary>
    internal class ToonLitGenerator : IMaterialGenerator
    {
        private readonly IToonLitConvertSettings settings;
        private readonly bool forEditorPreview;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToonLitGenerator"/> class.
        /// </summary>
        /// <param name="settings">Convert settings.</param>
        /// <param name="forEditorPreview">Whether the conversion is for the NDMF editor preview.</param>
        internal ToonLitGenerator(IToonLitConvertSettings settings, bool forEditorPreview = false)
        {
            this.settings = settings;
            this.forEditorPreview = forEditorPreview;
        }

        /// <inheritdoc/>
        public AsyncCallbackRequest GenerateMaterial(MaterialBase material, UnityEditor.BuildTarget buildTarget, bool saveTextureAsPng, string texturesPath, Action<Material> completion)
        {
            var newMaterial = material.ConvertToToonLit();
            if (settings.GenerateQuestTextures)
            {
                return GenerateToonLitTexture(material, saveTextureAsPng, texturesPath, (texture) =>
                {
                    newMaterial.mainTexture = texture;
                    completion?.Invoke(newMaterial);
                });
            }
            return new ResultRequest<UnityEngine.Object>(null, (_) =>
            {
                completion?.Invoke(newMaterial);
            });
        }

        /// <inheritdoc/>
        public AsyncCallbackRequest GenerateTextures(MaterialBase material, UnityEditor.BuildTarget buildTarget, bool saveTextureAsPng, string texturesPath, Action completion)
        {
            return GenerateToonLitTexture(material, saveTextureAsPng, texturesPath, (_) =>
            {
                completion?.Invoke();
            });
        }

        private AsyncCallbackRequest GenerateToonLitTexture(MaterialBase material, bool saveAsPng, string texturesPath, Action<Texture2D> completion)
        {
            var platformOverride = material.GetToonLitPlatformOverride();
            return MaterialGeneratorUtility.GenerateTexture(material.Material, settings, "main", saveAsPng, texturesPath, (compl) => material.GenerateToonLitImage(settings, compl), completion, platformOverride, forEditorPreview);
        }
    }
}
