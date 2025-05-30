#!/bin/bash
set -eu

REPO=kurotu/VRCQuestTools

# get all tags from github (excluding prereleases).
mapfile -t TAGS < <(curl -s "https://api.github.com/repos/${REPO}/tags?per_page=100" | jq -r '.[] | .name' | grep -v '-')

# print links to compare pages for each version.
# e.g. [1.1.0]: https://github.com/kurotu/VRCQuestTools/compare/1.0.0...1.1.0
BASE_URL="https://github.com/${REPO}"

echo ''
echo "[Unreleased]: ${BASE_URL}/compare/${TAGS[0]}...HEAD"
for i in "${!TAGS[@]}"; do
    CURRENT_VERSION=${TAGS[i]#v}
    if [ $((i + 1)) -eq "${#TAGS[@]}" ]; then
        echo "[${CURRENT_VERSION}]: ${BASE_URL}/releases/tag/${TAGS[i]}"
    else
        echo "[${CURRENT_VERSION}]: ${BASE_URL}/compare/${TAGS[i+1]}...${TAGS[$i]}"
    fi
done

# get all tags including prereleases.
mapfile -t ALL_TAGS < <(curl -s "https://api.github.com/repos/${REPO}/tags?per_page=100" | jq -r '.[] | .name' | grep -v 'dummy')
# print links to compare pages for each version including prereleases.
echo ''
for i in "${!ALL_TAGS[@]}"; do
    # skip non-prerelease tags.
    if [[ ! "${ALL_TAGS[i]}" =~ - ]]; then
        continue
    fi
    CURRENT_VERSION=${ALL_TAGS[i]#v}
    if [ $((i + 1)) -eq "${#ALL_TAGS[@]}" ]; then
        echo "[${CURRENT_VERSION}]: ${BASE_URL}/releases/tag/${ALL_TAGS[i]}"
    else
        echo "[${CURRENT_VERSION}]: ${BASE_URL}/compare/${ALL_TAGS[i+1]}...${ALL_TAGS[$i]}"
    fi
done
