// Tests for MSMapGenViewModel
using KRT.VRCQuestTools.ViewModels;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    internal class MSMapGenViewModelTests
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
        public void DisableGenerateButton_MetallicMapSet_ReturnsFalse()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                var vm = new MSMapGenViewModel();
                vm.metallicMap = tex;
                vm.smoothnessMap = null;
                Assert.IsFalse(vm.DisableGenerateButton);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void DisableGenerateButton_SmoothnessMapSet_ReturnsFalse()
        {
            var tex = new Texture2D(4, 4);
            try
            {
                var vm = new MSMapGenViewModel();
                vm.metallicMap = null;
                vm.smoothnessMap = tex;
                Assert.IsFalse(vm.DisableGenerateButton);
            }
            finally
            {
                Object.DestroyImmediate(tex);
            }
        }

        [Test]
        public void DisableGenerateButton_BothSet_ReturnsFalse()
        {
            var tex1 = new Texture2D(4, 4);
            var tex2 = new Texture2D(4, 4);
            try
            {
                var vm = new MSMapGenViewModel();
                vm.metallicMap = tex1;
                vm.smoothnessMap = tex2;
                Assert.IsFalse(vm.DisableGenerateButton);
            }
            finally
            {
                Object.DestroyImmediate(tex1);
                Object.DestroyImmediate(tex2);
            }
        }

        [Test]
        public void InvertSmoothness_DefaultsToFalse()
        {
            var vm = new MSMapGenViewModel();
            Assert.IsFalse(vm.invertSmoothness);
        }
    }
}
