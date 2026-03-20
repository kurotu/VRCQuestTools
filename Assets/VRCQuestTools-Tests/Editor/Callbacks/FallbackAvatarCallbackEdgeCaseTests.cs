// Additional tests for FallbackAvatarCallback covering:
// - OnPreprocessAvatar with null PipelineManager
// - OnPreprocessAvatar with empty blueprintId
// - OnSdkPanelEnable idempotency (already initialized)
// - OnPreprocessAvatar overwrite behavior for same blueprintId

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
    internal class FallbackAvatarCallbackEdgeCaseTests
    {
        private static readonly Type callbackType =
            typeof(KRT.VRCQuestTools.VRCQuestTools).Assembly.GetType("KRT.VRCQuestTools.NonDestructive.FallbackAvatarCallback");

        private static readonly Type pmType = SystemUtility.GetTypeByName("VRC.Core.PipelineManager");
        private MethodInfo onPreprocessAvatarMethod;
        private FieldInfo pendingField;

        [SetUp]
        public void SetUp()
        {
            if (callbackType == null)
            {
                Assert.Ignore("FallbackAvatarCallback type not found.");
            }

            if (pmType == null)
            {
                Assert.Ignore("PipelineManager type not found.");
            }

            onPreprocessAvatarMethod = callbackType.GetMethod("OnPreprocessAvatar",
                BindingFlags.Instance | BindingFlags.Public);
            if (onPreprocessAvatarMethod == null)
            {
                Assert.Ignore("OnPreprocessAvatar method not found.");
            }

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
            if (pendingField == null)
            {
                return null;
            }

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
        public void OnPreprocessAvatar_NoPipelineManager_ReturnsTrue()
        {
            var go = new GameObject("TestAvatar_NoPM");
            try
            {
                var callback = CreateCallback();
                var result = InvokeOnPreprocessAvatar(callback, go);
                Assert.IsTrue(result, "Should return true when no PipelineManager");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void OnPreprocessAvatar_EmptyBlueprintId_ReturnsTrue()
        {
            var go = new GameObject("TestAvatar_EmptyBP");
            go.AddComponent(pmType);
            SetBlueprintId(go.GetComponent(pmType), string.Empty);
            try
            {
                var callback = CreateCallback();
                var result = InvokeOnPreprocessAvatar(callback, go);
                Assert.IsTrue(result, "Should return true when blueprintId is empty");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void OnPreprocessAvatar_NullBlueprintId_ReturnsTrue()
        {
            var go = new GameObject("TestAvatar_NullBP");
            go.AddComponent(pmType);
            SetBlueprintId(go.GetComponent(pmType), null);
            try
            {
                var callback = CreateCallback();
                var result = InvokeOnPreprocessAvatar(callback, go);
                Assert.IsTrue(result, "Should return true when blueprintId is null");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void OnPreprocessAvatar_FallbackThenNoFallback_RemovesFromPending()
        {
            var pending = GetPendingDict();
            if (pending == null)
            {
                Assert.Ignore("PendingFallbackAvatars not accessible.");
            }

            var go = new GameObject("TestAvatar_Toggle");
            go.AddComponent(pmType);
            SetBlueprintId(go.GetComponent(pmType), "avtr_test_toggle_001");
            var fb = go.AddComponent<FallbackAvatar>();

            try
            {
                pending.Remove("avtr_test_toggle_001");
                var callback = CreateCallback();

                // First call with FallbackAvatar
                InvokeOnPreprocessAvatar(callback, go);
                Assert.IsTrue(pending.ContainsKey("avtr_test_toggle_001"));

                // Remove FallbackAvatar
                UnityEngine.Object.DestroyImmediate(fb);

                // Second call without FallbackAvatar
                InvokeOnPreprocessAvatar(callback, go);
                Assert.IsFalse(pending.ContainsKey("avtr_test_toggle_001"));
            }
            finally
            {
                pending.Remove("avtr_test_toggle_001");
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void OnPreprocessAvatar_SameBlueprintTwice_OverwritesPending()
        {
            var pending = GetPendingDict();
            if (pending == null)
            {
                Assert.Ignore("PendingFallbackAvatars not accessible.");
            }

            var go = new GameObject("TestAvatar_Double");
            go.AddComponent(pmType);
            SetBlueprintId(go.GetComponent(pmType), "avtr_test_double_001");
            go.AddComponent<FallbackAvatar>();

            try
            {
                pending.Remove("avtr_test_double_001");
                var callback = CreateCallback();

                InvokeOnPreprocessAvatar(callback, go);
                Assert.IsTrue(pending.ContainsKey("avtr_test_double_001"));

                // Call again - should not throw
                InvokeOnPreprocessAvatar(callback, go);
                Assert.IsTrue(pending.ContainsKey("avtr_test_double_001"));
            }
            finally
            {
                pending.Remove("avtr_test_double_001");
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void OnPreprocessAvatar_AlwaysReturnsTrue()
        {
            var go = new GameObject("TestAvatar_AlwaysTrue");
            go.AddComponent(pmType);
            SetBlueprintId(go.GetComponent(pmType), "avtr_test_always_001");
            go.AddComponent<FallbackAvatar>();

            try
            {
                var callback = CreateCallback();
                Assert.IsTrue(InvokeOnPreprocessAvatar(callback, go));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
