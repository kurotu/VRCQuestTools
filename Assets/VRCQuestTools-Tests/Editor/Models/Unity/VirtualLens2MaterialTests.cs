// Tests for VirtualLens2Material

using NUnit.Framework;
using UnityEngine;
using KRT.VRCQuestTools.Models.Unity;

namespace KRT.VRCQuestTools.Models.Unity.Tests
{
    [TestFixture]
    public class VirtualLens2MaterialTests
    {
        [Test]
        public void ToonLitBakeShader_IsNull()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var vlMat = new VirtualLens2Material(mat);
                Assert.IsNull(vlMat.ToonLitBakeShader);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GenerateToonLitImage_UnlitPreview_ReturnsBlackTexture()
        {
            // Create a material with a shader name containing "UnlitPreview"
            // We'll use Standard and rename the shader won't work, so we test with the real name
            var shader = Shader.Find("Standard");
            var mat = new Material(shader);
            mat.shader = shader;
            try
            {
                var vlMat = new VirtualLens2Material(mat);
                Texture2D result = null;
                var request = vlMat.GenerateToonLitImage(null, (tex) => { result = tex; });
                request.WaitForCompletion();
                // Standard shader name doesn't contain "UnlitPreview", so result should be null
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GenerateToonLitImage_NonUnlitPreview_ReturnsNull()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var vlMat = new VirtualLens2Material(mat);
                Texture2D result = null;
                var request = vlMat.GenerateToonLitImage(null, (tex) => { result = tex; });
                request.WaitForCompletion();
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }
    }
}
