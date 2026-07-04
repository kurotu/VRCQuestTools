using System.Collections.Generic;
using System.Linq;
using KRT.VRCQuestTools.I18n;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.VRChat;
using KRT.VRCQuestTools.Services;
using KRT.VRCQuestTools.Utils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase.Validation.Performance.Stats;
#pragma warning disable CS0618

namespace KRT.VRCQuestTools.Views
{
    /// <summary>
    /// Editor window for selecting avatar dynamics components to keep.
    /// </summary>
    internal class AvatarDynamicsSelectorWindow : EditorWindow, ISerializationCallbackReceiver
    {
        [SerializeField]
        private GameObject avatarRootGameObject;

        /// <summary>
        /// PhysBone providers to keep.
        /// Providers are not serializable, so the underlying components are serialized in
        /// <see cref="serializedPhysBonesToKeep"/> and providers are rebuilt on enable.
        /// </summary>
        internal VRCPhysBoneProviderBase[] physBoneProvidersToKeep = { };

        /// <summary>
        /// PhysBoneColliders to keep.
        /// </summary>
        [SerializeField]
        internal VRCPhysBoneCollider[] physBoneCollidersToKeep = { };

        /// <summary>
        /// ContactSenders & ContactReceivers to keep.
        /// </summary>
        [SerializeField]
        internal VRC.Dynamics.ContactBase[] contactsToKeep = { };

        [SerializeField]
        private List<Component> serializedPhysBonesToKeep = new List<Component>();

        [SerializeField]
        private Vector2 scrollPosition = Vector2.zero;
        [SerializeField]
        private bool foldoutPhysBones = true;
        [SerializeField]
        private bool foldoutPhysBonesColliders = true;
        [SerializeField]
        private bool foldoutContacts = true;
        [SerializeField]
        private bool foldoutEstimatedPerformance = true;

        private GUIStyle foldoutContentStyle;
        private I18nBase i18n;
        private AvatarPerformanceStatsLevelSet statsLevelSet;

        // Per-group foldout states keyed by relative path label. Default: open (true).
        private Dictionary<string, bool> physBoneGroupFoldouts = new Dictionary<string, bool>();
        private Dictionary<string, bool> colliderGroupFoldouts = new Dictionary<string, bool>();
        private Dictionary<string, bool> contactGroupFoldouts = new Dictionary<string, bool>();

        /// <summary>
        /// AvatarConverterSettings to edit. Backed by a GameObject reference rather than the component
        /// directly: this component is editor-only and gets destroyed by NDMF's Play Mode avatar
        /// processing, so a direct component reference would be unrecoverable after a Play Mode round
        /// trip. The backing GameObject is not destroyed, so it is re-resolved live on every access.
        /// </summary>
        internal KRT.VRCQuestTools.Components.AvatarConverterSettings converterSettings
        {
            get => avatarRootGameObject != null ? avatarRootGameObject.GetComponent<KRT.VRCQuestTools.Components.AvatarConverterSettings>() : null;
            set => avatarRootGameObject = value != null ? value.gameObject : null;
        }

        /// <summary>
        /// Stores the underlying components of the PhysBone providers, which are not serializable themselves.
        /// </summary>
        public void OnBeforeSerialize()
        {
            serializedPhysBonesToKeep = physBoneProvidersToKeep
                .Where(p => p != null && p.Component != null)
                .Select(p => p.Component)
                .ToList();
        }

        /// <summary>
        /// Providers are rebuilt in OnEnable because the Unity API is not available during deserialization.
        /// </summary>
        public void OnAfterDeserialize()
        {
        }

