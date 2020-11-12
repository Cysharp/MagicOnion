# How to build documents with DocFX

The documents are published using DocFX.

## Prepare
- Download [DocFX 2.x](https://dotnet.github.io/docfx/)
- `git clone --depth=1 https://github.com/Cysharp/DocfxTemplate _DocfxTemplate`

## Preview
```
docfx build --serve
```

## Publish
```
docfx build
```

The generated documents and static assets are published under `_site` directory.