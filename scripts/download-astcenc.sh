#!/bin/bash
set -euo pipefail

# Downloads the astcenc CLI binaries used for ASTC texture compression and
# extracts them into Packages/com.github.kurotu.vrc-quest-tools/Editor/Tools/astcenc/.
# The binaries are not committed to the repository (see .gitignore); this
# script is the only way to (re)populate them for local development and CI.
#
# Usage: scripts/download-astcenc.sh [--platform windows|linux|all]

VERSION="5.3.0"
BASE_URL="https://github.com/ARM-software/astc-encoder/releases/download/${VERSION}"
TOOLS_DIR="Packages/com.github.kurotu.vrc-quest-tools/Editor/Tools/astcenc"

WIN_ARCHIVE="astcenc-${VERSION}-windows-x64.zip"
WIN_ARCHIVE_SHA256="199b2287be0264182292869798bef91c35a64791e52bfd43824d1d3ac3e7846f"
WIN_AVX2_SHA256="c2eb4bbbad344666ccd9c176f3b915077c057366681a3093c4047a7677e6504b"
WIN_SSE2_SHA256="02154aaab77770cf279d09629fd75c1ae912e3748067616a38a2d75137429cdd"

LINUX_ARCHIVE="astcenc-${VERSION}-linux-x64.zip"
LINUX_ARCHIVE_SHA256="495b2f0cf0357ae05728a727e3d0e81d6e7f27b242c21cb5ef6254dd56dba5ff"
LINUX_AVX2_SHA256="eaa0d194e82790bd338ef00e69e5d085ae6a2134da30bf7b3f186b78fb19f5cb"
LINUX_SSE2_SHA256="61071c177f3c4b873097a223283be9b47d9c14f6a0f3de34d2e7c97019344300"

PLATFORM="all"
while [ $# -gt 0 ]; do
  case "$1" in
    --platform)
      PLATFORM="${2:-}"
      shift 2
      ;;
    --platform=*)
      PLATFORM="${1#*=}"
      shift
      ;;
    *)
      echo "Unknown argument: $1" >&2
      exit 1
      ;;
  esac
done

case "$PLATFORM" in
  windows | linux | all) ;;
  *)
    echo "Invalid --platform: ${PLATFORM} (expected windows|linux|all)" >&2
    exit 1
    ;;
esac

TMP_ROOT="$(mktemp -d)"
trap 'rm -rf "$TMP_ROOT"' EXIT

sha256_of() {
  sha256sum "$1" | awk '{print $1}'
}

# Returns 0 (true) when the given file already has the expected SHA256.
has_sha256() {
  local file="$1" expected="$2"
  [ -f "$file" ] && [ "$(sha256_of "$file")" = "$expected" ]
}

verify_sha256() {
  local file="$1" expected="$2" actual
  actual="$(sha256_of "$file")"
  if [ "$actual" != "$expected" ]; then
    echo "SHA256 mismatch for $file" >&2
    echo "  expected: $expected" >&2
    echo "  actual:   $actual" >&2
    exit 1
  fi
}

# download_platform <label> <archive> <archive-sha256> <out-dir> <avx2-name> <avx2-sha256> <sse2-name> <sse2-sha256> <chmod:0|1>
download_platform() {
  local label="$1" archive="$2" archive_sha="$3" out_dir="$4"
  local avx2_name="$5" avx2_sha="$6" sse2_name="$7" sse2_sha="$8" needs_chmod="$9"
  local dest="${TOOLS_DIR}/${out_dir}"

  if has_sha256 "${dest}/${avx2_name}" "$avx2_sha" && has_sha256 "${dest}/${sse2_name}" "$sse2_sha"; then
    echo "astcenc (${label}): already up to date, skipping."
    return 0
  fi

  echo "astcenc (${label}): downloading ${archive}..."
  local work="${TMP_ROOT}/${out_dir}"
  mkdir -p "$work"
  local archive_path="${work}/${archive}"
  curl -fsSL -o "$archive_path" "${BASE_URL}/${archive}"
  verify_sha256 "$archive_path" "$archive_sha"

  local extract_dir="${work}/extracted"
  mkdir -p "$extract_dir"
  unzip -oq "$archive_path" "bin/${avx2_name}" "bin/${sse2_name}" -d "$extract_dir"

  mkdir -p "$dest"
  cp "${extract_dir}/bin/${avx2_name}" "${dest}/${avx2_name}"
  cp "${extract_dir}/bin/${sse2_name}" "${dest}/${sse2_name}"

  if [ "$needs_chmod" = "1" ]; then
    chmod +x "${dest}/${avx2_name}" "${dest}/${sse2_name}"
  fi

  verify_sha256 "${dest}/${avx2_name}" "$avx2_sha"
  verify_sha256 "${dest}/${sse2_name}" "$sse2_sha"

  echo "astcenc (${label}): installed to ${dest}."
}

if [ "$PLATFORM" = "windows" ] || [ "$PLATFORM" = "all" ]; then
  download_platform "windows" "$WIN_ARCHIVE" "$WIN_ARCHIVE_SHA256" "win-x64" \
    "astcenc-avx2.exe" "$WIN_AVX2_SHA256" "astcenc-sse2.exe" "$WIN_SSE2_SHA256" 0
fi

if [ "$PLATFORM" = "linux" ] || [ "$PLATFORM" = "all" ]; then
  download_platform "linux" "$LINUX_ARCHIVE" "$LINUX_ARCHIVE_SHA256" "linux-x64" \
    "astcenc-avx2" "$LINUX_AVX2_SHA256" "astcenc-sse2" "$LINUX_SSE2_SHA256" 1
fi
