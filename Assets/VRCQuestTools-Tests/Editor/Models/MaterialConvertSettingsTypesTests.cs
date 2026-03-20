// <copyright file="MaterialConvertSettingsTypesTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Models;
using NUnit.Framework;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="MaterialConvertSettingsTypes"/>.
    /// </summary>
    public class MaterialConvertSettingsTypesTests
    {
        /// <summary>
        /// Test GetDefaultConvertTypePopups for default settings excludes MaterialReplace.
        /// </summary>
        [Test]
        public void GetDefaultConvertTypePopups_ForDefault_ExcludesMaterialReplace()
        {
            var popups = MaterialConvertSettingsTypes.GetDefaultConvertTypePopups(true);
            Assert.IsNotNull(popups);
            Assert.IsTrue(popups.Count >= 2);

            foreach (var popup in popups)
            {
                Assert.AreNotEqual(typeof(MaterialReplaceSettings), popup.Type);
            }
        }

        /// <summary>
        /// Test GetDefaultConvertTypePopups for non-default includes MaterialReplace.
        /// </summary>
        [Test]
        public void GetDefaultConvertTypePopups_ForNonDefault_IncludesMaterialReplace()
        {
            var popups = MaterialConvertSettingsTypes.GetDefaultConvertTypePopups(false);
            Assert.IsNotNull(popups);

            bool hasMaterialReplace = false;
            foreach (var popup in popups)
            {
                if (popup.Type == typeof(MaterialReplaceSettings))
                {
                    hasMaterialReplace = true;
                }
            }
            Assert.IsTrue(hasMaterialReplace);
        }

        /// <summary>
        /// Test popup items have non-empty labels.
        /// </summary>
        [Test]
        public void GetDefaultConvertTypePopups_AllHaveLabels()
        {
            var popups = MaterialConvertSettingsTypes.GetDefaultConvertTypePopups(false);
            foreach (var popup in popups)
            {
                Assert.IsNotNull(popup.Type);
                Assert.IsNotEmpty(popup.Label);
            }
        }

        /// <summary>
        /// Test popup items include ToonLit and ToonStandard.
        /// </summary>
        [Test]
        public void GetDefaultConvertTypePopups_IncludesExpectedTypes()
        {
            var popups = MaterialConvertSettingsTypes.GetDefaultConvertTypePopups(false);

            bool hasToonLit = false;
            bool hasToonStandard = false;
            bool hasMatCapLit = false;

            foreach (var popup in popups)
            {
                if (popup.Type == typeof(ToonLitConvertSettings))
                {
                    hasToonLit = true;
                }
                if (popup.Type == typeof(ToonStandardConvertSettings))
                {
                    hasToonStandard = true;
                }
                if (popup.Type == typeof(MatCapLitConvertSettings))
                {
                    hasMatCapLit = true;
                }
            }

            Assert.IsTrue(hasToonLit, "Should include ToonLitConvertSettings");
            Assert.IsTrue(hasToonStandard, "Should include ToonStandardConvertSettings");
            Assert.IsTrue(hasMatCapLit, "Should include MatCapLitConvertSettings");
        }

        /// <summary>
        /// Test PopupItem construction.
        /// </summary>
        [Test]
        public void PopupItem_ConstructsCorrectly()
        {
            var item = new MaterialConvertSettingsTypes.PopupItem(typeof(ToonLitConvertSettings), "Test Label");
            Assert.AreEqual(typeof(ToonLitConvertSettings), item.Type);
            Assert.AreEqual("Test Label", item.Label);
        }
    }
}
