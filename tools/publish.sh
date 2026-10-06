#!/bin/sh
# Builds the editor as one self-contained Windows exe with its data files, in publish/Dom6ModEditor.
# (Run from WSL; uses the Windows dotnet. The game's icons are compiled in; unit sprites and
# descriptions come from supplied folders (see README).)
set -e
cd "$(dirname "$0")/.."
DOTNET="/mnt/c/Program Files/dotnet/dotnet.exe"
OUT="publish/Dom6ModEditor"
rm -rf "$OUT"
"$DOTNET" publish "$(wslpath -w Dom5Editor/Dom5Editor.csproj)" -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
  -o "$(wslpath -w "$OUT")" -v q
ls -la "$OUT"
