# Work log

Running log of the autonomous work sessions: what was done, what was found, what's next.
Newest entries at the top. Commits are local unless noted; the user pushes.

## 2026-10-07 (evening): "used by" fixed, the editor on Mac and Linux

The user: monster 3's "used by" listed Bind Heliophagus ("definitely not correct! There's a
number of used by issues"), then "look at implementing cross platform".

- **"Used by"** (37e7770): spell effect numbers from `spell_effect_types.json` now replace the
  old Dom5 guesses instead of adding to them (effects 89, 114, 76, 120, 127, 68 took `#damage`
  for a monster number: Bind's 3 is the third list of uniques). A spell that picks from one of
  the game's unit lists (unique/terrain summons, Tartarian Gate, ...) is listed under each unit;
  a monster tag use under each tagged monster; an event's path boost names its monster only
  without target requirements or a commander it makes (monster 1: 85 users to 1); lines the
  game doesn't read link to nothing. `Dom5Tests usage [MOD]` counts every kind of link.
- **Portable core** (07a5759, adcd60a): the game found per OS (Steam roots, libraryfolders.vdf
  without the registry, a folder with `data/*.trs` is enough for sprites), TGA and PNG decoders
  of our own (System.Drawing and 2,574 lines of TargaImage gone; checked against the old
  decoders on 9,499 TGAs and 2,243 PNGs), mod file names in any case off Windows.
- **Dom5Editor.Core** (2f5fd0e): session, view models, game art and data files in a net8.0
  library without WPF; images as `Picture`, colours as `#RRGGBB`, dialogs/dispatcher through
  `UI.Ui`. The WPF editor renders pixel-identical before and after (17 pages).
- **Dom5Editor.Avalonia** (bdefae8, 211395a; three agents in worktrees, merged 573b6ae, 38708ef,
  e19ff76): every view of the WPF editor on the shared core. The event page and chain map; all
  panels, badge chips (one control for wrap and grid), copies/add box/long texts/removals,
  sprite slots (click, right-click, drop); the Load menu, Go to, layout, crash handler, the full
  issue list. The snapshot harness's view-model steps are one file for both editors
  (`Dom5Editor.Core/UI/SnapshotSteps.cs`). `RefPicker` picks only on Enter, a click, or Tab
  after typing (AutoCompleteBox picked on every arrow key: an edit per key).
- **Fixed after merging** (4afe5ad, 8ad38d1, d0f893c): every button's "can it run" listener sat
  in a static event without WPF's CommandManager, so closed pages stayed alive (383-page sweep:
  324 alive, 1.5 GB; now 1, 268 MB, as WPF); Fluent's blue accent made brown; the parent-finder
  runs in the background (it read 55 MB around Strigos: 3 s on a slow disk).
- **Mac/Linux packages** (2430d80, `tools/publish-avalonia.sh`): linux-x64/arm64 tar.gz,
  osx-arm64/x64 .app zips (40-44 MB each) in `publish/`; the linux-x64 one runs natively in WSL.
  Not signed as a bundle (no codesign here; the README has testers sign ad hoc); untested on a Mac.
- **The game's mod order** (ce66965, from the exe): enabled mods are read in the order they
  were enabled (enabling appends, nothing sorts), so a submod must be enabled after its parent;
  Strigos's own description says the same.
- **Mods a mod needs** (852a8e4): a submod is read over its parent (a chain: vanilla, the
  needed mods in order, the mod). The parent's lines are base data (edits add lines to the
  submod; the parent is never changed or saved; its new events and bands are read-only). Mod
  Info lists them (remembered per mod); the report bar suggests the parent when the mod uses
  numbers from another mod (NeededModFinder). Strigos over Sombre: 70 missing numbers to 0;
  all four Sombre submods save byte-identical; `check`/`resolve --with` read over the parent.

## 2026-10-07: workshop mods, never losing data, reports for authors, new mods from scratch

The user asked to try the newest workshop mods (Sombre Warhammer and its submods, Forgotten
Realms, Confluence, PS Bloodwar, FasterForts, DomEnhanced 2.14: 16 files) and see what errors
their authors made; to make sure saving can never wreck anyone's data (backups like on load?);
to report errors to the authors; to make focused new mods from scratch in the UI; to test
edits across every entity type; and asked whether mounted units are easy to edit now.

- **Loading and saving workshop mods** (d9e0017): a parser crash fixed (`##tag##` at a line's
  end), multi-command lines, missing `#end`, no final newline / LF / BOM kept. `Dom5Tests check`
  runs one mod through everything (6b785d3, `--with PARENT.dm` for a submod). All 16 files save
  byte-identical and read back with the same entities.
- **How the game reads a file** (0f4e879, from the exe): one pass per type over the whole
  text, every `#` tried; texts run to the next quote across lines; `#msg`/`#name`-like texts
  are skipped (their command lines are text), descriptions are read through; `--` is cut inside
  quotes too; a missing last line break loses the last character. The parser now follows
  this (6379ed3) and notes where the game reads lines differently than they look.
