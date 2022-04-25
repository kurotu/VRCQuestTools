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
- VRCQuestTools Extra https://kurotu.booth.pm/items/3375621 を導入していると、対応しているPC専用アセット向けの追加処理を実施

コピーを作成することで元のアバターに変更を加えないため、既存のプロジェクトでそのまま使用することができます。

### Remove Missing Components

オブジェクトから "Missing" 状態のコンポーネントを削除します。
DynamicBone を導入していないプロジェクトでアバターをアップロードできないときに使用します。

### Remove PhysBones

アバターから Avatar Dynamics のコンポーネント (PhysBone など) を選んで削除します。

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

### Scene の自動検証

Scene 内のアバターをアップロードできない状態になっている場合に警告を表示します。

## 使用方法

unitypackage を導入後、ヒエラルキーでアバターを選択した状態でメニューから「VRCQuestTools」を選択すると各機能を使用できます。
一部機能は自動的に有効になっています。

## 動作確認環境

- Windows 10 64-bit
- macOS Big Sur (Intel CPU)
- Ubuntu 20.04 LTS
- Unity 2018.4.20f1 / 2019.4.30f1 (macOS のみ Unity 2019 専用)
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
