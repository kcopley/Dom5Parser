# dom6exe: game facts straight from Dominions6.exe

The dom6inspector's game data (and through it our `vanilla.dm`) comes from a CSV extraction
tool that has no current maintainer. The exe itself holds both the vanilla tables and the
`.dm` parser, so this reads them directly. Only the exe is needed: no other game files.

```bash
python3 tools/dom6exe/dom6exe.py commands  --out tools/dom6exe/data/commands-6.37.json
python3 tools/dom6exe/dom6exe.py readonly  --out tools/dom6exe/data/readonly-monster-abilities-6.37.json
python3 tools/dom6exe/dom6exe.py monsters  --out monsters.json      # ~3 MB, not committed
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
  `#domversion`.

## Next

- Map every monster command to the record field it writes (stats, flags bits, slots), from
  the store instructions in its branch. That gives the full record layout from the parser itself.
- The same for weapons, armor, items, spells, sites and nations; then generate `vanilla.dm`
  from the exe and compare it with the inspector-generated one.
- Give Dom5Parser the command catalog: commands the game doesn't read in a context, and stored
  abilities no command sets (read-only in the editor).
