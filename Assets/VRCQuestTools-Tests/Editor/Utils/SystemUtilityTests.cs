using NUnit.Framework;

namespace KRT.VRCQuestTools.Utils
{
    /// <summary>
    /// Test SystemUtility.
    /// </summary>
    public class SystemUtilityTests
    {
        /// <summary>
        /// Test GetTypeByName.
        /// </summary>
        [Test]
        public void GetTypeByName()
        {
            Assert.AreEqual(typeof(VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone), SystemUtility.GetTypeByName("VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone"));
        }

        /// <summary>
        /// Test GetTypeByName for a missing type.
        /// </summary>
        [Test]
        public void GetTypeByNameNull()
        {
            Assert.IsNull(SystemUtility.GetTypeByName("VRCQuestTools.GetType.Test.Dummy"));
        }

        /// <summary>
        /// Test GetAppLocalCachePath.
        /// </summary>
        [Test]
        public void GetAppLocalCachePath()
        {
            var userName = System.Environment.UserName;
#if UNITY_EDITOR_WIN
            Assert.AreEqual($"C:\\Users\\{userName}\\AppData\\Local\\VRCQuestTools\\Cache", SystemUtility.GetAppLocalCachePath("VRCQuestTools"));
#elif UNITY_EDITOR_OSX
            Assert.AreEqual($"/Users/{userName}/Library/Caches/VRCQuestTools", SystemUtility.GetAppLocalCachePath("VRCQuestTools"));
#elif UNITY_EDITOR_LINUX
            var home = System.Environment.GetEnvironmentVariable("HOME");
            Assert.AreEqual($"{home}/.cache/VRCQuestTools", SystemUtility.GetAppLocalCachePath("VRCQuestTools"));
#else
            Assert.Fail("Unsupported editor platform");
#endif
        }

        /// <summary>
        /// Test GetTypeByName finds an editor type.
        /// </summary>
        [Test]
        public void GetTypeByName_FindsEditorType()
        {
            var type = SystemUtility.GetTypeByName("UnityEditor.Editor");
            Assert.IsNotNull(type);
            Assert.AreEqual(typeof(UnityEditor.Editor), type);
        }

        /// <summary>
        /// Test GetAppLocalCachePath includes app name.
        /// </summary>
        [Test]
        public void GetAppLocalCachePath_IncludesAppName()
        {
            var path = SystemUtility.GetAppLocalCachePath("MyTestApp");
            Assert.IsNotEmpty(path);
            Assert.IsTrue(path.Contains("MyTestApp"));
        }

        /// <summary>
        /// Test OpenFolder throws for nonexistent directory.
        /// </summary>
        [Test]
        public void OpenFolder_ThrowsForNonexistentDirectory()
        {
            Assert.Throws<System.IO.DirectoryNotFoundException>(() =>
            {
                SystemUtility.OpenFolder("/nonexistent/path/12345");
            });
        }
    }
}
