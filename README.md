# VRCQuestTools

Avatar Converter and Utilities for Mobile (Android, iOS).

<a href="https://kurotu.booth.pm/items/2436054"><img alt="Booth" src="https://asset.booth.pm/static-images/banner/200x40_01.png"></a>
<a href="https://kurotu.github.io/vpm-repos/vpm.html"><img alt="Add to VCC" src="https://img.shields.io/badge/-Add%20to%20VCC-%232baac1?style=for-the-badge"></a>

<a href="https://tooomm.github.io/github-release-stats/?username=kurotu&repository=VRCQuestTools"><img alt="All Downloads" src="https://img.shields.io/github/downloads/kurotu/VRCQuestTools/total?label=downloads@all"></a>
<a href="https://tooomm.github.io/github-release-stats/?username=kurotu&repository=VRCQuestTools"><img alt="Latest downloads" src="https://img.shields.io/github/downloads/kurotu/VRCQuestTools/latest/total"></a>
<a href="https://kurotu.github.io/vpm-repos/"><img alt="VPM Release" src="https://img.shields.io/vpm/v/com.github.kurotu.vrc-quest-tools?repository_url=https%3A%2F%2Fkurotu.github.io%2Fvpm-repos%2Fvpm.json"></a>

VRCQuestTools is a Unity editor extension which converts PC VRChat avatars so they can also be used on Android (Quest, PICO) and iOS.
The Android and iOS versions of VRChat are collectively called **Mobile** below.

VRCQuestTools は、PC 用の VRChat アバターを Android (Quest, PICO) や iOS でも使えるように変換する Unity エディター拡張です。
以下では、Android 版と iOS 版の VRChat をまとめて **Mobile** と呼びます。

## Features / 主な特徴

- **Material conversion**: Reads the settings of PC shaders and automatically generates materials and textures for Mobile-supported shaders.
- **Removal of unsupported components**: Removes components which can't be used on Mobile at build time.
- **Non-destructive conversion**: With Non-Destructive Modular Framework (NDMF), avatars are automatically converted on upload without creating a copy in the scene.
- **PhysBone reduction support**: Helps you reduce PhysBones to fit within the performance limits.
- **Various utilities**: Provides tools which help Mobile support, such as copying blendshapes and applying recommended Unity settings.

<!-- -->

- **マテリアルの変換**：PC 用シェーダーの設定を読み取り、Mobile 対応シェーダー用のマテリアルとテクスチャを自動生成します。
- **非対応コンポーネントの削除**：Mobile で使用できないコンポーネントをビルド時に削除します。
- **非破壊変換**：Non-Destructive Modular Framework (NDMF) がある場合、シーンに複製を作らず、アップロード時に自動で変換します。
- **PhysBone の削減支援**：パフォーマンス制限に収めるための PhysBone 削減を支援します。
- **各種ユーティリティ**：ブレンドシェイプのコピーや Unity の推奨設定の適用など、Mobile 対応を助けるツールを備えています。

## Requirements / 動作環境

- Unity 2022.3.22f1
- VRChat SDK - Avatars 3.9.0 or later / 以降

## Install / インストール

Use [this link](https://kurotu.github.io/vpm-repos/vpm.html) to add the repository to VCC or [ALCOM](https://vrc-get.anatawa12.com/en/alcom/).
Then add the `VRCQuestTools` package to your project.

[このリンク](https://kurotu.github.io/vpm-repos/vpm.html)を使用して VCC または [ALCOM](https://vrc-get.anatawa12.com/ja/alcom/) にリポジトリを追加します。
その後、プロジェクトに `VRCQuestTools` パッケージを追加します。

Repository URL / リポジトリ URL: `https://kurotu.github.io/vpm-repos/vpm.json`

## Usage / 使い方

See [the documentation site](https://kurotu.github.io/VRCQuestTools/).

[ドキュメント](https://kurotu.github.io/VRCQuestTools/ja/)を参照してください。

## License / ライセンス

The MIT License.

本ツールは MIT ライセンスで提供されます。

## Contact / 連絡先

- VRCID: kurotu
- X: [@kurotu](https://x.com/kurotu)
- GitHub: [kurotu/VRCQuestTools](https://github.com/kurotu/VRCQuestTools)

## Donation / ご支援

- [Booth](https://kurotu.booth.pm/items/2436054)
- [pixivFANBOX](https://kurotu.fanbox.cc/)
- [GitHub Sponsors](https://github.com/sponsors/kurotu)
