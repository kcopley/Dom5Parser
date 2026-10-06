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
python3 tools/dom6exe/dom6exe.py events    --out tools/dom6exe/data/events-6.37.dm
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
| `catalog` | For Dom5Parser (embedded in Dom5Edit): per entity type, the commands the game reads, and whether that list is complete (the event parser also reads `#2d6units`-style commands by pattern); and per command, what it writes (`effects`: fields and abilities it sets, appends to, ORs into or removes, flag bits it sets or clears, and the argument range of generic-handler commands). Dom5Parser's resolver (Dom5Edit/Resolve) uses the effects to tell whether a line replaces an earlier one. |
| `vanilla` | Every vanilla type the editor lists, written as the commands that store each value: weapons, armor, monsters, spells, items, sites and nations as `#select*` blocks (`vanilla_dm.py`), then blesses, poptypes and nametypes as `#select*` blocks and the mercenaries as `#newmerc` blocks (`vanilla_other.py`). Values no command can store are `-- ro:` lines (shown read-only). Each table's end marker (a record named "end") is left out. AI templates have no vanilla data: the game reads them only from mods. |
| `events` | The 3,302 vanilla events as `#selectevent N` blocks (`events.py`): rarity, requirements and effects in stored order. The messages (the game's text) are left out unless `--messages`; the header line `-- messages: exe <checksum> offset <file offset> record <size> size <message size> count <n>` says where an editor reads them from the player's own exe. Each stored (code, value) pair is written as the command that stores that code; codes no command writes are `-- ro: requirement N = v` / `-- ro: effect N = v` lines, with the game's own name for the code when it has one. A JSON summary goes to stdout. |

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
- **Event table:** from the event parser's own branches: `#msg` (the table's address, the
  record size it multiplies by, the message size), `#rarity` (a byte store), `#selectevent`
  (the end marker it refuses, the highest number), `#newevent` (the free marker it scans for,
  the first mod slot). The two pair lists come from the generic handler: it appends to one
  list or the other depending on which "current event" global is set, and the event parser
  sets one before the `#req_` commands and the other before the effects. Every run checks
  the record size against three consecutive vanilla events that share a message, and that
  the record after the last vanilla event reads "end".

## Game rules read from the code (6.37)

- `#clearmagic` empties only the magic-skill table. Magic boosts are abilities 10-22 on the
  monster, so they survive it.
- `#clearspec` clears the whole ability list (magic boosts and `#xpshape` included) and the
  flags word.
- `#clear` is `#clearmagic` + `#clearspec`, plus a reset of the base stats to defaults and of
  another block of slots. The name is kept. In detail (helper 0x14022a660(monster, stats,
  slots, name), which `#clear` calls with (1, 1, 0) and `#clearspec` with (0, 0, 0)): always the
  flags word and the ability list; stats: the 12 base stats, gold/resource costs, recruitment
  points, the second flags word (leadership classes, ...) and the body shape (humanoid); slots:
  the weapon and armor lists; name: the name (never, from a command). Sprites aren't touched.
  `#clearweapons` just ends the weapon list at slot 0.
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
- `#color`/`#secondarycolor`: three floats each (clamped to 0-1) at +0x94 and +0xa0, found
  from the branches' float stores; written with the shortest decimals that read back the same.
- `#epithet` reads up to 38 bytes, so it runs past its 36 into the 2 bytes before the
  abbreviation (+0x4a): Ind's "Magnificent Kingdom of Exalted Virtue" has 37 characters.
- The word at +0x90 is a status: `#name` turns -998 (an unused slot) into 0, and the game skips
  -998 nations; -999 ends the table (the record named "end", left out). Nations 114 ("Machaka
  xxxx") and 122 ("Oman") are unused slots with leftover data: written with `-- ro: status`.
- The nation's file name (+0x4f, "early_arcoscephale"; found from the code that formats the
  pretender files `newlords/<name>_N.2h`) has no command: `-- ro: file name`. The abbreviation
  isn't written.
