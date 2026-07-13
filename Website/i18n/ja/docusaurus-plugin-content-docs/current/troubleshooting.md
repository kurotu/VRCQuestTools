---
sidebar_position: 5
slug: /troubleshooting
---

# トラブルシューティング

よくある問題と対処方法をまとめています。
ここにない問題が起きた場合は、[GitHub の Issues](https://github.com/kurotu/VRCQuestTools/issues) で報告してください。

## 見た目の問題

### 変換後のアバターの明るさが PC 版と違う {#brightness}

Mobile 用シェーダーはライティングの仕組みが PC 用シェーダーと異なるため、明るさの印象はワールドによって変わります。

Toon Lit で変換している場合は、マテリアル変換設定の「明るさ」で調整できます。
Toon Lit シェーダーは環境光で明るく表示されるため、初期値は 0.83 に設定されています。

### 透過を使った表現（頬染めなど）がおかしくなる {#transparency}

Mobile 用シェーダーではテクスチャの透過が反映されません。
頬染めやメガネのレンズのような透過を前提とした表現は、そのままでは再現できません。

次のような対策があります。

- アニメーションを編集して、表示する必要のないメッシュを非表示にする
- [VQT Platform Component Remover](./components/platform-component-remover.md) や [VQT Platform GameObject Remover](./components/platform-gameobject-remover.md) で、Mobile では対象のオブジェクトを削除する
- メッシュやテクスチャを Mobile 用に編集する

### 非対応シェーダーの警告が表示される {#unsupported-shaders}

マテリアル変換は、次のシェーダーに対応しています。

- Standard
- Unity-Chan Toon Shader 2 (UTS2)
- arktoon-Shaders
- ArxCharacterShaders (AXCS)
- Sunao Shader
- lilToon
- Poiyomi

これ以外のシェーダーを使用したマテリアルは、テクスチャが正しく生成されない可能性があります。
その場合は、次の方法を試してください。

- 「Mobile用のテクスチャを生成する」をオフにして、シェーダーのみを変更する
- Mobile 用のマテリアルを自分で用意し、「マテリアル置換」または [VQT Material Swap](./components/material-swap.md) で置き換える

### スカートの裏側などが見えなくなる {#backface}

Mobile 用シェーダーではポリゴンの裏面が描画されません。
[VQT Mesh Flipper](./components/mesh-flipper.md) でメッシュを両面化すると表示できます。

### 生成されたテクスチャの内容が古い、またはおかしい {#texture-cache}

マテリアル変換で生成したテクスチャはキャッシュされています。
メニューバーの「Tools」→「VRCQuestTools」→「Clear Texture Cache」でキャッシュを削除してから、もう一度変換してください。

## アップロードの問題

### プラットフォームを Android や iOS に切り替えられない {#android-build-support}

Android への切り替えには Android Build Support、iOS への切り替えには iOS Build Support の各モジュールが必要です。
[環境を準備する](./getting-started/set-up-environment.md#android-build-support)の手順でインストールしてください。
Unity Hub からインストールできない場合は、[Unity ダウンロードアーカイブのインストーラー](./getting-started/set-up-environment.md#without-unity-hub)を使用してください。

### パフォーマンスランクが Very Poor と表示される {#very-poor}

Mobile ではパフォーマンスランクが Very Poor でもアップロードはできます。
ただし、他のプレイヤーからはデフォルトでインポスターまたはフォールバックアバターとして表示されます。
見る側が「アバターの表示 (Show Avatar)」を個別に許可すると、本来のアバターが表示されます。

### Avatar Dynamics のパフォーマンスランクが Very Poor になる {#avatar-dynamics}

PhysBone や Contact が多すぎる場合、アップロードしてもすべての PhysBone や Contact が VRChat 上で削除されます。
Avatar Dynamics のパフォーマンスランクが Poor に収まるように、コンポーネントを削減してください。

- アバター変換を使う場合：[VQT Avatar Converter Settings](./components/avatar-converter-settings.md) の「Avatar Dynamics 設定」で、残すコンポーネントを選択します。
- アバター変換を使わない場合：メニューバーの「Tools」→「VRCQuestTools」→「Remove PhysBones」で削除するコンポーネントを選択します。

## PhysBone の問題

### PC 版と Mobile 版で PhysBone の揺れ方が同期しない {#physbone-sync}

PhysBone を正しく同期させるには、PC 版と Mobile 版で PhysBone が同じネットワーク ID を持つ必要があります。

[VQT Avatar Converter Settings](./components/avatar-converter-settings.md) の「Network ID を割り当てる」を有効にするか、[VQT Network ID Assigner](./components/network-id-assigner.md) をアバターに追加してください。
その後、PC 用と Mobile 用の両方のアバターをアップロードし直すと同期するようになります。

Mobile 用の変換で PhysBone を削減している場合、1 つの GameObject に複数の PhysBone があると、正しく同期しないことがあります。

## エラーと警告

### "Missing" 状態のコンポーネントがあると表示される {#missing-components}

インポートし忘れたアセットやパッケージがないかを確認してください。
Dynamic Bone のような有料アセットを含むアバターを、そのアセットのないプロジェクトで開いた場合にも発生します。

不要なコンポーネントであれば、メニューバーの「Tools」→「VRCQuestTools」→「Remove Missing Components」で削除できます。

![Missing 状態のコンポーネント](/img/missing_script.png)

### Dynamic Bone に関する警告が表示される {#dynamic-bone}

VRCQuestTools は Dynamic Bone を PhysBone に変換しません。
アバターを変換する前に、VRChat SDK の「VRChat SDK」→「Utilities」→「Convert DynamicBones To PhysBones」などで PhysBone へ移行してください。

### Unity Constraints に関する警告が表示される {#unity-constraints}

Mobile では Unity 標準の Constraint コンポーネントを使用できず、変換時に削除されます。
VRChat Constraints へ移行すると Mobile でも使用できます。

Modular Avatar がプロジェクトにある場合は、アバターに「MA Convert Constraints」コンポーネントを追加すると、ビルド時に非破壊で VRChat Constraints へ変換されます。

### Prefab モードでアバターを変換できない {#prefab-mode}

Prefab モードではアバターを変換できません。
Prefab モードを抜けて、シーンに戻ってから変換してください。

### マスクテクスチャのエラーが表示される (Mesh Flipper) {#mesh-flipper-mask}

- 「マスクテクスチャがありません」：[VQT Mesh Flipper](./components/mesh-flipper.md) の「マスクテクスチャ」にテクスチャを設定してください。
- 「マスクテクスチャは読み取り可能に設定されている必要があります」：テクスチャのインポート設定で「Read/Write」を有効にしてください。

### 非対応のテクスチャフォーマットと表示される {#texture-format}

Mobile で使用できないテクスチャ形式（DXT など PC 用の形式）がアバターに含まれています。
マニュアルベイクで作成したテクスチャを使用している場合は、ターゲットプラットフォームを切り替えた後で、もう一度マニュアルベイクしてください。
