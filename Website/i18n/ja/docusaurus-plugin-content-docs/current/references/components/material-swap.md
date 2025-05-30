# Material Swap

Mobileプラットフォームでマテリアルを置換します。

:::info
このコンポーネントには Non-Destructive Modular Framework (NDMF) が必要です。
:::

## 概要

- 元のマテリアルとMobileでの置換先となるマテリアルとのマッピングを定義することができます。
- 定義したマッピングはアバター全体に適用されます。
- Mobile向けにビルドするときにだけMaterial Swapは有効になります。

## 使い方

Create material mapping pairs by:
- Setting the Original Material (material used on PC)
- Setting the Replacement Material (optimized material for Android)

## プロパティ

### マテリアル置換設定

元のマテリアルと置換マテリアルのペアを定義します。

## NDMF

ターゲットプラットフォームがMobileの場合、マテリアルを置換します。

NDMF変換フェーズは[Avatar Converter Settings]コンポーネントまたはルートレベルのMaterial Conversion Settingsコンポーネントによって管理されます。

ルートレベルのコンポーネントがない場合、Autoが使用されます。

[Avatar Converter Settings]: avatar-converter-settings.md
