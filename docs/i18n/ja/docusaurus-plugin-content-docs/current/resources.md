---
title: "関連リソース"
---
import { sortAndFilterResources, locales, formatDate } from '@site/src/data/resources';
export const acceptLocales = ['ja', 'en'];

# 関連リソース

このページでは MagicOnion に関連するスライドやブログ記事、ドキュメントなどのリソースを紹介しています。

:::info
紹介している関連リソースは MagicOnion プロジェクトおよび Cysharp によるものだけでなく、コミュニティーによって提供されているものを含みます。プロジェクトはリンク先のリソースの正確性については保証していません。
:::

## スライド {#presentations}
<ul>
{sortAndFilterResources('slide', acceptLocales).map(x =>
    <li>
        <a href={x.url}>{x.title}</a> {x.official && <span className={'badge badge--secondary'}>Official</span>}
        <div><span>({formatDate(x.year, x.month)})</span> {x.description}</div>
    </li>
)}
</ul>

## ブログ/ドキュメント {#articles}
<ul>
{sortAndFilterResources('article', acceptLocales).map(x =>
    <li>
        <a href={x.url}>{x.title}</a> <span>({formatDate(x.year, x.month)}) {x.description} {x.official && <span className={'badge badge--secondary'}>Official</span>}</span>
    </li>
)}
</ul>

## リンクを追加や削除、編集する {#edit-links}
このページのリンクは [GitHub 上のファイルで管理されています](https://github.com/Cysharp/MagicOnion/tree/main/docs/src/data/resources.tsx)。リンクを追加や削除、編集したい場合は、ソースファイルを編集して Pull Request を送信してください。
