#!/usr/bin/env pwsh

# Remove previous build log if it exists
if (Test-Path build.log) {
    Remove-Item build.log
}

# Build solution files quietly
Get-ChildItem -Path "*.sln" | ForEach-Object {
    echo "Building solution: $_.Name" >> build.log
    dotnet build $_.Name >> build.log
}

# Build VRCQuestTools project files with normal verbosity
Get-ChildItem -Path "VRCQuestTools*.csproj" | ForEach-Object {
    dotnet build $_.Name --no-incremental --no-dependencies --verbosity quiet
}
