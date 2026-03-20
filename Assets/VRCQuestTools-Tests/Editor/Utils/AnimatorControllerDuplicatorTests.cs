// Tests for AnimatorControllerDuplicator
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    internal class AnimatorControllerDuplicatorTests
    {
        [Test]
        public void Duplicate_EmptyController_ReturnsNewController()
        {
            var original = new AnimatorController();
            original.name = "TestController";
            original.AddLayer("BaseLayer");
            try
            {
                var duplicator = new AnimatorControllerDuplicator();
                var copy = duplicator.Duplicate(original);
                try
                {
                    Assert.IsNotNull(copy);
                    Assert.AreNotSame(original, copy);
                    Assert.AreEqual(1, copy.layers.Length);
                }
                finally
                {
                    Object.DestroyImmediate(copy);
                }
            }
            finally
            {
                Object.DestroyImmediate(original);
            }
        }

        [Test]
        public void Duplicate_WithParameters_CopiesParameters()
        {
            var original = new AnimatorController();
            original.name = "TestController";
            original.AddLayer("BaseLayer");
            original.AddParameter("FloatParam", AnimatorControllerParameterType.Float);
            original.AddParameter("BoolParam", AnimatorControllerParameterType.Bool);
            original.AddParameter("IntParam", AnimatorControllerParameterType.Int);
            original.AddParameter("TriggerParam", AnimatorControllerParameterType.Trigger);
            try
            {
                var duplicator = new AnimatorControllerDuplicator();
                var copy = duplicator.Duplicate(original);
                try
                {
                    Assert.AreEqual(4, copy.parameters.Length);
                    Assert.AreEqual("FloatParam", copy.parameters[0].name);
                    Assert.AreEqual(AnimatorControllerParameterType.Float, copy.parameters[0].type);
                    Assert.AreEqual("BoolParam", copy.parameters[1].name);
                    Assert.AreEqual(AnimatorControllerParameterType.Bool, copy.parameters[1].type);
                    Assert.AreEqual("IntParam", copy.parameters[2].name);
                    Assert.AreEqual(AnimatorControllerParameterType.Int, copy.parameters[2].type);
                    Assert.AreEqual("TriggerParam", copy.parameters[3].name);
                    Assert.AreEqual(AnimatorControllerParameterType.Trigger, copy.parameters[3].type);
                }
                finally
                {
                    Object.DestroyImmediate(copy);
                }
            }
            finally
            {
                Object.DestroyImmediate(original);
            }
        }

        [Test]
        public void Duplicate_WithState_CopiesState()
        {
            var original = new AnimatorController();
            original.name = "TestController";
            original.AddLayer("BaseLayer");
            var layer = original.layers[0];
            var state = layer.stateMachine.AddState("TestState");
            state.speed = 2.5f;
            state.tag = "TestTag";
            try
            {
                var duplicator = new AnimatorControllerDuplicator();
                var copy = duplicator.Duplicate(original);
                try
                {
                    Assert.AreEqual(1, copy.layers.Length);
                    var copyLayer = copy.layers[0];
                    Assert.AreEqual(1, copyLayer.stateMachine.states.Length);
                    var copyState = copyLayer.stateMachine.states[0].state;
                    Assert.AreEqual("TestState", copyState.name);
                    Assert.AreEqual(2.5f, copyState.speed);
                    Assert.AreEqual("TestTag", copyState.tag);
                    Assert.AreNotSame(state, copyState);
                }
                finally
                {
                    Object.DestroyImmediate(copy);
                }
            }
            finally
            {
                Object.DestroyImmediate(original);
            }
        }

        [Test]
        public void Duplicate_WithTransitions_CopiesTransitions()
        {
            var original = new AnimatorController();
            original.name = "TestController";
            original.AddLayer("BaseLayer");
            var layer = original.layers[0];
            var stateA = layer.stateMachine.AddState("StateA");
            var stateB = layer.stateMachine.AddState("StateB");
            var transition = stateA.AddTransition(stateB);
            transition.duration = 0.5f;
            transition.hasExitTime = true;
            transition.exitTime = 0.75f;
            try
            {
                var duplicator = new AnimatorControllerDuplicator();
                var copy = duplicator.Duplicate(original);
                try
                {
                    var copyLayer = copy.layers[0];
                    Assert.AreEqual(2, copyLayer.stateMachine.states.Length);
                    var copyStateA = copyLayer.stateMachine.states[0].state;
                    Assert.AreEqual(1, copyStateA.transitions.Length);
                    var copyTransition = copyStateA.transitions[0];
                    Assert.AreEqual(0.5f, copyTransition.duration);
                    Assert.IsTrue(copyTransition.hasExitTime);
                    Assert.AreEqual(0.75f, copyTransition.exitTime);
                }
                finally
                {
                    Object.DestroyImmediate(copy);
                }
            }
            finally
            {
                Object.DestroyImmediate(original);
            }
        }

        [Test]
        public void Duplicate_WithConditions_CopiesConditions()
        {
            var original = new AnimatorController();
            original.name = "TestController";
            original.AddParameter("Speed", AnimatorControllerParameterType.Float);
            original.AddLayer("BaseLayer");
            var layer = original.layers[0];
            var stateA = layer.stateMachine.AddState("StateA");
            var stateB = layer.stateMachine.AddState("StateB");
            var transition = stateA.AddTransition(stateB);
            transition.AddCondition(AnimatorConditionMode.Greater, 0.5f, "Speed");
            try
            {
                var duplicator = new AnimatorControllerDuplicator();
                var copy = duplicator.Duplicate(original);
                try
                {
                    var copyTransition = copy.layers[0].stateMachine.states[0].state.transitions[0];
                    Assert.AreEqual(1, copyTransition.conditions.Length);
                    Assert.AreEqual(AnimatorConditionMode.Greater, copyTransition.conditions[0].mode);
                    Assert.AreEqual(0.5f, copyTransition.conditions[0].threshold);
                    Assert.AreEqual("Speed", copyTransition.conditions[0].parameter);
                }
                finally
                {
                    Object.DestroyImmediate(copy);
                }
            }
            finally
            {
                Object.DestroyImmediate(original);
            }
        }

        [Test]
        public void Duplicate_WithSubStateMachine_CopiesSubStateMachine()
        {
            var original = new AnimatorController();
            original.name = "TestController";
            original.AddLayer("BaseLayer");
            var layer = original.layers[0];
            var subSM = layer.stateMachine.AddStateMachine("SubSM");
            subSM.AddState("SubState");
            try
            {
                var duplicator = new AnimatorControllerDuplicator();
                var copy = duplicator.Duplicate(original);
                try
                {
                    var copyLayer = copy.layers[0];
                    Assert.AreEqual(1, copyLayer.stateMachine.stateMachines.Length);
                    var copySub = copyLayer.stateMachine.stateMachines[0].stateMachine;
                    Assert.AreEqual("SubSM", copySub.name);
                    Assert.AreEqual(1, copySub.states.Length);
                    Assert.AreEqual("SubState", copySub.states[0].state.name);
                    Assert.AreNotSame(subSM, copySub);
                }
                finally
                {
                    Object.DestroyImmediate(copy);
                }
            }
            finally
            {
                Object.DestroyImmediate(original);
            }
        }

        [Test]
        public void Duplicate_WithBlendTree_CopiesBlendTree()
        {
            var original = new AnimatorController();
            original.name = "TestController";
            original.AddParameter("Blend", AnimatorControllerParameterType.Float);
            original.AddLayer("BaseLayer");
            var layer = original.layers[0];
            var state = layer.stateMachine.AddState("BlendState");
            var blendTree = new BlendTree();
            blendTree.blendParameter = "Blend";
            blendTree.blendType = BlendTreeType.Simple1D;
            state.motion = blendTree;
            try
            {
                var duplicator = new AnimatorControllerDuplicator();
                var copy = duplicator.Duplicate(original);
                try
                {
                    var copyState = copy.layers[0].stateMachine.states[0].state;
                    Assert.IsNotNull(copyState.motion);
                    Assert.IsInstanceOf<BlendTree>(copyState.motion);
                    var copyBlend = (BlendTree)copyState.motion;
                    Assert.AreEqual("Blend", copyBlend.blendParameter);
                }
                finally
                {
                    Object.DestroyImmediate(copy);
                }
            }
            finally
            {
                Object.DestroyImmediate(blendTree);
                Object.DestroyImmediate(original);
            }
        }

        [Test]
        public void Duplicate_WithAnyStateTransition_CopiesAnyStateTransitions()
        {
            var original = new AnimatorController();
            original.name = "TestController";
            original.AddLayer("BaseLayer");
            var layer = original.layers[0];
            var stateA = layer.stateMachine.AddState("StateA");
            layer.stateMachine.AddAnyStateTransition(stateA);
            try
            {
                var duplicator = new AnimatorControllerDuplicator();
                var copy = duplicator.Duplicate(original);
                try
                {
                    var copyLayer = copy.layers[0];
                    Assert.AreEqual(1, copyLayer.stateMachine.anyStateTransitions.Length);
                }
                finally
                {
                    Object.DestroyImmediate(copy);
                }
            }
            finally
            {
                Object.DestroyImmediate(original);
            }
        }

        [Test]
        public void Duplicate_WithEntryTransitions_CopiesEntryTransitions()
        {
            var original = new AnimatorController();
            original.name = "TestController";
            original.AddLayer("BaseLayer");
            var layer = original.layers[0];
            var stateA = layer.stateMachine.AddState("StateA");
            var stateB = layer.stateMachine.AddState("StateB");
            layer.stateMachine.AddEntryTransition(stateB);
            try
            {
                var duplicator = new AnimatorControllerDuplicator();
                var copy = duplicator.Duplicate(original);
                try
                {
                    var copyLayer = copy.layers[0];
                    Assert.AreEqual(1, copyLayer.stateMachine.entryTransitions.Length);
                }
                finally
                {
                    Object.DestroyImmediate(copy);
                }
            }
            finally
            {
                Object.DestroyImmediate(original);
            }
        }
    }
}
