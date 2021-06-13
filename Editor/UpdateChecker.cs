// <copyright file="UpdateChecker.cs" company="kurotu">
// Copyright (c) kurotu.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    [InitializeOnLoad]
    static class UpdateChecker
    {
        private const bool isDebug = false;

        private static readonly string GitHubRepo = "kurotu/VRCQuestTools";
        internal static readonly string BoothURL = "https://booth.pm/ja/items/2436054";

        private static readonly HttpClient client = new HttpClient();

        static UpdateChecker()
        {
            client.DefaultRequestHeaders.Add("User-Agent", "VRCQuestTools");
            EditorApplication.delayCall += DelayInit;
        }

        static void DelayInit()
        {
            EditorApplication.delayCall -= DelayInit;
            var dateDiff = System.DateTime.UtcNow.Subtract(VRCQuestToolsSettings.LastVersionCheckDateTime);
            if (isDebug && dateDiff.Seconds < 1)
            {
                return;
            }
            if (!isDebug && dateDiff.Days < 1)
            {
                return;
            }
            var context = SynchronizationContext.Current;
            var t = Task.Run(async () =>
            {
                var latestVersion = await GetLatestVersion();
                var currentVersion = isDebug ? new SemVer("0.0.0") : new SemVer(VRCQuestTools.Version);
                Debug.Log($"[VRCQuestTools] Current: {currentVersion}, Latest: {latestVersion}");
                if (latestVersion > currentVersion)
                {
                    Debug.LogWarning($"[VRCQuestTools] New version is available: {latestVersion} See {BoothURL}");
                }
                context.Post(state =>
                {
                    VRCQuestToolsSettings.LastVersionCheckDateTime = System.DateTime.UtcNow;
                }, null);
            });
        }

        internal static void CheckForUpdateFromMenu()
        {
            var t = Task.Run(GetLatestVersion);
            t.Wait();
            if (t.IsFaulted)
            {
                Debug.LogError(t.Exception.Message);
                return;
            }

            var currentVersion = isDebug ? new SemVer("0.0.0") : new SemVer(VRCQuestTools.Version);
            var latestVersion = t.Result;
            Debug.Log($"[VRCQuestTools] Current: {currentVersion}, Latest: {latestVersion}");
            if (latestVersion > currentVersion)
            {
                UpdateCheckerWindow.Init(latestVersion);
            }
            else
            {
                var i18n = I18n.GetI18n();
                EditorUtility.DisplayDialog("VRCQuestTools", i18n.ThereIsNoUpdate, "OK");
            }
            VRCQuestToolsSettings.LastVersionCheckDateTime = System.DateTime.UtcNow;
        }

        async static Task<SemVer> GetLatestVersion()
        {
            var url = $"https://api.github.com/repos/{GitHubRepo}/releases/latest";
            var result = await client.GetStringAsync(url);
            var package = JsonUtility.FromJson<GitHubRelease>(result);
            return new SemVer(package.tag_name);
        }
    }

    class UpdateCheckerWindow : EditorWindow
    {
        string latestVersion = "0.0.0";
        readonly I18nBase i18n = I18n.GetI18n();

        [MenuItem("VRCQuestTools/Debug")]
        internal static void Init(SemVer latestVersion)
        {
            var window = GetWindow<UpdateCheckerWindow>();
            window.latestVersion = latestVersion.ToString();
            window.Show();
        }

        private void OnGUI()
        {
            titleContent.text = "VRCQuestTools Updater";
            EditorGUILayout.LabelField(i18n.NewVersionIsAvailable(latestVersion));
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(i18n.GetUpdate))
                {
                    Application.OpenURL(UpdateChecker.BoothURL);
                }
                GUILayout.Space(8);
                if (GUILayout.Button(i18n.CheckLater))
                {
                    Close();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    [System.Serializable]
    class GitHubRelease
    {
        public string tag_name = null;
    }

    class SemVer
    {
        readonly int major;
        readonly int minor;
        readonly int patch;

        public SemVer(string version)
        {
            var part = version.TrimStart('v').Split('-');
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
