@echo off

if "%1" == "--fix" (
    echo dotnet style fix -r stylecop.ruleset -j stylecop.json Packages/com.github.kurotu.vrc-quest-tools Assets/VRCQuestTools-Tests Assets/VRCQuestTools-DebugUtil
    dotnet style fix -r stylecop.ruleset -j stylecop.json Packages/com.github.kurotu.vrc-quest-tools Assets/VRCQuestTools-Tests Assets/VRCQuestTools-DebugUtil
) else (
    echo dotnet style check -r stylecop.ruleset -j stylecop.json Packages/com.github.kurotu.vrc-quest-tools Assets/VRCQuestTools-Tests Assets/VRCQuestTools-DebugUtil
    dotnet style check -r stylecop.ruleset -j stylecop.json Packages/com.github.kurotu.vrc-quest-tools Assets/VRCQuestTools-Tests Assets/VRCQuestTools-DebugUtil
)
