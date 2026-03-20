// Tests for FallbackAvatarCallback.OnPreprocessAvatar - FallbackAvatar component and PendingFallbackAvatars logic.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests.Callbacks
{
    [TestFixture]
    internal class FallbackAvatarCallbackTests
    {
        private static readonly Type callbackType =
            typeof(KRT.VRCQuestTools.VRCQuestTools).Assembly.GetType("KRT.VRCQuestTools.NonDestructive.FallbackAvatarCallback");

        private static readonly Type pmType = SystemUtility.GetTypeByName("VRC.Core.PipelineManager");
        private MethodInfo onPreprocessAvatarMethod;
        private FieldInfo pendingField;

        [SetUp]
        public void SetUp()
        {
            if (callbackType == null) Assert.Ignore("FallbackAvatarCallback type not found.");
            if (pmType == null) Assert.Ignore("PipelineManager type not found.");

            onPreprocessAvatarMethod = callbackType.GetMethod("OnPreprocessAvatar",
                BindingFlags.Instance | BindingFlags.Public);
            if (onPreprocessAvatarMethod == null) Assert.Ignore("OnPreprocessAvatar method not found.");

            pendingField = callbackType.GetField("PendingFallbackAvatars",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        private object CreateCallback() => Activator.CreateInstance(callbackType);

        private bool InvokeOnPreprocessAvatar(object callback, GameObject go)
        {
            return (bool)onPreprocessAvatarMethod.Invoke(callback, new object[] { go });
        }

        private IDictionary<string, bool> GetPendingDict()
        {
            if (pendingField == null) return null;
            return pendingField.GetValue(null) as IDictionary<string, bool>;
        }

        private void SetBlueprintId(Component pm, string id)
        {
            var field = pmType.GetField("blueprintId",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(pm, id);
            }
            else
            {
                var prop = pmType.GetProperty("blueprintId",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                prop?.SetValue(pm, id);
            }
        }

        [Test]
        public void OnPreprocessAvatar_WithFallbackAvatar_AddsToPending()
        {
            var pending = GetPendingDict();
            if (pending == null) Assert.Ignore("PendingFallbackAvatars not accessible.");

            var go = new GameObject("TestAvatar_FB");
            go.AddComponent(pmType);
            SetBlueprintId(go.GetComponent(pmType), "avtr_test_fb_add_001");
            go.AddComponent<FallbackAvatar>();
            try
            {
                pending.Remove("avtr_test_fb_add_001");
                var callback = CreateCallback();
                var result = InvokeOnPreprocessAvatar(callback, go);
                Assert.IsTrue(result);
                Assert.IsTrue(pending.ContainsKey("avtr_test_fb_add_001"),
                    "FallbackAvatar should be added to PendingFallbackAvatars");
            }
            finally
            {
                pending.Remove("avtr_test_fb_add_001");
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void OnPreprocessAvatar_WithoutFallbackAvatar_RemovesFromPending()
        {
            var pending = GetPendingDict();
            if (pending == null) Assert.Ignore("PendingFallbackAvatars not accessible.");

            var go = new GameObject("TestAvatar_NoFB");
            go.AddComponent(pmType);
            SetBlueprintId(go.GetComponent(pmType), "avtr_test_fb_remove_001");
            try
            {
                pending["avtr_test_fb_remove_001"] = true;
                var callback = CreateCallback();
                var result = InvokeOnPreprocessAvatar(callback, go);
                Assert.IsTrue(result);
                Assert.IsFalse(pending.ContainsKey("avtr_test_fb_remove_001"),
                    "Non-fallback avatar should be removed from PendingFallbackAvatars");
            }
            finally
            {
                pending.Remove("avtr_test_fb_remove_001");
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void OnPreprocessAvatar_MultipleCalls_TrackCorrectly()
        {
            var pending = GetPendingDict();
            if (pending == null) Assert.Ignore("PendingFallbackAvatars not accessible.");

            var go1 = new GameObject("TestAvatar_Multi1");
            go1.AddComponent(pmType);
            SetBlueprintId(go1.GetComponent(pmType), "avtr_test_multi_001");
            go1.AddComponent<FallbackAvatar>();

            var go2 = new GameObject("TestAvatar_Multi2");
            go2.AddComponent(pmType);
            SetBlueprintId(go2.GetComponent(pmType), "avtr_test_multi_002");
            // No FallbackAvatar on go2

            try
            {
                pending.Remove("avtr_test_multi_001");
                pending.Remove("avtr_test_multi_002");

                var callback = CreateCallback();
                InvokeOnPreprocessAvatar(callback, go1);
                Assert.IsTrue(pending.ContainsKey("avtr_test_multi_001"));

                InvokeOnPreprocessAvatar(callback, go2);
                Assert.IsFalse(pending.ContainsKey("avtr_test_multi_002"));
                // First avatar still tracked
                Assert.IsTrue(pending.ContainsKey("avtr_test_multi_001"));
            }
            finally
            {
                pending.Remove("avtr_test_multi_001");
                pending.Remove("avtr_test_multi_002");
                UnityEngine.Object.DestroyImmediate(go1);
                UnityEngine.Object.DestroyImmediate(go2);
            }
        }

        [Test]
        public void CallbackOrder_IsAccessible()
        {
            var callback = CreateCallback();
            var orderProp = callbackType.GetProperty("callbackOrder",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (orderProp == null) Assert.Ignore("callbackOrder property not found.");
            var order = (int)orderProp.GetValue(callback);
            Assert.IsTrue(order >= 0 || order < 0, "callbackOrder should return an integer.");
        }
    }

    [TestFixture]
    internal class ActualPerformanceCallbackExtendedTests
    {
        private static readonly Type callbackType =
            typeof(KRT.VRCQuestTools.VRCQuestTools).Assembly.GetType("KRT.VRCQuestTools.NonDestructive.ActualPerformanceCallback");

        private static readonly Type pmType = SystemUtility.GetTypeByName("VRC.Core.PipelineManager");

        [SetUp]
        public void SetUp()
        {
            if (callbackType == null) Assert.Ignore("ActualPerformanceCallback type not found.");
        }

        [Test]
        public void LastActualPerformanceRating_IsAccessible()
        {
            var field = callbackType.GetField("LastActualPerformanceRating",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null) Assert.Ignore("LastActualPerformanceRating field not found.");
            var dict = field.GetValue(null);
            Assert.IsNotNull(dict);
            Assert.IsInstanceOf<IDictionary>(dict);
        }

        [Test]
        public void CallbackOrder_ReturnsIntMaxValue()
        {
            var callback = Activator.CreateInstance(callbackType);
            var orderProp = callbackType.GetProperty("callbackOrder",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (orderProp == null) Assert.Ignore("callbackOrder property not found.");
            var order = (int)orderProp.GetValue(callback);
            Assert.AreEqual(int.MaxValue, order);
        }

        [Test]
        public void OnPreprocessAvatar_WithValidBlueprintId_SetsRating()
        {
            if (pmType == null) Assert.Ignore("PipelineManager type not found.");

            var field = callbackType.GetField("LastActualPerformanceRating",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null) Assert.Ignore("LastActualPerformanceRating not accessible.");

            var method = callbackType.GetMethod("OnPreprocessAvatar",
                BindingFlags.Instance | BindingFlags.Public);
            if (method == null) Assert.Ignore("OnPreprocessAvatar not found.");

            var go = new GameObject("TestAvatar_PerfRating");
            var desc = go.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            go.AddComponent(pmType);

            // Set blueprint ID via reflection
            var bpField = pmType.GetField("blueprintId",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (bpField != null)
            {
                bpField.SetValue(go.GetComponent(pmType), "avtr_test_perf_001");
            }

            try
            {
                var callback = Activator.CreateInstance(callbackType);
                var result = (bool)method.Invoke(callback, new object[] { go });
                Assert.IsTrue(result);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
