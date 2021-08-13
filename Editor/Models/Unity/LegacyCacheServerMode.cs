// <copyright file="LegacyCacheServerMode.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace KRT.VRCQuestTools.Models.Unity
{
    /// <summary>
    /// Unity asset cache server mode.
    /// </summary>
    internal enum LegacyCacheServerMode
    {
        /// <summary>
        /// Local cache server.
        /// </summary>
        Local = 0,

        /// <summary>
        /// Remote cache server.
        /// </summary>
        Remote,

        /// <summary>
        /// Never use cache server.
        /// </summary>
        Disable,
    }
}
