#!/bin/sh
# Builds the macOS/Linux editor (Dom5Editor.Avalonia) for testers: one self-contained program per
# system with its data files (vanilla.dm, vanilla-events.dm, vanilla-sprites.json, the spell
# tables, Data/*.json: what the Windows zip has), and tools/publish-avalonia-readme.txt as
# README.txt:
#   publish/Dom6ModEditor-<version>-linux-x64.tar.gz, -linux-arm64.tar.gz   a Dom6ModEditor folder
#   publish/Dom6ModEditor-<version>-osx-arm64.zip, -osx-x64.zip              "Dom6 Mod Editor.app"
# Run from WSL (the Windows dotnet, as tools/publish.sh); tools/package_avalonia.py writes the
# archives with the program's executable bit set. The macOS apps aren't signed (Apple Silicon
# runs no unsigned code: the README says how testers sign them ad hoc; see docs/CROSS_PLATFORM.md).
#   tools/publish-avalonia.sh [RID ...]      (default: linux-x64 linux-arm64 osx-arm64 osx-x64)
set -e
cd "$(dirname "$0")/.."
DOTNET="/mnt/c/Program Files/dotnet/dotnet.exe"
PROJECT=Dom5Editor.Avalonia/Dom5Editor.Avalonia.csproj
VERSION=$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' "$PROJECT")
DATE=$(date +%Y-%m-%d)
COMMIT=$(git rev-parse --short HEAD)
RIDS=${*:-linux-x64 linux-arm64 osx-arm64 osx-x64}
mkdir -p publish
for RID in $RIDS; do
  OUT="publish/avalonia/$RID"
  rm -rf "$OUT"
  # Linux: the native libraries (Skia, HarfBuzz) inside the one file, unpacked to ~/.net on the
  # first start. macOS: next to it, so signing the app (codesign --deep) signs them too.
  case $RID in
    osx-*) NATIVE_INSIDE=false ;;
    *) NATIVE_INSIDE=true ;;
  esac
  "$DOTNET" publish "$(wslpath -w "$PROJECT")" -c Release -r "$RID" --self-contained true \
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=$NATIVE_INSIDE \
    -p:DebugType=embedded -o "$(wslpath -w "$OUT")" -nologo -v q -clp:ErrorsOnly
  sed -e "s/@VERSION@/$VERSION/g" -e "s/@DATE@/$DATE/g" -e "s/@COMMIT@/$COMMIT/g" -e "s/@RID@/$RID/g" \
    tools/publish-avalonia-readme.txt > "publish/avalonia/README-$RID.txt"
  python3 tools/package_avalonia.py "$OUT" "$RID" "$VERSION" "publish/avalonia/README-$RID.txt" publish
done
