# Platform GameObject Remover

ビルドプラットフォームに応じて、GameObjectを削除するかどうかを設定します。
特定のプラットフォーム設定を強制するには、[VQT Platform Target Settings](./platform-target-settings)コンポーネントを使用してください。

:::info
このコンポーネントには Non-Destructive Modular Framework (NDMF) が必要です。
:::

## プロパティ

### PCでは削除

ビルドターゲットがPCの場合、このコンポーネントがアタッチされたGameObjectを削除します。

### Androidでは削除

ビルドターゲットがAndroidの場合、このコンポーネントがアタッチされたGameObjectを削除します。

## NDMF

VRCQuestToolsプラグインによって以下の処理を実行します。

### Resolving Phase

現在のビルドターゲットに応じて、このコンポーネントがアタッチされたGameObjectを削除します。
