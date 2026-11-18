// <copyright file="LilToonMaterial.Albedo.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using KRT.VRCQuestTools.Utils;
using UnityEngine;

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <content>
    /// Albedo source implementation of lilToon material.
    /// </content>
    internal partial class LilToonMaterial
    {
        /// <inheritdoc/>
        internal override bool RequiresAlbedoTexture(AlbedoBakeRequest request)
        {
            if (request.FoldEmission && (UseEmission || UseEmission2nd))
            {
                return true;
            }
            return Material.mainTexture != null;
        }

        /// <inheritdoc/>
        internal override AsyncCallbackRequest GenerateAlbedo(AlbedoBakeRequest request, Action<Texture2D> completion)
        {
            if (!request.FoldEmission || request.TargetRendersMatcap)
            {
                // Bake a copy which excludes what the target renders by itself.
                var bakeMat = new LilToonMaterial(new Material(Material));
                if (!request.FoldEmission)
                {
                    bakeMat.UseEmission = false;
                    bakeMat.UseEmission2nd = false;
                }
                if (request.TargetRendersMatcap)
                {
                    bakeMat.Material.SetFloat("_VQT_UseToonStandardMatCap", 1);
                }
                return bakeMat.BakeAlbedo(request, (texture) =>
                {
                    UnityEngine.Object.DestroyImmediate(bakeMat.Material);
                    completion?.Invoke(texture);
                });
            }
            return BakeAlbedo(request, completion);
        }

        private AsyncCallbackRequest BakeAlbedo(AlbedoBakeRequest request, Action<Texture2D> completion)
        {
            var main = DisposableObject.New(MainBake(Material, 0));
            return EmissionBake(main.Object, Material, request, (texture) =>
            {
                main.Dispose();
                completion?.Invoke(texture);
            });
        }
    }
}
