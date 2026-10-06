# dom6exe: game facts straight from Dominions6.exe

The dom6inspector's game data (and through it our `vanilla.dm`) comes from a CSV extraction
tool that has no current maintainer. The exe itself holds both the vanilla tables and the
`.dm` parser, so this reads them directly. Only the exe is needed: no other game files.

```bash
python3 tools/dom6exe/dom6exe.py commands  --out tools/dom6exe/data/commands-6.37.json
python3 tools/dom6exe/dom6exe.py readonly  --out tools/dom6exe/data/readonly-monster-abilities-6.37.json
python3 tools/dom6exe/dom6exe.py monsters  --out monsters.json      # ~3 MB, not committed
python3 tools/dom6exe/dom6exe.py vanilla   --out vanilla.dm    # the editor's vanilla base
python3 tools/dom6exe/dom6exe.py catalog   --out Dom5Edit/GameData/game-commands-6.37.json
```
Options: `--exe PATH` (or env `DOM6_EXE`; default
`/mnt/c/Games/Steam/steamapps/common/Dominions6/Dominions6.exe`), `--inspector DIR` (a
dom6inspector checkout; its unit CSV names ability numbers, as hints only). Needs Python 3.8+
and GNU `objdump`. The exe is never copied into the repo.

## What it reads

| Output | What |
|---|---|
| `contexts` | Every command the game's `.dm` parser compares against, per entity type (monster, item, weapon, armor, spell, site, nation, merc, poptype, nametype, event, global, bless, sound, and the top level `#new*`/`#select*`). A command that isn't listed is ignored by the game. |
| `ability_keys` | For monster and item commands: the numbered ability each one sets (`#xpshape` = 1145). Monsters and items share the numbering. |
| `monsters` | The vanilla monster table: name, 12 base stats, up to 48 `(ability, value)` pairs. |
| `not_settable` | Abilities vanilla monsters have that no monster command sets. An editor shows them, read-only. `possibly_set_by` lists commands whose own handler uses that number (e.g. `#blind`, `#assassin`, `#unmountedspr1`); for small numbers that is often noise. |
| `catalog` | For Dom5Parser (embedded in Dom5Edit): per entity type, the commands the game reads, and whether that list is complete (the event parser also reads `#2d6units`-style commands by pattern). |
| `vanilla` | All vanilla weapons, armor, monsters, spells, items, sites and nations as `#select*` commands (`vanilla_dm.py`): each stored value written as the command the parser stores it with. Values no command can store are `-- ro:` lines (shown read-only). |

## How it finds things (no hard-coded addresses)

- **Parser functions:** each entity parser compares the line against its own command names, so
  it's located by an anchor command (`#copystats` for monsters, `#copyweapon` for weapons, ...).
  The function bounds come from the exe's exception table (`.pdata`).
- **Generic handlers:** most simple commands go through one of two functions called with
  (line, name, ability number, ...). They're the two callees most often called with a command
  name. The ability number is a constant, often written as `r9 + N`.
- **Ability setter:** the function `#magicboost 51 <n>` calls after loading ability 20.
- **Monster table:** located from known vanilla names (1 Logrian Slinger, 2 Standard, 3
  Serpent Cataphract), so the record size falls out (888 bytes in 6.37). The parser caps
  monster numbers at 19999. Every run checks the layout against Heavy Cavalry (20).

## Game rules read from the code (6.37)

- `#clearmagic` empties only the magic-skill table. Magic boosts are abilities 10-22 on the
  monster, so they survive it.
- `#clearspec` clears the whole ability list (magic boosts and `#xpshape` included) and the
  flags word.
- `#clear` is `#clearmagic` + `#clearspec`, plus a reset of the base stats to defaults and of
  another block of slots. The name is kept.
- `#copystats` copies the name, base stats, every ability (so magic boosts, `#xpshape`,
  `#growhp`/`#shrinkhp`), the flags and slot blocks, and replaces the target's magic skills with
  the source's. A `#name` above `#copystats` is overwritten.
