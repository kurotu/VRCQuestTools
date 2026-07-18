---
slug: /references/components
---

# コンポーネント一覧

VRCQuestTools は、アバターに追加して使う Unity コンポーネントを提供します。
Inspector の「Add Component」ボタンから「VRCQuestTools」カテゴリを選ぶと追加できます。

どのコンポーネントも Unity エディター上でのみ動作します。
アップロードされるアバターには含まれないため、アバターのパフォーマンスには影響しません。

## コンポーネントの種類

| コンポーネント | 役割 |
|---|---|
| [VQT Avatar Converter Settings](./avatar-converter-settings.md) | アバターを Mobile 向けに変換するための設定 |
| [VQT Converted Avatar](./converted-avatar.md) | 変換済みアバターを示すマーカー |
| [VQT Fallback Avatar](./fallback-avatar.md) | アップロード時にフォールバックアバターとして設定 |
| [VQT Material Conversion Settings](./material-conversion-settings.md) | マテリアル変換の設定（アバター変換とは独立して使用） |
| [VQT Material Swap](./material-swap.md) | Mobile ビルド時にマテリアルを別のマテリアルへ置き換え |
| [VQT Menu Icon Resizer](./menu-icon-resizer.md) | Expressions Menu のアイコンをリサイズ |
| [VQT Mesh Flipper](./mesh-flipper.md) | メッシュを両面化、または面を反転 |
| [VQT Network ID Assigner](./network-id-assigner.md) | PhysBone などにネットワーク ID を割り当て |
| [VQT Platform Component Remover](./platform-component-remover.md) | プラットフォームごとにコンポーネントを削除 |
| [VQT Platform GameObject Remover](./platform-gameobject-remover.md) | プラットフォームごとに GameObject を削除 |
| [VQT Platform Target Settings](./platform-target-settings.md) | ビルド時のプラットフォーム判定を固定 |
| [VQT Vertex Color Remover](./vertex-color-remover.md) | メッシュの頂点カラーを削除 |

## NDMF が必要なコンポーネント

次のコンポーネントは、ビルド時の処理に Non-Destructive Modular Framework (NDMF) を使用します。

- VQT Material Conversion Settings
- VQT Material Swap
- VQT Menu Icon Resizer
- VQT Mesh Flipper
- VQT Platform Component Remover
- VQT Platform GameObject Remover
- VQT Platform Target Settings

プロジェクトに NDMF がない場合、これらのコンポーネントは動作せず、Inspector に警告が表示されます。
NDMF は、[Modular Avatar](https://modular-avatar.nadena.dev/ja) のトップページにある「ダウンロード」ボタンでリポジトリを追加してインストールできます。

## ビルドターゲットについて

「PC で維持」「Mobile で有効」のような設定を持つコンポーネントは、ビルド時のターゲットプラットフォームを見て動作を切り替えます。
通常は Unity のターゲットプラットフォーム（Windows なら PC、Android や iOS なら Mobile）がそのまま使われます。
[VQT Platform Target Settings](./platform-target-settings.md) をアバターに追加すると、この判定を固定できます。
