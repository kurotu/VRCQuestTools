using System.Linq;
using UnityEngine;

namespace KRT.VRCQuestTools.Components
{
    /// <summary>
    /// Enum for mesh flip direction.
    /// </summary>
    public enum MeshFlipperMeshDirection
    {
        /// <summary>
        /// Create a flipped mesh.
        /// </summary>
        Flip,

        /// <summary>
        /// Create a double-sided mesh.
        /// </summary>
        BothSides,
    }

    /// <summary>
    /// Component to flip the mesh of its game object's renderer.
    /// </summary>
    [AddComponentMenu("VRCQuestTools/VQT Mesh Flipper")]
    [HelpURL("https://kurotu.github.io/VRCQuestTools/docs/references/components/mesh-flipper?lang=auto")]
    [DisallowMultipleComponent]
    public class MeshFlipper : VRCQuestToolsEditorOnly, INdmfComponent, IPlatformDependentComponent, IExperimentalComponent
    {
        /// <summary>
        /// Direction to flip the mesh.
        /// </summary>
        public MeshFlipperMeshDirection direction = MeshFlipperMeshDirection.BothSides;

        /// <summary>
        /// Enable on PC target.
        /// </summary>
        public bool enabledOnPC = false;

        /// <summary>
        /// Enable on Android target.
        /// </summary>
        public bool enabledOnAndroid = true;

        /// <summary>
        /// Get shared mesh of the component.
        /// </summary>
        /// <returns>Shared mesh object.</returns>
        public Mesh GetSharedMesh()
        {
            var smr = GetComponent<SkinnedMeshRenderer>();
            if (smr != null)
            {
                return smr.sharedMesh;
            }
            var mf = GetComponent<MeshFilter>();
            if (mf != null)
            {
                return mf.sharedMesh;
            }
            return null;
        }

        /// <summary>
        /// Set shared mesh to the component.
        /// </summary>
        /// <param name="mesh">Mesh to set.</param>
        public void SetSharedMesh(Mesh mesh)
        {
            var smr = GetComponent<SkinnedMeshRenderer>();
            if (smr != null)
            {
                smr.sharedMesh = mesh;
            }
            var mf = GetComponent<MeshFilter>();
            if (mf != null)
            {
                mf.sharedMesh = mesh;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Create a new mesh based on the direction.
        /// </summary>
        /// <returns>New mesh.</returns>
        public Mesh CreateMesh()
        {
            var sharedMesh = GetSharedMesh();
            switch (direction)
            {
                case MeshFlipperMeshDirection.Flip:
                    return CreateFlippedMesh(sharedMesh);
                case MeshFlipperMeshDirection.BothSides:
                    return CreateBothSidesMesh(sharedMesh);
                default:
                    throw new System.InvalidProgramException("Unknown direction");
            }
        }

        private static Mesh CreateFlippedMesh(Mesh mesh)
        {
            Mesh newMesh = Instantiate(mesh);
            newMesh.name = mesh.name + "_flipped";
            for (int i = 0; i < newMesh.subMeshCount; i++)
            {
                newMesh.SetTriangles(newMesh.GetTriangles(i).Reverse().ToArray(), i);
            }
            return newMesh;
        }

        private static Mesh CreateBothSidesMesh(Mesh mesh)
        {
            Mesh newMesh = Instantiate(mesh);
            newMesh.name = mesh.name + "_bothSides";
            for (int i = 0; i < newMesh.subMeshCount; i++)
            {
                var triangles = newMesh.GetTriangles(i);
                newMesh.SetTriangles(triangles.Concat(triangles.Reverse()).ToArray(), i);
            }
            return newMesh;
        }
#endif
    }
}
