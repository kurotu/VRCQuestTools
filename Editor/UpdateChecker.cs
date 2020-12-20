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

        private static readonly HttpClient client = new HttpClient();
        private static SynchronizationContext unityContext = SynchronizationContext.Current;

        static UpdateChecker()
        {
            EditorApplication.delayCall += DelayInit;
        }

        static void DelayInit()
        {
            EditorApplication.delayCall -= DelayInit;
            if (VRCQuestToolsSettings.LastVersionCheckDateTime.Subtract(System.DateTime.UtcNow).Days >= 7)
            {
                var skippedVersion = new SemVer(VRCQuestToolsSettings.SkippedVersion);
                var t = Task.Run(() => CheckForUpdate(skippedVersion));
                t.Wait();
                if (t.IsFaulted)
                {
                    Debug.LogError(t.Exception.Message);
                }
                else
                {
                    VRCQuestToolsSettings.LastVersionCheckDateTime = System.DateTime.UtcNow;
                }
            }
        }

        internal static void CheckForUpdateFromMenu()
        {
            var i18n = UpdateCheckerI18n.Create();
            var skippedVersion = new SemVer(VRCQuestToolsSettings.SkippedVersion);
            var t = Task.Run(() => CheckForUpdate(new SemVer("0.0.0")));
            t.Wait();
            if (t.IsFaulted)
            {
                Debug.LogError(t.Exception.Message);
            }
            else
            {
                if (!t.Result)
                {
                    EditorUtility.DisplayDialog("VRCQuestTools", i18n.ThereIsNoUpdate, "OK");
                }
                VRCQuestToolsSettings.LastVersionCheckDateTime = System.DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Check for update to github package.json
        /// </summary>
        /// <param name="skippedVersion"></param>
        /// <returns>true when there is applicable update.</returns>
        static async Task<bool> CheckForUpdate(SemVer skippedVersion)
        {
            var latestVersion = await GetLatestVersion();
            var currentVersion = isDebug ? new SemVer("0.0.0") : new SemVer(VRCQuestTools.Version);
            Debug.Log($"[VRCQuestTools] Current: {currentVersion}, Latest: {latestVersion}, Skipped: {skippedVersion.ToString()}");
            var hasApplicableUpdate = HasApplicableUpdate(currentVersion, latestVersion, skippedVersion);
            if (hasApplicableUpdate)
            {
                unityContext.Post(obj =>
                {
                    var version = (SemVer)obj;
                    UpdateCheckerWindow.Init(version);
                }, latestVersion);
            }
            return hasApplicableUpdate;
        }

        async static Task<SemVer> GetLatestVersion()
        {
            var url = $"{VRCQuestTools.GitHubURL}/raw/master/package.json";
            var result = await client.GetStringAsync(url);
            var package = JsonUtility.FromJson<PackageJson>(result);
            return new SemVer(package.version);
        }

        static bool HasApplicableUpdate(SemVer currentVersion, SemVer latestVersion, SemVer skippedVersion)
        {
            var baseVersion = currentVersion > skippedVersion ? currentVersion : skippedVersion;
            return latestVersion > baseVersion;
        }
    }

    class UpdateCheckerWindow : EditorWindow
    {
        string latestVersion = "0.0.0";
        readonly UpdateCheckerI18nBase i18n = UpdateCheckerI18n.Create();

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
                    Application.OpenURL(VRCQuestTools.BoothURL);
                    Close();
                }
                GUILayout.Space(8);
                if (GUILayout.Button(i18n.RemindMeLater))
                {
                    Close();
                }
                GUILayout.Space(8);
                if (GUILayout.Button(i18n.SkipThisVersion))
                {
                    VRCQuestToolsSettings.SkippedVersion = latestVersion.ToString();
                    Close();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    static class UpdateCheckerI18n
    {
        public static UpdateCheckerI18nBase Create()
        {
            if (System.Globalization.CultureInfo.CurrentCulture.Name == "ja-JP")
            {
                return new UpdateCheckerI18nJapanese();
            }
            else
            {
                return new UpdateCheckerI18nEnglish();
            }
        }
    }

    abstract class UpdateCheckerI18nBase
    {
        public abstract string RemindMeLater { get; }
        public abstract string GetUpdate { get; }
        public abstract string SkipThisVersion { get; }
        public abstract string NewVersionIsAvailable(string latestVersion);
        public abstract string ThereIsNoUpdate { get; }
    }

    class UpdateCheckerI18nEnglish : UpdateCheckerI18nBase
    {
        public override string RemindMeLater => "Remind me later";
        public override string GetUpdate => "Get update";
        public override string SkipThisVersion => "Skip this version";
        public override string NewVersionIsAvailable(string latestVersion) => $"VRCQuestTools {latestVersion} is available.";
        public override string ThereIsNoUpdate => "There is no update.";
    }

    class UpdateCheckerI18nJapanese : UpdateCheckerI18nBase
    {
        public override string RemindMeLater => "後で通知";
        public override string GetUpdate => "アップデート";
        public override string SkipThisVersion => "このバージョンをスキップ";
        public override string NewVersionIsAvailable(string latestVersion) => $"VRCQuestTools {latestVersion} が公開されています。";
        public override string ThereIsNoUpdate => "アップデートはありません。";
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
