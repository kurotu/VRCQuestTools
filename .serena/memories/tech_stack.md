# VRCQuestTools Tech Stack

## Development Environment
- **Unity Version**: 2022.3.22f1
- **C# Version**: 9.0
- **.NET SDK**: 9.0.304 (with latest feature rollforward)
- **Platform**: Windows (PowerShell scripts)

## Dependencies
- **VRChat SDK**: VRCSDKBase, VRCSDK3A, VRC.SDK3.Dynamics.PhysBone
- **Unity Packages**: Various editor and runtime packages
- **Third-party Dependencies**: 
  - Newtonsoft.Json
  - NUnit (for testing)
  - Various VRChat ecosystem packages (Modular Avatar, NDMF, etc.)

## Project Structure
- **Main Package**: `Packages/com.github.kurotu.vrc-quest-tools/`
  - `Editor/`: Editor scripts organized by functionality
  - `Runtime/`: Runtime components
  - `Shader/`: Custom shaders
  - `ComputeShader/`: Compute shaders
- **Tests**: `Assets/VRCQuestTools-Tests/`
- **Debug Utilities**: `Assets/VRCQuestTools-DebugUtil/`
- **Documentation**: `Website/` (Docusaurus-based)

## Build System
- **Solution File**: `VRCQuestTools.sln`
- **Multiple Project Files**: VRCQuestTools*.csproj files for different components
- **Analyzers**: Roslyn Analyzers enabled (`VRCQuestTools-Analyzers.csproj`)

## Documentation
- **Framework**: Docusaurus 3.8.1
- **Languages**: English and Japanese (i18n support)
- **Tech Stack**: React 19, TypeScript 5.6, Node.js 18+