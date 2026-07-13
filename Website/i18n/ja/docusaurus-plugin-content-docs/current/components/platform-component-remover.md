---
slug: /references/components/platform-component-remover
---

# Platform Component Remover

プラットフォームに応じて、同じ GameObject にあるコンポーネントをビルド時に削除するコンポーネントです。
削除したいコンポーネントのある GameObject に追加します。
動作には NDMF が必要です。

PC では使いたいが Mobile では削除したいコンポーネント（またはその逆）があるときに使用します。
たとえば、Mobile 用にアップロードするときだけ特定の PhysBone を削除する、といった使い方ができます。

:::info スクリーンショット準備中
ここに VQT Platform Component Remover の Inspector のスクリーンショットが入ります。
:::

## 設定項目

「コンポーネント維持設定」に、同じ GameObject にあるコンポーネントの一覧が表示されます。

| 項目 | 説明 |
|---|---|
| PC | チェックを入れたコンポーネントを PC ビルドで維持します。 |
| Mobile | チェックを入れたコンポーネントを Mobile ビルドで維持します。 |

チェックを外したプラットフォームのビルドでは、そのコンポーネントが削除されます。

## 補足

[VQT Avatar Converter Settings](./avatar-converter-settings.md) の「Avatar Dynamics 設定」で残すコンポーネントを選択すると、対象の GameObject にこのコンポーネントが自動で設定されます。
