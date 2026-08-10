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

頬染めなどの表情が不透明なベタ塗り（いわゆる「海苔」）になる場合は、アバターが対応していれば次のツールでも対策できます。

- [NoriBlocker](https://riceworks.booth.pm/items/5808613)：ベタ塗りになる表情をアニメーションで上書きし、表示されないようにします。
- [海苔はずシート & 海苔む～ば～](https://riceworks.booth.pm/items/6955600)：ベタ塗りになる部分のメッシュを削除します。

対応アバターの一覧は、それぞれの配布ページを確認してください。

VRoid Studio で作成したアバターについては、[眉やまつげが四角く見える](#vroid-transparency)も参照してください。

### VRoid Studio で作成したアバターの眉やまつげが四角く見える {#vroid-transparency}

VRoid Studio で作成したアバターは、眉やまつげなどを透過テクスチャのメッシュで表現しています。
Mobile 用シェーダーは透過を描画しないため、変換すると透過していた部分が不透明な面として残り、眉やまつげが四角く見えます。

[Yoridori Modifiers](https://yoridrill.booth.pm/items/8189252) の「YM Mesh Trimmer」を使用すると、テクスチャの透過に合わせてメッシュを切り取り、この問題を回避できます。

### 頬染めなどの透過をパーティクルシェーダーで再現したい {#particle-shader}

「VRChat/Mobile/Particles/Additive」などのパーティクル用シェーダーを使うと、Mobile でも頬染めなどの透過を表現できることが知られています。
VRCQuestTools はこの方法を推奨しておらず、アバターのマテリアルをパーティクル用シェーダーに変換する機能を提供する予定もありません。

- [VRChat のドキュメント](https://creators.vrchat.com/platforms/android/quest-content-limitations)では、これらのシェーダーは「Should be used on particles.」（パーティクルに使用してください）と説明されています。
- 透過（アルファブレンド）の描画は Mobile 向けの GPU では負荷が高く、[Meta のドキュメント](https://developers.meta.com/horizon/design/design-graphic-rendering-pipeline/)でも過度な使用を避けるよう案内されています。

パーティクル用シェーダーへの変換は、パーティクルシステム、Trail Renderer、Line Renderer で使われているマテリアルに限り提供します。

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

Toon Lit ではポリゴンの裏面が描画されません。
Toon Standard は裏面の描画に対応しているため、[マテリアル変換設定](./components/avatar-converter-settings.md)を Toon Standard にすると表示できます。
デフォルトのマテリアル変換設定は Toon Standard です。
lilToon や Poiyomi のマテリアルから変換する場合は、元のマテリアルの両面表示の設定を引き継ぎます。

Toon Lit で変換する場合は、[VQT Mesh Flipper](./components/mesh-flipper.md) でメッシュを両面化すると表示できます。

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

## 同期の問題

### PC と Mobile でギミックや衣装の状態が一致しない {#parameter-sync}

VRChat がプラットフォーム間でパラメーターを同期するには、同期するパラメーターが Expression Parameters の先頭に同じ順序で並んでいる必要があります。
PC と Mobile で並び順がずれていると、別のパラメーターに値が入り、衣装やギミックの状態が食い違います。

Modular Avatar の「MA Sync Parameter Sequence」をアバターに追加すると、プラットフォーム間で並び順が揃うように調整されます。
基準にするプラットフォーム（Primary Platform）を選び、そのプラットフォームから先にアップロードしてください。
[VQT Avatar Converter Settings](./components/avatar-converter-settings.md) では、このコンポーネントがない場合に追加を促す警告が表示されます。

詳しくは Modular Avatar の [Sync Parameter Sequence](https://modular-avatar.nadena.dev/ja/docs/reference/sync-parameter-sequence) を参照してください。

### PC 版と Mobile 版で PhysBone の同期がずれる {#physbone-sync}

PhysBone の状態は、ネットワーク ID でどの PhysBone かを識別して同期されます。
PC 版と Mobile 版で ID が一致していないと、PhysBone をつかんだときに、別のプラットフォームのプレイヤーの画面では違うボーンが動いてしまいます。

正しく同期させるには、PC 版と Mobile 版で PhysBone が同じネットワーク ID を持つ必要があります。

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
