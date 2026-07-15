---
slug: /references/components/vertex-color-remover
---

# Vertex Color Remover

GameObject に紐づくメッシュから頂点カラーを削除するコンポーネントです。
一部の PC 用アバターは頂点カラーを持っており、そのままでは Mobile 用シェーダーでテクスチャの色が正しく表示されないことがあります。

他のコンポーネントと異なり、ビルド時ではなく、追加した時点でシーン上のメッシュから頂点カラーを削除します。

![VQT Vertex Color Remover の Inspector](/img/vertex-color-remover.png)

## 設定項目

| 項目 | 説明 |
|---|---|
| 子オブジェクトを含む | 子オブジェクトのメッシュからも頂点カラーを削除します。 |

「頂点カラーを復元」ボタンを押すと、メッシュを再インポートして頂点カラーを元に戻します。

## 補足

- [VQT Avatar Converter Settings](./avatar-converter-settings.md) の「メッシュから頂点カラーを削除」が有効な場合、変換時に頂点カラーが処理されるため、このコンポーネントを別途使う必要はありません。
- 頂点カラーを必要とする特別なシェーダーを PC 用アバターで使用している場合は、このコンポーネントを無効にしてください。
