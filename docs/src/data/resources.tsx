import { useDateTimeFormat } from '@docusaurus/theme-common/internal';

const Slides: Resource[] = [
    {
        type: 'slide',
        locale: 'ja',
        title: '【CEDEC2025】FINAL FANTASY VII EVER CRISIS のマルチプレイを支えるバックエンド技術と仕組み (動画)',
        url: 'https://www.youtube.com/watch?v=EkPBh1vBzXA',
        description: 'CEDEC 2025',
        year: 2025,
        month: 7,
    },
    {
        type: 'slide',
        locale: 'ja',
        title: 'FINAL FANTASY VII EVER CRISIS のマルチプレイを支えるバックエンド技術と仕組み',
        url: 'https://cedil.cesa.or.jp/cedil_sessions/view/3118',
        description: 'CEDEC 2025',
        year: 2025,
        month: 7,
    },
    {
        type: 'slide',
        locale: 'ja',
        title: '.NET のための通信フレームワーク MagicOnion 入門',
        url: 'https://speakerdeck.com/mayuki/introduction-to-magiconion',
        description: 'Cysharp x Sansan: イマドキのC#/.NET開発 〜最新の言語とフレームワークの使い方〜',
        year: 2024,
        month: 11,
        official: true,
    },
    {
        type: 'slide',
        locale: 'ja',
        title: 'MagicOnionサーバーのパフォーマンス調査と.NET更新によるパフォーマンス改善',
        url: 'https://www.docswell.com/s/toutou/Z7RJLD-2024-11-17-155150',
        description: 'IwakenLab Tech Conference 2024',
        year: 2024,
        month: 11,
    },
    {
        type: 'slide',
        locale: 'ja',
        title: 'メタバースプラットフォーム 「INSPIX WORLD」はPHPもC++もまとめてC#に統一！ ～MagicOnionが支えるバックエンド最適化手法～',
        url: 'https://speakerdeck.com/pulse1923/metabasupuratutohuomu-inspix-world-haphpmoc-plus-plus-momatometec-number-nitong-magiconiongazhi-erubatukuendozui-shi-hua-shou-fa',
        description: 'CEDEC 2023',
        year: 2023,
        month: 9,
        official: true,
    },
    {
        type: 'slide',
        locale: 'ja',
        title: 'C#によるクライアント/サーバーの開発言語統一がもたらす高効率な開発体制 ～プリコネ！グランドマスターズ開発事例～',
        url: 'https://speakerdeck.com/cygames/sabanokai-fa-yan-yu-tong-gamotarasugao-xiao-lu-nakai-fa-ti-zhi-purikone-gurandomasutazukai-fa-shi-li',
        description: 'CEDEC 2022',
        year: 2022,
        month: 8,
    },
    {
        type: 'slide',
        locale: 'ja',
        title: '[CEDEC 2021] 運用中タイトルでも怖くない！ 『メルクストーリア』におけるハイパフォーマンス・ローコストなリアルタイム通信技術の導入事例',
        url: 'https://www.slideshare.net/slideshow/cedec-2021/250046958',
        description: 'CEDEC 2021',
        year: 2021,
        month: 8,
    },
    {
        type: 'slide',
        locale: 'ja',
        title: 'Unity C# × gRPC × サーバーサイドKotlinによる次世代のサーバー/クライアント通信 〜ハイパフォーマンスな通信基盤の開発とMagicOnionによるリアルタイム通信の実現〜',
        url: 'https://speakerdeck.com/n_takehata/kuraiantotong-xin-haipahuomansunatong-xin-ji-pan-falsekai-fa-tomagiconionniyoruriarutaimutong-xin-falseshi-xian',
        description: 'CEDEC 2019',
        year: 2019,
        month: 9,
        official: true,
    },
    {
        type: 'slide',
        locale: 'ja',
        title: 'MagicOnion〜C#でゲームサーバを開発しよう〜 | Unity Learning Materials',
        url: 'https://learning.unity3d.jp/4018/',
        year: 2019,
        month: 10,
    },
];

