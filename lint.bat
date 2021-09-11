@echo off

if "%1" == "--fix" (
    echo dotnet style fix -r stylecop.ruleset -j stylecop.json .
    dotnet style fix -r stylecop.ruleset -j stylecop.json .
) else (
    echo dotnet style check -r stylecop.ruleset -j stylecop.json .
    dotnet style check -r stylecop.ruleset -j stylecop.json .
)
