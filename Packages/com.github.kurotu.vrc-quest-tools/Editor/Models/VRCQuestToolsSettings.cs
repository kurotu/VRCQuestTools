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

        /// <summary>
        /// Upper bound accepted for <see cref="TextureCacheSize"/> (1TB). Guards against a value large enough
        /// to overflow the byte arithmetic in the settings UI, and against a nonsensical limit reached by
        /// hand-editing the settings file.
        /// </summary>
        internal const ulong MaxTextureCacheSize = 1024UL * 1024 * 1024 * 1024;

        /// <summary>
        /// Texture cache size default used before <see cref="VRCQuestToolsProjectSettings.CurrentSettingsVersion"/>
        /// 1 (128MB). Kept only to recognize stored values that came from that default rather than from a
        /// deliberate choice; see <see cref="MigrateProjectSettings"/>.
        /// </summary>
        private const ulong LegacyDefaultTextureCacheSize = 128UL * 1024 * 1024;

        private static readonly string DefaultTextureCacheDirectory = Path.Combine("Library", "VRCQuestTools", "Cache", "TextureCache");

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
        /// Gets or sets the total size of texture cache. Clamped to <see cref="MaxTextureCacheSize"/> on both
        /// read and write, so neither a caller nor a hand-edited settings file can install a limit that breaks
        /// eviction.
        /// </summary>
        internal static ulong TextureCacheSize
        {
            get
            {
                var settings = GetProjectSettings();
                return ClampTextureCacheSize(settings.TextureCacheSize);
            }

            set
            {
                var settings = GetProjectSettings();
                settings.TextureCacheSize = ClampTextureCacheSize(value);
                SaveProjectSettings(settings);
            }
        }

        /// <summary>
        /// Gets the directory path of texture cache.
        /// </summary>
        internal static string TextureCacheFolder
        {
            get
            {
                return DefaultTextureCacheDirectory;
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

        /// <summary>
        /// Gets or sets a value indicating whether debug logging is enabled.
        /// </summary>
        internal static bool IsDebugLogEnabled
        {
            get { return GetBooleanConfigValue(Keys.DebugLogEnabled, false); }
            set { SetBooleanConfigValue(Keys.DebugLogEnabled, value); }
        }

        private static DateTime UnixEpoch => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Resets all preferences to default.
        /// </summary>
        internal static void ResetPreferences()
        {
            var settings = GetProjectSettings();
            settings.TextureCacheSize = VRCQuestToolsProjectSettings.DefaultTextureCacheSize;
            SaveProjectSettings(settings);
        }

        private static ulong ClampTextureCacheSize(ulong size)
        {
            return size > MaxTextureCacheSize ? MaxTextureCacheSize : size;
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
            var loaded = JsonUtility.FromJson<VRCQuestToolsProjectSettings>(json);
            if (MigrateProjectSettings(loaded))
            {
                SaveProjectSettings(loaded);
            }
            return loaded;
        }

        /// <summary>
        /// Brings settings written by an older schema version up to date.
        /// </summary>
        /// <param name="settings">Settings just loaded from the settings file.</param>
        /// <returns>true when something changed and the settings should be written back.</returns>
        private static bool MigrateProjectSettings(VRCQuestToolsProjectSettings settings)
        {
            if (settings.SettingsVersion >= VRCQuestToolsProjectSettings.CurrentSettingsVersion)
            {
                return false;
            }

            // Adopt the new, larger texture cache default for anyone who never chose a size themselves.
            // GetProjectSettings writes the whole settings file the first time anything reads a setting, so an
            // unversioned file always carries a texture cache size even when the user never opened the settings
            // UI -- meaning "stored value == the old default" cannot be told apart from "user deliberately
            // picked the old default", and is treated as the former. Any other value is left untouched.
            if (settings.TextureCacheSize == LegacyDefaultTextureCacheSize)
            {
                settings.TextureCacheSize = VRCQuestToolsProjectSettings.DefaultTextureCacheSize;
            }

            return true;
        }

        private static void SaveProjectSettings(VRCQuestToolsProjectSettings settings)
        {
            // Stamped here rather than by each caller so every write records the schema the file was written
            // with, which is what keeps MigrateProjectSettings a one-time operation.
            settings.SettingsVersion = VRCQuestToolsProjectSettings.CurrentSettingsVersion;
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
            internal const string DebugLogEnabled = PREFIX + "DebugLogEnabled";
            private const string PREFIX = "dev.kurotu.VRCQuestTools.";
        }
    }
}
