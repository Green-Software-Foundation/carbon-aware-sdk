// @ts-check
// Note: type annotations allow type checking and IDEs autocompletion

const lightCodeTheme = require('prism-react-renderer/themes/github');
const darkCodeTheme = require('prism-react-renderer/themes/dracula');

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'Carbon Aware SDK',
  tagline: 'An SDK to enable the creation of carbon aware applications, that do more when the electricity comes from clean low-carbon sources and less when it does not.',
  url: 'https://github.com/Green-Software-Foundation/carbon-aware-sdk',
  baseUrl: '/',
  onBrokenLinks: 'throw',
  onBrokenMarkdownLinks: 'warn',
  favicon: 'img/favicon.ico',
  organizationName: 'Green-Software-Foundation', // Usually your GitHub org/user name.
  projectName: 'carbon-aware-sdk', // Usually your repo name.
  trailingSlash: false,

  presets: [
    [
      'classic',
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        docs: {
          sidebarPath: require.resolve('./sidebars.js'),
          // Please change this to your repo.
          editUrl: 'https://github.com/Green-Software-Foundation/carbon-aware-sdk',
        },
        blog: {
          showReadingTime: true,
          // Please change this to your repo.
          editUrl:
            'https://github.com/Green-Software-Foundation/carbon-aware-sdk',
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
      navbar: {
        title: 'Carbon Aware SDK',
        logo: {
          alt: 'Carbon Aware SDK Logo',
          src: 'img/GSF-logo.jpg',
        },
        items: [
          {
            type: 'doc',
            docId: 'intro',
            position: 'left',
            label: 'Tutorial',
          },
          {to: 'https://greensoftware.foundation/', label: 'GSF', position: 'left'},
          {
            href: 'https://github.com/Green-Software-Foundation/carbon-aware-sdk',
            label: 'GitHub',
            position: 'right',
          },
        ],
      },
      footer: {
        style: 'dark',
        links: [
          {
            title: 'Docs',
            items: [
              {
                label: 'Tutorial',
                to: '/docs/intro',
              },
            ],
          },
          {
            title: 'Community',
            items: [
              {
                label: 'Linkedin',
                href: 'https://www.linkedin.com/company/green-software-foundation/',
              },
              {
                label: 'Twitter',
                href: 'https://twitter.com/gsfcommunity',
              },         
              {
                label: 'Slack (private members only)',
                href: 'https://greensoftwarefdn.slack.com/archives/C02JRAV4QEP',
              }
            ],
          },
          {
            title: 'More',
            items: [
              {
                label: 'greensoftware.foundation',
                to: 'https://greensoftware.foundation/',
              },
              {
                label: 'GitHub',
                href: 'https://github.com/Green-Software-Foundation/carbon-aware-sdk',
              },
            ],
          },
        ],
      },
      prism: {
        theme: lightCodeTheme,
        darkTheme: darkCodeTheme,
      },
    }),
};

module.exports = config;
