# Fidelity Suite

Automated, end-to-end checks that Dom5Parser preserves mod data, judged by what the
**dom6inspector holds in memory**, not by file text. File differences are expected
(ordering, formatting, names vs IDs). The inspector's parsed data must be identical, or
differ only in ways we've explicitly listed as expected.

**Runner:** `tools/fidelity/run.mjs` · **Config:** `tools/fidelity/suite.json` ·
**Baselines:** `tools/fidelity/baselines/` · **CI:** `.github/workflows/fidelity.yml`

**Last Updated:** 2026-10-04

---

## The four stages

| Stage | Question | How | Compared at |
|---|---|---|---|
| **1. Inspector self-check** | Can the oracle write back out everything it read from a mod? | Inspector loads vanilla + mod, exports every entity the mod touched (`export-mod.js`), reloads the export | **final**: after the inspector's full post-processing (functional equality) |
| **2. Vanilla data** | Is `vanilla.dm` faithful to the game data? | (a) exporter round-trips the CSV data (`verify-export.js`); (b) committed `vanilla.dm` = what the pinned oracle generates; (c) audit of `vanilla.dm` values that differ from the raw game data | parse |
| **3. Save fidelity** | Does saving in Dom5Parser change any data? | `Dom5Tests roundtrip` (load → save) → oracle compares original vs saved | **parse**: strict, right after parsing (catches even a dropped command that only restated a default) |
| **4. Edits** | Does an edit change exactly what it should, and nothing else? | `Dom5Tests edit` applies scripted edits → oracle compares an unedited save vs the edited save → must match the case's `expect` list exactly | parse |

Stage 4 compares against an *unedited Dom5Parser save*, not against the original mod.
That isolates the edit from any existing save losses, which stage 3 tracks separately.
Once stage 3 is clean, the two are equivalent.

The oracle is the kcopley/dom6inspector fork (`scripts/headless/`, see its README), pinned
for CI by `tools/fidelity/oracle-ref.txt`.

## Running it

```bash
# prerequisites: build Dom5Tests; a checkout of kcopley/dom6inspector (branch export-test)
"/mnt/c/Program Files/dotnet/dotnet.exe" build "D:\\Projects\\Dom5Parser\\Dom5Tests\\Dom5Tests.csproj"

node tools/fidelity/run.mjs --quick          # stages 3-4, ~1-2 min: the everyday loop
node tools/fidelity/run.mjs                  # all stages, ~15 min (stage 1 is slow, see below)
node tools/fidelity/run.mjs --only e04       # a single case (substring match)
node tools/fidelity/run.mjs --update-baselines   # accept current counts as the new baseline
```
Options: `--oracle DIR` (or env `DOM6INSPECTOR`; default looks in `../dom6inspector` and
`C:\Projects\dom6inspector`), `--stages 1,3`, `--json summary.json`. Work files land
in `Dom5Tests/bin/fidelity/`, which is gitignored. Every check writes its oracle report there.

Stages 1–2 need the inspector's full post-processing, about 50s per load: `MNation` takes
~22s and `MSite` ~19s, from nested loops over all events and units for each site. So they're
skipped by `--quick` and run in CI, after oracle or `vanilla.dm` changes, and weekly.

## Results and policies

| Status | Meaning |
|---|---|
| `PASS` | zero differences, or for baseline cases, no count rose |
| `PASS+` | baseline case improved; run `--update-baselines` to lock it in |
| `FAIL` | unexpected differences, or a baseline count rose |
| `xfail` | listed as known-failing, with a reason |
| `XPASS` | known-failing case now passes; remove its `knownFailing` |
| `NEW` | baseline case without a baseline file yet |

**Baselines (ratchet):** large real mods (DomEnhanced) can't be at zero yet, so their
per-field difference counts are committed. The suite fails if any field's count rises, or
a new field appears. Fixes lower the counts, and `--update-baselines` records the
progress. Commit baseline changes with the fix that caused them.

## Adding cases

**A mod** (stages 1 and 3): add it to `suite.json` with `"expect": "pass"` (must be identical)
or `"baseline"`. Mark a stage known-failing with
`"knownFailing": { "3": "reason" }`.

