// Tests for AvatarConverter methods:
// CreateMaterialConvertSettingsMap, RemoveExtraMaterialSlots

using System.Collections.Generic;
using System.Reflection;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    public class AvatarConverterMethodTests
    {
        private AvatarConverter converter;

        [SetUp]
        public void SetUp()
        {
            converter = new AvatarConverter(new MaterialWrapperBuilder());
        }

        // --- RemoveExtraMaterialSlots ---

        [Test]
        public void RemoveExtraMaterialSlots_ExtraMaterials_RemovedToMatchSubMeshCount()
        {
            var go = new GameObject("Avatar");
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up, Vector3.right };
            mesh.triangles = new int[] { 0, 1, 2 };
            mesh.subMeshCount = 1;
            smr.sharedMesh = mesh;
            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            smr.sharedMaterials = new Material[] { mat1, mat2 };
            try
            {
                // smr has 2 materials but mesh only has 1 submesh
                Assert.That(smr.sharedMaterials.Length, Is.EqualTo(2));

                var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots", BindingFlags.Instance | BindingFlags.NonPublic);
                method.Invoke(converter, new object[] { go });

                Assert.That(smr.sharedMaterials.Length, Is.EqualTo(1));
                Assert.That(smr.sharedMaterials[0], Is.EqualTo(mat1));
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(mesh);
                Object.DestroyImmediate(mat1);
                Object.DestroyImmediate(mat2);
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_EqualMaterials_NoChange()
        {
            var go = new GameObject("Avatar");
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up, Vector3.right };
            mesh.triangles = new int[] { 0, 1, 2 };
            mesh.subMeshCount = 1;
            smr.sharedMesh = mesh;
            var mat = new Material(Shader.Find("Standard"));
            smr.sharedMaterials = new Material[] { mat };
            try
            {
                var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots", BindingFlags.Instance | BindingFlags.NonPublic);
                method.Invoke(converter, new object[] { go });

                Assert.That(smr.sharedMaterials.Length, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(mesh);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_NoMesh_SkipsRenderer()
        {
            var go = new GameObject("Avatar");
            var child = new GameObject("Body");
            child.transform.SetParent(go.transform);
            var smr = child.AddComponent<SkinnedMeshRenderer>();
            // No mesh assigned
            var mat = new Material(Shader.Find("Standard"));
            smr.sharedMaterials = new Material[] { mat };
            try
            {
                var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.DoesNotThrow(() => method.Invoke(converter, new object[] { go }));
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void RemoveExtraMaterialSlots_MeshRenderer_ExtraMaterials_Removed()
        {
            var go = new GameObject("Avatar");
            var child = new GameObject("Decor");
            child.transform.SetParent(go.transform);
            var mf = child.AddComponent<MeshFilter>();
            var mr = child.AddComponent<MeshRenderer>();
            var mesh = new Mesh();
            mesh.vertices = new Vector3[] { Vector3.zero, Vector3.one, Vector3.up };
            mesh.triangles = new int[] { 0, 1, 2 };
            mesh.subMeshCount = 1;
            mf.sharedMesh = mesh;
            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            var mat3 = new Material(Shader.Find("Standard"));
            mr.sharedMaterials = new Material[] { mat1, mat2, mat3 };
            try
            {
                var method = typeof(AvatarConverter).GetMethod("RemoveExtraMaterialSlots", BindingFlags.Instance | BindingFlags.NonPublic);
                method.Invoke(converter, new object[] { go });

                Assert.That(mr.sharedMaterials.Length, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(mesh);
                Object.DestroyImmediate(mat1);
                Object.DestroyImmediate(mat2);
                Object.DestroyImmediate(mat3);
            }
        }

        // --- CreateMaterialConvertSettingsMap ---

        [Test]
        public void CreateMaterialConvertSettingsMap_NoComponents_ReturnsEmpty()
        {
            var go = new GameObject("Avatar");
            go.AddComponent<VRCAvatarDescriptor>();
            try
            {
                var materials = new Material[0];
                var result = converter.CreateMaterialConvertSettingsMap(go, materials);
                Assert.That(result.Count, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithAvatarConverterSettings_AppliesDefault()
        {
            var go = new GameObject("Avatar");
            go.AddComponent<VRCAvatarDescriptor>();
            var settings = go.AddComponent<AvatarConverterSettings>();
            // AvatarConverterSettings provides default MaterialConvertSettings

            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var materials = new Material[] { mat };
                var result = converter.CreateMaterialConvertSettingsMap(go, materials);
                // Should have an entry for the material via the default settings
                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result.ContainsKey(mat), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_MaterialNotInAvatar_OmitsFromResult()
        {
            var go = new GameObject("Avatar");
            go.AddComponent<VRCAvatarDescriptor>();
            var settings = go.AddComponent<AvatarConverterSettings>();

            var mat = new Material(Shader.Find("Standard"));
            var unusedMat = new Material(Shader.Find("Standard"));
            try
            {
                // Only mat is in the avatar materials list
                var materials = new Material[] { mat };
                var result = converter.CreateMaterialConvertSettingsMap(go, materials);
                Assert.That(result.ContainsKey(unusedMat), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(mat);
                Object.DestroyImmediate(unusedMat);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithMaterialConversionSettings_NullTarget_Throws()
        {
            var go = new GameObject("Avatar");
            go.AddComponent<VRCAvatarDescriptor>();
            var mcs = go.AddComponent<MaterialConversionSettings>();
            mcs.additionalMaterialConvertSettings = new AdditionalMaterialConvertSettings[]
            {
                new AdditionalMaterialConvertSettings
                {
                    targetMaterial = null,
                    materialConvertSettings = new ToonLitConvertSettings(),
                },
            };

            try
            {
                var materials = new Material[0];
                Assert.Throws<TargetMaterialNullException>(() => converter.CreateMaterialConvertSettingsMap(go, materials));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithMaterialSwap_NullOriginal_Throws()
        {
            var go = new GameObject("Avatar");
            go.AddComponent<VRCAvatarDescriptor>();
            var swap = go.AddComponent<MaterialSwap>();
            var replacementMat = new Material(Shader.Find("VRChat/Mobile/Toon Lit"));
            swap.materialMappings = new System.Collections.Generic.List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping
                {
                    originalMaterial = null,
                    replacementMaterial = replacementMat,
                },
            };

            try
            {
                var materials = new Material[0];
                Assert.Throws<InvalidMaterialSwapNullException>(() => converter.CreateMaterialConvertSettingsMap(go, materials));
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(replacementMat);
            }
        }

        [Test]
        public void CreateMaterialConvertSettingsMap_WithMaterialSwap_NullReplacement_Throws()
        {
            var go = new GameObject("Avatar");
            go.AddComponent<VRCAvatarDescriptor>();
            var swap = go.AddComponent<MaterialSwap>();
            var originalMat = new Material(Shader.Find("Standard"));
            swap.materialMappings = new System.Collections.Generic.List<MaterialSwap.MaterialMapping>
            {
                new MaterialSwap.MaterialMapping
                {
                    originalMaterial = originalMat,
                    replacementMaterial = null,
                },
            };

            try
            {
                var materials = new Material[] { originalMat };
                Assert.Throws<InvalidMaterialSwapNullException>(() => converter.CreateMaterialConvertSettingsMap(go, materials));
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(originalMat);
            }
        }
    }
}
