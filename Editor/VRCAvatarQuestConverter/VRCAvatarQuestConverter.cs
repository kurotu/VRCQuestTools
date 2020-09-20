// <copyright file="VRCAvatarQuestConverter.cs" company="kurotu">
// Copyright (c) kurotu. All rights reserved.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

using ImageMagick;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace KRTQuestTools
{
    public class VRCAvatarQuestConverterWindow : EditorWindow
    {
        VRC.SDKBase.VRC_AvatarDescriptor avatar;
        string outputPath = "";
        bool allowOverwriting = false;
        bool combineEmission = true;

        [MenuItem("KRTQuestTools/Convert Avatar For Quest")]
        static void Init()
        {
            var window = GetWindow<VRCAvatarQuestConverterWindow>();
            if (window.avatar == null && VRCAvatarQuestConverter.IsAvatar(Selection.activeGameObject))
            {
                window.avatar = Selection.activeGameObject.GetComponent<VRC.SDKBase.VRC_AvatarDescriptor>();
                window.SetArtifactsPath(window.avatar);
            }
            window.Show();
        }

        private void OnGUI()
        {
            titleContent.text = "Convert Avatar for Quest";

            EditorGUILayout.LabelField("Converter Settings", EditorStyles.boldLabel);
            var selectedAvatar = (VRC.SDKBase.VRC_AvatarDescriptor)EditorGUILayout.ObjectField("Avatar", avatar, typeof(VRC.SDKBase.VRC_AvatarDescriptor), true);
            if (selectedAvatar == null)
            {
                outputPath = "";
            }
            else if (avatar != selectedAvatar)
            {
                SetArtifactsPath(selectedAvatar);
            }
            avatar = selectedAvatar;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Experimental Settings", EditorStyles.boldLabel);
            combineEmission = EditorGUILayout.Toggle("Combine Emission", combineEmission);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Output Folder", EditorStyles.boldLabel);
            outputPath = EditorGUILayout.TextField("Save To", outputPath);
            if (GUILayout.Button("Select"))
            {
                var split = outputPath.Split('/');
                var folder = string.Join("/", split.Where((s, i) => i <= split.Length - 2));
                var defaultName = split.Last();
                var dest = EditorUtility.SaveFolderPanel("Artifacts", folder, defaultName);
                if (dest != "") // Cancel
                {
                    outputPath = "Assets" + dest.Remove(0, Application.dataPath.Length);
                }
            }
            // allowOverwriting = EditorGUILayout.Toggle("AllowOverwriting", allowOverwriting);

            EditorGUILayout.Space();
            if (GUILayout.Button("Convert"))
            {
                VRCAvatarQuestConverter.ConvertForQuest(avatar.gameObject, outputPath, combineEmission);
            }
        }

        private void SetArtifactsPath(VRC.SDKBase.VRC_AvatarDescriptor avatar)
        {
            outputPath = $"Assets/KRT/KRTQuestTools/Artifacts/{avatar.name}";
        }
    }

    public static class VRCAvatarQuestConverter
    {
        const string Tag = "VRCAvatarQuestConverter";
        const string ArtifactsRootDir = "Assets/KRT/KRTQuestTools/Artifacts";
        const string QuestShader = "VRChat/Mobile/Toon Lit";
        readonly static VRCAvatarQuestConverterI18nBase i18n = VRCAvatarQuestConverterI18n.Create();

        [MenuItem("GameObject/Convert Avatar For Quest", false)]
        public static void ConvertToQuest()
        {
            var original = Selection.activeGameObject;
            var artifactsDir = $"{ArtifactsRootDir}/{original.name}";
            ConvertForQuest(original, artifactsDir, false);
        }

        internal static void ConvertForQuest(GameObject original, string artifactsDir, bool combineEmission)
        {
            if (Directory.Exists(artifactsDir))
            {
                var altDir = AssetDatabase.GenerateUniqueAssetPath(artifactsDir);
                var option = EditorUtility.DisplayDialogComplex(
                    i18n.OverwriteWarningDialogTitle(),
                    i18n.OverwriteWarningDialogMessage(artifactsDir),
                    i18n.OverwriteWarningDialogButtonOK(),
                    i18n.OverwriteWarningDialogButtonCancel(),
                    i18n.OverwriteWarningDialogButtonUseAltDir(altDir));
                switch (option)
                {
                    case 0: // OK
                        // do nothing
                        break;
                    case 1: // Cancel
                        return;
                    case 2: // Alt
                        artifactsDir = altDir;
                        break;
                }
            }

            var undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Convert Avatar for Quest");

            Directory.CreateDirectory(artifactsDir);

            var materials = GetMaterialsInChildren(original);
            var convertedMaterials = new Dictionary<string, Material>();
            for (var i = 0; i < materials.Length; i++)
            {
                var progress = i / (float)materials.Length;
                EditorUtility.DisplayProgressBar("VRCAvatarQuestConverter", "Converting materials...", progress);
                var m = materials[i];
                if (m == null) { continue; }
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(m, out string guid, out long localid);
                if (convertedMaterials.ContainsKey(guid)) { continue; }
                var shader = Shader.Find(QuestShader);
                Material mat = ConvertMaterialForQuest(artifactsDir, m, guid, shader, combineEmission);
                convertedMaterials.Add(guid, mat);
            }
            EditorUtility.ClearProgressBar();

            var newName = original.name + " (Quest)";
            newName = GenerateUniqueRootGameObjectName(SceneManager.GetActiveScene(), newName);
            var blueprintId = original.GetComponent<VRC.Core.PipelineManager>().blueprintId;
            var questObj = Object.Instantiate(original);
            Undo.RegisterCreatedObjectUndo(questObj, "Create copy for Quest");

            questObj.SetActive(true);
            questObj.name = newName;
            questObj.GetComponent<VRC.Core.PipelineManager>().blueprintId = null;
            var renderers = questObj.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                var newMaterials = r.sharedMaterials.Select((m) =>
                {
                    if (m == null) { return null; }
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(m, out string guid, out long localid);
                    return convertedMaterials[guid];
                });
                r.sharedMaterials = newMaterials.ToArray();
            }
            RemoveMissingComponents(questObj);
            RemoveMissingComponentsInChildren(questObj, true);
            RemoveUnsupportedComponentsInChildren(questObj, true);
            Selection.activeGameObject = questObj;
            if (System.Type.GetType("VertexColorRemover") != null)
            {
                Debug.Log($"[{Tag}] VertexColorRemover found. Apply to the converted avatar");
                EditorApplication.ExecuteMenuItem("GameObject/Remove All Vertex Colors");
            }

            var prefab = $"{artifactsDir}/{questObj.name}.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(questObj, prefab, InteractionMode.UserAction);
            AssetDatabase.Refresh();
            Debug.LogFormat("[{0}] Converted as {1}", Tag, prefab);
            questObj.GetComponent<VRC.Core.PipelineManager>().blueprintId = blueprintId;

            if (original.activeInHierarchy)
            {
                Undo.RecordObject(original, "Disable original avatar");
                original.SetActive(false);
            }

            Undo.CollapseUndoOperations(undoGroup);
        }

        private static Material ConvertMaterialForQuest(string artifactsDir, Material material, string guid, Shader newShader, bool combineEmission)
        {
            Material mat = MaterialConverter.Convert(material, newShader);
            if (combineEmission)
            {
                var mw = MaterialUtils.CreateWrapper(material);
                using (var combined = mw.CompositeLayers())
                {
                    var outFile = $"{artifactsDir}/{material.name}_from_{guid}.png";
                    combined.Write(outFile, MagickFormat.Png24);
                    AssetDatabase.Refresh();
                    var tex = AssetDatabase.LoadAssetAtPath<Texture>(outFile);
                    mat.mainTexture = tex;
                }
            }
            var file = $"{artifactsDir}/{material.name}_from_{guid}.mat";
            AssetDatabase.CreateAsset(mat, file);
            return mat;
        }

        [MenuItem("GameObject/Convert Avatar For Quest", true)]
        public static bool ValidateMenu()
        {
            var obj = Selection.activeGameObject;
            return IsAvatar(obj);
        }

        internal static bool IsAvatar(GameObject obj)
        {
            if (obj == null) { return false; }
            if (obj.GetComponent<VRC.SDKBase.VRC_AvatarDescriptor>() == null) { return false; }
            return true;
        }

        private static Material[] GetMaterialsInChildren(GameObject gameObject)
        {
            var renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            return renderers.SelectMany(r => r.sharedMaterials).Distinct().ToArray();
        }

        private static void RemoveMissingComponentsInChildren(GameObject gameObject, bool includeInactive)
        {
            var children = gameObject.GetComponentsInChildren<Transform>(includeInactive).Select(t => t.gameObject);
            foreach (var c in children)
            {
                RemoveMissingComponents(c);
            }
        }

        private static void RemoveMissingComponents(GameObject gameObject)
        {
            var serializedObj = new SerializedObject(gameObject);
            var serializedComponentList = serializedObj.FindProperty("m_Component");
            var components = gameObject.GetComponents<Component>();

            for (int i = components.Length - 1; i > -1; i--)
            {
                if (components[i] == null)
                {
                    serializedComponentList.DeleteArrayElementAtIndex(i);
                }
            }
            serializedObj.ApplyModifiedProperties();
        }

        private static void RemoveUnsupportedComponentsInChildren(GameObject gameObject, bool includeInactive)
        {
            var types = new System.Type[] {
                GetType("DynamicBoneColliderBase"), GetType("DynamicBone"), // DynamicBone may be missing
                typeof(Cloth),
                typeof(Camera),
                typeof(Light),
                typeof(AudioSource),
                typeof(Joint), typeof(Rigidbody),typeof(Collider),
                typeof(UnityEngine.Animations.IConstraint)
            }.Where(e => e != null);
            foreach (var type in types)
            {
                var components = gameObject.GetComponentsInChildren(type, includeInactive);
                foreach (var c in components)
                {
                    var message = $"[{Tag}] Removed {c.GetType().Name} from {c.gameObject.name}";
                    Object.DestroyImmediate(c);
                    Debug.Log(message);
                }
            }
        }

        private static string GenerateUniqueRootGameObjectName(Scene scene, string name)
        {
            var names = scene.GetRootGameObjects().Select(o => o.name).ToArray();
            return ObjectNames.GetUniqueName(names, name);
        }

        private static System.Type GetType(string fullName)
        {
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in asm.GetTypes())
                {
                    if (type.FullName == fullName)
                    {
                        return type;
                    }
                }
            }
            return null;
        }
    }
}
