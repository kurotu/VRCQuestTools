using System;
using System.Collections.Generic;
using EditorDriver;
using KRT.VRCQuestTools.Components;
using KRT.VRCQuestTools.Menus;
using KRT.VRCQuestTools.ViewModels;
using KRT.VRCQuestTools.Views;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase;

namespace KRT.VRCQuestTools.Debug.Screenshots
{
    /// <summary>
    /// Debug menu to capture fixed-size, English-language screenshots of VRCQuestTools
    /// EditorWindows and component inspectors for documentation.
    /// </summary>
    /// <remarks>
    /// Every capture is asynchronous: opening a window and capturing it must be separated by a
    /// couple of Editor update ticks so its first OnGUI/layout pass has run (see
    /// <see cref="CaptureScheduler"/>), so each capture method takes a completion callback instead
    /// of returning once finished. "Capture All" chains captures one at a time through that
    /// callback rather than firing them in parallel, to avoid opening many windows/temp
    /// GameObjects at once.
    /// </remarks>
    internal static class ScreenshotMenu
    {
        private const string Root = VRCQuestToolsMenus.MenuPaths.RootMenu + "Debug/Screenshots/";

        [MenuItem(Root + "Setup Avatar for Mobile Window")]
        private static void MenuCaptureAvatarConverterWindow() => CaptureAvatarConverterWindow(null);

        [MenuItem(Root + "Unity Settings for Mobile Window")]
        private static void MenuCaptureUnityQuestSettingsWindow() => CaptureUnityQuestSettingsWindow(null);

        [MenuItem(Root + "BlendShapes Copy Window")]
        private static void MenuCaptureBlendShapesCopyWindow() => CaptureBlendShapesCopyWindow(null);

        [MenuItem(Root + "Metallic Smoothness Map Window")]
        private static void MenuCaptureMSMapGenWindow() => CaptureMSMapGenWindow(null);

        [MenuItem(Root + "Remove PhysBones Window")]
        private static void MenuCapturePhysBonesRemoveWindow() => CapturePhysBonesRemoveWindow(null);

        [MenuItem(Root + "Components/Avatar Converter Settings")]
        private static void MenuCaptureAvatarConverterSettings() => CaptureAvatarConverterSettings(null);

        [MenuItem(Root + "Components/Material Swap")]
        private static void MenuCaptureMaterialSwap() => CaptureMaterialSwap(null);

        [MenuItem(Root + "Components/Material Conversion Settings")]
        private static void MenuCaptureMaterialConversionSettings() => CaptureMaterialConversionSettings(null);

        [MenuItem(Root + "Components/Menu Icon Resizer")]
        private static void MenuCaptureMenuIconResizer() => CaptureMenuIconResizer(null);

        [MenuItem(Root + "Components/Mesh Flipper")]
        private static void MenuCaptureMeshFlipper() => CaptureMeshFlipper(null);

        [MenuItem(Root + "Components/Platform Component Remover")]
        private static void MenuCapturePlatformComponentRemover() => CapturePlatformComponentRemover(null);

        [MenuItem(Root + "Components/Platform GameObject Remover")]
        private static void MenuCapturePlatformGameObjectRemover() => CapturePlatformGameObjectRemover(null);

        [MenuItem(Root + "Components/Platform Target Settings")]
        private static void MenuCapturePlatformTargetSettings() => CapturePlatformTargetSettings(null);

        [MenuItem(Root + "Components/Vertex Color Remover")]
        private static void MenuCaptureVertexColorRemover() => CaptureVertexColorRemover(null);

        [MenuItem(Root + "Capture All")]
        private static void CaptureAll()
        {
            var steps = new Queue<Action<Action>>();
            steps.Enqueue(CaptureAvatarConverterWindow);
            steps.Enqueue(CaptureUnityQuestSettingsWindow);
            steps.Enqueue(CaptureBlendShapesCopyWindow);
            steps.Enqueue(CaptureMSMapGenWindow);
            steps.Enqueue(CapturePhysBonesRemoveWindow);
            steps.Enqueue(CaptureAvatarConverterSettings);
            steps.Enqueue(CaptureMaterialSwap);
            steps.Enqueue(CaptureMaterialConversionSettings);
            steps.Enqueue(CaptureMenuIconResizer);
            steps.Enqueue(CaptureMeshFlipper);
            steps.Enqueue(CapturePlatformComponentRemover);
            steps.Enqueue(CapturePlatformGameObjectRemover);
            steps.Enqueue(CapturePlatformTargetSettings);
            steps.Enqueue(CaptureVertexColorRemover);

            RunNext(steps);
        }

