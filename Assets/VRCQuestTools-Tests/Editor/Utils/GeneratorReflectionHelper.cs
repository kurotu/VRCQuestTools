using System;
using System.Reflection;
using KRT.VRCQuestTools.Models;
using KRT.VRCQuestTools.Models.Unity;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests
{
    internal static class GeneratorReflectionHelper
    {
        private static readonly BindingFlags NonPublicInstance =
            BindingFlags.Instance | BindingFlags.NonPublic;

        internal static object InvokeProtected(
            LilToonToonStandardGenerator generator, string methodName, params object[] args)
        {
            var method = typeof(LilToonToonStandardGenerator).GetMethod(methodName, NonPublicInstance)
                ?? typeof(ToonStandardGenerator).GetMethod(methodName, NonPublicInstance);
            if (method == null)
            {
                throw new MissingMethodException(
                    $"Method '{methodName}' not found on LilToonToonStandardGenerator or ToonStandardGenerator");
            }
            return method.Invoke(generator, args);
        }

        internal static LilToonToonStandardGenerator CreateGenerator(
            LilToonMaterial lilMat,
            ToonStandardConvertSettings settings = null,
            Texture2D blackTex = null)
        {
            if (settings == null)
            {
                settings = new ToonStandardConvertSettings();
                settings.SetAllFeatures(true);
            }
            if (blackTex == null)
            {
                blackTex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            }
            return new LilToonToonStandardGenerator(lilMat, settings, blackTex);
        }
    }
}
