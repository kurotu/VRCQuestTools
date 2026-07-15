---
sidebar_position: 2
---

# アバターを変換する

アバターに変換用の設定を追加し、Mobile 向けにアップロードできる状態にします。

:::caution 先に PC 版をアップロードしてください
PC と Mobile で PhysBone の揺れ方を同期させるには、両方のプラットフォームに同じアバターをアップロードする必要があります。
変換前のアバターを PC 向けにアップロードしていない場合は、先にアップロードを済ませてください。
:::

## 変換の設定を始める

1. メニューバーの「Tools」→「VRCQuestTools」→「Setup Avatar for Mobile」を選択します。
2. 表示されたウィンドウでアバターを選択します。
3. 「変換の設定を始める」ボタンを押します。

![Setup Avatar for Mobile ウィンドウ](/img/convert-avatar.png)

アバターに [VQT Avatar Converter Settings](../components/avatar-converter-settings.md) コンポーネントが追加され、変換の設定画面が表示されます。
Modular Avatar や Avatar Optimizer がプロジェクトにある場合は、あわせて追加するコンポーネントをボタンの上のチェックボックスで選べます。

設定はデフォルトのままで変換できます。
マテリアルごとに変換方法を変えたい場合や、テクスチャの解像度を調整したい場合は、[VQT Avatar Converter Settings](../components/avatar-converter-settings.md) の設定項目を参照してください。

## 警告が表示された場合

アバターの内容によっては、設定画面に警告が表示されます。
代表的なものは次のとおりです。

- **非対応コンポーネントの削除**：Mobile で使用できないコンポーネント（Constraint など）は変換時に削除されます。変換後にアバターの機能へ支障がないか確認してください。
- **Dynamic Bone**：VRCQuestTools は Dynamic Bone を PhysBone に変換しません。先に VRChat SDK の機能で PhysBone へ変換しておいてください。
- **Avatar Dynamics のパフォーマンス**：PhysBone や Contact が多すぎると、アップロードできてもコンポーネントが削除されます。[PhysBone を削減する](../troubleshooting.md#avatar-dynamics)必要があります。

## アップロードする

ここからの手順は、プロジェクトに NDMF があるかどうかで変わります。

### NDMF がある場合（推奨）

アバターはアップロード時に自動で変換されます。
元のアバターを変更しないため、PC 用と Mobile 用でシーンやアバターを分ける必要がありません。

1. VRChat SDK のコントロールパネルを開き、「Builder」タブを選択します。
2. 「Selected Platform(s)」でプラットフォームを Android に切り替えます。
3. そのまま「Build & Publish」でアップロードします。

:::info スクリーンショット準備中
ここに VRChat SDK コントロールパネルでプラットフォームを切り替えるスクリーンショットが入ります。
:::

iOS 版の VRChat 向けにもアップロードする場合は、「Selected Platform(s)」で iOS も選択します。
プラットフォームを iOS に切り替えるには、[iOS Build Support](./set-up-environment.md#ios-build-support) が必要です。

### NDMF がない場合

「手動変換」でアバターの複製を作成してからアップロードします。

1. 変換の設定画面の「変換する」ボタンを押します。
2. 変換が終わると、シーンに「(アバター名) (Android)」という名前の複製が作成されます。元のアバターは自動的に非アクティブになります。
3. VRChat SDK のコントロールパネルでプラットフォームを Android (iOS 向けの場合は iOS) に切り替えます。
4. 複製されたアバターを、元のアバターと同じ Blueprint ID でアップロードします。

変換で生成されたマテリアルやテクスチャは、設定画面の「保存先フォルダ」に表示されたフォルダーへ保存されます。

## 次のステップ

アップロードの前後で、[変換後のアバターを確認する](./check-avatar.md)ことをお勧めします。
