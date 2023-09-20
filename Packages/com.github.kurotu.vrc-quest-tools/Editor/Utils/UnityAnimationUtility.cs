// <copyright file="UnityAnimationUtility.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.IO;
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
        /// Gets blend trees from AnimatorController.
        /// </summary>
        /// <param name="controller">AnimatorController.</param>
        /// <returns>Blend trees.</returns>
        internal static BlendTree[] GetBlendTrees(AnimatorController controller)
        {
            var trees = controller.layers
                .SelectMany(layer =>
                {
                    var t = new BlendTree[] { };
                    t = GetBlendTrees(layer.stateMachine, t);
                    return t;
                })
                .Distinct()
                .ToArray();
            return trees;
        }

        /// <summary>
        /// Gets blend trees recursively.
        /// </summary>
        /// <param name="stateMachine">State machine.</param>
        /// <param name="currentTrees">Blend trees state.</param>
        /// <returns>New blend trees state.</returns>
        internal static BlendTree[] GetBlendTrees(AnimatorStateMachine stateMachine, BlendTree[] currentTrees)
        {
            List<BlendTree> trees = new List<BlendTree>(currentTrees);
            if (stateMachine.stateMachines.Length > 0)
            {
                var childStateMachineTrees = stateMachine.stateMachines
                    .SelectMany(child => GetBlendTrees(child.stateMachine, currentTrees))
                    .Distinct();
                trees.AddRange(childStateMachineTrees);
            }
            var directTrees = stateMachine.states
                .Select(state =>
                {
                    if (state.state.motion is BlendTree tree)
                    {
                        return tree;
                    }
                    return null;
                })
               .Where(t => t != null);
            trees.AddRange(directTrees);
            return trees.Distinct().ToArray();
        }

        /// <summary>
        /// Whether a motion exists in descendants of a blend tree.
        /// </summary>
        /// <param name="rootTree">Root blend tree.</param>
        /// <param name="targetMotion">Motion to search.</param>
        /// <returns>true when targetTree exists in rootTree descendants.</returns>
        internal static bool DoesMotionExistInBlendTreeDescendants(BlendTree rootTree, Motion targetMotion)
        {
            if (rootTree.children.Count(c => c.motion == targetMotion) > 0)
            {
                return true;
            }
            return rootTree.children.Count(c =>
            {
                if (c.motion is BlendTree tree)
                {
                    return DoesMotionExistInBlendTreeDescendants(tree, targetMotion);
                }
                return false;
            }) > 0;
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
        /// <param name="saveDir">Asset directory for new controller.</param>
        /// <param name="newMotions">Animation clips to replace (key: original clip).</param>
        /// <returns>New animator controller.</returns>
        internal static AnimatorController ReplaceAnimationClips(RuntimeAnimatorController controller, string saveDir, Dictionary<Motion, Motion> newMotions)
        {
            Directory.CreateDirectory($"{saveDir}/AnimatorControllers");
            AssetDatabase.Refresh();

            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(controller, out string guid, out long localId);
            var outFile = $"{saveDir}/AnimatorControllers/{controller.name}_from_{guid}.controller";
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(controller), outFile);
            AssetDatabase.Refresh();
            AnimatorController cloneController = (AnimatorController)AssetDatabase.LoadAssetAtPath(outFile, typeof(AnimatorController));

            // コピーしたコントローラーに修正したアニメーションクリップを反映
            // レイヤー→ステートマシン→ステート→アニメーションクリップ
            for (int layerIndex = 0; layerIndex < cloneController.layers.Length; layerIndex++)
            {
                AnimatorControllerLayer layer = cloneController.layers[layerIndex];
                AnimatorStateMachine stateMachine = layer.stateMachine;
                if (layer.syncedLayerIndex >= 0)
                {
                    stateMachine = cloneController.layers[layer.syncedLayerIndex].stateMachine;
                }
                foreach (var childState in stateMachine.states)
                {
                    var motion = cloneController.GetStateEffectiveMotion(childState.state, layerIndex);
                    if (VRCSDKUtility.IsExampleAsset(motion))
                    {
                        continue;
                    }
                    if (motion != null && newMotions.ContainsKey(motion))
                    {
                        motion = newMotions[motion];
                    }
                    else if (motion is BlendTree tree)
                    {
                        // embedded blend tree
                        var newTree = DeepCopyBlendTree(tree);
                        newTree.children = newTree.children.Select(child =>
                        {
                            if (child.motion != null && newMotions.ContainsKey(child.motion))
                            {
                                child.motion = newMotions[child.motion];
                            }
                            return child;
                        }).ToArray();
                        motion = newTree;
                        AssetDatabase.AddObjectToAsset(newTree, outFile);
                    }
                    cloneController.SetStateEffectiveMotion(childState.state, motion, layerIndex);
                }
            }

            AssetDatabase.SaveAssets();
            return cloneController;
        }

        /// <summary>
        /// Gets deep copy of blend tree.
        /// </summary>
        /// <param name="blendTree">Blend tree to copy.</param>
        /// <returns>Copied blend tree.</returns>
        internal static BlendTree DeepCopyBlendTree(BlendTree blendTree)
        {
            var originalSO = new SerializedObject(blendTree);
            var originalProp = originalSO.GetIterator();
            var newTree = new BlendTree();
            var newSO = new SerializedObject(newTree);

            while (originalProp.Next(true))
            {
                newSO.CopyFromSerializedProperty(originalProp);
            }
            newSO.ApplyModifiedProperties();

            newTree.children = blendTree.children.Select(child =>
                {
                    var newChild = new ChildMotion()
                    {
                        cycleOffset = child.cycleOffset,
                        directBlendParameter = child.directBlendParameter,
                        mirror = child.mirror,
                        motion = child.motion,
                        position = child.position,
                        threshold = child.threshold,
                        timeScale = child.timeScale,
                    };
                    return newChild;
                }).ToArray();
            return newTree;
        }

        /// <summary>
        /// Gets materials which a clip uses.
        /// </summary>
        /// <param name="clip">Animation Clip.</param>
        /// <returns>Materials.</returns>
        internal static Material[] GetMaterials(AnimationClip clip)
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
