#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1516 // Elements should be separated by blank line
#pragma warning disable SA1600 // Elements should be documented

namespace KRT.VRCQuestTools.I18n
{
    /// <summary>
    /// Russian strings.
    /// </summary>
    internal class I18nRussian : I18nBase
    {
        internal override string CancelLabel => "Отмена";
        internal override string OpenLabel => "Открыть";
        internal override string CloseLabel => "Закрыть";
        internal override string DismissLabel => "Отклонить";
        internal override string YesLabel => "Да";
        internal override string NoLabel => "Нет";
        internal override string AbortLabel => "Прервать";
        internal override string RemoveLabel => "Удалить";

        internal override string Maximum => "Максимум";

        internal override string IncompatibleSDK => "Несовместимый VRChat SDK. Пожалуйста, сообщите об этом.";

        // Convert Avatar for Quest
        internal override string AvatarConverterSettingsEditorDescription => "Конвертировать этот аватар для загрузки на платформу Android.";
        internal override string AvatarConverterSettingsEditorDescriptionNDMF => "Конвертировать этот аватар для загрузки на платформу Android, когда целевая платформа - Android.";
        internal override string ExitPlayModeToEdit => "Выйдите из игрового режима для редактирования.";
        internal override string BeingConvertSettingsButtonLabel => $"Начать настройки конвертера";
        internal override string AvatarLabel => "Аватар";
        internal override string GenerateAndroidTexturesLabel => "Создать текстуры для Android";
        internal override string GenerateAndroidTexturesTooltip => "Создавая новые текстуры с применением параметров материалов, а не только основных текстур, приближайтесь к версии аватара для ПК";
        internal override string SupportedShadersLabel => "Поддерживаемые шейдеры";
        internal override string SaveToLabel => "Папка для сохранения";
        internal override string SelectButtonLabel => "Выбрать";
        internal override string ConvertButtonLabel => "Конвертировать";
        internal override string AssignButtonLabel => "Назначить";
        internal override string AttachButtonLabel => "Прикрепить";
        internal override string UpdateTexturesLabel => "Обновить конвертированные текстуры для Android";
        internal override string AdvancedConverterSettingsLabel => "Расширенные настройки конвертера";
        internal override string RemoveVertexColorLabel => "Удалить цвет вершин из сеток";
        internal override string RemoveVertexColorTooltip => "Обычно вам не нужно отключать эту опцию. Вы можете отключить эту опцию, чтобы предотвратить неожиданное поведение при использовании специальных шейдеров, которые требуют цвет вершин в аватарах ПК.";
        internal override string AnimationOverrideLabel => "Переопределение анимации";
        internal override string AnimationOverrideTooltip => "Конвертировать контроллеры анимации с анимациями контроллера переопределения анимации.";
        internal override string NdmfPhaseLabel => "Этап NDMF для конвертации";
        internal override string NdmfPhaseTooltip => "Конвертировать аватар на выбранном этапе NDMF.";
        internal override string GeneratingTexturesDialogMessage => "Создание текстур...";
        internal override string MaterialExceptionDialogMessage => "Произошла ошибка при конвертации материалов. Прервано.";
        internal override string AnimationClipExceptionDialogMessage => "Произошла ошибка при конвертации клипов анимации. Прервано.";
        internal override string AnimatorControllerExceptionDialogMessage => "Произошла ошибка при конвертации контроллеров анимации. Прервано.";
        internal override string InfoForNdmfConversion => "Вы можете неразрушающе конвертировать аватар во время сборки, когда проект имеет пакет Неразрушаемая Модульная Структура (NDMF). Пожалуйста, используйте Конструктор Аватаров, чтобы избежать ограничений VRChat SDK.";
        internal override string InfoForNetworkIdAssigner => "Вы можете неразрушающе назначить сетевые ID, прикрепив компонент назначателя сетевых ID к корневому объекту аватара. Пожалуйста, загрузите снова, чтобы применить ID после прикрепления компонента.";
        internal override string WarningForPerformance => $"Оценка производительности составляет Очень Плохо. Вы можете загрузить конвертированный аватар для Android, но есть следующие ограничения.";
        internal override string WarningForAppearance => "Прозрачность текстуры не оказывает никакого эффекта, поэтому это будет проблемой для выражения лица.";
        internal override string WarningForUnsupportedShaders => $"Следующие материалы используют неподдерживаемые шейдеры. Текстуры могут быть сгенерированы неправильно.";
        internal override string AlertForComponents => "Следующие неподдерживаемые компоненты будут удалены. Проверьте функции аватара после конвертации.";
        internal override string AlertForDynamicBoneConversion => "Не конвертируйте динамические кости(PhysBone) в физические кости(DynamicBone). Пожалуйста, настройте физические кости перед конвертацией аватара.";
        internal override string AlertForUnityConstraintsConversion => "Не конвертируйте ограничения Unity в ограничения VRChat. Пожалуйста, настройте ограничения VRChat перед конвертацией аватара.";
        internal override string AlertForMissingNetIds => "Есть физические кости, у которых нет сетевого ID. Чтобы сохранить синхронизацию между ПК и Android, назначьте сетевые ID, затем повторно загрузите аватар для ПК.";
        internal override string AlertForAvatarDynamicsPerformance => "Оценка производительности динамики аватара (физические кости и контакты) будет \"Очень Плохо\", поэтому вы не сможете загрузить для Android.";
        
