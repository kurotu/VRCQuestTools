// <copyright file="LoggerTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Tests for <see cref="Logger"/>.
    /// </summary>
    public class LoggerTests
    {
        /// <summary>
        /// Test Log outputs message with prefix.
        /// </summary>
        [Test]
        public void Log_OutputsMessageWithPrefix()
        {
            LogAssert.Expect(LogType.Log, "[VRCQuestTools] test message");
            Logger.Log("test message");
        }

        /// <summary>
        /// Test LogWarning outputs warning with prefix.
        /// </summary>
        [Test]
        public void LogWarning_OutputsWarningWithPrefix()
        {
            LogAssert.Expect(LogType.Warning, "[VRCQuestTools] warning message");
            Logger.LogWarning("warning message");
        }

        /// <summary>
        /// Test LogError outputs error with prefix.
        /// </summary>
        [Test]
        public void LogError_OutputsErrorWithPrefix()
        {
            LogAssert.Expect(LogType.Error, "[VRCQuestTools] error message");
            Logger.LogError("error message");
        }

        /// <summary>
        /// Test LogException logs the exception.
        /// </summary>
        [Test]
        public void LogException_LogsException()
        {
            var exception = new System.Exception("test exception");
            LogAssert.Expect(LogType.Exception, "Exception: test exception");
            Logger.LogException(exception);
        }

        /// <summary>
        /// Test LogDebug when UseDebug is true outputs message.
        /// </summary>
        [Test]
        public void LogDebug_WhenEnabled_OutputsMessage()
        {
            var original = Logger.UseDebug;
            try
            {
                Logger.UseDebug = true;
                LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(@"\[VRCQuestTools\] \(debug\).*debug message"));
                Logger.LogDebug("debug message");
            }
            finally
            {
                Logger.UseDebug = original;
            }
        }

        /// <summary>
        /// Test LogDebug when UseDebug is false does not output message.
        /// </summary>
        [Test]
        public void LogDebug_WhenDisabled_DoesNotOutput()
        {
            var original = Logger.UseDebug;
            try
            {
                Logger.UseDebug = false;
                Logger.LogDebug("should not appear");
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                Logger.UseDebug = original;
            }
        }

        /// <summary>
        /// Test Log with context object.
        /// </summary>
        [Test]
        public void Log_WithContext_DoesNotThrow()
        {
            var go = new GameObject("TestLogContext");
            try
            {
                LogAssert.Expect(LogType.Log, "[VRCQuestTools] context test");
                Assert.DoesNotThrow(() => Logger.Log("context test", go));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
