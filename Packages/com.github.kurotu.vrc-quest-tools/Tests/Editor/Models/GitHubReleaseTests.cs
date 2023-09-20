// <copyright file="GitHubReleaseTests.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Test <see cref="GitHubRelease"/>.
    /// </summary>
    public class GitHubReleaseTests
    {
        /// <summary>
        /// Test parse json.
        /// </summary>
        [Test]
        public void ParseJson()
        {
            var json = File.ReadAllText(Path.Combine(TestUtils.FixturesFolder, "GitHubRelease.json"));
            var release = JsonUtility.FromJson<GitHubRelease>(json);

            Assert.AreEqual("v1.0.0", release.tag_name);

            var date = new DateTime(2013, 2, 27, 19, 35, 32, DateTimeKind.Utc);
            Assert.AreEqual(TimeSpan.Zero, release.PublishedDateTime - date);
        }
    }
}
