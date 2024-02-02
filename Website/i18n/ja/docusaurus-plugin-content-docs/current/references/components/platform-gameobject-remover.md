# Platform GameObject Remover

ビルドプラットフォームに応じて、GameObjectを削除するかどうかを設定します。

:::info
このコンポーネントには Non-Destructive Modular Framework (NDMF) が必要です。
:::

## プロパティ

### ビルドターゲット

適用するビルドプラットフォームを選択します。
`Auto` の場合、Unityのターゲットプラットフォーム設定に応じて自動的に決定されます。

### PCでは削除

ビルドターゲットがPCの場合、このコンポーネントがアタッチされたGameObjectを削除します。

### Androidでは削除

ビルドターゲットがAndroidの場合、このコンポーネントがアタッチされたGameObjectを削除します。

## NDMF

VRCQuestToolsプラグインによって以下の処理を実行します。

### Resolving Phase

現在のビルドターゲットに応じて、このコンポーネントがアタッチされたGameObjectを削除します。
