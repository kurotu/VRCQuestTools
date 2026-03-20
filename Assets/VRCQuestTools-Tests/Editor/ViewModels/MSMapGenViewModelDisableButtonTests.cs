// Tests for MSMapGenViewModel property and additional ViewModel coverage.

using KRT.VRCQuestTools.ViewModels;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests.ViewModels
{
    [TestFixture]
    internal class MSMapGenViewModelDisableButtonTests
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
        public void DisableGenerateButton_MetallicSet_ReturnsFalse()
        {
            var tex = new Texture2D(2, 2);
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
        public void DisableGenerateButton_SmoothnessSet_ReturnsFalse()
        {
            var tex = new Texture2D(2, 2);
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
            var tex1 = new Texture2D(2, 2);
            var tex2 = new Texture2D(2, 2);
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
        public void InvertSmoothness_DefaultIsFalse()
        {
            var vm = new MSMapGenViewModel();
            Assert.IsFalse(vm.invertSmoothness);
        }
    }
}
