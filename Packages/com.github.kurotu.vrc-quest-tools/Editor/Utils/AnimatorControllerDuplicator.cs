using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Deeply duplicates AnimatorController.
    /// </summary>
    internal class AnimatorControllerDuplicator
    {
        private Dictionary<AnimatorStateMachine, AnimatorStateMachine> stateMachineMap = new Dictionary<AnimatorStateMachine, AnimatorStateMachine>();
        private Dictionary<AnimatorState, AnimatorState> stateMap = new Dictionary<AnimatorState, AnimatorState>();

        /// <summary>
        /// Deeply duplicates AnimatorController.
        /// </summary>
        /// <param name="controller">Animator Controller.</param>
        /// <returns>Duplicated one.</returns>
        internal AnimatorController Duplicate(AnimatorController controller)
        {
            ClearMaps();

            if (controller == null)
            {
                return null;
            }

            var newController = Object.Instantiate(controller);
            newController.hideFlags = controller.hideFlags;
            newController.layers = controller.layers.Select(Duplicate).ToArray();
            newController.name = controller.name;
            newController.parameters = controller.parameters.Select(Duplicate).ToArray();

            ClearMaps();

            return newController;
        }

        private void ClearMaps()
        {
            stateMachineMap.Clear();
            stateMap.Clear();
        }

        private AnimatorControllerLayer Duplicate(AnimatorControllerLayer layer)
        {
            if (layer == null)
            {
                return null;
            }

            var newLayer = new AnimatorControllerLayer();
            newLayer.avatarMask = layer.avatarMask;
            newLayer.blendingMode = layer.blendingMode;
            newLayer.defaultWeight = layer.defaultWeight;
            newLayer.iKPass = layer.iKPass;
            newLayer.name = layer.name;
            newLayer.syncedLayerIndex = layer.syncedLayerIndex;
            newLayer.stateMachine = Duplicate(layer.stateMachine);
            return newLayer;
        }

        private AnimatorStateMachine Duplicate(AnimatorStateMachine stateMachine)
        {
            if (stateMachine == null)
            {
                return null;
            }
            if (stateMachineMap.ContainsKey(stateMachine))
            {
                return stateMachineMap[stateMachine];
            }

            var newStateMachine = new AnimatorStateMachine();

            stateMachineMap[stateMachine] = newStateMachine;

            newStateMachine.anyStatePosition = stateMachine.anyStatePosition;
            newStateMachine.anyStateTransitions = stateMachine.anyStateTransitions.Select(Duplicate).ToArray();
            newStateMachine.behaviours = stateMachine.behaviours.Select(Duplicate).ToArray();
            newStateMachine.defaultState = Duplicate(stateMachine.defaultState);
            newStateMachine.entryPosition = stateMachine.entryPosition;
            newStateMachine.entryTransitions = stateMachine.entryTransitions.Select(Duplicate).ToArray();
            newStateMachine.exitPosition = stateMachine.exitPosition;
            newStateMachine.hideFlags = stateMachine.hideFlags;
            newStateMachine.name = stateMachine.name;
            newStateMachine.parentStateMachinePosition = stateMachine.parentStateMachinePosition;
            newStateMachine.states = stateMachine.states.Select(Duplicate).ToArray();
            newStateMachine.stateMachines = stateMachine.stateMachines.Select(Duplicate).ToArray();
            return newStateMachine;
        }

        private ChildAnimatorStateMachine Duplicate(ChildAnimatorStateMachine child)
        {
            var newChild = new ChildAnimatorStateMachine();
            newChild.position = child.position;
            newChild.stateMachine = Duplicate(child.stateMachine);
            return newChild;
        }

        private ChildAnimatorState Duplicate(ChildAnimatorState child)
        {
            var newChild = new ChildAnimatorState();
            newChild.position = child.position;
            newChild.state = Duplicate(child.state);
            return newChild;
        }

        private AnimatorState Duplicate(AnimatorState state)
        {
            if (state == null)
            {
                return null;
            }
            if (stateMap.ContainsKey(state))
            {
                return stateMap[state];
            }

            var newState = new AnimatorState();

            stateMap[state] = newState;

            newState.behaviours = state.behaviours.Select(Duplicate).ToArray();
            newState.cycleOffset = state.cycleOffset;
            newState.cycleOffsetParameter = state.cycleOffsetParameter;
            newState.cycleOffsetParameterActive = state.cycleOffsetParameterActive;
            newState.hideFlags = state.hideFlags;
            newState.iKOnFeet = state.iKOnFeet;
            newState.mirror = state.mirror;
            newState.mirrorParameter = state.mirrorParameter;
            newState.mirrorParameterActive = state.mirrorParameterActive;
            newState.motion = state.motion;
            newState.name = state.name;
            newState.speed = state.speed;
            newState.speedParameter = state.speedParameter;
            newState.speedParameterActive = state.speedParameterActive;
            newState.tag = state.tag;
            newState.timeParameter = state.timeParameter;
            newState.timeParameterActive = state.timeParameterActive;
            newState.transitions = state.transitions.Select(Duplicate).ToArray();
            newState.writeDefaultValues = state.writeDefaultValues;
            return newState;
        }

        private AnimatorTransition Duplicate(AnimatorTransition transition)
        {
            if (transition == null)
            {
                return null;
            }

            var newTransition = new AnimatorTransition();
            newTransition.conditions = transition.conditions.Select(Duplicate).ToArray();
            newTransition.destinationState = Duplicate(transition.destinationState);
            newTransition.destinationStateMachine = Duplicate(transition.destinationStateMachine);
            newTransition.hideFlags = transition.hideFlags;
            newTransition.isExit = transition.isExit;
            newTransition.mute = transition.mute;
            newTransition.name = transition.name;
            newTransition.solo = transition.solo;
            return newTransition;
        }

        private AnimatorStateTransition Duplicate(AnimatorStateTransition transition)
        {
            if (transition == null)
            {
                return null;
            }

            var newTransition = new AnimatorStateTransition();
            newTransition.canTransitionToSelf = transition.canTransitionToSelf;
            newTransition.conditions = transition.conditions.Select(Duplicate).ToArray();
            newTransition.destinationState = Duplicate(transition.destinationState);
            newTransition.destinationStateMachine = Duplicate(transition.destinationStateMachine);
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

        private AnimatorCondition Duplicate(AnimatorCondition condition)
        {
            var newCondition = new AnimatorCondition();
            newCondition.mode = condition.mode;
            newCondition.parameter = condition.parameter;
            newCondition.threshold = condition.threshold;
            return newCondition;
        }

        private AnimatorControllerParameter Duplicate(AnimatorControllerParameter parameter)
        {
            if (parameter == null)
            {
                return null;
            }

            var newParam = new AnimatorControllerParameter();
            newParam.defaultBool = parameter.defaultBool;
            newParam.defaultFloat = parameter.defaultFloat;
            newParam.defaultInt = parameter.defaultInt;
            newParam.name = parameter.name;
            newParam.type = parameter.type;
            return newParam;
        }

        private StateMachineBehaviour Duplicate(StateMachineBehaviour behaviour)
        {
            if (behaviour == null)
            {
                return null;
            }

            var newBehaviour = Object.Instantiate(behaviour);
            return newBehaviour;
        }
    }
}