        /// <summary>
        /// Applies avatar dynamics settings to PlatformComponentRemover components.
        /// Components not in the keep lists will be configured for Android removal.
        /// Also clears legacy physBonesToKeep fields on converterSettings.
        /// </summary>
        /// <param name="converterSettings">AvatarConverterSettings to update.</param>
        /// <param name="providersToKeep">PhysBone providers to keep.</param>
        /// <param name="collidersToKeep">PhysBoneColliders to keep.</param>
        /// <param name="contactsToKeep">Contacts to keep.</param>
        internal static void ApplyDynamicsSettings(
            KRT.VRCQuestTools.Components.AvatarConverterSettings converterSettings,
            VRCPhysBoneProviderBase[] providersToKeep,
            VRCPhysBoneCollider[] collidersToKeep,
            ContactBase[] contactsToKeep)
        {
            AvatarDynamicsSettingsUtility.Apply(converterSettings, providersToKeep, collidersToKeep, contactsToKeep);
        }

        /// <summary>
        /// Applies avatar dynamics settings to PlatformComponentRemover components.
        /// Components not in the keep lists will be configured for Android removal.
        /// </summary>
        /// <param name="avatarDescriptor">Target avatar descriptor.</param>
        /// <param name="providersToKeep">PhysBone providers to keep.</param>
        /// <param name="collidersToKeep">PhysBoneColliders to keep.</param>
        /// <param name="contactsToKeep">Contacts to keep.</param>
        internal static void ApplyDynamicsSettings(
            VRC.SDKBase.VRC_AvatarDescriptor avatarDescriptor,
            VRCPhysBoneProviderBase[] providersToKeep,
            VRCPhysBoneCollider[] collidersToKeep,
            ContactBase[] contactsToKeep)
        {
            AvatarDynamicsSettingsUtility.Apply(avatarDescriptor, providersToKeep, collidersToKeep, contactsToKeep);
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Avatar Dynamics Selector");
            wantsMouseMove = true;
            wantsMouseEnterLeaveWindow = true;
            foldoutContentStyle = new GUIStyle()
            {
                padding = new RectOffset(16, 0, 0, 0),
            };
            statsLevelSet = VRCSDKUtility.LoadAvatarPerformanceStatsLevelSet(true);
            AvatarDynamicsPreviewService.Initialize();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            RestorePhysBoneProviders();
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            AvatarDynamicsPreviewService.Cleanup();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                ResetSelectionAfterPlayMode();
            }
        }

        // Rebuilds PhysBone providers from the serialized components after a domain reload.
        private void RestorePhysBoneProviders()
        {
            if (physBoneProvidersToKeep.Length > 0 || serializedPhysBonesToKeep.Count == 0)
            {
                return;
            }
            if (converterSettings == null || converterSettings.AvatarDescriptor == null)
            {
                return;
            }
            var components = new HashSet<Component>(serializedPhysBonesToKeep.Where(c => c != null));
            physBoneProvidersToKeep = new VRChatAvatar(converterSettings.AvatarDescriptor)
                .GetPhysBoneProviders()
                .Where(p => components.Contains(p.Component))
                .ToArray();
        }

        // Re-derives the selection after returning from Play Mode, using the same safe default logic
        // as opening the window fresh (see AvatarConverterSettingsEditor.InitializeSelectorWindow).
        // The precise pre-play selection can't be preserved: NDMF's Play Mode avatar processing
        // destroys, and can merge/replace, the underlying PhysBone and Collider components while
        // testing, so any component reference captured before Play Mode still ends up dangling once
        // Unity remaps object references through the round trip. Falling back to the default
        // selection avoids silently ending up with an empty "keep" list, which would mark every
        // component for Android removal if Apply were pressed.
        private void ResetSelectionAfterPlayMode()
        {
            if (converterSettings == null || converterSettings.AvatarDescriptor == null)
            {
                return;
            }
            KRT.VRCQuestTools.Inspector.AvatarConverterSettingsEditor.InitializeSelectorWindow(this, converterSettings, converterSettings.AvatarDescriptor);
        }

