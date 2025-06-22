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
        public const string Version = "2.11.0";

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
        /// VPM service instance.
        /// </summary>
        internal static VPMService VPM = new VPMService();

        /// <summary>
        /// VPM repository URL.
        /// </summary>
        internal static string VPMRepositoryURL = "https://kurotu.github.io/vpm-repos/vpm.json";

        /// <summary>
        /// Documentation URL.
        /// </summary>
        internal static string DocsURL = "https://kurotu.github.io/VRCQuestTools/";

        /// <summary>
        /// package name.
        /// </summary>
        internal static string PackageName = "com.github.kurotu.vrc-quest-tools";

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

        private static void PrintProjectInfo()
        {
#if VQT_HAS_VRCSDK_BASE
            Debug.Log($"[{Name}] VCC project. (com.vrchat.base is imported)");
#elif VRC_SDK_VRCSDK3
            Debug.LogError($"[{Name}] Legacy VRCSDK3 is no longer supported. (com.vrchat.avatars 3.3.0 or later is missing in Packages)");
#elif VRC_SDK_VRCSDK2
            Debug.LogError($"[{Name}] VRCSDK2 is no longer supported. (VRC_SDK_VRCSDK2 is defined)");
#endif
        }
    }
}
