using System.Collections.Generic;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Representation of VPM repository JSON.
    /// </summary>
    internal class VPMRepository
    {
        /// <summary>
        /// packages in the repository.
        /// </summary>
        public Dictionary<string, VPMPackage> packages;

        /// <summary>
        /// VPM package info.
        /// </summary>
        internal class VPMPackage
        {
            /// <summary>
            /// versions in the package.
            /// </summary>
            public Dictionary<string, VPMPackageJson> versions;
        }

        /// <summary>
        /// Representation of VPM package JSON.
        /// </summary>
        internal class VPMPackageJson
        {
        }
    }
}
