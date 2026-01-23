#!/usr/bin/env pwsh

# Build solution files quietly
Get-ChildItem -Path "*.sln" | ForEach-Object {
    dotnet build $_.Name > build.log
    if ($LASTEXITCODE -ne 0) {
        Get-Content build.log
        exit 1
    }
}

# Build VRCQuestTools project files with normal verbosity
Get-ChildItem -Path "VRCQuestTools*.csproj" | ForEach-Object {
    echo "Building project: $_.Name"
    dotnet build $_.Name --no-incremental --no-dependencies --verbosity quiet
}
