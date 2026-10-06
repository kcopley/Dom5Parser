# Round-Trip Testing

How we verify the mod editor preserves mod **data** (not text) when it opens and
re-exports a `.dm` file. An independent external parser serves as the oracle.

**Last Updated:** 2026-10-04 (oracle moved to the dom6inspector fork)

> The round trip is now **stage 3** of the automated fidelity suite. For the other stages
> (inspector self-check, vanilla data, scripted edits), how to run them, and the CI setup, see
> **`FIDELITY_SUITE.md`**: `node tools/fidelity/run.mjs --quick`. This page covers the oracle
> mechanics.

---

## The idea

The Dominions game itself is impractical to use as a verifier: you'd have to
inspect thousands of values in game. Instead we use the **dom6inspector** parser
as an independent oracle:

> If we open a mod in our editor and re-export it, the inspector must parse the
> re-exported file into the **same data model** as it parsed the original.
> The file *text* may change (formatting, ordering, names → IDs); the parsed
> *data* must not.

This is a **differential test**: `parse(original)` vs `parse(roundtrip)`, both
through the **same** parser. Because both sides use the same imperfect parser,
the oracle doesn't need to be correct, only deterministic. Any parser limitation
shows up identically on both sides and cancels out.

**Pass = identical parsed data model and identical diagnostics. Fail = divergence.**

---

## The oracle: kcopley/dom6inspector

- **Repo:** https://github.com/kcopley/dom6inspector, a fork of
  `larzm42/dom6inspector`. Local checkout: `C:\Projects\dom6inspector`
  (`/mnt/c/Projects/dom6inspector`). Remotes: `origin` = the fork,
  `upstream` = larzm42.
- **Branch `export-test`**: upstream `main` (game **6.37**) plus our patches,
  namely the vanilla exporter (`ModExport.js`), its tests (`ModExportTests.js`),
  parser fixes, and the headless tools in `scripts/headless/`.
- **Real vanilla base:** the harness loads the inspector's Dom6 CSV gamedata,
  just like the web page does, so `#select*` of vanilla IDs resolves. The parser
  recognizes about 98% of the manual's commands.
- **Previous oracle (retired 2026-10-04):** `D:\Projects\dom5inspectorkc`, a
  Dom5-era fork. It loaded **no** CSV data, so it had no vanilla base: every
  vanilla `#select` failed (243k diagnostics on vanilla+DomEnhanced) and all
  vanilla edits were invisible. Its untracked `roundtrip_check.js` /
  `roundtrip_poc.js` are superseded.

## The loop

```bash
# 1. editor side: import + re-export (from the Dom5Parser repo root)
Dom5Tests/bin/Debug/net8.0/Dom5Tests.exe roundtrip <in.dm> <out.dm> [nonorm]

# 2. oracle side (from /mnt/c/Projects/dom6inspector, Node 18+)
node scripts/headless/roundtrip_check.js <in.dm> <out.dm> [--json report.json] [--show N]
```
Exit code `0` = identical, `1` = divergence, `2` = setup error. The two sides
parse in parallel child processes, about 4s total on DomEnhanced.

**How it works:**
1. `scripts/headless/boot.js` runs the inspector scripts under Node with small
   shims (no browser), feeds `gamedata/*.csv` to `DMI.parseData`, then parses
   the mod.
2. The snapshot is taken **after the mod is parsed, before
   `prepareData_PostMod`** links objects together. Values are still raw parsed
   values, and post-processing can't crash the run.
3. Entities are keyed by id. The volatile `modded` field is dropped. Reference
   fields are canonicalized (weapons, armor, unit refs, `onebattlespell`,
   `nextspell`, `startitem`, `futuresite`, `addgod` …) so that name vs ID isn't
   a diff. Unset, null and empty lists compare equal: the parser pre-creates
   empty lists on `#select*`, so whether a list exists depends on block
   structure, not data.
4. The script deep-diffs with sorted keys, compares diagnostics by message
   (line numbers stripped), and prints a **by-field frequency triage**. With
   `--json` it also writes the full report, which is meant for a CI ratchet
   baseline.

## The vanilla data: vanilla.dm

`vanilla.dm` (the editor's vanilla base) is **written from Dominions6.exe** by
`tools/dom6exe` since 2026-10-05 (before that the fork generated it from its CSVs):

```bash
python3 tools/dom6exe/dom6exe.py vanilla --out vanilla.dm
```

The output is deterministic and its header records the game version and the
exe's hash. To pick up a new game patch: rerun it (and `catalog`, see
`tools/dom6exe/README.md`) against the patched exe and commit the results; the
tool stops with an error if the game's parser changed in a way it doesn't expect.

---

## Current baseline (2026-10-04, game 6.37)

| Input | Result |
|---|---|
| Copy fixtures (`Dom5Tests/fixtures/copy/`) | 4 PASS; `name_before_copy` FAIL (known repro) |
| DomEnhanced 2.13, normalized | **1,478** differing entities, 19 diagnostic diffs |
| DomEnhanced 2.13, `nonorm` | 1,499 |

The jump from 351 under the old oracle is expected: the oracle can now see the
~8,000 `#select` blocks that edit vanilla entities. Top clusters:
- `event.description` 450 and `unit.descr` 239: blank lines inside multi-line
  strings are dropped.
- `unit.battleshape` 399.
- `unit.misc`, `gcost`, `diseaseres` around 175–200 each.
- `unit.armor` 108, `unit.pathcost` 82, `unit.mastersmith` 62, `spell.details`
  59 (multi-line `#details` truncated).

Many of these involve `#select` blocks folded into `#newmonster` blocks and
re-ordered on export. That's the class the planned **original-file-order
export** addresses. See `PROJECT_EVALUATION.md` §4–§5b for root causes found so far.

---

## Open follow-ups

- Commit a `--json` baseline and wire a CI ratchet (fail when any field count rises).
- Triage the new clusters: `battleshape`, the `misc`/`gcost`/`diseaseres` group, `mastersmith`, `pathcost`.
- Teach the harness about global settings, which it doesn't snapshot today, and
  add a raw command-line multiset check for anything the oracle ignores.
- Fix the 4 merc re-import mismatches in `verify-export.js`.
