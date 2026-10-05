# Work log

Running log of the autonomous work sessions: what was done, what was found, what's next.
Newest entries at the top. Commits are local unless noted; the user pushes.

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
