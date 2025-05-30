---
sidebar_position: 5
---

# 非破壊的な変換

プロジェクトにNon-Destructive Modular Framework (NDMF)を導入することで、アバターの変換をビルド中に行うことができます。
シーンには変換前のアバターだけを保存すればよく、また他の非破壊的ツールとの連携も可能です。

## NDMFのインストール

プロジェクトに[Modular Avatar]や[Anatawa12's AvatarOptimizer]がインストールされている場合、NDMFは既にインストールされています。

手動でNDMFをインストールする場合は、以下の手順に従ってください。

1. [Modular Avatar]のWebサイトからVCCにリポジトリを追加します。
2. VCCで、NDMFを追加したいアバタープロジェクトを選択します。
3. **Manage Project**ボタンをクリックします。
4. **Non-Destructive Modular Framework**の行の⊕ボタンをクリックします。
5. 最新版のNDMFがプロジェクトに追加されます。

## アバターの設定

通常通り、アバターにAvatar Converter Settingsコンポーネントを追加して変換設定をします。

非破壊的ワークフローではすべての調整をビルド中に実施するため、追加のコンポーネントが必要な場合があります。

### ネットワークIDの割り当て

アバターにNetwork ID Assignerコンポーネントを追加することで、PhysBoneに自動的にネットワークIDを割り当てます。
ヒエラルキーパスのハッシュ値をもとにIDを割り当てるためオブジェクトが増減しても同じIDが割り当てられ、PC-Quest間でPhysBoneの同期が可能です。

この時点で一度PC用にアバターをアップロードして、ネットワークIDを反映しておきます。

### 頬染め用メッシュの削除

頬染め用シェイプキーのメッシュを削除する設定をします。
ここでは例として、[Anatawa12's AvatarOptimizer]の[AAO Remove Mesh By BlendShape](https://vpm.anatawa12.com/avatar-optimizer/ja/docs/reference/remove-mesh-by-blendshape/)コンポーネントを使用します。

Remove Mesh By BlendShapeコンポーネントを顔のメッシュに追加し、削除したい頬染め用シェイプキーのチェックボックスをオンにします。

### Androidビルドの時だけメッシュを削除

このままではPC向けのビルドでも頬染め用メッシュが削除されてしまいます。
そのため、Platform Component Removerを追加し、PCのチェックボックスをオフにします。
これにより、PC向けのビルドではRemove Mesh By BlendShapeコンポーネントが削除され、頬染め用メッシュが削除されません。

![Platform Component Remover](/img/platform-component-remover.png)

## アバターのテスト

アバターをアップロードする前に、アバターをテストします。

:::note
Android端末でテストする場合、USBケーブルでPCに接続してデバッグを有効にします。

参照:
- [Build and Test for Android Mobile](https://creators.vrchat.com/platforms/android/build-test-mobile/)
:::

1. VRChat SDKコントロールパネルを開き、テストするアバターを**Builder**タブで選択します。
2. メニューバーの**Tools** > **VRCQuestTools** > **Show Avatar Builder**を選択します。
3. **VQT Avatar Builder**ウィンドウが表示されます。
4. **Build & Test on PC/Android**ボタンをクリックします。
5. Android用の設定でアバターがビルドされます。

ビルドしたアバターはVRChatのアバター一覧の**Others**セクションに表示されます。
VRChatを起動し、アバターをテストしましょう。

## アバターのアップロード

テストで問題がなければ、アバターをAndroid向けにアップロードします。
アバターを非破壊的に変換しながらアップロードするには以下の方法があります。

- [通常のアップロード](#通常のアップロード) (VRChat SDK)
- [Multi-Platform Build](#multi-platform-build) (VRChat SDK)
- [VQT Avatar Builder](#vqt-avatar-builder) (VRCQuestTools)
- [ContinuousAvatarUploader](https://github.com/anatawa12/ContinuousAvatarUploader)

### 通常のアップロード

VRChat SDK 3.8.1以降では、シーン上のアバターがAndroid対応の条件を満たしていなくてもアップロードを開始できます。
PC用にアバターをアップロードするときと同様にVRChat SDKコントロールパネルを開いてアップロードします。

### Multi-Platform Build

VRChat SDK 3.7.6以降の**Multi-Platform Build**を使用するとPCとAndroidの両方に対して一度にアバターをアップロードできます。

1. Unityのビルド設定を**Windows**に変更します。
2. VRChat SDKコントロールパネルを開き、アップロードするアバターを**Builder**タブで選択します。
3. Platform(s)で**Windows**と**Android**を選択します。
    ![Multi-Platform Build](/img/multi_platform_build.png)
4. **Multi-Platform Build & Publish**ボタンをクリックします。
5. PCとAndroidの両方にアバターがアップロードされます。

### VQT Avatar Builder

[VQT Avatar Builder]を使用すると、ビルド前のチェックを無視してアバターをアップロードできます。

1. Unityのビルド設定をAndroidに変更します。
2. VRChat SDKコントロールパネルを開き、アップロードするアバターを**Builder**タブで選択します。
3. メニューバーの**Tools** > **VRCQuestTools** > **Show Avatar Builder**を選択します。
4. **VQT Avatar Builder**ウィンドウが表示されます。
5. **Build & Publish for Android**ボタンをクリックします。
6. アップロードが完了すると、アバターのサムネイルに緑色のMobileアイコンが表示されます。

[Modular Avatar]: https://modular-avatar.nadena.dev/ja
[Anatawa12's AvatarOptimizer]: https://vpm.anatawa12.com/avatar-optimizer/ja/
[VQT Avatar Builder]: ../references/main-menu/show-avatar-builder.md