- `#djinn`, `#humanoid`, `#quadruped` and the other body types are read (they set item slots).
- Documented commands the parser never compares against: `#limitedregen`, `#statbreak`,
  `#domversion`. `#stunimmunity` (which the inspector exports) isn't a monster command.
- Generic handler arguments are read by following register constants from the previous call,
  including callee-saved registers a parser only ever sets to one value. Where the kind is
  kept in a register the parser also uses otherwise (sites, some spell and nation commands),
  it stays unknown and the command is treated as taking a plain number.
- Generic handler: a command with min = max takes no argument; kind 5 takes an optional one
  (default = the handler's last parameter); otherwise the argument is clamped and an offset
  added (`#incscale 1` stores 101, `#coldrec 2` stores 12). Tens digit 1: the command appends
  (repeatable, `#batstartsum1`), 2: it ORs into the current value.
- Abilities: with overwrite a value replaces the existing one and 0 removes it; repeatable
  commands append. The game reads the first of repeated non-repeatable ones (some vanilla
  monsters have two `#maxage`).
- `#eyes N` stores N - 2. `#horrormark` stores 5. `#sailing a b` stores a (999 if below 1) and
  b (10 unless 1-10) as two abilities. `#gemprod g n` is ability 30 + g. `#teleport` sets the
  teleport flag and map move 100; `#blink` only the flag. `#magicskill` takes paths 0-9 and
  50-52.
- Intrinsic resistances: `#fireres/#coldres/#shockres/#poisonres 100` set a flag instead of a
  value; any other value clears that flag. The flag counts 15, +10 for fire with the heat-aura
  flag or a heat aura, cold with the cold-aura flag or a cold aura, poison for undead or
  inanimate or with a poison cloud (the ability getter, 0x1401c5ae0). The heat/cold aura flags
  add 3 to `#heat`/`#cold`, which don't clear them. Spirit sight is 1 for horrors.
- Item slots: ability 182 (`#itemslots`, and set by `#mountedhumanoid`, `#quadruped`, ...),
  else 0xc0000 when `#noitem`'s flag is set, else 0xf2206; `#humanoid` and `#noitem` remove
  the ability. The body shape doesn't affect slots.
- Leadership: class + abilities 157 (`#command`) + 158 + 159 + 160. No command sets 160, the
  bonus 122 vanilla monsters carry (it doesn't apply to `#noleader` ones); `#command` doesn't
  clear it, so it's read-only rather than written as `#command`.
- Fixed names: game init copies all 417 vanilla fixed names (pretender gods included) into the
  table `#fixedname` writes.
- `#demon` and `#undead` also set the "almost undead" bit; 128 vanilla demons and undead lack
  it (written as `#almostliving`, which clears it).

## Weapons and armor (6.37)

- Weapon `#clear` (0x140227f90) sets the defaults: dmg 2, dt_normal, len 1, nratt 1, sound 7,
  no sprites, flags = full strength + a "not magic" bit that `#magic` clears.
- Weapons keep up to 7 (ability, value) pairs at +0x60 (setter 0x1402fd530): `#woodenweapon`
  268, `#iceweapon` 482, `#speedmult` 302, `#flail` 935, `#beam` 938, `#skip` 900, `#range0`
  930 = 100, `#range050` 930 = 50, `#melee50` 931 = 50, `#fireifhit` 911 etc. `#ironweapon`
  sets a flag bit and ability 266.
- `#range N` also sets len 0 and, if ammo is 0, ammo 12. `#explspr` always stores 9 frames
  (103 vanilla weapons have 5). `#secondaryeffect N` stores N, `#secondaryeffectalways N`
  stores -N in the same field. `#thirdstr` does what `#halfstr` does.
- Armor protection is a list of (body part, value): `#prot` sets the part(s) of its type
  (shield 5, helmet 1, else torso, arms and legs alike); `#protparts h b` sets head and body.
  Most vanilla body armor has a stronger torso than arms and legs; the game uses body and head
  protection only (the user), so it's written as `#prot <torso>` (or `#protparts <head>
  <torso>`). Part 6 (Twisting Thorns, Skull Necklace, Mail Barding, the bracers, Flame Helmet)
  has no command: modders reach it with `#copyarmor` of one of these, e.g. to give a helmet
  body protection. Those stay read-only. Armor abilities: 4 numbers at +0x48, values at +0x58
  (`#magicarmor` 557 = 1; 11 vanilla armors have 2).
- Commands the inspector writes that the weapon parser doesn't have: `#flammable` (is
  `#woodenweapon`), `#nofirebless` (`#iceweapon`), `#defnegate` (`#defroll`), `#mrcheckhalfdmg`
  (`#mrhalf`), `#usedinmelee`, `#dismounted`, `#hithead`, `#aironly`, `#demonimmune` (flag bits
  and abilities with no command: read-only).

## Items (6.37)

- Item `#clear` (0x1402290d0): main path 0 level 1, no second path, type 8, no flags.
- `#constlevel N` stores N / 2 (the manual's levels are 1, 3, 5, ...: written as 2s + 1).
  `#mainpath` also raises the main level to at least 1. `#type 9` stores type 6 plus ability
  1423 = 1, `#type 10` stores type 9. `#restricted N` appends ability 278 = N. `#spell` and
  `#autospell` are names at +0x30 and +0x54. `#magicboost` and `#gemprod` use the monster
  numbers; abilities go through the same generic handler.
- The restriction commands (`#nomounted`, `#noundead`, `#nofemale`, `#noinanim`, ...) OR a bit
  each into ability 1417.
- The item ability getter has no intrinsic flags. Flag bits no command sets look like battle
  buffs: 0x1f8 bit 0x8 is what the inspector writes as `#airshield 80`.
- Compared with the inspector: it lacks `#hp`, `#itemdrawsize`, ~40 more abilities and repeated
  `#nationrebate`s; it expands `#magicboost 51` and `#elementrange`/`#allrange` into one line
  per path; it writes `#bers` for `#autoberserk` and `#type 9` where the record has 6 + 1423.

## Spells (6.37)

- Spell `#clear` (0x140258af0): school -1, path 0 (fire) level 1, no second path, fatigue
  20, effect 2, range 5025, damage 10, nreff 1. `#path n p` / `#pathlevel n l` store at
  +0x26 + n / +0x28 + n. `#flightspr`/`#explspr` also store 1 / 9 frames. Abilities: 15 int32
  numbers at +0x64 with int64 values at +0xa0; `#restricted N` appends ability 278 = N.
- `#reqspellsinger`/`#reqtaskmaster`/`#reqseduce`/`#reqplant` append ability 718 = 616 / 379 /
  298 / 500 (`#reqno...` the same on 719).
- `#nogeosrc`/`#nogeodst`/`#onlygeosrc`/`#onlygeodst` clamp to 2^31 - 1: terrain bits above 31
  (vanilla Second Sun's 34359744512) can't be set by a mod.
- Compared with the inspector: it omits zero fields and `#sound`, `#flightspr`, `#strikesound`,
  `#spec2` and ~20 abilities; it writes commands the spell parser doesn't have
  (`#coldsummon`, `#uwsummon`, `#uniquetarget`, `#requiresench`) and other names for some
  abilities (`#onlyowndst` for `#onlyfriendlydst`, `#preventcast` for `#reqnoplant`,
  `#extraeffectgeo` for `#nextingeo`); `#casttime 100` where the record has no value.

## Sites (6.37)

- Site record: look +0x28, path +0x2a, level +0x2c, rarity +0x2e, 16 (ability, value) pairs
  from +0x30, terrain mask +0x130. `#gems p n` stores ability p + 1 = n.
- Vanilla sites use abilities no site command writes: 551/552/554/555 (the inspector writes
  them as `#summon`, which is 550) and 100-106 (written as `#decscale`, which is 102).
- Compared with the inspector: it lacks `#look`, `#claim`, `#gold`/`#minegold` on some, the
  per-path ranges, and 154 sites; it writes `#incunrest -50` for `#decunrest 5`.

## Nations (6.37)

- Nation record: name, epithet +0x24, era +0xac, 200 abilities (int32 numbers from +0xb0,
  int64 values from +0x3d0), the recruitment list +0xa10, the god list +0xab8.
- `#addreccom`/`#addforeignunit`/`#addforeigncom` insert into one int32 list with section
  markers -2/-3/-4 (`#addrecunit` before -2; -1 ends it). `#addgod N` appends N to the god
  list, `#delgod N` appends -N.
- Nation ability setter 0x14022a300; 0x140258820 removes an ability (`#startcom` first removes
  start units 91-96; `#clearsites` removes 52). `#startcom` 90, `#startunittype1-3` /
  `#startunitnbrs1-3` 91-96, `#startscout` 97, `#hero1-10` 139-148 (-1 removes), `#startsite`
  52 (appends), the rest through the generic handler.
- Not read yet: `#color`/`#secondarycolor` (floats, set through a helper), `#flag`, and the
  texts (`#descr`, `#summary`, `#brief` live outside the record).
- Compared with the inspector, which exports names, recruitment, heroes, start sites, home
  realms and cheap gods: the exe adds start units, defenders, wall and guard units, temple
  picture, fort era, god lists, AI and dominion settings (~100 commands per nation). The
  inspector's 25 extra nations are empty slots it names `nation_35` etc.

## Vanilla monsters compared with the inspector's vanilla.dm (6.37)

`vanilla` writes 4,138 monsters (the inspector's vanilla.dm has 4,091). Every flag bit a
command can set is written; 1,605 stored values have no command (read-only: 311 body shapes,
262 vanilla unmounted sprites, 122 leadership bonuses, 153 aura flags, 46 flags2 bits, and
abilities). Where the two files differ, by cause:

| Cause | Commands (monsters) |
|---|---|
| The inspector writes defaults or derived values the record doesn't hold | `#ressize` = size (3857), `#spiritsight` for horrors (22), leadership bonus folded into a class and `#command` (`#command` 109, leader classes 47) |
| The record holds values the inspector doesn't export | body shapes (`#quadruped` 287, `#miscshape` 190, ...), `#noitem` (581; the inspector writes only its slots), 153 fixed names, `#almostliving` (128), `#grandcom` 68, `#transformation` 64, `#bravemount`, `#icenatprot`, `#praise`, `#ainorec`, ... (~300), repeated `#batstartsum*`/`#armor`, 47 monsters |
| The inspector names a different command than the one that stores the ability | `#domsummon`/`#domsummon2` (22), `#battlesum5`/`#battlesum4` (4), `#summon5`/`#makemonsters5`, `#domimmortal` (is `#springimmortal`), `#secondshape` (ability 197 has no command; `#secondshape` is 194), `#stunimmunity` (no such command), `#magicskill 50 n` written as `#custommagic 32640 100n` |
| The inspector's value differs from the stored one | `#startage` (459: the inspector's is ~10% higher), `#fireres` +10 from a heat aura without the intrinsic flag (24), `#coldrec` (offset), `#itemslots` (10), `#templetrainer` (3) |
| Stored values no command reproduces (read-only here, written by the inspector) | `#heat 3`/`#cold 3` from the aura flags (153), `#horrormark` (3; stores 5), `#blessbers`, `#entangle` (stored 3, the commands store 1), `#stealthy -15` without the flag (4), `#sailing 5 0` (13) |
| Not found in the exe | `#startitem` (18), `#siegebonus` (1), `#eyeloss` (9) |

## Next

- Dom5Parser reads the `-- ro:` lines (`IDEntity.GameValues`); the monster view lists them.
  Other views, and switching the editor's vanilla base to this file, are next.
