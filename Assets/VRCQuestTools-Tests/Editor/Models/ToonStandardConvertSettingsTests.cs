// <copyright file="ToonStandardConvertSettingsTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Reflection;
using NUnit.Framework;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Tests for ToonStandardConvertSettings feature mode.
    /// </summary>
    internal class ToonStandardConvertSettingsTests
    {
        /// <summary>
        /// Default featureMode should be OptIn.
        /// </summary>
        [Test]
        public void DefaultFeatureMode_IsOptIn()
        {
            var settings = new ToonStandardConvertSettings();
            Assert.AreEqual(ToonStandardFeaturesMode.OptIn, settings.featureMode);
        }

        /// <summary>
        /// All feature fields should have C# default value of true.
        /// </summary>
        [Test]
        public void FeatureFields_DefaultToTrue()
        {
            var settings = new ToonStandardConvertSettings();
            foreach (var propName in ToonStandardConvertSettings.FeaturePropertyNames)
            {
                var field = typeof(ToonStandardConvertSettings).GetField(propName, BindingFlags.Instance | BindingFlags.Public);
                Assert.IsNotNull(field, $"Field {propName} not found");
                var value = (bool)field.GetValue(settings);
                Assert.IsTrue(value, $"Field {propName} should default to true");
            }
        }

        /// <summary>
        /// LoadDefaultAssets sets featureMode=OptIn and all features to false.
        /// </summary>
        [Test]
        public void LoadDefaultAssets_SetsOptInAndAllFalse()
        {
            var settings = new ToonStandardConvertSettings();
            settings.LoadDefaultAssets();
            Assert.AreEqual(ToonStandardFeaturesMode.OptIn, settings.featureMode);
            foreach (var propName in ToonStandardConvertSettings.FeaturePropertyNames)
            {
                var field = typeof(ToonStandardConvertSettings).GetField(propName, BindingFlags.Instance | BindingFlags.Public);
                var value = (bool)field.GetValue(settings);
                Assert.IsFalse(value, $"Field {propName} should be false after LoadDefaultAssets");
            }
        }

        /// <summary>
        /// SetAllFeatures(true) sets all feature fields to true.
        /// </summary>
        [Test]
        public void SetAllFeatures_True_SetsAllTrue()
        {
            var settings = new ToonStandardConvertSettings();
            settings.SetAllFeatures(true);
            foreach (var propName in ToonStandardConvertSettings.FeaturePropertyNames)
            {
                var field = typeof(ToonStandardConvertSettings).GetField(propName, BindingFlags.Instance | BindingFlags.Public);
                var value = (bool)field.GetValue(settings);
                Assert.IsTrue(value, $"Field {propName} should be true");
            }
        }

        /// <summary>
        /// SetAllFeatures(false) sets all feature fields to false.
        /// </summary>
        [Test]
        public void SetAllFeatures_False_SetsAllFalse()
        {
            var settings = new ToonStandardConvertSettings();
            settings.SetAllFeatures(false);
            foreach (var propName in ToonStandardConvertSettings.FeaturePropertyNames)
            {
                var field = typeof(ToonStandardConvertSettings).GetField(propName, BindingFlags.Instance | BindingFlags.Public);
                var value = (bool)field.GetValue(settings);
                Assert.IsFalse(value, $"Field {propName} should be false");
            }
        }

        /// <summary>
        /// All ToonStandardFeature-attributed fields must start with "use".
        /// </summary>
        [Test]
        public void FeatureFields_AllStartWithUse()
        {
            foreach (var propName in ToonStandardConvertSettings.FeaturePropertyNames)
            {
                Assert.IsTrue(propName.StartsWith("use"), $"Feature field '{propName}' must start with 'use'");
            }
        }

        /// <summary>
        /// OnAfterDeserialize with old version and OptOut mode enables new features.
        /// </summary>
        [Test]
        public void OnAfterDeserialize_OldOptOut_EnablesNewFeature()
        {
            var settings = new ToonStandardConvertSettings();
            settings.featureMode = ToonStandardFeaturesMode.OptOut;

            var versionField = typeof(ToonStandardConvertSettings).GetField("serializedVersion", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(versionField, "serializedVersion field not found");
            versionField.SetValue(settings, -1);

            settings.SetAllFeatures(false);

            ((UnityEngine.ISerializationCallbackReceiver)settings).OnAfterDeserialize();

            foreach (var propName in ToonStandardConvertSettings.FeaturePropertyNames)
            {
                var field = typeof(ToonStandardConvertSettings).GetField(propName, BindingFlags.Instance | BindingFlags.Public);
                var attr = field.GetCustomAttribute<ToonStandardFeatureAttribute>();
                if (attr.IntroVersion > -1)
                {
                    var value = (bool)field.GetValue(settings);
                    Assert.IsTrue(value, $"OptOut: Field {propName} should be true after OnAfterDeserialize");
                }
            }
        }

        /// <summary>
        /// OnAfterDeserialize with old version and OptIn mode disables new features.
        /// </summary>
        [Test]
        public void OnAfterDeserialize_OldOptIn_DisablesNewFeature()
        {
            var settings = new ToonStandardConvertSettings();
            settings.featureMode = ToonStandardFeaturesMode.OptIn;

            var versionField = typeof(ToonStandardConvertSettings).GetField("serializedVersion", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(versionField, "serializedVersion field not found");
            versionField.SetValue(settings, -1);

            settings.SetAllFeatures(true);

            ((UnityEngine.ISerializationCallbackReceiver)settings).OnAfterDeserialize();

            foreach (var propName in ToonStandardConvertSettings.FeaturePropertyNames)
            {
                var field = typeof(ToonStandardConvertSettings).GetField(propName, BindingFlags.Instance | BindingFlags.Public);
                var attr = field.GetCustomAttribute<ToonStandardFeatureAttribute>();
                if (attr.IntroVersion > -1)
                {
                    var value = (bool)field.GetValue(settings);
                    Assert.IsFalse(value, $"OptIn: Field {propName} should be false after OnAfterDeserialize");
                }
            }
        }
    }
}
