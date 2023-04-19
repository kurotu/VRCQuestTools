// <copyright file="VRCQuestTools.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.IO;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Services;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEngine;

namespace KRT.VRCQuestTools
{
    /// <summary>
    /// VRCQuestTools configuration.
    /// </summary>
    [InitializeOnLoad]
    public static class VRCQuestTools
    {
        /// <summary>
        /// Tool name.
        /// </summary>
        public const string Name = "VRCQuestTools";

        /// <summary>
        /// VRCQuestTools version (semver).
        /// </summary>
        public const string Version = "1.10.1";

        /// <summary>
        /// Days to delay update notification.
        /// </summary>
        internal const int DaysToDelayUpdateNotification = 1;

        /// <summary>
        /// GitHub repository (username/reponame).
        /// </summary>
        internal const string GitHubRepository = "kurotu/VRCQuestTools";

        /// <summary>
        /// Booth URL.
        /// </summary>
        internal const string BoothURL = "https://kurotu.booth.pm/items/2436054";

        /// <summary>
        /// ComponentRemover object to use in application.
        /// </summary>
        internal static Models.ComponentRemover ComponentRemover = new Models.ComponentRemover();

        /// <summary>
        /// AvatarConverter object to use in application.
        /// </summary>
        internal static AvatarConverter AvatarConverter = new AvatarConverter(new MaterialWrapperBuilder());

        /// <summary>
        /// GitHub API service instance.
        /// </summary>
        internal static GitHubService GitHub = new GitHubService(GitHubRepository);

        private const string PackageJsonGUID = "a965857078462df4a879e07cb70812bb";

        static VRCQuestTools()
        {
            PrintProjectInfo();
        }

        /// <summary>
        /// Gets root folder in Assets.
        /// </summary>
        internal static string AssetRoot => Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(PackageJsonGUID));

        /// <summary>
        /// Gets a value indicating whether VRCQuestTools is imported as a package.
        /// </summary>
        internal static bool IsImportedAsPackage => AssetRoot.StartsWith("Packages");

        /// <summary>
        /// Export as .unitypackage for release.
        /// </summary>
        /// <param name="filename">File to export.</param>
        public static void ExportUnityPackage(string filename)
        {
            AssetDatabase.ExportPackage(AssetRoot, filename, ExportPackageOptions.Recurse);
        }

        private static void Export()
        {
            ExportUnityPackage("VRCQuestTools.unitypackage");
        }

        private static async void PrintProjectInfo()
        {
#if VRC_SDK_VRCSDK3
            Debug.Log($"[{Name}] VRCSDK3 project. (VRC_SDK_VRCSDK3 is defined)");
            var isPackage = await VRCSDKUtility.IsImportedAsPackage();
            if (!isPackage)
            {
                Debug.LogWarning($"[{Name}] Legacy VRCSDK3 is no longer supported. Functionalities may be broken in future. (com.vrchat.avatars is missing in Packages)");
            }
#elif VRC_SDK_VRCSDK2
            Debug.LogWarning($"[{Name}] VRCSDK2 is no longer supported. Functionalities may be broken in future. (VRC_SDK_VRCSDK2 is defined)");
#endif

#if UNITY_2018
            Debug.LogWarning($"[{Name}] Unity 2018 is no longer supported. Functionalities may be broken in future. (UNITY_2018 is defined)");
#endif
        }
    }
}
