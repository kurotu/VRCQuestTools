using System;
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
    }
}
