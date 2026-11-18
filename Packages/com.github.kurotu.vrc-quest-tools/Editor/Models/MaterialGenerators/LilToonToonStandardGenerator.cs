// <copyright file="LilToonToonStandardGenerator.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models.Unity;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Material generator for LilToon to ToonStandard conversion.
    /// </summary>
    internal class LilToonToonStandardGenerator : ToonStandardGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LilToonToonStandardGenerator"/> class.
        /// </summary>
        /// <param name="material">LilToon material. Not used; the material passed to GenerateMaterial is converted.</param>
        /// <param name="settings">Convert settings.</param>
        /// <param name="sharedBlackTexture">Shared black texture to disable emission.</param>
        /// <param name="forEditorPreview">Whether the conversion is for the NDMF editor preview.</param>
        public LilToonToonStandardGenerator(LilToonMaterial material, ToonStandardConvertSettings settings, Texture2D sharedBlackTexture, bool forEditorPreview)
            : base(settings, sharedBlackTexture, forEditorPreview)
        {
        }
    }
}
