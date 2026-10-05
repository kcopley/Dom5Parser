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

## Current state (2026-10-04, oracle 5961cf1, game 6.37)

| Stage | Case | Result |
|---|---|---|
| 1 | 5 copy fixtures, edits-base | pass |
| 1 | DomEnhanced 2.13 | baseline **6,269** (8,945 at first). What remains is mostly the inspector's model of *mods*, not vanilla data: copies don't inherit the source's attribute-based properties (provrange, hiddenench, weapon flags), plus derived display fields (`rt`, `bow`, `leader`, `gemcost`, `sorttype`, ...). |
| 2 | exporter round trip | **pass**: now strict. Each entity re-imports into a fresh object and must re-export the same command lines. The old test compared each object with itself. |
| 2 | vanilla.dm current | pass |
| 2 | vanilla base values | **pass, only expected differences** (3,568 at first). The expected ones are listed in `suite.json` `vanillaExpected`. |
| 3 | copy fixtures + edits-base | pass, except `name_before_copy` (xfail) |
| 3 | DomEnhanced 2.13 | baseline **932** |
| 4 | e01-e06, e08 | pass |
| 4 | e07 live template | xfail |

## Open findings (need decisions or work)

1. ~~`vanilla.dm` carries display values~~ **Resolved 2026-10-04.** The exporter now
   writes the game's base values. Rules, evidence and expected differences are in the
   fork's `docs/EXPORT_RULES.md`. Highlights:
   - map move: 3,171 units fixed (Serpent Cataphract 14 -> 18);
   - resource cost: 228 fixed;
   - national spell restrictions: they were missing entirely, 852 lines now;
   - `#magicarmor`: missing on all 175 armors;
   - affliction weapons: exported as plain damage (`#dmg 256` without `#dt_aff`);
   - weapon damage types: `#dt_*` from the game data's own annotations
     (placeholders 190 -> 37);
   - nations: first hero, multiheroes and underwater commanders were lost; Dom6
     `#uwrec`/`#coastrec` forms now used;
   - no defaults or display tags written as data;
   - no `%` or `NaN` in values.
2. **Live templates (stage 4 `e07`).** Editing a mod template doesn't reach the
   monsters that copy it (copy snapshots are taken at parse). Decide together with the
   original-file-order export design.
3. **`name_before_copy` (stage 3).** `#name` declared above a `#copy*` is dropped on
   save. An in-game check settles it.
4. **Never-exported game data.** About 180 parsed unit/item/site fields still aren't
   exported. Each needs mapping to a Dom6 command (plus a parser handler in the
   inspector) or a read-only ruling. In progress.
5. **Inspector: copies don't inherit attributes.** `#copyspell`/`#copyweapon` should
   carry the source's attribute-based properties, as in game. The inspector applies
   attributes by id only, which is the main remaining stage 1 class.
6. **Dom5Parser: read-only vanilla lines.** About 94 of the 127 warnings when loading
   `vanilla.dm` are the informational read-only commands (`#flammable`,
   `#nofirebless`, ...). Dom5Parser should know the list (`ModExport.readOnlyCommands`)
   and show them read-only. About 25 more come from `##placeholders##` in event
   message text being parsed as commands.
7. **Upstream candidates** for larzm42/dom6inspector (all fixed in the fork):
   - the `#uwcom`/`#coastcom`/`#coastrec`/`#uwrec` crashes and overrides;
   - `#dt_aff` double decoding;
   - armor `type` erased for mod armors;
   - `#prot NaN`;
   - post-processing performance.
8. ~~4 mercenaries not re-imported~~ resolved.
