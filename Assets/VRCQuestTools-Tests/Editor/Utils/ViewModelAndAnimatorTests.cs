// Tests for MSMapGenViewModel, AnimatorControllerDuplicator, MaterialWrapperBuilder additional - using System;
using System.Linq;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using KRT.VRCQuestTools.Utils;
using KRT.VRCQuestTools.ViewModels;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests
{
    // --- MSMapGenViewModel Tests ---

    [TestFixture]
    public class MSMapGenViewModelAdditionalTests
    {
        [Test]
        public void DisableGenerateButton_BothNull_ReturnsTrue()
        {
            var vm = new MSMapGenViewModel();
            vm.metallicMap = null;
            vm.smoothnessMap = null;
            Assert.IsTrue(vm.DisableGenerateButton);
        }

        [Test]
        public void DisableGenerateButton_MetallicOnly_ReturnsFalse()
        {
            Texture2D tex = null;
            try
            {
                tex = new Texture2D(4, 4);
                var vm = new MSMapGenViewModel();
                vm.metallicMap = tex;
                vm.smoothnessMap = null;
                Assert.IsFalse(vm.DisableGenerateButton);
            }
            finally
            {
                if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void DisableGenerateButton_SmoothnessOnly_ReturnsFalse()
        {
            Texture2D tex = null;
            try
            {
                tex = new Texture2D(4, 4);
                var vm = new MSMapGenViewModel();
                vm.metallicMap = null;
                vm.smoothnessMap = tex;
                Assert.IsFalse(vm.DisableGenerateButton);
            }
            finally
            {
                if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void DisableGenerateButton_BothSet_ReturnsFalse()
        {
            Texture2D tex1 = null, tex2 = null;
            try
            {
                tex1 = new Texture2D(4, 4);
                tex2 = new Texture2D(4, 4);
                var vm = new MSMapGenViewModel();
                vm.metallicMap = tex1;
                vm.smoothnessMap = tex2;
                Assert.IsFalse(vm.DisableGenerateButton);
            }
            finally
            {
                if (tex1 != null) UnityEngine.Object.DestroyImmediate(tex1);
                if (tex2 != null) UnityEngine.Object.DestroyImmediate(tex2);
            }
        }

        [Test]
        public void InvertSmoothness_DefaultFalse()
        {
            var vm = new MSMapGenViewModel();
            Assert.IsFalse(vm.invertSmoothness);
        }
    }

    // --- AnimatorControllerDuplicator Additional Tests ---

    [TestFixture]
    public class AnimatorControllerDuplicatorAdditionalTests
    {
        [Test]
        public void Duplicate_EmptyController_CreatesNewInstance()
        {
            AnimatorController original = null;
            AnimatorController copy = null;
            try
            {
                original = new AnimatorController();
                original.name = "OriginalController";
                original.AddLayer("Base Layer");
                var duplicator = new AnimatorControllerDuplicator();
                copy = duplicator.Duplicate(original);
                Assert.IsNotNull(copy);
                Assert.AreNotSame(original, copy);
                Assert.AreEqual(original.layers.Length, copy.layers.Length);
            }
            finally
            {
                if (original != null) UnityEngine.Object.DestroyImmediate(original);
                if (copy != null) UnityEngine.Object.DestroyImmediate(copy);
            }
        }

        [Test]
        public void Duplicate_ControllerWithState_CopiesStates()
        {
            AnimatorController original = null;
            AnimatorController copy = null;
            try
            {
                original = new AnimatorController();
                original.name = "WithStates";
                original.AddLayer("Base Layer");
                var layer = original.layers[0];
                var state = layer.stateMachine.AddState("TestState");
                state.speed = 2.0f;

                var duplicator = new AnimatorControllerDuplicator();
                copy = duplicator.Duplicate(original);
                Assert.IsNotNull(copy);
                Assert.AreEqual(1, copy.layers.Length);
                var copyStates = copy.layers[0].stateMachine.states;
                Assert.AreEqual(1, copyStates.Length);
                Assert.AreEqual("TestState", copyStates[0].state.name);
                Assert.AreEqual(2.0f, copyStates[0].state.speed, 0.001f);
            }
            finally
            {
                if (original != null) UnityEngine.Object.DestroyImmediate(original);
                if (copy != null) UnityEngine.Object.DestroyImmediate(copy);
            }
        }

        [Test]
        public void Duplicate_ControllerWithTransition_CopiesTransitions()
        {
            AnimatorController original = null;
            AnimatorController copy = null;
            try
            {
                original = new AnimatorController();
                original.name = "WithTransitions";
                original.AddLayer("Base Layer");
                var layer = original.layers[0];
                var stateA = layer.stateMachine.AddState("StateA");
                var stateB = layer.stateMachine.AddState("StateB");
                var transition = stateA.AddTransition(stateB);
                transition.duration = 0.5f;
                transition.hasExitTime = true;

                var duplicator = new AnimatorControllerDuplicator();
                copy = duplicator.Duplicate(original);
                Assert.IsNotNull(copy);
                var copyStates = copy.layers[0].stateMachine.states;
                Assert.AreEqual(2, copyStates.Length);

                // Check transitions exist
                var copyStateA = copyStates.First(s => s.state.name == "StateA").state;
                Assert.AreEqual(1, copyStateA.transitions.Length);
                Assert.AreEqual(0.5f, copyStateA.transitions[0].duration, 0.001f);
            }
            finally
            {
                if (original != null) UnityEngine.Object.DestroyImmediate(original);
                if (copy != null) UnityEngine.Object.DestroyImmediate(copy);
            }
        }

        [Test]
        public void Duplicate_ControllerWithBlendTree_CopiesBlendTree()
        {
            AnimatorController original = null;
            AnimatorController copy = null;
            try
            {
                original = new AnimatorController();
                original.name = "WithBlendTree";
                original.AddParameter("Blend", AnimatorControllerParameterType.Float);
                original.AddLayer("Base Layer");

                BlendTree blendTree;
                var state = original.CreateBlendTreeInController("BlendTree", out blendTree, 0);
                blendTree.blendType = BlendTreeType.Simple1D;
                blendTree.blendParameter = "Blend";

                var duplicator = new AnimatorControllerDuplicator();
                copy = duplicator.Duplicate(original);
                Assert.IsNotNull(copy);

                // Find the blend tree in copy
                var copyStates = copy.layers[0].stateMachine.states;
                bool hasBlendTree = false;
                foreach (var cs in copyStates)
                {
                    if (cs.state.motion is BlendTree bt)
                    {
                        hasBlendTree = true;
                        Assert.AreEqual(BlendTreeType.Simple1D, bt.blendType);
                        // BlendTree may or may not be same instance depending on implementation
                    }
                }
                Assert.IsTrue(hasBlendTree, "Copy should contain a BlendTree");
            }
            finally
            {
                if (original != null) UnityEngine.Object.DestroyImmediate(original);
                if (copy != null) UnityEngine.Object.DestroyImmediate(copy);
            }
        }

        [Test]
        public void Duplicate_ControllerWithMultipleLayers_CopiesAll()
        {
            AnimatorController original = null;
            AnimatorController copy = null;
            try
            {
                original = new AnimatorController();
                original.name = "MultiLayer";
                original.AddLayer("Layer0");
                original.AddLayer("Layer1");
                original.AddLayer("Layer2");

                var duplicator = new AnimatorControllerDuplicator();
                copy = duplicator.Duplicate(original);
                Assert.AreEqual(3, copy.layers.Length);
            }
            finally
            {
                if (original != null) UnityEngine.Object.DestroyImmediate(original);
                if (copy != null) UnityEngine.Object.DestroyImmediate(copy);
            }
        }

        [Test]
        public void Duplicate_ControllerWithParameters_CopiesParameters()
        {
            AnimatorController original = null;
            AnimatorController copy = null;
            try
            {
                original = new AnimatorController();
                original.name = "WithParams";
                original.AddParameter("FloatParam", AnimatorControllerParameterType.Float);
                original.AddParameter("BoolParam", AnimatorControllerParameterType.Bool);
                original.AddParameter("IntParam", AnimatorControllerParameterType.Int);
                original.AddLayer("Base Layer");

                var duplicator = new AnimatorControllerDuplicator();
                copy = duplicator.Duplicate(original);
                Assert.AreEqual(3, copy.parameters.Length);
                Assert.AreEqual("FloatParam", copy.parameters[0].name);
                Assert.AreEqual(AnimatorControllerParameterType.Float, copy.parameters[0].type);
                Assert.AreEqual("BoolParam", copy.parameters[1].name);
                Assert.AreEqual(AnimatorControllerParameterType.Bool, copy.parameters[1].type);
            }
            finally
            {
                if (original != null) UnityEngine.Object.DestroyImmediate(original);
                if (copy != null) UnityEngine.Object.DestroyImmediate(copy);
            }
        }

        [Test]
        public void Duplicate_ControllerWithSubStateMachine_CopiesSubSM()
        {
            AnimatorController original = null;
            AnimatorController copy = null;
            try
            {
                original = new AnimatorController();
                original.name = "WithSubSM";
                original.AddLayer("Base Layer");
                var layer = original.layers[0];
                var subSM = layer.stateMachine.AddStateMachine("SubStateMachine");
                subSM.AddState("SubState");

                var duplicator = new AnimatorControllerDuplicator();
                copy = duplicator.Duplicate(original);

                var copyLayer = copy.layers[0];
                Assert.AreEqual(1, copyLayer.stateMachine.stateMachines.Length);
                var copySub = copyLayer.stateMachine.stateMachines[0].stateMachine;
                Assert.AreEqual("SubStateMachine", copySub.name);
                Assert.AreEqual(1, copySub.states.Length);
                Assert.AreEqual("SubState", copySub.states[0].state.name);
            }
            finally
            {
                if (original != null) UnityEngine.Object.DestroyImmediate(original);
                if (copy != null) UnityEngine.Object.DestroyImmediate(copy);
            }
        }
    }

    // --- MaterialWrapperBuilder Additional Tests ---

    [TestFixture]
    public class MaterialWrapperBuilderAdditionalTests
    {
        private MaterialWrapperBuilder builder;

        [SetUp]
        public void SetUp()
        {
            builder = new MaterialWrapperBuilder();
        }

        [Test]
        public void Build_StandardShader_ReturnsMaterialBase()
        {
            Material mat = null;
            try
            {
                mat = new Material(Shader.Find("Standard"));
                var wrapper = builder.Build(mat);
                Assert.IsNotNull(wrapper);
                Assert.IsInstanceOf<StandardMaterial>(wrapper);
            }
            finally
            {
                if (mat != null) UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_UnlitColorShader_ReturnsMaterialBase()
        {
            Material mat = null;
            try
            {
                mat = new Material(Shader.Find("Unlit/Color"));
                var wrapper = builder.Build(mat);
                Assert.IsNotNull(wrapper);
            }
            finally
            {
                if (mat != null) UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_ToonLitShader_ReturnsMaterialBase()
        {
            Material mat = null;
            try
            {
                var shader = Shader.Find("VRChat/Mobile/Toon Lit");
                if (shader == null)
                {
                    Assert.Ignore("VRChat/Mobile/Toon Lit shader not available");
                    return;
                }
                mat = new Material(shader);
                var wrapper = builder.Build(mat);
                Assert.IsNotNull(wrapper);
            }
            finally
            {
                if (mat != null) UnityEngine.Object.DestroyImmediate(mat);
            }
        }

        [Test]
        public void Build_MatCapLitShader_ReturnsMaterialBase()
        {
            Material mat = null;
            try
            {
                var shader = Shader.Find("VRChat/Mobile/MatCap Lit");
                if (shader == null)
                {
                    Assert.Ignore("VRChat/Mobile/MatCap Lit shader not available");
                    return;
                }
                mat = new Material(shader);
                var wrapper = builder.Build(mat);
                Assert.IsNotNull(wrapper);
            }
            finally
            {
                if (mat != null) UnityEngine.Object.DestroyImmediate(mat);
            }
        }
    }

    // --- UnityAnimationUtility Additional Tests ---

    [TestFixture]
    public class UnityAnimationUtilityAdditionalTests
    {
        [Test]
        public void GetMaterials_EmptyAnimatorController_ReturnsEmpty()
        {
            AnimatorController controller = null;
            try
            {
                controller = new AnimatorController();
                controller.AddLayer("Base Layer");
                var mats = UnityAnimationUtility.GetMaterials(controller);
                Assert.IsNotNull(mats);
                Assert.AreEqual(0, mats.Length);
            }
            finally
            {
                if (controller != null) UnityEngine.Object.DestroyImmediate(controller);
            }
        }

        [Test]
        public void GetBlendTrees_NoBlendTrees_ReturnsEmpty()
        {
            AnimatorController controller = null;
            try
            {
                controller = new AnimatorController();
                controller.AddLayer("Base Layer");
                var layer = controller.layers[0];
                layer.stateMachine.AddState("RegularState");
                var trees = UnityAnimationUtility.GetBlendTrees(controller);
                Assert.IsNotNull(trees);
                Assert.AreEqual(0, trees.Count());
            }
            finally
            {
                if (controller != null) UnityEngine.Object.DestroyImmediate(controller);
            }
        }

        [Test]
        public void GetBlendTrees_WithBlendTree_ReturnsBlendTree()
        {
            AnimatorController controller = null;
            try
            {
                controller = new AnimatorController();
                controller.AddParameter("Blend", AnimatorControllerParameterType.Float);
                controller.AddLayer("Base Layer");

                BlendTree blendTree;
                controller.CreateBlendTreeInController("MyBlendTree", out blendTree, 0);

                var trees = UnityAnimationUtility.GetBlendTrees(controller);
                Assert.IsNotNull(trees);
                Assert.GreaterOrEqual(trees.Count(), 1);
            }
            finally
            {
                if (controller != null) UnityEngine.Object.DestroyImmediate(controller);
            }
        }

        [Test]
        public void DeepCopyBlendTree_SimpleTree_CopiesCorrectly()
        {
            AnimatorController controller = null;
            try
            {
                controller = new AnimatorController();
                controller.AddParameter("Blend", AnimatorControllerParameterType.Float);
                controller.AddLayer("Base Layer");

                BlendTree blendTree;
                controller.CreateBlendTreeInController("TestTree", out blendTree, 0);
                blendTree.blendType = BlendTreeType.Simple1D;
                blendTree.blendParameter = "Blend";

                var copy = UnityAnimationUtility.DeepCopyBlendTree(blendTree);
                Assert.IsNotNull(copy);
                Assert.AreNotSame(blendTree, copy);
                Assert.AreEqual(BlendTreeType.Simple1D, copy.blendType);
                Assert.AreEqual("Blend", copy.blendParameter);
            }
            finally
            {
                if (controller != null) UnityEngine.Object.DestroyImmediate(controller);
            }
        }

        [Test]
        public void DoesMotionExistInBlendTreeDescendants_EmptyTree_ReturnsFalse()
        {
            AnimatorController controller = null;
            AnimationClip clip = null;
            try
            {
                controller = new AnimatorController();
                controller.AddParameter("Blend", AnimatorControllerParameterType.Float);
                controller.AddLayer("Base Layer");

                BlendTree blendTree;
                controller.CreateBlendTreeInController("EmptyTree", out blendTree, 0);

                clip = new AnimationClip();
                var exists = UnityAnimationUtility.DoesMotionExistInBlendTreeDescendants(blendTree, clip);
                Assert.IsFalse(exists);
            }
            finally
            {
                if (controller != null) UnityEngine.Object.DestroyImmediate(controller);
                if (clip != null) UnityEngine.Object.DestroyImmediate(clip);
            }
        }

        [Test]
        public void GetMaterials_AnimationClipWithNoMaterialBindings_ReturnsEmpty()
        {
            AnimationClip clip = null;
            try
            {
                clip = new AnimationClip();
                clip.SetCurve("", typeof(Transform), "m_LocalPosition.x", AnimationCurve.Linear(0, 0, 1, 1));
                var mats = UnityAnimationUtility.GetMaterials(clip);
                Assert.IsNotNull(mats);
                Assert.AreEqual(0, mats.Length);
            }
            finally
            {
                if (clip != null) UnityEngine.Object.DestroyImmediate(clip);
            }
        }
    }
}
