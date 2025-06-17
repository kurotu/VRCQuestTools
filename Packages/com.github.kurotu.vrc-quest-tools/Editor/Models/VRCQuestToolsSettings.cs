// <copyright file="VRCQuestToolsSettings.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.IO;
using KRT.VRCQuestTools.I18n;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Settings store for VRCQuestTools.
    /// </summary>
    internal static class VRCQuestToolsSettings
    {
        private const string FALSE = "FALSE";
        private const string TRUE = "TRUE";
        private const string ProjectSettingsFile = "ProjectSettings/VRCQuestToolsSettings.json";
        private const ulong DefaultTextureCacheSize = 1024 * 1024 * 1024; // 1GB
        private static readonly string DefaultTextureCacheDirectory = Path.Combine(SystemUtility.GetAppLocalCachePath(VRCQuestTools.Name), "TextureCache");

        private static I18nBase i18n = null;

        /// <summary>
        /// Gets I18nBase object currently referencing.
        /// </summary>
        internal static I18nBase I18nResource
        {
            get
            {
                if (i18n == null)
                {
                    i18n = I18n.I18n.GetI18n();
                }
                return i18n;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show UnitySettings window..
        /// </summary>
        internal static bool IsShowUnitySettingsWindowOnLoadEnabled
        {
            get { return GetBooleanConfigValue(Keys.ShowSettingsOnLoad, true); }
            set { SetBooleanConfigValue(Keys.ShowSettingsOnLoad, value); }
        }

        /// <summary>
        /// Gets or sets the cached latest version.
        /// </summary>
        internal static SemVer LatestVersionCache
        {
            get { return new SemVer(EditorUserSettings.GetConfigValue(Keys.LatestVersionCache) ?? "0.0.0"); }
            set { EditorUserSettings.SetConfigValue(Keys.LatestVersionCache, value.ToString()); }
        }

        /// <summary>
        /// Gets or sets the skipped version.
        /// </summary>
        internal static SemVer SkippedVersion
        {
            get
            {
                var str = EditorUserSettings.GetConfigValue(Keys.SkippedVersion) ?? "0.0.0";
                return new SemVer(str);
            }

            set
            {
                var str = value.ToString();
                EditorUserSettings.SetConfigValue(Keys.SkippedVersion, str);
            }
        }

        /// <summary>
        /// Gets or sets the last date time which the version checker checked.
        /// </summary>
        internal static DateTime LastVersionCheckDateTime
        {
            get
            {
                var unixTime = int.Parse(EditorUserSettings.GetConfigValue(Keys.LastVersionCheckData) ?? "0");
                return UnixEpoch.AddSeconds(unixTime);
            }

            set
            {
                var date = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
                var unixTime = (int)date.Subtract(UnixEpoch).TotalSeconds;
                EditorUserSettings.SetConfigValue(Keys.LastVersionCheckData, unixTime.ToString());
            }
        }

        /// <summary>
        /// Gets or sets display language.
        /// </summary>
        internal static DisplayLanguage DisplayLanguage
        {
            get
            {
                var value = EditorUserSettings.GetConfigValue(Keys.DisplayLanguage);
                if (value == null)
                {
                    return DisplayLanguage.Auto;
                }

                var result = Enum.TryParse(value, out DisplayLanguage language);
                if (result == false)
                {
                    return DisplayLanguage.Auto;
                }

                return language;
            }

            set
            {
                var language = Enum.GetName(typeof(DisplayLanguage), value);
                EditorUserSettings.SetConfigValue(Keys.DisplayLanguage, language);
                i18n = I18n.I18n.GetI18n();
            }
        }

        /// <summary>
        /// Gets or sets the total size of texture cache.
        /// </summary>
        internal static ulong TextureCacheSize
        {
            get
            {
                var value = EditorPrefs.GetString(Keys.TextureCacheSize);
                if (ulong.TryParse(value, out ulong size))
                {
                    return size;
                }

                TextureCacheSize = DefaultTextureCacheSize;
                return DefaultTextureCacheSize;
            }

            set
            {
                EditorPrefs.SetString(Keys.TextureCacheSize, value.ToString());
            }
        }

        /// <summary>
        /// Gets or sets the directory path of texture cache.
        /// </summary>
        internal static string TextureCacheFolder
        {
            get
            {
                var path = EditorPrefs.GetString(Keys.TextureCacheDirectory).Trim();
                if (string.IsNullOrEmpty(path))
                {
                    return DefaultTextureCacheDirectory;
                }
                return path;
            }

            set
            {
                EditorPrefs.SetString(Keys.TextureCacheDirectory, value.Trim());
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether ValidationAutomator is enabled.
        /// </summary>
        internal static bool IsValidationAutomatorEnabled
        {
            get { return GetBooleanConfigValue(Keys.ValidationAutomatorEnabled, true); }
            set { SetBooleanConfigValue(Keys.ValidationAutomatorEnabled, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether texture format check is enabled on standalone builds.
        /// </summary>
        internal static bool IsCheckTextureFormatOnStandaloneEnabled
        {
            get { return GetBooleanConfigValue(Keys.CheckTextureFormatOnStandalone, false); }
            set { SetBooleanConfigValue(Keys.CheckTextureFormatOnStandalone, value); }
        }

        private static DateTime UnixEpoch => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Resets all preferences to default.
        /// </summary>
        internal static void ResetPreferences()
        {
            TextureCacheSize = DefaultTextureCacheSize;
            TextureCacheFolder = DefaultTextureCacheDirectory;
        }

        private static void SetBooleanConfigValue(string name, bool value)
        {
            var v = value ? TRUE : FALSE;
            EditorUserSettings.SetConfigValue(name, v);
        }

        private static bool GetBooleanConfigValue(string name, bool defaultValue)
        {
            var d = defaultValue ? TRUE : FALSE;
            return (EditorUserSettings.GetConfigValue(name) ?? d) == TRUE;
        }

        private static VRCQuestToolsProjectSettings GetProjectSettings()
        {
            if (!File.Exists(ProjectSettingsFile))
            {
                var settings = new VRCQuestToolsProjectSettings();

                // Migrate AutoRemoveVertexColors from EditorUserSettings to ProjectSettings/
                if (EditorUserSettings.GetConfigValue(Keys.AutoRemoveVertexColors) != null)
                {
                    settings.AutoRemoveVertexColors = GetBooleanConfigValue(Keys.AutoRemoveVertexColors, true);
                }

                SaveProjectSettings(settings);
            }
            var json = File.ReadAllText(ProjectSettingsFile);
            return JsonUtility.FromJson<VRCQuestToolsProjectSettings>(json);
        }

        private static void SaveProjectSettings(VRCQuestToolsProjectSettings settings)
        {
            var json = JsonUtility.ToJson(settings, true);
            File.WriteAllText(ProjectSettingsFile, json);
        }

        private static class Keys
        {
            internal const string LatestVersionCache = PREFIX + "LatestVersionCache";
            internal const string ShowSettingsOnLoad = PREFIX + "ShowSettingsOnLoad";
            internal const string AutoRemoveVertexColors = PREFIX + "AutoRemoveVertexColors";
            internal const string SkippedVersion = PREFIX + "SkippedVersion";
            internal const string LastVersionCheckData = PREFIX + "LastVersionCheckDate";
            internal const string DisplayLanguage = PREFIX + "DisplayLanguage";
            internal const string TextureCacheSize = PREFIX + "TextureCacheSize";
            internal const string TextureCacheDirectory = PREFIX + "TextureCacheDirectory";
            internal const string ValidationAutomatorEnabled = PREFIX + "ValidationAutomatorEnabled";
            internal const string CheckTextureFormatOnStandalone = PREFIX + "CheckTextureFormatOnStandalone";
            private const string PREFIX = "dev.kurotu.VRCQuestTools.";
        }
    }
}
