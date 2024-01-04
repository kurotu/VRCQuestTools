---
sidebar_position: 2
---

# アバターの変換

アバターをAndroid対応に変換します。

まず始めにPC用アバターをアップロードした後、アバターを変換してAndroid用にアップロードします。

:::info
VRChatではアバターはBlueprint IDで管理されており、同じBlueprint IDに対してプラットフォーム毎にアバターデータをアップロードできます。
そのためPCユーザーは元のアバターを、Androidユーザーは変換後のアバターを見ることになります。
:::

:::caution
VRoid Studioで作成されたアバターは透過表現を多用しており、Android用のシェーダーでは正しく表示できません。
VRCQuestToolsだけでは対応できず、別途作業が必要です。
:::

## 必要な知識

- VRChat用の基本的なUnityの知識
- VRChatにアバターをアップロードする方法

## PCアバターのアップロード

アバターを変換する前に、PC向けのアバターをアップロードしておきます。
ただし通常のPC向けのアップロードの前に、追加の作業をします。

### PhysBoneへのネットワークIDの割り当て

PCとAndroidでPhysBoneを正しく同期させるために、PhysBoneにネットワークIDを割り当てる必要があります。

1. メニューバーの**VRChat SDK** > **Utilities** > **Network ID Import and Export Utility**を選択します。
2. **Network ID Utility**ウィンドウが表示されます。
3. **Target**ドロップダウンからアバターを選択します。
4. **Regenerate Scene IDs**ボタンをクリックします。
5. **Generate New Scene IDs**ダイアログの**Generate New IDs**ボタンをクリックします。
6. PhysBoneの一覧が表示されます。

### アバターのアップロード

通常通りアバターをPC用にアップロードします。

## アバターの変換設定

アバターをAndroid対応に変換するための設定をします。

1. ヒエラルキーからアバターを選択します。
2. アバターを右クリックして**VRCQuestTools** > **Convert Avatar for Quest** を選択します。
3. **Convert Avatar for Quest**ウィンドウが表示されます。
4. ウィンドウの**変換の設定を始める**ボタンをクリックします。
5. アバターに**VQT Avatar Converter Settings**コンポーネントが追加され、変換の設定が表示されます。

以下の項目の設定をします。

### Avatar Dynamicsコンポーネントの削減

AndroidアバターはAvatar Dynamics(PhysBone, Collider, Contact)に上限があります。アバターに多くのPhysBoneがある場合は、減らす必要があります。

https://docs.vrchat.com/docs/avatar-performance-ranking-system#quest-limits

- 8 PhysBone components
- 64 PhysBones affected transforms
- 16 PhysBones colliders
- 64 PhysBones collider checks
- 16 Avatar Dynamics Contacts

言い換えると、アバターのパフォーマンスランクが「Very Poor」であっても、Avatar Dynamicsカテゴリーでは「Poor」以内にする必要があります。

1. 設定項目から**Avatar Dynamics 設定**を展開し、**Avatar Dynamics 設定**ボタンをクリックします。
2. **Avatar Dynamics Selector**ウィンドウが表示されます。
3. 残したいPhysBoneなどのチェックボックスを選択し、推定パフォーマンスランクをPoor以内にします。
4. **適用**ボタンをクリックします。

## アバターの変換

:::tip
Prefabワークフローを使用している場合、このタイミングでPrefab (またはPrefab Variant)を作成するとよいでしょう。
VRCQuestToolsはアバターの変換後もプレハブへの参照を保持します。
また、同じ変換設定を流用することができます。
:::

1. 設定項目を一番下までスクロールし、**変換**ボタンをクリックします。
2. 変換が完了すると、同じシーンに変換後のアバターが作成されます。
    - 名前には` (Quest)`という接尾語が付きます。
    - 生成されたアセットは`Assets/VRCQuestToolsOutput/<アバターのオブジェクト名>`に保存されます。

:::note
変換後、元のアバターは非アクティブになります。インスペクターから元に戻すことができます。
:::

:::info
VRCQuestToolsはアバターのパフォーマンスを最適化しません。
そのためほとんどの場合、変換後のアバターはAndroidプラットフォームでパフォーマンスランクが「Very Poor」になります。

Androidでは、セーフティのMinimum Displayed Performance Rankは「Good」または「Poor」になります。
これは「Very Poor」のアバターは常にブロックされ、他の人が個別にAvatar Display設定(「Show Avatar」機能として知られています)を変更しない限り、フォールバックアバターまたはインポスターで置き換えられることを意味します。

また、スマートフォン版VRChatではVery Poorのアバターは表示されません。

参照:
- [Quest Limits - Performance Ranks](https://creators.vrchat.com/avatars/avatar-performance-ranking-system/#quest-limits)
- [Avatar Fallback System](https://docs.vrchat.com/docs/avatar-fallback-system)
- [Impostors](https://creators.vrchat.com/avatars/avatar-impostors)
:::


## プラットフォームをAndroidに変更

Androidプラットフォームにアバターをアップロードするには、UnityのターゲットプラットフォームをAndroidに変更する必要があります。

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

## Androidアバターのアップロード

変換後のアバターをAndroidプラットフォームにアップロードできるようになりました。元のアバターと変換後のアバターがPipeline Managerで同じBlueprint IDを使用していることを確認してください。

アップロード後、アバターのサムネイルに緑色のMobileアイコンがあることを確認してください。
