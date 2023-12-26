---
sidebar_position: 2
---

# Converted Avatar

変換後のアバターにアタッチされるコンポーネントです。
VRCQuestToolsで変換されたアバターであることを示します。

## NDMF

Non-Destructive Modular Framework (NDMF) が導入されているプロジェクトではVRCQuestToolsプラグインによって以下の処理を実行します。

### Optimizing Phase

Modualr Avatarなどの非破壊ツールによってビルド中に生成された使用不可コンポーネントを削除します。
この処理はAnatawa12's Avatar Optimizerによる最適化の前に実行されます。
