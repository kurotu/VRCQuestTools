// Tests for VirtualLensUtility
using KRT.VRCQuestTools.Utils;
using NUnit.Framework;
using UnityEngine;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    internal class VirtualLensUtilityTests
    {
        [Test]
        public void VirtualLensSettingsType_MayBeNull()
        {
            // VirtualLens2 may or may not be installed
            var type = VirtualLensUtility.VirtualLensSettingsType;
            if (type == null)
            {
                Assert.IsNull(type);
            }
            else
            {
                Assert.AreEqual("VirtualLens2.VirtualLensSettings", type.FullName);
            }
        }

        [Test]
        public void RemoteOnlyMode_EnumValues_AreCorrect()
        {
            Assert.AreEqual(0, (int)VirtualLensUtility.RemoteOnlyMode.ForceDisable);
            Assert.AreEqual(1, (int)VirtualLensUtility.RemoteOnlyMode.ForceEnable);
            Assert.AreEqual(2, (int)VirtualLensUtility.RemoteOnlyMode.MobileOnly);
        }
    }
}
