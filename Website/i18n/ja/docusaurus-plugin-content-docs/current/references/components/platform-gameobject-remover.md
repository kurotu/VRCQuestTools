# Platform GameObject Remover

ビルドプラットフォームに応じて、GameObjectを削除するかどうかを設定します。
特定のプラットフォーム設定を強制するには、[VQT Platform Target Settings](platform-target-settings.md)コンポーネントを使用してください。

:::info
このコンポーネントには Non-Destructive Modular Framework (NDMF) が必要です。
:::

## プロパティ

### PCでは維持

ビルドターゲットがPCの場合、このコンポーネントがアタッチされたGameObjectを削除しません。

### Androidでは維持

ビルドターゲットがAndroidの場合、このコンポーネントがアタッチされたGameObjectを削除しません。

## NDMF

VRCQuestToolsプラグインによって以下の処理を実行します。

### Resolving Phase

現在のビルドターゲットに応じて、このコンポーネントがアタッチされたGameObjectを削除します。