        private void OnGUI()
        {
            i18n = VRCQuestToolsSettings.I18nResource;

            if (EditorApplication.isPlaying)
            {
                // The referenced AvatarConverterSettings is editor-only and gets destroyed by NDMF's
                // Play Mode avatar processing, so it looks "missing" while playing even though the
                // selection is preserved (see OnPlayModeStateChanged) and will reappear on returning
                // to Edit Mode.
                EditorGUILayout.HelpBox(i18n.ExitPlayModeToEdit, MessageType.Warning);
                return;
            }

            if (converterSettings == null)
            {
                EditorGUILayout.LabelField("Referenced AvatarConverter is missing.");
                if (GUILayout.Button(i18n.CloseLabel))
                {
                    Close();
                }
                return;
            }

            AvatarDynamicsPreviewService.BeginPreviewFrame(this);

            var avatar = new VRChatAvatar(converterSettings.AvatarDescriptor);
            var avatarRoot = converterSettings.AvatarDescriptor.gameObject;

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(i18n.AvatarLabel, converterSettings.AvatarDescriptor, typeof(VRC.SDKBase.VRC_AvatarDescriptor), true);
            }

            EditorGUILayout.LabelField(i18n.SelectComponentsToKeep, EditorStyles.wordWrappedLabel);
            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scrollView.scrollPosition;

