// <copyright file="DisposableObjectTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="DisposableObject"/> and <see cref="DisposableObject{T}"/>.
    /// </summary>
    public class DisposableObjectTests
    {
        [Test]
        public void New_CreatesDisposableObject()
        {
            var mat = new Material(Shader.Find("Standard"));
            try
            {
                var disposable = DisposableObject.New(mat);
                Assert.IsNotNull(disposable);
                Assert.AreSame(mat, disposable.Object);
                disposable.Dispose();
            }
            finally
            {
                if (mat != null)
                {
                    Object.DestroyImmediate(mat);
                }
            }
        }

        [Test]
        public void DisposableObject_Dispose_DestroysNonAssetObject()
        {
            var mat = new Material(Shader.Find("Standard"));
            using (var disposable = new DisposableObject<Material>(mat))
            {
                Assert.IsNotNull(disposable.Object);
            }

            // After dispose, the material should be destroyed
            Assert.IsTrue(mat == null);
        }

        [Test]
        public void DisposableObject_UsingPattern_Works()
        {
            Material mat = new Material(Shader.Find("Standard"));
            using (var disposable = DisposableObject.New(mat))
            {
                Assert.IsNotNull(disposable.Object);
                Assert.AreEqual("Standard", disposable.Object.shader.name);
            }
        }
    }
}
