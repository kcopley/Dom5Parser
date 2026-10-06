Dom6 Mod Editor @VERSION@ (test build, @DATE@, @COMMIT@)
============================================================

An editor for Dominions 6 mod files (.dm): browse the game's own data and a mod's, edit every
entity with real panels, build event chains, and save back without disturbing lines you
didn't change.

Running it
- Windows 10 or 11, 64-bit. Nothing to install: unzip the folder anywhere and run
  Dom5Editor.exe. Keep the other files next to it (vanilla.dm and the rest are the game's data
  the editor builds on).
- Windows may warn that the program is unrecognised (it isn't signed): "More info", then
  "Run anyway".

Dominions 6
- The game's own texts (unit, item, spell and nation descriptions, event messages) and its
  unit, item and site pictures and nation flags are read from your Dominions 6 install when
  the editor starts. It looks in your Steam libraries. If it doesn't find the game, the status
  bar says so: use the small arrow next to Load, "Dominions 6 folder...", and pick
  Dominions6.exe (read at the next start).
- These are read from Dominions 6 version 6.37. With another version they aren't shown; the
  rest works.

Using it
- Load (Ctrl+O) opens a .dm file; Save (Ctrl+S) writes it back (the previous file is kept as a
  .bak). Lines you didn't touch are saved exactly as they were. Undo/Redo: Ctrl+Z / Ctrl+Y.
- Ctrl+P: go to anything by name or ID. Ctrl+F: search the list (for events, also the message
  text). Hover anything for what it does.
- Validate lists problems (lines the game won't read, missing references, ID clashes).

If something goes wrong
- The editor keeps running and writes the details to %APPDATA%\Dom5Editor\errors.log. Please
  send that file with what you were doing. Settings (window, recent mods, the game folder) are
  in %APPDATA%\Dom5Editor\settings.json.
