---
slug: /references/components/platform-gameobject-remover
---

# Platform GameObject Remover

プラットフォームに応じて、GameObject をビルド時に削除するコンポーネントです。
削除したい GameObject に追加します。
動作には NDMF が必要です。

PC 専用のアクセサリーを Mobile では丸ごと外す、といった使い方ができます。

![VQT Platform GameObject Remover の Inspector](/img/platform-gameobject-remover.png)

## 設定項目

| 項目 | 説明 |
|---|---|
| PCで維持 | チェックを入れると PC ビルドでこの GameObject を維持します。 |
| Mobileで維持 | チェックを入れると Mobile ビルドでこの GameObject を維持します。 |

チェックを外したプラットフォームのビルドでは、この GameObject が子オブジェクトごと削除されます。
