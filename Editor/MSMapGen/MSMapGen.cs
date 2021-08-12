// <copyright file="MSMapGen.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

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

        internal static void ShowWindow()
        {
            var window = GetWindow<MSMapGen>(typeof(MSMapGen));
            window.Show();
        }

        private void OnGUI()
        {
            var i18n = I18n.GetI18n();
            titleContent.text = "Metallic Smoothness";
            EditorGUILayout.BeginVertical("HelpBox");
            {
                EditorGUILayout.LabelField("Metallic", EditorStyles.boldLabel);
                metallicMap = (Texture2D)EditorGUILayout.ObjectField(i18n.TextureLabel, metallicMap, typeof(Texture2D), false);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("HelpBox");
            {
                EditorGUILayout.LabelField("Smoothness", EditorStyles.boldLabel);
                smoothnessMap = (Texture2D)EditorGUILayout.ObjectField(i18n.TextureLabel, smoothnessMap, typeof(Texture2D), false);
                EditorGUI.BeginDisabledGroup(smoothnessMap == null);
                invertSmoothness = EditorGUILayout.Toggle(i18n.InvertLabel, invertSmoothness);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            var disableGenerate = (smoothnessMap == null) && (metallicMap == null);
            EditorGUI.BeginDisabledGroup(disableGenerate);
            if (GUILayout.Button(i18n.GenerateButtonLabel))
            {
                var dest = EditorUtility.SaveFilePanelInProject(i18n.SaveFileDialogTitle("Metallic Smoothness"), "MetallicSmoothness", "png", i18n.SaveFileDialogMessage);
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