        internal override string ErrorForPrefabStage => "Невозможно конвертировать аватар в режиме префаба. Пожалуйста, вернитесь к сцене из режима префаба.";

        internal override string AvatarConverterMaterialConvertSettingsLabel => "Настройки конвертации материалов";
        internal override string AvatarConverterDefaultMaterialConvertSettingLabel => "Настройки конвертации материалов по умолчанию";
        internal override string AvatarConverterAdditionalMaterialConvertSettingsLabel => "Дополнительные настройки конвертации материалов";

        internal override string AvatarConverterAvatarDynamicsSettingsLabel => "Настройки динамики аватара";
        internal override string AvatarConverterPhysBonesTooltip => "Установите физические кости, которые нужно сохранить во время конвертации.";
        internal override string AvatarConverterPhysBoneCollidersTooltip => "Установите коллайдеры физической кости, которые нужно сохранить во время конвертации.";
        internal override string AvatarConverterContactsTooltip => "Установите отправители и получатели контактов, которые нужно сохранить во время конвертации.";

        // IMaterialConvertSettings
        internal override string IMaterialConvertSettingsTexturesSizeLimitLabel => "Ограничение размера текстур";
        internal override string IMaterialConvertSettingsMainTextureBrightnessLabel => "Яркость основной текстуры";
        internal override string IMaterialConvertSettingsMainTextureBrightnessTooltip => "Настройте цвет основной текстуры.";
        internal override string ToonLitConvertSettingsGenerateShadowFromNormalMapLabel => "Создать тень из нормальной карты";
        internal override string MatCapLitConvertSettingsMatCapTextureLabel => "Текстура MatCap";
        internal override string MatCapLitConvertSettingsMatCapTextureWarning => "Установите текстуру MatCap.";
        internal override string AdditionalMaterialConvertSettingsTargetMaterialLabel => "Целевой материал";
        internal override string AdditionalMaterialConvertSettingsSelectMaterialLabel => "Выбрать материал";
        internal override string MaterialConvertTypePopupLabelToonLit => "Мультяшный свет";
        internal override string MaterialConvertTypePopupLabelMatCapLit => "MatCap свет";
        internal override string MaterialConvertTypePopupLabelMaterialReplace => "Замена материала";
        internal override string MaterialReplaceSettingsMaterialLabel => "Заменяемый материал";
        internal override string MaterialReplaceSettingsMaterialTooltip => "Целевой материал будет заменен этим материалом.";

        // Remove Missing Components
        internal override string NoMissingComponentsMessage(string objectName) => $"В {objectName} нет \"недостающих\" компонентов.";
        internal override string MissingRemoverConfirmationMessage(string objectName) => $"Удалить \"недостающие\" компоненты из {objectName}?";
        internal override string UnpackPrefabMessage => "Это также выполняет операцию \"Разупаковать префаб\".";
        internal override string MissingRemoverOnBuildDialogMessage(string objectName) => $"{objectName} имеет \"недостающие\" компоненты. Вы хотите продолжить сборку аватара без таких компонентов?";

        // BlendShapes Copy
        internal override string SourceMeshLabel => "Исходная сетка";
        internal override string TargetMeshLabel => "Целевая сетка";
        internal override string CopyButtonLabel => "Копировать веса BlendShape";
        internal override string SwitchButtonLabel => "Переключить источник/цель";

        // Remove Unsupported Components
        internal override string NoUnsupportedComponentsMessage(string objectName) => $"В {objectName} нет неподдерживаемых компонентов.";
        internal override string UnsupportedRemoverConfirmationMessage(string objectName) => $"Удалить следующие неподдерживаемые компоненты из {objectName}?";

