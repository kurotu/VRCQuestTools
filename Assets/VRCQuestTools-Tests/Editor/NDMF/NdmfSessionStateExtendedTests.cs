// Tests for NdmfSessionState and NdmfLocalizer.

using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

namespace KRT.VRCQuestTools.Tests.NDMF
{
    [TestFixture]
    internal class NdmfSessionStateExtendedTests
    {
        private Type sessionStateType;

        [SetUp]
        public void SetUp()
        {
            sessionStateType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfSessionState");
            if (sessionStateType == null)
                Assert.Ignore("NdmfSessionState type not found.");
        }

        [Test]
        public void BuildTarget_GetterAndSetter_Work()
        {
            var prop = sessionStateType.GetProperty("BuildTarget",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (prop == null) Assert.Ignore("BuildTarget property not found.");

            // Read current value
            var current = prop.GetValue(null);
            Assert.IsNotNull(current);

            // Set to Auto (0) and verify
            var autoValue = Enum.ToObject(prop.PropertyType, 0);
            prop.SetValue(null, autoValue);
            var result = prop.GetValue(null);
            Assert.AreEqual(autoValue, result);
        }

        [Test]
        public void BuildTarget_SetToAndroid_Persists()
        {
            var prop = sessionStateType.GetProperty("BuildTarget",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (prop == null) Assert.Ignore("BuildTarget property not found.");

            // Set to Android (2) and verify
            var androidValue = Enum.ToObject(prop.PropertyType, 2);
            prop.SetValue(null, androidValue);
            var result = prop.GetValue(null);
            Assert.AreEqual(androidValue, result);

            // Restore Auto
            var autoValue = Enum.ToObject(prop.PropertyType, 0);
            prop.SetValue(null, autoValue);
        }

        [Test]
        public void LastActualPerformanceRating_IsAccessible()
        {
            var field = sessionStateType.GetField("LastActualPerformanceRating",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null) Assert.Ignore("LastActualPerformanceRating field not found.");

            var dict = field.GetValue(null);
            Assert.IsNotNull(dict);
        }
    }

    [TestFixture]
    internal class NdmfLocalizerTests
    {
        private Type localizerType;

        [SetUp]
        public void SetUp()
        {
            localizerType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfLocalizer");
            if (localizerType == null)
                Assert.Ignore("NdmfLocalizer type not found.");
        }

        [Test]
        public void Instance_FieldExists()
        {
            var field = localizerType.GetField("Instance",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.IsNotNull(field, "Instance field should exist on NdmfLocalizer.");
        }

        [Test]
        public void Instance_IsNotNull()
        {
            var field = localizerType.GetField("Instance",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null) Assert.Ignore("Instance field not found.");
            var value = field.GetValue(null);
            Assert.IsNotNull(value, "Instance should not be null.");
        }

        [Test]
        public void HasLocalizationKeyConstants()
        {
            // NdmfLocalizer defines many internal const string fields for localization keys
            var fields = localizerType.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            int constCount = 0;
            foreach (var f in fields)
            {
                if (f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                    constCount++;
            }
            Assert.IsTrue(constCount >= 10, $"Expected at least 10 localization key constants, found {constCount}.");
        }

        [Test]
        public void LocalizationKeys_StartWithNDMFPrefix()
        {
            var fields = localizerType.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var f in fields)
            {
                if (f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                {
                    var value = (string)f.GetRawConstantValue();
                    Assert.IsTrue(value.StartsWith("NDMF:"),
                        $"Localization key '{f.Name}' should start with 'NDMF:' but was '{value}'.");
                }
            }
        }
    }

    [TestFixture]
    internal class NdmfErrorReportTests
    {
        private Type errorReportType;

        [SetUp]
        public void SetUp()
        {
            errorReportType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.NdmfErrorReport");
            if (errorReportType == null)
                Assert.Ignore("NdmfErrorReport type not found.");
        }

        [Test]
        public void ReportError_MethodExists()
        {
            var methods = errorReportType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var found = false;
            foreach (var m in methods)
            {
                if (m.Name == "ReportError")
                {
                    found = true;
                    break;
                }
            }
            Assert.IsTrue(found, "ReportError method should exist.");
        }
    }

    [TestFixture]
    internal class NdmfPluginTests
    {
        private Type pluginType;

        [SetUp]
        public void SetUp()
        {
            pluginType = NdmfTestHelper.GetNdmfType("KRT.VRCQuestTools.Ndmf.VRCQuestToolsNdmfPlugin");
            if (pluginType == null)
                Assert.Ignore("VRCQuestToolsNdmfPlugin type not found.");
        }

        [Test]
        public void Plugin_HasQualifiedName()
        {
            var prop = pluginType.GetProperty("QualifiedName",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop == null) Assert.Ignore("QualifiedName not found.");

            var instance = Activator.CreateInstance(pluginType);
            var name = (string)prop.GetValue(instance);
            Assert.IsNotNull(name);
            Assert.IsTrue(name.Contains("vrc-quest-tools") || name.Contains("VRCQuestTools"),
                $"QualifiedName should contain vrc-quest-tools or VRCQuestTools but was: {name}");
        }

        [Test]
        public void Plugin_HasDisplayName()
        {
            var prop = pluginType.GetProperty("DisplayName",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop == null) Assert.Ignore("DisplayName not found.");

            var instance = Activator.CreateInstance(pluginType);
            var name = (string)prop.GetValue(instance);
            Assert.IsNotNull(name);
            Assert.IsNotEmpty(name);
        }
    }
}
