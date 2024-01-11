#!/bin/bash
set -eu

function printVersionBody() {
    local FILE=$1
    local VERSION=$2
    sed -n "/^## \[${VERSION}\]/,/^## /p" "${FILE}" \
      | tail -n +2 \
      | head -n -1
}

VERSION="${1}"

echo "## What's Changed"
printVersionBody CHANGELOG.md "${VERSION}"
echo '## 変更点'
printVersionBody CHANGELOG_JP.md "${VERSION}"
