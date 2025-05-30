# Material Conversion Settings

Mobileプラットフォーム向けのマテリアル変換設定を保持します。

:::info
このコンポーネントには Non-Destructive Modular Framework (NDMF) が必要です。
:::

インスペクターで無効になっている設定を使用するには、アバターのルートオブジェクトにコンポーネントを配置してください。
ただし、[Avatar Converter Settings]が優先されます。

## プロパティ

詳細は[Avatar Converter Settings]を参照してください。

### デフォルトのマテリアル変換設定
### 追加のマテリアル変換設定
### 余分なマテリアルスロットを削除
### NDMF変換フェーズ

## NDMF

ターゲットプラットフォームがMobileの場合、マテリアルを変換します。

NDMF変換フェーズは[Avatar Converter Settings]コンポーネントまたはルートレベルのMaterial Conversion Settingsコンポーネントによって管理されます。

ルートレベルのコンポーネントがない場合、Autoが使用されます。

[Avatar Converter Settings]: avatar-converter-settings.md
