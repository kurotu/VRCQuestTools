# VRCQuestTools Instructions

## Structure

### Unity Project

#### Environments

- Unity 2022.3.22f1
- C# 9.0

#### Source Codes

C# Source codes for VRCQuestTools can be found in the following directories:

- Assets
    - VRCQuestTools-DebugUtil: Debug utilities.
    - VRCQuestTools-Tests: Unit tests.
- Packages
    - com.github.kurotu.vrc-quest-tools: Main package.
        - Editor: Editor scripts.
            - Automators: Scripts for automating tasks.
            - I18n: Internationalization support.
            - Inspector: Custom inspectors.
            - Menus: Custom menus.
            - Models: Data models for the project.
            - NDMF: Non-Destructive Modular Framework plugin.
            - Services: Backend services for the project.
            - Utils: Utility scripts and functions.
            - ViewModels: View models for the project.
            - Views: Editor UI components for the project.
        - Runtime: Runtime scripts such as components.
        - Shaders: Shader files for the project.
    - com.github.kurotu.vrc-quest-tools-analyzers: Roslyn Analyzers.

Other assets and packages are not part of the VRCQuestTools source code and should not be modified.

#### Code Quality

This project has Roslyn Analyzers enabled to enforce code quality and consistency.
However, existing code may not fully comply with these rules, and developers should be aware of this when making changes.

You can run analyzers by executing `scripts/lint.ps1` or `scripts/lint.sh`.

#### Testing

This project uses NUnit for testing. Tests are located in the `Assets/VRCQuestTools-Tests` directory.
To run the tests, open the Unity Test Runner window and execute all tests.
Note: Test execution may require a specific Unity environment setup.

#### Documentation Policy

- Instruction files are not needed. If testing instructions are necessary, write them as comments in test code.
- Do not create separate instruction or guide files for development or testing purposes.

### Documentation

The documentation for VRCQuestTools is located in the `Website` directory.
It's built using Docusaurus.

## Code of Conduct

- Do not commit to the git repository.