- **Regenerated lines** (6379ed3): every workshop file saved with every line regenerated and
  compared in the inspector. Fixed, each a change in what the game would read had the line
  been rewritten: an unresolved nation (a submod's `#req_fornation 173`) was dropped; a
  `#copystats`-renamed monster's old name turned into the new one; `#domshape "Mara"` became
  `-1`; `#damagemon "Name"` became `#damage "Name"`; `#dmg 5 -1` was lost; extra values
  dropped (`#custommagic 8704 100 WN`, `#dt_aff 22`); a missing `#end` was added. Now the
  inspector agrees with all of them except where it reads `#descr` like `#msg` and keeps tabs.
- **Data safety** (7f3957a): backups on open and before each save (`%APPDATA%\Dom5Editor\backups`),
  a save that must read back with the same entities before it replaces the file, a warning
  before saving into Steam's workshop folder, a recovery copy on a crash.
- **Author reports** (6e463b8, 6b785d3): `ModReport` (the Validate window's "Save report for
  the author"): what goes wrong in game, lines the game ignores (with "did you mean"), numbers
  from another mod, things worth a look. Reports for the 15 latest files are in
  `publish/mod-reports/` (not committed).
- **Mounts** (b38b930): a MOUNT panel on monster pages (mount and co-rider with the mount's
  stats, riders, skilled rider).
- **Five mods made in the editor** (merge f7d8de1; agent): a nation, a ritual with an item, an
  event chain, a balance tweak, mercenaries and a poptype, each made by UI operations,
  reloaded and compared; now fidelity cases (33 checks). Fixed on the way: a new nation is
  `#selectnation` (the game ignores `#newnation N`'s number), a nation's name/epithet/era come
  first, ability badges were added as 0, `#clearrec` saved after the recruits, follow-up events
  lost their owner, a sprite-from picker, dangling references after a delete.
- **Edit tests for every type** (merge b1d02ff; agent): `base_types.dm` (every type the
  inspector compares, own/selected/copied, mounted units) and 31 cases t01-t31 (set, add/remove,
  references, reset, inherited removals, text edits, create/delete, mounts and their own
  stats), each also undone and redone byte for byte; stage 5 stress: ~300 generated edits per
  mod, expectations from the inspector's parse, seeds 1-25 pass. Bugs found and fixed: an
  item's `#weapon` was treated as a list (a change wrote `#clear` and lost values no command
  sets), text edits in CRLF files rewrote multi-line messages with LF. Full run: 76 checks, 74
  pass, 2 known-failing (a vanilla rider's sprite is a game value `#clearspec` removes; an
  inspector export limit), ~7.5 min with 6 jobs; `--quick` 65 checks in ~1.5 min.

Found in the mods (details in the reports): FR 0.95's 'Darkstalker Wars' message swallows its
`#removesite 2995` and `#gold 125`; Sombre has 10 events without `#rarity`, typos (`#stealty`,
`#copywweapon`, `#addrecnunit`, `#neednotneat`) and a doubled `#descr "#descr "`; its boost
and PD submods use nation 175, which Sombre 1.611 doesn't define; Confluence has events with
22 effects / 14 requirements and an `#addname` swallowing the next name; PS Bloodwar ends
without a line break (harmless: the last block is a nation) and refers to "Annointed Bulezau",
which its own `#copystats` renames to "Bulezau"; FasterForts uses nations 203-207 from other
mods.

Open: the editor can't load the mod a submod needs (only `check --with`); a loaded mod's
`#newnation N` is still taken as nation N; the game merges two events when the first has no
`#end` (reported, not modelled).

Later the same day (the user: "what the game reads is actually what's important"):
- **The game's reading as the referee** (61c59a3): `tools/dom6exe/gameread.py` replays the
  game's passes (rules from the exe in `tools/dom6exe/data/dmread-6.37.json`, with each command's
  sscanf format) on two files and compares what the game reads; fidelity stage 3 requires it for
  every save (80 checks). It found what the inspector couldn't: `#newmonster 7665 MAIN` taken as
  a name, `#custommagic 200`/`#path -1`/64-bit numbers dropped, a single dash cutting an open
  text, an empty `#descr ""` (fatal in game) from a doubled `#descr`. All fixed; all 16 workshop
  files, every line rewritten, read the same in game.
- **A report on every mod opened** (364cb51): a bar under the toolbar with what the check
  found, a report window grouped like the author report with Go to on every line, Save for the
  author, Copy. 0.4 s for Forgotten Realms.
- **Test build 0.10.0** (fdefb83): `publish/Dom6ModEditor-0.10.0-2026-10-07.zip`.
- **Mac/Linux** (agent, assessment only): `docs/CROSS_PLATFORM.md`. The core and CLI already run
  on Linux; the editor needs a UI port (Avalonia recommended) and a few core fixes.
- **Several mods at once, from the exe:** the game reads every enabled mod in turn, all 15
  passes for one mod before the next (0x140229c20), so a submod sees its parent only when the
  parent comes first in the list of enabled mods.

## 2026-10-06: pages like the game's, hints from the manuals, events

The user's review: the editor is "90% there"; streamline the monster page (stats grouped
like the game's unit window, easier random magic), icons with text, better hover hints,
then the big one: events, which chain. Done:

- **Monster page** (f85281d): stats in the game's three columns with icons and hints (normal
  values from the manual), leadership as class + bonus, a reset to the inherited value on
  every panel field; magic paths as chips with the game's path icons, add a path by clicking
  it; random paths as toggle rows with a chance (over 100% is a linked random: the manual);
  weapons, armor and nation recruits as tables; "+ New weapon/armor".
