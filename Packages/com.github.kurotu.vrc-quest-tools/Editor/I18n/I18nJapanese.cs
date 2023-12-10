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

        internal override string Maximum => "最大";

        // Convert Avatar for Quest
        internal override string ExitPlayModeToEdit => "編集するには再生モードを終了してください。";
        internal override string AddAvatarConverterButtonLabel(string name) => $"{name} に VQT Avatar Converter を追加";
        internal override string ConvertedAvatarLabel => "変換後アバター";
        internal override string OverwriteWarningDialogButtonCancel => "キャンセル";
        internal override string OverwriteWarningDialogButtonOK => "OK";
        internal override string OverwriteWarningDialogButtonUseAltDir(string altDir) => $"\"{altDir}\" を使用する";
        internal override string OverwriteWarningDialogMessage(string artifactsDir) => $"\"{artifactsDir}\" が既に存在します。上書きしますか？";
        internal override string OverwriteWarningDialogTitle => $"{VRCQuestTools.Name} 警告";
        internal override string AvatarLabel => "アバター";
        internal override string GenerateQuestTexturesLabel => "Quest用のテクスチャを生成する";
        internal override string QuestTexturesDescription => "メインテクスチャ以外にもマテリアルのパラメーターを参照してテクスチャを生成し、PC版アバターの外観に近づけます。";
        internal override string SupportedShadersLabel => "対応シェーダー";
        internal override string SaveToLabel => "保存先フォルダ";
        internal override string InvalidCharsInOutputPath => "フォルダ名に使用できない文字を含んでいます。";
        internal override string SelectButtonLabel => "選択";
        internal override string ConvertButtonLabel => "変換";
        internal override string AssignButtonLabel => "割り当て";
        internal override string UpdateTexturesLabel => "Quest用テクスチャのみ更新";
        internal override string AdvancedConverterSettingsLabel => "高度な変換設定";
        internal override string RemoveVertexColorLabel => "メッシュから頂点カラーを削除";
        internal override string RemoveVertexColorTooltip => "通常このオプションを無効にする必要はありません。PC用アバターで頂点カラーの必要な特別なシェーダーを使用している場合は、誤動作を防ぐためにこのオプションを無効にできます。\n誤って頂点カラーを削除した場合は、アバターの\"VertexColorRemover\"コンポーネントで復元できます。";
        internal override string OverwriteDestinationAvatarLabel => "アバターを上書き";
        internal override string OverwriteDestinationAvatarTooltip => "変換後、シーンに既に存在している変換済みのアバターを削除します。";
        internal override string AnimationOverrideLabel => "アニメーションオーバーライド";
        internal override string AnimationOverrideTooltip => "Animator Override Controller で指定したアニメーションを使用して Animator Controller を変換します。";
        internal override string AnimationOverrideMaterialErrorMessage => "Animator Override Controller に Quest で使用できないマテリアルへ変更するアニメーションがあります。";
        internal override string ConvertingMaterialsDialogMessage => "マテリアルを変換中";
        internal override string GeneratingTexturesDialogMessage => "テクスチャを生成中";
        internal override string MaterialExceptionDialogMessage => "マテリアルの変換中にエラーが発生しました。変換を中止します。";
        internal override string AnimationClipExceptionDialogMessage => "アニメーションの変換中にエラーが発生しました。変換を中止します。";
        internal override string AnimatorControllerExceptionDialogMessage => "Animator Controllerの変換中にエラーが発生しました。変換を中止します。";
        internal override string WarningForPerformance => $"{VRCQuestTools.Name}はポリゴン数削減などのパフォーマンスランクの最適化をしません。ほとんどの場合、変換後のアバターは Quest のパフォーマンスランクで Very Poor になります。\n・デフォルトではフォールバックアバターが表示され、見る側が Avatar Display 設定で個別に表示する必要があります。\n・Very Poor のアバターは Android スマートフォン版 VRChat では使用・表示できません。";
        internal override string WarningForAppearance => "テクスチャの透過が反映されないため、頬染めなどの表現に問題がある場合があります。そのような場合はアニメーション編集やメッシュ削除などの方法で対策する必要があります。\n\n" +
            "別の Blueprint ID でのアップロードや Avatars 3.0 のローカルテストを使用して、変換後のアバターの見た目をPCで確認することをお勧めします。";
        internal override string WarningForUnsupportedShaders => $"以下のマテリアルは非対応のシェーダーを使用しており、テクスチャが正しく生成されない可能性があります。\n「{GenerateQuestTexturesLabel}」をオフにするとシェーダーのみを変更します。";
        internal override string AlertForComponents => "以下の非対応コンポーネントを削除します。変換後、アバターの機能に支障がないか確認してください。";
        internal override string AlertForMaterialAnimation => "マテリアルを変更するアニメーションがあるため、Animator ControllerおよびアニメーションをQuest用に複製・変換します。";
        internal override string AlertForDynamicBoneConversion => $"{VRCQuestTools.Name} は Dynamic Bone を PhysBones に変換しません。アバターを変換する前に PhysBones を設定してください。";
        internal override string AlertForMissingNetIds => "ネットワークIDの割り当てられていない PhysBones があります。 PC と Quest で正しく同期させるため、ネットワークIDを割り当てた後でPC用のアバターを再度アップロードしてください。";
        internal override string AlertForMultiplePhysBones => "1つの GameObject に複数の PhysBone があります。変換後に Quest 用に PhysBone を削除した場合、PC と Quest で正しく同期しない可能性があります。";
        internal override string AlertForAvatarDynamicsPerformance => "Avatar Dynamics コンポーネントの数が Poor の制限値を超えています (Very Poor)。 Avatar Dynamics 関連のパフォーマンスランクが Poor に収まるようコンポーネントを削除してください。";
        internal override string TexturesSizeLimitLabel => "最大テクスチャサイズ";
        internal override string MainTextureBrightnessLabel => "メインテクスチャの明るさ";
        internal override string MainTextureBrightnessTooltip => "Toon Lit 用にメインテクスチャの色を調整します。";
        internal override string CompletedDialogMessage(string originalName) => $"{originalName} の変換が完了しました。\n表情などを確認した後、PC用と同じBlueprint IDを使ってAndroidプラットフォーム用にアップロードしてください。";

        internal override string AvatarConverterMustBeOnAvatarRoot => "VRC_AvatarDescriptor のあるオブジェクトに配置してください。";
        internal override string AvatarConverterMaterialConvertSettingLabel => "マテリアル変換設定";
        internal override string AvatarConverterDefaultMaterialConvertSettingLabel => "デフォルトのマテリアル変換設定";
        internal override string AvatarConverterAdditionalMaterialConvertSettingsLabel => "追加のマテリアル変換設定";
        internal override string AvatarConverterTargetMaterialLabel => "対象マテリアル";
        internal override string AvatarConverterReplaceMaterialLabel => "置換マテリアル";

        internal override string AvatarConverterAvatarDynamicsSettingLabel => "Avatar Dynamics 設定";
        internal override string AvatarConverterPhysBonesTooltip => "変換時に残しておく PhysBone を選択します。";
        internal override string AvatarConverterPhysBoneCollidersTooltip => "変換時に残しておく PhysBoneCollider を選択します。";
        internal override string AvatarConverterContactsTooltip => "変換時に残しておく ContactSender と ContactReceiver を選択します。";

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
        internal override string AvatarDynamicsPreventsUpload => "Quest用にアップロードできません。 Avatar Dynamics のランクは少なくとも Poor にしてください。";
        internal override string PhysBonesWillBeRemovedAtRunTime => "Quest用にアップロードできません。 PhysBone の数を減らしてください。";
        internal override string PhysBoneCollidersWillBeRemovedAtRunTime => "Quest用にアップロードしても PhysBoneCollider は動作しません。 PhysBoneCollider の数を減らしてください。";
        internal override string ContactsWillBeRemovedAtRunTime => "Quest用にアップロードできません。 ContactReceiver と ContactSender の数を減らしてください。";
        internal override string PhysBonesTransformsShouldBeReduced => "Quest用にアップロードできません。 PhysBone の数を減らすか、PhysBone の子オブジェクトの数を減らしてください。";

        internal override string PhysBonesCollisionCheckCountShouldBeReduced => "Quest用にアップロードできません。 PhysBone と PhysBoneCollider の衝突判定の数を減らしてください。";
        internal override string PhysBonesOrderMustMatchWithPC => "PhysBones を正しく同期させるには PhysBones の Network ID をPC版と一致させる必要があります。残すコンポーネントをリスト先頭から順番に選んでください。";
        internal override string PhysBonesShouldHaveNetworkID => "PhysBones を正しく同期させるには PhysBones の Network ID をPC版と一致させる必要があります。VRCSDK の Network ID Utility を使用してPC用とQuest用で同じ Network ID を割り当てた後、両方を再アップロードしてください。";
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
        internal override string CacheServerModeLabel => "キャッシュサーバー";
        internal override string CacheServerHelp => "ローカルキャッシュサーバーを使用すると、テクスチャ圧縮などの結果を保存して次回にかかる時間を短縮できます。デフォルト設定では C ドライブを最大 10 GB 使用します。";
        internal override string CacheServerButtonLabel => "ローカルキャッシュサーバーを有効化";
        internal override string TextureCompressionLabel => "Android テクスチャ圧縮";
        internal override string TextureCompressionHelp => "ASTCを使用するとQuest用のテクスチャ圧縮に時間がかかる代わりに画質が向上します。";
        internal override string TextureCompressionButtonLabel => "ASTCでテクスチャを圧縮";
        internal override string ApplyAllButtonLabel => "すべての設定を適用";
        internal override string ShowOnStartupLabel => "起動時に表示";
        internal override string AllAppliedHelp => "すべての推奨設定が適用されています。";

        // Check for Update
        internal override string CheckLater => "後で確認";
        internal override string GetUpdate => "アップデート";
        internal override string SeeChangelog => "変更履歴";
        internal override string SkipThisVersion => "スキップ";
        internal override string NewVersionIsAvailable(string latestVersion) => $"新しいバージョン {latestVersion} があります。";
        internal override string NewVersionHasBreakingChanges => "このバージョンには互換性に関する重要な変更がある可能性があります。";
        internal override string ThereIsNoUpdate => "アップデートはありません。";

        // Validations
        internal override string DeactivateAvatar => "アバターを非表示";
        internal override string IncompatibleForQuest => $"ビルドターゲットが Android の場合 Quest 用にアップロードできないアバターがシーン内にあるとアバターをアップロードできません。エラーのあるアバターを非表示にするか、ビルドターゲットを PC に戻してください。";
        internal override string MissingScripts => "\"Missing\"\u00A0状態のコンポーネントがあります。インポートし忘れたアセットやパッケージがないか確認してください。";
        internal override string ValidatorAlertsProhibitedShaders(string shaderName, string[] materialNames) =>
            $"シェーダー \"{shaderName}\" (マテリアル: {string.Join(", ", materialNames)}) は Quest で使用できません。";
        internal override string ValidatorAlertsUnsupportedComponents(string componentName, string objectName) =>
            $"コンポーネント \"{componentName}\" ({objectName}) は Quest で使用できません。";
        internal override string ValidatorAlertsVeryPoorPhysBones(int count) =>
            $"PhysBone の数が多すぎます: {count} (Very Poor)";
        internal override string ValidatorAlertsVeryPoorPhysBoneColliders(int count) =>
            $"PhysBoneCollider の数が多すぎます: {count} (Very Poor)";
        internal override string ValidatorAlertsVeryPoorContacts(int count) =>
            $"ContactSender と ContactReceiver の数が多すぎます: {count} (Very Poor)";

        // Vertex Color
        internal override string VertexColorRemoverEditorRemove => "頂点カラーを削除";
        internal override string VertexColorRemoverEditorRestore => "頂点カラーを復元";
        internal override string VertexColorRemoverDialogTitle => $"頂点カラーの削除 - {VRCQuestTools.Name}";
        internal override string VertexColorRemoverDialogMessage(string name) =>
            $"\"{name}\" のメッシュには頂点カラーがあります。テクスチャ色を正しく反映するために頂点カラーを削除しますか？\n\n※通常は「{YesLabel}」を選択します。頂点カラーを必要とする特別なシェーダーをPC用アバターで使用している場合に「{NoLabel}」を選択します。";
    }
}
