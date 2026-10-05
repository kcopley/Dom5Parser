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
