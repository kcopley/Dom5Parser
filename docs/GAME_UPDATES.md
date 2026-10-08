# Dominions 6 updates

Plan of 2026-10-07 (the user: patch notes often say only "misc stats and adjustments"; log every
change to stats, events, any data in the exe). The change log is built (`dom6exe.py snapshot`,
`dom6exe.py changelog`, below); the rest of the update routine is a checklist.

## What depends on the game version

Everything `tools/dom6exe` writes from `Dominions6.exe` (6.37 today): `vanilla.dm` (every entity's
stats, the read-only values), `vanilla-events.dm`, `vanilla-sprites.json`, the command catalog and
reading rules (`data/dmread-6.37.json`: commands per type, argument formats, text limits), what a
spell's `#damage` is per `#effect` (`data/spell-effects-6.37.json`, `dom6exe.py spelleffects`:
which numbers a merge renumbers; embedded in Dom5Edit), the game tables, the compiled-in icon pack. The editor reads the game's texts and event messages from the
player's exe at run time, and only from a version it knows (the places are checksum-checked):
with another version it shows none, and says so; the rest works.

## When a new version comes out

1. Before Steam updates (or with Steam's `download_depot` for an older build): keep the old
   version's data, outside the repository:
   `python3 tools/dom6exe/dom6exe.py snapshot --out ~/dom6-snapshots/6.37` (35 s, 3.6 MB). The
   repo's data files are the 6.37 data too (the change log reads them: see below), but a
   snapshot also keeps a checksum of every description and event message, so text changes show.
2. Run `dom6exe.py` on the new exe. Its checks (record sizes against known vanilla records, the
   anchors each parser function is found by, the end markers) say whether the layouts moved; fix
   the offsets that did.
3. Write the new data files, the text locations for the new exe, the catalog and reading rules.
4. **The change log** (below): `python3 tools/dom6exe/dom6exe.py changelog ~/dom6-snapshots/6.37
   /path/to/new/Dominions6.exe --out changes-6.38.md`.
5. Run the fidelity suite and the editor's sweep; the workshop mods must still read the same.
6. Release the editor with the new version's data (and keep the old version's for old exes?
   one data set per version, picked by the exe's checksum, if players lag behind).

## The change log: `dom6exe.py changelog OLD NEW` (built 2026-10-07, `tools/dom6exe/changelog.py`)

    python3 tools/dom6exe/dom6exe.py snapshot --out DIR [--texts]
    python3 tools/dom6exe/dom6exe.py changelog OLD NEW [--out FILE.md] [--texts]

OLD and NEW are each a snapshot folder, a `Dominions6.exe` (read on the spot into a temporary
snapshot, ~35 s) or a vanilla `.dm` file (the entities only). A snapshot holds `vanilla.dm`,
`events.dm` (without messages), the command catalog, the reading rules, the spell effects table
and a checksum of each game text (`text-hashes.json`): the same data the repo's files hold.
To compare with the repo's 6.37 data instead of a snapshot, copy `vanilla.dm`,
`tools/dom6exe/data/events-6.37.dm` (as `events.dm`), `Dom5Edit/GameData/game-commands-6.37.json`
(as `catalog.json`), `data/dmread-6.37.json` (`dmread.json`) and `data/spell-effects-6.37.json`
(`spell-effects.json`) into a folder (`changelog_test.py` does this).

The Markdown it writes, entity by entity:
- **Units**: new and removed; for each changed one every changed line with the modding command
  that sets it (`Hoplite #14: #hp 11 -> 12; #gcost 10013 -> 10014`), abilities gained or lost
  (`gained #flying`), weapons and armor in and out with their names (`#weapon +28 (Spear),
  -30 (...)`), magic paths by path (`#magicskill 3: 2 -> 3`), read-only values
  (`ro: ability 358 1 -> 2`); grouped by the nation whose recruit lists, heroes, start or defence
  units or gods have them, then units of several nations (named when six or fewer), then the rest.
- **Weapons, armor, magic items, spells, sites, nations, mercenaries, blesses, poptypes,
  nametypes**: the same (a spell's summoned unit named where its `#damage` is a unit).
- **Events**: rarity, requirements, effects and read-only codes.
- **Texts**: descriptions and event messages that changed, came or went, by checksum: which
  ones, never the words. With `--texts` (a snapshot made with `--texts` keeps the texts in
  `texts.json`) the log shows the old and new text: the game's own words, for local reading;
  neither that folder nor that log goes into the repository.
- **Modding**: commands added or no longer read per entity type, what a command stores, argument
  formats, text commands' limits, which types refuse a `#new` in an open block or a missing
  `#end`, and what a spell's `#damage` is per `#effect`.
Same data on both sides: "No changes." Checks: `python3 tools/dom6exe/changelog_test.py` (the
repo's data against itself, then against a copy with eight deliberate changes: each listed,
nothing else); the Steam exe against a snapshot of itself gives "No changes", and a fresh
snapshot of 6.37 matches the repo's data files exactly.
