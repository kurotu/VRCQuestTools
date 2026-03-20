// <copyright file="AAOMergePhysBoneReflectionInfoTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using KRT.VRCQuestTools.Models.VRChat;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests.Models.VRChat
{
    [TestFixture]
    internal class AAOMergePhysBoneReflectionInfoTests
    {
        [Test]
        public void Default_IsNullOrValid()
        {
            var info = AAOMergePhysBoneProvider.AAOMergePhysBoneReflectionInfo.Default;
            if (info != null)
            {
                Assert.IsNotNull(info.MergePhysBoneType);
                Assert.IsNotNull(info.ComponentsSetField);
                Assert.IsNotNull(info.GetAsListMethod);
            }
            else
            {
                Assert.Pass("AAO is not installed, Default is null as expected");
            }
        }

        [Test]
        public void Constructor_NullTypeName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new AAOMergePhysBoneProvider.AAOMergePhysBoneReflectionInfo(null, "field", "method");
            });
        }

        [Test]
        public void Constructor_EmptyTypeName_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new AAOMergePhysBoneProvider.AAOMergePhysBoneReflectionInfo("", "field", "method");
            });
        }

        [Test]
        public void Constructor_NullFieldName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new AAOMergePhysBoneProvider.AAOMergePhysBoneReflectionInfo("SomeType", null, "method");
            });
        }

        [Test]
        public void Constructor_EmptyFieldName_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new AAOMergePhysBoneProvider.AAOMergePhysBoneReflectionInfo("SomeType", "", "method");
            });
        }

        [Test]
        public void Constructor_NullMethodName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new AAOMergePhysBoneProvider.AAOMergePhysBoneReflectionInfo("SomeType", "field", null);
            });
        }

        [Test]
        public void Constructor_EmptyMethodName_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new AAOMergePhysBoneProvider.AAOMergePhysBoneReflectionInfo("SomeType", "field", "");
            });
        }

        [Test]
        public void Constructor_NonExistentType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                new AAOMergePhysBoneProvider.AAOMergePhysBoneReflectionInfo("NonExistent.Type.Name.That.Doesnt.Exist", "field", "method");
            });
        }

        // ---- AAOMergePhysBoneProvider constructor tests ----

        [Test]
        public void Provider_NullComponent_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new AAOMergePhysBoneProvider(null);
            });
        }

        [Test]
        public void Provider_WrongComponentType_ThrowsArgumentException()
        {
            var go = new GameObject("Test");
            var comp = go.AddComponent<MeshRenderer>();
            try
            {
                Assert.Throws<ArgumentException>(() =>
                {
                    new AAOMergePhysBoneProvider(comp);
                });
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