- **Icons** (`GameIcon`): the game's path and gem icons already shipped with the editor,
  vector drawings for stats and costs, in path/gem choices everywhere.
- **Hover hints** (717c266): `tools/command_hints.py` reads both manuals column by column
  (the old extraction ran the columns together, garbling badge descriptions) into
  `command_hints.json`: 1,874 commands with arguments, text and value tables. Badges show
  what a value means next to it. Six badge types fixed (#dragonlord was a monster link).
- **Events** (8bc5e6d, e59c2a3; design and analysis in `docs/EVENT_EDITOR.md`): the chain
  model (`Dom5Edit.Events`: codes, delays, variables, choices, enchantment and cause-event
  spells; checks), an event page that reads as a script (rarity, owner, message with tags,
  requirements by group and effects in order as sentences with values edited in place,
  links to the other end of every code/variable/enchantment), a chain map, follow-up /
  delayed follow-up / player-choice buttons, list filters. Save plan: line order (move
  up/down) and an entity placed after another (a delayed follow-up is the next event).

Found in DomEnhanced: codes -310, -311, -312, -315 are required by events but never set (those
events can't happen); a message `[Mountain of the Mystics` lacks its closing bracket; the
Serpent Cult events require monsters #7731, #7516, #7734, #7736 that exist nowhere.

Checks: quick suite 26/26 after the save-plan changes; sweep of 543 pages, 0 failures; edits
on DomEnhanced's event pages 120-140 ms (the event graph is rebuilt after each event edit; spells
only after spell edits).

Later the same day:
- **Pages reviewed:** weapons (stat block, damage type and strength as choices, qualities
  as checkboxes), armor and spell stat blocks (spell area/effects decoded), forms for
  mercenaries/poptypes/blesses/templates, nations (starting army, province defence with
  multipliers, pretender lists), sites (recruitable units as tables), spells that make an
  enchantment or cause an event list those events and make new ones, an event message
  preview. Monster and item abilities grouped by the manual's sections. Icons on common
  ability badges. List filters for every main type.
- **Speed:** pages are virtualized lists of parts (only what's on screen is built) and badges
  build only the parts they show: a nation page 1.2 s -> 0.26 s.
- **Fixed:** saving after "+ New poptype" crashed (no #newpoptype: new poptypes are
  #selectpoptype from 150, nametypes from 170, mercenaries #newmerc); moved lines keep their
  text as read; move + undo saves byte-identical.
- Two background jobs: vanilla events decoded from the exe (E-6), and the game's own icons
  read from its .trs archives at runtime (not shipped).

Then:
- **Vanilla events** (75b8b22, d648482): `tools/dom6exe events` writes the game's 3,302
  events; the editor lists them with the mod's (titled, numbered, searchable by message text:
  "comet" finds 8 of 5,000 in 0.3 s), messages read from the player's exe. The event graph
  links them (the game's chains read as in game) and `#delay` goes to the next record (the
  next `#newevent`, or N+1 after `#selectevent N`). A game event's page is read-only where a
  change would stack on the game's line. New checks from the exe: a `#newevent` without
  `#rarity` is lost (14 in DomEnhanced), more than 12 requirements / 20 effects are dropped (5
  DomEnhanced werewolf events have 14-15 requirements).
- **Game icons** (2443f60, merged): `tools/gameart/trs.py` and `Sprites/TrsArchive.cs` decode
  the game's .trs archives; `Data/game_icons.json` maps 151 keys (stats, costs, paths, gems,
  damage types, abilities by command) found in the exe's draw code. `GameIcon` uses the game's
  icon when the game is installed, the vector drawing otherwise. Read at run time, not shipped.

## 2026-10-06 (evening): vanilla data, the game's art and text, inspector values, tooltips

The user's review ("it looks amazing"), asking for: more vanilla data (nation descriptions,
mercenaries, anything missing), the inspector's computed values next to the stat boxes,
the game's icons compiled in for easier test deploys, tooltips on everything (and then
shorter), the file text editable if safe, and sprites settable on a new monster (copied
into the mod, saying so). Four background agents (worktrees) and the lead:

