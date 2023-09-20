using NUnit.Framework;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Test <see cref="SemVer"/>.
    /// </summary>
    public class SemVerTests
    {
        /// <summary>
        /// Test parse semver.
        /// </summary>
        [Test]
        public void ParseTest()
        {
            var v123 = new SemVer(1, 2, 3);
            Assert.False(v123.IsPrerelease);
            Assert.AreEqual("1.2.3", v123.ToString());

            var v456beta1 = new SemVer(4, 5, 6, "beta.1");
            Assert.True(v456beta1.IsPrerelease);
            Assert.AreEqual("4.5.6-beta.1", v456beta1.ToString());

            var v789alpha1 = new SemVer("7.8.9-alpha.1");
            Assert.True(v789alpha1.IsPrerelease);
            Assert.AreEqual("7.8.9-alpha.1", v789alpha1.ToString());
        }

        /// <summary>
        /// Test comparison of semver.
        /// </summary>
        [Test]
        public void ComapreTest()
        {
            Assert.True(new SemVer(1, 0, 0) > new SemVer(0, 0, 0));
            Assert.True(new SemVer(0, 1, 0) > new SemVer(0, 0, 0));
            Assert.True(new SemVer(0, 0, 1) > new SemVer(0, 0, 0));
            Assert.True(new SemVer(0, 0, 0) > new SemVer(0, 0, 0, "alpha.1"));
            Assert.True(new SemVer(0, 0, 0, "alpha.2") > new SemVer(0, 0, 0, "alpha.1"));

            Assert.True(new SemVer(0, 0, 0) < new SemVer(1, 0, 0));
            Assert.True(new SemVer(0, 0, 0) < new SemVer(0, 1, 0));
            Assert.True(new SemVer(0, 0, 0) < new SemVer(0, 0, 1));
            Assert.True(new SemVer(0, 0, 0, "alpha.1") < new SemVer(0, 0, 0));
            Assert.True(new SemVer(0, 0, 0, "alpha.1") < new SemVer(0, 0, 0, "alpha.2"));
        }
    }
}
