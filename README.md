# Dom6 Mod Editor (Dom5Parser)

An editor for Dominions 6 mod files (`.dm`). Open a mod, see what every monster, weapon,
armor, spell, item, site, nation, event and the rest *are in game* after the mod, edit any of
it, and save without losing anything the file had.

- **What the game sees:** every value shows where it comes from: set by the mod, vanilla, or
  copied from another entity (`#copystats`, `#copyweapon`, ...). Copies, clears and repeated
  lines are combined the way the game's own parser does it (read from `Dominions6.exe`).
- **Every command editable:** each command the game reads for a type has a badge or a panel:
  monster stats laid out like the game's unit window, weapons and armor as tables, magic paths
  and random magic with the game's path icons, leadership, body and item slots; weapon damage
  types and qualities; spell paths, cost, effect and area; item and site paths; nation
  recruitment; forms for mercenaries, poptypes, blesses and AI templates. Abilities are grouped
  by the modding manual's sections. Commands the game ignores are marked "n/r"; values no
  command can set are shown read-only.
- **Hints from the manuals:** pointing at any value shows what the command does and its table
  of values (paths, rarities, scales, orders, afflictions), and values read as words next to
  the number ("Fire", "always (unlimited)").
- **Events as scripts:** an event reads as sentences (how it's rolled, who owns it, when it
  happens, what it does, in order), each value edited in place; codes, delays, variables,
  player choices and enchantment spells link events into chains, drawn as a map and checked
  (a code no event sets, a missing [site name], ...). Follow-ups, delayed follow-ups and player
  choices are one button each.
- **Edits that mean what you expect:** changing a vanilla unit adds a `#select` block for it
  (vanilla data is never changed); removing an inherited value writes what the game needs
  (`#fear 0`, or a group clear plus the rest of the group); a change to a template reaches the
  copies that don't set the value themselves. Full undo and redo.
- **Saves without losing anything:** the file keeps its order, comments and formatting;
  unedited lines are written exactly as read. The previous file is kept as `.bak`.
- **Browsing:** links between entities (a unit's weapons, a nation's recruits, a summon spell's
  monster), back and forward (Alt+arrows, mouse buttons), "used by" on every page, go to any
  entity (Ctrl+P), list search (Ctrl+F), lists filtered to vanilla / changed / new and by kind
  (mages, rituals, events in a chain, ...), sprites and key stats in the lists. "Copy & edit"
  gives a unit its own changed copy of a weapon or armor; "+ New weapon" makes one for it.
- **Several entities side by side:** "New window" on a page (or a row's right-click menu,
  Ctrl+click or a middle click on a row, Ctrl+click on a link) opens the entity in a window of
  its own, to arrange beside the main window or on another screen. Edits show in every window
  at once and undo is the mod's; a window's links open in it, with its own back and forward.

## Running it

Download a release, unzip, run `Dom5Editor.exe` (Windows, no install; macOS and Linux
packages too, see `tools/publish-avalonia-readme.txt`). `vanilla.dm` (the
game's data, written from the game by `tools/dom6exe`) must stay next to the exe.

The game's own texts (monster, item and spell descriptions, a spell's details, portent and
cure, a nation's description, summary and brief) and the vanilla events' messages are read
from your Dominions6.exe (6.37) when the editor starts: it looks in your Steam libraries (or
pick the folder: the arrow next to Load, "Dominions 6 folder..."; or set `DOM6_EXE`). Another game version shows none (the places they're read from
are checked first). Vanilla unit, item and site sprites are read from the same install's data
folder; the editor ships only which picture each one is (`vanilla-sprites.json`), not the art.
A folder with `icons/sprites` next to the exe overrides them. The game's icons (stats, paths,
gems, abilities) are compiled into the editor.

## Building

.NET 8 SDK, on Windows, macOS or Linux (the editor is Avalonia).

```
dotnet build Dom5Edit.sln
dotnet run --project Dom5Editor.Avalonia/Dom5Editor.Avalonia.csproj
./tools/publish.sh            # Windows: one self-contained Dom5Editor.exe in publish/Dom6ModEditor, and a zip for testers (from WSL)
./tools/publish-avalonia.sh   # macOS/Linux: tar.gz per Linux, .app zip per macOS (from WSL)
```

## Project layout

- `Dom5Edit`: the core library: parsing, the model, the resolver (`Resolve/`: what an entity is
  in game), edits (`Editing/`: every change as an undoable edit), saving (`Mod/SavePlan`,
  `ModExporter`). Also older merge code, not used by the editor.
- `Dom5Editor.Core`: the editor's session and page view models, without a UI toolkit.
- `Dom5Editor.Avalonia`: the editor's windows (Avalonia; the program is `Dom5Editor`). The WPF
  editor it replaced is in the history up to the tag `wpf-final`.
- `Dom5Tests`: command-line checks (`resolve`, `roundtrip`, `edit`).
- `tools/dom6exe`: reads the game's data and parser rules from `Dominions6.exe`; writes
  `vanilla.dm` and the command catalog the core uses.
- `tools/fidelity`: the test suite (load, edit, save and compare with an independent parser).
- `docs/`: how saving and editing work (`SAVE_FLOW.md`, `EDIT_FLOW.md`), the work log and
  roadmap.

## License

MIT license / public domain, but please give credit somewhere.