        private static void RunNext(Queue<Action<Action>> steps)
        {
            if (steps.Count == 0)
            {
                Logger.Log($"All screenshots written to {ScreenshotSettings.OutputDirectory}");
                return;
            }

            var step = steps.Dequeue();
            step(() => RunNext(steps));
        }

        private static void CaptureAvatarConverterWindow(Action onDone)
        {
            var scope = new CaptureEnvironmentScope();
            var avatar = AvatarFixtures.InstantiateSimpleCubeAvatar();
            WindowScreenshotCapture.Capture<AvatarConverterWindow>(
                "convert-avatar.png",
                ScreenshotSettings.AvatarConverterWindowSize,
                handle => handle.SetFieldValue("targetRoot", avatar),
                onDone: () =>
                {
                    UnityEngine.Object.DestroyImmediate(avatar);
                    scope.Dispose();
                    onDone?.Invoke();
                });
        }

        private static void CaptureUnityQuestSettingsWindow(Action onDone)
        {
            var scope = new CaptureEnvironmentScope();
            WindowScreenshotCapture.Capture<UnityQuestSettingsWindow>(
                "set-up-environment.png",
                ScreenshotSettings.UnityQuestSettingsWindowSize,
                onDone: () =>
                {
                    scope.Dispose();
                    onDone?.Invoke();
                });
        }

        private static void CaptureBlendShapesCopyWindow(Action onDone)
        {
            var scope = new CaptureEnvironmentScope();
            var source = SampleContentFactory.CreateSampleSkinnedMeshRenderer("Body", "Smile");
            var target = SampleContentFactory.CreateSampleSkinnedMeshRenderer("Body (Mobile)", "Smile");
            WindowScreenshotCapture.Capture<BlendShapesCopyWindow>(
                "blendshapes-copy-window.png",
                ScreenshotSettings.BlendShapesCopyWindowSize,
                handle =>
                {
                    var model = handle.GetFieldValue<BlendShapesCopyViewModel>("model");
                    model.sourceMesh = source;
                    model.targetMesh = target;
                },
                onDone: () =>
                {
                    UnityEngine.Object.DestroyImmediate(source.sharedMesh);
                    UnityEngine.Object.DestroyImmediate(source.gameObject);
                    UnityEngine.Object.DestroyImmediate(target.sharedMesh);
                    UnityEngine.Object.DestroyImmediate(target.gameObject);
                    scope.Dispose();
                    onDone?.Invoke();
                });
        }

        private static void CaptureMSMapGenWindow(Action onDone)
        {
            var scope = new CaptureEnvironmentScope();
            var metallic = SampleContentFactory.CreateSampleTexture("Metallic", Color.gray);
            var smoothness = SampleContentFactory.CreateSampleTexture("Smoothness", Color.gray);
            WindowScreenshotCapture.Capture<MSMapGenWindow>(
                "metallic-smoothness-map-window.png",
                ScreenshotSettings.MSMapGenWindowSize,
                handle =>
                {
                    var model = handle.GetFieldValue<MSMapGenViewModel>("model");
                    model.metallicMap = metallic;
                    model.smoothnessMap = smoothness;
                },
                onDone: () =>
                {
                    UnityEngine.Object.DestroyImmediate(metallic);
                    UnityEngine.Object.DestroyImmediate(smoothness);
                    scope.Dispose();
                    onDone?.Invoke();
                });
        }

        private static void CapturePhysBonesRemoveWindow(Action onDone)
        {
            var scope = new CaptureEnvironmentScope();
            var avatar = AvatarFixtures.InstantiateSimpleCubeAvatarWithDynamics();
            var descriptor = avatar.GetComponent<VRC_AvatarDescriptor>();
            WindowScreenshotCapture.Capture<PhysBonesRemoveWindow>(
                "remove-physbones-window.png",
                ScreenshotSettings.PhysBonesRemoveWindowSize,
                handle =>
                {
                    var model = handle.GetFieldValue<PhysBonesRemoveViewModel>("model");
                    model.SelectAvatar(descriptor);
                },
                onDone: () =>
                {
                    UnityEngine.Object.DestroyImmediate(avatar);
                    scope.Dispose();
                    onDone?.Invoke();
                });
        }

        private static void CaptureAvatarConverterSettings(Action onDone)
        {
            var scope = new CaptureEnvironmentScope();
            var foldoutScope = new AvatarConverterSettingsFoldoutScope();
            ComponentScreenshotCapture.Capture<AvatarConverterSettings>(
                "avatar-converter-settings.png",
                ScreenshotSettings.AvatarConverterSettingsSize,
                onDone: () =>
                {
                    foldoutScope.Dispose();
                    scope.Dispose();
                    onDone?.Invoke();
                });
        }