- **Icons compiled in** (222fabe): `tools/gameart/trs.py pack` -> `Resources/game-icons.pack`
  (151 icons, 83 KB, embedded). The user, a beta tester, has the developers' permission for
  game tools; unit/item/site sprites stay install-only (the user's call).
- **Sprites from the install** (d31b0ae): `dom6exe sprites` decodes how the game picks a
  monster's/item's image (group-relative numbers in monster.trs/item.trs; sites by path,
  level, look); `vanilla-sprites.json` ships only numbers. 4,129 monsters, 529 items, 1,406
  sites. Vanilla flags are composed by the game (pole, banner, emblem): not shown.
- **Game texts from the exe** (37f53d9): `dom6exe texts` finds the game's text lists from the
  parser code; 6,602 texts (monster/item/spell descriptions, spell details/portent/cure, nation
  description/summary/brief) read at start (~0.1 s), checksum-checked, display assets only.
- **Vanilla tables** (055094d): blesses (93), poptypes (82), nametypes (67 lists, 14,532
  names), mercenaries (78; read-only in the editor: the game can't select them) and nation
  colors/epithets in vanilla.dm; "end" records dropped; number 0 (bless/nation/event 0)
  handled as a real number. Mercs agree 1,092/1,092 with the inspector's CSV.
- **Inspector values** (508a730): `Dom5Edit/Derived` ports the inspector's unit maths (defence,
  protection, encumbrance, map move, ages, old age, costs, leadership, per-weapon attack and
  damage, item gem costs) with each value's parts; shown in brackets after the boxes and as
  "in game" chips. 99.92% of 131,936 values agree with the inspector on its own inputs; the
  differences are intended and listed in docs/DERIVED_VALUES.md.
- **Tooltips** (075e119, fe6511b): every control says what it does to what ("Make a copy of
  Claw #824 for Moloch to use instead, and open it"); snapshot `--tooltips` finds controls
  without one (0 on every page type). StringFormat doesn't apply to ToolTip: a Format
  converter. Then shortened to one sentence.
- **Edit as text** (075e119): `ModEditor.ReplaceText` diffs the block's text against its lines
  (unchanged lines keep text and place; a changed line replaces in place; new lines go where
  typed, saved as typed). Fidelity e18/e19; a DomEnhanced edit changes exactly those lines.
- **Sprite import** (fe6511b): header image slots (normal/attack/unmounted/... ; item picture;
  nation flag): click or drop a file; copied into the mod's `sprites/` folder (safe name, other
  formats converted to .png), the page says where; size checked against the manual.
- **Fixes:** a window handler kept every closed page alive (~1.5 MB each; a full sweep reached
  5 GB) (ada0c30); random magic skills (#magicskill 50-53) each add a pick (9d6de32).

Loose ends after the user's answers (random skills stack; descriptions.dm stays; commander +2
map move, earth protection and path resistances confirmed):
- Vanilla mercenary pages read-only throughout, with "New band from this one" (#newmerc
  copy) (e9edcff).
- #clearmercs in the catalog (a mod-level command in the merc parser's entry chunk).
- Intrinsic resistances: #fireres/#coldres/#shockres/#poisonres 100 = 15 (+10 with the aura,
  undead/inanimate/poison cloud for poison), from the exe's getter; vanilla.dm already has
  totals, so the inspector's +10 on Summer Lion etc. is its Dom5 rule.
- Bless effect 550 is the "incarnate only" marker (09fd570); 551 still unknown.
- Vanilla nation flags (79bd0b6): the game builds image nation+1 of flag.trs at start and after
  mods load (pole 501, cloth 502 tinted by #color, border 503 by #secondarycolor, emblem 500 +
  nation up to 135); `dom6exe flags` writes the recipe (numbers only), GameArt.NationFlag
  rebuilds it from the install: 110/110 pixel-identical to the Python composition. A mod's
  #color recolors a vanilla flag; #flag wins. Shown on the nation page and list rows.
- Full fidelity suite 36/36; full sweep 12,747 pages, 0 failures, 260 MB.

Smaller screens and a test build: the toolbar is a grid that shrinks ("go to" 140-400 px, the
mod name trimmed), the tabs stay in one scrolling row (ae0ee00); checked at 900-1600 px. The
game is found through Steam's libraries (registry + libraryfolders.vdf) or a folder picked in
the ▾ menu, and the status bar says when it isn't (880f528). `tools/publish.sh` + a README.txt
for testers: publish/Dom6ModEditor-0.9.0-2026-10-06.zip (64 MB, self-contained, not in git),
run from a fresh unzip.

Worktree note: a worktree checks out docs/DomEnhanced2_13.dm with LF endings, which makes
DomEnhanced stage 3 fail with hundreds of diffs (multi-line descriptions); use main's CRLF copy.

## 2026-10-05 (later): a complete editor, rebuilt on the resolver

The user asked for a complete mod editor (every entity editable, real panels for the
structured parts), with every existing view verified and rewritten where needed, plus
browsing and navigation (lists, links between entities, hiding vanilla). Roadmap:
`docs/EDIT_FLOW.md`, "Roadmap: a complete editor" (E1-E5). Done, in order:

