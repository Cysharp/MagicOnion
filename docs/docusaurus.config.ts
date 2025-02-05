import {themes as prismThemes} from 'prism-react-renderer';
import type {Config} from '@docusaurus/types';
import type * as Preset from '@docusaurus/preset-classic';

import additionalHeaderMetaRow from './src/remark/additionalHeaderMetaRow';

// This runs in Node.js - Don't use client-side code here (browser APIs, JSX...)

const config: Config = {
  title: 'MagicOnion',
  tagline: 'Unified Realtime/API framework for .NET platform and Unity.',
  favicon: 'img/favicon.ico',

  // Set the production url of your site here
  url: 'https://cysharp.github.io',
  // Set the /<baseUrl>/ pathname under which your site is served
  // For GitHub pages deployment, it is often '/<projectName>/'
  baseUrl: process.env.GITHUB_REPOSITORY?.replace(/^[^/]+\//, '') || '/MagicOnion/',

  // GitHub pages deployment config.
  // If you aren't using GitHub pages, you don't need these.
  organizationName: 'Cysharp', // Usually your GitHub org/user name.
  projectName: 'MagicOnion', // Usually your repo name.

  trailingSlash: false,
  onBrokenLinks: 'throw',
  onBrokenMarkdownLinks: 'warn',

  // Even if you don't use internationalization, you can use this field to set
  // useful metadata like html lang. For example, if your site is Chinese, you
  // may want to replace "en" with "zh-Hans".
  i18n: {
    defaultLocale: 'en',
    locales: ['en', 'ja', 'ko'],
    localeConfigs: {
      ja: {
        htmlLang: 'ja-JP',
      },
      ko: {
        htmlLang: 'ko-KR',
      }
    }
  },

  presets: [
    [
      'classic',
      {
        docs: {
           // docs-only mode https://docusaurus.io/docs/next/docs-introduction#home-page-docs
          routeBasePath: '/',

          sidebarPath: './sidebars.ts',
          sidebarCollapsed: true,

          showLastUpdateTime: true,

          editUrl: (params) => params.locale == 'en'
            ? `https://github.com/Cysharp/MagicOnion/tree/main/docs/docs/${params.docPath}`
            : `https://github.com/Cysharp/MagicOnion/tree/main/docs/i18n/${params.locale}/docusaurus-plugin-content-docs/current/${params.docPath}`,

          remarkPlugins: [additionalHeaderMetaRow],
        },
        theme: {
          customCss: './src/css/custom.css',
        },
      } satisfies Preset.Options,
    ],
  ],

  themes: ['@docusaurus/theme-mermaid'],
  themeConfig: {
    // Replace with your project's social card
    //image: 'img/docusaurus-social-card.jpg',
    navbar: {
      title: 'MagicOnion',
      logo: {
        alt: '',
        src: 'img/icon.png',
      },
      items: [
        {
          type: 'docSidebar',
          sidebarId: 'docsSidebar',
          position: 'left',
          label: 'Documents',
        },
        {
          type: 'doc',
          position: 'left',
          label: 'License',
          docId: 'license',
        },
        {
          type: 'doc',
          position: 'left',
          label: 'Support',
          docId: 'support',
        },
        {
          href: 'https://cysharp.co.jp/en/',
          label: 'Cysharp, Inc.',
          position: 'right',
        },
        {
          href: 'https://github.com/Cysharp/MagicOnion',
          label: 'GitHub',
          position: 'right',
        },
        {
          type: 'localeDropdown',
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
              label: 'About MagicOnion',
              to: '/',
            },
            {
              label: 'Quick Start',
              to: '/quickstart',
            },
          ],
        },
        {
          title: 'More',
          items: [
            {
              label: 'Cysharp, Inc.',
              href: 'https://cysharp.co.jp/en/',
            },
            {
              label: 'GitHub',
              href: 'https://github.com/Cysharp/MagicOnion',
            },
          ],
        },
      ],
      copyright: `Copyright © ${new Date().getFullYear()} Cysharp, Inc. Built with Docusaurus.`,
    },
    prism: {
      additionalLanguages: [ 'csharp' ],
      theme: prismThemes.vsLight,
      darkTheme: prismThemes.vsDark,
    },
    algolia: {
      appId: '0DYBMKEZO1',
      apiKey: '64d5987b3063e5de9b6da30e6171fb8e',
      indexName: 'cysharpio',
    },
  } satisfies Preset.ThemeConfig,

  markdown: {
    mermaid: true,
  }
};

export default config;
