using KRT.VRCQuestTools.Models;
using UnityEngine;

namespace KRT.VRCQuestTools.Debug.Screenshots
{
    /// <summary>
    /// Creates small procedural, in-memory-only sample objects (materials, textures, skinned
    /// meshes) so documentation screenshots can show plausible non-empty data without depending on
    /// any asset checked into the repository. Everything returned here is transient: the caller is
    /// responsible for destroying it after capture.
    /// </summary>
    internal static class SampleContentFactory
    {
        /// <summary>
        /// Creates a transient <see cref="Material"/> for display purposes only.
        /// </summary>
        /// <param name="name">Display name shown in the Inspector.</param>
        /// <param name="shaderName">Preferred shader name; falls back to "Standard" if not found.</param>
        /// <returns>A new material. The caller must destroy it.</returns>
        internal static Material CreateSampleMaterial(string name, string shaderName = "Standard")
        {
            var shader = Shader.Find(shaderName) ?? Shader.Find("Standard");
            return new Material(shader) { name = name };
        }

        /// <summary>
        /// Creates a small solid-color <see cref="Texture2D"/> for display purposes only.
        /// </summary>
        /// <param name="name">Display name shown in the Inspector.</param>
        /// <param name="color">Fill color.</param>
        /// <returns>A new texture. The caller must destroy it.</returns>
        internal static Texture2D CreateSampleTexture(string name, Color color)
        {
            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false) { name = name };
            var pixels = new Color[4 * 4];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Creates a sample additional material conversion entry targeting <paramref name="material"/>,
        /// for MaterialConversionSettings/AvatarConverterSettings' additionalMaterialConvertSettings list.
        /// </summary>
        /// <param name="material">Sample target material.</param>
        /// <returns>A new additional material convert settings entry.</returns>
        internal static AdditionalMaterialConvertSettings CreateAdditionalMaterialConvertSettings(Material material)
        {
            return new AdditionalMaterialConvertSettings { targetMaterial = material };
        }

        /// <summary>
        /// Creates a temporary GameObject with a <see cref="SkinnedMeshRenderer"/> driving a
        /// minimal procedural quad mesh with one named blend shape, so BlendShapes Copy screenshots
        /// can show a real mesh reference instead of "None".
        /// </summary>
        /// <param name="gameObjectName">Name of the temporary GameObject (shown in the Inspector).</param>
        /// <param name="blendShapeName">Name of the single blend shape frame added to the mesh.</param>
        /// <returns>
        /// A <see cref="SkinnedMeshRenderer"/> on a hidden temporary GameObject. The caller must
        /// destroy both <c>renderer.gameObject</c> and <c>renderer.sharedMesh</c> after capture,
        /// since the mesh is a separate object not owned by the GameObject.
        /// </returns>
        internal static SkinnedMeshRenderer CreateSampleSkinnedMeshRenderer(string gameObjectName, string blendShapeName)
        {
            var vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
            };
            var mesh = new Mesh
            {
                name = gameObjectName,
                vertices = vertices,
                triangles = new[] { 0, 2, 1, 2, 3, 1 },
                normals = new[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back },
            };

            var deltaVertices = new[] { Vector3.zero, Vector3.zero, new Vector3(0f, 0f, 0.1f), new Vector3(0f, 0f, 0.1f) };
            var deltaNormals = new Vector3[vertices.Length];
            var deltaTangents = new Vector3[vertices.Length];
            mesh.AddBlendShapeFrame(blendShapeName, 100f, deltaVertices, deltaNormals, deltaTangents);

            var gameObject = new GameObject(gameObjectName)
            {
                hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave,
            };
            var renderer = gameObject.AddComponent<SkinnedMeshRenderer>();
            renderer.sharedMesh = mesh;
            return renderer;
        }
    }
}
