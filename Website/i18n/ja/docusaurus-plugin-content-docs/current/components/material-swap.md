---
slug: /references/components/material-swap
---

# Material Swap

Mobile プラットフォームのビルド時に、マテリアルを別のマテリアルへ置き換えるコンポーネントです。
Mobile 用のマテリアルを自分で用意している場合に使用します。
動作には NDMF が必要です。

このコンポーネントを追加した GameObject とその子オブジェクトのレンダラーが置き換えの対象になります。

![VQT Material Swap の Inspector](/img/material-swap.png)

## 設定項目

| 項目 | 説明 |
|---|---|
| 元のマテリアル | 置き換える元のマテリアルです。「子オブジェクトから選択」ボタンで対象から選べます。 |
| 置換マテリアル | 置き換え先のマテリアルです。Mobile アバターで許可されたシェーダーを使用している必要があります。 |

「マテリアル置換設定」のリストに、置き換えたいマテリアルの組を追加します。

## 補足

- 置換マテリアルのシェーダーが Mobile アバターで許可されていない場合、Inspector にエラーが表示されます。
- マテリアル単位で変換方法を変えたいだけであれば、[VQT Avatar Converter Settings](./avatar-converter-settings.md) の「追加のマテリアル変換設定」で「マテリアル置換」を選ぶ方法もあります。
