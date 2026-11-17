# Website

This website is built using [Docusaurus](https://docusaurus.io/), a modern static website generator.

### Installation

```
$ pnpm
```

### Local Development

```
$ pnpm start
```

This command starts a local development server and opens up a browser window. Most changes are reflected live without having to restart the server.

### Build

```
$ pnpm build
```

This command generates static content into the `build` directory and can be served using any static contents hosting service.

### Dependency maintenance

Run `pnpm audit` after updating dependencies and verify the website with `pnpm build`.

The overrides in `package.json` select patched versions of Tinypool, Serialize JavaScript, SockJS's UUID dependency, and Mermaid's KaTeX dependency until upstream dependency ranges include those fixes. KaTeX stays on 0.18.4 because later releases require Node.js 22 through their CLI dependency. Mermaid stays on version 11 to avoid introducing the EPL/GPL-licensed `elkjs` dependency. The Can I Use data, `graceful-fs`, and `sax` retain existing versions to avoid unrelated dependency and license changes. Review these overrides when upgrading Docusaurus.
