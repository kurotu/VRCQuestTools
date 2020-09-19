using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using ImageMagick;
using UnityEditor;
using UnityEngine;

namespace KRTQuestTools
{
    public class GenericMaterialTests
    {
        [Test]
        public void StandardNoEmission()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("Standard_NoEmission.mat");
            using (var image = wrapper.CompositeLayers())
            using (var original = TestUtils.LoadMagickImage("albedo_1024px.png"))
            {
                var result = image.CompareTo(original);
                Assert.AreEqual(result, 0);
            }
        }

        [Test]
        public void StandardEmission()
        {
            var wrapper = TestUtils.LoadMaterialWrapper("Standard_Emission.mat");
            using (var image = wrapper.CompositeLayers())
            using (var main = TestUtils.LoadMagickImage("albedo_1024px.png"))
            using (var emission = TestUtils.LoadMagickImage("emission_1024px.png"))
            {
                main.Composite(emission, CompositeOperator.Screen);
                var result = image.CompareTo(main);
                Assert.AreEqual(result, 0);
            }
        }
    }
}