        private static void CaptureMaterialSwap(Action onDone)
        {
            var scope = new CaptureEnvironmentScope();
            var originalMaterial = SampleContentFactory.CreateSampleMaterial("Body (PC)");
            var replacementMaterial = SampleContentFactory.CreateSampleMaterial("Body (Mobile)", "VRChat/Mobile/Toon Lit");
            ComponentScreenshotCapture.Capture<MaterialSwap>(
                "material-swap.png",
                ScreenshotSettings.MaterialSwapSize,
                populate: c =>
                {
                    c.materialMappings.Add(new MaterialSwap.MaterialMapping
                    {
                        originalMaterial = originalMaterial,
                        replacementMaterial = replacementMaterial,
                    });

                    // The Material Mappings list foldout defaults collapsed (SerializedProperty.isExpanded),
                    // so force it open or the added entry won't be visible in the screenshot.
                    var so = new SerializedObject(c);
                    so.FindProperty("materialMappings").isExpanded = true;
                    so.ApplyModifiedProperties();
                },
                onDone: () =>
                {
                    UnityEngine.Object.DestroyImmediate(originalMaterial);
                    UnityEngine.Object.DestroyImmediate(replacementMaterial);
                    scope.Dispose();
                    onDone?.Invoke();
                });
        }

        private static void CaptureMaterialConversionSettings(Action onDone)
        {
            var scope = new CaptureEnvironmentScope();
            var material = SampleContentFactory.CreateSampleMaterial("Body");
            ComponentScreenshotCapture.Capture<MaterialConversionSettings>(
                "material-conversion-settings.png",
                ScreenshotSettings.MaterialConversionSettingsSize,
                populate: c => c.additionalMaterialConvertSettings = new[] { SampleContentFactory.CreateAdditionalMaterialConvertSettings(material) },
                onDone: () =>
                {
                    UnityEngine.Object.DestroyImmediate(material);
                    scope.Dispose();
                    onDone?.Invoke();
                });
        }

        private static void CaptureMenuIconResizer(Action onDone) =>
            CaptureComponent<MenuIconResizer>(
                "menu-icon-resizer.png",
                ScreenshotSettings.MenuIconResizerSize,
                c => c.resizeModeAndroid = MenuIconResizer.TextureResizeMode.Max128x128,
                onDone);

        private static void CaptureMeshFlipper(Action onDone) =>
            CaptureComponent<MeshFlipper>("mesh-flipper.png", ScreenshotSettings.MeshFlipperSize, null, onDone);

        private static void CapturePlatformComponentRemover(Action onDone) =>
            CaptureComponent<PlatformComponentRemover>(
                "platform-component-remover.png",
                ScreenshotSettings.PlatformComponentRemoverSize,
                c =>
                {
                    // Rename off the internal temp-GameObject name so the Component field shown in
                    // the "Components to Keep" list looks like a real avatar bone, not an
                    // implementation detail of this screenshot tool.
                    c.gameObject.name = "Hair";
                    var physBone = c.gameObject.AddComponent<VRCPhysBone>();

                    // Pre-seed componentSettings before the Inspector's own UpdateComponentSettings()
                    // runs on first draw; it preserves entries that already reference a known
                    // component, so this survives instead of resetting to the false/false default.
                    // Matches the component's own documented example: keep on PC, remove on Mobile.
                    c.componentSettings = new[]
                    {
                        new KRT.VRCQuestTools.Models.PlatformComponentRemoverItem { component = physBone, removeOnAndroid = true },
                    };
                },
                onDone);

        private static void CapturePlatformGameObjectRemover(Action onDone) =>
            CaptureComponent<PlatformGameObjectRemover>(
                "platform-gameobject-remover.png",
                ScreenshotSettings.PlatformGameObjectRemoverSize,
                c => c.removeOnAndroid = true,
                onDone);

        private static void CapturePlatformTargetSettings(Action onDone) =>
            CaptureComponent<PlatformTargetSettings>(
                "platform-target-settings.png",
                ScreenshotSettings.PlatformTargetSettingsSize,
                c => c.buildTarget = KRT.VRCQuestTools.Models.BuildTarget.Android,
                onDone);

        private static void CaptureVertexColorRemover(Action onDone) =>
            CaptureComponent<VertexColorRemover>(
                "vertex-color-remover.png",
                ScreenshotSettings.VertexColorRemoverSize,
                c => c.includeChildren = true,
                onDone);

        private static void CaptureComponent<TComponent>(string fileName, Vector2 size, Action<TComponent> populate, Action onDone)
            where TComponent : Component
        {
            var scope = new CaptureEnvironmentScope();
            ComponentScreenshotCapture.Capture<TComponent>(
                fileName,
                size,
                populate,
                onDone: () =>
                {
                    scope.Dispose();
                    onDone?.Invoke();
                });
        }
    }
}
