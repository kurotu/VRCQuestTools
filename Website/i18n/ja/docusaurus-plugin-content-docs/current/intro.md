---
sidebar_position: 1
slug: /intro
---

# VRCQuestTools とは

VRCQuestTools は、PC 用の VRChat アバターを Android (Quest, PICO) や iOS でも使えるように変換する Unity エディター拡張です。
このドキュメントでは、Android 版と iOS 版の VRChat をまとめて **Mobile** と呼びます。

PC 用アバターをそのまま Mobile 向けにアップロードしようとすると、シェーダーや一部のコンポーネントが対応していないため、VRChat SDK のエラーで止まってしまいます。
VRCQuestTools は、この対応作業の大部分を自動化します。

## 主な機能

- **マテリアルの変換**：PC 用シェーダーの設定を読み取り、Mobile 対応シェーダー用のマテリアルとテクスチャを自動生成します。
- **非対応コンポーネントの削除**：Mobile で使用できないコンポーネントをビルド時に削除します。
- **非破壊変換**：Non-Destructive Modular Framework (NDMF) がプロジェクトにある場合、アップロード時に自動で変換します。元のアバターは変更されません。
- **PhysBone の削減支援**：パフォーマンス制限に収めるための PhysBone 削減を支援します。
- **各種ユーティリティ**：ブレンドシェイプのコピーや Unity の推奨設定の適用など、Mobile 対応を助けるツールを備えています。

## 動作環境

- VRChat Creator Companion (VCC) で作成したアバタープロジェクト
- VRChat SDK - Avatars 3.9.0 以降

## このドキュメントの読み方

初めて使う場合は、[はじめに](./getting-started/set-up-environment.md)の手順に沿って進めてください。
インストールからアップロードまでを順番に説明しています。

各コンポーネントの設定項目を調べたい場合は、[コンポーネントリファレンス](./components/index.md)を参照してください。

問題が起きた場合は、[トラブルシューティング](./troubleshooting.md)を確認してください。
