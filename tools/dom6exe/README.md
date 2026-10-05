# dom6exe: game facts straight from Dominions6.exe

The dom6inspector's game data (and through it our `vanilla.dm`) comes from a CSV extraction
tool that has no current maintainer. The exe itself holds both the vanilla tables and the
`.dm` parser, so this reads them directly. Only the exe is needed: no other game files.

```bash
python3 tools/dom6exe/dom6exe.py commands  --out tools/dom6exe/data/commands-6.37.json
python3 tools/dom6exe/dom6exe.py readonly  --out tools/dom6exe/data/readonly-monster-abilities-6.37.json
python3 tools/dom6exe/dom6exe.py monsters  --out monsters.json      # ~3 MB, not committed
python3 tools/dom6exe/dom6exe.py vanilla   --out tools/dom6exe/data/vanilla-monsters-6.37.dm
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
| `vanilla` | Every vanilla monster as `#selectmonster` commands (`vanilla_dm.py`): each stored value written as the command the parser stores it with. Values no command can store are `-- ro:` lines (shown read-only). |

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

- `vanilla` for weapons, armor, items, spells, sites and nations.
- Give Dom5Parser the command catalog: commands the game doesn't read in a context, and stored
  abilities no command sets (read-only in the editor).
