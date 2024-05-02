# Platform Component Remover

ビルドプラットフォームに応じて、コンポーネントを削除するかどうかを設定します。
特定のプラットフォーム設定を強制するには、[VQT Platform Target Settings](./platform-target-settings)コンポーネントを使用してください。

:::info
このコンポーネントには Non-Destructive Modular Framework (NDMF) が必要です。
:::

## プロパティ

### コンポーネント設定

プラットフォームのチェックボックスをオフにすると、オフにしたプラットフォームでビルドするときにコンポーネントが削除されます。

下の例では、PC向けにビルドするときにRemoveMeshByBlendShapeコンポーネントが削除されます。

![Platform Component Remover](/img/platform-component-remover.png)

## NDMF

VRCQuestToolsプラグインによって以下の処理を実行します。

### Resolving Phase

コンポーネント設定にしたがってコンポーネントを削除します。
