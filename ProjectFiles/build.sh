#!/bin/bash

set -euo pipefail

PROJECT_PATH=$(realpath "../RivenFramework-Unity")
BUILD_PATH=$(realpath "./Builds")
mkdir -p "$BUILD_PATH"
APP_NAME="CorridorGeodesic"
UNITY_EDITOR="/home/lizband/Unity/Hub/Editor/2022.3.15f1/Editor/Unity"
LOG_FILE=$(realpath "./build-log.txt")

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

log() { echo -e "${CYAN}[INFO]${NC} $*"; }
ok() { echo -e "${GREEN}[OK]${NC} $*"; }
warn() { echo -e "${YELLOW}[WARN]${NC} $*"; }
error() { echo -e "${RED}[ERROR]${NC} $*" >&2; }
die() { error "$*"; exit 1; }


BUILD_LINUX=false
BUILD_WINDOWS=false
DEV_BUILD=false
ZIP_BUILD=false

usage() {
    cat <<EOF
Usage: ./build.sh [OPTIONS]

Options:
  --linux       Build for Linux 64-bit
  --windows     Build for Windows 64-bit
  --dev         Enable development build (auto-connect profiler + deep profiling)
  --zip         Zip each platform's build output after building
  -h, --help    Show this help message and exit

Examples:
  ./build.sh --linux --windows --zip
  ./build.sh --linux --dev --zip
  ./build.sh --linux --windows --dev --zip
EOF
    exit 0
}

if [[ $# -eq 0 ]]; then
    warn "No arguments provided"
    echo "Run with --help for usage information"
    exit 0
fi

while [[ $# -gt 0 ]]; do
    case "$1" in
        --linux) BUILD_LINUX=true ;;
        --windows) BUILD_WINDOWS=true ;;
        --dev) DEV_BUILD=true ;;
        --zip) ZIP_BUILD=true ;;
        -h|--help) usage ;;
        *) die "Unknown option: $1, Run with --help for usage" ;;
    esac
    shift
done


[[ "$BUILD_LINUX" == false && "$BUILD_WINDOWS" == false ]] \
    && die "No build target selected, Use --linux and/or --windows"

[[ ! -f "$UNITY_EDITOR" ]] \
    && die "Unity editor not found at: $UNITY_EDITOR"

[[ ! -d "$PROJECT_PATH" ]] \
    && die "Unity project not found at: $PROJECT_PATH"

if $ZIP_BUILD && ! command -v zip &>/dev/null; then
    die "'zip' is not installed, Run: sudo apt install zip"
fi

mkdir -p "$BUILD_PATH"




log "========================================"
log "  Unity Project Builder"
log "========================================"
log "Project  : $PROJECT_PATH"
log "Output   : $BUILD_PATH"
log "Editor   : $UNITY_EDITOR"
log "Log file : $LOG_FILE"
TARGETS=""
$BUILD_LINUX   && TARGETS="${TARGETS}Linux "
$BUILD_WINDOWS && TARGETS="${TARGETS}Windows"
log "Targets  : $TARGETS"
log "Dev build: $DEV_BUILD"
log "Zip      : $ZIP_BUILD"
log "========================================"
echo ""



run_build() {
    local platform="$1"
    local output_path="$2"
    local method="$3"

    log "Building for $platform → $output_path"
    mkdir -p "$(dirname "$output_path")"

    local dev_flag=""
    $DEV_BUILD && dev_flag="-devBuild"

    "$UNITY_EDITOR" \
        -quit \
        -batchmode \
        -projectPath   "$PROJECT_PATH" \
        -logFile       "$LOG_FILE" \
        -executeMethod "$method" \
        -outputPath    "$output_path" \
        $dev_flag

    local exit_code=$?
    if [[ $exit_code -eq 0 ]]; then
        ok "$platform build succeeded!"
    else
        error "$platform build FAILED (exit $exit_code), Check log: $LOG_FILE"
        return 1
    fi
}


zip_build() {
    local platform="$1"
    local build_dir="$2"
    local zip_file="$BUILD_PATH/${APP_NAME}_${platform}.zip"

    log "Zipping $platform build → $zip_file"
    [[ -f "$zip_file" ]] && rm -f "$zip_file"

    (cd "$build_dir" && zip -r "$zip_file" .) \
        && ok "Zipped: $zip_file" \
        || { error "Zip failed for $platform"; return 1; }
}


$BUILD_LINUX   && run_build "Linux"   "$BUILD_PATH/Linux/${APP_NAME}.x86_64" "BuildScript.BuildLinux"
$BUILD_LINUX   && $ZIP_BUILD && zip_build "Linux"   "$BUILD_PATH/Linux"

$BUILD_WINDOWS && run_build "Windows" "$BUILD_PATH/Windows/${APP_NAME}.exe"  "BuildScript.BuildWindows"
$BUILD_WINDOWS && $ZIP_BUILD && zip_build "Windows" "$BUILD_PATH/Windows"


echo ""
ok "All requested builds finished"
log "Build log: $LOG_FILE"
