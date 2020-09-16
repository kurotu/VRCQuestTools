using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;

namespace KRTQuestTools
{
    public class MaterialUtilsTest
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
            Assert.AreEqual(MaterialUtils.GetShaderCategory(material), ShaderCategory.Generic);
            using (var emission = MaterialUtils.GetEmissionImage(material))
            {
                Assert.Null(emission);
            }
            for (var i = 0; i < 2; i++)
            {
                using (var emission = MaterialUtils.GetEmissiveFreakImage(material, i))
                {
                    Assert.Null(emission);
                }
            }
        }

        [Test]
        public void StandardEmission()
        {
            var material = LoadMaterial("Standard_Emission.mat");
            Assert.AreEqual(MaterialUtils.GetShaderCategory(material), ShaderCategory.Generic);
            using (var emission = MaterialUtils.GetEmissionImage(material))
            {
                Assert.NotNull(emission);
            }
            for (var i = 0; i < 2; i++)
            {
                using (var emission = MaterialUtils.GetEmissiveFreakImage(material, i))
                {
                    Assert.Null(emission);
                }
            }
        }

        [Test]
        public void Arktoon()
        {
            var material = LoadMaterial("arktoon.mat");
            Assert.AreEqual(MaterialUtils.GetShaderCategory(material), ShaderCategory.Arktoon);
            using (var emission = MaterialUtils.GetEmissionImage(material))
            {
                Assert.NotNull(emission);
            }
            for (var i = 0; i < 2; i++)
            {
                using (var emission = MaterialUtils.GetEmissiveFreakImage(material, i))
                {
                    Assert.Null(emission);
                }
            }
        }

        [Test]
        public void ArktoonEmissiveFreak()
        {
            var material = LoadMaterial("arktoon_EmissiveFreak.mat");
            Assert.AreEqual(MaterialUtils.GetShaderCategory(material), ShaderCategory.ArktoonEmissiveFreak);
            using (var emission = MaterialUtils.GetEmissionImage(material))
            {
                Assert.NotNull(emission);
            }
            for (var i = 0; i < 2; i++)
            {
                using (var emission = MaterialUtils.GetEmissiveFreakImage(material, i))
                {
                    Assert.NotNull(emission);
                }
            }
        }
    }
}
