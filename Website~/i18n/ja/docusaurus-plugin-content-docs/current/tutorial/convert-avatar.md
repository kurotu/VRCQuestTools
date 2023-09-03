---
sidebar_position: 2
---

# アバターの変換

アバターをQuest対応に変換します。

## 必要な知識

- VRChat用の基本的なUnityの知識
- VRChatにアバターをアップロードする方法
- [アバターフォールバックシステム](https://docs.vrchat.com/docs/avatar-fallback-system)

## PCアバターのアップロード

アバターを変換する前に、PC向けのアバターをアップロードしておきます。

## アバターの変換

アバターをQuest対応に変換します。

1. ヒエラルキーからアバターを選択します。
2. アバターを右クリックして**VRCQuestTools** > **Convert Avatar for Quest** を選択します。
3. **Convert Avatar for Quest**ウィンドウが表示されます。
4. ウィンドウの**変換**ボタンをクリックします。
5. 変換が完了すると、同じシーンに変換後のアバターが作成されます。名前には`(Quest)`という接尾語が付きます。

:::info
変換後、元のアバターは非アクティブになります。インスペクターから元に戻すことができます。
:::

:::caution
VRCQuestToolsはアバターのパフォーマンスを最適化しません。
そのためほとんどの場合、変換後のアバターはQuestプラットフォームでパフォーマンスランクが「Very Poor」になります。

Questでは、セーフティのMinimum Displayed Performance Rankは「Good」または「Poor」になります。
これは「Very Poor」のアバターは常にブロックされ、他の人が個別にAvatar Display設定(「Show Avatar」機能として知られています)を変更しない限り、フォールバックアバターで置き換えられることを意味します。

適切なフォールバックアバターを設定する必要があります。
:::

## Avatar Dynamicsコンポーネントの削減

QuestアバターはAvatar Dynamics(PhysBone, Collider, Contact)に上限があります。アバターに多くのPhysBoneがある場合は、減らす必要があります。

https://docs.vrchat.com/docs/avatar-performance-ranking-system#quest-limits

- 8 PhysBone components
- 64 PhysBones affected transforms
- 16 PhysBones colliders
- 64 PhysBones collider checks
- 16 Avatar Dynamics Contacts

言い換えると、アバターのパフォーマンスランクが「Very Poor」であっても、Avatar Dynamicsカテゴリーでは「Poor」以内にする必要があります。

1. 変換後のアバターが制限を超えている場合、**PhysBones Remover**ウィンドウが表示されます。
2. 残したいPhysBonesなどのチェックボックスを選択します。
3. **選択していないコンポーネントを削除**ボタンをクリックします。

## プラットフォームをAndroidに変更

Questプラットフォームにアバターをアップロードするには、プラットフォームをAndroidに変更する必要があります。

1. メニューバーの**File** > **Build Settings**を選択します。
2. プラットフォームリストで**Android**を選択します。
3. **Switch Platform**ボタンをクリックします。
4. プラットフォームが変更されるまでしばらく待ちます。初回は時間がかかります。

:::note
Unity 2019以降、プラットフォームの切り替え結果はプロジェクトに保存されます。次回から長時間待つ必要はありません。
:::

:::tip
一部の状況では、[Unity Accelerator](https://docs.unity3d.com/Manual/UnityAccelerator.html)を使用してプラットフォームの切り替えを高速化できます。
詳細については、[Unityのドキュメント](https://docs.unity3d.com/Manual/UnityAccelerator.html)を参照してください。
:::

## Questアバターのアップロード

変換後のアバターをQuestプラットフォームにアップロードできるようになりました。元のアバターと変換後のアバターがPipeline Managerで同じBlueprint IDを使用していることを確認してください。

アップロード後、アバターのサムネイルにQuestアイコンがあることを確認してください。

:::info
アバターはBlueprint IDで管理されます。そのため、同じBlueprint IDで各プラットフォームのアバターデータをアップロードできます。
PCプレイヤーは元のアバターを、Questプレイヤーは変換後のアバターを見ることになります。
:::
