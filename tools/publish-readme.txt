Dom6 Mod Editor @VERSION@ (test build, @DATE@, @COMMIT@)
============================================================

An editor for Dominions 6 mod files (.dm): browse the game's own data and a mod's, edit every
entity with real panels, build event chains, and save back without disturbing lines you
didn't change.

New in this build
- A new look: the editor is now the same program on Windows, macOS and Linux (built with
  Avalonia instead of WPF). Everything works as before; pages, lists and windows look a little
  different (another font, slightly tighter rows). Your settings and backups are where they
  were. If something works worse than in 0.11.0, please say what: that build stays available.

Also in 0.11.0
- Submods: a mod can be read over the mods it needs (Sombre Warhammer's submods over Sombre),
  as the game reads mods: in the order they were enabled. Mod Info, "Needs": add, remove and
  order them (remembered for the mod). The parent's units, items and events show and link like
  the game's; changing one adds a line to your mod, and the parent's file is never changed.
  Opening a submod alone, the bar suggests the mod it probably needs ("Read it over that mod").
- "Used by" lists what really uses an entity (it listed spells that pick from the game's unit
  lists under the wrong monster, and every event that boosts a path under monster 1).
- The report also says when a submod's new unit takes a number its parent already uses.
- The editor also runs on macOS and Linux (separate test packages, ask for them).

Earlier (0.10.0)
- Opening a mod checks it: a bar says what goes wrong in game, which lines the game ignores
  (with "did you mean" for typos) and what's worth a look. "Open report" lists every finding
  with its line and a Go to; "Save for the author..." writes it as a file to send to the mod's
  author. What the game reads is taken from the game itself.
- Your files are safe: a copy of the mod is made when you open it and before every save, and a
  save must read back the same before it replaces the file (see "Backups" below).
- Mounted units: a Mount panel on unit pages (mount, co-rider, riders, skilled rider).

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
  bar says so: use the small arrow next to Load, "Dominions 6 folder...", and pick the
  folder Dominions6.exe is in (read at the next start).
- These are read from Dominions 6 version 6.37. With another version they aren't shown; the
  rest works.

Using it
- Load (Ctrl+O) opens a .dm file; Save (Ctrl+S) writes it back. Lines you didn't touch are
  saved exactly as they were. Undo/Redo: Ctrl+Z / Ctrl+Y.
- Ctrl+P: go to anything by name or ID. Ctrl+F: search the list (for events, also the message
  text). Hover anything for what it does.
- Validate checks the mod as it is now and opens the report.
- Mods from the Steam workshop: Steam replaces those files when the mod updates, so the editor
  asks before saving there. Save a copy in your Dominions 6 mods folder instead.

Backups
- A copy of the mod's file is made when you open it and before each save, in
  %APPDATA%\Dom5Editor\backups (the last 30 per mod). The small arrow next to Load has
  "Backups of this mod...". If a save doesn't read back the same, the file isn't replaced and
  the editor says why. If the editor fails with unsaved edits, a copy of the mod with them goes
  to %APPDATA%\Dom5Editor\recovery.

If something goes wrong
- The editor keeps running and writes the details to %APPDATA%\Dom5Editor\errors.log. Please
  send that file with what you were doing. Settings (window, recent mods, the game folder) are
  in %APPDATA%\Dom5Editor\settings.json.
