# Platform Target Settings

[VQT Platform Component Remover]や[VQT Platform GameObject Remover]などの、ビルドターゲットに応じて処理の切り替わるコンポーネントのビルドターゲットを設定します。

:::info
このコンポーネントには Non-Destructive Modular Framework (NDMF) が必要です。
:::

## プロパティ

### ビルドターゲット

適用するビルドプラットフォームを選択します。
`Auto` の場合、Unityのターゲットプラットフォーム設定に応じて自動的に決定されます。

## NDMF

このコンポーネントは特定の処理を実行しません。
他のコンポーネントから参照されてビルドターゲットに依存する処理を変更します。

[VQT Platform Component Remover]: ./platform-component-remover
[VQT Platform GameObject Remover]: ./platform-gameobject-remover
