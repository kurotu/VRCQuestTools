// <copyright file="VRCAvatarQuestConverter.cs" company="kurotu">
// Copyright (c) kurotu.
// </copyright>
// <author>kurotu</author>
// <remarks>Licensed under the MIT license.</remarks>

using ImageMagick;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KRT.VRCQuestTools
{
    public class VRCAvatarQuestConverterWindow : EditorWindow
    {
        enum TexturesSizeLimit
        {
            None = 0,
            UpTo256x256 = 256,
            UpTo512x512 = 512,
            UpTo1024x1024 = 1024,
            UpTo2048x2048 = 2048
        }

        VRC.SDKBase.VRC_AvatarDescriptor avatar;
        string outputPath = "";
        bool generateQuestTextures = true;
        TexturesSizeLimit texturesSizeLimit = TexturesSizeLimit.UpTo1024x1024;
        readonly VRCAvatarQuestConverterI18nBase i18n = VRCAvatarQuestConverterI18n.Create();

        internal static void InitFromMenu()
        {
            var window = GetWindow<VRCAvatarQuestConverterWindow>();
            if (window.avatar == null && VRCSDKUtils.IsAvatar(Selection.activeGameObject))
            {
                window.avatar = Selection.activeGameObject.GetComponent<VRC.SDKBase.VRC_AvatarDescriptor>();
                window.SetArtifactsPath(window.avatar);
            }
            window.Show();
        }

        private void OnGUI()
        {
            titleContent.text = "Convert Avatar for Quest";

            var selectedAvatar = (VRC.SDKBase.VRC_AvatarDescriptor)EditorGUILayout.ObjectField(i18n.AvatarLabel, avatar, typeof(VRC.SDKBase.VRC_AvatarDescriptor), true);
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

            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                generateQuestTextures = EditorGUILayout.BeginToggleGroup(i18n.GenerateQuestTexturesLabel, generateQuestTextures);
                EditorGUILayout.HelpBox($"{i18n.QuestTexturesDescription}\n\n" +
                    $"{i18n.SupportedShadersLabel}: Standard, UTS2, arktoon", MessageType.Info);
                texturesSizeLimit = (TexturesSizeLimit)EditorGUILayout.EnumPopup(i18n.TexturesSizeLimitLabel, texturesSizeLimit);
                EditorGUILayout.EndToggleGroup();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                outputPath = EditorGUILayout.TextField(i18n.SaveToLabel, outputPath);
                if (GUILayout.Button(i18n.SelectButtonLabel))
                {
                    var split = outputPath.Split('/');
                    var folder = string.Join("/", split.Where((s, i) => i <= split.Length - 2));
                    var defaultName = split.Last();
                    var dest = EditorUtility.SaveFolderPanel("QuestAvatars", folder, defaultName);
                    if (dest != "") // Cancel
                    {
                        outputPath = "Assets" + dest.Remove(0, Application.dataPath.Length);
                    }
                }
                // allowOverwriting = EditorGUILayout.Toggle("AllowOverwriting", allowOverwriting);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(i18n.WarningForPerformance, MessageType.Warning);
            EditorGUILayout.HelpBox(i18n.WarningForAppearance, MessageType.Warning);

            if (avatar == null)
            {
                EditorGUI.BeginDisabledGroup(true);
            }
            if (GUILayout.Button(i18n.ConvertButtonLabel))
            {
                var questAvatar = VRCAvatarQuestConverter.ConvertForQuest(avatar.gameObject, outputPath, generateQuestTextures, (int)texturesSizeLimit);
                if (questAvatar != null)
                {
                    EditorUtility.DisplayDialog(i18n.CompletedDialogTitle, i18n.CompletedDialogMessage(avatar.name), "OK");
                    Selection.activeGameObject = questAvatar;
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private void SetArtifactsPath(VRC.SDKBase.VRC_AvatarDescriptor avatar)
        {
            outputPath = $"{VRCAvatarQuestConverter.ArtifactsRootDir}/{avatar.name}";
        }
    }

    public static class VRCAvatarQuestConverter
    {
        const string Tag = "VRCAvatarQuestConverter";
        internal const string ArtifactsRootDir = "Assets/KRT/QuestAvatars";
        const string QuestShader = "VRChat/Mobile/Toon Lit";
        internal readonly static VRCAvatarQuestConverterI18nBase i18n = VRCAvatarQuestConverterI18n.Create();

        internal static GameObject ConvertForQuest(GameObject original, string artifactsDir, bool generateQuestTextures, int maxTextureSize)
        {
            if (Directory.Exists(artifactsDir))
            {
                var altDir = AssetDatabase.GenerateUniqueAssetPath(artifactsDir);
                var option = EditorUtility.DisplayDialogComplex(
                    i18n.OverwriteWarningDialogTitle,
                    i18n.OverwriteWarningDialogMessage(artifactsDir),
                    i18n.OverwriteWarningDialogButtonOK,
                    i18n.OverwriteWarningDialogButtonCancel,
                    i18n.OverwriteWarningDialogButtonUseAltDir(altDir));
                switch (option)
                {
                    case 0: // OK
                        // do nothing
                        break;
                    case 1: // Cancel
                        return null;
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
            try
            {
                for (var i = 0; i < materials.Length; i++)
                {
                    var progress = i / (float)materials.Length;
                    EditorUtility.DisplayProgressBar("VRCAvatarQuestConverter", $"{i18n.ConvertingMaterialsDialogMessage} : {i + 1}/{materials.Length}", progress);
                    var m = materials[i];
                    if (m == null) { continue; }
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(m, out string guid, out long localid);
                    if (convertedMaterials.ContainsKey(guid)) { continue; }
                    var shader = Shader.Find(QuestShader);
                    Material mat = ConvertMaterialForQuest(artifactsDir, m, guid, shader, generateQuestTextures, maxTextureSize);
                    convertedMaterials.Add(guid, mat);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                EditorUtility.DisplayDialog("VRCAvatarQuestConverter", $"{i18n.MaterialExceptionDialogMessage}\n\n{e.Message}", "OK");
                return null;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
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
            VRCSDKUtils.RemoveMissingComponentsInChildren(questObj, true);
            VRCSDKUtils.RemoveUnsupportedComponentsInChildren(questObj, true);

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
            return questObj;
        }

        private static Material ConvertMaterialForQuest(string artifactsDir, Material material, string guid, Shader newShader, bool generateQuestTextures, int maxTextureSize)
        {
            var resizeTextures = maxTextureSize > 0;
            Material mat = MaterialConverter.Convert(material, newShader);
            if (generateQuestTextures)
            {
                var mw = MaterialUtils.CreateWrapper(material);
                using (var combined = mw.CompositeLayers())
                {
                    var texturesDir = $"{artifactsDir}/Textures";
                    Directory.CreateDirectory(texturesDir);
                    var outFile = $"{texturesDir}/{material.name}_from_{guid}.png";
                    var format = combined.HasAlpha ? MagickFormat.Png32 : MagickFormat.Png24;
                    if (resizeTextures && (combined.Width > maxTextureSize || combined.Height > maxTextureSize))
                    {
                        combined.Resize(maxTextureSize, maxTextureSize);
                    }
                    combined.Write(outFile, format);
                    AssetDatabase.Refresh();
                    var tex = AssetDatabase.LoadAssetAtPath<Texture>(outFile);
                    mat.mainTexture = tex;
                }
            }
            var materialsDir = $"{artifactsDir}/Materials";
            Directory.CreateDirectory(materialsDir);
            var file = $"{materialsDir}/{material.name}_from_{guid}.mat";
            AssetDatabase.CreateAsset(mat, file);
            return mat;
        }

        private static Material[] GetMaterialsInChildren(GameObject gameObject)
        {
            var renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            List<Material> animMats = gameObject
                .GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>()    // avaterDescriptor
                .baseAnimationLayers
                .Where(obj => !obj.isDefault)   // AnimationControllerが設定されている
                .SelectMany(obj => obj.animatorController.animationClips)   // 設定されているAnimationファイルすべて
                .SelectMany(layer =>
                    {
                        EditorCurveBinding[] binding = AnimationUtility.GetObjectReferenceCurveBindings(layer); //Animationに設定されているオブジェクト
                        binding = binding.Where(b => b.type == typeof(MeshRenderer) || b.type == typeof(SkinnedMeshRenderer)).ToArray();    // Renderer系のみ

                        List<Material> keyframes = binding.SelectMany(b => AnimationUtility.GetObjectReferenceCurve(layer, b))  // keyframeに設定されているオブジェクト
                        .Where(keyframe => keyframe.value.GetType() == typeof(Material))    // マテリアルのみ取得
                        .Select(keyframe => (Material)keyframe.value)  // マテリに変換
                        .ToList();
                        
                        return keyframes;
                    })
                .ToList();
            return renderers.SelectMany(r => r.sharedMaterials).Distinct().ToArray();
        }


        private static string GenerateUniqueRootGameObjectName(Scene scene, string name)
        {
            var names = scene.GetRootGameObjects().Select(o => o.name).ToArray();
            return ObjectNames.GetUniqueName(names, name);
        }
    }
}
