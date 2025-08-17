# VRCQuestTools Instructions

## Structure

### Unity Project

C# Source codes for VRCQuestTools can be found in the following directories:

- Assets/VRCQuestTools-DebugUtil
- Assets/VRCQuestTools-Tests
- Packages/com.github.kurotu.vrc-quest-tools

Other assets and packages are not part of the VRCQuestTools source code and should not be modified.

#### Code Quality

This project has Roslyn Analyzers enabled to enforce code quality and consistency.
However, existing code may not fully comply with these rules, and developers should be aware of this when making changes.

#### Testing

This project uses NUnit for testing. Tests are located in the `Assets/VRCQuestTools-Tests` directory.
To run the tests, open the Unity Test Runner window and execute all tests.
Note: Test execution may require a specific Unity environment setup.

### Documentation

The documentation for VRCQuestTools is located in the `Website` directory.
It's built using Docusaurus.

## Code of Conduct

- Do not commit to the git repository.
