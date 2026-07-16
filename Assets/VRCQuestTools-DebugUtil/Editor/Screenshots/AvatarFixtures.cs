using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace KRT.VRCQuestTools.Debug.Screenshots
{
    /// <summary>
    /// Provides temporary avatar instances from test fixtures for windows that require a target avatar.
    /// </summary>
    internal static class AvatarFixtures
    {
        private const string SimpleCubeAvatarPath = "Assets/VRCQuestTools-Tests/Fixtures/Prefabs/SimpleCubeAvatar.prefab";

        /// <summary>
        /// Instantiates the SimpleCubeAvatar fixture prefab into a hidden temporary GameObject.
        /// The caller is responsible for destroying the returned instance.
        /// </summary>
        /// <returns>A temporary avatar instance.</returns>
        internal static GameObject InstantiateSimpleCubeAvatar()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SimpleCubeAvatarPath);
            if (prefab == null)
            {
                throw new FileNotFoundException($"Fixture prefab not found: {SimpleCubeAvatarPath}");
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
            return instance;
        }

        /// <summary>
        /// Instantiates the SimpleCubeAvatar fixture with a sample PhysBone, PhysBoneCollider, and a
        /// non-local ContactReceiver added as children, so the Remove PhysBones window shows real
        /// entries instead of "not found" placeholders. The caller is responsible for destroying the
        /// returned instance (its children are destroyed along with it).
        /// </summary>
        /// <returns>A temporary avatar instance with sample avatar dynamics components.</returns>
        internal static GameObject InstantiateSimpleCubeAvatarWithDynamics()
        {
            var avatar = InstantiateSimpleCubeAvatar();

            var hair = new GameObject("Hair") { hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave };
            hair.transform.SetParent(avatar.transform);
            hair.AddComponent<VRCPhysBone>();
            hair.AddComponent<VRCPhysBoneCollider>();

            var sensor = new GameObject("Sensor") { hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave };
            sensor.transform.SetParent(avatar.transform);
            var contact = sensor.AddComponent<ContactReceiver>();

            // ContactReceiver defaults to localOnly = true, which the Remove PhysBones window
            // excludes from its "Non-Local Contact" section. IsLocalOnly has no public setter, so
            // the serialized field must be flipped through SerializedObject.
            var so = new SerializedObject(contact);
            var localOnlyProperty = so.FindProperty("localOnly");
            if (localOnlyProperty == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ContactReceiver)}'s serialized \"localOnly\" field was not found. The VRChat SDK's internal layout may have changed.");
            }

            localOnlyProperty.boolValue = false;
            so.ApplyModifiedProperties();

            return avatar;
        }
    }
}
