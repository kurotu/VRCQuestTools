---
sidebar_position: 4
---

# トラブルシューティング

## UnityをAndroid用に切り替えられない

### Android Build Supportをインストールする

Android用にアバターをアップロードするにはUnityのビルド設定をAndroidに切り替える必要があります(Switch Platform)。
UnityにAndroid Build Supportがインストールされていない場合、ビルド設定をAndroidに切り替えることができません。

[チュートリアル: 環境の準備](./tutorial/set-up-environment.mdx)

## 非圧縮サイズが大きくてアップロードできない

`Avatar uncompressed size is too large for the target platform. XX.XX MB > 40.00 MB`

非圧縮サイズ(Uncompressed Size)が大きくなる原因として、主に以下のようなものがあります。

- 使用していないシェイプキー
- 使用していないメッシュ
- 使用していないテクスチャ

### 最適化により不要なデータを削除する

アバターの最適化ツールを使うことで、これらの不要なデータを削除できます。
例として[Avatar Optimizer](https://anatawa12.booth.pm/items/4885109)の[Trace And Optimize](https://vpm.anatawa12.com/avatar-optimizer/ja/docs/reference/trace-and-optimize/)コンポーネントを使うと不要なものをアップロード時に自動的に削除できます。 

## ダウンロードサイズが大きくてアップロードできない

`Avatar download size is too large for the target platform. XX.XX MB > 10.00 MB`

ダウンロードサイズ(Download Size)が大きくなる原因として、主に以下のようなものがあります。

- テクスチャの枚数が多い
- テクスチャの圧縮率が低い
- テクスチャの解像度が大きい
- メッシュの数が多い

### テクスチャのサイズを調整する

[Avatar Converter Settings](./references/components/avatar-converter-settings.md)コンポーネントの`最大テクスチャサイズ`を小さくすることでダウンロードサイズは小さくなりますが、テクスチャの品質を大きく損ないます。
最大テクスチャサイズを変更するよりも先に、`圧縮形式`を変更して調整することを推奨します。

### テクスチャをアトラス化する

テクスチャのアトラス化をした上で合計の解像度を下げることでダウンロードサイズを小さくすることもできます。
例として[TexTransTool](https://rs-shop.booth.pm/items/4833984)を使って[アトラス化](https://ttt.rs64.net/docs/Tutorial/ReductionTextureMemoryByAtlasing)をすることができます。

### メニュー用アイコンを小さくする

表情やポーズアニメーションを追加するツールを使用している場合、大量のメニュー用アイコンがダウンロードサイズを圧迫することがあります。
[Menu Icon Resizer](./references/components/menu-icon-resizer.md)コンポーネントを使うことで、メニュー用アイコンの解像度を小さくしたり削除したりすることができます。

### 衣装の着替えなどのギミックを減らす

着替えなどのギミックを実装している場合、着替え用の衣装の分だけメッシュやテクスチャが増えるためダウンロードサイズが大きくなります。
ギミックを減らし、可能であれば一つのアバターにつき衣装は一つだけにしてアップロードします。
同期ズレにより着替えなどのギミックが片方のプラットフォームで意図しない状態になってしまうことを防ぐため、PCとAndroidでは同じ構成のアバターにすることを推奨します。

## アップロードに成功したがSecurity checks failedと表示される

VRChatのサーバー側でのセキュリティチェックに失敗した場合、アップロードに成功してもアバターが使用できないことがあります。

### プラットフォームを再度切り替えてアップロードする

実際には問題がないのにセキュリティチェックに失敗するケースとして、Unityのビルドターゲット切り替えに失敗していた事例が報告されています。
もう一度Switch Platfromを行うか、Unityを再起動してからアップロードを試みてください。
このとき、Unityのビルドターゲットがアップロード先のプラットフォームになっていることを確認してください。

![Unityのビルドターゲットの確認方法](/img/unity_titlebar_android.png)

## PCとAndroidでギミックが同期しない

PCとAndroidでExpression Parametersの順番が一致しない場合、誤った値が同期されてギミックの誤動作に繋がります。

### PCとAndroidで同じ構成のアバターにする

PCの方にだけ特定のギミックがありAndroidの方にはない場合に特に発生しやすくなるため、PCとAndroidでは同じ構成のアバターにすることを推奨します。

### Expression Parametersの順番を一致させる

[Modular Avatar](https://modular-avatar.nadena.dev/ja)の[Sync Parameter Sequence](https://modular-avatar.nadena.dev/ja/docs/reference/sync-parameter-sequence)コンポーネントを使用することでExpression Parametersの順番を一致させることができます。

## ギミックが動作しない

Androidでは使用不可能なコンポーネントがあるため、ギミックが動作しないことがあります。

### Uniyt ConstraintsをVRChat Constraintsにする

UnityのConstraintsコンポーネントはAndroidでは使用できないため、VRChat Constraintsに置き換える必要があります。
[Modular Avatar](https://modular-avatar.nadena.dev/ja)の[Convert Constraints](https://modular-avatar.nadena.dev/ja/docs/reference/convert-constraints)コンポーネントを使用することでConstraintsを自動的に変換することができます。

### Contact ReceiverとContact Senderを有効にする

[Avatar Converter Settings](./references/components/avatar-converter-settings.md)コンポーネントの`Avatar Dynamics 設定`でContact ReceiverとContact Senderのチェックボックスがオンになっていることを確認してください。

## PCとAndroidでPhysBoneが同期しない

PCとAndroidでPhysBoneの数や順序が異なる場合、PhysBoneを掴んだときの動作が同期しません。

### ネットワークIDを一致させる

[Network ID Assigner](./references/components/network-id-assigner.md)コンポーネントを使用してPCとAndroidの両方に再度アップロードすることで、PhysBoneを同期させることができます。
