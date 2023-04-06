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
    internal class SemVer : IComparable
    {
        [SerializeField]
        private readonly int major;
        [SerializeField]
        private readonly int minor;
        [SerializeField]
        private readonly int patch;
        [SerializeField]
        private readonly string prerelease = string.Empty;

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
            if (part.Length > 1)
            {
                prerelease = part[1];
            }
            else
            {
                prerelease = string.Empty;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SemVer"/> class.
        /// </summary>
        /// <param name="major">Major.</param>
        /// <param name="minor">Minor.</param>
        /// <param name="patch">Patch.</param>
        /// <param name="prerelease">Prerelease.</param>
        internal SemVer(int major, int minor, int patch, string prerelease = "")
        {
            this.major = major;
            this.minor = minor;
            this.patch = patch;
            this.prerelease = prerelease;
        }

        /// <summary>
        /// Gets a value indicating whether the version is a prerelease.
        /// </summary>
        internal bool IsPrerelease => !string.IsNullOrEmpty(prerelease);

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

            if (a.major == b.major && a.minor == b.minor && a.patch == b.patch)
            {
                if (!a.IsPrerelease && b.IsPrerelease)
                {
                    return true;
                }
                if (a.IsPrerelease && b.IsPrerelease && a.prerelease.CompareTo(b.prerelease) > 0)
                {
                    return true;
                }
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

            if (a.major == b.major && a.minor == b.minor && a.patch == b.patch)
            {
                if (a.IsPrerelease && !b.IsPrerelease)
                {
                    return true;
                }
                if (a.IsPrerelease && b.IsPrerelease && a.prerelease.CompareTo(b.prerelease) < 0)
                {
                    return true;
                }
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
            var str = $"{major}.{minor}.{patch}";
            if (IsPrerelease)
            {
                return str + "-" + prerelease;
            }
            return str;
        }

        /// <summary>
        /// Implementation of IComparable.
        /// </summary>
        /// <param name="obj">Other value.</param>
        /// <returns>1 when this is later, 0 when same, -1 when this is earlier.</returns>
        /// <exception cref="ArgumentException">Object is not a SemVer.</exception>
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            var other = obj as SemVer;
            if (other == null)
            {
                throw new ArgumentException("Object is not a SemVer");
            }
            if (this > other)
            {
                return 1;
            }
            if (this < other)
            {
                return -1;
            }
            return 0;
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
