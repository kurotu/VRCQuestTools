# VRCQuestTools Codebase Structure

## Root Directory Structure
```
VRCQuestTools/
├── .github/          # GitHub workflows and configuration
├── .serena/          # Serena MCP configuration
├── .vscode/          # VS Code settings and tasks
├── Assets/           # Unity Assets folder
│   ├── VRCQuestTools-DebugUtil/  # Debug utilities
│   └── VRCQuestTools-Tests/      # Unit tests (NUnit)
├── Packages/         # Unity Packages
│   ├── com.github.kurotu.vrc-quest-tools/     # Main package
│   ├── com.github.kurotu.vrc-quest-tools-analyzers/  # Roslyn analyzers
│   └── [other VRChat ecosystem packages]
├── ProjectSettings/  # Unity project settings
├── scripts/          # Build and utility scripts
├── Website/          # Docusaurus documentation
├── *.csproj         # Various project files for dependencies
├── VRCQuestTools.sln # Main solution file
└── global.json      # .NET SDK configuration
```

## Main Package Structure
```
Packages/com.github.kurotu.vrc-quest-tools/
├── Editor/           # Editor-only code
│   ├── Automators/   # Automation scripts
│   ├── I18n/         # Internationalization
│   ├── Inspector/    # Custom inspectors
│   ├── Menus/        # Unity menu items
│   ├── Models/       # Data models
│   ├── NDMF/         # Non-Destructive Modular Framework
│   ├── Services/     # Backend services
│   ├── Utils/        # Utility functions
│   ├── ViewModels/   # MVVM view models
│   └── Views/        # Editor UI components
├── Runtime/          # Runtime components
├── Shader/           # Custom shaders
├── ComputeShader/    # Compute shaders
└── package.json      # Package manifest
```

## Test Structure
```
Assets/VRCQuestTools-Tests/
├── Editor/           # Editor test scripts
│   ├── Inspector/    # UI component tests
│   ├── Models/       # Model tests
│   │   ├── Unity/    # Unity-specific model tests
│   │   └── VRChat/   # VRChat-specific model tests
│   └── Utils/        # Utility function tests
├── Fixtures/         # Test assets and data
│   ├── Animations/   # Animation test assets
│   ├── Materials/    # Material test assets
│   ├── Prefabs/      # Prefab test assets
│   ├── Scenes/       # Scene test assets
│   └── Textures/     # Texture test assets
└── Shader/           # Test shaders
```

## Key Assembly Definitions
- `VRCQuestTools` - Main runtime assembly
- `VRCQuestTools-Editor` - Editor assembly
- `VRCQuestTools-EditorTests` - Test assembly
- `VRCQuestTools-DebugUtil-Editor` - Debug utilities

## Dependencies and References
- VRChat SDK (VRCSDKBase, VRCSDK3A)
- Unity Editor APIs
- Newtonsoft.Json for JSON processing
- NUnit for testing framework
- Various VRChat ecosystem packages (Modular Avatar, NDMF, etc.)