# VRCQuestTools Code Style and Conventions

## General Guidelines
- **License Header**: All C# files must include MIT license header with copyright notice
- **Encoding**: UTF-8 with BOM for C# files
- **Line Endings**: LF (Unix-style) - configured in .editorconfig
- **Trailing Whitespace**: Must be trimmed
- **Final Newline**: Required in all files

## C# Code Style
- **Indentation**: 4 spaces (no tabs)
- **Naming Convention**: PascalCase for types, methods, properties
- **Namespace**: `KRT.VRCQuestTools` as root namespace
- **XML Documentation**: Required for public APIs using /// comments
- **Access Modifiers**: Explicit specification required

## Code Organization
- **Assembly Definitions**: Use .asmdef files to organize code
- **Editor Code**: Separate from runtime code using `Editor/` folders
- **Tests**: Located in `Assets/VRCQuestTools-Tests/` with NUnit framework
- **Utilities**: Helper classes in `Utils/` subdirectories

## Roslyn Analyzers
- Enabled for code quality enforcement
- Some rules disabled (e.g., RCS1194, S101) 
- Unity-specific analyzers enabled (UNT0007, UNT0008, UNT0023)
- Warning level for unnecessary using directives (IDE0005)

## File Structure Example
```csharp
// <copyright file="ClassName.cs" company="kurotu">
// Copyright (c) kurotu.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

using System;
using UnityEngine;

namespace KRT.VRCQuestTools.Models
{
    /// <summary>
    /// Class description.
    /// </summary>
    internal class ClassName
    {
        /// <summary>
        /// Method description.
        /// </summary>
        /// <param name="parameter">Parameter description.</param>
        /// <returns>Return value description.</returns>
        internal bool MethodName(string parameter)
        {
            // Implementation
        }
    }
}
```