- **Resolver** (`Dom5Edit.Resolve`, cf7d0e6): what each entity is in game, every value with
  its source (own line, vanilla, copied from X). Replays the save's lines (`SavePlan`, shared
  with the exporter) with the game's rules; replace-or-append per command from the exe
  (`tools/dom6exe` catalog now exports each command's effects: fields, abilities, flag bits,
  argument ranges, stored constants). Checked against the inspector on 5,946 DomEnhanced units:
  agrees except where ours follows the exe (#clearmagic, #morale not read). Found and fixed a
  parser bug: `#weapon 474 "Golden Sword"` was read as a name.
- **Core edit layer** (`Dom5Edit.Editing.ModEditor`, b206a29): every change an undoable edit
  with exact undo/redo; copy-on-write for vanilla; removing an inherited value writes `#x 0`
  or the group's clear plus the rest of the group; added copies/clears saved before the
  entity's own lines. Fixtures e12-e17.
- **GUI rebuilt** (0ee2316, 3bcbad5): session, tabs, light list rows, one generic entity page
  from the JSON badge sections + type panels (monster weapons/armor/magic/cost/leadership/
  body/item slots; spell paths/cost/effect with the summoned monster; item type/paths;
  site path/level/rarity/gems), "other lines", game values, removed abilities. The old
  per-type views, view models, EditCommands and ChangesMod are gone (-13k lines).
- **Speed** (bd463c1, 68bbe28): per-edit cost on DomEnhanced from 2.1 s to 50-85 ms end to end
  (cached rules; save-plan fast path; lazy per-entity resolver; picker lists updated in place).
- **Browsing** (094bced): used by, go to (Ctrl+P), Ctrl+F, filters kept per tab, back/forward
  (Alt+arrows, mouse buttons), list follows navigation.
- **Coverage** (25dffb3): every command the game reads has a badge or a panel
  (`tools/badge_fill.py`, `tools/editor_coverage.py`).

Verified with `Dom5Editor --snapshot` renders and scripted sessions (set, add, remove, field,
new, delete, undo/redo, jump, dump, save) and the fidelity suite: full 34/34, quick 26/26.
DomEnhanced still saves byte-identical.

Later the same day:
- **More panels:** monster body shape and item slots as counts (following the body shape's own
  slots, read from the exe); armor type; nation recruits/commanders as lists; "Copy & edit"
  (a unit's own changed copy of a weapon, armor or unit, in one undo step); derived values
  (#teleport's map move 100); long texts (#msg, #summary, #brief, #details) in boxes; one add
  box per page; "in the file" shows the entity's lines as the save writes them.
- **Lists:** sprites and key stats per row, sort by ID or name, filters kept per tab.
- **Robustness and packaging:** crash guard with errors.log; remembered window layout and
  recent mods; delete asks first when others refer to the entity; a clear error when
  vanilla.dm is missing; data beside the exe; `tools/publish.sh` (one self-contained exe,
  tested); README rewritten for the editor.
- **Game rules found and fixed along the way:** #clearrec clears only the recruitment list and
  #clearnation the abilities and gods (start units and terrain recruits were taken by
  #clearrec before); a second `#selectbless "Name"` block continues the first (it made a
  duplicate); a spell selected by name in one block and by ID in another gets both blocks;
  `#weapon 474 "Golden Sword"` is weapon 474.
- Every entity type checked in scripted sessions (monsters, weapons, armor, spells, items,
  sites, nations, events, mercenaries, poptypes, nametypes, blesses, templates): each edits and
  saves exactly.
- **Typing:** value boxes commit on Enter or leaving them (not after a pause mid-number); Tab to
  the next value keeps the cursor there after the page rebuilds.
- **Sweep** (`--sweep`): every mod entity's page on DomEnhanced opens (383 pages, none fail;
  fixed monster-tag references, nametype pages as one names box, lazy picker filtering). The
  slowest page is a big nation at ~2 s (many reference badges): a candidate for later.
- Full suite 34/34; DomEnhanced saves byte-identical.

## 2026-10-05: game data from Dominions6.exe

Plan agreed with the user:
1. Map each monster command to the record field it writes (stats, flag bits, slots): complete
   the monster layout and the read-only list.
2. Apply the rules read from the exe (#clearmagic keeps magic boosts, #clearspec removes them,
   #copystats copies #xpshape and the name) in Dom5Parser and the inspector; tightrein =
   #undisleader in the exporter.
3. Extend tools/dom6exe to weapons, armor, items, spells, sites, nations.
4. Generate vanilla.dm from the exe; compare with the inspector-generated one.
5. Give Dom5Parser the command catalog (commands the game doesn't read; read-only abilities).
6. Then: copy-edit rule C and original-order saving.

### Vanilla data now from the exe; game defaults in the editor

The user (2026-10-05): migrate away from the inspector; show the inspector's defaults as
defaults, and leadership too; limb armor isn't used in game (body and head only); part-6 armor
is what modders `#copyarmor` to get unusual protection (e.g. body protection on a helmet).

- `vanilla.dm` is now written from Dominions6.exe (`dom6exe.py vanilla --out vanilla.dm`); the
  copy under tools/dom6exe/data is gone. Body armor is written with its torso value; 6 part-6
  armors stay read-only. Dom5Parser loads it with 49 warnings (48 poptype references vanilla
  doesn't define, one vanilla monster with nonexistent armor 502). Quick suite unchanged.
- Fidelity stage 2 (the inspector's own vanilla export) retired. Stages 3-4 keep the inspector
  as an independent parser for comparing saves.
- Editor: game defaults shown as defaults (resource size = size, spirit sight for horrors, cast
  time 100); the leadership bonus (ability 160) as a read-only "Leader bonus" badge; every
  entity view has a collapsed "Game Data (read-only)" group listing the `-- ro:` values.
  Built, not run.
- Answering the user's question about the byte-identical save: the data is fully loaded and
  edited in the normal model; original text is only used to write unedited lines. Because that
  would hide export bugs, stage 3 also saves DomEnhanced with every line regenerated (baseline 6,
  all equivalences); that check found references with no value being written as 0 (fixed).

### Original-order saving (step 6): DomEnhanced saves byte-identical

The flow map the user asked for is `docs/SAVE_FLOW.md`: parse → source blocks → live model →
edits → save, with the placement rules, the reasons, code locations and tests.

- Parse records each `#new`/`#select` block with the properties it parsed, in order
  (`SourceBlock`), plus every line that gives the entity nothing (comments, blank lines,
  `#dependency`, unknown commands, commands the entity doesn't accept) where it was, and each
  command's text as read (`Property.RawText`). Multi-line strings keep their trailing spaces.
- Save (`ModExporter.WriteInSourceOrder`, the default for a mod read from a file) writes the
  blocks in file order. An unedited line is written as read (`Property.SaveText`: still equal to
  its export text at the first `Resolve()`); an edited value stays where it was; a removed one
  is dropped; a replacement goes in its slot; an added property goes at the end of the entity's
  first block, or its last if a later block sets the command, clears its group or copies over
  it. Properties a later clear or copy took out at parse are still written (the game reads them).
- Rule C comes from this: a template edit is saved in the template's block, before its copies,
  so the game carries it to copies that don't set the field. `Mod.NormalizeCopies` is now only
  for the canonical writer (`PreserveSourceOrder = false`, `roundtrip ... canonical`).
- The editor's Save (`ChangesModExporter`) now writes the loaded mod this way, then vanilla
  overrides and new entities (old merge kept as a fallback when entities were removed).
  `Dom5Tests edit ... editorsave` exercises it; it writes the same files as `Mod.Export` on all
  edit cases.
- Type fixes found on the way: `#portent` is text, `#blessbonus` one number (the exe), negative
  bitmasks (`#nextingeo -1`) kept.

Results: DomEnhanced 2.13 load → save is byte-identical to the file (130k lines); stage 3 on it
873 → 0. `name_before_copy` and `e07_template_cascade` pass (no known failures left). New stage-4
cases: e09 replacement in place, e10 added ability after a later `#clearspec`, e11 added ability
on a template reaching its copies; the base gained a copy that sets hp itself (e07 checks it
keeps 15). e09 and e10 fail when their rule is broken. Full suite: 30 checks, 30 pass,
no known failures; DomEnhanced stage-3 baseline now 0.

Not done: the editor still edits vanilla entities on the shared vanilla object (saved after the
mod's blocks via `ChangesMod`); its display of copies walks live values rather than replaying
the file. Both are in SAVE_FLOW.md's known gaps.

### Read-only game values in Dom5Parser (step 5, second part)

- The parser reads the exe-written vanilla data's `-- ro: label = value` lines into
  `IDEntity.GameValues` (never exported). All 3,491 load.
- Editor: the monster view lists them under "GAME DATA (read-only: no mod command sets these)"
  in the Abilities group; for a mod's edit of a vanilla monster it shows the vanilla entity's.
  Built, not run. Other entity views don't show them yet; copies don't inherit them in the
  display yet.
- Takes effect once the editor loads the exe-written file instead of the inspector's
  vanilla.dm. That switch waits until after original-order saving (step 6), which removes the
  save path's dependence on vanilla values.
- On the user's question (2026-10-05): the Python tool stays a developer-side extractor; its
  outputs (vanilla data, command catalog) ship with Dom5Parser. Reading the exe at runtime
  would need a disassembler library in C# and re-run fragile analysis on every start.

### Command catalog in Dom5Parser (step 5, first part)

- `dom6exe.py catalog` writes `Dom5Edit/GameData/game-commands-6.37.json` (embedded resource):
  per entity type, the commands the game's parser compares against, and whether the list is
  complete. `GameCommandCatalog.IsRead(entity type, command)` answers true / false / unknown.
  Context fix: the bless parser is the one with `#selectbless`/`#clearfx`; the function I had
  called bless reads `#form`/`#domstr`/`#prison`/`#favrit`/`#researchgoal`, i.e. Templates.
- Dom5Parser now knows the 59 commands the game reads that it didn't (`#grandcom`, `#spec2`,
  `#mrhalf`, `#startunittype3`, `#sabbathmaster`, `#statsiege`, `#bugshape`, `#req_school`, ...)
  and accepts on items the commands the item parser reads (`#patience`, `#spikes`, `#dread`,
  ...), plus `#cure`/`#reqno*`/`#aiassmod` on spells and `#nametype` on nations. The exe-written
  vanilla file now loads with no unknown commands (49 warnings left: poptype references, which
  vanilla doesn't define as entities, and one vanilla monster pointing at nonexistent armor 502).
- Parsing a command the game doesn't read for that entity type adds a `NotReadByGame` parse
  issue; the command is kept. DomEnhanced 2.13 has 223: mostly inspector display hints
  (`#mountedinspector`, `#iceprotinspector`, `#protinspector`), and also `#morale`, `#regen`,
  `#amphibious`, `#colres`, `#hpoverslow`, `#uwguard*` and `#clear` on nations,
  `#batstartsum8d6`/`9d6` on items, `#prec`/`#nreff`/`#damage` on a weapon. `Dom5Tests mod` prints them.
- Editor: badges for those commands show "n/r", their value and reference can't be edited (they
  can still be removed), and they aren't offered in the Add list. Built, not run (no display
  here). `#unseen`, `#plaguedoctor`, `#mindcollar` were on the old hand-made read-only list but
  the game reads them.
- Quick suite unchanged: 14 pass, 2 known failures, DomEnhanced stage 3 at 873.

### Nations from the exe: step 4 done for every type

111 nations added; `data/vanilla-6.37.dm` now holds every vanilla entity type (887 weapons,
298 armor, 4,138 monsters, 1,475 spells, 531 items, 1,407 sites, 111 nations). Nation parsers
branch on "name or number" and "-1 removes", so the directly-set abilities are found by
following the code to the first setter call (both ways at a conditional jump). Read-only:
3,491 values in all, about a third of them sprite numbers and frame counts.

Step 4 comparison summary (details per type in tools/dom6exe/README.md): wherever the two
disagree, the exe's version is the one the game's parser would produce. The inspector's data
lacks a lot (body shapes, sounds, sprites, start units and defenders, ~300 abilities) and in
places names a different command than the one that stores an ability, or a command the game
doesn't read at all.

### Sites from the exe (step 4)

1,407 sites added (the inspector has 1,253). Generic-handler arguments are now read by
following register constants from the previous call, including registers a parser only ever
sets to one value; this resolved every monster command's minimum and the item restriction bits
(`#nofemale` is 0x80000000). Earlier sections unchanged apart from line order on one item.

### Spells from the exe (step 4)

1,475 spells added (345 read-only values, mostly sprite frame counts the sprite commands
don't store). Facts in tools/dom6exe/README.md; one worth knowing: `#nogeosrc`/`#nogeodst`
clamp to 2^31 - 1, so a mod can't set terrain bits above 31 the way vanilla spells have them.

### Items from the exe (step 4)

531 items added to `data/vanilla-6.37.dm`. Items use the generic handler and the monster
ability numbers; the special cases (`#constlevel` halves, `#type 9`/`10`, `#restricted`, OR-ed
restriction bits) are in tools/dom6exe/README.md. 666 read-only values, 529 of them sprite
numbers. Earlier sections unchanged.

### Weapons and armor from the exe (step 4)

`vanilla` now writes weapons, armor and monsters into one file,
`tools/dom6exe/data/vanilla-6.37.dm` (887 weapons, 298 armors, 4,138 monsters). Shared
`ContextModel` per entity parser: fields stored (constant or parsed argument), flag bits set
and cleared (the default bits from `#clear`), abilities through the type's own setter. New in
the parser model: bits cleared with `add reg,-bit`, ability values passed into shared tails from
the parsed argument, constants built from a register known to be 0 (`test eax,eax; jne` falls
through with eax = 0, then `lea r8d,[rax+0x64]` = 100).

Compared with the inspector: weapons differ mostly where the inspector lacks data (sounds,
sprites, ~15 weapon abilities) or names commands the weapon parser doesn't have (`#flammable`
is `#woodenweapon`, `#nofirebless` is `#iceweapon`, `#defnegate` is `#defroll`). Armor: most
body armor has a stronger torso than limbs, which no command can write (read-only per part; the
inspector writes a weighted `#prot`). Details in tools/dom6exe/README.md.

### Vanilla monsters written from the exe (step 4, monsters)

`dom6exe.py vanilla` (new `tools/dom6exe/vanilla_dm.py`) writes every vanilla monster as
`#selectmonster` commands: the parser read backwards, each stored value written as the command
that stores it. Output: `tools/dom6exe/data/vanilla-monsters-6.37.dm` (4,138 monsters).

To get there the parser model learned: register constants through moves and `lea`, values
passed into shared tails (`#blind`, `#assassin` jump into `#spy`'s setter call), arithmetic on
the argument (`#eyes N` stores N - 2), 64-bit minimums in the generic calls, and the generic
handler itself (no-argument commands, optional argument kind 5, offsets, append/OR kinds).

Also read from the code (details in tools/dom6exe/README.md): the ability getter's intrinsic
resistances (+15, +10; `#poisonres 100` sets the flag rather than 100), item slots (ability 182,
else by the `#noitem` flag; the body shape doesn't matter), leadership (class + 157 + 158 + 159
+ 160; 160 has no command), fixed names (one init function copies all 417), the magic table.

Compared with the inspector-generated vanilla.dm: every difference has a cause, tabled in the
README. Mostly the inspector writes derived values (ressize = size, spirit sight of horrors,
leadership folded into classes) or lacks data (body shapes, the "no items" flag, 153 fixed
names, ~300 abilities without a CSV column, 47 monsters); 40-odd cases where it names a
different command than the one that stores the ability (`#domsummon` is `#domsummon2`, ...),
and 459 `#startage` values about 10% above the stored ones. 1,605 stored values have no command
and are read-only lines.

Choices made, for the user to confirm:
- Resistances and item slots are written as the value the game uses (`#poisonres 25` for the
  intrinsic flag, `#itemslots` on every monster). Editing them is safe: those commands clear the
  flag/replace the value, so the game then uses the value written.
- The heat/cold aura flags (+3) and the leadership bonus 160 are read-only, not `#heat 3` or
  `#command 25`: `#heat`/`#command` add to them, so an edit wouldn't give the value written.
- Not yet used by Dom5Parser: vanilla.dm is still the inspector's. Switching needs the other
  entity types and a way for Dom5Parser to show the read-only lines (step 5).

### All tables and parsers (step 3, in progress)

- `dom6exe.py tables`: all seven vanilla tables located (monster 888 B, weapon 152, armor 104,
  item 528, spell 280, site 312, nation 3000), with exact matches of inspector CSV columns.
- `layout` now works for every entity parser (branch = from a command name to the next one;
  register clobber tracking). Weapon record: dmg 0x28 (int64), att 0x30, def 0x32, effect type
  0x34 (#dt_*), len 0x36, range 0x38, nratt 0x3a, ammo 0x3c, flags 0x40 (43 bits),
  secondaryeffectalways 0x50, flyspr 0x52/54, explspr 0x56/58, aoe 0x5a, sound 0x5c, rcost 0x5e.
  Items keep an ability list at 0x78 (24 pairs) plus flag words at 0x1f8/0x200/0x208.
- Plan from here: invert the parser (record value -> the command that writes it) to write
  vanilla.dm straight from the exe, monsters first; check it against the inspector-generated one.

### Rules from the exe applied (step 2)

Dom5Parser (`PropertyGroupMap`, `IDEntity`):
- `#magicboost` is no longer in the magic group: `#clearmagic` keeps it, `#clearspec` removes it.
- "Stats" (cleared only by #clear) are exactly the monster record's fields; #eyes, #pathcost,
  #startdom, #drawsize, ages, #ressize, #homerealm, #nametype and the other abilities are
  cleared by #clearspec. Leadership classes, magic being and body types are cleared only by
  #clear (they live outside the ability list).
- Copies include #xpshape / #growhp / #shrinkhp / #labxpshape (#copystats copies the whole
  ability list).

Inspector (fork ff69107): #clearspec / #clear modelled on the game; tightrein, ownblood,
isashah, researchwithoutmagic are the abilities of #undisleader, #tmpbloodslaves,
#userestricteditem, #magicimmune (manual text agrees) and are now exported.

Docs corrected: COPY_INHERITANCE_REDESIGN (ID-relative commands are copied),
PROJECT_EVALUATION 4b (three game-rule questions answered).

Suite: 27 checks, 24 pass + 2 known failures + DomEnhanced stage 3 (900 -> 873; the two new
fields there are the known save-order issue, copies of vanilla units the mod edited, now
visible on more fields because the oracle's #clearspec is accurate). Baseline updated.

### Monster record layout from the parser (step 1)

`dom6exe.py layout` reads, for each monster command, what its branch in the parser writes:
record fields, flag bits (including read-modify-write of the flags words), and abilities set
through shared code. Monster record (888 bytes): name 0x00, sprite 0x24, 12 stats from 0x28
(ap, mapmove, size, hp, prot, str, enc, prec, att, def, mr, mor), 48 abilities from 0x40,
weapons 0x340 (10), armor 0x354 (5), gcost 0x35e, rcost 0x360, rpcost 0x364, flags 0x368
(64 bits, 44 mapped to commands: #amphibian = 0x8, #flying = 0x1000, #female = 0x80000000,
...), flags2 0x370 (leadership classes, magic being), body type 0x374.

Checked against the inspector's unit CSV: all 15 stats and costs agree on all 4,091 monsters.

Read-only (vanilla data no command sets): 107 abilities, 8 flag bits (some likely the heat/cold
aura bits set by #heat/#cold arithmetic the analysis doesn't model yet).

Done so far today:
- `tools/dom6exe` (e706e17): commands per entity type with ability numbers, the vanilla
  monster table, read-only monster abilities. Game rules read from the code are in its README.
- Shared names resolve to the lowest id in Dom5Parser and the inspector (87beb45, fork d0b67d1).
- Stage 1 (inspector self-check) on DomEnhanced: 6,923 -> 6 unexpected differences.
