# Task Completion Checklist for VRCQuestTools

## Code Quality Checks
1. **Run Analyzers**: Execute `.\scripts\lint.ps1` or use VS Code lint task
   - Ensures all Roslyn Analyzers pass
   - Builds solution files and VRCQuestTools projects
   - Check build.log for any issues

2. **Code Style Verification**:
   - Verify proper license headers in new/modified C# files
   - Check indentation (4 spaces, no tabs)
   - Ensure XML documentation for public APIs
   - Verify namespace usage (`KRT.VRCQuestTools`)

3. **Build Verification**:
   - `dotnet build VRCQuestTools.sln` should complete without errors
   - All VRCQuestTools*.csproj projects should build successfully

## Testing
1. **Unit Tests**: Run tests via Unity Test Runner
   - Navigate to Window > General > Test Runner in Unity
   - Execute all tests in VRCQuestTools-Tests assembly
   - Ensure all tests pass

2. **Integration Testing**: 
   - Test functionality in Unity environment
   - Verify avatar conversion workflows work correctly

## Documentation Updates
1. **Code Documentation**: Update XML comments for new public APIs
2. **Website Documentation**: Update `Website/docs/` if adding user-facing features
3. **Changelog**: Update `CHANGELOG.md` and `CHANGELOG_JP.md` for significant changes

## Pre-Commit Checks
1. Verify no build errors or warnings
2. Run linting to ensure code quality standards
3. Check that all new files have proper license headers
4. Ensure .editorconfig rules are followed
5. Test in Unity environment if making runtime changes

## Release Preparation (if applicable)
1. Update version numbers in package.json files
2. Update documentation website
3. Run full test suite
4. Verify VPM package compatibility
5. Test installation and basic functionality