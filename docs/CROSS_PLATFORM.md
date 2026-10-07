# Mac and Linux builds

Assessment of 2026-10-07: what runs where, what has to change, how to build. "Measured" means it
was tried; the rest is from reading the code or general knowledge.

## Where things stand

- **Dom5Edit (core) and Dom5Tests (CLI) run on Linux** (measured: built with a local .NET 8 SDK
  in WSL, 0 errors, no platform warnings; `Dom5Tests check` on hermit.dm and DomEnhanced 2.13 saves
  byte-identical). The only registry use is guarded by `OperatingSystem.IsWindows()`
  (`Dom5Edit/Events/VanillaEventMessages.cs`).
- **Cross-compiling works from WSL/Windows** (measured): self-contained single-file publishes
  for linux-x64, linux-arm64, osx-x64 and osx-arm64; the linux-x64 binary ran `check`.
- **The editor (Dom5Editor) is WPF: Windows only.** 15 XAML files (3,467 lines), 10 code-behind
  files (3,302 lines), 44 other .cs files. The view models are mostly plain MVVM; a few hold WPF
  brushes, images or open dialogs (listed below).
- **The game's files:** the Windows Steam install of Dominions 6 also holds `dom6_amd64` (Linux),
  `dom6_mac` (macOS, arm64 only) and one shared `data/*.trs` (sprites: the same on every OS). The
  game's texts and event messages are in all three binaries at different offsets; the editor
  reads them from `Dominions6.exe` (checksum-checked). Guess: Mac/Linux Steam installs have the
  same files, `Dominions6.exe` included; verify on a real install. If not, `tools/dom6exe` has to
  write text locations for the ELF/Mach-O binaries too.
- **Folders:** user data `~/.dominions6/` on Linux and Mac (mods in `~/.dominions6/mods`, the
  manual); Steam `~/.local/share/Steam/steamapps/common/Dominions6` (Linux; Flatpak under
  `~/.var/app/com.valvesoftware.Steam/.local/share/Steam`) and
  `~/Library/Application Support/Steam/steamapps/common/Dominions6` (Mac).

## The way there (recommended): Avalonia 11

Avalonia's XAML is the closest to WPF. Carries over: layout, DataTemplates, ancestor bindings,
DynamicResource, converters, geometry drawing, RenderTargetBitmap, virtualizing lists. Reworked:
triggers become style classes/selectors (70 DataTriggers, 32 Triggers), `Visibility` becomes
`IsVisible` (130 uses), ControlTemplates become ControlThemes (74), dependency properties become
StyledProperty (74), dialogs become the async storage picker, no MessageBox. Avalonia.Headless
renders without a screen, so the snapshot harness can run in Linux CI. Guess: 4-7 weeks.

Rejected: Uno (WinUI dialect, further from WPF), MAUI (no Linux desktop), a web UI (every view
rewritten, 6-10 weeks). Wine/CrossOver with the WPF build: worth an hour's try as a stopgap only.

Stages:
1. Core fixes (below).
2. Move the view models to a UI-neutral net8.0 project: colors as hex strings, images behind an
   interface, dialogs and messages through a service. WPF keeps working.
3. An Avalonia shell next to WPF, view by view, theme first.
4. Snapshot harness on Avalonia.Headless; compare its PNGs with WPF's.
5. Retire WPF.

## Changes needed before any UI port

1. Game folder per OS (`VanillaEventMessages.cs` GameInstall): Steam roots for Linux/Mac, read
   `libraryfolders.vdf` without the registry, no `\\` path rewrites; accept a folder with
   `data/*.trs` or `dom6_amd64`/`dom6_mac`, not only `Dominions6.exe`. `GameArt.cs` defaults follow.
2. Texts and messages: on Mac/Linux read `Dominions6.exe` from the game folder; if it isn't there,
   ELF/Mach-O text locations from `tools/dom6exe`.
3. `System.Drawing` (`TargaImage.cs`, `Utility.cs`): a small TGA-to-BGRA decoder instead.
4. `RelayCommand`: explicit `CanExecuteChanged` instead of `CommandManager.RequerySuggested`.
5. Opening folders (`MainWindow.xaml.cs`, explorer.exe): `open` / `xdg-open` elsewhere.
6. Sprite paths (`SpriteLoader.cs`): convert every `\` in a mod's relative paths; file names
   are case-sensitive on Linux.
7. Settings/backups use `ApplicationData` (`~/.config` on Linux/Mac): works; the tester README
   says `%APPDATA%`.
8. `Mod.cs` opens its log with `Process.Start(file)` without `UseShellExecute` (fails on modern
   .NET everywhere).
9. Unused packages: `DotNetKit.Wpf.AutoCompleteComboBox`, `WindowsAPICodePack-Shell`.
10. Scripts (`tools/publish.sh`, `tools/fidelity/run.mjs`) assume the Windows dotnet and `wslpath`.

## Building and packaging

- `dotnet publish -r {linux-x64|linux-arm64|osx-arm64|osx-x64} --self-contained
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true` (all from Windows/WSL).
- Linux: a tar.gz, or an AppImage (appimagetool runs in WSL).
- Mac: a `.app` bundle (Contents/MacOS, Info.plist, .icns), assembled anywhere. Apple Silicon
  won't run unsigned arm64 code (measured: the cross-built binaries have no signature): ad-hoc
  sign at least (`codesign -s -` on a Mac, or `rcodesign` on Linux). Without the Gatekeeper
  warning: an Apple Developer ID ($99/year) and notarization (`notarytool`, or
  `rcodesign notary-submit`). Unsigned downloads: "Open Anyway" in Privacy & Security, or
  `xattr -dr com.apple.quarantine`. The game's Mac binary is arm64 only, so osx-x64 is optional.
