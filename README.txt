# VRCQuestTools

VRChat アバターの Oculus Quest 対応を想定した Unity Editor 拡張です。

## 内容

### Convert Avatar for Quest

選択したアバターに以下の操作を自動的に行い、Quest 用にアップロードできるように変換します (大抵は Very Poor)。

- アバターとマテリアルの複製
- シェーダーを VRChat/Mobile/Toon Lit に変更
- 元々のマテリアルの Color, Emission を反映したテクスチャの生成
- DynamicBone や Cloth などの使用不可コンポーネントの削除
- (Avatars 3.0 のみ) マテリアル変更アニメーションがある場合に Animator Controller とアニメーションを複製・変換

コピーを作成することで元のアバターに変更を加えないため、既存のプロジェクトでそのまま使用することができます。

### Remove Missing Components

オブジェクトから "Missing" 状態のコンポーネントを削除します。
DynamicBone を導入していないプロジェクトでアバターをアップロードできないときに使用します。

### Tools/Remove Unsupported Components

DynamicBone や Cloth など、Quest 用アバターで使用できないコンポーネントを削除します。

### Tools/BlendShapes Copy

SkinnedMeshRenderer に設定されたブレンドシェイプ(シェイプキー)の値を別の SkinnedMeshRenderer にコピーします。
PC 用と Quest 用で別々のモデルを使用する場合などに、設定済みシェイプキーを移す際に使用します。

### Tools/Metallic Smoothness Map

Metallic マップや Smoothness/Roughness マップから Metallic Smoothness マップを生成します。
生成したテクスチャは VRChat/Mobile/Standard Lite シェーダーで使用できます。

### Auto Remove Vertex Colors

シーン内のアバターのメッシュから頂点カラーを自動的に取り除き、一部アバターで VRChat/Mobile 系シェーダーを使用する際に真っ黒になるなどテクスチャの色が正しく表示されない問題を対策します。

### Unity Settings for Quest

Quest 対応に有用な Unity の設定を有効化します。

## 使用方法

unitypackage を導入後、ヒエラルキーでアバターを選択した状態でメニューから「VRCQuestTools」を選択すると各機能を使用できます。
一部機能は自動的に有効になっています。

## 動作確認環境

- Windows 10 64-bit
- macOS Big Sur (Intel CPU)
- Ubuntu 20.04 LTS
- Unity 2019.4.29f1
- VRCSDK2 / VRCSDK3

## 利用規約

本ツールは MIT ライセンスで提供されます。

```
MIT License

Copyright (c) 2020 kurotu

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## 連絡先

VRCID: kurotu
Twitter: https://twitter.com/kurotu
GitHub: https://github.com/kurotu/VRCQuestTools

## 更新履歴

- 2021/8/5: v0.6.0
    - Unity 2019 では Local Cache Server が不要なため推奨設定から削除
    - (macOS) 内部プラグインを Unity 2019 用にリネーム
- 2021/8/4: v0.5.2
    - Resources/unity_builtin_extra を使用するマテリアルの変換に失敗する問題を修正
    - Standard シェーダーが動作未確認のシェーダーとして警告される問題を修正
    - 「Quest用テクスチャのみ更新」にエラーダイアログを追加
- 2021/6/30: v0.5.1
    - VRCSDK2のプロジェクトでコンパイルエラーになる問題を修正
- 2021/6/27: v0.5.0
    - (Avatars 3.0) マテリアル変更アニメーションがある場合に Animator Controller とアニメーションを変換する機能を追加 (Thanks zin3)
    - 表示言語の選択機能を追加 (日/英)
    - 「Remove Missing Components」「Remove Unsupported Components」の実行を確認するダイアログを追加
    - 動作未確認のシェーダーに対してのテクスチャ生成について警告を追加
    - 空のマテリアルスロットがある場合にテクスチャのみの更新に失敗する問題を修正
    - UI の細かな改善
- 2021/5/23: v0.4.1
    - マテリアル変換時のエラーをより詳細に表示するよう変更
    - 「Quest用テクスチャを更新」ボタンの配置を変更
- 2021/4/1: v0.4.0
    - 「Metallic Smoothness Map」を追加
    - 変換済みアバターのテクスチャのみを更新する機能を追加
    - 変換後のアバターで削除されるコンポーネントについて警告を追加
    - 変換後のマテリアルから不要なプロパティを削除するように変更
    - 変換後のマテリアルでは GPU インスタンシングを有効にするように変更
- 2021/2/6: v0.3.0
    - 更新確認機能を追加
    - macOS, Linux で「Quest用のテクスチャを生成する」機能が動作するように変更
    - アバターの変換完了時にダイアログで通知するように変更
    - 変換済みアバターのデフォルトの保存先を Assets/KRT/QuestAvatars に変更
    - 生成したマテリアルとテクスチャをそれぞれ Materials, Textures フォルダに保存するように変更
    - 「Remove Missing Components」で Unpack Prefab が不要な場合には実行しないように変更
    - メニューの表示順を調整
- 2021/1/24: v0.2.1
    - プロジェクトを開いたときに「Auto Remove Vertex Colors」のチェックが反映されない問題を修正
- 2020/11/29: v0.2.0
    - 「Remove Missing Components」「Remove Unsupported Components」を追加
    - オブジェクトの右クリックメニューに VRCQuestTools を追加
    - Quest 用テクスチャを生成する際にテクスチャサイズを制限する機能を追加
    - メニューの実装を整理
- 2020/11/09: v0.1.2
    - Missing になっている DynamicBone を含むアバターを変換すると Unity がクラッシュする問題を修正
- 2020/10/28: v0.1.1
    - RenderTexture を使用するマテリアルがあると変換が停止する問題を修正
    - メッセージの内容を一部変更
- 2020/10/10: v0.1.0
    - 公開
