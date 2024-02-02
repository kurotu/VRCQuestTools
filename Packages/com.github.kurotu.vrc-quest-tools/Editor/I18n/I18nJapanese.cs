// <copyright file="I18nJapanese.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1516 // Elements should be separated by blank line
#pragma warning disable SA1600 // Elements should be documented

namespace KRT.VRCQuestTools.I18n
{
    /// <summary>
    /// Japanese strings.
    /// </summary>
    internal class I18nJapanese : I18nBase
    {
        internal override string CancelLabel => "キャンセル";
        internal override string CloseLabel => "閉じる";
        internal override string DismissLabel => "閉じる";
        internal override string YesLabel => "はい";
        internal override string NoLabel => "いいえ";
        internal override string AbortLabel => "中止";
        internal override string RemoveLabel => "削除";

        internal override string Maximum => "最大";

        // Convert Avatar for Quest
        internal override string ExitPlayModeToEdit => "編集するには再生モードを終了してください。";
        internal override string BeingConvertSettingsButtonLabel => "変換の設定を始める";
        internal override string AvatarLabel => "アバター";
        internal override string GenerateAndroidTexturesLabel => "Android用のテクスチャを生成する";
        internal override string GenerateAndroidTexturesTooltip => "メインテクスチャ以外にもマテリアルのパラメーターを参照してテクスチャを生成し、PC版アバターの外観に近づけます。";
        internal override string SupportedShadersLabel => "対応シェーダー";
        internal override string SaveToLabel => "保存先フォルダ";
        internal override string SelectButtonLabel => "選択";
        internal override string ConvertButtonLabel => "変換";
        internal override string AssignButtonLabel => "割り当て";
        internal override string UpdateTexturesLabel => "変換後のAndroid用テクスチャを更新";
        internal override string AdvancedConverterSettingsLabel => "高度な変換設定";
        internal override string RemoveVertexColorLabel => "メッシュから頂点カラーを削除";
        internal override string RemoveVertexColorTooltip => "通常このオプションを無効にする必要はありません。PC用アバターで頂点カラーの必要な特別なシェーダーを使用している場合は、誤動作を防ぐためにこのオプションを無効にできます。\n誤って頂点カラーを削除した場合は、アバターの\"VertexColorRemover\"コンポーネントで復元できます。";
        internal override string AnimationOverrideLabel => "アニメーションオーバーライド";
        internal override string AnimationOverrideTooltip => "Animator Override Controller で指定したアニメーションを使用して Animator Controller を変換します。";
        internal override string GeneratingTexturesDialogMessage => "テクスチャを生成中";
        internal override string MaterialExceptionDialogMessage => "マテリアルの変換中にエラーが発生しました。変換を中止します。";
        internal override string AnimationClipExceptionDialogMessage => "アニメーションの変換中にエラーが発生しました。変換を中止します。";
        internal override string AnimatorControllerExceptionDialogMessage => "Animator Controllerの変換中にエラーが発生しました。変換を中止します。";
        internal override string WarningForPerformance => $"変換後のアバターの推定パフォーマンスランクは Very Poor です。Android 用にアップロードできますが、以下の制限があります。\n・デフォルトではフォールバックアバターが表示され、見る側が Avatar Display 設定で個別に表示する必要があります。\n・Android スマートフォン版 VRChat では表示できません。";
        internal override string WarningForAppearance => "テクスチャの透過が反映されないため、頬染めなどの表現に問題がある場合があります。そのような場合はアニメーション編集やメッシュ削除などの方法で対策する必要があります。\n" +
            "別の Blueprint ID でのアップロードや Avatars 3.0 のローカルテストを使用して、変換後のアバターの見た目をPCで確認することをお勧めします。";
        internal override string WarningForUnsupportedShaders => $"以下のマテリアルは非対応のシェーダーを使用しており、テクスチャが正しく生成されない可能性があります。\n「{GenerateAndroidTexturesLabel}」をオフにするとシェーダーのみを変更します。";
        internal override string AlertForComponents => "以下の非対応コンポーネントを削除します。変換後、アバターの機能に支障がないか確認してください。";
        internal override string AlertForDynamicBoneConversion => $"{VRCQuestTools.Name} は Dynamic Bone を PhysBone に変換しません。アバターを変換する前に PhysBone を設定してください。";
        internal override string AlertForMissingNetIds => "ネットワークIDの割り当てられていない PhysBones があります。 PC と Android で正しく同期させるため、ネットワークIDを割り当てた後でPC用のアバターを再度アップロードしてください。";
        internal override string AlertForMultiplePhysBones => "1つの GameObject に複数の PhysBone があります。Android 用に PhysBone を削除した場合、PC と Android で正しく同期しない可能性があります。";
        internal override string AlertForAvatarDynamicsPerformance => "Avatar Dynamics (PhysBones, Contacts) のパフォーマンスランクが Very Poor になっており Android 用にアップロードできません。 Avatar Dynamics のパフォーマンスランクが Poor に収まるようコンポーネントを削減してください。";

