---
sidebar_position: 6
slug: /migration-to-v3
---

# v2 系から v3.0.0 への移行ガイド

VRCQuestTools v3.0.0 はメジャーアップデートです。
アップデートによって、v2 系で作成したシーンやプレハブ、変換の手順の一部に影響が生じます。

Non-Destructive Modular Framework (NDMF) を使った変換はシーン上のアバターを書き換えないため、既存の設定はアップデート後もビルド時に同じように適用されます。
後述する必要バージョンを満たしていない場合や、削除された機能を使っている場合は、移行にあたり対応が必要になります。

## 動作要件の変更

v3.0.0 の動作に必要なバージョンは以下のとおりです。

- Unity 2022.3 以降
- VRChat SDK - Avatars 3.9.0 以降
- lilToon 1.10.0 以降 (使用している場合)
- NDMF 1.5.0 以降 (使用している場合)

lilToon と NDMF は、古いバージョンのままだとビルドや変換がエラーにより中断するため、アップデート前に VCC または ALCOM でバージョンを確認してください。

## VQT Avatar Builder ウィンドウの削除

**VQT Avatar Builder** ウィンドウを削除しました。
このウィンドウから行っていた操作は、次のいずれかに置き換わります。

- 通常のアップロードは、VRChat SDK の Control Panel からそのままビルドしてアップロードしてください。
- ローカルでの動作確認だけを行いたい場合は、アバターを右クリックして「VRCQuestTools」→「[NDMF] Build and Test for PC with Mobile Settings」を選んでください。
    Mobile 向け設定を適用した状態で PC 向けにビルドし、その場でテストできます。

## 動作が変わった機能

### Avatar Dynamics の保存先

**Avatar Dynamics Selector** で選んだ PhysBone/PhysBone Collider/Contacts の設定は、**Avatar Converter Settings** コンポーネント内ではなく、**Platform Component Remover** コンポーネントに保存されるようになりました。

この移行は自動では行われません。
v2 系で設定済みのアバターは、Avatar Dynamics Selector の「適用」ボタンを押さない限り、従来どおり Avatar Converter Settings 側の設定で動作し続けます。
そのため、アップデートしただけで設定が失われることはありません。
新しい保存先へ移行する場合だけ、「適用」ボタンを押してください。

### 頂点カラー除去のしくみ

変換時の頂点カラー除去は、**Vertex Color Remover** コンポーネントを使う方式から、変換後のアバター専用のメッシュ (`.vqtmesh` アセット) を生成する方式に変わりました。
v2 系では元のメッシュから直接頂点カラーを削除していたため、輪郭線の制御に頂点カラーを使うアバターでは、変換前のアバターの表示も変わってしまうことがありました。
v3.0.0 の変換では、この影響はありません。
Vertex Color Remover コンポーネント自体は引き続き使用でき、アタッチ済みのコンポーネントもそのまま動作します。

### 手動変換後のアバターのアクティブ状態

Avatar Converter Settings から手動変換したとき、変換前のアバターは非アクティブにならなくなりました。
両方のアバターがアクティブな状態でシーンに残る点に注意してください。

### 手動変換での Platform GameObject Remover と Platform Component Remover の適用

手動変換でも、**Platform GameObject Remover** と Platform Component Remover の設定が Mobile 向けに適用されるようになりました。
v2 系の手動変換ではこれらの設定は反映されなかったため、Mobile 向けの削除対象を指定しているアバターは変換結果が変わります。
なお、Avatar Dynamics の設定を Platform Component Remover に移行していないアバターでは、従来どおり Avatar Converter Settings 側の設定に基づく削除が行われます。

## 移行手順チェックリスト

1. プロジェクトの Unity、VRChat SDK、lilToon、NDMF のバージョンが、上記の必要バージョンを満たしているか確認する。
2. VCC または ALCOM で VRCQuestTools を v3.0.0 に更新する。
3. 必要であれば、Avatar Dynamics Selector で「適用」を押して Platform Component Remover への移行を済ませる。

v3.0.0 におけるすべての変更点は [変更履歴](./changelog.md) にまとめています。
