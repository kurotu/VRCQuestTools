#!/bin/env bash
dotnet build *.sln > build.log || cat build.log
ls VRCQuestTools*.csproj | xargs -n 1 dotnet build --no-incremental --no-dependencies --verbosity quiet
