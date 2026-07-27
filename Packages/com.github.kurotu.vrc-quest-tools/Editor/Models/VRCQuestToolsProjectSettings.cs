// <copyright file="VRCQuestToolsProjectSettings.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Settings which should be stored in ProjectSettings folder for source control.
    /// </summary>
    [Serializable]
    internal class VRCQuestToolsProjectSettings
    {
        /// <summary>
        /// Schema version written by the current implementation. Bump this when a stored value needs a
        /// migration in <see cref="VRCQuestToolsSettings"/>.
        /// </summary>
        internal const int CurrentSettingsVersion = 1;

        /// <summary>
        /// Default texture cache size in bytes (1GB).
        /// </summary>
        /// <remarks>
        /// Deliberately generous: the folder lives under <c>Library/</c> (regenerable, not source controlled),
        /// while a single cache entry ranges from well under a megabyte (1024x1024 ASTC 6x6) to several megabytes
        /// (2048x2048 ASTC 4x4). A limit that only holds a dozen or so entries would evict an avatar's textures
        /// before they are ever reused, spending disk I/O for no cache hits at all.
        /// </remarks>
        internal const ulong DefaultTextureCacheSize = 1024UL * 1024 * 1024;

        /// <summary>
        /// Schema version of the loaded settings. 0 means the settings were written before versioning was
        /// introduced, which drives the one-time migrations in <see cref="VRCQuestToolsSettings"/>.
        /// </summary>
        public int SettingsVersion = 0;

        /// <summary>
        /// Enable Auto Remove Vertex Colors.
        /// </summary>
        public bool AutoRemoveVertexColors = true;

        /// <summary>
        /// Texture cache size in bytes.
        /// </summary>
        public ulong TextureCacheSize = DefaultTextureCacheSize;
    }
}
