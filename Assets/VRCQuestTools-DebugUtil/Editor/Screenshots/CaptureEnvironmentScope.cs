using System;
using KRT.VRCQuestTools.Models;

namespace KRT.VRCQuestTools.Debug.Screenshots
{
    /// <summary>
    /// Forces a deterministic capture environment (English display language, no "update available"
    /// notification banner) for the duration of the scope, restoring the developer's original
    /// settings on Dispose, even if an exception occurs.
    /// </summary>
    internal sealed class CaptureEnvironmentScope : IDisposable
    {
        private readonly DisplayLanguage originalLanguage;
        private readonly SemVer originalSkippedVersion;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CaptureEnvironmentScope"/> class.
        /// </summary>
        internal CaptureEnvironmentScope()
        {
            originalLanguage = VRCQuestToolsSettings.DisplayLanguage;
            originalSkippedVersion = VRCQuestToolsSettings.SkippedVersion;

            VRCQuestToolsSettings.DisplayLanguage = DisplayLanguage.English;

            // VRCQuestToolsEditorOnlyEditorBase<T> draws an "update available" banner whenever
            // LatestVersionCache > SkippedVersion. That cache is machine/day dependent, so it
            // would make inspector screenshots non-reproducible. Skip straight to the cached
            // "latest" version for the duration of the capture.
            VRCQuestToolsSettings.SkippedVersion = VRCQuestToolsSettings.LatestVersionCache;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            VRCQuestToolsSettings.DisplayLanguage = originalLanguage;
            VRCQuestToolsSettings.SkippedVersion = originalSkippedVersion;
        }
    }
}
