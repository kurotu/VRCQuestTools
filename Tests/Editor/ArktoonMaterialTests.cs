using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using ImageMagick;
using UnityEditor;
using UnityEngine;

namespace KRTQuestTools
{
    public class ArktoonMaterialTests
    {
        [Test]
        public void EmissionColor()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("arktoon.mat");
            using (var image = wrapper.CompositeLayers())
            using (var original = TestUtils.LoadMagickImage("albedo_1024px.png"))
            using (var emission = new MagickImage(new MagickColorFactory().Create("#1F1F1F"), original.Width, original.Height))
            {
                original.Composite(emission, CompositeOperator.Screen);
                var result = image.CompareTo(original);
                Assert.AreEqual(result, 0);
            }
        }

        [Test]
        public void EmissiveFreak()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("arktoon_EmissiveFreak.mat");
            using (var image = wrapper.CompositeLayers())
            using (var main = TestUtils.LoadMagickImage("albedo_1024px.png"))
            using (var emission = TestUtils.LoadMagickImage("emission_1024px.png"))
            using (var ef1 = TestUtils.LoadMagickImage("emissive_freak_1_1024px.png"))
            using (var ef2 = TestUtils.LoadMagickImage("emissive_freak_2_1024px.png"))
            {
                main.Composite(emission, CompositeOperator.Screen);
                main.Composite(ef1, CompositeOperator.Screen);
                main.Composite(ef2, CompositeOperator.Screen);
                var result = image.CompareTo(main);
                Assert.AreEqual(result, 0);
            }
        }
    }
}
