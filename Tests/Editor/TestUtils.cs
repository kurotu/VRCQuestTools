using System.Collections;
using System.Collections.Generic;
using ImageMagick;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace KRTQuestTools
{
    static class TestUtils
    {
        internal const string FixturesFolder = "Assets/KRT/KRTQuestTools/Tests/Fixtures";
        internal const string MaterialsFolder = FixturesFolder + "/Materials";
        internal const string TexturesFolder = FixturesFolder + "/Textures";

        internal static Material LoadMaterial(string file)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "/" + file);
            Assert.NotNull(material);
            return material;
        }

        internal static MaterialWrapper LoadMaterialWrapper(string file)
        {
            var material = LoadMaterial(file);
            var wrapper = MaterialUtils.CreateWrapper(material);
            Assert.NotNull(wrapper);
            return wrapper;
        }

        internal static MagickImage LoadMagickImage(string file)
        {
            var image = new MagickImage(TexturesFolder + "/" + file);
            Assert.NotNull(image);
            return image;
        }
    }
}
