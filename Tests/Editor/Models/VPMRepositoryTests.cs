using System.IO;
using System.Linq;
using NUnit.Framework;
#if VQT_HAS_NEWTONSOFT_JSON
using Newtonsoft.Json;
#endif

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Test <see cref="VPMRepository"/>.
    /// </summary>
    public class VPMRepositoryTests
    {
        private string packageName = "com.github.kurotu.vrc-quest-tools";

        /// <summary>
        /// Test parse json.
        /// </summary>
        [Test]
        public void ParseJson()
        {
#if !VQT_HAS_NEWTONSOFT_JSON
            Assert.Ignore("NewtonSoft Json is missing");
#else
            var json = File.ReadAllText(Path.Combine(TestUtils.FixturesFolder, "vpm.json"));

            var repo = JsonConvert.DeserializeObject<VPMRepository>(json);
            Assert.IsTrue(repo.packages.ContainsKey(packageName));

            var vqt = repo.packages[packageName];
            Assert.IsTrue(vqt.versions.Keys.Count > 0);

            var versions = vqt.versions.Keys.Select(v => new SemVer(v)).OrderBy(v => v).ToList();
            Assert.AreEqual("1.9.0", versions.First().ToString());
            Assert.AreEqual("1.11.0", versions.Last().ToString());

            var replacer = repo.packages["com.github.kurotu.material-replacer"];
            versions = replacer.versions.Keys.Select(v => new SemVer(v)).OrderBy(v => v).ToList();
            Assert.AreEqual("1.1.1", versions.First().ToString());
            Assert.AreEqual("1.1.1", versions.Last().ToString());
#endif
        }
    }
}
