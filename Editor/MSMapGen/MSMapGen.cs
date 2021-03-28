// <copyright file="MSMapGen.cs" company="kurotu">
// Copyright (c) kurotu. All rights reserved.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

using ImageMagick;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    public class MSMapGen : EditorWindow
    {
        Texture2D metallicMap;
        Texture2D smoothnessMap;
        bool invertSmoothness;
        bool allowOverwriting;
        readonly MSMapGenI18nBase i18n = MSMapGenI18n.Create();

        internal static void ShowWindow()
        {
            GetWindow(typeof(MSMapGen));
        }

        private void OnGUI()
        {
            titleContent.text = "MS Map Generator";
            EditorGUILayout.LabelField("Textures", EditorStyles.boldLabel);
            metallicMap = (Texture2D)EditorGUILayout.ObjectField("Metallic", metallicMap, typeof(Texture2D), false);
            EditorGUILayout.Space();
            smoothnessMap = (Texture2D)EditorGUILayout.ObjectField("Smoothness", smoothnessMap, typeof(Texture2D), false);
            EditorGUI.BeginDisabledGroup(smoothnessMap == null);
            invertSmoothness = EditorGUILayout.Toggle(i18n.InvertSmoothness, invertSmoothness);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            EditorGUILayout.Space();

            var disableGenerate = (smoothnessMap == null) && (metallicMap == null);
            EditorGUI.BeginDisabledGroup(disableGenerate);
            if (GUILayout.Button(i18n.Generate))
            {
                var dest = EditorUtility.SaveFilePanelInProject(i18n.SaveFileDialogTitle("Metallic Smoothness"), "MetallicSmoothness", "png", "Please enter a file name to save the texture to");
                if (dest != "") // Cancel
                {
                    var metallicPath = metallicMap ? AssetDatabase.GetAssetPath(metallicMap) : "";
                    var smoothPath = smoothnessMap ? AssetDatabase.GetAssetPath(smoothnessMap) : "";
                    using (var metallic = metallicMap ?
                        new MagickImage(AssetDatabase.GetAssetPath(metallicMap)) :
                        new MagickImage(MagickColors.White, 2, 2))
                    using (var smoothness = smoothnessMap ?
                        new MagickImage(AssetDatabase.GetAssetPath(smoothnessMap)) :
                        new MagickImage(MagickColors.White, 2, 2))
                    using (var msMap = GenerateMetallicSmoothness(metallic, false, smoothness, invertSmoothness && smoothnessMap))
                    {
                        msMap.Write(dest, MagickFormat.Png32);
                    }
                    AssetDatabase.Refresh();
                    var importer = AssetImporter.GetAtPath(dest) as TextureImporter;
                    importer.sRGBTexture = false;
                    importer.alphaIsTransparency = false;
                    importer.alphaSource = TextureImporterAlphaSource.FromInput;
                    importer.SaveAndReimport();
                }
            }
        }

        public static MagickImage GenerateMetallicSmoothness(MagickImage metallic, bool invertMetallic, MagickImage smoothness, bool invertSmoothness)
        {
            var width = System.Math.Max(metallic.Width, smoothness.Width);
            var height = System.Math.Max(metallic.Height, smoothness.Height);
            using (var m = new MagickImage(metallic))
            using (var s = new MagickImage(smoothness))
            using (var green = new MagickImage(MagickColors.Black, metallic.Width, metallic.Height))
            using (var blue = new MagickImage(MagickColors.Black, metallic.Width, metallic.Height))
            using (var rgb = new MagickImageCollection())
            {
                m.ColorSpace = ImageMagick.ColorSpace.Gray;
                m.Resize(width, height);
                if (invertMetallic) { m.Negate(); }
                s.ColorSpace = ImageMagick.ColorSpace.Gray;
                s.Resize(width, height);
                if (invertSmoothness) { s.Negate(); }

                rgb.Add(m);
                rgb.Add(green);
                rgb.Add(blue);
                var rgba = (MagickImage)rgb.Combine(ImageMagick.ColorSpace.sRGB);
                rgba.Alpha(AlphaOption.On);
                rgba.Composite(s, CompositeOperator.CopyAlpha);
                return rgba;
            }
        }
    }
}
