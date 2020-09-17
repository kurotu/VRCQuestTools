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
        private const string FixturesFolder = "Assets/KRT/KRTQuestTools/Tests/Fixtures";
        private const string MaterialsFolder = FixturesFolder + "/Materials";
        private const string TexturesFolder = FixturesFolder + "/Textures";

        private Material LoadMaterial(string file)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "/" + file);
            Assert.NotNull(material);
            return material;
        }

        [Test]
        public void StandardNoEmission()
        {
            var material = LoadMaterial("Standard_NoEmission.mat");
            var generic = MaterialUtils.CreateWrapper(material);
            using(var image = generic.CompositeLayers())
            using (var original = new MagickImage(TexturesFolder + "/albedo_1024px.png"))
            {
                var result = image.CompareTo(original);
                Assert.AreEqual(result, 0);
            }
        }

        [Test]
        public void StandardEmission()
        {
            var material = LoadMaterial("Standard_Emission.mat");
            var generic = MaterialUtils.CreateWrapper(material);
            using (var image = generic.CompositeLayers())
            using (var main = new MagickImage(TexturesFolder + "/albedo_1024px.png"))
            using (var emission = new MagickImage(TexturesFolder + "/emission_1024px.png"))
            {
                main.Composite(emission, CompositeOperator.Screen);
                var result = image.CompareTo(main);
                Assert.AreEqual(result, 0);
            }
        }
    }
}
