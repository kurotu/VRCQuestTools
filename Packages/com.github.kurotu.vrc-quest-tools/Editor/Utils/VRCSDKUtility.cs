// <copyright file="VRCSDKUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KRT.VRCQuestTools.Models.VRChat;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase;

#if VQT_HAS_VRCSDK_BASE
using VRC.SDKBase.Validation.Performance;
using VRC.SDKBase.Validation.Performance.Stats;
using AvatarPerformanceStatsLevelSet = VRC.SDKBase.Validation.Performance.Stats.AvatarPerformanceStatsLevelSet;
using VRC_AvatarDescriptor = VRC.SDKBase.VRC_AvatarDescriptor;
#else
using AvatarPerformance = KRT.VRCQuestTools.Mocks.Mock_AvatarPerformance;
using AvatarPerformanceStats = KRT.VRCQuestTools.Mocks.Mock_AvatarPerformanceStats;
using AvatarPerformanceStatsLevel = KRT.VRCQuestTools.Mocks.Mock_AvatarPerformanceStatsLevel;
using AvatarPerformanceStatsLevelSet = KRT.VRCQuestTools.Mocks.Mock_AvatarPerformanceStatsLevelSet;
using PerformanceRating = KRT.VRCQuestTools.Mocks.Mock_PerformanceRating;
using VRC_AvatarDescriptor = KRT.VRCQuestTools.Mocks.Mock_VRC_AvatarDescriptor;
#endif

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// VRCSDK Utility.
    /// </summary>
    internal static class VRCSDKUtility
    {
        /// <summary>
        /// Limit for Quest Poor.
        /// </summary>
        internal const int PoorPhysBonesCountLimit = 8;

        /// <summary>
        /// Limit for Quest Poor.
        /// </summary>
        internal const int PoorPhysBoneCollidersCountLimit = 16;

        /// <summary>
        /// Limit for Quest Poor.
        /// </summary>
        internal const int PoorContactsCountLimit = 16;

        /// <summary>
        /// AvatarPerformanceCategory for Avatar Dynamics.
        /// </summary>
        internal static readonly AvatarPerformanceCategory[] AvatarDynamicsPerformanceCategories =
        {
            AvatarPerformanceCategory.PhysBoneComponentCount,
            AvatarPerformanceCategory.PhysBoneTransformCount,
            AvatarPerformanceCategory.PhysBoneColliderCount,
            AvatarPerformanceCategory.PhysBoneCollisionCheckCount,
            AvatarPerformanceCategory.ContactCount,
        };

        /// <summary>
        /// Type object of VRCPhysBone.
        /// </summary>
        internal static readonly System.Type PhysBoneType = SystemUtility.GetTypeByName("VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone");

        /// <summary>
        /// Type object of VRCPhysBoneCollider.
        /// </summary>
        internal static readonly System.Type PhysBoneColliderType = SystemUtility.GetTypeByName("VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBoneCollider");

        /// <summary>
        /// Type object of VRCContactReceiver.
        /// </summary>
        internal static readonly System.Type ContactReceiverType = SystemUtility.GetTypeByName("VRC.SDK3.Dynamics.Contact.Components.VRCContactReceiver");

        /// <summary>
        /// Type object of VRCContactSender.
        /// </summary>
        internal static readonly System.Type ContactSenderType = SystemUtility.GetTypeByName("VRC.SDK3.Dynamics.Contact.Components.VRCContactSender");

        private const string VpmSdk3DemoFolder = "Packages/com.vrchat.avatars/Samples/AV3 Demo Assets";

        /// <summary>
        /// Types which is not allowed for Quest avatars. (except FinalIK).
        /// </summary>
        private static readonly System.Type[] UnsupportedComponentTypes = new System.Type[]
        {
            SystemUtility.GetTypeByName("DynamicBoneColliderBase"), SystemUtility.GetTypeByName("DynamicBone"), // DynamicBone may be missing
            typeof(Cloth),
            typeof(Camera),
            typeof(Light),
            typeof(VRC_SpatialAudioSource),
            typeof(AudioSource),
            typeof(Joint), typeof(Rigidbody), typeof(Collider),
            typeof(UnityEngine.Animations.IConstraint),
        }.Where(e => e != null).ToArray();

        private static readonly Regex VpmSdk3ProxyAnimPattern = new Regex($"{VpmSdk3DemoFolder}/Animation/ProxyAnim/.*\\.anim", RegexOptions.Compiled);
        private static readonly Regex VpmSdk3DemoPattern = new Regex($"{VpmSdk3DemoFolder}/.*", RegexOptions.Compiled);
        private static readonly Regex VpmBetaSdk3ProxyAnimPattern = new Regex("Assets/Samples/VRChat SDK - Avatars/.*/AV3 Demo Assets/Animation/ProxyAnim/.*\\.anim", RegexOptions.Compiled);
        private static readonly Regex VpmBetaSdk3DemoPattern = new Regex("Assets/Samples/VRChat SDK - Avatars/.*/AV3 Demo Assets/.*", RegexOptions.Compiled);

        /// <summary>
        /// Whether the game object is a VRC avatar root.
        /// </summary>
        /// <param name="obj">Target object.</param>
        /// <returns>true when the object has VRC_AvatarDescriptor.</returns>
        internal static bool IsAvatarRoot(GameObject obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetComponent<VRC_AvatarDescriptor>() == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Whether the animation clips is a proxy animation.
        /// </summary>
        /// <param name="animationClip">Animation clip.</param>
        /// <returns>true when the clips exists in proxy animation folder.</returns>
        internal static bool IsProxyAnimationClip(AnimationClip animationClip)
        {
            var path = AssetDatabase.GetAssetPath(animationClip);

            // VPM SDK3
            if (VpmSdk3ProxyAnimPattern.IsMatch(path))
            {
                return true;
            }

            // VPM beta SDK3
            if (VpmBetaSdk3ProxyAnimPattern.IsMatch(path))
            {
                return true;
            }

            // SDK3
            if (path.StartsWith("Assets/VRCSDK/Examples3/Animation/ProxyAnim/"))
            {
                return true;
            }

            // SDK2
            if (path.StartsWith("Assets/VRChat Examples/Examples2/Animation/"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Whether the asset object is an example from VRCSDK.
        /// </summary>
        /// <param name="obj">Asset object.</param>
        /// <returns>true when the object exists in VRCSDK examples folder.</returns>
        internal static bool IsExampleAsset(UnityEngine.Object obj)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            return IsExampleAsset(path);
        }

        /// <summary>
        /// Whether the asset path is an example from VRCSDK.
        /// </summary>
        /// <param name="path">Asset path.</param>
        /// <returns>true when the path exists in VRCSDK examples folder.</returns>
        internal static bool IsExampleAsset(string path)
        {
            // VPM SDK3
            if (VpmSdk3DemoPattern.IsMatch(path))
            {
                return true;
            }

            // VPM beta SDK3
            if (VpmBetaSdk3DemoPattern.IsMatch(path))
            {
                return true;
            }

            // SDK3
            return path.StartsWith("Assets/VRCSDK/Examples3/");
        }

        /// <summary>
        /// Whether a component type is unsupported for Quest.
        /// </summary>
        /// <param name="type">Compoent type to check.</param>
        /// <returns>true when unsupported.</returns>
        internal static bool IsUnsupportedComponentType(System.Type type)
        {
            var unsupported = UnsupportedComponentTypes.FirstOrDefault(t =>
            {
                return t.IsAssignableFrom(type);
            }) != null;
            if (unsupported)
            {
                return true;
            }

            return FinalIKUtility.IsFinalIKComponent(type);
        }

        /// <summary>
        /// Remove "Missing" script components.
        /// </summary>
        /// <param name="gameObject">Target object.</param>
        /// <param name="includeInactive">Whether to include inactive objects.</param>
        internal static void RemoveMissingComponentsInChildren(GameObject gameObject, bool includeInactive)
        {
            var children = gameObject.GetComponentsInChildren<Transform>(includeInactive).Select(t => t.gameObject);
            foreach (var c in children)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(c);
            }
        }

        /// <summary>
        /// Get game objects which have "Missing" script components.
        /// </summary>
        /// <param name="gameObject">Target object.</param>
        /// <param name="includeInactive">Whether to incldue inactive objects.</param>
        /// <returns>Game objects which have "Missing" script components.</returns>
        internal static GameObject[] GetGameObjectsWithMissingComponents(GameObject gameObject, bool includeInactive)
        {
            var children = gameObject.GetComponentsInChildren<Transform>(includeInactive).Select(t => t.gameObject);
            return children.Where(c => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(c) > 0).ToArray();
        }

        /// <summary>
        /// Count number of "Missing" script components.
        /// </summary>
        /// <param name="gameObject">Target object.</param>
        /// <param name="includeInactive">Whether to incldue inactive objects.</param>
        /// <returns>Number of missing components.</returns>
        internal static int CountMissingComponentsInChildren(GameObject gameObject, bool includeInactive)
        {
            var children = gameObject.GetComponentsInChildren<Transform>(includeInactive).Select(t => t.gameObject);
            return children.Sum(c => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(c));
        }

        /// <summary>
        /// Gets avatar root objects from the scene.
        /// </summary>
        /// <param name="scene">Target scene.</param>
        /// <returns>Avatar root objects.</returns>
        internal static VRC_AvatarDescriptor[] GetAvatarsFromScene(Scene scene)
        {
            var avatars = new List<VRC_AvatarDescriptor>();
            var rootGameObjects = scene.GetRootGameObjects();
            foreach (var obj in rootGameObjects)
            {
                avatars.AddRange(obj.GetComponentsInChildren<VRC_AvatarDescriptor>());
            }
            return avatars.ToArray();
        }

        /// <summary>
        /// Whether the material is allowed for Quest avatar.
        /// </summary>
        /// <param name="material">Material.</param>
        /// <returns>true when the material is allowed.</returns>
        internal static bool IsMaterialAllowedForQuestAvatar(Material material)
        {
            var usableShaders = new string[]
            {
                "Standard Lite", "Bumped Diffuse", "Bumped Mapped Specular", "Diffuse",
                "MatCap Lit", "Toon Lit", "Particles/Additive", "Particles/Multiply",
            }.Select(s => $"VRChat/Mobile/{s}");
            return usableShaders.Contains(material.shader.name);
        }

        /// <summary>
        /// Whether VRCSDK has PhysBones.
        /// </summary>
        /// <returns>true when PhysBones exists in the project.</returns>
        internal static bool IsPhysBonesImported()
        {
            return PhysBoneType != null;
        }

        /// <summary>
        /// Gets whether VRCSDK is imported as a package.
        /// </summary>
        /// <returns>true when com.vrchat.avatars exists.</returns>
        internal static async Task<bool> IsImportedAsPackage()
        {
            var request = UnityEditor.PackageManager.Client.List(true, true);
            while (!request.IsCompleted)
            {
                await Task.Delay(100);
            }
            if (request.Status == UnityEditor.PackageManager.StatusCode.Success)
            {
                foreach (var package in request.Result)
                {
                    if (package.name == "com.vrchat.avatars")
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                throw new Exception($"Failed to list packages: {request.Error.message}");
            }
        }

        /// <summary>
        /// Gets whether the avatar has missing network ids.
        /// </summary>
        /// <param name="avatarDescriptor">Taget avatar.</param>
        /// <returns>true when the avatar has missing network ids.</returns>
        internal static bool HasMissingNetworkIds(VRC_AvatarDescriptor avatarDescriptor)
        {
#if VQT_HAS_VRCSDK_BASE
            var ids = avatarDescriptor.NetworkIDCollection;
            var pbs = avatarDescriptor.gameObject.GetComponentsInChildren(PhysBoneType, true);
            if (ids.Count == 0)
            {
                return pbs.Length > 0;
            }

            var pbNetId0 = pbs.FirstOrDefault((pb) =>
            {
                var missingInIds = ids.FirstOrDefault((id_pair) =>
                {
                    return id_pair.gameObject == pb.gameObject;
                }) == null;
                return missingInIds;
            }) != null;
            return pbNetId0;
#else
            return false;
#endif
        }

        /// <summary>
        /// Assigns network ids to PhysBones.
        /// </summary>
        /// <param name="avatarDescriptor">Target avatar.</param>
        /// <exception cref="NotImplementedException">VRCSDK dones't support Network IDs.</exception>
        internal static void AssignNetworkIdsToPhysBones(VRC_AvatarDescriptor avatarDescriptor)
        {
#if VQT_HAS_VRCSDK_BASE
            var ids = avatarDescriptor.NetworkIDCollection;
            var pbs = avatarDescriptor.GetComponentsInChildren(PhysBoneType, true)
                .Select(c => new Reflection.PhysBone(c))
                .OrderBy(pb => GetFullPathInHierarchy(pb.GameObject))
                .ToArray();
            var assignedIds = new HashSet<int>(ids.Select(oair => oair.ID));

            int id = 10;
            foreach (var pb in pbs)
            {
                while (assignedIds.Contains(id))
                {
                    id++;
                }
                var alreadyAssigned = ids.FirstOrDefault(pair => pair.gameObject == pb.GameObject) != null;
                if (!alreadyAssigned)
                {
                    var pair = new VRC.SDKBase.Network.NetworkIDPair();
                    pair.ID = id;
                    pair.gameObject = pb.GameObject;
                    avatarDescriptor.NetworkIDCollection.Add(pair);
                    assignedIds.Add(id);
                }
            }
            PrefabUtility.RecordPrefabInstancePropertyModifications(avatarDescriptor);
#else
            throw new NotImplementedException();
#endif
        }

        /// <summary>
        /// Deletes avatar dynamics components.
        /// </summary>
        /// <param name="avatar">Target avatar.</param>
        /// <param name="physBonesToKeep">PhysBones to keep.</param>
        /// <param name="physBoneCollidersToKeep">PhysBoneColliders to keep.</param>
        /// <param name="contactsToKeep">ContanctSenders and ContactReceivers to keep.</param>
        internal static void DeleteAvatarDynamicsComponents(VRChatAvatar avatar, VRCPhysBone[] physBonesToKeep, VRCPhysBoneCollider[] physBoneCollidersToKeep, ContactBase[] contactsToKeep)
        {
            foreach (var c in avatar.GetPhysBones().Except(physBonesToKeep))
            {
                var go = c.gameObject;
                Undo.DestroyObjectImmediate(c);
                PrefabUtility.RecordPrefabInstancePropertyModifications(go);
            }
            foreach (var c in avatar.GetPhysBoneColliders().Except(physBoneCollidersToKeep))
            {
                var physbones = avatar.GetPhysBones();

                // Remove reference from PhysBone before destroying.
                for (var boneIndex = 0; boneIndex < physbones.Length; boneIndex++)
                {
                    var boneObject = physbones[boneIndex];
                    var boneComponent = new Reflection.PhysBone(boneObject);
                    for (var colliderIndex = 0; colliderIndex < boneComponent.Colliders.Count; colliderIndex++)
                    {
                        if (boneComponent.Colliders[colliderIndex] == c)
                        {
                            boneComponent.ClearCollider(colliderIndex);
                        }
                    }
                    PrefabUtility.RecordPrefabInstancePropertyModifications(boneObject);
                }
                var go = c.gameObject;
                Undo.DestroyObjectImmediate(c);
                PrefabUtility.RecordPrefabInstancePropertyModifications(go);
            }
            foreach (var c in avatar.GetContacts().Except(contactsToKeep))
            {
                var go = c.gameObject;
                Undo.DestroyObjectImmediate(c);
                PrefabUtility.RecordPrefabInstancePropertyModifications(go);
            }

            StripeUnusedNetworkIds(avatar.AvatarDescriptor);
            PrefabUtility.RecordPrefabInstancePropertyModifications(avatar.GameObject);
        }

        /// <summary>
        /// Stripes unused network ids from the avatar.
        /// </summary>
        /// <param name="avatarDescriptor">Target avatar.</param>
        /// <exception cref="NotImplementedException">VRCSDK dones't support Network IDs.</exception>
        internal static void StripeUnusedNetworkIds(VRC_AvatarDescriptor avatarDescriptor)
        {
#if VQT_HAS_VRCSDK_BASE
            for (var i = 0; i < avatarDescriptor.NetworkIDCollection.Count; i++)
            {
                var pair = avatarDescriptor.NetworkIDCollection[i];
                if (pair.gameObject == null || pair.gameObject.GetComponent(PhysBoneType) == null)
                {
                    avatarDescriptor.NetworkIDCollection.RemoveAt(i);
                    i--;
                }
            }
#else
            throw new NotImplementedException();
#endif
        }

        /// <summary>
        /// Gets the full path of the game object in the hierarchy.
        /// </summary>
        /// <param name="gameObject">Target object.</param>
        /// <returns>Full path which begins /.</returns>
        internal static string GetFullPathInHierarchy(GameObject gameObject)
        {
            var go = gameObject;
            var path = $"/{go.name}";
            while (go.transform.parent != null)
            {
                path = $"/{go.transform.parent.name}/{path}";
                go = go.transform.parent.gameObject;
            }
            return path;
        }

        /// <summary>
        /// Loads AvatarPerformanceStatsLevelSet.
        /// </summary>
        /// <param name="isMobile">Whether loads mobile set.</param>
        /// <returns>StatsLevelSet.</returns>
        internal static AvatarPerformanceStatsLevelSet LoadAvatarPerformanceStatsLevelSet(bool isMobile)
        {
#if !VQT_HAS_VRCSDK_BASE
            throw new InvalidOperationException("VRCSDK3 is not imported.");
#endif
            var guid = isMobile
                ? "f0f530dea3891c04e8ab37831627e702" // AvatarPerformanceStatLevels_Quest.asset
                : "438f83f183e95f740877d4c22ed91af2"; // AvatarPerformanceStatLevels_Windows.asset
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (path == string.Empty)
            {
                path = isMobile
                    ? "Packages/com.vrchat.base/Runtime/VRCSDK/Dependencies/VRChat/Resources/Validation/Performance/StatsLevels/Quest/AvatarPerformanceStatLevels_Quest.asset"
                    : "Packages/com.vrchat.base/Runtime/VRCSDK/Dependencies/VRChat/Resources/Validation/Performance/StatsLevels/Windows/AvatarPerformanceStatLevels_Windows.asset";
                Debug.LogWarning($"[{VRCQuestTools.Name}] Failed to find AvatarPerformanceStatLevelSet by GUID. Using {path}.");
            }
            var statsLevelSet = AssetDatabase.LoadAssetAtPath<AvatarPerformanceStatsLevelSet>(path);
            if (statsLevelSet == null)
            {
                throw new InvalidOperationException($"Failed to load AvatarPerformanceStatLevelSet from {path}");
            }
            return statsLevelSet;
        }

        /// <summary>
        /// Loads the icon of the performance rating.
        /// </summary>
        /// <param name="rating">Rating.</param>
        /// <returns>Icon texture.</returns>
        internal static Texture2D LoadPerformanceIcon(PerformanceRating rating)
        {
            var guid = string.Empty;
            switch (rating)
            {
                case PerformanceRating.None:
                    throw new InvalidOperationException();
                case PerformanceRating.Excellent:
                    guid = "644caf5607820c7418cf0d248b12f33b";
                    break;
                case PerformanceRating.Good:
                    guid = "4109d4977ddfb6548b458318e220ac70";
                    break;
                case PerformanceRating.Medium:
                    guid = "9296abd40c7c1934cb668aae07b41c69";
                    break;
                case PerformanceRating.Poor:
                    guid = "e561d0406779ab948b7f155498d101ee";
                    break;
                case PerformanceRating.VeryPoor:
                    guid = "2886eb1248200a94d9eaec82336fbbad";
                    break;
            }
            var path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        /// <summary>
        /// Calculate performance stats of the avatar.
        /// </summary>
        /// <param name="gameObject">Target GameObject.</param>
        /// <param name="isMobile">Whether the platform is mobile.</param>
        /// <returns>Calculated stats.</returns>
        internal static AvatarPerformanceStats CalculatePerformanceStats(GameObject gameObject, bool isMobile)
        {
            var stats = new AvatarPerformanceStats(isMobile);
            AvatarPerformance.CalculatePerformanceStats(gameObject.name, gameObject, stats, isMobile);
            return stats;
        }

        /// <summary>
        /// Reflection to use VRCSDK features.
        /// </summary>
        internal class Reflection
        {
            /// <summary>
            /// Reflection wrapper for VRCPhysBone.
            /// </summary>
            internal class PhysBone
            {
                private static readonly FieldInfo RootTransformField = PhysBoneType?.GetField("rootTransform");
                private static readonly FieldInfo IgnoreTransformsField = PhysBoneType?.GetField("ignoreTransforms");
                private static readonly FieldInfo EndpointPositionField = PhysBoneType?.GetField("endpointPosition");
                private static readonly FieldInfo MultiChildTypeField = PhysBoneType?.GetField("multiChildType");
                private static readonly FieldInfo CollidersField = PhysBoneType?.GetField("colliders");
                private readonly Component component;

                /// <summary>
                /// Initializes a new instance of the <see cref="PhysBone"/> class.
                /// </summary>
                /// <param name="component">Component to wrap.</param>
                internal PhysBone(Component component)
                {
                    this.component = component;
                }

                /// <summary>
                /// VRCPhysBoneBase.MultiChildType.
                /// </summary>
                internal enum MultiChildTypeEnum
                {
                    /// <summary>
                    /// VRCPhysBoneBase.MultiChildType.Ignore.
                    /// </summary>
                    Ignore,

                    /// <summary>
                    /// VRCPhysBoneBase.MultiChildType.First.
                    /// </summary>
                    First,

                    /// <summary>
                    /// VRCPhysBoneBase.MultiChildType.Average.
                    /// </summary>
                    Average,
                }

                /// <summary>
                /// Gets gameObject of the component.
                /// </summary>
                internal GameObject GameObject => component.gameObject;

                /// <summary>
                /// Gets root tansform set by inspector.
                /// </summary>
                internal Transform RootTransform => (Transform)RootTransformField.GetValue(component);

                /// <summary>
                /// Gets ignore transforms set by inspector.
                /// </summary>
                internal List<Transform> IgnoreTransforms => (List<Transform>)IgnoreTransformsField.GetValue(component);

                /// <summary>
                /// Gets endpoint position set by inspector.
                /// </summary>
                internal Vector3 EndpointPosition => (Vector3)EndpointPositionField.GetValue(component);

                /// <summary>
                /// Gets multi child type set by inspector.
                /// </summary>
                internal MultiChildTypeEnum MultiChildType => (MultiChildTypeEnum)MultiChildTypeField.GetValue(component);

                /// <summary>
                /// Gets PhysBoneCollider instances.
                /// </summary>
                internal List<Component> Colliders
                {
                    get
                    {
                        var colliders = CollidersField.GetValue(component);
                        dynamic[] c = Enumerable.ToArray((dynamic)colliders);
                        return c.Cast<Component>().ToList();
                    }
                }

                /// <summary>
                /// Sets null to PhysBoneCollider at index.
                /// </summary>
                /// <param name="index">index to set null.</param>
                internal void ClearCollider(int index)
                {
                    var colliders = CollidersField.GetValue(component);
                    ((dynamic)colliders)[index] = null;
                }
            }

            /// <summary>
            /// Reflection wrapper for VRCPhysBoneCollider.
            /// </summary>
            internal class PhysBoneCollider
            {
                /// <summary>
                /// Gets the component object of PhysBoneCollider.
                /// </summary>
                internal readonly Component Component;
                private static readonly FieldInfo RootTransformField = PhysBoneColliderType?.GetField("rootTransform");

                /// <summary>
                /// Initializes a new instance of the <see cref="PhysBoneCollider"/> class.
                /// </summary>
                /// <param name="component">Component to wrap.</param>
                internal PhysBoneCollider(Component component)
                {
                    this.Component = component;
                }

                /// <summary>
                /// Gets root tansform set by inspector.
                /// </summary>
                internal Transform RootTransform => (Transform)RootTransformField.GetValue(Component);
            }

            /// <summary>
            /// Reflection wrapper for ContactBase.
            /// </summary>
            internal class ContactBase
            {
                private static readonly System.Type ContactBaseType = SystemUtility.GetTypeByName("VRC.Dynamics.ContactBase");
                private static readonly FieldInfo RootTransformField = ContactBaseType?.GetField("rootTransform");
                private readonly Component component;

                /// <summary>
                /// Initializes a new instance of the <see cref="ContactBase"/> class.
                /// </summary>
                /// <param name="component">Component to wrap.</param>
                internal ContactBase(Component component)
                {
                    this.component = component;
                }

                /// <summary>
                /// Gets gameObject of the component.
                /// </summary>
                internal GameObject GameObject => component.gameObject;

                /// <summary>
                /// Gets root tansform set by inspector.
                /// </summary>
                internal Transform RootTransform => (Transform)RootTransformField.GetValue(component);
            }
        }
    }
}