        internal override string AvatarConverterMustBeOnAvatarRoot => "VRC_AvatarDescriptor のあるオブジェクトに配置してください。";
        internal override string AvatarConverterMaterialConvertSettingsLabel => "マテリアル変換設定";
        internal override string AvatarConverterDefaultMaterialConvertSettingLabel => "デフォルトのマテリアル変換設定";
        internal override string AvatarConverterAdditionalMaterialConvertSettingsLabel => "追加のマテリアル変換設定";

        internal override string AvatarConverterAvatarDynamicsSettingsLabel => "Avatar Dynamics 設定";
        internal override string AvatarConverterPhysBonesTooltip => "変換時に残しておく PhysBone を選択します。";
        internal override string AvatarConverterPhysBoneCollidersTooltip => "変換時に残しておく PhysBoneCollider を選択します。";
        internal override string AvatarConverterContactsTooltip => "変換時に残しておく ContactSender と ContactReceiver を選択します。";

        // IMaterialConvertSettings
        internal override string IMaterialConvertSettingsTexturesSizeLimitLabel => "最大テクスチャサイズ";
        internal override string IMaterialConvertSettingsMainTextureBrightnessLabel => "メインテクスチャの明るさ";
        internal override string IMaterialConvertSettingsMainTextureBrightnessTooltip => "メインテクスチャの色を調整します。";
        internal override string ToonLitConvertSettingsGenerateShadowFromNormalMapLabel => "ノーマルマップから影を生成する";
        internal override string MatCapLitConvertSettingsMatCapTextureLabel => "MatCap テクスチャ";
        internal override string MatCapLitConvertSettingsMatCapTextureWarning => "MatCap テクスチャを設定してください。";
        internal override string AdditionalMaterialConvertSettingsTargetMaterialLabel => "対象マテリアル";
        internal override string AdditionalMaterialConvertSettingsSelectMaterialLabel => "マテリアルを選択";
        internal override string MaterialConvertTypePopupLabelToonLit => "Toon Lit";
        internal override string MaterialConvertTypePopupLabelMatCapLit => "MatCap Lit";
        internal override string MaterialConvertTypePopupLabelMaterialReplace => "マテリアル置換";
        internal override string MaterialReplaceSettingsMaterialLabel => "置換マテリアル";
        internal override string MaterialReplaceSettingsMaterialTooltip => "対象マテリアルはこのマテリアルで置換されます。";

        // Remove Missing Components
        internal override string NoMissingComponentsMessage(string objectName) => $"{objectName} に \"Missing\" 状態のコンポーネントはありません。";
        internal override string MissingRemoverConfirmationMessage(string objectName) => $"{objectName} から \"Missing\" 状態のコンポーネントを削除します。";
        internal override string UnpackPrefabMessage => "同時に Unpack Prefab を実行します。";
        internal override string MissingRemoverOnBuildDialogMessage(string objectName) => $"{objectName} には \"Missing\" 状態のコンポーネントがあります。コンポーネントを削除した状態でビルドを続けますか？";

        // BlendShapes Copy
        internal override string SourceMeshLabel => "コピー元メッシュ";
        internal override string TargetMeshLabel => "コピー先メッシュ";
        internal override string CopyButtonLabel => "ブレンドシェイプの値をコピー";
        internal override string SwitchButtonLabel => "コピー元/コピー先を入れ替え";

        // Remove Unsupported Components
        internal override string NoUnsupportedComponentsMessage(string objectName) => $"{objectName} に非対応コンポーネントはありません。";
        internal override string UnsupportedRemoverConfirmationMessage(string objectName) => $"{objectName} から以下の非対応コンポーネントを削除します。";

        // Remove PhysBones
        internal override string PhysBonesSDKRequired => "Avatar Dynamics に対応した VRCSDK が必要です。";
        internal override string SelectComponentsToKeep => "削除せずに残すコンポーネントを選択してください。";
        internal override string PhysBonesListTooltip => "コンポーネントと Root Transform の一覧";
        internal override string KeepAll => "すべて残す";
        internal override string AvatarDynamicsPreventsUpload => "Android用にアップロードできません。 Avatar Dynamics のランクは少なくとも Poor にしてください。";
        internal override string PhysBonesWillBeRemovedAtRunTime => "Android用にアップロードできません。 PhysBone の数を減らしてください。";
        internal override string PhysBoneCollidersWillBeRemovedAtRunTime => "Android用にアップロードしても PhysBoneCollider は動作しません。 PhysBoneCollider の数を減らしてください。";
        internal override string ContactsWillBeRemovedAtRunTime => "Android用にアップロードできません。 ContactReceiver と ContactSender の数を減らしてください。";
        internal override string PhysBonesTransformsShouldBeReduced => "Android用にアップロードできません。 PhysBone の数を減らすか、PhysBone の子オブジェクトの数を減らしてください。";

