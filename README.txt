# VRCQuestTools

主にOculus Quest対応を想定したVRChat向けUnity Editor拡張です。

## 内容

### Convert Avatar for Quest

VRChatアバターをQuestから見えるように無理やり変換します(大抵はVery Poor)。
元のアバターに変更を加えないため、既存のプロジェクトでそのまま使用することができます。

既存ツール VRCAvatarQuestConverter (https://booth.pm/ja/items/2089584) の改良版です。

### Remove Missing Components

オブジェクトから"Missing"状態のコンポーネントを削除します。
Dynamic Boneを導入していないプロジェクトでアバターをアップロードできないときに使用します。

### Remove Unsupported Components

Dynamic BoneやClothなど、Quest用アバターで使用できないコンポーネントを削除します。

### Auto Remove Vertex Colors

シーン内のアバターのメッシュから頂点カラーを自動的に取り除き、一部アバターでVRChat/Mobile系シェーダーを使用する際に真っ黒になるなどテクスチャの色が正しく表示されない問題を対策します。

既存ツール VertexColorRemover (https://booth.pm/ja/items/1849557) とほぼ同様です。

### BlendShapes Copy

SkinnedMeshRendererに設定されたブレンドシェイプ(シェイプキー)の値を別のSkinnedMeshRendererにコピーします。
PC用とQuest用で別々のモデルを使用する場合などに、設定済みシェイプキーを移す際に使用します。

### Unity Settings for Quest

Quest対応に有用なUnityの設定を有効化します。

## 使用方法

unitypackageを導入後、メニューから「VRCQuestTools」を選択すると各機能を使用できます。
一部機能は自動的に有効になっています。

## 動作確認環境

- Windows 10 64-bit
- macOS Big Sur (Intel CPU)
- Ubuntu 20.04 LTS
- Unity 2018.4.20f1
- VRCSDK2 / VRCSDK3

## 利用規約

本ツールはMITライセンスで提供されます。

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

Twitter: https://twitter.com/kurotu

## 更新履歴

- Under development
    - 更新確認機能を追加
    - アバターの変換完了時にダイアログで通知するように変更
    - 変換済みアバターのデフォルトの保存先を Assets/KRT/QuestAvatars に変更
    - メニューの表示を調整
    - macOS, Linuxで「Quest用テクスチャを生成する」機能が動作するように修正
    - 「Remove Missing Components」でUnpack Prefabが不要な場合には実行しないように修正
- 2021/1/24: v0.2.1
    - プロジェクトを開いたときに「Auto Remove Vertex Colors」のチェックが反映されない問題を修正
- 2020/11/29: v0.2.0
    - 「Remove Missing Components」「Remove Unsupported Components」を追加
    - オブジェクトの右クリックメニューにVRCQuestToolsを追加
    - Quest用テクスチャを生成する際にテクスチャサイズを制限する機能を追加
    - メニューの実装を整理
- 2020/11/09: v0.1.2
    - MissingになっているDynamicBoneを含むアバターを変換するとUnityがクラッシュする問題を修正
- 2020/10/28: v0.1.1
    - RenderTextureを使用するマテリアルがあると変換が停止する問題を修正
    - メッセージの内容を一部変更
- 2020/10/10: v0.1.0
    - 公開