**An edit case** (stage 4): add `Dom5Tests/fixtures/edits/<name>.json`:
```json
{
  "description": "What this checks.",
  "base": "base_edits.dm",
  "edits": [ { "op": "set", "entity": "monster", "id": 7410, "command": "#hp", "value": "20" } ],
  "expect": [ { "type": "unit", "id": "7410", "field": "hp", "from": "12", "to": "20" } ],
  "knownFailing": "optional reason"
}
```
- `op`:
  - `set` replaces the value. Int commands go through `Set<IntProperty>`, like the editor's `SetIntPropertyCommand`.
  - `add` adds one more (multi-valued commands such as `#weapon`).
  - `remove` removes every property with that command, or only those whose argument equals `value`.
- `entity`: `monster`, `weapon`, `armor`, `item`, `spell`, `site`, `nation`. Vanilla entities are
  selected into the mod first (copy-on-write, `Mod.SelectForEdit`).
- `value` is the argument as written in a `.dm` file. Surrounding quotes are optional.
- `expect` uses the **inspector's** field names and values: references as IDs, lists as
  arrays, unset as `null`. `from`/`to` can be omitted to accept any value. Write
  expectations from intent, never by copying observed output.

## Current state (2026-10-04, oracle 5b44841, game 6.37)

Full run: **25 checks: 21 pass, 4 known-failing, 0 failing.**

| Stage | Case | Result |
|---|---|---|
| 1 | 5 copy fixtures | pass |
| 1 | edits-base | xfail: inspector exports vanilla unit 3's *displayed* mapmove/rcost (finding 1) |
| 1 | DomEnhanced 2.13 | baseline **8,945**: mostly the same display-value class (`mapmove`, `rt`, `bow`, `leader`, `gemcost`) plus derived display fields |
| 2 | exporter round trip | xfail: 4 mercs (finding 5) |
| 2 | vanilla.dm current | pass |
| 2 | vanilla base values | baseline **3,568** entities (finding 1) |
| 3 | 5 copy fixtures + edits-base | pass, except `name_before_copy` xfail (finding 3) |
| 3 | DomEnhanced 2.13 | baseline **932** (was 1,478 before the multi-line/negative-`#dmg` fixes) |
| 4 | e01–e06, e08 (incl. edits on DomEnhanced) | pass: each edit changes exactly the expected fields |
| 4 | e07 live template | xfail (finding 2) |

DomEnhanced stage 3, largest remaining clusters:
- `unit.battleshape` 399
- `unit.descr` 198
- `unit.misc` 197
- `unit.gcost` 176
- `unit.diseaseres` 173
- `unit.armor` 108

Many involve `#select` blocks folded into `#newmonster` blocks and re-ordered on save,
which is the class the original-file-order export targets.

## Open findings (need decisions)

1. **`vanilla.dm` carries display values for some fields** (stage 2c). The inspector's
   post-processing turns base values into displayed ones, and the exporter writes the
   displayed value. Example: Serpent Cataphract `#mapmove` base 18 → displayed 14 (armor
   penalty), and `vanilla.dm` says 14. Read as a mod command, the game would apply the
   penalty again. Affected so far:
   - `unit.mapmove` (base + commander bonus − armor penalty)
   - `unit.rcost` (base + equipment)
   - `armor.def` (shown as −encumbrance)

   Value can't be expressed as a mod command (5 → on/off):
   - `eyeloss`, `horrormark`, `norange`

   Exporting the *base* value needs per-field decisions. Your `DATA_TRANSFORMATIONS.md`
   ("Auto-Calculated Values") is the starting point, but its map-move formula doesn't match
   the inspector's actual code (`MUnit.js`). Exporting from the pre-post-processing state
   wholesale is *not* the fix: it drops weapon damage, armor protection, nation recruit lists
   and more, which are only assembled during post-processing.
2. **Live templates (stage 4 `e07`).** Editing a mod template doesn't reach the monsters
   that copy it. Copy snapshots are taken at parse time, so the old value gets baked into
   the copies. The design doc says copies of mod templates should follow edits. Where an
   edit lands in file order is the same question as the original-file-order export design,
   so decide them together.
3. **`name_before_copy` (stage 3).** `#name` declared above a `#copy*` is dropped on save.
   The dom6inspector treats `#copystats` as overwriting an earlier name, which matches
   Dom5Parser for monsters, but `#copyweapon` doesn't. An in-game check settles it.
4. **Upstream candidates** for larzm42/dom6inspector:
   - the `#uwcom`/`#coastcom`/`#coastrec` crash fix (fixed in the fork)
   - the affliction `#dmg` export
   - the post-processing performance (precompute site→event/unit indexes)
5. **4 mercenaries** (`unit "0"`) aren't re-imported by the exporter round trip. Known-failing.
