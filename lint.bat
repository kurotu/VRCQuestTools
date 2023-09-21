@echo off

if "%1" == "--fix" (
    echo dotnet style fix -r stylecop.ruleset -j stylecop.json Packages/com.github.kurotu.vrc-quest-tools Assets/VRCQuestTools-Tests
    dotnet style fix -r stylecop.ruleset -j stylecop.json Packages/com.github.kurotu.vrc-quest-tools Assets/VRCQuestTools-Tests
) else (
    echo dotnet style check -r stylecop.ruleset -j stylecop.json Packages/com.github.kurotu.vrc-quest-tools Assets/VRCQuestTools-Tests
    dotnet style check -r stylecop.ruleset -j stylecop.json Packages/com.github.kurotu.vrc-quest-tools Assets/VRCQuestTools-Tests
)
