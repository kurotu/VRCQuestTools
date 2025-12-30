#!/bin/env bash
dotnet build *.sln > build.log || { cat build.log; exit 1; }
ls VRCQuestTools*.csproj | while read -r project; do
    echo "Building project: $project..."
    dotnet build "$project" --no-incremental --no-dependencies --verbosity quiet
done
