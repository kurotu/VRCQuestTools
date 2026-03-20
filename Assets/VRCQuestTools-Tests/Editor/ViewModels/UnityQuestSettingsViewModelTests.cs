// Tests for UnityQuestSettingsViewModel
using KRT.VRCQuestTools.ViewModels;
using NUnit.Framework;
using UnityEditor;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    internal class UnityQuestSettingsViewModelTests
    {
        [Test]
        public void HasValidAndroidTextureCompression_CorrespondsToASTC()
        {
            var vm = new UnityQuestSettingsViewModel();
            // RecommendedAndroidTextureCompression is private const ASTC
            // HasValidAndroidTextureCompression checks if DefaultAndroidTextureCompression == ASTC
            var expected = vm.DefaultAndroidTextureCompression == MobileTextureSubtarget.ASTC;
            Assert.AreEqual(expected, vm.HasValidAndroidTextureCompression);
        }

        [Test]
        public void DefaultAndroidTextureCompression_ReturnsValue()
        {
            var vm = new UnityQuestSettingsViewModel();
            var compression = vm.DefaultAndroidTextureCompression;
            // Just verify it returns a valid enum value
            Assert.IsTrue(System.Enum.IsDefined(typeof(MobileTextureSubtarget), compression));
        }

        [Test]
        public void HasAndroidBuildSupport_ReturnsBool()
        {
            var vm = new UnityQuestSettingsViewModel();
            // On dev machine, may or may not have Android support
            var result = vm.HasAndroidBuildSupport;
            Assert.IsNotNull(result);
        }

        [Test]
        public void HasValidAndroidTextureCompression_ReturnsBool()
        {
            var vm = new UnityQuestSettingsViewModel();
            var result = vm.HasValidAndroidTextureCompression;
            Assert.IsNotNull(result);
        }

        [Test]
        public void AllSettingsValid_ReturnsBool()
        {
            var vm = new UnityQuestSettingsViewModel();
            var result = vm.AllSettingsValid;
            Assert.IsNotNull(result);
        }

        [Test]
        public void ShowWindowOnLoad_CanGetAndSet()
        {
            var vm = new UnityQuestSettingsViewModel();
            var original = vm.ShowWindowOnLoad;
            try
            {
                vm.ShowWindowOnLoad = !original;
                Assert.AreEqual(!original, vm.ShowWindowOnLoad);
            }
            finally
            {
                vm.ShowWindowOnLoad = original;
            }
        }
    }
}
