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
                .SelectMany(state =>
                {
                    if (state.state.motion is BlendTree tree)
                    {
                        var descendants = GetDescendantBlendTrees(tree, new List<BlendTree>());
                        return descendants.Append(tree);
                    }
                    return new BlendTree[] { };
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
                if (typeof(Renderer).IsAssignableFrom(binding[j].type))
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
        /// <param name="saveAsAsset">Whether to save as asset.</param>
        /// <param name="saveDir">Asset directory for new controller.</param>
        /// <param name="newMotions">Animation clips to replace (key: original clip).</param>
        /// <returns>New animator controller.</returns>
        internal static AnimatorController ReplaceAnimationClips(AnimatorController controller, bool saveAsAsset, string saveDir, Dictionary<Motion, Motion> newMotions)
        {
            string outFile = null;
            AnimatorController cloneController = DeepCopyAnimatorController(controller);
            cloneController.name = controller.name + "(Clone)";
            if (saveAsAsset)
            {
                Directory.CreateDirectory($"{saveDir}/AnimatorControllers");
                AssetDatabase.Refresh();

                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(controller, out string guid, out long localId);
                outFile = $"{saveDir}/AnimatorControllers/{controller.name}_from_{guid}.controller";
                cloneController = AssetUtility.CreateAsset(cloneController, outFile, c =>
                {
                    foreach (var obj in AssetUtility.GetAllObjectReferences(cloneController))
                    {
                        if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(obj)))
                        {
                            AssetDatabase.AddObjectToAsset(obj, c);
                        }
                    }
                });
            }

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
                ReplaceAnimationClips(saveAsAsset, newMotions, outFile, cloneController, layerIndex, stateMachine);
            }

            AssetDatabase.SaveAssets();
            return cloneController;
        }

        /// <summary>
        /// Replace override controller's animation clips with new clips.
        /// </summary>
        /// <param name="controller">Target controller.</param>
        /// <param name="saveAsAsset">Whether to save as asset.</param>
        /// <param name="saveDir">Asset directory for new controller.</param>
        /// <param name="newMotions">Animation clips to replace (key: original clip).</param>
        /// <returns>New animator override controller.</returns>
        internal static AnimatorOverrideController ReplaceAnimationClips(AnimatorOverrideController controller, bool saveAsAsset, string saveDir, Dictionary<Motion, Motion> newMotions)
        {
            string outFile = null;
            AnimatorOverrideController cloneController = Object.Instantiate(controller);
            cloneController.name = controller.name + "(Clone)";
            if (saveAsAsset)
            {
                Directory.CreateDirectory($"{saveDir}/AnimatorControllers");
                AssetDatabase.Refresh();

                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(controller, out string guid, out long localId);
                outFile = $"{saveDir}/AnimatorControllers/{controller.name}_from_{guid}.overrideController";
                cloneController = AssetUtility.CreateAsset(cloneController, outFile);
            }

            var clipPairs = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            cloneController.GetOverrides(clipPairs);
            foreach (var pair in clipPairs)
            {
                if (pair.Value && newMotions.ContainsKey(pair.Value))
                {
                    cloneController[pair.Key] = newMotions[pair.Value] as AnimationClip;
                }
            }
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
            binding = binding.Where(b => typeof(Renderer).IsAssignableFrom(b.type)).ToArray(); // Renderer系のみ

            var keyframes = binding.SelectMany(b => AnimationUtility.GetObjectReferenceCurve(clip, b)); // keyframeに設定されているオブジェクト
            var materials = keyframes.Where(keyframe => keyframe.value && keyframe.value.GetType() == typeof(Material)) // マテリアルのみ取得
                .Select(keyframe => (Material)keyframe.value); // マテリアルに変換

            return materials.Where(m => m != null).Distinct().ToArray();
        }

        private static void ReplaceAnimationClips(bool saveAsAsset, Dictionary<Motion, Motion> newMotions, string outFile, AnimatorController cloneController, int layerIndex, AnimatorStateMachine stateMachine)
        {
            foreach (var childState in stateMachine.states)
            {
                var motion = cloneController.GetStateEffectiveMotion(childState.state, layerIndex);
                if (VRCSDKUtility.IsExampleAsset(motion))
                {
                    continue;
                }

                if (motion != null && newMotions.ContainsKey(motion))
                {
                    var newMotion = newMotions[motion];
                    if (newMotion is BlendTree tree)
                    {
                        tree.children = tree.children.Select(child =>
                        {
                            if (child.motion != null && newMotions.ContainsKey(child.motion))
                            {
                                child.motion = newMotions[child.motion];
                            }
                            return child;
                        }).ToArray();
                        if (saveAsAsset && !AssetDatabase.IsMainAsset(tree))
                        {
                            AssetDatabase.AddObjectToAsset(tree, outFile);
                        }
                    }
                    SetStateEffectiveMotion(cloneController, childState.state, newMotion, layerIndex);
                }
            }

            foreach (var childStateMachine in stateMachine.stateMachines)
            {
                ReplaceAnimationClips(saveAsAsset, newMotions, outFile, cloneController, layerIndex, childStateMachine.stateMachine);
            }
        }

        private static AnimatorController DeepCopyAnimatorController(AnimatorController controller)
        {
            var duplicator = new AnimatorControllerDuplicator();
            return duplicator.Duplicate(controller);
        }

        private static BlendTree[] GetDescendantBlendTrees(BlendTree blendTree, List<BlendTree> currentTrees)
        {
            currentTrees.Add(blendTree);
            var childTrees = blendTree.children
                .Where(child => child.motion is BlendTree)
                .Select(child => child.motion as BlendTree)
                .Where(tree => !currentTrees.Contains(tree))
                .ToArray();
            var descendants = childTrees.SelectMany(tree => GetDescendantBlendTrees(tree, currentTrees));
            return childTrees.Concat(descendants).Distinct().ToArray();
        }

        /// <summary>
        /// Simplified AnimatorController.SetStateEffectiveMotion.
        /// This method doesn't destroy the original blendtreee motion.
        /// </summary>
        /// <param name="controller">Target Animator Controller.</param>
        /// <param name="state">State to set the motion.</param>
        /// <param name="motion">Motion to set.</param>
        /// <param name="layerIndex">Layer index to manipulate.</param>
        private static void SetStateEffectiveMotion(AnimatorController controller, AnimatorState state, Motion motion, int layerIndex)
        {
            if (controller.layers[layerIndex].syncedLayerIndex == -1)
            {
                state.motion = motion;
                return;
            }

            AnimatorControllerLayer[] array = controller.layers;
            array[layerIndex].SetOverrideMotion(state, motion);
            controller.layers = array;
        }
    }
}
