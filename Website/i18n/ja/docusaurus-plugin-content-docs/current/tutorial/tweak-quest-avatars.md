---
sidebar_position: 4
---

# Androidアバターの調整

テスト後、Androidアバターに問題がある場合があります。
このページでは、例を交えてAndroidアバターの調整方法を説明します。

:::note
このページでは、基本的なUnityの用語の知識が必要です。
:::

## 半透明なメッシュ

Androidアバターは半透明なマテリアルをサポートしていません。
そのため、変換されたアバターには半透明なマテリアルに問題がある場合があります。
例えば、表情（赤面や青ざめ）、眼鏡のレンズ、目(角膜)などです。

問題を解決するにはいくつか方法がありますが、このページでは3つの方法を説明します。

- [アニメーションの編集](#アニメーションの編集)
- [半透明メッシュの削除](#半透明メッシュの削除)
- [テクスチャの調整](#テクスチャの調整)

:::caution
`VRChat/Mobile/Particles`にあるシェーダーを半透明なマテリアルの代替として使用しないでください。
これらはパーティクル用であり、アバター用ではありません。

[Quest Content Limitations](https://creators.vrchat.com/platforms/android/quest-content-limitations/#shaders)を参照してください。
:::

### アニメーションの編集

多くの場合、問題のある表情はブレンドシェイプのアニメーションとして実装されています。
そのため、アニメーションを編集することで抑制できます。

1. プロジェクトフォルダから問題のあるアニメーションクリップを見つけて複製します。
2. 複製したアニメーションクリップをアニメーションウィンドウで開きます。
3. 問題のあるブレンドシェイプを使用しないようにアニメーションパラメータを編集します。
4. FXレイヤーのAnimator Controllerを複製して開きます。
5. 問題のあるアニメーションを編集したアニメーションに置き換えます。
6. 複製したAnimator ControllerをFXレイヤーに設定します。

:::tip
新しいアニメーションクリップを作成した後、元のアニメーションをオーバーライドするために**[Animator Override Controller](https://docs.unity3d.com/2019.4/Documentation/Manual/AnimatorOverrideController.html)**を作成できます。
VRCQuestToolsはアバターを変換する際に自動的にAnimator Override Controllerを解決して新しいAnimator Controllerを作成します。
詳細については、[リファレンスページ](../references/components/avatar-converter-settings.md)を参照してください。
:::

### 半透明メッシュの削除

アニメーションを編集する代わりに、メッシュの問題のある部分を削除することもできます。
このページでは、メッシュを編集するためのツールの紹介に留めます。

- [MeshDeleterWithTexture](https://gatosyocora.booth.pm/items/1501527) by gatosyocora
- [Avatar Optimizer](https://vpm.anatawa12.com/avatar-optimizer/ja/) by anatawa12
- [Blender](https://www.blender.org/)

### テクスチャの調整

多くの場合、問題のある表情は半透明なメッシュとして実装されており、そのようなメッシュはアバターの顔の表面に重ねて表示されます。
そのため、透明な領域をアバターの肌色で塗りつぶすことで問題を抑制できます。

### サンプル

#### 青ざめ

ブレンドシェイプのアニメーションまたはメッシュを編集して青ざめを抑制します。

モデル:
- [I-s Ver.2.0](https://atelier-alca.booth.pm/items/2460693) by トクナガ

| PC版 | 変換後 | 調整後 |
|---|---|---|
| ![Is2_Darkness_PC](/img/Is2_Darkness_PC.png) | ![Is2_Darkness_Convert](/img/Is2_Darkness_Convert.png) | ![Is2_Darkness_Convert](/img/Is2_Darkness_Tweak.png) |

#### 眼鏡のレンズ

眼鏡のメッシュを編集してレンズを削除します。

モデル:
- [桔梗](https://ponderogen.booth.pm/items/3681787) by ぽんでろ
- [Slim Wing](https://wotapacchin.booth.pm/items/1460758) by をたぱち

| PC版 | 変換後 | 調整後 |
|---|---|---|
| ![Kikyo_Glasses_PC](/img/Kikyo_Glasses_PC.png) | ![Kikyo_Glasses_Convert](/img/Kikyo_Glasses_Convert.png) | ![Kikyo_Glasses_Tweak](/img/Kikyo_Glasses_Tweak.png) |

#### 目(角膜)

目のメッシュを編集して角膜を削除します。

モデル:
- [店員ちゃん](https://avatarchan.booth.pm/items/2704657) by コトブキヤ

| PC版 | 変換後 | 調整後 |
|---|---|---|
| ![DP001_Eyes_PC](/img/DP001_Eyes_PC.png) | ![DP001_Eyes_Convert](/img/DP001_Eyes_Convert.png) | ![DP001_Eyes_Tweak](/img/DP001_Eyes_Tweak.png) |

## ビルドサイズ

一般的に、テクスチャとメッシュがビルドサイズの主な要因です。
Androidアバターの10MB制限のため、テクスチャとメッシュのサイズを削減する必要がある場合があります。

### 不要なGameObjectの除外

**EditrOnly**タグが付いているGameObjectはビルドに含まれません。これは不要なメッシュやマテリアルをアバターから除外できることを意味します。
そのため不要なGameObjectに**EditorOnly**タグを付けることでビルドサイズを削減できる場合があります。

![EditorOnly_Tag](/img/EditorOnly_Tag.png)

### 不要なシェイプキーの削除

メッシュにシェイプキー(ブレンドシェイプ)があるとメッシュのデータサイズが大きくなり、その分だけ非圧縮サイズが大きくなります。

Avatar Optimizerの [`Trace and Optimize`](https://vpm.anatawa12.com/avatar-optimizer/ja/docs/reference/trace-and-optimize/) コンポーネントを使用することで、不要なシェイプキーを削除できます。

### テクスチャ圧縮設定の調整

多くの場合、テクスチャがビルドサイズの主な要因です。
テクスチャのサイズを削減するには、次の2つの方法があります。

- [テクスチャ解像度の削減](#テクスチャ解像度の削減)
- [テクスチャ圧縮設定の調整](#テクスチャ圧縮設定の調整)

#### テクスチャ解像度の削減

テクスチャの解像度を削減することは、テクスチャのサイズを削減する最も簡単で効果的な方法です。
インスペクタの**Max Size**を変更することでテクスチャの解像度を削減できます。

![Texture Max Size](/img/texture_max_size.png)

#### テクスチャ圧縮設定の調整

UnityはデフォルトでASTC圧縮に**ASTC 6x6 block**を使用します。
ASTCブロックサイズを変更することでテクスチャの品質を変更できます。

| ASTCブロックサイズ | テクスチャ品質 | サイズ |
|---|---|---|
| 4x4 | 高 | 大 |
| : | : | : |
| 6x6 | デフォルト | デフォルト |
| : | : | : |
| 12x12 | 低 | 小 |

インスペクタのプラットフォーム別オーバーライド設定でASTCブロックサイズを設定できます。
**Override for Android**のチェックボックスをオンにし、**Format**のドロップダウンメニューを変更します。

![Texture Override](/img/texture_override_android.png)
