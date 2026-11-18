// <copyright file="MaterialConversionCharacterizationTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Characterization tests which pin the current output of material conversion (Toon Lit, MatCap Lit and Toon Standard)
    /// to guard refactoring of the conversion pipeline.
    /// </summary>
    /// <remarks>
    /// <para>Each case converts a source material through <see cref="AvatarConverter.ConvertMaterialsForMobile"/> and compares
    /// a snapshot of the output material (shader, keywords, every property, which properties share a texture, and 4x4 block
    /// averages of each texture as the shader decodes it) with a recorded golden file.</para>
    /// <para>Goldens are stored per build target because texture compression depends on it. When a golden is missing, the case
    /// records it and reports Inconclusive. To accept an intended output change, delete the golden and run the case again.</para>
    /// </remarks>
    public class MaterialConversionCharacterizationTests
    {
        private const float PropertyTolerance = 1e-3f;
        private const float SampleTolerance = 0.03f;
        private const int SampleGrid = 4;
        private const int SampleResolution = 64;
        private const string PoiyomiShaderName = ".poiyomi/Poiyomi Toon";

        private static readonly Dictionary<string, CharacterizationCase> Cases = CreateCases().ToDictionary(c => c.Name);

        private enum SourceRequirement
        {
            None,
            LilToon,
            Poiyomi,
        }

        /// <summary>
        /// Gets the case names.
        /// </summary>
        public static IEnumerable<string> CaseNames => Cases.Keys;

        private static string GoldenFolder => $"{TestUtils.FixturesFolder}/Characterization/{EditorUserBuildSettings.activeBuildTarget}";

        /// <summary>
        /// Clears the texture cache so that every case runs the texture bakes.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            CacheManager.Texture.Clear();
        }

        /// <summary>
        /// Converts a source material and compares the output with the recorded golden.
        /// </summary>
        /// <param name="caseName">Case name.</param>
        [TestCaseSource(nameof(CaseNames))]
        public void ConversionOutputMatchesGolden(string caseName)
        {
            var testCase = Cases[caseName];
            IgnoreIfUnavailable(testCase.Requirement);

            var disposables = new List<Object>();
            try
            {
                var source = testCase.CreateSource(disposables);
                disposables.Add(source);
                var settings = testCase.CreateSettings();

                var converter = new AvatarConverter(new MaterialWrapperBuilder());
                var map = new Dictionary<Material, IMaterialConvertSettings> { [source] = settings };
                var converted = converter.ConvertMaterialsForMobile(map, false, string.Empty, null, false);
                Assert.IsTrue(converted.TryGetValue(source, out var output), "The material should be converted.");
                CollectGenerated(output, disposables);

                var actual = TakeSnapshot(output);
                var goldenPath = $"{GoldenFolder}/{caseName}.json";
                if (!File.Exists(goldenPath))
                {
                    Directory.CreateDirectory(GoldenFolder);
                    File.WriteAllText(goldenPath, JsonUtility.ToJson(actual, true));
                    Assert.Inconclusive($"Recorded golden: {goldenPath}");
                    return;
                }

                var expected = JsonUtility.FromJson<MaterialSnapshot>(File.ReadAllText(goldenPath));
                AssertSnapshotEqual(expected, actual);
            }
            finally
            {
                foreach (var obj in disposables.Where(o => o != null).Distinct())
                {
                    Object.DestroyImmediate(obj);
                }
            }
        }

        private static IEnumerable<CharacterizationCase> CreateCases()
        {
            ToonLitConvertSettings ToonLit() => new ToonLitConvertSettings
            {
                generateQuestTextures = true,
                maxTextureSize = TextureSizeLimit.Max256x256,
                mainTextureBrightness = 0.83f,
                generateShadowFromNormalMap = true,
            };

            ToonStandardConvertSettings ToonStandard(bool generateQuestTextures = true)
            {
                var settings = new ToonStandardConvertSettings();
                settings.LoadDefaultAssets();
                settings.generateQuestTextures = generateQuestTextures;
                settings.maxTextureSize = TextureSizeLimit.Max256x256;
                settings.maskMaxTextureSize = TextureSizeLimit.Max256x256;
                settings.matcapMaxTextureSize = TextureSizeLimit.Max256x256;
                return settings;
            }

            // Toon Lit for every source shader which has a dedicated bake.
            foreach (var file in new[] { "Standard_Emission", "Standard_NoEmission", "Sunao", "UTS2", "arktoon", "arktoon_EmissiveFreak" })
            {
                yield return new CharacterizationCase($"ToonLit_{file}", SourceRequirement.None, _ => Object.Instantiate(TestUtils.LoadMaterial($"{file}.mat")), ToonLit);
            }
            yield return new CharacterizationCase("ToonLit_lilToon", SourceRequirement.LilToon, CreateLilToonFull, ToonLit);
            yield return new CharacterizationCase("ToonLit_Poiyomi", SourceRequirement.Poiyomi, CreatePoiyomiFull, ToonLit);

            yield return new CharacterizationCase("MatCapLit_Standard_Emission", SourceRequirement.None, _ => Object.Instantiate(TestUtils.LoadMaterial("Standard_Emission.mat")), () => new MatCapLitConvertSettings
            {
                generateQuestTextures = true,
                maxTextureSize = TextureSizeLimit.Max256x256,
                matCapTexture = LoadTexture("albedo_1024px_png.png"),
            });

            // Toon Standard.
            yield return new CharacterizationCase("ToonStandard_Standard_Emission", SourceRequirement.None, _ => Object.Instantiate(TestUtils.LoadMaterial("Standard_Emission.mat")), () => ToonStandard());
            yield return new CharacterizationCase("ToonStandard_lilToon_Basic", SourceRequirement.LilToon, CreateLilToonBasic, () => ToonStandard());
            yield return new CharacterizationCase("ToonStandard_lilToon_Full", SourceRequirement.LilToon, CreateLilToonFull, () => ToonStandard());
            yield return new CharacterizationCase("ToonStandard_lilToon_Tiling", SourceRequirement.LilToon, CreateLilToonTiling, () => ToonStandard());
            yield return new CharacterizationCase("ToonStandard_lilToon_NoBake", SourceRequirement.LilToon, CreateLilToonFull, () => ToonStandard(false));
            yield return new CharacterizationCase("ToonStandard_lilToon_NoShadowRamp", SourceRequirement.LilToon, CreateLilToonFull, () =>
            {
                var settings = ToonStandard();
                settings.generateShadowRamp = false;
                return settings;
            });
            yield return new CharacterizationCase("ToonStandard_Poiyomi_Full", SourceRequirement.Poiyomi, CreatePoiyomiFull, () => ToonStandard());
            yield return new CharacterizationCase("ToonStandard_Poiyomi_Stylized", SourceRequirement.Poiyomi, CreatePoiyomiStylized, () => ToonStandard());
            yield return new CharacterizationCase("ToonStandard_Poiyomi_NoBake", SourceRequirement.Poiyomi, CreatePoiyomiFull, () => ToonStandard(false));
        }

        private static Material CreateLilToonBasic(List<Object> disposables)
        {
            var material = new Material(Shader.Find("lilToon"));
            material.mainTexture = LoadTexture("albedo_1024px_png.png");
            material.color = new Color(1.0f, 0.9f, 0.8f, 1.0f);
            return material;
        }

        private static Material CreateLilToonFull(List<Object> disposables)
        {
            var material = CreateLilToonBasic(disposables);
            var gradation = LoadTexture("linear_gradation.png");

            material.SetFloat("_UseBumpMap", 1);
            material.SetTexture("_BumpMap", LoadTexture("NormalMapSample01.png"));
            material.SetFloat("_BumpScale", 0.8f);

            material.SetFloat("_UseShadow", 1);
            material.SetTexture("_ShadowBorderMask", gradation);
            material.SetColor("_ShadowColor", new Color(0.7f, 0.6f, 0.8f, 1.0f));
            material.SetColor("_Shadow2ndColor", new Color(0.5f, 0.4f, 0.6f, 0.5f));
            material.SetFloat("_ShadowStrength", 0.9f);
            material.SetFloat("_LightMinLimit", 0.08f);

            material.SetFloat("_UseEmission", 1);
            material.SetTexture("_EmissionMap", LoadTexture("emission_1024px.png"));
            material.SetColor("_EmissionColor", new Color(1.0f, 0.5f, 0.2f, 1.0f));
            material.SetFloat("_UseEmission2nd", 1);
            material.SetColor("_Emission2ndColor", new Color(0.2f, 0.4f, 1.0f, 0.5f));

            material.SetFloat("_UseReflection", 1);
            material.SetFloat("_Metallic", 0.6f);
            material.SetFloat("_Smoothness", 0.7f);
            material.SetTexture("_MetallicGlossMap", gradation);
            material.SetTexture("_SmoothnessTex", gradation);
            material.SetColor("_ReflectionColor", new Color(0.9f, 0.8f, 0.7f, 0.8f));
            material.SetFloat("_ApplyReflection", 1);

            material.SetFloat("_UseMatCap", 1);
            material.SetTexture("_MatCapTex", LoadTexture("albedo_1024px_png.png"));
            material.SetTexture("_MatCapBlendMask", gradation);
            material.SetFloat("_MatCapBlendMode", 1);
            material.SetFloat("_MatCapBlend", 0.7f);

            material.SetFloat("_UseRim", 1);
            material.SetColor("_RimColor", new Color(0.6f, 0.8f, 1.0f, 0.7f));
            return material;
        }

        private static Material CreateLilToonTiling(List<Object> disposables)
        {
            var material = CreateLilToonFull(disposables);
            material.SetTextureScale("_MainTex", new Vector2(2, 3));
            material.SetTextureOffset("_MainTex", new Vector2(0.25f, 0.5f));
            material.SetTextureScale("_BumpMap", new Vector2(2, 2));
            material.SetTextureScale("_EmissionMap", new Vector2(3, 1));
            return material;
        }

        private static Material CreatePoiyomiFull(List<Object> disposables)
        {
            var material = new Material(Shader.Find(PoiyomiShaderName));
            var gradation = LoadTexture("linear_gradation.png");
            material.mainTexture = LoadTexture("albedo_1024px_png.png");
            material.color = new Color(1.0f, 0.9f, 0.8f, 1.0f);

            material.SetTexture("_BumpMap", LoadTexture("NormalMapSample01.png"));
            material.SetFloat("_BumpScale", 0.8f);

            material.SetFloat("_ShadingEnabled", 1);
            material.SetTexture("_LightingAOMaps", gradation);
            material.SetFloat("_LightDataAOStrengthR", 0.8f);
            material.SetFloat("_LightingMinLightBrightness", 0.05f);

            material.SetFloat("_EnableEmission", 1);
            material.SetTexture("_EmissionMap", LoadTexture("emission_1024px.png"));
            material.SetColor("_EmissionColor", new Color(1.0f, 0.5f, 0.2f, 1.0f));
            material.SetFloat("_EmissionStrength", 2.0f);

            material.SetFloat("_MochieBRDF", 1);
            material.SetTexture("_MochieMetallicMaps", gradation);
            material.SetFloat("_MochieMetallicMultiplier", 0.6f);
            material.SetFloat("_MochieRoughnessMultiplier", 0.7f);

            material.SetFloat("_MatcapEnable", 1);
            material.SetTexture("_Matcap", LoadTexture("albedo_1024px_png.png"));

            material.SetFloat("_EnableRimLighting", 1);
            return material;
        }

        private static Material CreatePoiyomiStylized(List<Object> disposables)
        {
            var material = CreatePoiyomiFull(disposables);
            material.SetFloat("_StylizedSpecular", 1);
            material.SetFloat("_StylizedReflectionMode", 1);
            material.SetFloat("_Metallic", 0.5f);
            material.SetFloat("_Smoothness", 0.6f);
            return material;
        }

        private static Texture2D LoadTexture(string file)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TestUtils.TexturesFolder}/{file}");
            Assert.IsNotNull(texture, $"Fixture texture {file} should exist.");
            return texture;
        }

        private static void IgnoreIfUnavailable(SourceRequirement requirement)
        {
            switch (requirement)
            {
                case SourceRequirement.LilToon:
                    var version = AssetUtility.LilToonVersion;
                    if (!AssetUtility.IsLilToonImported() || Shader.Find("lilToon") == null || version < new SemVer(1, 10, 0) || version >= new SemVer(3, 0, 0))
                    {
                        Assert.Ignore("Supported lilToon is not installed.");
                    }
                    break;
                case SourceRequirement.Poiyomi:
                    if (!AssetUtility.IsPoiyomiImported() || Shader.Find(PoiyomiShaderName) == null)
                    {
                        Assert.Ignore("Poiyomi is not installed.");
                    }
                    break;
            }
        }

        private static void CollectGenerated(Material output, List<Object> disposables)
        {
            if (output == null || AssetDatabase.Contains(output))
            {
                return;
            }
            disposables.Add(output);
            foreach (var name in output.GetTexturePropertyNames())
            {
                var texture = output.GetTexture(name);
                if (texture != null && !AssetDatabase.Contains(texture))
                {
                    disposables.Add(texture);
                }
            }
        }

        private static MaterialSnapshot TakeSnapshot(Material material)
        {
            var snapshot = new MaterialSnapshot
            {
                shader = material.shader.name,
                keywords = material.shaderKeywords.OrderBy(k => k, StringComparer.Ordinal).ToArray(),
            };

            var textures = new List<Texture>();
            var shader = material.shader;
            for (var i = 0; i < shader.GetPropertyCount(); i++)
            {
                var name = shader.GetPropertyName(i);
                var type = shader.GetPropertyType(i);
                var property = new PropertySnapshot { name = name, type = type.ToString(), textureIndex = -1 };
                switch (type)
                {
                    case ShaderPropertyType.Color:
                        var color = material.GetColor(name);
                        property.values = new[] { color.r, color.g, color.b, color.a };
                        break;
                    case ShaderPropertyType.Vector:
                        var vector = material.GetVector(name);
                        property.values = new[] { vector.x, vector.y, vector.z, vector.w };
                        break;
                    case ShaderPropertyType.Float:
                    case ShaderPropertyType.Range:
                        property.values = new[] { material.GetFloat(name) };
                        break;
                    case ShaderPropertyType.Texture:
                        var scale = material.GetTextureScale(name);
                        var offset = material.GetTextureOffset(name);
                        property.values = new[] { scale.x, scale.y, offset.x, offset.y };
                        var texture = material.GetTexture(name);
                        if (texture != null)
                        {
                            property.textureIndex = textures.IndexOf(texture);
                            if (property.textureIndex < 0)
                            {
                                property.textureIndex = textures.Count;
                                textures.Add(texture);
                                snapshot.textures.Add(TakeTextureSnapshot(texture));
                            }
                        }
                        break;
                }
                snapshot.properties.Add(property);
            }
            return snapshot;
        }

        private static TextureSnapshot TakeTextureSnapshot(Texture texture)
        {
            var snapshot = new TextureSnapshot
            {
                type = texture.GetType().Name,
                width = texture.width,
                height = texture.height,
                isAsset = AssetDatabase.Contains(texture),
            };
            if (!(texture is Texture2D))
            {
                return snapshot;
            }

            // Sample what the shader reads: sRGB textures are decoded to linear by the blit.
            var rt = RenderTexture.GetTemporary(SampleResolution, SampleResolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var previous = RenderTexture.active;
            var readable = new Texture2D(SampleResolution, SampleResolution, TextureFormat.RGBA32, false, true);
            try
            {
                Graphics.Blit(texture, rt);
                RenderTexture.active = rt;
                readable.ReadPixels(new Rect(0, 0, SampleResolution, SampleResolution), 0, 0);
                readable.Apply(false, false);
                var pixels = readable.GetPixels();
                var block = SampleResolution / SampleGrid;
                var samples = new List<float>();
                for (var by = 0; by < SampleGrid; by++)
                {
                    for (var bx = 0; bx < SampleGrid; bx++)
                    {
                        var sum = Vector4.zero;
                        for (var y = 0; y < block; y++)
                        {
                            for (var x = 0; x < block; x++)
                            {
                                sum += (Vector4)pixels[((by * block) + y) * SampleResolution + (bx * block) + x];
                            }
                        }
                        var average = sum / (block * block);
                        samples.AddRange(new[] { average.x, average.y, average.z, average.w });
                    }
                }
                snapshot.samples = samples.ToArray();
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
                Object.DestroyImmediate(readable);
            }
            return snapshot;
        }

        private static void AssertSnapshotEqual(MaterialSnapshot expected, MaterialSnapshot actual)
        {
            var errors = new List<string>();
            if (expected.shader != actual.shader)
            {
                errors.Add($"shader: expected {expected.shader}, actual {actual.shader}");
            }
            if (!expected.keywords.SequenceEqual(actual.keywords))
            {
                errors.Add($"keywords: expected [{string.Join(", ", expected.keywords)}], actual [{string.Join(", ", actual.keywords)}]");
            }

            var actualProperties = actual.properties.ToDictionary(p => p.name);
            foreach (var e in expected.properties)
            {
                if (!actualProperties.TryGetValue(e.name, out var a))
                {
                    errors.Add($"{e.name}: missing");
                    continue;
                }
                if (!ValuesEqual(e.values, a.values, PropertyTolerance))
                {
                    errors.Add($"{e.name}: expected ({string.Join(", ", e.values)}), actual ({string.Join(", ", a.values)})");
                }
                if (e.textureIndex != a.textureIndex)
                {
                    errors.Add($"{e.name}: expected texture #{e.textureIndex}, actual #{a.textureIndex}");
                    continue;
                }
                if (e.textureIndex >= 0)
                {
                    var et = expected.textures[e.textureIndex];
                    var at = actual.textures[a.textureIndex];
                    if (et.type != at.type || et.width != at.width || et.height != at.height || et.isAsset != at.isAsset)
                    {
                        errors.Add($"{e.name}: expected {et.type} {et.width}x{et.height} asset={et.isAsset}, actual {at.type} {at.width}x{at.height} asset={at.isAsset}");
                    }
                    else if (!ValuesEqual(et.samples, at.samples, SampleTolerance))
                    {
                        var maxDiff = et.samples.Zip(at.samples, (x, y) => Mathf.Abs(x - y)).DefaultIfEmpty(float.PositiveInfinity).Max();
                        errors.Add($"{e.name}: texture samples differ (max diff {maxDiff})");
                    }
                }
            }

            Assert.IsEmpty(errors, string.Join("\n", errors));
        }

        private static bool ValuesEqual(float[] expected, float[] actual, float tolerance)
        {
            expected = expected ?? Array.Empty<float>();
            actual = actual ?? Array.Empty<float>();
            if (expected.Length != actual.Length)
            {
                return false;
            }
            for (var i = 0; i < expected.Length; i++)
            {
                if (float.IsNaN(expected[i]) != float.IsNaN(actual[i]) || Mathf.Abs(expected[i] - actual[i]) > tolerance)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Snapshot of a converted material.
        /// </summary>
        [Serializable]
        internal class MaterialSnapshot
        {
            /// <summary>Shader name.</summary>
            public string shader;

            /// <summary>Sorted shader keywords.</summary>
            public string[] keywords;

            /// <summary>Every shader property.</summary>
            public List<PropertySnapshot> properties = new List<PropertySnapshot>();

            /// <summary>Distinct textures referenced by the properties.</summary>
            public List<TextureSnapshot> textures = new List<TextureSnapshot>();
        }

        /// <summary>
        /// Snapshot of a material property.
        /// </summary>
        [Serializable]
        internal class PropertySnapshot
        {
            /// <summary>Property name.</summary>
            public string name;

            /// <summary>Property type.</summary>
            public string type;

            /// <summary>Value components, or scale and offset for textures.</summary>
            public float[] values;

            /// <summary>Index of the texture in <see cref="MaterialSnapshot.textures"/>, or -1 when none.</summary>
            public int textureIndex;
        }

        /// <summary>
        /// Snapshot of a texture.
        /// </summary>
        [Serializable]
        internal class TextureSnapshot
        {
            /// <summary>Texture type name.</summary>
            public string type;

            /// <summary>Width.</summary>
            public int width;

            /// <summary>Height.</summary>
            public int height;

            /// <summary>Whether the texture is an asset (not generated).</summary>
            public bool isAsset;

            /// <summary>RGBA block averages as decoded by shaders, row by row.</summary>
            public float[] samples;
        }

        private class CharacterizationCase
        {
            internal CharacterizationCase(string name, SourceRequirement requirement, Func<List<Object>, Material> createSource, Func<IMaterialConvertSettings> createSettings)
            {
                Name = name;
                Requirement = requirement;
                CreateSource = createSource;
                CreateSettings = createSettings;
            }

            internal string Name { get; }

            internal SourceRequirement Requirement { get; }

            internal Func<List<Object>, Material> CreateSource { get; }

            internal Func<IMaterialConvertSettings> CreateSettings { get; }
        }
    }
}