- `#flag` and `#indepflag` load images (the game's own flags are in its data files), and
  `#nametype` is refused for nations ("#nametype cannot be used for nations"). The texts
  (`#descr`, `#summary`, `#brief`) live outside the record and aren't written.
- The other nation commands store abilities and are written when a nation has them (fort,
  temple and lab costs, `#idealcold`, `#likesterr`, `#fortera`, ...); a nation without one uses
  the game's default, which isn't a stored value.
- Compared with the inspector, which exports names, recruitment, heroes, start sites, home
  realms and cheap gods: the exe adds start units, defenders, wall and guard units, temple
  picture, fort era, god lists, AI and dominion settings (~100 commands per nation) and the
  colors. The inspector's 25 extra nations are empty slots it names `nation_35` etc. Against
  its `nations.csv`: name, era and file name agree on all 110 nations, the epithet on 109 (it
  has Ind's full epithet; the writer cut it at 36 bytes until 2026-10-06).

## Blesses, poptypes, nametypes and mercenaries (6.37, `vanilla_other.py`)

Each table is located from its parser's branches (the store each command makes, the record
size it multiplies by, the limit it compares with) and checked against an end marker or a
known value on every run.

- **Blesses** (93, numbers 0-92): `#selectbless N` picks record N (below 100) of a static table
  of 192-byte records: the name (32 bytes), `#path0`/`#cost0`/`#path1`/`#cost1` (int16), two
  battle-buff words, up to 7 (monster ability, value) effects and 4 (scale, value) bytes ended
  by 0xff. The scale commands jump to one setter with the scale's number (`#chaosscale` 0,
  `#slothscale` 1, `#coldscale` 2, `#deathscale` 3, `#misfortscale` 4, `#drainscale` 5,
  `#orderscale` 6, `#prodscale` 7, `#heatscale` 8, `#growthscale` 9, `#luckscale` 10,
  `#magicscale` 11; the parser compares `prodscale` twice). No command adds an effect
  (`#clearfx` only empties them), so effects and buffs are `-- ro:` lines: 138 effects (32
  named by the monster command that stores that ability and value, e.g. `#heat 3`; the rest,
  bless-only numbers like 550 and 551, as "ability N") and 11 buff words. `#cost1` also raises
  `#path1` to at least 0, so a bless without a second path gets neither. The record after the
  last is named "end". (The inspector has no bless data.)
- **Poptypes** (82, numbers 25-106): `#selectpoptype N` (0-249) picks entry N of two tables:
  recruitment (42 int32: units, -2, commanders, -1; `#addrecunit` inserts before -2,
  `#addreccom` before -1, `#clearrec` leaves -2, -1) and defenders (8 (key, value) int32 pairs
  ended by key 0: `#defunit1` 215, `#defmult1` 216, `#defcom1` 217, `#defunit1b` 218,
  `#defmult1b` 219, `#defunit1c` 220, `#defmult1c` 221). The game reads the first pair with a
  key (0x1402e9760), so a repeated key is `-- ro:` (5); keys 230 and 235 have no poptype
  command (3). Checked: Barbarians (25) recruit 139 and 140 with commander 141. Poptypes have
  no name in the game. (No inspector data.)
- **Nametypes** (67 lists, 14,532 names, numbers 100-168): `#selectnametype N` takes 100-499;
  each list is 1,500 name pointers ended by a pointer to "end" (`#addname` appends, `#clear`
  empties). The first `#selectnametype` a game reads empties lists 169 on, so 169-499 are free
  for mods; 127 and 128 are empty. The inspector's `nametypes.csv` has only labels (200 ids,
  from the manual's table): every exe list has an id there.
- **Mercenaries** (78): a table of 300 bands of 312 bytes; the vanilla ones are the records
  before the first whose `#level` byte is 99 (named "end"). There is no `#selectmerc`: a mod
  can't change a vanilla band. `#clearmercs` (in a parser chunk the context anchor misses, so
  the catalog lacks it) marks record 0 as the end, and `#newmerc` takes the first free record
  (`#eramask 7`, `#minpay 100` by default). So the vanilla bands are written as the `#newmerc`
  blocks that would make them; the editor shows them read-only. `#unit` sets `#nrunits` 10
  when it is 0, so `#nrunits` follows it. Up to 7 (nation, percent) pairs no command writes
  set a band's minimum pay for that nation (the hire price, 0x140224a00: `#minpay` x percent /
  100; other nations pay 100%, or the nation's `#merccost`): 90 `-- ro: minimum pay for nation
  N` lines on 50 bands (holy orders cost Ermor, Sceleria and Lemuria 300%, monkey bands cost
  the monkey nations 75%, ...). Against the inspector's `Mercenary.csv` (78 bands): all
  1,092 fields agree (name, boss, commander, unit, counts, level, pay, xp, equipment, rate,
  items, era mask).

## Events (6.37)

- Event table at 0x140639c60: 13,000 records of 2,920 bytes. Message +0 (2,400 bytes, so at
  most 2,399 characters), rarity +2400 (a signed byte), 12 requirement pairs +2408, 20 effect
  pairs +2600; a pair is two int64, (code, value). Vanilla events are records 0-3301; record
  3302 reads "end" with rarity 99.
- Requirements and effects are numbered separately: `#req_code` stores requirement 59,
  `#decscale3` effect 59. (The catalog's `a59` for both doesn't say which list.)
