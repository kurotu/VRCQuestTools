---
sidebar_position: 3
---

# 変換したアバターのテスト

アバターはクロスプラットフォーム対応になりましたが、完ぺきではないかもしれません。テストしてみましょう。

## PCでQuestアバターをテストする

完全なテストにはQuestが必要です。QuestがあってもPCとQuestの間を行き来する必要があり大変です。
しかしほとんどの場合、PCでQuestアバターをテストすることができます。なぜなら、PCプラットフォームで完全にサポートされているシェーダーやコンポーネントを使用しているからです。

VRCSDK3では、[Local Avatar Testing](https://docs.vrchat.com/docs/avatars-30#local-avatar-testing)という機能があります。これにより、アップロードせずにPCでアバターをテストできます。
ローカルテストを使用することで、PCで素早くQuestアバターをテストできます。

## オフラインアバターのビルド

オフラインテスト用のアバターをビルドするには、以下の手順に従ってください。

1. メニューバーから**File** > **Build Settings**を選択します。
2. プラットフォームリストから**PC, Mac & Linux Standalone**を選択します。
3. **Switch Platform**をクリックします。
4. メニューバーから**VRChat SDK** > **Show Control Panel**を選択します。
5. **Builder**タブを選択します。
6. シーンからテストするアバターを選択します。
7. **Build & Test**をクリックします。

ビルドが完了したら、PCでアバターをテストできます。

## VRChatでのオフラインアバターのテスト

VRChatでオフラインテスト用のアバターを使用するには、以下の手順に従ってください。

1. PCでVRChatを起動します。
2. メインメニューを開き、**Avatars**タブを表示します。
3. **Other**セクションを選択します。
4. `SDK: <Unityのシーン内での名前>`のアバターを選択します。

![Other Category](/img/other_avatars.png)

これで、変換したアバターをVRChatで確認することができます。
一般的には、以下の項目をテストする必要があります。
- 表情アニメーション
- FXレイヤーのギミック
- 透明・半透明の表現

デスクトップモードでテストする場合は、以下の表を参照してキーボードでアバターを操作します。

| ハンドジェスチャー | 左手 | 右手 |
|---|---|---|
| Idle | <kbd>左Shift</kbd> + <kbd>F1</kbd> | <kbd>右Shift</kbd> + <kbd>F1</kbd> |
| Fist | <kbd>左Shift</kbd> + <kbd>F2</kbd> | <kbd>右Shift</kbd> + <kbd>F2</kbd> |
| Open | <kbd>左Shift</kbd> + <kbd>F3</kbd> | <kbd>右Shift</kbd> + <kbd>F3</kbd> |
| Point | <kbd>左Shift</kbd> + <kbd>F4</kbd> | <kbd>右Shift</kbd> + <kbd>F4</kbd> |
| Peace | <kbd>左Shift</kbd> + <kbd>F5</kbd> | <kbd>右Shift</kbd> + <kbd>F5</kbd> |
| RockNRoll | <kbd>左Shift</kbd> + <kbd>F6</kbd> | <kbd>右Shift</kbd> + <kbd>F6</kbd> |
| Gun | <kbd>左Shift</kbd> + <kbd>F7</kbd> | <kbd>右Shift</kbd> + <kbd>F7</kbd> |
| Thumbs Up | <kbd>左Shift</kbd> + <kbd>F8</kbd> | <kbd>右Shift</kbd> + <kbd>F8</kbd> |

| アクション | キー |
|---|---|
| アクションメニュー | <kbd>R</kbd> |
| ジャンプ | <kbd>Space</kbd> |
| しゃがみ | <kbd>C</kbd> |
| うつ伏せ | <kbd>Z</kbd> |
