---
sidebar_position: 1
slug: /tutorial/set-up-environment
---

import AddToVccLink from '@site/src/components/AddToVccLink';

# 環境を準備する

アバターを変換する前に、プロジェクトに VRCQuestTools をインストールし、Unity を Mobile 向けの設定に整えます。
準備が要るのはプロジェクトだけではありません。
Unity 本体に Android Build Support というモジュールを追加していないと、そもそも Android 向けにアップロードするためのプラットフォーム切り替えができないのです。

## 前提

アバターを PC 向けにアップロードできるプロジェクトを想定しています。

## VRCQuestTools をインストールする

VRCQuestTools は VPM リポジトリで配布されています。
VRChat Creator Companion (VCC) または [ALCOM](https://vrc-get.anatawa12.com/ja/alcom/) にリポジトリを追加してインストールします。

1. 次のボタンを押し、リポジトリを VCC または ALCOM に追加します。

    <AddToVccLink className="button button--primary" />

2. プロジェクトのパッケージ管理画面で「VRCQuestTools」を追加します。

:::note
リポジトリを手動で追加する場合は、次の URL を使用します。

```
https://kurotu.github.io/vpm-repos/vpm.json
```

:::

インストールが終わると、Unity のメニューバーに「Tools」→「VRCQuestTools」が表示されます。

:::tip アップロード時に自動で変換する NDMF
変換と聞くと、セットアップ済みのアバターが書き換えられてしまわないか気になるかもしれません。
変換は元のアバターを書き換えません。アバターとアセットを複製し、複製の側を変換します。
プロジェクトに Non-Destructive Modular Framework (NDMF) パッケージがあると、さらに複製も作らず、アップロード時に自動で変換できます。
Modular Avatar のような他の非破壊ツールとも、そのまま組み合わせて使えます。
NDMF は、[Modular Avatar](https://modular-avatar.nadena.dev/ja) のトップページにある「ダウンロード」ボタンでリポジトリを追加してインストールできます。
Modular Avatar を導入している場合、NDMF はすでにプロジェクトに含まれています。
:::

## Android Build Support をインストールする {#android-build-support}

Android 向けにアバターをアップロードするには、Unity の Android Build Support モジュールが必要です。
これがないと、VRChat SDK のコントロールパネルでプラットフォームを Android に切り替えられません。
Unity Hub から次の手順でインストールします。

1. Unity Hub の「Installs」を開きます。
2. プロジェクトで使用している Unity バージョンの歯車アイコンから「Add modules」を選択します。
3. 「Android Build Support」にチェックを入れてインストールします。

![Unity Hub の Installs 画面](/img/unity-hub_installs.png)

![Add modules 画面](/img/unity-hub_add-modules.png)

:::note
「OpenJDK」にチェックを入れる必要はありません。
「Android SDK & NDK Tools」も通常は不要ですが、Android 実機でのローカルテスト (Build & Test) を行う場合には必要です。
:::

インストール後に Unity を起動し直すと、VRChat SDK のコントロールパネルでプラットフォームを Android に切り替えられるようになります。

### Unity Hub からインストールできない場合 {#without-unity-hub}

「Add modules」の一覧に表示されないなど、Unity Hub からインストールできない場合は、Unity 公式サイトの追加インストーラーを使用します。

1. [Unity ダウンロードアーカイブ](https://unity.com/releases/editor/archive)で、プロジェクトと同じ Unity バージョン（例: 2022.3.22f1）を選択します。
2. 「Component installers」から「Android Build Support」をダウンロードします。
3. ダウンロードしたインストーラーを実行し、画面の指示に従ってインストールします。
4. Unity エディターを起動していた場合は再起動します。

iOS Build Support も同じ手順でインストールできます（手順 2 で「iOS Build Support」をダウンロードします）。

## iOS Build Support をインストールする（オプション） {#ios-build-support}

iOS 版の VRChat 向けにもアップロードする場合は、iOS Build Support モジュールも必要です。
Android Build Support と同じく、Unity Hub の「Add modules」から「iOS Build Support」をインストールします。

iOS Build Support をインストールすると、VRChat SDK のコントロールパネルでプラットフォームを iOS に切り替えられるようになります。

## Unity の推奨設定を適用する {#unity-settings}

メニューバーの「Tools」→「VRCQuestTools」→「Unity Settings for Mobile」を開くと、Mobile 向け作業のための推奨設定を確認できます。
Android Build Support と iOS Build Support の導入状況も、このウィンドウで確認できます。

![Unity Settings for Mobile ウィンドウ](/img/set-up-environment.png)

- **Android テクスチャ圧縮**：ASTC を使用すると、Android 用のテクスチャ圧縮に時間がかかる代わりに画質が向上します。

「すべての設定を適用」を押すと、推奨設定をまとめて適用できます。

## 次のステップ

環境の準備ができたら、[アバターを変換する](./convert-avatar.md)に進んでください。
