// <copyright file="VRCQuestToolsSettings.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.IO;
using KRT.VRCQuestTools.I18n;
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
        /// Gets or sets a value indicating whether to enable VertexColorRemoverAutomator.
        /// </summary>
        internal static bool IsVertexColorRemoverAutomatorEnabled
        {
            get
            {
                return GetProjectSettings().AutoRemoveVertexColors;
            }

            set
            {
                var settings = GetProjectSettings();
                settings.AutoRemoveVertexColors = value;
                SaveProjectSettings(settings);
            }
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

        private static DateTime UnixEpoch => new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

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
            internal const string LastVersion = PREFIX + "LastQuestToolsVersion";
            internal const string ShowSettingsOnLoad = PREFIX + "ShowSettingsOnLoad";
            internal const string AutoRemoveVertexColors = PREFIX + "AutoRemoveVertexColors";
            internal const string SkippedVersion = PREFIX + "SkippedVersion";
            internal const string LastVersionCheckData = PREFIX + "LastVersionCheckDate";
            internal const string DisplayLanguage = PREFIX + "DisplayLanguage";
            private const string PREFIX = "dev.kurotu.VRCQuestTools.";
        }
    }
}
