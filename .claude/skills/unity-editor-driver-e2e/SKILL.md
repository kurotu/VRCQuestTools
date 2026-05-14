---
name: unity-editor-driver-e2e
description: >
  Implement E2E tests for Unity Editor extensions (EditorWindow and custom Inspector) using
  the unity-editor-driver framework in this repository. Use this skill whenever you need to
  write, fix, or extend EditMode E2E tests that interact with editor UI controls —
  even if the request only mentions "test", "automate inspector", "UI test", "check window
  behavior", or similar phrases. The framework works non-invasively: no changes to the target
  window or inspector code are ever required.
---

# unity-editor-driver E2E Testing

EditorDriver lets you automate and assert on Unity Editor windows and custom Inspectors without
touching the target code. Tests live in EditMode test assemblies and run via Unity Test Runner.

## Table of Contents
1. [Assembly Setup](#assembly-setup)
2. [Test Structure](#test-structure)
3. [Opening Windows and Inspectors](#opening-windows-and-inspectors)
4. [Discovering Controls with Page.Scan](#discovering-controls-with-pagescan)
5. [Manual Control Descriptor with Page.Describe](#manual-control-descriptor-with-pagedescribe)
6. [Locator API](#locator-api)
7. [Interacting with Dynamic UIs](#interacting-with-dynamic-uis)
8. [State Verification Patterns](#state-verification-patterns)
9. [UIElements-Specific Notes](#uielements-specific-notes)
10. [Screenshots](#screenshots)
11. [Cross-Platform Notes](#cross-platform-notes)

---

## Assembly Setup

Create an `.asmdef` for your test assembly. Reference `EditorDriver.Editor` (and your target
window/component assemblies). Enable `UNITY_INCLUDE_TESTS` so the assembly is only included
in test builds.

```json
{
  "name": "MyPlugin.E2ETests.Editor",
  "rootNamespace": "MyPlugin.Tests",
  "references": [
    "EditorDriver.Editor",
    "MyPlugin.Editor",
    "MyPlugin.Runtime"
  ],
  "includePlatforms": ["Editor"],
  "overrideReferences": true,
  "precompiledReferences": ["nunit.framework.dll"],
  "autoReferenced": false,
  "defineConstraints": ["UNITY_INCLUDE_TESTS"]
}
```

---

## Test Structure

Every E2E test fixture follows the same shape:

```csharp
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using EditorDriver;

[TestFixture]
public class MyWindowE2ETests
{
    private Driver _driver;

    [SetUp]
    public void SetUp()
    {
        _driver = new Driver();
    }

    [TearDown]
    public void TearDown()
    {
        _driver?.Dispose(); // closes all windows and destroys GameObjects
    }

    [UnityTest]
    public IEnumerator SomeTest()
    {
        var handle = _driver.OpenWindow<MyWindow>();
        yield return null; // let Unity process the window's first OnGUI

        // ... interact and assert ...
    }
}
```

Key rules:
- Use `[UnityTest]` with `IEnumerator` for any test that needs an OnGUI frame.
- Always `yield return null` immediately after opening a window or inspector.
- Always call `_driver.Dispose()` (or `CloseAll()`) in `[TearDown]` — never rely on
  the test framework to clean up Unity objects.

---

## Opening Windows and Inspectors

### EditorWindows

```csharp
// Opens a fresh instance (never reuses an existing window)
var handle = _driver.OpenWindow<MyWindow>();
var window = handle.Window; // the EditorWindow instance
```

### Custom Inspectors

```csharp
// Creates a temporary GameObject, adds TComponent, opens the default Editor in an isolated window
var handle = _driver.OpenInspector<MyComponent>();

// With an explicit Editor type
var handle = _driver.OpenInspector<MyComponent, MyComponentEditor>();

// InspectorHandle properties
handle.Window    // EditorWindow hosting the inspector
handle.Editor    // UnityEditor.Editor instance
handle.Component // the Component on the temporary GameObject
handle.GameObject // the temporary GameObject
```

### Cleanup

```csharp
_driver.CloseWindow(windowHandle);        // close a specific window
_driver.CloseInspector(inspectorHandle);  // close inspector and destroy its GameObject
_driver.CloseAll();                       // close everything (also called by Dispose)
```

---

## Discovering Controls with Page.Scan

`Page.Scan` is the recommended approach. It auto-discovers all interactive controls without any
manual descriptor. It works for both IMGUI and UIElements windows.

```csharp
var handle = _driver.OpenWindow<MyWindow>();
yield return null;

var page = Page.Scan(handle.Window);
```

### Querying controls

```csharp
// By displayed text value (IMGUI and UIElements)
var locator = page.GetByValue("Hello World");

// By label text — works with UIElements; also works with IMGUI via Page.Describe
var locator = page.GetByLabel("Text Field");

// By type and zero-based index
var firstToggle  = page.GetByType(ControlType.Toggle, 0);
var secondToggle = page.GetByType(ControlType.Toggle, 1);

// All discovered controls
IReadOnlyList<ControlInfo> all = page.Controls;
```

`GetByValue` / `GetByLabel` / `GetByType` all throw `InvalidOperationException` when the
control is not found — use this as an implicit assertion.

---

## Manual Control Descriptor with Page.Describe

Use `Page.Describe` when you know the window's layout in advance and prefer label-based access.
This works for both IMGUI and UIElements; the resolver is auto-detected.

```csharp
var page = Page.Describe(handle.Window, p =>
{
    p.Label("Base Settings", EditorStyles.boldLabel);
    p.TextField("Text Field");
    p.BeginToggleGroup("Optional Settings");
    p.Toggle("Inner Toggle");
    p.Slider("Slider", -3f, 3f);
    p.EndToggleGroup();
    p.Button("Apply");
    p.IntField("Count");
    p.FloatField("Scale");
    p.ObjectField("Target");
});

page.GetByLabel("Text Field").Fill("new value");
```

Available descriptor methods: `Label`, `TextField`, `Toggle`, `Slider`, `BeginToggleGroup`,
`EndToggleGroup`, `Button`, `IntField`, `FloatField`, `ObjectField`.

---

## Locator API

All query methods return a `Locator`. Use it to interact with or inspect a single control.

| Method | Description |
|--------|-------------|
| `ReadText()` | Reads the displayed text (via clipboard SelectAll→Copy) |
| `Fill(string)` | Clears the field and types the given text |
| `Toggle()` | Clicks a checkbox or toggle control |
| `SetSlider(float)` | Types a value into the slider's numeric field |
| `Click()` | Clicks the center of the control |
| `CaptureScreenshot()` | Returns a `Texture2D` cropped to the control area |
| `.ControlType` | The `ControlType` enum value |
| `.Rect` | The `Rect` of the control in screen space |

After `Fill`, `Toggle`, or `SetSlider`, always repaint and yield before asserting:

```csharp
page.GetByValue("Hello World").Fill("New Text");
handle.Repaint();
yield return null;

page.Refresh(); // re-discover controls after UI change
Assert.AreEqual("New Text", page.GetByValue("New Text").ReadText());
```

---

## Interacting with Dynamic UIs

When the UI changes (e.g. a toggle group expands to reveal more controls), call
`page.Refresh()` after the change to re-discover the new state. Controls inside a disabled
group are not discoverable until the group is enabled.

```csharp
var page = Page.Scan(handle.Window);
int countBefore = page.Controls.Count;

// Enable a toggle group (toggle at index 1 in this example)
page.GetByType(ControlType.Toggle, 1).Toggle();
handle.Repaint();
yield return null;

page.Refresh(); // picks up newly active controls

Assert.Greater(page.Controls.Count, countBefore);

// Inner slider is now reachable
var slider = page.GetByValue("1.23");
slider.Fill("2.5");
```

---

## State Verification Patterns

### EditorWindow — UI→State and State→UI

`WindowHandle` exposes reflection helpers for white-box state assertions:

```csharp
// UI → State: interact, then read internal field
page.GetByValue("Hello World").Fill("Updated");
handle.Repaint();
yield return null;

string val = handle.GetFieldValue<string>("myString");
Assert.AreEqual("Updated", val);

// State → UI: write internal field, then verify it appears in the UI
handle.SetFieldValue("myString", "From Code");
handle.Repaint();
yield return null;

page.Refresh();
Assert.AreEqual("From Code", page.GetByValue("From Code").ReadText());
```

### IMGUI Inspector — Component field access

For IMGUI inspectors, cast `handle.Component` and read/write public fields directly:

```csharp
var component = (MyComponent)handle.Component;

// UI → State
page.GetByValue("Hello World").Fill("Inspector Test");
handle.Repaint();
yield return null;
Assert.AreEqual("Inspector Test", component.myString);

// State → UI
component.myString = "From Code";
handle.Repaint();
yield return null;
page.Refresh();
Assert.AreEqual("From Code", page.GetByValue("From Code").ReadText());
```

### UIElements Inspector — SerializedObject sync required

UIElements inspectors use `BindProperty`, so after a direct field change you must notify
the binding system via `serializedObject.Update()` before repainting:

```csharp
var component = (MyUIElementsComponent)handle.Component;

// State → UI (UIElements binding requires serializedObject.Update)
component.myString = "From Code";
handle.Editor.serializedObject.Update();
handle.Repaint();
yield return WaitForBindings();

page.Refresh();
Assert.AreEqual("From Code", page.GetByLabel("Text Field").ReadText());
```

---

## UIElements-Specific Notes

### Waiting for bindings

UIElements bindings may not resolve in a single frame. For read operations on UIElements
inspectors, yield multiple frames before scanning:

```csharp
private static IEnumerator WaitForBindings()
{
    yield return null;
    yield return null;
    yield return null;
}

[UnityTest]
public IEnumerator ReadUIElementsField()
{
    var handle = _driver.OpenInspector<MyUIElementsComponent>();
    yield return WaitForBindings(); // instead of a single yield return null

    var page = Page.Scan(handle.Window);
    Assert.AreEqual("Hello World", page.GetByLabel("Text Field").ReadText());
}
```

### Label-based lookup available

Unlike IMGUI Scan (value-only), UIElements Scan exposes labels from the visual tree, so
`GetByLabel` works out of the box:

```csharp
// UIElements windows support both:
page.GetByLabel("Text Field").Fill("new");
page.GetByValue("Hello World").ReadText();
```

### InspectorHandle.Repaint()

On Linux, call `handle.Repaint()` before reading UIElements state after mutating serialized
data. It walks `IBindable` elements and forces `binding.PreUpdate()`/`binding.Update()`,
which avoids stale bindings.

---

## Screenshots

Always build paths with `Path.Combine` and `Path.GetTempPath()` for cross-platform safety.
Clean up in `[TearDown]`.

```csharp
using System.IO;

[SetUp]
public void SetUp()
{
    _driver = new Driver();
    _screenshotDir = Path.Combine(Path.GetTempPath(), "E2E_" + System.Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(_screenshotDir);
}

[TearDown]
public void TearDown()
{
    _driver?.Dispose();
    if (Directory.Exists(_screenshotDir))
        Directory.Delete(_screenshotDir, recursive: true);
}

[UnityTest]
public IEnumerator TakeScreenshot()
{
    var handle = _driver.OpenWindow<MyWindow>();
    yield return null;

    // Whole window
    var path = Path.Combine(_screenshotDir, "window.png");
    handle.Screenshot(path);
    Assert.IsTrue(File.Exists(path));

    // Individual control
    var page = Page.Scan(handle.Window);
    var tex = page.GetByValue("Hello World").CaptureScreenshot();
    Assert.Greater(tex.width, 0);
    UnityEngine.Object.DestroyImmediate(tex);
}
```

---

## Cross-Platform Notes

- **Window positioning**: Use `EditorWindowExtensions.SetPosition()` instead of assigning
  `EditorWindow.position` directly. On Linux, the window manager applies position changes
  asynchronously; `SetPosition` uses a min/max-size trick to make resizes deterministic.

- **UIElements on Linux**: Call `handle.Repaint()` before reading UIElements state after
  mutating serialized data. This forces `IBindable.binding.PreUpdate()`/`Update()` and
  avoids stale bindings.

- **File paths**: Always use `Path.Combine(...)` — never concatenate with `\` or `/` directly.

---

## Running Tests

```bash
# Run all tests in the E2E assembly
uloop run-tests --filter-type assembly --filter-value "MyPlugin.E2ETests.Editor"

# Run a single test by full name
uloop run-tests --filter-type exact --filter-value "MyPlugin.Tests.MyWindowE2ETests.SomeTest"
```

See `Assets/Tests/Editor/` for complete worked examples covering IMGUI windows, IMGUI
inspectors, UIElements windows, and UIElements inspectors — including bidirectional state
verification patterns.
