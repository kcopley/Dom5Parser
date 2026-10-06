# Work log

Running log of the autonomous work sessions: what was done, what was found, what's next.
Newest entries at the top. Commits are local unless noted; the user pushes.

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
