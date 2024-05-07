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
        /// <param name="saveAsAsset">Whether to save as asset.</param>
        /// <param name="saveDir">Asset directory for new controller.</param>
        /// <param name="newMotions">Animation clips to replace (key: original clip).</param>
        /// <returns>New animator controller.</returns>
        internal static AnimatorController ReplaceAnimationClips(RuntimeAnimatorController controller, bool saveAsAsset, string saveDir, Dictionary<Motion, Motion> newMotions)
        {
            string outFile = null;
            AnimatorController cloneController;
            if (saveAsAsset)
            {
                Directory.CreateDirectory($"{saveDir}/AnimatorControllers");
                AssetDatabase.Refresh();

                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(controller, out string guid, out long localId);
                outFile = $"{saveDir}/AnimatorControllers/{controller.name}_from_{guid}.controller";
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(controller), outFile);
                AssetDatabase.Refresh();
                cloneController = (AnimatorController)AssetDatabase.LoadAssetAtPath(outFile, typeof(AnimatorController));
            }
            else
            {
                cloneController = DeepCopyAnimatorController((AnimatorController)controller);
                cloneController.name = controller.name + "(Clone)";
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
                    if (saveAsAsset)
                    {
                        AssetDatabase.AddObjectToAsset(newTree, outFile);
                    }
                }
                cloneController.SetStateEffectiveMotion(childState.state, motion, layerIndex);
            }

            foreach (var childStateMachine in stateMachine.stateMachines)
            {
                ReplaceAnimationClips(saveAsAsset, newMotions, outFile, cloneController, layerIndex, childStateMachine.stateMachine);
            }
        }

        private static AnimatorController DeepCopyAnimatorController(AnimatorController controller)
        {
            var stateMachineMap = new Dictionary<AnimatorStateMachine, AnimatorStateMachine>();
            var stateMap = new Dictionary<AnimatorState, AnimatorState>();
            var newController = new AnimatorController();
            newController.hideFlags = controller.hideFlags;
            newController.layers = controller.layers.Select(layer =>
            {
                var newLayer = new AnimatorControllerLayer();
                newLayer.avatarMask = layer.avatarMask;
                newLayer.blendingMode = layer.blendingMode;
                newLayer.defaultWeight = layer.defaultWeight;
                newLayer.iKPass = layer.iKPass;
                newLayer.name = layer.name;
                newLayer.syncedLayerIndex = layer.syncedLayerIndex;
                newLayer.stateMachine = DeepCopyStateMachine(layer.stateMachine, stateMachineMap, stateMap);
                return newLayer;
            }).ToArray();
            newController.name = controller.name;
            newController.parameters = controller.parameters.Select(p =>
            {
                var newParam = new AnimatorControllerParameter();
                newParam.defaultBool = p.defaultBool;
                newParam.defaultFloat = p.defaultFloat;
                newParam.defaultInt = p.defaultInt;
                newParam.name = p.name;
                newParam.type = p.type;
                return newParam;
            }).ToArray();

            return newController;
        }

        private static AnimatorStateMachine DeepCopyStateMachine(AnimatorStateMachine stateMachine, Dictionary<AnimatorStateMachine, AnimatorStateMachine> stateMachineMap, Dictionary<AnimatorState, AnimatorState> stateMap)
        {
            var newMachine = new AnimatorStateMachine();

            newMachine.anyStatePosition = stateMachine.anyStatePosition;
            newMachine.anyStateTransitions = stateMachine.anyStateTransitions.Select(DeepCopyAnimatorStateTransition).ToArray();
            newMachine.behaviours = stateMachine.behaviours.Select(b =>
            {
                var newBehaviour = Object.Instantiate(b);
                return newBehaviour;
            }).ToArray();
            newMachine.entryPosition = stateMachine.entryPosition;
            newMachine.entryTransitions = stateMachine.entryTransitions.Select(DeepCopyAnimatorTransition).ToArray();
            newMachine.exitPosition = stateMachine.exitPosition;
            newMachine.hideFlags = stateMachine.hideFlags;
            newMachine.name = stateMachine.name;
            newMachine.parentStateMachinePosition = stateMachine.parentStateMachinePosition;
            newMachine.stateMachines = stateMachine.stateMachines.Select(machine =>
            {
                var newChild = DeepCopyChildAnimatorStateMachine(machine, stateMachineMap, stateMap);
                stateMachineMap[machine.stateMachine] = newChild.stateMachine;
                return newChild;
            }).ToArray();
            newMachine.states = stateMachine.states.Select(state =>
            {
                var newChild = DeepCopyChildAnimatorState(state);
                stateMap[state.state] = newChild.state;
                return newChild;
            }).ToArray();

            if (stateMachine.defaultState != null)
            {
                newMachine.defaultState = stateMap[stateMachine.defaultState];
            }
            newMachine.anyStateTransitions = newMachine.anyStateTransitions.Select(t =>
            {
                if (t.destinationState != null)
                {
                    t.destinationState = stateMap[t.destinationState];
                }
                if (t.destinationStateMachine != null)
                {
                    t.destinationStateMachine = stateMachineMap[t.destinationStateMachine];
                }
                return t;
            }).ToArray();
            newMachine.entryTransitions = newMachine.entryTransitions.Select(t =>
            {
                if (t.destinationState != null)
                {
                    t.destinationState = stateMap[t.destinationState];
                }
                if (t.destinationStateMachine != null)
                {
                    t.destinationStateMachine = stateMachineMap[t.destinationStateMachine];
                }
                return t;
            }).ToArray();
            newMachine.states = newMachine.states.Select(s =>
            {
                s.state.transitions = s.state.transitions.Select(t =>
                {
                    if (t.destinationState != null)
                    {
                        t.destinationState = stateMap[t.destinationState];
                    }
                    if (t.destinationStateMachine != null)
                    {
                        t.destinationStateMachine = stateMachineMap[t.destinationStateMachine];
                    }
                    return t;
                }).ToArray();
                return s;
            }).ToArray();

            return newMachine;
        }

        private static AnimatorStateTransition DeepCopyAnimatorStateTransition(AnimatorStateTransition transition)
        {
            var newTransition = new AnimatorStateTransition();
            newTransition.canTransitionToSelf = transition.canTransitionToSelf;
            newTransition.conditions = transition.conditions.Select(c =>
            {
                var newCondition = new AnimatorCondition();
                newCondition.mode = c.mode;
                newCondition.parameter = c.parameter;
                newCondition.threshold = c.threshold;
                return newCondition;
            }).ToArray();
            newTransition.destinationState = transition.destinationState;
            newTransition.destinationStateMachine = transition.destinationStateMachine;
            newTransition.duration = transition.duration;
            newTransition.exitTime = transition.exitTime;
            newTransition.hasExitTime = transition.hasExitTime;
            newTransition.hasFixedDuration = transition.hasFixedDuration;
            newTransition.hideFlags = transition.hideFlags;
            newTransition.interruptionSource = transition.interruptionSource;
            newTransition.isExit = transition.isExit;
            newTransition.mute = transition.mute;
            newTransition.name = transition.name;
            newTransition.offset = transition.offset;
            newTransition.orderedInterruption = transition.orderedInterruption;
            newTransition.solo = transition.solo;
            return newTransition;
        }

        private static AnimatorTransition DeepCopyAnimatorTransition(AnimatorTransition transition)
        {
            var newTransition = new AnimatorTransition();
            newTransition.conditions = transition.conditions.Select(c =>
            {
                var newCondition = new AnimatorCondition();
                newCondition.mode = c.mode;
                newCondition.parameter = c.parameter;
                newCondition.threshold = c.threshold;
                return newCondition;
            }).ToArray();
            newTransition.destinationState = transition.destinationState;
            newTransition.destinationStateMachine = transition.destinationStateMachine;
            newTransition.hideFlags = transition.hideFlags;
            newTransition.isExit = transition.isExit;
            newTransition.mute = transition.mute;
            newTransition.name = transition.name;
            newTransition.solo = transition.solo;
            return newTransition;
        }

        private static ChildAnimatorState DeepCopyChildAnimatorState(ChildAnimatorState state)
        {
            var newState = new ChildAnimatorState();
            newState.position = state.position;
            newState.state = new AnimatorState();
            newState.state.behaviours = state.state.behaviours.Select(b =>
            {
                var newBehaviour = Object.Instantiate(b);
                return newBehaviour;
            }).ToArray();
            newState.state.cycleOffset = state.state.cycleOffset;
            newState.state.cycleOffsetParameter = state.state.cycleOffsetParameter;
            newState.state.cycleOffsetParameterActive = state.state.cycleOffsetParameterActive;
            newState.state.hideFlags = state.state.hideFlags;
            newState.state.iKOnFeet = state.state.iKOnFeet;
            newState.state.mirror = state.state.mirror;
            newState.state.mirrorParameter = state.state.mirrorParameter;
            newState.state.mirrorParameterActive = state.state.mirrorParameterActive;
            newState.state.motion = state.state.motion;
            newState.state.name = state.state.name;
            newState.state.speed = state.state.speed;
            newState.state.speedParameter = state.state.speedParameter;
            newState.state.speedParameterActive = state.state.speedParameterActive;
            newState.state.tag = state.state.tag;
            newState.state.timeParameter = state.state.timeParameter;
            newState.state.timeParameterActive = state.state.timeParameterActive;
            newState.state.transitions = state.state.transitions.Select(DeepCopyAnimatorStateTransition).ToArray();
            newState.state.writeDefaultValues = state.state.writeDefaultValues;
            return newState;
        }

        private static ChildAnimatorStateMachine DeepCopyChildAnimatorStateMachine(ChildAnimatorStateMachine stateMachine, Dictionary<AnimatorStateMachine, AnimatorStateMachine> stateMachineMap, Dictionary<AnimatorState, AnimatorState> stateMap)
        {
            var newStateMachine = new ChildAnimatorStateMachine();
            newStateMachine.position = stateMachine.position;
            newStateMachine.stateMachine = DeepCopyStateMachine(stateMachine.stateMachine, stateMachineMap, stateMap);
            return newStateMachine;
        }
    }
}