        internal override string PhysBonesCollisionCheckCountShouldBeReduced => "Android用にアップロードできません。 PhysBone と PhysBoneCollider の衝突判定の数を減らしてください。";
        internal override string PhysBonesShouldHaveNetworkID => "PhysBone を正しく同期させるには PhysBone の Network ID をPC版と一致させる必要があります。VRCSDK の Network ID Utility を使用してPC用とAndroid用で同じ Network ID を割り当てた後、両方を再アップロードしてください。";
        internal override string EstimatedPerformanceStats => "推定パフォーマンスランク";
        internal override string DeleteUnselectedComponents => "選択していないコンポーネントを削除";

        // Avatar Dynamics Selector
        internal override string SelectAllButtonLabel => "すべて選択";
        internal override string DeselectAllButtonLabel => "すべて解除";
        internal override string ApplyButtonLabel => "適用";

        // Metallic Smoothness
        internal override string TextureLabel => "テクスチャ";
        internal override string InvertLabel => "反転";
        internal override string SaveFileDialogTitle(string thing) => $"{thing} を保存";
        internal override string SaveFileDialogMessage => "テクスチャの保存先を選択してください";
        internal override string GenerateButtonLabel => "Metallic Smoothness を生成";

        // Unity Settings
        internal override string TextureCompressionLabel => "Android テクスチャ圧縮";
        internal override string TextureCompressionHelp => "ASTCを使用するとAndroid用のテクスチャ圧縮に時間がかかる代わりに画質が向上します。";
        internal override string TextureCompressionButtonLabel => "ASTCでテクスチャを圧縮";
        internal override string ApplyAllButtonLabel => "すべての設定を適用";
        internal override string ShowOnStartupLabel => "起動時に表示";
        internal override string AllAppliedHelp => "すべての推奨設定が適用されています。";

        // Check for Update
        internal override string GetUpdate => "アップデート";
        internal override string SeeChangelog => "変更履歴";
        internal override string SkipThisVersion => "スキップ";
        internal override string NewVersionIsAvailable(string latestVersion) => $"新しいバージョン {latestVersion} があります。";
        internal override string NewVersionHasBreakingChanges => "このバージョンには互換性に関する重要な変更がある可能性があります。";

        // Validations
        internal override string MissingScripts => "\"Missing\"\u00A0状態のコンポーネントがあります。インポートし忘れたアセットやパッケージがないか確認してください。";

        // Vertex Color
        internal override string VertexColorRemoverEditorRemove => "頂点カラーを削除";
        internal override string VertexColorRemoverEditorRestore => "頂点カラーを復元";

        // Converted Avatar
        internal override string ConvertedAvatarEditorMessage => "このコンポーネントはアバターが VRCQuestTools で変換されたことを示します。";
        internal override string ConvertedAvatarEditorNDMFMessage => "非対応コンポーネントは NDMF の Optimization Phase で削除されます。";

        // Platform Component Remover
        internal override string ComponentRequiresNdmf => "このコンポーネントには Non-Destructive Modular Framework (NDMF) が必要です。";
        internal override string BuildTargetLabel => "ビルドターゲット";
        internal override string BuildTargetTooltip => "ビルド時のプラットフォームを指定します。Autoの場合、Unityのターゲットプラットフォームと同じになります。";
        internal override string PlatformComponentRemoverEditorComponentSettingsLabel => "コンポーネント削除設定";
        internal override string PlatformComponentRemoverEditorComponentSettingsTooltip => "コンポーネントを削除するプラットフォームを選択します。";
        internal override string ComponentLabel => "コンポーネント";

        // Platform GameObject Remover
        internal override string PlatformGameObjectRemoverEditorRemoveOnPCLabel => "PCでは削除";
        internal override string PlatformGameObjectRemoverEditorRemoveOnAndroidLabel => "Androidでは削除";

        // NDMF
        internal override string NdmfPluginRemovedUnsupportedComponent(string typeName, string objectName) => $"ビルド中に非対応コンポーネント {typeName} を \"{objectName}\" から削除しました。動作に問題がないか確認してください。";
    }
}