        // Remove PhysBones
        internal override string PhysBonesSDKRequired => "Требуется VRCSDK, поддерживающий динамику аватара.";
        internal override string SelectComponentsToKeep => "Выберите компоненты для сохранения.";
        internal override string PhysBonesListTooltip => "Список компонентов и их корневых трансформаций.";
        internal override string KeepAll => "Сохранить все";
        internal override string AvatarDynamicsPreventsUpload => "Вы не можете загрузить этот аватар для Android. По крайней мере, динамика аватара должна иметь оценку \"Плохо\".";
        internal override string PhysBonesWillBeRemovedAtRunTime => "Вы не можете загрузить этот аватар для Android. Пожалуйста, уменьшите количество компонентов PhysBone.";
        internal override string PhysBoneCollidersWillBeRemovedAtRunTime => "Все коллайдеры PhysBone будут удалены во время выполнения на Android. Пожалуйста, уменьшите количество компонентов PhysBoneCollider.";
        internal override string ContactsWillBeRemovedAtRunTime => "Вы не можете загрузить этот аватар для Android. Пожалуйста, уменьшите количество компонентов VRCContact.";
        internal override string PhysBonesTransformsShouldBeReduced => "Вы не можете загрузить этот аватар для Android. Пожалуйста, уменьшите количество компонентов VRCPhysBone или количество трансформаций в иерархии под компонентами VRCPhysBone.";
        
        internal override string PhysBonesCollisionCheckCountShouldBeReduced => "Вы не можете загрузить этот аватар для Android. Пожалуйста, уменьшите количество проверок столкновений между компонентами VRCPhysBone и VRCPhysBoneCollider.";
        
        internal override string PhysBonesShouldHaveNetworkID => "Чтобы правильно синхронизировать физические кости, физические кости должны иметь одинаковый сетевой ID между ПК и Android.";
        internal override string AlertForMultiplePhysBones => "В одном GameObject есть несколько физических костей. Они могут быть неправильно синхронизированы между ПК и Android после удаления физических костей.";
        internal override string EstimatedPerformanceStats => "Оценка производительности";
        internal override string DeleteUnselectedComponents => "Удалить невыбранные компоненты";

        // Avatar Dynamics Selector
        internal override string SelectAllButtonLabel => "Выбрать все";
        internal override string DeselectAllButtonLabel => "Снять выделение";
        internal override string ApplyButtonLabel => "Применить";

        // Metallic Smoothness
        internal override string TextureLabel => "Текстура";
        internal override string InvertLabel => "Инвертировать";
        internal override string SaveFileDialogTitle(string thing) => $"Сохранить {thing}";
        internal override string SaveFileDialogMessage => "Пожалуйста, введите имя файла для сохранения текстуры";
        internal override string GenerateButtonLabel => "Создать металлическую гладкость";

        // Unity Settings
        internal override string RecommendedUnitySettingsForAndroid => "Рекомендуемые настройки для Android";
        internal override string TextureCompressionLabel => "Сжатие текстур для Android";
        internal override string TextureCompressionHelp => "ASTC улучшает качество текстур для Android в обмен на длительное время сжатия";
        internal override string TextureCompressionButtonLabel => "Установить сжатие текстур на ASTC";
        internal override string ApplyAllButtonLabel => "Применить все настройки";
        internal override string ShowOnStartupLabel => "Показывать при запуске";
        internal override string AllAppliedHelp => "Хорошо, все рекомендуемые настройки применены.";

        // Check for Update
        internal override string GetUpdate => "Получить обновление";
        internal override string SeeChangelog => "Журнал изменений";
        internal override string SkipThisVersion => "Пропустить эту версию";
        internal override string NewVersionIsAvailable(string latestVersion) => $"Доступна новая версия {latestVersion}.";
        internal override string NewVersionHasBreakingChanges => $"Эта версия может иметь критические изменения в совместимости.";

        // Validations
        internal override string MissingScripts => "Есть \"недостающие\" скрипты. Пожалуйста, проверьте активы или пакеты, которые вы забыли импортировать.";

        // Inspector
        internal override string AvatarRootComponentMustBeOnAvatarRoot => "Этот компонент должен быть прикреплен к объекту VRC_AvatarDescriptor.";

        // Vertex Color
        internal override string VertexColorRemoverEditorDescription => "Цвет вершин автоматически удаляется из сетки этого объекта.";
        internal override string VertexColorRemoverEditorRemove => "Удалить цвет вершин";
        internal override string VertexColorRemoverEditorRestore => "Восстановить цвет вершин";

        // Converted Avatar
        internal override string ConvertedAvatarEditorMessage => "Этот компонент указывает на то, что аватар был конвертирован с помощью VRCQuestTools.";
        internal override string ConvertedAvatarEditorNDMFMessage => "Компоненты, которые не поддерживаются на Android, будут удалены на этапе оптимизации NDMF.";

        // Network ID Assigner
        internal override string NetworkIDAssignerEditorDescription => "Назначьте сетевые ID компонентам аватара, таким как физические кости. ID определяются хеш-значениями иерархических путей от корня аватара.";

