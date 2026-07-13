---
sidebar_position: 4
slug: /references/menus
---

# メニューリファレンス

VRCQuestTools の機能は、メニューバーの「Tools」→「VRCQuestTools」から使用できます。
一部の機能は、Hierarchy でアバターを右クリックして表示される「VRCQuestTools」メニューからも使用できます。

## Setup Avatar for Mobile

アバターを Mobile 向けに変換するためのウィンドウを開きます。
使い方は[アバターを変換する](./getting-started/convert-avatar.md)を参照してください。

## Remove Unsupported Components

選択したアバターから、Mobile で使用できないコンポーネント（Dynamic Bone, Cloth, Camera, Light, Audio Source, Rigidbody, Collider, Constraint など）を削除します。
削除する前に、対象のコンポーネントが一覧で表示されます。

## Remove Missing Components

選択したアバターから、"Missing" 状態のコンポーネント（スクリプトが見つからないコンポーネント）を削除します。
Dynamic Bone を含むアバターを Dynamic Bone のないプロジェクトで開いた場合などに使用します。

## Remove All Vertex Colors

選択したアバターのすべてのメッシュから頂点カラーを削除します。
アバターの各オブジェクトに [VQT Vertex Color Remover](./components/vertex-color-remover.md) コンポーネントが追加されます。

## Remove PhysBones

選択したアバターの PhysBone, PhysBone Collider, Contact を一覧表示し、チェックを入れたものを削除します。
Avatar Dynamics のパフォーマンスランクを確認しながら削減できます。

![Remove PhysBones ウィンドウ](/img/remove_physbones.png)

## BlendShapes Copy

ブレンドシェイプ（シェイプキー）の値を、別のメッシュへコピーするウィンドウを開きます。
コピー元とコピー先の Skinned Mesh Renderer を指定し、「ブレンドシェイプの値をコピー」を押します。
体型調整などのブレンドシェイプ値を、変換後のアバターへ移すときに使用します。

![BlendShapes Copy ウィンドウ](/img/blendshapes_copy.png)

Skinned Mesh Renderer コンポーネントの右クリックメニュー「Copy BlendShape Weights」からも使用できます。

## Metallic Smoothness Map

Metallic マップと Smoothness (または Roughness) マップから、Standard Lite シェーダー用の Metallic Smoothness マップを生成するウィンドウを開きます。

![Metallic Smoothness Map ウィンドウ](/img/metallic_smoothness_map.png)

## Unity Settings for Mobile

Mobile 向け作業のための Unity 推奨設定を確認するウィンドウを開きます。
Android Build Support と iOS Build Support の導入状況、Android テクスチャ圧縮の設定を確認できます。
詳細は[環境を準備する](./getting-started/set-up-environment.md#unity-settings)を参照してください。

## Clear Texture Cache

マテリアル変換で生成したテクスチャのキャッシュを削除します。
キャッシュはテクスチャの再生成を高速化するために使われており、削除しても次回の変換時に再生成されます。
ディスク容量を空けたいときや、生成されたテクスチャに問題があるときに使用します。

## Settings

VRCQuestTools の動作設定を切り替えます。

- **Enable Validation Automator**：シーン内のアバターを自動で検証し、問題があれば通知します。
- **[NDMF] Enable Texture Format Check on Windows Build**：Windows 向けビルド時に、Mobile で使用できないテクスチャ形式が含まれていないかを確認します。

## Languages

VRCQuestTools の表示言語を切り替えます。
Auto (default), English, 日本語, Русский から選べます。

## Check for Update

新しいバージョンがあるかを確認します。

## Help

このドキュメントサイトをブラウザーで開きます。

## Hierarchy の右クリックメニュー

アバターを右クリックして表示される「VRCQuestTools」メニューには、上記のほかに NDMF 用の項目があります。

- **[NDMF] Manual Bake with Mobile Settings**：Mobile 用の設定で変換したアバターの複製をシーンに作成します。
- **[NDMF] Build and Test for PC with Mobile Settings**：Mobile 用の設定で変換したアバターを、PC の VRChat でローカルテストします。実行には VRChat SDK のコントロールパネルを開いておく必要があります。
