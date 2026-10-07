# Mac and Linux builds

Assessment of 2026-10-07: what runs where, what has to change, how to build. "Measured" means it
was tried; the rest is from reading the code or general knowledge.

## Where things stand

**Status (2026-10-07, evening): stages 1-4 done.** `Dom5Editor.Avalonia` has every view the WPF
editor has (window, lists, page header and parts, all panels and badge chips, the event page and
chain map, report and issue windows, Mod Info with the mods a mod needs) on the shared
`Dom5Editor.Core`. Measured: sweeps open every page of DomEnhanced 2.13 (383) with 0 failures in
both editors and let closed pages go (1 alive, ~268 MB); the linux-x64 package runs natively in
WSL (Strigos over Sombre, texts from the exe, a 35-page sweep). Not yet: a run on a real Mac or a
Linux desktop, signing (below), whether Mac/Linux Steam installs have `Dominions6.exe`.

**Stage 5 done (2026-10-07):** the user preferred the Avalonia editor's look and left the call to
retire WPF to me. Removed after the tag `wpf-final` (48 files, ~8,200 lines): Windows now ships
the Avalonia editor too (`tools/publish.sh`, 0.12.0); the program is called `Dom5Editor` on every
system. Before removing, the WPF harness's last own steps were ported (`--flags` gives the same
pixel checksum, `--type-tab` the same cursor); the 0.11.0 WPF zip stays as a fallback.

The rest of this section is the assessment as written that morning.

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

**Now (2026-10-07): `tools/publish-avalonia.sh`** (WSL, the Windows dotnet) builds
`Dom5Editor.Avalonia` for linux-x64, linux-arm64, osx-arm64 and osx-x64 into `publish/`:
`Dom6ModEditor-<version>-linux-*.tar.gz` (a folder: the program, the data files the Windows zip
has, README.txt) and `Dom6ModEditor-<version>-osx-*.zip` (`Dom6 Mod Editor.app` and README.txt).
`tools/package_avalonia.py` writes the archives so the program keeps its executable bit (files on
a Windows drive have no real modes). Choices:
- In the .app everything is in Contents/MacOS, next to the program (Avalonia's documented layout):
  the editor finds its data next to its executable (`EditorStartup`, `CommandHints`, `GameArt`
  ... use `AppContext.BaseDirectory`). Info.plist declares .dm files (Finder's Open With; the app
  takes them through Avalonia's `IActivatableLifetime`).
- Linux bundles the native libraries (Skia, HarfBuzz) in the one file (unpacked to `~/.net` on
  first start); macOS keeps them as files next to it, so `codesign --deep` signs them too.
- **Signing**: no `codesign` or `rcodesign` here, so the .app isn't signed as a bundle (no
  `_CodeSignature`, Info.plist not bound). But the default SDK here is .NET 10 (10.0.401), whose
  publish gives the single-file program an ad-hoc signature itself (measured: a valid code
  directory over the whole file, every page hash matches; the "no signature" above was an SDK 8
  build), and the three dylibs carry their vendors' signatures. Untested on a Mac. The tester
  README (`tools/publish-avalonia-readme.txt`) has testers run `xattr -dr com.apple.quarantine`
  and `codesign --force --deep --sign -` once (codesign ships with macOS). A release without
  that step needs a Developer ID and notarization (above).
- Measured: the linux-x64 package, unpacked in WSL, runs its `--snapshot` (headless) on
  DomEnhanced 2.13 with `DOM6_EXE` set: texts, sprites, an edit, undo/redo, the report, a save
  that differs from the original only by the edit. The macOS builds are untested (no Mac).

## The Avalonia editor's window and harness

- `Views/MainWindow` has what the WPF window has: the Load ▾ menu (recent mods, "Dominions 6
  folder..." — a folder with Dominions6.exe, or data/*.trs: on Mac/Linux it says the texts need
  Dominions6.exe — and "Backups of this mod...", opened with explorer/open/xdg-open), the "Go to"
  box (Ctrl+P; it goes on Enter or a click, not while arrowing through the list), Ctrl+F, the
  window's size, place and the list's width remembered (`Settings.ListWidth`, which WPF doesn't
  use yet), the status note when the game isn't found, a crash handler (errors.log and a
  recovery copy in `Settings.Folder`), a .dm on the command line. The core's synchronous
  questions (`EntityTypeTab.Confirm`, `EntityPageViewModel.SaveFirst`) run a nested dispatcher
  frame. Window shortcuts are taken before the focused control (text boxes would eat Alt+arrows),
  except Option+arrows in a Mac text box (word moves there); undo/redo after it.
- `Views/ValidationWindow` is WPF's ValidationReportWindow (the report's "Every issue...");
  `ValidationIssueItem` moved to Dom5Editor.Core for both.
- **The snapshot harness's view-model steps are shared**: `Dom5Editor.Core/UI/SnapshotSteps.cs`
  (--mod, --select, --set/--add/--remove, --undo/--redo, --save, --sweep, --used-by, --dump,
  --field, --event-*, ...), used by both harnesses, so a command line does and logs the same in
  both (checked: five scripted sessions give the same logs and byte-identical saves in WPF before
  and after the move, and in Avalonia). Each harness keeps the steps that look at controls
  (--png, --view, --report, --tooltips; Avalonia also --validation, --key, --type, --load-menu).
  The Avalonia harness points `Settings.Folder` and the backups at temp folders.
