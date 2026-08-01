// Tests for forceMaterialPreview behavior
using NUnit.Framework;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Models;
using nadena.dev.ndmf.preview;

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
        public void GetCacheKey_DoesNotInclude_ForceMaterialPreviewFlag()
        {
            var comp = new DummyMaterialConversionComponent();
            comp.EnableMaterialPreview = false;
            comp.ForceMaterialPreview = true;

            var key = (comp as IMaterialConversionComponent).GetCacheKey();

            // ForceMaterialPreview is a temporary flag and must not be included in the cache key
            Assert.IsFalse(key.Contains($"_{comp.ForceMaterialPreview}"), $"Cache key included force preview flag: {key}");
        }

        /// <summary>
        /// forceMaterialPreview is a non-serialized field which the inspector button writes directly, so Unity's
        /// object change stream reports nothing and an observation of it is never invalidated on its own. The
        /// inspectors therefore call NdmfUtility.NotifyObjectUpdate; this locks in that the call is what makes
        /// the preview filters observing the flag rebuild.
        /// </summary>
        [Test]
        public void NotifyObjectUpdate_InvalidatesForceMaterialPreviewObservation()
        {
            var gameObject = new UnityEngine.GameObject("ForceMaterialPreviewProbe");
            try
            {
                var settings = gameObject.AddComponent<AvatarConverterSettings>();
                settings.forceMaterialPreview = false;

                var context = new ComputeContext("test force material preview");
                context.Observe((UnityEngine.Object)settings, o => ((AvatarConverterSettings)o).ForceMaterialPreview);

                // Flip the flag exactly like the inspector button does.
                settings.forceMaterialPreview = true;
                Utils.NdmfUtility.NotifyObjectUpdate(settings);
                ComputeContext.FlushInvalidates();

                Assert.IsTrue(context.IsInvalidated, "NotifyObjectUpdate must invalidate an observation of ForceMaterialPreview.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }
    }
}
