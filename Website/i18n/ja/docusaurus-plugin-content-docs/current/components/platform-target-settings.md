---
slug: /references/components/platform-target-settings
---

# Platform Target Settings

ビルド時のプラットフォーム判定を特定のプラットフォームに固定するコンポーネントです。
アバターのルートオブジェクトに追加します。
動作には NDMF が必要です。

「PCで維持」「Mobileで有効」のような設定を持つ VRCQuestTools のコンポーネントは、通常 Unity のターゲットプラットフォームを見て動作を切り替えます。
このコンポーネントを追加すると、Unity のターゲットプラットフォームに関係なく、指定したプラットフォームの設定でビルドできます。

:::info スクリーンショット準備中
ここに VQT Platform Target Settings の Inspector のスクリーンショットが入ります。
:::

## 設定項目

| 項目 | 説明 |
|---|---|
| ビルドターゲット | ビルド時のプラットフォームを指定します。Auto の場合、Unity のターゲットプラットフォームと同じになります。 |

## 補足

PC 用のビルドで Mobile 向けの動作を確認したいときに便利です。
たとえば「ビルドターゲット」を Android にすると、Unity が Windows ターゲットのままでも Mobile 用の設定でビルドされます。
