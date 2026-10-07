"""Packs one `dotnet publish` of Dom5Editor.Avalonia for testers (tools/publish-avalonia.sh runs it).

    python3 tools/package_avalonia.py PUBLISH_DIR RID VERSION README OUT_DIR

Linux RIDs: OUT_DIR/Dom6ModEditor-VERSION-RID.tar.gz, a Dom6ModEditor folder with the program,
its data files and README.txt. macOS RIDs: OUT_DIR/Dom6ModEditor-VERSION-RID.zip with
"Dom6 Mod Editor.app" (Contents/Info.plist, Contents/MacOS: the program, its native libraries and
data files, where the editor looks for them; Contents/Resources) and README.txt.

The archives are written here rather than with tar/zip because the files come from a Windows
drive (every file looks executable, or none does): each entry gets its mode set, 755 for the
program and native libraries, 644 for the rest, so it runs after unpacking on macOS and Linux.
"""
import io
import os
import sys
import tarfile
import time
import zipfile

APP = "Dom6 Mod Editor.app"
EXE = "Dom5Editor.Avalonia"  # the program's name (the assembly's: its resources are found by it)

INFO_PLIST = """<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>
    <string>Dom6 Mod Editor</string>
    <key>CFBundleDisplayName</key>
    <string>Dom6 Mod Editor</string>
    <key>CFBundleIdentifier</key>
    <string>net.dom5parser.dom6modeditor</string>
    <key>CFBundleVersion</key>
    <string>{version}</string>
    <key>CFBundleShortVersionString</key>
    <string>{version}</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleExecutable</key>
    <string>{exe}</string>
    <key>CFBundleInfoDictionaryVersion</key>
    <string>6.0</string>
    <key>LSMinimumSystemVersion</key>
    <string>12.0</string>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>CFBundleDocumentTypes</key>
    <array>
        <dict>
            <key>CFBundleTypeName</key>
            <string>Dominions mod</string>
            <key>CFBundleTypeRole</key>
            <string>Editor</string>
            <key>LSHandlerRank</key>
            <string>Alternate</string>
            <key>CFBundleTypeExtensions</key>
            <array>
                <string>dm</string>
            </array>
        </dict>
    </array>
</dict>
</plist>
"""


def files(src):
    """The publish folder's files, relative paths in a stable order (debug symbols left out)."""
    out = []
    for root, dirs, names in os.walk(src):
        dirs.sort()
        for name in sorted(names):
            if name.endswith(".pdb"):
                continue
            path = os.path.join(root, name)
            out.append((os.path.relpath(path, src).replace(os.sep, "/"), path))
    return out


def mode_of(rel):
    """755 for what runs (the program, native libraries), 644 for data."""
    name = rel.rsplit("/", 1)[-1]
    return 0o755 if name == EXE or name.endswith((".so", ".dylib")) else 0o644


def dirs_of(paths):
    """Every folder the paths are in, parents first."""
    seen = []
    for p in paths:
        parts = p.split("/")[:-1]
        for k in range(1, len(parts) + 1):
            d = "/".join(parts[:k])
            if d not in seen:
                seen.append(d)
    return seen


def tar_linux(src, out, readme):
    top = "Dom6ModEditor"
    entries = [(f"{top}/{rel}", path, mode_of(rel)) for rel, path in files(src)]
    now = int(time.time())
    with tarfile.open(out, "w:gz", compresslevel=9) as tar:
        for d in dirs_of([e[0] for e in entries] + [f"{top}/README.txt"]):
            info = tarfile.TarInfo(d)
            info.type, info.mode, info.mtime = tarfile.DIRTYPE, 0o755, now
            tar.addfile(info)
        for name, path, mode in entries:
            info = tar.gettarinfo(path, arcname=name)
            info.mode, info.uid, info.gid, info.uname, info.gname = mode, 0, 0, "", ""
            with open(path, "rb") as f:
                tar.addfile(info, f)
        data = readme.encode("utf-8")
        info = tarfile.TarInfo(f"{top}/README.txt")
        info.size, info.mode, info.mtime = len(data), 0o644, now
        tar.addfile(info, io.BytesIO(data))


def zip_entry(z, name, data, mode):
    info = zipfile.ZipInfo(name, date_time=time.localtime()[:6])
    info.create_system = 3  # Unix: macOS's Archive Utility and unzip keep the mode then
    if name.endswith("/"):
        info.external_attr = (0o040000 | mode) << 16 | 0x10
        z.writestr(info, b"")
    else:
        info.external_attr = (0o100000 | mode) << 16
        info.compress_type = zipfile.ZIP_DEFLATED
        z.writestr(info, data, compresslevel=9)


def zip_mac(src, out, readme, version):
    contents = f"{APP}/Contents"
    entries = [(f"{contents}/MacOS/{rel}", path, mode_of(rel)) for rel, path in files(src)]
    plist = INFO_PLIST.format(version=version, exe=EXE).encode("utf-8")
    names = [e[0] for e in entries] + [f"{contents}/Info.plist"]
    with zipfile.ZipFile(out, "w") as z:
        # (Resources stays empty: the editor finds its data next to the program, in MacOS)
        for d in dirs_of(names) + [f"{contents}/Resources"]:
            zip_entry(z, d + "/", b"", 0o755)
        zip_entry(z, f"{contents}/Info.plist", plist, 0o644)
        zip_entry(z, f"{contents}/PkgInfo", b"APPL????", 0o644)
        for name, path, mode in entries:
            with open(path, "rb") as f:
                zip_entry(z, name, f.read(), mode)
        zip_entry(z, "README.txt", readme.encode("utf-8"), 0o644)


def main():
    src, rid, version, readme_path, out_dir = sys.argv[1:6]
    with open(readme_path, encoding="utf-8") as f:
        readme = f.read()
    if not os.path.isfile(os.path.join(src, EXE)):
        sys.exit(f"{EXE} isn't in {src}: was it published for {rid}?")
    if rid.startswith("osx-"):
        out = os.path.join(out_dir, f"Dom6ModEditor-{version}-{rid}.zip")
        zip_mac(src, out, readme, version)
    else:
        out = os.path.join(out_dir, f"Dom6ModEditor-{version}-{rid}.tar.gz")
        tar_linux(src, out, readme)
    print(f"{out}  {os.path.getsize(out) / 1e6:.1f} MB")


if __name__ == "__main__":
    main()
