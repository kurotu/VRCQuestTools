// <copyright file="ComponentUtilityTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="ComponentUtility"/>.
    /// </summary>
    public class ComponentUtilityTests
    {
        [Test]
        public void GetPrimaryMaterialConversionComponent_NoComponent_ReturnsNull()
        {
            var go = new GameObject("Test");
            try
            {
                var result = ComponentUtility.GetPrimaryMaterialConversionComponent(go);
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetPrimaryMaterialConversionComponent_WithAvatarConverterSettings_ReturnsPrimary()
        {
            var go = new GameObject("Test");
            try
            {
                var settings = go.AddComponent<AvatarConverterSettings>();
                var result = ComponentUtility.GetPrimaryMaterialConversionComponent(go);
                Assert.IsNotNull(result);
                Assert.AreSame(settings, result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void GetPrimaryMaterialConversionComponent_MaterialConversionSettingsOnly_ReturnsNull()
        {
            // MaterialConversionSettings.IsPrimaryRoot requires VRC_AvatarDescriptor on same GO
            // Without it, IsPrimaryRoot is false, so GetPrimaryMaterialConversionComponent returns null
            var go = new GameObject("Test");
            try
            {
                go.AddComponent<MaterialConversionSettings>();
                var result = ComponentUtility.GetPrimaryMaterialConversionComponent(go);
                Assert.IsNull(result);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
