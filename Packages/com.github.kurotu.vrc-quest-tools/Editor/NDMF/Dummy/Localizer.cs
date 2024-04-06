#if !VQT_HAS_NDMF_ERROR_REPORT
using System;
using System.Collections.Generic;

namespace KRT.VRCQuestTools.Ndmf.Dummy
{
    /// <summary>
    /// Localizer is a dummy class for NDMF.
    /// </summary>
    internal class Localizer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Localizer"/> class.
        /// </summary>
        /// <param name="defaultLocale">Default Locale.</param>
        /// <param name="p">Localization resolvers.</param>
        public Localizer(string defaultLocale, Func<List<(string Locale, Func<string, string> Resolve)>> p)
        {
        }
    }
}
#endif
