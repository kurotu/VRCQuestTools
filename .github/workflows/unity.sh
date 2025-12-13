#!/bin/env bash
set -eu

## Use all arguments to this script as Unity command line arguments
unity-editor -username "${UNITY_EMAIL}" -password "${UNITY_PASSWORD}" -serial "${UNITY_SERIAL}" "$@"
