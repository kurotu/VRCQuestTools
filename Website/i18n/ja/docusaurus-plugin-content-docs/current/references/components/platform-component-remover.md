# Platform Component Remover

ビルドプラットフォームに応じて、コンポーネントを削除するかどうかを設定します。
特定のプラットフォーム設定を強制するには、[VQT Platform Target Settings](./platform-target-settings)コンポーネントを使用してください。

:::info
このコンポーネントには Non-Destructive Modular Framework (NDMF) が必要です。
:::

## プロパティ

### コンポーネント設定

プラットフォームのチェックボックスをオンにすると、そのコンポーネントは選択したプラットフォームで削除されます。

## NDMF

VRCQuestToolsプラグインによって以下の処理を実行します。

### Resolving Phase

コンポーネント設定で現在のビルドターゲットに対応するチェックボックスがオンになっているコンポーネントを削除します。
