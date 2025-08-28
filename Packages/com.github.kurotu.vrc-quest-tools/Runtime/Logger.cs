using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// Logger for VRCQuestTools.
    /// </summary>
    public static class Logger
    {
        private const string Prefix = "[VRCQuestTools] ";

        /// <summary>
        /// Gets or sets a value indicating whether to use debug logging.
        /// </summary>
        public static bool UseDebug { get; set; }

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="context">The context object for the log entry.</param>
        [HideInCallstack]
        public static void Log(object message, UnityEngine.Object context = null)
        {
            Debug.Log(Prefix + message, context);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The warning message to log.</param>
        /// <param name="context">The context object for the log entry.</param>
        [HideInCallstack]
        public static void LogWarning(object message, UnityEngine.Object context = null)
        {
            Debug.LogWarning(Prefix + message, context);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        /// <param name="context">The context object for the log entry.</param>
        [HideInCallstack]
        public static void LogError(object message, UnityEngine.Object context = null)
        {
            Debug.LogError(Prefix + message, context);
        }

        /// <summary>
        /// Logs an exception.
        /// </summary>
        /// <remarks>
        /// Unlike other logging methods, this does <b>not</b> add the "[VRCQuestTools] " prefix,
        /// as it directly calls <see cref="Debug.LogException(Exception, UnityEngine.Object)"/>,
        /// which handles exception formatting differently and does not accept a string message.
        /// </remarks>
        /// <param name="exception">The exception to log.</param>
        /// <param name="context">The context object for the log entry.</param>
        [HideInCallstack]
        public static void LogException(Exception exception, UnityEngine.Object context = null)
        {
            Debug.LogException(exception, context);
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">The debug message to log.</param>
        /// <param name="context">The context object for the log entry.</param>
        /// <param name="filePath">The source file path of the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <param name="memberName">The name of the calling member.</param>
        [HideInCallstack]
        public static void LogDebug(
            object message,
            UnityEngine.Object context = null,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string memberName = "")
        {
            if (UseDebug)
            {
                var fileName = Path.GetFileName(filePath);
                Log($"(debug) [{fileName}:{lineNumber}] {memberName}: {message}", context);
            }
        }
    }
}
