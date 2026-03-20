// <copyright file="CompositeDisposableTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="CompositeDisposable"/>.
    /// </summary>
    public class CompositeDisposableTests
    {
        private class TestDisposable : IDisposable
        {
            public bool Disposed { get; private set; }

            public void Dispose()
            {
                Disposed = true;
            }
        }

        [Test]
        public void Dispose_DisposesAllItems()
        {
            var d1 = new TestDisposable();
            var d2 = new TestDisposable();
            var d3 = new TestDisposable();

            var composite = new CompositeDisposable();
            composite.Add(d1);
            composite.Add(d2);
            composite.Add(d3);

            composite.Dispose();

            Assert.IsTrue(d1.Disposed);
            Assert.IsTrue(d2.Disposed);
            Assert.IsTrue(d3.Disposed);
        }

        [Test]
        public void Dispose_Empty_DoesNotThrow()
        {
            var composite = new CompositeDisposable();
            Assert.DoesNotThrow(() => composite.Dispose());
        }

        [Test]
        public void Add_SingleItem_DisposedOnDispose()
        {
            var d = new TestDisposable();
            var composite = new CompositeDisposable();
            composite.Add(d);

            Assert.IsFalse(d.Disposed);
            composite.Dispose();
            Assert.IsTrue(d.Disposed);
        }
    }
}