        // Platform Target Settings
        internal override string PlatformTargetSettingsEditorDescription => "Компоненты принудительно используют определенные настройки платформы в процессе сборки NDMF.";
        internal override string PlatformTargetSettingsIsRequiredToEnforcePlatform => "Компонент настройки целевой платформы требуется для корневого объекта аватара, чтобы принудительно применить определенные настройки платформы.";

        // Platform Component Remover
        internal override string ComponentRequiresNdmf => "Требуется Неразрушаемая Модульная Структура (NDMF).";
        internal override string BuildTargetLabel => "Целевая сборка";
        internal override string BuildTargetTooltip => "Выберите целевую платформу сборки. Используйте целевую платформу Unity, когда выбрана Авто.";
        internal override string PlatformComponentRemoverEditorDescription => "Компоненты будут удалены при сборке для невыбранных целевых платформ.";
        internal override string PlatformComponentRemoverEditorComponentSettingsLabel => "Компоненты для сохранения";
        internal override string PlatformComponentRemoverEditorComponentSettingsTooltip => "Выберите платформу для сохранения компонентов.";
        internal override string PlatformComponentRemoverEditorCheckboxPCTooltip => "Сохранить выбранные компоненты при сборке для ПК.";
        internal override string PlatformComponentRemoverEditorCheckboxAndroidTooltip => "Сохранить выбранные компоненты при сборке для Android.";
        internal override string ComponentLabel => "Компонент";

        // Platform GameObject Remover
        internal override string PlatformGameObjectRemoverEditorDescription => "Этот объект будет удален при сборке для невыбранных целевых платформ.";
        internal override string PlatformGameObjectRemoverEditorKeepOnPCLabel => "Сохранить на ПК";
        internal override string PlatformGameObjectRemoverEditorKeepOnAndroidLabel => "Сохранить на Android";

        // Avatar Builder
        internal override string AvatarBuilderWindowExitPlayMode => "Выйдите из игрового режима, чтобы собрать аватар.";
        internal override string AvatarBuilderWindowExitPrefabStage => "Выйдите из режима редактирования префаба, чтобы собрать аватар.";
        internal override string AvatarBuilderWindowNoActiveAvatarsFound => "Нет активных аватаров в сцене.";
        internal override string AvatarBuilderWindowSelectAvatar => "Выберите аватар для сборки в панели управления VRChat SDK.";
        internal override string AvatarBuilderWindowNoNdmfComponentsFound => "В аватаре не найдено компонентов VRCQuestTools.";


        internal override string AvatarBuilderWindowSucceededBuild => "Сборка аватара прошла успешно.";
        internal override string AvatarBuilderWindowSucceededUpload => "Загрузка аватара прошла успешно.";
        internal override string AvatarBuilderWindowFailedBuild => "Не удалось собрать аватар. Посмотрите журналы консоли.";
        internal override string AvatarBuilderWindowRequiresControlPanel => "Необходимо открыть панель управления VRChat SDK для сборки аватара.";
        internal override string AvatarBuilderWindowOfflineTestingLabel => "Офлайн-тестирование (ПК)";
        internal override string AvatarBuilderWindowOfflineTestingDescription(string name) => $"Соберите этот аватар с настройками сборки для Android, чтобы протестировать его на ПК.";
        internal override string AvatarBuilderWindowOnlinePublishingLabel(string platformName) => $"Онлайн-публикация ({platformName})";
        internal override string AvatarBuilderWindowOnlinePublishingDescription => "Загрузите этот аватар в VRChat с настройками сборки для Android.";
        internal override string AvatarBuilderWindowSetAsFallbackIfPossible => "Установить как резервный аватар, если возможно";
        internal override string AvatarBuilderWindowSetAsFallbackIfPossibleTooltip => "Рейтинг производительности должен быть Хорошим или лучше после сборки аватара.";
        internal override string AvatarBuilderWindowFallbackNotAllowed(string rating) => $"Не удалось установить как резервный аватар, потому что рейтинг производительности {rating}.";
        internal override string AvatarBuilderWindowNdmfManualBakingLabel => "Ручное выпекание NDMF";
        internal override string AvatarBuilderWindowNdmfManualBakingDescription => "Выполните меню, Инструменты -> NDM Framework -> Ручное выпекание аватара с настройками сборки для Android.";
        internal override string AvatarBuilderWindowRequiresAvatarNameAndThumb => "Необходимо установить имя и миниатюру в панели управления VRChat SDK при загрузке нового аватара.";

        // NDMF
        internal override string NdmfPluginRequiresNdmfUpdate(string version) => $"Требуется Неразрушаемая Модульная Структура (NDMF) {version} или более поздняя версия. Пожалуйста, обновите NDMF.";
        internal override string NdmfPluginRemovedUnsupportedComponent(string typeName, string objectName) => $"Удален неподдерживаемый компонент \"{typeName}\" из \"{objectName}\". Пожалуйста, протестируйте аватар.";
    }
}
