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
using UnityEditor.Animations;
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

        private Vector2 _scrollPosition = Vector2.zero;

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
            var i18n = VRCQuestToolsSettings.I18nResource;
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

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                generateQuestTextures = EditorGUILayout.BeginToggleGroup(i18n.GenerateQuestTexturesLabel, generateQuestTextures);
                EditorGUILayout.HelpBox($"{i18n.QuestTexturesDescription}\n\n" +
                    $"{i18n.VerifiedShadersLabel}: Standard, UTS2, arktoon", MessageType.Info);
                if (avatar != null)
                {
                    var unverifiedMaterials = VRCAvatarQuestConverter.GetMaterialsInChildrenWithUnverifiedShaders(avatar.gameObject);
                    if (generateQuestTextures && unverifiedMaterials.Length > 0)
                    {
                        EditorGUILayout.HelpBox($"{i18n.WarningForUnverifiedShaders}\n\n" +
                            $"{string.Join("\n", unverifiedMaterials.Select(m => $"  - {m.name} ({m.shader.name})"))}", MessageType.Error);
                    }
                }
                texturesSizeLimit = (TexturesSizeLimit)EditorGUILayout.EnumPopup(i18n.TexturesSizeLimitLabel, texturesSizeLimit);

                EditorGUILayout.Space();

                if (GUILayout.Button(i18n.UpdateTexturesLabel))
                {
                    VRCAvatarQuestConverter.GenerateTexturesForQuest(avatar.gameObject, outputPath, (int)texturesSizeLimit);
                }
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
            EditorGUILayout.HelpBox(i18n.WarningForPerformance, MessageType.Info);
            EditorGUILayout.HelpBox(i18n.WarningForAppearance, MessageType.Warning);
            if (avatar != null)
            {
                var componentsToBeAlearted = VRCSDKUtils.GetUnsupportedComponentsInChildren(avatar.gameObject, true)
                    .Select(c => c.GetType().Name)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToArray();
                if (componentsToBeAlearted.Count() > 0)
                {
                    EditorGUILayout.HelpBox(i18n.AlertForComponents + "\n\n" + string.Join("\n", componentsToBeAlearted.Select(c => $"  - {c}")), MessageType.Error);
                }

                if (VRCAvatarQuestConverter.GetAnimatedMaterialsInChildren(avatar.gameObject).Length > 0)
                {
                    EditorGUILayout.HelpBox(i18n.AlertForMaterialAnimation, MessageType.Error);
                }
            }

            EditorGUI.BeginDisabledGroup(avatar == null);
            {
                if (GUILayout.Button(i18n.ConvertButtonLabel))
                {
                    var questAvatar = VRCAvatarQuestConverter.ConvertForQuest(avatar.gameObject, outputPath, generateQuestTextures, (int)texturesSizeLimit);
                    if (questAvatar != null)
                    {
                        EditorUtility.DisplayDialog(i18n.CompletedDialogTitle, i18n.CompletedDialogMessage(avatar.name), "OK");
                        Selection.activeGameObject = questAvatar;
                    }
                }
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            EditorGUILayout.EndScrollView();
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

        internal static GameObject ConvertForQuest(GameObject original, string artifactsDir, bool generateQuestTextures, int maxTextureSize)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
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
            for (var i = 0; i < materials.Length; i++)
            {
                var progress = i / (float)materials.Length;
                EditorUtility.DisplayProgressBar("VRCAvatarQuestConverter", $"{i18n.ConvertingMaterialsDialogMessage} : {i + 1}/{materials.Length}", progress);
                var m = materials[i];
                if (m == null) { continue; }
                try
                {
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(m, out string guid, out long localid);
                    if (convertedMaterials.ContainsKey(guid)) { continue; }
                    var shader = Shader.Find(QuestShader);
                    Material mat = ConvertMaterialForQuest(artifactsDir, m, shader, generateQuestTextures, maxTextureSize);
                    convertedMaterials.Add(guid, mat);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                    EditorUtility.DisplayDialog("VRCAvatarQuestConverter",
                        $"{i18n.MaterialExceptionDialogMessage}\n" +
                        "\n" +
                        $"Material: {AssetDatabase.GetAssetPath(m)}\n" +
                        $"Shader: {m.shader.name}\n" +
                        "\n" +
                        $"Exception: {e.Message}"
                        , "OK");
                    EditorUtility.ClearProgressBar();
                    return null;
                }
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

            if (GetAnimatedMaterialsInChildren(original).Length > 0)
            {
                // アニメーション内容書き換え
                Dictionary<string, AnimationClip> convertedAnimatoinClip = new Dictionary<string, AnimationClip>();
                var animationClips = GetAnimationClipsInChildren(questObj)
                    .Where(anim => !VRCSDKUtils.IsProxyAnimationClip(anim))
                    .ToArray();
                var animationClipDir = $"{artifactsDir}/Animations";
                Directory.CreateDirectory(animationClipDir);
                for (var i = 0; i < animationClips.Length; i++)
                {
                    try
                    {
                        var progress = i / (float)animationClips.Length;
                        EditorUtility.DisplayProgressBar("VRCAvatarQuestConverter", $"Convert AnimationCilp : {i + 1}/{animationClips.Length}", progress);

                        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(animationClips[i], out string guid, out long localid);
                        var outFile = $"{animationClipDir}/{animationClips[i].name}_from_{guid}.anim";
                        var anim = ConvertAnimationClipForQuest(animationClips[i], convertedMaterials);//Object.Instantiate(animationClips[i]);

                        //
                        AssetDatabase.CreateAsset(anim, outFile);
                        convertedAnimatoinClip.Add(guid, anim);
                        Debug.Log("create asset: " + outFile);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                        EditorUtility.DisplayDialog("VRCAvatarQuestConverter",
                            $"{i18n.AnimationClipExceptionDialogMessage}\n" +
                            $"\n" +
                            $"AnimationClip: {animationClips[i].name}\n" +
                            $"\n" +
                            $"Exception: {e.Message}", "OK");
                        EditorUtility.ClearProgressBar();
                        return null;
                    }
                }
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // アニメーションファイル差し替え,コントローラーコピー
                Dictionary<string, RuntimeAnimatorController> convertedAnimatorControllers = new Dictionary<string, RuntimeAnimatorController>();
                var controllerDir = $"{artifactsDir}/AnimatorControllers";
                Directory.CreateDirectory(controllerDir);
                RuntimeAnimatorController[] controllers = GetAnimatorControllerInChildren(questObj);
                var indx = 0;
                foreach (var c in controllers)
                {
                    try
                    {
                        var progress = indx / (float)controllers.Length;
                        EditorUtility.DisplayProgressBar("VRCAvatarQuestConverter", $"Convert AnimatorController : {indx + 1}/{controllers.Length}", progress);
                        indx++;
                        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(c, out string guid, out long localid);
                        var outFile = $"{controllerDir}/{c.name}_from_{guid}.controller";
                        Debug.Log("originalPath :" + AssetDatabase.GetAssetPath(c));
                        Debug.Log("copy Path    :" + outFile);
                        AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(c), outFile);
                        AssetDatabase.Refresh();
                        AnimatorController cloneController = (AnimatorController)AssetDatabase.LoadAssetAtPath(outFile, typeof(AnimatorController));

                        //コピーしたコントローラーに修正したアニメーションクリップを反映
                        // レイヤー→ステートマシン→ステート→アニメーションクリップ
                        for (int i = 0; i < cloneController.layers.Length; i++)
                        {
                            AnimatorControllerLayer layer = cloneController.layers[i];
                            AnimatorStateMachine stateMachine = layer.stateMachine;
                            for (int j = 0; j < stateMachine.states.Length; j++)
                            {
                                AnimatorState animState = stateMachine.states[j].state;
                                if (animState.motion == null) continue;
                                // BlendTreeも設定できるので型チェック
                                if (animState.motion.GetType() != typeof(AnimationClip)) continue;

                                AnimationClip anim = (AnimationClip)animState.motion;
                                Debug.Log("am :" + anim.name);
                                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(anim, out string _guid, out long _localid);
                                if (convertedAnimatoinClip.ContainsKey(_guid))
                                {
                                    //animState.motion = convertedAnimatoinClip[_guid];
                                    cloneController.layers[i].stateMachine.states[j].state.motion = convertedAnimatoinClip[_guid];
                                    Debug.Log("replace animationClip : " + convertedAnimatoinClip[_guid].name);
                                }
                            }
                        }

                        AssetDatabase.SaveAssets();
                        convertedAnimatorControllers.Add(guid, cloneController);
                        Debug.Log("create asset: " + outFile);
                        Debug.Log("test: " + c.Equals(cloneController));

                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                        EditorUtility.DisplayDialog("VRCAvatarQuestConverter",
                            $"{i18n.AnimatorControllerExceptionDialogMessage}\n" +
                            $"\n" +
                            $"AnimatorController: {c.name}\n" +
                            $"\n" +
                            $"Exception: {e.Message}", "OK");
                        EditorUtility.ClearProgressBar();
                        return null;
                    }
                }
#if VRC_SDK_VRCSDK3
                // アバターのアニメーターコントローラー差し替え                
                var customAnimationLayers = questObj.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>().baseAnimationLayers;
                for (int i = 0; i < customAnimationLayers.Length; i++)
                {
                    if (!customAnimationLayers[i].isDefault)
                    {
                        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(customAnimationLayers[i].animatorController, out string guid, out long localid);
                        if (convertedAnimatorControllers.ContainsKey(guid))
                        {
                            Debug.Log("replace asset: " + customAnimationLayers[i].animatorController.name + " to " + convertedAnimatorControllers[guid].name);
                            customAnimationLayers[i].animatorController = convertedAnimatorControllers[guid];
                        }
                    }
                }
#endif
                AssetDatabase.SaveAssets();
                EditorUtility.ClearProgressBar();

                // GameObjectに付属するAnimatorのアニメーターコントローラーを差し替える
                Animator[] animators = questObj.GetComponentsInChildren<Animator>();
                for (int i = 0; i < animators.Length; i++)
                {
                    if (animators[i].runtimeAnimatorController)
                    {
                        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(animators[i].runtimeAnimatorController, out string guid, out long localid);
                        if (convertedAnimatorControllers.ContainsKey(guid))
                        {
                            Debug.Log("replace asset: " + animators[i].runtimeAnimatorController.name + " to " + convertedAnimatorControllers[guid].name);
                            animators[i].runtimeAnimatorController = convertedAnimatorControllers[guid];
                        }
                    }
                }
            }

            // Materials
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

        internal static void GenerateTexturesForQuest(GameObject original, string artifactsDir, int maxTextureSize)
        {
            var i18n = VRCQuestToolsSettings.I18nResource;
            var materials = GetMaterialsInChildren(original);
            for (int i = 0; i < materials.Length; i++)
            {
                var m = materials[i];
                var progress = i / (float)materials.Length;
                EditorUtility.DisplayProgressBar("VRCAvatarQuestConverter", $"{i18n.GeneratingTexturesDialogMessage} : {i + 1}/{materials.Length}", progress);
                GenerateTextureForQuest(artifactsDir, m, maxTextureSize);
            }
            EditorUtility.ClearProgressBar();
        }

        private static Material ConvertMaterialForQuest(string artifactsDir, Material material, Shader newShader, bool generateQuestTextures, int maxTextureSize)
        {
            var resizeTextures = maxTextureSize > 0;
            Material mat = MaterialConverter.Convert(material, newShader);
            if (generateQuestTextures)
            {
                var tex = GenerateTextureForQuest(artifactsDir, material, maxTextureSize);
                mat.mainTexture = tex;
            }
            var materialsDir = $"{artifactsDir}/Materials";
            Directory.CreateDirectory(materialsDir);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(material, out string guid, out long localid);
            var file = $"{materialsDir}/{material.name}_from_{guid}.mat";
            AssetDatabase.CreateAsset(mat, file);
            return mat;
        }

        private static Texture GenerateTextureForQuest(string artifactsDir, Material material, int maxTextureSize)
        {
            var resizeTextures = maxTextureSize > 0;
            var mw = MaterialUtils.CreateWrapper(material);
            using (var combined = mw.CompositeLayers())
            {
                var texturesDir = $"{artifactsDir}/Textures";
                Directory.CreateDirectory(texturesDir);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(material, out string guid, out long localid);
                var outFile = $"{texturesDir}/{material.name}_from_{guid}.png";
                var format = combined.HasAlpha ? MagickFormat.Png32 : MagickFormat.Png24;
                if (resizeTextures && (combined.Width > maxTextureSize || combined.Height > maxTextureSize))
                {
                    combined.Resize(maxTextureSize, maxTextureSize);
                }
                combined.Write(outFile, format);
                AssetDatabase.Refresh();
                return AssetDatabase.LoadAssetAtPath<Texture>(outFile);
            }
        }

        private static Material[] GetMaterialsInChildren(GameObject gameObject)
        {
            return GetRendererMaterialsInChildren(gameObject)
                .Concat(GetAnimatedMaterialsInChildren(gameObject))
                .Distinct()
                .ToArray();
        }

        private static Material[] GetRendererMaterialsInChildren(GameObject gameObject)
        {
            var renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            return renderers.SelectMany(r => r.sharedMaterials).Where(m => m != null).Distinct().ToArray();
        }

        // マテリアルアニメーションが入っていることをGUIで出すための関数化
        internal static Material[] GetAnimatedMaterialsInChildren(GameObject gameObject)
        {
#if VRC_SDK_VRCSDK3
            var animMats = gameObject
                .GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>()    // avaterDescriptor
                .baseAnimationLayers
                .Where(obj => !obj.isDefault)   // AnimatorControllerが設定されている
                .SelectMany(obj => obj.animatorController.animationClips)   // 設定されているAnimationファイルすべて
                .SelectMany(layer =>
                {
                    EditorCurveBinding[] binding = AnimationUtility.GetObjectReferenceCurveBindings(layer); //Animationに設定されているオブジェクト
                    binding = binding.Where(b => b.type == typeof(MeshRenderer) || b.type == typeof(SkinnedMeshRenderer)).ToArray();    // Renderer系のみ

                    List<Material> keyframes = binding.SelectMany(b => AnimationUtility.GetObjectReferenceCurve(layer, b))  // keyframeに設定されているオブジェクト
                    .Where(keyframe => keyframe.value && keyframe.value.GetType() == typeof(Material))    // マテリアルのみ取得
                    .Select(keyframe => (Material)keyframe.value)  // マテリアルに変換
                    .ToList();

                    return keyframes;
                })
                .Where(m => m != null)
                .Distinct()
                .ToArray();
            Debug.Log("anims start");
            foreach (var a in animMats)
            {
                Debug.Log(a.name);
            }
            Debug.Log("anims end");
#else
            var animMats = new Material[0];
#endif
            return animMats;
        }

        private static AnimationClip ConvertAnimationClipForQuest(AnimationClip clip, Dictionary<string, Material> convertedMaterials)
        {
            var anim = Object.Instantiate(clip);
            EditorCurveBinding[] bindng = AnimationUtility.GetObjectReferenceCurveBindings(anim);
            for (int j = 0; j < bindng.Length; j++)
            {
                if (bindng[j].type == typeof(MeshRenderer) || bindng[j].type == typeof(SkinnedMeshRenderer))
                {
                    var keyframes = AnimationUtility.GetObjectReferenceCurve(anim, bindng[j]);
                    for (int k = 0; k < keyframes.Length; k++)
                    {
                        if (keyframes[k].value && keyframes[k].value.GetType() == typeof(Material))
                        {
                            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(keyframes[k].value, out string _guid, out long _localid);
                            if (convertedMaterials.ContainsKey(_guid))
                            {
                                keyframes[k].value = convertedMaterials[_guid];
                                Debug.Log("replace animationClip: " + convertedMaterials[_guid]);
                            }
                        }
                    }
                    AnimationUtility.SetObjectReferenceCurve(anim, bindng[j], keyframes);
                }
            }
            return anim;
        }
        private static RuntimeAnimatorController[] GetAnimatorControllerInChildren(GameObject gameObject)
        {
#if VRC_SDK_VRCSDK3
            // AV3 Playable Layers
            RuntimeAnimatorController[] controller = gameObject
                .GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>()
                .baseAnimationLayers
                .Where(obj => !obj.isDefault)
                .Select(obj => obj.animatorController)
                .ToArray();
#else
            RuntimeAnimatorController[] controller = { };
#endif

            // Animator Controller
            RuntimeAnimatorController[] avatercontrollers = gameObject
                .GetComponentsInChildren<Animator>()
                .Select(obj => obj.runtimeAnimatorController)
                .ToArray();

            return controller.Concat(avatercontrollers).Where(c => c != null).Distinct().ToArray();
        }

        private static AnimationClip[] GetAnimationClipsInChildren(GameObject gameObject)
        {
            AnimationClip[] animations = GetAnimatorControllerInChildren(gameObject)
                .SelectMany(obj => obj.animationClips)
                .Distinct()
                .ToArray();

            return animations;
        }

        private static Material[] GetMaterialInChildrenAnimation(GameObject gameObject)
        {
            Material[] mats = GetAnimationClipsInChildren(gameObject)
                .SelectMany(layer =>
                {

                    EditorCurveBinding[] binding = AnimationUtility.GetObjectReferenceCurveBindings(layer); //Animationに設定されているオブジェクト
                    binding = binding.Where(b => b.type == typeof(MeshRenderer) || b.type == typeof(SkinnedMeshRenderer)).ToArray();    // Renderer系のみ

                    Material[] keyframes = binding.SelectMany(b => AnimationUtility.GetObjectReferenceCurve(layer, b))  // keyframeに設定されているオブジェクト
                    .Where(keyframe => keyframe.value.GetType() == typeof(Material))    // マテリアルのみ取得
                    .Select(keyframe => (Material)keyframe.value)  // マテリアルに変換
                    .ToArray();

                    return keyframes;
                })
                .ToArray();

            return mats;
        }


        private static string GenerateUniqueRootGameObjectName(Scene scene, string name)
        {
            var names = scene.GetRootGameObjects().Select(o => o.name).ToArray();
            return ObjectNames.GetUniqueName(names, name);
        }

        internal static Material[] GetMaterialsInChildrenWithUnverifiedShaders(GameObject gameObject)
        {
            var materials = GetMaterialsInChildren(gameObject);
            return materials.Where(m => MaterialUtils.DetectShaderType(m) == ShaderCategory.Unverified).ToArray();
        }
    }
}
