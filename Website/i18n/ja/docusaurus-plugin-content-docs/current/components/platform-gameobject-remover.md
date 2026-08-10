---
slug: /references/components/platform-gameobject-remover
---

# Platform GameObject Remover

プラットフォームに応じて、GameObject をビルド時に削除するコンポーネントです。
削除したい GameObject に追加します。
ビルド時の削除には NDMF が必要です。
NDMF がない場合でも、[VQT Avatar Converter Settings](./avatar-converter-settings.md) の手動変換では、Mobile で削除する設定が変換後のアバターに適用されます。

PC 専用のアクセサリーを Mobile では丸ごと外す、といった使い方ができます。

![VQT Platform GameObject Remover の Inspector](/img/platform-gameobject-remover.png)

## 設定項目

| 項目 | 説明 |
|---|---|
| PCで維持 | チェックを入れると PC ビルドでこの GameObject を維持します。 |
| Mobileで維持 | チェックを入れると Mobile ビルドでこの GameObject を維持します。 |

チェックを外したプラットフォームのビルドでは、この GameObject が子オブジェクトごと削除されます。

NDMF のプレビューが有効な場合、削除される GameObject のメッシュは Scene ビューに表示されなくなります。
パーティクルなどメッシュ以外の描画はプレビューに反映されませんが、ビルド時には GameObject ごと削除されます。