const Articles: Resource[] = [
    {
        type: 'article',
        locale: 'ja',
        title: 'MagicOnionでgRPC-Webを使う #C# - Qiita',
        url: 'https://qiita.com/inco-cyber/items/74715318a7f40d819d64',
        year: 2025,
        month: 3
    },
    {
        type: 'article',
        locale: 'ja',
        title: '【.NET8】MagicOnionでAPIサーバを立てる #C# - Qiita',
        url: 'https://qiita.com/inco-cyber/items/3253235a0a9d5fda2b1e',
        year: 2025,
        month: 3
    },
    {
        type: 'article',
        locale: 'ja',
        title: 'ASP.NETでMagicOnionに認証機能を実装する',
        url: 'https://qiita.com/ANIZA_15/items/862d53832ab152bebc35',
        year: 2024,
        month: 12,
    },
    {
        type: 'article',
        locale: 'ja',
        title: 'MagicOnion + NATS + LogicLooperでC#大統一！やってみた - Qiita',
        url: 'https://qiita.com/Euglenach/items/bbafa918f114f51e4104',
        year: 2024,
        month: 12,
    },
    {
        type: 'article',
        locale: 'ja',
        title: 'MagicOnion + Unity + Firebase Auth でユーザー認証できるようにしたい',
        url: 'https://zenn.dev/yuki8128/scraps/53224b5a85bc80',
        year: 2024,
        month: 3,
    },
    {
        type: 'article',
        locale: 'ja',
        title: 'MagicOnion を使って AWS で専用ゲームサーバーを作ろう - builders.flash☆ - 変化を求めるデベロッパーを応援するウェブマガジン | AWS',
        url: 'https://aws.amazon.com/jp/builders-flash/202403/magiconion-game-server/',
        year: 2024,
        month: 2,
    },
    {
        type: 'article',
        locale: 'ja',
        title: 'Unity(YetAnotherHttpHandler)とMagicOnionで自己署名証明書を使ったHTTPS&HTTP/2通信',
        url: 'https://zenn.dev/toutou/articles/2611fe24bd1939',
        year: 2024,
        month: 1,
    },
    {
        type: 'article',
        locale: 'ko',
        title: 'MagicOnion을 이용한 채팅 만들기 1 : Server',
        url: 'https://podo1017.tistory.com/402',
        year: 2024,
        month: 8,
    },
    {
        type: 'article',
        locale: 'cn',
        title: 'MagicOnion 结合 Orleans – Bruce2077',
        url: 'https://www.bruce2077.xyz/2024/07/23/magiconion-%E7%BB%93%E5%90%88-orleans/',
        year: 2024,
        month: 7,
    },
    {
        type: 'article',
        locale: 'cn',
        title: 'C# 使用 MagicOnion + MessagePack + YetAnotherHttpHandler 进行实时通信 – Bruce2077',
        url: 'https://www.bruce2077.xyz/2024/07/17/c-%E4%BD%BF%E7%94%A8-magiconion-messagepack-yetanotherhttphandler-%E8%BF%9B%E8%A1%8C%E5%AE%9E%E6%97%B6%E9%80%9A%E4%BF%A1/',
        year: 2024,
        month: 7,
    },
    {
        type: 'article',
        locale: 'ja',
        title: 'MagicOnion + MessagePack + YetAnotherHttpHandler でリアルタイム通信を行う',
        url: 'https://zenn.dev/toutou/articles/7918da3d1a9e1d',
        year: 2024,
        month: 1,
    },
    {
        type: 'article',
        locale: 'ja',
        title: 'VRメタバースのリアルタイム通信サーバーの技術にMagicOnionとNATSを選んだ話 - ambr Tech Blog',
        url: 'https://zenn.dev/ambr_inc/articles/c2cd63556eed88',
        year: 2023,
        month: 5,
    },
    {
        type: 'article',
        locale: 'ja',
        title: 'メルクストーリアのリアルタイム通信開発におけるコード共有について',
        url: 'https://zenn.dev/happy_elements/articles/hekk_ac_20211214',
        year: 2021,
        month: 12,
    },
    {
        type: 'article',
        locale: 'ja',
        title: 'MagicOnionから始めるリアルタイム通信 (後編)',
        url: 'https://zenn.dev/happy_elements/articles/hekk_ac_20201208',
        year: 2020,
        month: 12,
    },
    {
        type: 'article',
        locale: 'ja',
        title: 'MagicOnionから始めるリアルタイム通信 (前編)',
        url: 'https://zenn.dev/happy_elements/articles/hekk_ac_20201207',
        year: 2020,
        month: 12,
    },
    {
        type: 'article',
        locale: 'ja',
        title: 'MagicOnion – C#による .NET Core/Unity 用のリアルタイム通信フレームワーク | Cygames Engineers\' Blog',
        url: 'https://tech.cygames.co.jp/archives/3181/',
        year: 2018,
        month: 12,
        official: true,
    },
];

export type ResourceType = "slide" | "video" | "article" | "book" | "other";
export type Resource = {
    type: ResourceType,
    locale: string,
    title: string,
    url: string,
    description?: string,
    year: number,
    month: number,
    official?: boolean,
};

export const resources = [...Slides, ...Articles];
export const locales = resources.map(x => x.locale).reduce((acc, x) => acc.includes(x) ? acc : [...acc, x], [] as string[]);

export function sortAndFilterResources(type: ResourceType, acceptLocales: string[]) {
    const newResources = resources
        .filter(x => x.type == type && acceptLocales.includes(x.locale))
        .slice();
     newResources.sort((a, b) => (b.year * 100 + b.month) - (a.year * 100 + a.month));
     return newResources;
}

export function formatDate(year, month) {
    const formatter = useDateTimeFormat({
        month: 'short',
        year: 'numeric',
        timeZone: 'UTC',
    });
    return formatter.format(new Date(year, month));
}
