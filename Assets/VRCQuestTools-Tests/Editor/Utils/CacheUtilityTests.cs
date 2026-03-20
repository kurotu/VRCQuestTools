// <copyright file="CacheUtilityTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="CacheUtility"/>.
    /// </summary>
    public class CacheUtilityTests
    {
        [Test]
        public void GetContentCacheKey_StandardMaterial_ReturnsNonEmptyString()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var key = CacheUtility.GetContentCacheKey(mat);
                Assert.IsNotNull(key);
                Assert.IsNotEmpty(key);
                Assert.IsTrue(key.Contains("Standard"));
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void GetContentCacheKey_DifferentMaterials_ReturnDifferentKeys()
        {
            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            mat2.color = Color.red;
            try
            {
                var key1 = CacheUtility.GetContentCacheKey(mat1);
                var key2 = CacheUtility.GetContentCacheKey(mat2);
                Assert.AreNotEqual(key1, key2);
            }
            finally
            {
                Object.DestroyImmediate(mat1);
                Object.DestroyImmediate(mat2);
            }
        }

        [Test]
        public void GetContentCacheKey_SameMaterials_ReturnSameKeys()
        {
            var mat1 = new Material(Shader.Find("Standard"));
            var mat2 = new Material(Shader.Find("Standard"));
            try
            {
                var key1 = CacheUtility.GetContentCacheKey(mat1);
                var key2 = CacheUtility.GetContentCacheKey(mat2);
                Assert.AreEqual(key1, key2);
            }
            finally
            {
                Object.DestroyImmediate(mat1);
                Object.DestroyImmediate(mat2);
            }
        }

        [Test]
        public void GetContentCacheKey_IncludesShaderKeywords()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var keyBefore = CacheUtility.GetContentCacheKey(mat);
                mat.EnableKeyword("_EMISSION");
                var keyAfter = CacheUtility.GetContentCacheKey(mat);
                Assert.AreNotEqual(keyBefore, keyAfter);
            }
            finally
            {
                Object.DestroyImmediate(mat);
            }
        }
    }
}
