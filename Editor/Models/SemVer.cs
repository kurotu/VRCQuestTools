// <copyright file="SemVer.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Represents semantic versioning.
    /// </summary>
    [Serializable]
    internal class SemVer
    {
        [SerializeField]
        private readonly int major;
        [SerializeField]
        private readonly int minor;
        [SerializeField]
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

        /// <summary>
        /// Initializes a new instance of the <see cref="SemVer"/> class.
        /// </summary>
        /// <param name="major">Major.</param>
        /// <param name="minor">Minor.</param>
        /// <param name="patch">Patch.</param>
        internal SemVer(int major, int minor, int patch)
        {
            this.major = major;
            this.minor = minor;
            this.patch = patch;
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

        public static bool operator >=(SemVer a, SemVer b)
        {
            if (a > b)
            {
                return true;
            }
            return a.HasSameVersion(b);
        }

        public static bool operator <=(SemVer a, SemVer b)
        {
            if (a < b)
            {
                return true;
            }
            return a.HasSameVersion(b);
        }

        /// <summary>
        /// Make semver string.
        /// </summary>
        /// <returns>"major.minor.patch".</returns>
        public override string ToString()
        {
            return $"{major}.{minor}.{patch}";
        }

        /// <summary>
        /// Gets whether the version is a major update against to old one.
        /// </summary>
        /// <param name="old">An old version to compare.</param>
        /// <returns>true when the major version is larger than old one.</returns>
        internal bool IsMajorUpdate(SemVer old)
        {
            if (this > old)
            {
                return major > old.major;
            }

            return false;
        }

        private bool HasSameVersion(SemVer a)
        {
            return ToString() == a.ToString();
        }
    }
}
