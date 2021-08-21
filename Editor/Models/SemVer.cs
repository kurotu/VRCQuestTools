// <copyright file="SemVer.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Represents semantic versioning.
    /// </summary>
    internal class SemVer
    {
        private readonly int major;
        private readonly int minor;
        private readonly int patch;

        /// <summary>
        /// Initializes a new instance of the <see cref="SemVer"/> class.
        /// </summary>
        /// <param name="version">Version string.</param>
        internal SemVer(string version)
        {
            var part = version.TrimStart('v').Split('-');
            var split = part[0].Split('.');
            major = int.Parse(split[0]);
            minor = int.Parse(split[1]);
            patch = int.Parse(split[2]);
        }

        public static bool operator >(SemVer a, SemVer b)
        {
            if (a.major > b.major)
            {
                return true;
            }

            if (a.major == b.major && a.minor > b.minor)
            {
                return true;
            }

            if (a.major == b.major && a.minor == b.minor && a.patch > b.patch)
            {
                return true;
            }

            return false;
        }

        public static bool operator <(SemVer a, SemVer b)
        {
            if (a.major < b.major)
            {
                return true;
            }

            if (a.major == b.major && a.minor < b.minor)
            {
                return true;
            }

            if (a.major == b.major && a.minor == b.minor && a.patch < b.patch)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Make semver string.
        /// </summary>
        /// <returns>"major.minor.patch".</returns>
        public override string ToString()
        {
            return $"{major}.{minor}.{patch}";
        }
    }
}
