using System;
using System.Reflection;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests
{
    internal static class LilToonTestHelper
    {
        internal static Shader FindLilToonShader()
        {
            var shader = Shader.Find("lilToon");
            if (shader == null)
            {
                throw new InvalidOperationException("lilToon shader not found. Is the lilToon package installed?");
            }

            return shader;
        }

        internal static Material CreateLilToonMaterial(string name = "TestLilToon")
        {
            var shader = FindLilToonShader();
            var mat = new Material(shader);
            mat.name = name;
            return mat;
        }

        internal static LilToonMaterial CreateLilToonMaterialWrapper(string name = "TestLilToon")
        {
            var mat = CreateLilToonMaterial(name);
            var ctor = typeof(LilToonMaterial).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new[] { typeof(Material) },
                null);
            return (LilToonMaterial)ctor.Invoke(new object[] { mat });
        }

        internal static Texture2D CreateTestTexture(int width = 64, int height = 64)
        {
            return new Texture2D(width, height, TextureFormat.RGBA32, false);
        }

        /// <summary>
        /// Skips the current test or fixture if lilToon is not imported.
        /// Call this from [OneTimeSetUp] to skip all tests in a fixture.
        /// </summary>
        internal static void SkipIfNotImported()
        {
            if (!AssetUtility.IsLilToonImported())
            {
                Assert.Ignore("lilToon is not imported.");
            }
        }
    }
}
