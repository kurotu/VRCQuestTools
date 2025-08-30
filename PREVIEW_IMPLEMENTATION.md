# PhysBone Preview Drawing - Implementation Summary

## 🎯 Feature Overview

This implementation adds visual preview functionality to VRCQuestTools, allowing users to see the area of effect for PhysBones, Colliders, and Contacts when hovering over them in the AvatarDynamicsSelector and PhysBoneRemover windows.

## 📋 How to Use

### 1. Open Target Window
- **Avatar Dynamics Selector**: `Window → VRCQuestTools → Avatar Dynamics Selector`
- **PhysBones Remover**: `Window → VRCQuestTools → PhysBones Remover`

### 2. Hover Over Components
- Move your mouse cursor over any PhysBone, Collider, or Contact in the component list
- **Wireframe previews will automatically appear in the Scene view**

### 3. Preview Types
- **PhysBones**: Blue wireframe spheres at each transform connected by lines
- **Colliders**: Type-specific wireframe shapes (sphere/capsule/plane)
- **Contacts**: Wireframe spheres showing interaction radius

### 4. Testing
Use the new menu items under `VRCQuestTools/Test/` to verify functionality:
- `Initialize Preview Service` - Tests core service functionality
- `Open Avatar Dynamics Selector` - Opens selector with preview enabled
- `Open PhysBones Remove Window` - Opens remover with preview enabled

## 🔧 Technical Implementation

### Core Components

1. **AvatarDynamicsPreviewService** - Central drawing service
2. **Enhanced EditorGUIUtility** - Hover detection in component lists
3. **Window Integration** - Automatic service lifecycle management

### Key Features

- ✅ Real-time hover detection
- ✅ Component-specific wireframe drawing
- ✅ Proper Unity scene view integration
- ✅ Memory leak prevention with proper cleanup
- ✅ Separation from VRCPhysBoneProviderBase
- ✅ Unit test coverage

## 🎨 Visual Behavior

```
[Component List]          [Scene View]
┌─────────────────┐      ┌──────────────────┐
│ ☐ PhysBone_A   ←───────→ ○───○───○ (blue) │
│ ☐ PhysBone_B    │      │                  │
│ ☐ Collider_A    │      │   (wireframe     │
│ ☐ Contact_A     │      │    preview)      │
└─────────────────┘      └──────────────────┘
     ^hover here            appears here
```

## 📝 Files Modified/Created

### Created:
- `Editor/Services/AvatarDynamicsPreviewService.cs` - Main preview service
- `Editor/Utils/PreviewServiceValidation.cs` - Testing utilities
- `Assets/VRCQuestTools-Tests/Editor/Services/AvatarDynamicsPreviewServiceTests.cs` - Unit tests

### Modified:
- `Editor/Views/EditorGUIUtility.cs` - Added hover detection
- `Editor/Views/AvatarDynamicsSelectorWindow.cs` - Service integration
- `Editor/Views/PhysBonesRemoveWindow.cs` - Service integration

The implementation provides exactly the functionality requested in issue #130 and is ready for use!