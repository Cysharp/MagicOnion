---
title: "関連リソース"
---
import { sortAndFilterResources, locales, formatDate } from '@site/src/data/resources';
export const acceptLocales = ['ja', 'en'];

# 関連リソース

:::info
このページで紹介している関連リソースは MagicOnion プロジェクトが提供するものではなく、リンク先のコンテンツの正確性について保証しません。
:::

## スライド {#presentations}
<ul>
{sortAndFilterResources('slide', acceptLocales).map(x =>
    <li>
        <a href={x.url}>{x.title}</a>
        <div><span>({formatDate(x.year, x.month)})</span> {x.description}</div>
    </li>
)}
</ul>

## ブログ/ドキュメント {#articles}
<ul>
{sortAndFilterResources('article', acceptLocales).map(x =>
    <li>
        <a href={x.url}>{x.title}</a> <span>({formatDate(x.year, x.month)})</span>
    </li>
)}
</ul>

## リンクを追加や削除、編集する {#edit-links}
このページのリンクは [GitHub 上のファイルで管理されています](https://github.com/Cysharp/MagicOnion/tree/main/docs/src/data/resources.tsx)。リンクを追加や削除、編集したい場合は、ソースファイルを編集して Pull Request を送信してください。
