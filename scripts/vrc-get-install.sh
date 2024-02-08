#!/bin/bash
set -eu

PACKAGE=$1
VERSION=$2
if [ "${VERSION}" = "latest" ]; then
  VERSION=""
fi

# shellcheck disable=SC2086
vrc-get install -y "${PACKAGE}" ${VERSION}
