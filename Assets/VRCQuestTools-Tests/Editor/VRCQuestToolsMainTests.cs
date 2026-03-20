// Tests for VRCQuestTools main class
using NUnit.Framework;

namespace KRT.VRCQuestTools.Tests
{
    [TestFixture]
    internal class VRCQuestToolsMainTests
    {
        [Test]
        public void Version_IsNotNullOrEmpty()
        {
            Assert.IsFalse(string.IsNullOrEmpty(VRCQuestTools.Version));
        }

        [Test]
        public void Version_IsValidSemVer()
        {
            var version = VRCQuestTools.Version;
            Assert.IsTrue(version.Contains("."), $"Version '{version}' should contain dots");
            var parts = version.Split('.');
            Assert.IsTrue(parts.Length >= 2, $"Version '{version}' should have at least 2 parts");
        }
    }
}
