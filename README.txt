# VRCQuestTools

主にOculus Quest対応を想定したVRChat向けUnity Editor拡張の詰め合わせです。

## 内容

### VRCAvatarQuestConverter

VRChatアバターをQuestから見えるように無理やり変換します(大抵はVery Poor)。
元のアバターに変更を加えないため、既存のプロジェクトでそのまま使用することができます。

既存ツール VRCAvatarQuestConverter (https://booth.pm/ja/items/2089584) の改良版です。

### VertexColorRemover

シーン内のアバターのメッシュから頂点カラーを自動的に取り除き、一部アバターでVRChat/Mobile系シェーダーを使用する際に真っ黒になるなどテクスチャの色が正しく表示されない問題を対策します。

既存ツール VertexColorRemover (https://booth.pm/ja/items/1849557) とほぼ同様です。

### BlendShapesCopy

SkinnedMeshRendererに設定されたブレンドシェイプ(シェイプキー)の値を別のSkinnedMeshRendererにコピーします。
PC用とQuest用で別々のモデルを使用する場合などに、設定済みシェイプキーを移す際に使用します。

### UnityQuestSettings

Quest対応に有用なUnityの設定を有効化します。

## 使用方法

unitypackageを導入後、メニューから「VRCQuestTools」を選択すると各機能を使用できます。
一部機能は自動的に有効になっています。

## 動作環境

- Windows 10 64-bit
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

- 2020/11/09: v0.1.2
    - MissingになっているDynamicBoneを含むアバターを変換するとUnityがクラッシュする問題を修正
- 2020/10/28: v0.1.1
    - RenderTextureを使用するマテリアルがあると変換が停止する問題を修正
    - メッセージの内容を一部変更
- 2020/10/10: v0.1.0
    - 公開
