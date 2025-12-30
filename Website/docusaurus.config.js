// @ts-check
// Note: type annotations allow type checking and IDEs autocompletion

const {themes} = require('prism-react-renderer');
const lightCodeTheme = themes.github;
const darkCodeTheme = themes.dracula;

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'VRCQuestTools',
  tagline: 'Avatar Converter and Utilities for Android',
  favicon: 'img/favicon.ico',

  // Set the production url of your site here
  url: 'https://kurotu.github.io',
  // Set the /<baseUrl>/ pathname under which your site is served
  // For GitHub pages deployment, it is often '/<projectName>/'
  baseUrl: '/VRCQuestTools/',

  // GitHub pages deployment config.
  // If you aren't using GitHub pages, you don't need these.
  organizationName: 'kurotu', // Usually your GitHub org/user name.
  projectName: 'VRCQuestTools', // Usually your repo name.

  // onBrokenLinks: 'warn', // Temporary ignore broken links for missing i18n pages.
  onBrokenMarkdownLinks: 'warn',

  // Even if you don't use internalization, you can use this field to set useful
  // metadata like html lang. For example, if your site is Chinese, you may want
  // to replace "en" with "zh-Hans".
  i18n: {
    defaultLocale: 'en',
    locales: ['en', 'ja'],
  },

  presets: [
    [
      'classic',
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        docs: {
          sidebarPath: require.resolve('./sidebars.js'),
          // Please change this to your repo.
          // Remove this to remove the "edit this page" links.
          editUrl:
            'https://github.com/kurotu/VRCQuestTools/edit/master/Website/',
          editLocalizedFiles: true,
          editCurrentVersion: true,
        },
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
      }),
    ],
  ],

  themeConfig:
    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    ({
      metadata: [
        {name: 'google-site-verification', content: 'iHat4XFlFF2SfNgjFM-hqhqAdP7KxL_s0WEFRySldpo'},
        {name: 'algolia-site-verification', content: '2DC43CBAE38141BC'},
      ],
      // Replace with your project's social card
      image: 'img/social-card.png',
      navbar: {
        title: 'VRCQuestTools',
        logo: {
          alt: 'Docusaurus Logo',
          src: 'img/logo.png',
        },
        items: [
          {
            type: 'docSidebar',
            sidebarId: 'tutorialSidebar',
            position: 'left',
            label: 'Docs',
          },
          {
            type: 'docsVersionDropdown',
            position: 'right',
          },
          {
            type: 'localeDropdown',
            position: 'right',
          },
          {
            href: 'https://github.com/kurotu/VRCQuestTools',
            label: 'GitHub',
            position: 'right',
          },
          {
            href: 'https://kurotu.booth.pm/items/2436054',
            label: 'Booth',
            position: 'right',
          },
        ],
      },
      footer: {
        style: 'dark',
        links: [
          {
            label: 'Documentation',
              to: '/docs/intro',
          },
          {
              label: 'GitHub',
              href: 'https://github.com/kurotu/VRCQuestTools',
          },
          {
            label: 'Booth',
            href: 'https://kurotu.booth.pm/items/2436054',
          },
        ],
        copyright: `Copyright © 2023 kurotu. Built with Docusaurus.`,
      },
      prism: {
        theme: lightCodeTheme,
        darkTheme: darkCodeTheme,
        additionalLanguages: ['bash', 'diff', 'json'],
      },
      algolia: {
        // The application ID provided by Algolia
        appId: '0I3QRXID8E',

        // Public API key: it is safe to commit it
        apiKey: '47f8fb49101477f633796a8ed3d3f506',

        indexName: 'Documentation',

        // Optional: see doc section below
        contextualSearch: true,

        // Optional: Specify domains where the navigation should occur through window.location instead on history.push. Useful when our Algolia config crawls multiple documentation sites and we want to navigate with window.location.href to them.
        externalUrlRegex: 'external\\.com|domain\\.com',

        // Optional: Replace parts of the item URLs from Algolia. Useful when using the same search index for multiple deployments using a different baseUrl. You can use regexp or string in the `from` param. For example: localhost:3000 vs myCompany.com/docs
        // replaceSearchResultPathname: {
        //   from: '/docs/', // or as RegExp: /\/docs\//
        //   to: '/',
        // },

        // Optional: Algolia search parameters
        searchParameters: {},

        // Optional: path for search page that enabled by default (`false` to disable it)
        searchPagePath: 'search',

        // Optional: whether the insights feature is enabled or not on Docsearch (`false` by default)
        insights: false,

        // Optional: whether you want to use the new Ask AI feature (undefined by default)
        // askAi: 'YOUR_ALGOLIA_ASK_AI_ASSISTANT_ID',

        //... other Algolia params
      },
    }),
};

module.exports = config;
