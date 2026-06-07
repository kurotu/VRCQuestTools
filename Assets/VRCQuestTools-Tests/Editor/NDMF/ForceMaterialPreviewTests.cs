// Tests for forceMaterialPreview behavior
using NUnit.Framework;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;

namespace KRT.VRCQuestTools.Ndmf
{
    public class ForceMaterialPreviewTests
    {
        private class DummyConvertSettings : IMaterialConvertSettings
        {
            public MobileTextureFormat MobileTextureFormat => MobileTextureFormat.ASTC_8x8;
            public void LoadDefaultAssets() { }
            public string GetCacheKey() => "DummyConvertSettingsKey";
        }

        private class DummyMaterialConversionComponent : IMaterialConversionComponent
        {
            public IMaterialConvertSettings DefaultMaterialConvertSettings { get; } = new DummyConvertSettings();
            public AdditionalMaterialConvertSettings[] AdditionalMaterialConvertSettings { get; set; } = new AdditionalMaterialConvertSettings[0];
            public bool RemoveExtraMaterialSlots => false;
            public AvatarConverterNdmfPhase NdmfPhase => AvatarConverterNdmfPhase.Auto;
            public bool IsPrimaryRoot => false;
            public bool EnableMaterialPreview { get; set; } = false;
            public bool ForceMaterialPreview { get; set; } = false;
        }

        [Test]
        public void GetCacheKey_Includes_ForceMaterialPreviewFlag()
        {
            var comp = new DummyMaterialConversionComponent();
            comp.EnableMaterialPreview = false;
            comp.ForceMaterialPreview = true;

            var key = (comp as IMaterialConversionComponent).GetCacheKey();

            Assert.IsTrue(key.EndsWith($"_{comp.EnableMaterialPreview}_{comp.ForceMaterialPreview}"), $"Cache key did not include force preview flag: {key}");
        }
    }
}
