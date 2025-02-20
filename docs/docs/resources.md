---
unlisted: true
title: "Resources"
---
import { sortAndFilterResources, locales, formatDate } from '@site/src/data/resources';
export const acceptLocales = ['en'];

# Resources

:::info
The resources listed on this page include slides, blog posts, and documents related to MagicOnion provided by the community, not just the MagicOnion project and Cysharp, Inc. The project does not guarantee the accuracy of the resources linked to.
:::

## Presentations {#presentations}
<ul>
{sortAndFilterResources('slide', acceptLocales).map(x =>
    <li>
        <a href={x.url}>{x.title}</a> {x.official && <span className={'badge badge--secondary'}>Official</span>}
        <div><span>({formatDate(x.year, x.month)})</span> {x.description}</div>
    </li>
)}
</ul>

## Articles/Documents {#articles}
<ul>
{sortAndFilterResources('article', acceptLocales).map(x =>
    <li>
        <a href={x.url}>{x.title}</a> <span>({formatDate(x.year, x.month)})</span> {x.official && <span className={'badge badge--secondary'}>Official</span>}
    </li>
)}
</ul>

## Add, Remove, or Edit Links {#edit-links}
The links on this page are [managed in a file on GitHub](https://github.com/Cysharp/MagicOnion/tree/main/docs/src/data/resources.tsx). If you would like to add, remove, or edit a link, please edit the source file and submit a pull request.
