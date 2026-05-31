# Round-Trip Testing

How we verify the mod editor preserves mod **data** (not text) when it opens and
re-exports a `.dm` file — using an independent external parser as the oracle.

**Last Updated:** 2026-05-31

---

## The idea

The Dominions game itself is impractical to use as a verifier (you'd have to
inspect thousands of values in-game). Instead we use the **dom5inspector**
parser as an independent oracle:

> If we open a mod in our editor and re-export it, the inspector must parse the
> re-exported file into the **same data model** as it parsed the original.
> The file *text* may change (formatting, ordering, names → IDs); the parsed
> *data* must not.

This is a **differential test**: `parse(original)` vs `parse(roundtrip)`, both
through the **same** parser. Because both sides use the same (imperfect) parser,
the oracle does **not** need to be correct — only deterministic. Any parser
limitation appears identically on both sides and cancels out. We are hunting
*divergence between the two parses*, which can only come from the editor
changing the data.

**Pass = identical parsed data model (and identical diagnostics). Fail = divergence.**

---

## The oracle: local dom5inspectorkc

- Location: `/mnt/d/Projects/dom5inspector**kc**/` — a **fork** of dom5inspector
  with local improvements. Use this fork, **not** upstream.
- It is **Dom5-era** and was never fully updated to Dom6, so it does not
  recognize many Dom6 commands (`#nofirebless`, newer `#dmg "poison(...)"`
  syntax, etc.). It silently skips what it can't parse.
- **Consequence: partial coverage.** The round-trip test currently validates
  only the **Dom5-understood subset** of commands. This is acceptable (it still
  covers most copy/inheritance, stats, weapons, etc.) and is why "identical
  errors on both sides" is itself a useful signal. Widening the parser's Dom6
  coverage is a future improvement.
- The parser core is `scripts/parsemod.js` (`modctx.parseMod(str, modnum, modname)`),
  with data modules under `scripts/DMI/`. It is only lightly browser-coupled.

## The base data: vanilla.dm

- The harness loads `Dom5Parser/vanilla.dm` first as the **referenced base** so
  that copies/inheritance (`#copystats`, `#copyitem`, name references) resolve.
  Vanilla data is *referenced*, not *tested* — it is identical on both sides and
  produces zero false diffs (verified: all diffs are mod entities, IDs ≥ 6500).
- `vanilla.dm` was **exported from the inspector** by us (the inspector natively
  uses opaque CSV gamedata; we built a standard-`.dm` export instead). Using it
  as the base is fine for the differential test even if it has flaws, because
  both sides share it.
- **Separate (future) check:** whether `vanilla.dm` faithfully represents the
  inspector's CSV gamedata is its own validation — `parse(vanilla.dm)` vs the
  CSV-derived data — runnable with the same headless harness later.

---

## The harness

Two Node scripts live in the inspector fork (currently **untracked scratch**;
permanent home / committing them is an open decision):

| File | Purpose |
|------|---------|
| `dom5inspectorkc/roundtrip_poc.js`  | Feasibility demo: boot the parser headless, parse one `.dm`, dump `modctx`. |
| `dom5inspectorkc/roundtrip_check.js` | The differential comparator. |

**Run:**
```bash
cd /mnt/d/Projects/dom5inspector*kc*
node roundtrip_check.js <base.dm> <original.dm> <roundtrip.dm>
# e.g.
node roundtrip_check.js \
  /mnt/d/Projects/Dom5Parser/vanilla.dm \
  /mnt/d/Projects/Dom5Parser/docs/de_original.dm \
  /mnt/d/Projects/Dom5Parser/docs/de_new.dm
```
Exit code `0` = identical, `1` = divergence, `2` = setup error. Runs in seconds.

**How it works:**
1. Loads the 16 non-UI inspector scripts under Node with ~6 lines of shims
   (`window`, `Image`, `PaneManager`, `ParsedQueryString`, and a `jQuery` with
   `trim`/`isArray`/`extend`/`each`). No browser, no DOM.
2. Parses `(base + original)` and `(base + roundtrip)` in **isolated VM contexts**.
3. Snapshots entities keyed by id, **dropping** the volatile `modded` debug field
   and **canonicalizing reference fields** (weapons→wpn, armor→armor, shape/summon
   `_ref` fields→unit lookups) so name↔ID representation is not counted as a diff.
4. Deep-diffs with sorted keys (so command reordering/reformatting is invisible),
   normalizes diagnostics (strips line numbers), and prints a **by-field frequency
   triage** of all divergences.

**End-to-end (future wiring):** have `Dom5Tests` (C# console) import `original.dm`
and export `roundtrip.dm`, then invoke `roundtrip_check.js` — one command, CI-able.

---

## Current findings (de_original.dm → de_new.dm, 2026-05-31)

Result: **FAIL — 731 data diffs, 18 diagnostic diffs** (down from 978 before
reference canonicalization). Entity counts match exactly (3483 units, 3493 spells,
1258 weapons, 295 items, 370 armor, 160 nations). The diffs cluster:

| Category | Fields | Likely nature |
|----------|--------|---------------|
| Spell nation lists | `spell.notnations` (227) | systematic export difference — investigate |
| **Unit stat cluster** | ~25 units differ on **all** core stats (`hp`/`att`/`def`/`prot`/slots…) | **probable `#copystats` divergence — copy/inheritance** |
| Name→ID disagreement | `unit.weapons` (66), `onebattlespell` (59) | editor vs inspector resolve a name to different IDs (e.g. katana 858 vs 378) — duplicate-name tie-breaking |
| Descriptions | `spell.details` (69), `unit.descr` (41) | text reformatting/escaping |
| Spell encodings | `spell.spec`/`school`/`precision`/`fatiguecost` | representation/encoding |
| Added values | `unit.xpshape` (26, `undefined`→`-1085`) | editor materializes a value the source lacked |

Diagnostics: roundtrip has ~1,000 fewer "data not found" errors — consistent with
the editor resolving names→IDs on export.

**The ~25-unit all-stats cluster is the strongest lead and is squarely
copy/inheritance territory** — likely the same class of issue the planned
copy/inheritance revision targets. See `ENHANCEMENT_PLAN.md` / `ISSUES.md`.

---

## Open follow-ups

- Extend reference canonicalization (e.g. `onebattlespell`→spell) and/or normalize
  descriptions/encodings to further separate representation noise from real bugs.
- Investigate the real divergences, starting with the `#copystats` unit cluster.
- Decide a permanent home for the harness and whether to commit it to the fork.
- Optionally widen the inspector's Dom6 command coverage to broaden the validated set.
- Separate: validate `vanilla.dm` against the inspector's CSV gamedata.
