#!/bin/sh
# Builds the editor as one self-contained Windows exe with its data files, in publish/Dom6ModEditor,
# and zips it for testers (publish/Dom6ModEditor-<version>-<date>.zip, with tools/publish-readme.txt
# as README.txt). Run from WSL; uses the Windows dotnet. The game's icons are compiled in; the
# game's texts, unit/item/site sprites and flags are read from the player's install at run time.
set -e
cd "$(dirname "$0")/.."
DOTNET="/mnt/c/Program Files/dotnet/dotnet.exe"
OUT="publish/Dom6ModEditor"
VERSION=$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' Dom5Editor/Dom5Editor.csproj)
DATE=$(date +%Y-%m-%d)
COMMIT=$(git rev-parse --short HEAD)
rm -rf "$OUT"
"$DOTNET" publish "$(wslpath -w Dom5Editor/Dom5Editor.csproj)" -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
  -o "$(wslpath -w "$OUT")" -v q
sed -e "s/@VERSION@/$VERSION/" -e "s/@DATE@/$DATE/" -e "s/@COMMIT@/$COMMIT/" tools/publish-readme.txt > "$OUT/README.txt"
ZIP="publish/Dom6ModEditor-$VERSION-$DATE.zip"
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
