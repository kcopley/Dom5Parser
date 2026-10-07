# Dominions 6 updates

Plan of 2026-10-07 (the user: patch notes often say only "misc stats and adjustments"; log every
change to stats, events, any data in the exe). Not built yet.

## What depends on the game version

Everything `tools/dom6exe` writes from `Dominions6.exe` (6.37 today): `vanilla.dm` (every entity's
stats, the read-only values), `vanilla-events.dm`, `vanilla-sprites.json`, the command catalog and
reading rules (`data/dmread-6.37.json`: commands per type, argument formats, text limits), the game
tables, the compiled-in icon pack. The editor reads the game's texts and event messages from the
player's exe at run time, and only from a version it knows (the places are checksum-checked):
with another version it shows none, and says so; the rest works.

## When a new version comes out

1. Before Steam updates (or with Steam's `download_depot` for an older build): keep the old exe's
   extracted data. `vanilla.dm` and the other data files in the repo are the 6.37 snapshot.
2. Run `dom6exe.py` on the new exe. Its checks (record sizes against known vanilla records, the
   anchors each parser function is found by, the end markers) say whether the layouts moved; fix
   the offsets that did.
3. Write the new data files, the text locations for the new exe, the catalog and reading rules.
4. **The change log** (below) between the two versions' data.
5. Run the fidelity suite and the editor's sweep; the workshop mods must still read the same.
6. Release the editor with the new version's data (and keep the old version's for old exes?
   one data set per version, picked by the exe's checksum, if players lag behind).

## The change log: `dom6exe.py changelog OLD NEW`

Compares two versions' extracted data, entity by entity, and writes Markdown:
- **Units**: added/removed; every changed stat (`Hoplite #14: hp 11 -> 12, gold 10 -> 11`),
  abilities gained/lost/changed, weapons/armor swapped, magic paths, read-only values; grouped
  by nation (from the nations' recruit lists), then the rest.
- **Weapons, armor, items, spells, sites, nations**: the same, with names (a spell's path, level,
  cost, effect, damage; a nation's recruits, heroes, sites, gods).
- **Events**: rarity, requirements and effects changed, messages changed (from the exe's texts,
  shown only locally: game prose isn't committed).
- **Modding**: commands added or removed per type, argument formats and text limits changed
  (the reading rules), so mod authors know what a patch changed for them.
The data files are text, one line per value, so a plain diff already shows changes; the tool
names them (which unit, what the number means) and groups them so the log reads like patch notes.