                using (var foldout = new EditorGUIUtility.FoldoutHeaderGroupScope(foldoutPhysBones, new GUIContent("PhysBones", i18n.PhysBonesListTooltip)))
                {
                    foldoutPhysBones = foldout.Foldout;
                    if (foldoutPhysBones)
                    {
                        using (var vertical = new EditorGUILayout.VerticalScope(foldoutContentStyle))
                        {
                            var pbs = avatar.GetPhysBoneProviders();
                            if (pbs.Length == 0)
                            {
                                EditorGUILayout.LabelField("No PhysBones found.");
                            }
                            else
                            {
                                using (var horizontal = new EditorGUILayout.HorizontalScope())
                                {
                                    if (GUILayout.Button(i18n.SelectAllButtonLabel))
                                    {
                                        physBoneProvidersToKeep = pbs.ToArray();
                                    }
                                    if (GUILayout.Button(i18n.DeselectAllButtonLabel))
                                    {
                                        physBoneProvidersToKeep = new VRCPhysBoneProviderBase[] { };
                                    }
                                }
                                physBoneProvidersToKeep = EditorGUIUtility.GroupedAvatarDynamicsComponentSelectorList(pbs, physBoneProvidersToKeep, avatarRoot, physBoneGroupFoldouts);
                            }
                        }
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                EditorGUILayout.Space();

                using (var foldout = new EditorGUIUtility.FoldoutHeaderGroupScope(foldoutPhysBonesColliders, new GUIContent("PhysBone Colliders", i18n.PhysBonesListTooltip)))
                {
                    foldoutPhysBonesColliders = foldout.Foldout;
                    if (foldoutPhysBonesColliders)
                    {
                        using (var vertical = new EditorGUILayout.VerticalScope(foldoutContentStyle))
                        {
                            var colliders = converterSettings.AvatarDescriptor.GetComponentsInChildren<VRCPhysBoneCollider>(true);
                            if (colliders.Length == 0)
                            {
                                EditorGUILayout.LabelField("No PhysBone Colliders found.");
                            }
                            else
                            {
                                using (var horizontal = new EditorGUILayout.HorizontalScope())
                                {
                                    if (GUILayout.Button(i18n.SelectAllButtonLabel))
                                    {
                                        physBoneCollidersToKeep = colliders.ToArray();
                                    }
                                    if (GUILayout.Button(i18n.DeselectAllButtonLabel))
                                    {
                                        physBoneCollidersToKeep = new VRCPhysBoneCollider[] { };
                                    }
                                }
                                var colliderProviders = colliders.Select(c => new VRCPhysBoneColliderProvider(c)).ToArray();
                                var selectedColliders = physBoneCollidersToKeep.Select(c => new VRCPhysBoneColliderProvider(c)).ToArray();
                                selectedColliders = EditorGUIUtility.GroupedAvatarDynamicsComponentSelectorList(colliderProviders, selectedColliders, avatarRoot, colliderGroupFoldouts);
                                physBoneCollidersToKeep = selectedColliders.Select(p => p.Component).Cast<VRCPhysBoneCollider>().ToArray();
                            }
                        }
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();

                EditorGUILayout.Space();

                var contactsHeader = "Non-Local Contact Senders & Non-Local Contact Receivers";
                using (var foldout = new EditorGUIUtility.FoldoutHeaderGroupScope(foldoutContacts, new GUIContent(contactsHeader, i18n.PhysBonesListTooltip)))
                {
                    foldoutContacts = foldout.Foldout;
                    if (foldoutContacts)
                    {
                        using (var vertical = new EditorGUILayout.VerticalScope(foldoutContentStyle))
                        {
                            var contacts = new VRChatAvatar(converterSettings.AvatarDescriptor).GetNonLocalContacts();
                            if (contacts.Length == 0)
                            {
                                EditorGUILayout.LabelField($"No {contactsHeader} found.");
                            }
                            else
                            {
                                using (var horizontal = new EditorGUILayout.HorizontalScope())
                                {
                                    if (GUILayout.Button(i18n.SelectAllButtonLabel))
                                    {
                                        contactsToKeep = contacts.ToArray();
                                    }
                                    if (GUILayout.Button(i18n.DeselectAllButtonLabel))
                                    {
                                        contactsToKeep = new VRC.Dynamics.ContactBase[] { };
                                    }
                                }
                                var contactProviders = contacts.Select(c => new VRCContactBaseProvider(c)).ToArray();
                                var selectedContacts = contactsToKeep.Select(c => new VRCContactBaseProvider(c)).ToArray();
                                selectedContacts = EditorGUIUtility.GroupedAvatarDynamicsComponentSelectorList(contactProviders, selectedContacts, avatarRoot, contactGroupFoldouts);
                                contactsToKeep = selectedContacts.Select(p => p.Component).Cast<ContactBase>().ToArray();
                            }
                        }
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            EditorGUILayout.Space();

            // Update the fallback scene preview with the current selection (drawn while no row is hovered).
            AvatarDynamicsPreviewService.SetSelectedPreviewComponents(
                physBoneProvidersToKeep.Cast<IVRCAvatarDynamicsProvider>()
                    .Concat(contactsToKeep.Where(c => c != null).Select(c => (IVRCAvatarDynamicsProvider)new VRCContactBaseProvider(c))));

            using (var foldout = new EditorGUIUtility.FoldoutHeaderGroupScope(foldoutEstimatedPerformance, new GUIContent(i18n.EstimatedPerformanceStats)))
            {
                foldoutEstimatedPerformance = foldout.Foldout;
                if (foldoutEstimatedPerformance)
                {
                    var pbcToKeep = physBoneCollidersToKeep.Where(x => x != null).ToArray();
                    var cToKeep = contactsToKeep.Where(x => x != null).ToArray();
                    var stats = avatar.EstimatePerformanceStats(physBoneProvidersToKeep, pbcToKeep, cToKeep, true);
                    var categories = VRCSDKUtility.AvatarDynamicsPerformanceCategories;
                    foreach (var category in categories)
                    {
                        EditorGUIUtility.PerformanceRatingPanel(stats, statsLevelSet, category, i18n);
                    }
                }
            }

            EditorGUILayout.Space(8);

            if (EditorGUIUtility.LargeButton(i18n.ApplyButtonLabel))
            {
                ApplyDynamicsSettings(converterSettings, physBoneProvidersToKeep, physBoneCollidersToKeep, contactsToKeep);

                var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null)
                {
                    EditorSceneManager.MarkSceneDirty(prefabStage.scene);
                }

                Close();
            }

            EditorGUILayout.Space(8);

            AvatarDynamicsPreviewService.EndPreviewFrame();
        }
    }
}
