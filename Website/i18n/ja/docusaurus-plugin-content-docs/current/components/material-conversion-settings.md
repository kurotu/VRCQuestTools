---
slug: /references/components/material-conversion-settings
---

# Material Conversion Settings

Mobile プラットフォーム向けにマテリアルを変換するコンポーネントです。
アバター全体を変換せず、マテリアルだけを変換したい場合に使用します。
動作には NDMF が必要です。

このコンポーネントを追加した GameObject とその子オブジェクトのマテリアルが、Mobile ビルド時に変換されます。

![VQT Material Conversion Settings の Inspector](/img/material-conversion-settings.png)

## 設定項目

設定内容は [VQT Avatar Converter Settings](./avatar-converter-settings.md) のマテリアル変換設定と共通です。

| 項目 | 説明 |
|---|---|
| デフォルトのマテリアル変換設定 | マテリアルの変換方法です。初期設定は「Toon Lit」です。 |
| 追加のマテリアル変換設定 | 特定のマテリアルにデフォルトと異なる変換方法を指定します。 |
| 余分なマテリアルスロットを削除する | メッシュのサブメッシュ数より多いマテリアルスロットを削除します。 |
| NDMF変換フェーズ | NDMF のどのフェーズで変換するかを指定します。通常は Auto のままにします。 |
| マテリアルのプレビューを有効化 | 変換後のマテリアルを Scene ビューでプレビューします。 |

## 補足

- 「余分なマテリアルスロットを削除する」などの一部の設定は、アバターのルートオブジェクトに追加した場合にのみ機能します。
- アバターのルートオブジェクトに [VQT Avatar Converter Settings](./avatar-converter-settings.md) がある場合は、そちらの設定が優先されます。
