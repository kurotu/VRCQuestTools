import React from 'react';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import getUserLocale from 'get-user-locale';
import { Redirect, useLocation } from '@docusaurus/router';

export default function Root({children}) {
    const location = useLocation();
    if (location.search.includes('lang=auto')) {
        const {siteConfig, i18n} = useDocusaurusContext();
        const currentBaseUrlWithLocale = siteConfig.baseUrl + (i18n.currentLocale === i18n.defaultLocale ? '' : i18n.currentLocale + '/');

        let redirectBaseUrl = siteConfig.baseUrl;
        const locale = getUserLocale();
        if (locale === 'ja' || locale === 'ja-JP') {
            redirectBaseUrl = siteConfig.baseUrl + 'ja/';
        }

        if (currentBaseUrlWithLocale !== redirectBaseUrl) {
            const redirectPathname = useLocation().pathname.replace(currentBaseUrlWithLocale, redirectBaseUrl);
            window.location.href = window.location.origin + redirectPathname;
        } else {
            return <Redirect to={location.pathname} />;
        }
    }

    return <>{children}</>;
}
