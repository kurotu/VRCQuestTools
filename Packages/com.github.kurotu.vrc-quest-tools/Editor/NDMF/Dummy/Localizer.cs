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
        private readonly string defaultLocale;
        private readonly Func<List<(string Locale, Func<string, string> Resolve)>> p;

        /// <summary>
        /// Initializes a new instance of the <see cref="Localizer"/> class.
        /// </summary>
        /// <param name="defaultLocale">Default Locale.</param>
        /// <param name="p">Localization resolvers.</param>
        public Localizer(string defaultLocale, Func<List<(string Locale, Func<string, string> Resolve)>> p)
        {
            this.defaultLocale = defaultLocale;
            this.p = p;
        }

        /// <summary>
        /// Get localized string.
        /// </summary>
        /// <param name="key">key.</param>
        /// <returns>Resolved string.</returns>
        public string Get(string key)
        {
            var locale = System.Globalization.CultureInfo.CurrentCulture.Name;
            var resolver = p().Find(p => p.Locale == locale);
            if (resolver == default)
            {
                resolver = p().Find(p => p.Locale == defaultLocale);
            }
            return resolver.Resolve(key);
        }
    }
}
#endif
