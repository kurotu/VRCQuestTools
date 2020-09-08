// <copyright file="VRCAvatarQuestConverter.cs" company="kurotu">
// Copyright (c) kurotu. All rights reserved.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace KRTQuestTools
{
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
            ConvertForQuest(original, artifactsDir);
        }

        private static void ConvertForQuest(GameObject original, string artifactsDir)
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
            foreach (var m in materials)
            {
                if (m == null) { continue; }
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(m, out string guid, out long localid);
                if (convertedMaterials.ContainsKey(guid)) { continue; }
                var shader = Shader.Find(QuestShader);
                Material mat = ConvertMaterialForQuest(artifactsDir, m, guid, shader);
                convertedMaterials.Add(guid, mat);
            }

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

        private static Material ConvertMaterialForQuest(string artifactsDir, Material m, string guid, Shader shader)
        {
            Material mat = MaterialConverter.Convert(m, shader);
            var file = $"{artifactsDir}/{m.name}_from_{guid}.mat";
            AssetDatabase.CreateAsset(mat, file);
            return mat;
        }

        [MenuItem("GameObject/Convert Avatar For Quest", true)]
        public static bool ValidateMenu()
        {
            var obj = Selection.activeGameObject;
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
