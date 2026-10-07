#!/bin/sh
# Builds the editor (Dom5Editor.Avalonia: the same program as on macOS and Linux, see
# tools/publish-avalonia.sh) as one self-contained Windows exe, Dom5Editor.exe, with its data files,
# in publish/Dom6ModEditor, and zips it for testers (publish/Dom6ModEditor-<version>-windows.zip,
# named like the macOS/Linux packages,
# with tools/publish-readme.txt as README.txt). Run from WSL; uses the Windows dotnet. The game's
# icons are compiled in; the game's texts, unit/item/site sprites and flags are read from the
# player's install at run time. (Until wpf-final the Windows build was the WPF editor.)
set -e
cd "$(dirname "$0")/.."
DOTNET="/mnt/c/Program Files/dotnet/dotnet.exe"
OUT="publish/Dom6ModEditor"
PROJECT=Dom5Editor.Avalonia/Dom5Editor.Avalonia.csproj
VERSION=$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' "$PROJECT")
DATE=$(date +%Y-%m-%d)
COMMIT=$(git rev-parse --short HEAD)
rm -rf "$OUT"
# (Skia's and HarfBuzz's native libraries inside the one file, unpacked on the first start)
"$DOTNET" publish "$(wslpath -w "$PROJECT")" -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=embedded \
  -o "$(wslpath -w "$OUT")" -nologo -v q -clp:ErrorsOnly
sed -e "s/@VERSION@/$VERSION/" -e "s/@DATE@/$DATE/" -e "s/@COMMIT@/$COMMIT/" tools/publish-readme.txt > "$OUT/README.txt"
ZIP="publish/Dom6ModEditor-$VERSION-windows.zip"
rm -f "$ZIP"
python3 - "$OUT" "$ZIP" <<'PY'
import os, sys, zipfile
src, out = sys.argv[1], sys.argv[2]
with zipfile.ZipFile(out, 'w', zipfile.ZIP_DEFLATED, compresslevel=6) as z:
    for root, dirs, files in os.walk(src):
        for f in files:
            p = os.path.join(root, f)
            z.write(p, os.path.join('Dom6ModEditor', os.path.relpath(p, src)))
PY
ls -la "$OUT" "$ZIP"
