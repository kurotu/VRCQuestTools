// <copyright file="UnityAnimationUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Utility for Unity animation.
    /// </summary>
    internal static class UnityAnimationUtility
    {
        /// <summary>
        /// Gets materials from AnimatorController.
        /// </summary>
        /// <param name="controller">AnimatorController.</param>
        /// <returns>Materials.</returns>
        internal static Material[] GetMaterials(RuntimeAnimatorController controller)
        {
            return controller.animationClips
                .SelectMany(clip => GetMaterials(clip))
                .Distinct()
                .ToArray();
        }

        /// <summary>
        /// Replace animation clip's materials with new materials.
        /// </summary>
        /// <param name="clip">AnimationClip.</param>
        /// <param name="newMaterials">Materials to replace (key: original material).</param>
        /// <returns>New animation clip.</returns>
        internal static AnimationClip ReplaceAnimationClipMaterials(AnimationClip clip, Dictionary<Material, Material> newMaterials)
        {
            var anim = Object.Instantiate(clip);
            EditorCurveBinding[] binding = AnimationUtility.GetObjectReferenceCurveBindings(anim);
            for (int j = 0; j < binding.Length; j++)
            {
                if (binding[j].type == typeof(MeshRenderer) || binding[j].type == typeof(SkinnedMeshRenderer))
                {
                    var keyframes = AnimationUtility.GetObjectReferenceCurve(anim, binding[j]);
                    for (int k = 0; k < keyframes.Length; k++)
                    {
                        if (keyframes[k].value && keyframes[k].value.GetType() == typeof(Material))
                        {
                            var material = (Material)keyframes[k].value;
                            if (newMaterials.ContainsKey(material))
                            {
                                keyframes[k].value = newMaterials[material];
                                Debug.Log("replace animationClip: " + newMaterials[material]);
                            }
                        }
                    }
                    AnimationUtility.SetObjectReferenceCurve(anim, binding[j], keyframes);
                }
            }
            return anim;
        }

        /// <summary>
        /// Replace animator controller's animation clips with new clips.
        /// </summary>
        /// <param name="controller">Target controller.</param>
        /// <param name="outFile">Asset path for new controller.</param>
        /// <param name="newAnimationClips">Animation clips to replace (key: original clip).</param>
        /// <returns>New animator controller.</returns>
        internal static AnimatorController ReplaceAnimationClips(RuntimeAnimatorController controller, string outFile, Dictionary<AnimationClip, AnimationClip> newAnimationClips)
        {
            Debug.Log("originalPath :" + AssetDatabase.GetAssetPath(controller));
            Debug.Log("copy Path    :" + outFile);
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(controller), outFile);
            AssetDatabase.Refresh();
            AnimatorController cloneController = (AnimatorController)AssetDatabase.LoadAssetAtPath(outFile, typeof(AnimatorController));

            // コピーしたコントローラーに修正したアニメーションクリップを反映
            // レイヤー→ステートマシン→ステート→アニメーションクリップ
            for (int i = 0; i < cloneController.layers.Length; i++)
            {
                AnimatorControllerLayer layer = cloneController.layers[i];
                AnimatorStateMachine stateMachine = layer.stateMachine;
                for (int j = 0; j < stateMachine.states.Length; j++)
                {
                    AnimatorState animState = stateMachine.states[j].state;
                    if (animState.motion == null)
                    {
                        continue;
                    }

                    // BlendTreeも設定できるので型チェック
                    if (animState.motion.GetType() != typeof(AnimationClip))
                    {
                        continue;
                    }

                    AnimationClip anim = (AnimationClip)animState.motion;
                    Debug.Log("am :" + anim.name);
                    if (newAnimationClips.ContainsKey(anim))
                    {
                        cloneController.layers[i].stateMachine.states[j].state.motion = newAnimationClips[anim];
                        Debug.Log("replace animationClip : " + newAnimationClips[anim].name);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            return cloneController;
        }

        private static Material[] GetMaterials(AnimationClip clip)
        {
            EditorCurveBinding[] binding = AnimationUtility.GetObjectReferenceCurveBindings(clip); // Animationに設定されているオブジェクト
            binding = binding.Where(b => b.type == typeof(MeshRenderer) || b.type == typeof(SkinnedMeshRenderer)).ToArray(); // Renderer系のみ

            var keyframes = binding.SelectMany(b => AnimationUtility.GetObjectReferenceCurve(clip, b)); // keyframeに設定されているオブジェクト
            var materials = keyframes.Where(keyframe => keyframe.value && keyframe.value.GetType() == typeof(Material)) // マテリアルのみ取得
                .Select(keyframe => (Material)keyframe.value); // マテリアルに変換

            return materials.Where(m => m != null).Distinct().ToArray();
        }
    }
}