- `#selectevent N` picks record N. It ignores N above 12998 and records with rarity 99. The
  first `#newevent`, `#selectevent` or `#clearallevents` backs up the table, marks every
  record after the vanilla ones free (rarity 98, message "Insert event message here") and
  record 12999 as the end.
- `#newevent` takes the first free record from 3500 on, so mod events are numbered 3500, 3501,
  ... in the order the game reads them. Records 3302-3499 are only reachable with
  `#selectevent`.
- `#clear` empties the event: no pairs, the placeholder message, rarity 98 (free). The event
  never happens until a new `#rarity`, and a mod event left with rarity 98 (a `#newevent`
  block without `#rarity` too) is taken again by the next `#newevent`.
- Every other event command goes through the generic handler. A list holds 12 requirements
  and 20 effects; lines beyond that are dropped. A command appends its pair, or replaces the
  pair with the same code if it is not repeatable: most requirements replace (repeatable:
  `#req_code`, `#req_notcode`, `#req_anycode`, `#req_notanycode`, `#req_fornation`, the
  monster lists `#req_mnr`, `#req_nomnr`, `#req_targmnr`, ..., `#req_targorder`,
  `#req_targnoorder`, `#req_targpath1-4`, `#req_targnopath1-4`, `#req_var*`); most effects
  append (replacing: `#delay`, `#delay25`, `#delay50`, `#delayskip`, `#notext`, `#nolog`,
  `#header`, `#arena*`, `#resolvearena*`, `#setpoptype`, `#addascension`, `#minascension`,
  `#dispglobals`, `#newnbor`, `#remnbor`). Values are clamped to the command's range.
- The game reads a list up to its first empty slot (the event describers, the spell-event
  lookup by `#id`).
- `#delay N` plans record (this one + 1) N turns later; with `#delayskip p`, p% of the time
  record + 2. "The next event" is the next record number, not the next block in the file: a
  `#selectevent` block in between doesn't count.
- The event parser also compares `#2com`, `#4com`, `#5com`, `#1unit`, `#1d3units`-`#4d3units`,
  `#1d6units`-`#16d6units`, `#1d3vis`, `#1d6vis`, `#2d4vis`, `#2d6vis`, `#3d6vis` and
  `#4d6vis` by name (effects 151-153, 155, 156-159, 160-175, 14-19). The catalog misses them
  (its name pattern needs a leading letter).
- `#req_notnation` stores what `#req_notfornation` does, `#arena1` what `#arena` does.
- Vanilla stores 522 of its 20,095 pairs (2.6%) with codes no command writes: effect 190
  (gold with a random part, value x 1.0-2.0; the game's debug text calls it "gold" like
  `#gold`'s 42; 221 pairs), 39 (a value the next `#assassin` passes to its battle; 154),
  177/179/183 (18d6, 20d6, 24d6 units: one branch handles 160-187 as (code - 159)d6, the
  commands stop at 16d6; 68), 77 (`#researchaff`, documented but not read by the parser; 13),
  requirement 22 (`req_siege`; 14), 133 and 137 (`orpathearth`, `orpathglamour`), and 21 more
  codes used 1-7 times. They are `-- ro:` lines.
- Vanilla also has values a mod can't write: the same non-repeatable code twice (91, e.g. two
  `#req_monster` or two `#req_rare`: the game checks both), values outside the command's range
  (`#req_mydominion 2`/`3`, `#req_commander` with a monster number, `#req_noera 0`,
  `#req_story 3`), `#visitors` stored as 0 (the command stores 1), and one effect after an
  empty slot (3086, never read). The .dm marks each with a comment.
- Vanilla rarities the manual doesn't list: 14 (the two arena events), 15 (the two seasonal
  weather events), -9 (two empty placeholders, 3186 and 3204, message "...").
- Compared with the inspector's `events.csv` (3,300 events; it skips the two placeholders):
  rarity and the number of pairs agree on every event, and every code that occurs once in an
  event has the same value (77 after the inspector's 32-bit truncation). Where a code repeats,
  the inspector writes its first value each time (750 pairs). Messages agree except its
  Latin-1 reading of UTF-8 (25) and newlines (3). Some of its names are from an older
  numbering (effect 183 "16d6units", effect 91 "holyboost": 91 is `#bloodboost`).

## Vanilla monsters compared with the inspector's vanilla.dm (6.37)

`vanilla` writes 4,136 monsters (the inspector's vanilla.dm has 4,091). Every flag bit a
command can set is written; 1,603 stored values have no command (read-only: 309 body shapes,
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

- Dom5Parser reads the `-- ro:` lines (`IDEntity.GameValues`); every page lists them.
- Bless effects and buff bits by name (they are battle buffs and monster abilities the bless
  gives sacred units); poptype defender keys 230 and 235; the nation abbreviation.
