using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KRTQuestTools
{
    public static class KRTQuestTools
    {
        public const string Version = "0.0.0";
    }

    internal static class MenuPaths
    {
        private const string RootMenu = "KRTQuestTools/";
        internal const string ConvertAvatarForQuest = RootMenu + "Convert Avatar For Quest";
        internal const string BlendShapesCopy = RootMenu + "BlendShapes Copy";
        internal const string AutoRemoveVertexColors = RootMenu + "Auto Remove Vertex Colors";
        internal const string UnitySettings = RootMenu + "Unity Settings";

        internal const string GameObjectRemoveAllVertexColors = "GameObject/Remove All Vertex Colors";
        internal const string GameObjectConvertAvatarForQuest = "GameObject/Convert Avatar For Quest";
        internal const string ContextBlendShapesCopy = "CONTEXT/SkinnedMeshRenderer/Copy BlendShape Weights";
    }

    internal enum MenuPriorities : int
    {
        ConvertAvatarForQuest = 0,
        BlendShapesCopy,
        AutoRemoveVertexColors = 100,
        UnitySettings = 200
    }
}
