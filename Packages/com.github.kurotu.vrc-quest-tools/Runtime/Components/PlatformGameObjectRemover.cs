﻿using UnityEngine;

namespace KRT.VRCQuestTools.Components
{
    /// <summary>
    /// PlatformComponentRemover removes the attached GameObject depending on the target platform.
    [AddComponentMenu("VRCQuestTools/VQT Platform GameObject Remover")]
    [HelpURL("https://kurotu.github.io/VRCQuestTools/ja/docs/references/components/platform-gameobject-remover?lang=auto")]
    [DisallowMultipleComponent]
    public class PlatformGameObjectRemover : VRCQuestToolsPlatformComponent, IVRCQuestToolsNdmfComponent
    {
        /// <summary>
        /// Remove the GameObject this component is attached when the target platform is PC.
        /// </summary>
        public bool removeOnPC = false;

        /// <summary>
        /// Remove the GameObject this component is attached when the target platform is Android.
        /// </summary>
        public bool removeOnAndroid = false;
    }
}
