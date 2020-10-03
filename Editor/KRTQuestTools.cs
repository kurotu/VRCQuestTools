using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KRTQuestTools
{
    public static class KRTQuestTools
    {
        public const string Version = "0.0.0";
        internal const string RootMenu = "KRTQuestTools/";
    }

    internal enum MenuPriority : int
    {
        AvatarQuestConverter = 0,
        BlendShapesCopy,
        VertexColorRemover,
        UnitySettings = 100
    }
}
