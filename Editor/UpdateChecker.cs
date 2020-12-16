// <copyright file="UpdateChecker.cs" company="kurotu">
// Copyright (c) kurotu.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

using UnityEditor;
using UnityEngine;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace KRT.VRCQuestTools
{
    static class UpdateChecker
    {

        internal static void CheckForUpdateFromMenu()
        {
            var t = Task.Run(CheckForUpdate);
            t.Wait();
            if (t.IsFaulted)
            {
                Debug.LogError(t.Exception.Message);
            }
        }

        private const string githubURL = "https://github.com/kurotu/VRCQuestTools";
        private static readonly HttpClient client = new HttpClient();
        private static SynchronizationContext unityContext = SynchronizationContext.Current;

        async static void CheckForUpdate()
        {
            var url = $"{githubURL}/raw/master/package.json";
            var result = await client.GetStringAsync(url);
            var latestVersion = GetVersion(result);
            var currentVersion = new SemVer(VRCQuestTools.Version);
            Debug.Log($"[VRCQuestTools] Current: {currentVersion}, Latest: {latestVersion}");
            if (latestVersion > currentVersion)
            {
                Debug.Log($"[VRCQuestTools] Update available");
                unityContext.Post(o =>
                {
                    var version = (SemVer)o;
                    Debug.Log("Open");
                    if (EditorUtility.DisplayDialog("VRCQuestTools", $"Update available: {version}", "Update", "Cancel"))
                    {
                        Application.OpenURL(githubURL);
                    }
                }, latestVersion);
            }
        }

        static SemVer GetVersion(string packageJsonString)
        {
            var package = JsonUtility.FromJson<PackageJson>(packageJsonString);
            return new SemVer(package.version);
        }
    }

    [System.Serializable]
    class PackageJson
    {
        public string version;
    }

    class SemVer
    {
        readonly int major;
        readonly int minor;
        readonly int patch;

        public SemVer(string version)
        {
            var part = version.Split('-');
            var split = part[0].Split('.');
            major = int.Parse(split[0]);
            minor = int.Parse(split[1]);
            patch = int.Parse(split[2]);
        }

        public int Compare(object x, object y)
        {
            throw new System.NotImplementedException();
        }

        public static bool operator >(SemVer a, SemVer b)
        {
            if (a.major > b.major) return true;
            if (a.major == b.major && a.minor > b.minor) return true;
            if (a.major == b.major && a.minor == b.minor && a.patch > b.patch) return true;
            return false;
        }

        public static bool operator <(SemVer a, SemVer b)
        {
            if (a.major < b.major) return true;
            if (a.major == b.major && a.minor < b.minor) return true;
            if (a.major == b.major && a.minor == b.minor && a.patch < b.patch) return true;
            return false;
        }

        public override string ToString()
        {
            return $"{major}.{minor}.{patch}";
        }
    }
}
