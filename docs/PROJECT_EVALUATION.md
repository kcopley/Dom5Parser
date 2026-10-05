# Project Evaluation

Full evaluation of where the project stands, what blocks a release, how to get
end-to-end automated testing, and how to keep the vanilla data current from the
live inspector repo.

**Date:** 2026-10-04 (first look since 2026-06-08)

> **Progress, same day.** M0 is done and most of M1.
> - **The fork exists:** https://github.com/kcopley/dom6inspector. Branch
>   `export-test` holds all the Dec–Jan exporter work, merged with upstream 6.37,
>   plus `scripts/headless/` (vanilla export, export verification, round-trip
>   oracle). It also has `.gitattributes` pinning LF line endings. That ended the
>   CRLF churn and fixed a CRLF bug that exported affliction weapons as
>   `#dmg "decay"`.
> - **`vanilla.dm` is regenerated at 6.37.**
> - **The copy redesign is committed.**
> - **New oracle baseline on DomEnhanced:** 1,478 (normalized) vs 1,499
>   (`nonorm`). The old oracle's 351 couldn't see vanilla `#select` edits.
>
> Decisions: merging stays out of v1 (keep it in mind for v2). Export will move to
> **original file order**, provided it stays intuitive and never loses data.
> Still to do from M1: CI and the ratchet baseline. See `ROUND_TRIP_TESTING.md`.
>
> **Later the same day:** the four-stage fidelity suite and CI were built (`FIDELITY_SUITE.md`);
> the parser fixes cut DomEnhanced's save differences from 1,478 to 932; and `vanilla.dm` now records the
> game's base values instead of the inspector's display values (finding 1 resolved; see the fork's
> `docs/EXPORT_RULES.md`).

**How this was done:** built the solution and re-ran the round-trip loop today;
read the code (with parallel audits of the core library, the GUI/release path, and
the inspector pipeline); reconstructed Dec 2025 – Jun 2026 work from the Claude
Code prompt log (the old session transcripts themselves have been cleaned up, so
only prompts survive).

---

## 1. Bottom line

The core library and the copy/inheritance redesign are in good shape and, more
importantly, *measurable*. What stands between you and a release is **save
fidelity**, not missing features. Three findings change the plan:

1. **The round-trip oracle runs on the wrong inspector.** The harness uses
   `D:\Projects\dom5inspectorkc`, which has a Dom5 parser and Dom5 CSV data. Your
   Dom6 work lives in **`C:\Projects\dom6inspector`**, a clone of
   `larzm42/dom6inspector`. That's where the CSV→`.dm` exporter that generated
   `vanilla.dm` is (Dec 2025), plus a browser-based mod-vs-mod comparator
   (`ModExportTests.compareModFiles`, Jan 2026) that does the same job as
   `roundtrip_check.js`. Upstream has kept that repo current: **game 6.37,
   2026-09-18**.
   - The current harness loads no CSV data, so it has **no vanilla base at all**.
     Every `#select` of a vanilla ID fails with "data not found", and the rest of
     that block is ignored. That's every vanilla edit in `vanilla.dm` and all
     8,289 `#select` blocks in DomEnhanced.
   - On vanilla + DomEnhanced it emits **243,251 parse diagnostics**.
   - Its parser recognizes about 70% of the manual's commands; dom6inspector's
     recognizes 98%.
   - A headless dom6inspector prototype with real 6.37 CSV data now works:
     7,950 units and a few hundred diagnostics (§7).
2. **The GUI's Save doesn't use the exporter the harness tests.** It goes through
   `ChangesModExporter`, which stores one property per command. Monsters lose
   weapons, armor and other multi-valued lists on save. No test covers this path.
3. **The 351 remaining diffs split three ways.** Roughly a third are real
   data-loss bugs with small fixes, a quarter are oracle noise, and the rest are
   open questions about game rules that only an in-game check can settle (§4).
   Some losses never show up in that count at all, because the oracle only
   compares entities: global settings (DomEnhanced's `#gemlongevity`,
   `#slothincome` and three more vanish on save), comments, and unknown commands
   are all dropped silently (§4d).

On top of that, important work currently exists **only on one disk** (§3).

---

## 2. Verified state (2026-10-04)

| Check | Result |
|---|---|
| Build (`Dom5Edit.sln`, Windows .NET 8 SDK) | 0 errors, ~1,680 warnings (almost all nullable-reference) |
| Copy fixtures (5) | 4 pass; `name_before_copy` fails (known repro) |
| DomEnhanced 2.13 round trip | **351** data diffs normalized vs **374** `nonorm`, matching the 2026-06-08 numbers exactly |
| Test project | `Dom5Tests` is a console app (one 356-line `Program.cs`); no test framework |
| CI | None (`.github/` doesn't exist) |
| GUI save path | `ChangesModExporter`; no test covers it |
| Merge | Library only, no GUI. `Mod.ResolveDependencies(List<Mod>)` throws on every declared dependency (the `break` only exits the inner loop, `Mod.cs:410-421`) |
| Packaging | None: no publish profile, version, icon, installer, or release |
| Git | `main` 2 commits ahead of origin; copy redesign uncommitted since 2026-06-08 |

Stale project docs: `CLAUDE.md` still lists the deleted legacy UI folders and an
`UndeadAnimalPower` project that isn't in the solution, and it doesn't mention `Dom5Tests`.

---

## 3. Do first: secure work that exists in only one place

| Location | What | Risk |
|---|---|---|
| `C:\Projects\dom6inspector` (branch `export-test`) | 2 unpushed commits, including `b4bccc610 exporter for mod data` (5,750 lines); ~1,865 lines of uncommitted real changes (Jan: mod comparison, event export, prot export, parser fixes); 9 untracked docs; spell-effect JSON and script. **Only the uncommitted working tree reproduces `vanilla.dm`.** The committed exporter yields 2.1 MB with no nations or events. | `origin` is **larzm42's** repo and you have no fork remote, so this can't be pushed anywhere today. **This is the only copy.** |
| `dom5inspectorkc` | `roundtrip_check.js` and `roundtrip_poc.js` untracked | The harness itself |
| Dom5Parser | Copy redesign (+353 lines) uncommitted; 4 fixtures untracked; 2 commits unpushed | 4 months uncommitted |

Steps:
1. Fork `larzm42/dom6inspector` on GitHub, add the fork as a remote, commit the
   WIP on `export-test`, and push.
2. Commit and push Dom5Parser.
3. Move the harness into Dom5Parser (§6).

Most working-tree "modifications" in both inspector repos are CRLF churn. With
`--ignore-cr-at-eol`, `dom5inspectorkc` has no real changes and `dom6inspector`
has the 8 script files above.

---

## 4. Data fidelity: what the 351 diffs actually are

Counts are field occurrences from the oracle's by-field table. Root causes were
confirmed by reading the source and export for sample entities.

### 4a. Real bugs (fix)

| Cluster | Count | Root cause | Fix |
|---|---|---|---|
| `spell.details` | 59 | `ModParser.Parse` only joins multi-line strings for `#descr`/`#summary`/`#msg` (`ModParser.cs:82`). A multi-line `#details` is cut at its first line, and the remaining lines are parsed as junk. | Handle an open quote generically, for any string command |
| `unit.descr` | 40 | `if (s.Length < 1) continue;` (`ModParser.cs:72`) also runs inside a multi-line string, so blank lines (paragraph breaks) are dropped | Don't skip empty lines while `isMultiLine` |
| `wpn.dmg` | 12 | Negative damage (e.g. Parrying Shield `#dmg -3`) is exported as an empty line. `WeaponDamage.IsSpecialEffectType` uses `ulong.TryParse`, so `"-3"` counts as a special-effect string and `ToExportString` returns `""` | Use `long.TryParse` |
| `spell.nations` | 10 | Re-derive appends duplicates (`["60","101"]` → `["60","101","60"]`); this is the one cluster normalization itself introduces. Re-derive treats group `None` as single-valued, but only Monster and Nation override `GetPropertyGroup`, so a spell's multi-valued `#restricted` is compared against its last entry only (`IDEntity.cs:561-568`). | Give Spell (and the other entity types) real group maps, or detect multi-valued commands generically |
| `unit.armor`, `unit.sprite`, singletons | ~20 | Duplicate armor entries, a lost `#spr1`/`#spr2`, a changed copyspr target, and assorted one-offs | Triage one by one |

### 4b. Open questions about game rules (the editor rewrites, the oracle disagrees; only the game can decide)

| Cluster | Count | What happens | Question |
|---|---|---|---|
| `unit.weapons` | 66 | Vanilla has duplicate weapon names (Spear = 1 and 96, Katana = 378 and 858, Javelin = 21 and 409, Claw, Fist, Long Spear…). The editor resolves the name to the **lowest** ID and writes the ID. The inspector's lookup was last-wins. | **Answered (2026-10-05): the lowest ID** (per the user). The inspector and Dom5Parser both resolve shared names that way now; fixture `names/duplicate_names.dm`. |
| `unit.magicboost_*` | ~60 | `#magicboost 53 -10` placed before `#clearmagic`: the parser deletes it, because `PropertyGroupMap` puts `MAGICBOOST` in the Magic group. The manual says `#clearmagic` "removes all magic skills." | **Answered (2026-10-05, from the exe's parser): no.** Boosts are abilities; `#clearmagic` empties only the magic-skill table, `#clearspec` removes boosts. Fixed in Dom5Parser. |
| `unit.name` / `descr` before `#copystats` | ~8 | `#name` / `#descr` declared above `#copystats` are wiped (e.g. 8131 "Lion Hero", War Horse ×3). This is the `name_before_copy` repro. | **Answered for the name (2026-10-05, from the exe):** `#copystats` copies the name, so the wipe is correct. |
| `unit.xpshape` | 26 | 6599 gets `#xpshapemon -1085` from a *later* `#selectmonster`, after 6603/6604 copied it. The exporter merges `#select` blocks into the definition, so the oracle now copies it. | **Answered (2026-10-05, from the exe): yes**, as it stands at the moment of the copy (it copies the whole ability list). Dom5Parser now copies them. |
| Duplicate `#newmonster` IDs | ~4 | DE defines 8647/8657/8667 (and 2 sites) twice; the editor merges them (last name wins) | Mod authoring error; game behavior unknown |

**Principle worth adopting:** the parser currently *deletes* properties declared
before a clear or copy, based on assumptions about what the game does. If an
assumption is wrong, that destroys data. Until a rule is confirmed in game,
preserve the data and have the validator warn. The duplicate-name question is
settled (lowest ID), so writing the resolved ID where the source used a name is now
safe.

### 4c. Oracle noise (fix in the harness)

`roundtrip_check.js`'s `REF_FIELD` doesn't canonicalize `onebattlespell` (59),
`nextspell` (24), `addgod` (3), `batstartsum2d6` (3), `startitem` (2), or
`domsummon` given as a lowercase name (1), about 92 in total. You flagged exactly
this in January (`"personal luck" vs "1271"`). Add those fields to `REF_FIELD`
with the right target lookup.

### 4d. Losses the oracle can't see (core audit; global settings verified)

The harness compares entities only, so these never show up in the 351:
- **Commands outside any entity are dropped with no log** (`Mod.cs:400-403`).
  That's every global setting. In DomEnhanced, `#gemlongevity`,
  `#slothincome`, `#turmoilincome`, `#deathincome` and `#luckevents` are in the
  source and gone from the export. 75 mapped commands have no handler at all,
  mostly global settings (`#clearallspells`, `#clearmercs`, `#poppergold`,
  `#resourcemult`, …).
- **Unknown commands** (`ModParser.cs:368-371`) and commands outside the current
  entity's property map (`IDEntity.cs:650-667`) are dropped. There is no verbatim
  passthrough.
- **About 30 real commands are unmapped**, among them `#bugshape*`,
  `#battlesum1d2`/`1d3`, `#faysummon`, `#sleepres`, `#spikes`, `#mrhalf`,
  `#sabbathmaster`/`#sabbathslave`, `#foreignguard*`/`#foreignwall*`,
  `#makecrater`, `#notindoors`, and the whole sound entity.
- **Text handling:**
  - `--` inside a quoted string truncates it.
  - A `#` inside a string splits the line.
  - Unquoted values with a single dash are cut.
  - `IntProperty` moves a second argument or a decimal part into the comment.
  - Template `#favrit` is re-exported wrapped in quotes.
  - Floats use the current culture, so nation `#color` breaks on a German-locale PC.
  - Files are read as UTF-8 with no detection, so Windows-1252 mods get mangled.
  - Full-line comments and comments between entities are lost.

Two fixes follow:
- **Preserve what you don't understand.** Keep unknown or out-of-context lines
  as raw passthrough properties, in place, so they survive a round trip.
- **Teach the harness the rest.** Snapshot global settings, and add a
  "raw line multiset" check (sorted non-comment command lines, before vs after)
  as a cheap second oracle that needs no inspector at all.

### 4e. Rough tally

About 140 real bugs, 100–160 rule questions, and about 92 noise. Fixing 4a and 4c
alone should bring DomEnhanced down to roughly 120–150. Most of what remains then
waits on the in-game probes (§6, layer 6).

---

## 5. The GUI save path (biggest release blocker)

Verified in code:
- Save → `ChangesModExporter` (`MainWindowViewModel.cs:618-629`).
- `EntityChanges.ChangedProperties` is a `Dictionary<Command, Property>`
  (`EntityChanges.cs:33`). The exporter writes the changed properties, then skips
  every original property with the same command (`ChangesModExporter.cs:168-187`).
  Add a fourth `#weapon` to a monster that has three, and only the new one is
  written.
- Changed properties are written *before* the originals, so an edit can land
  above an existing `#copystats` and be overwritten in game.
- `ChangesMod` keys entities by `(EntityType, ID)` (`ChangesMod.cs:22`). All
  1,698 events in DomEnhanced are numberless (ID −1), so editing one event
  touches the shared key. The GUI also lists only the first ID −1 entity per
  type, and edited numberless entities are written as `#newmonster -1` (core
  audit).

From the GUI audit (code reading, not exercised at runtime):
- The header drops `#version`, `#domversion` and `#icon`, even though ModInfoView
  edits them.
- Bless, Template and Mercenary overrides are skipped.
- The write isn't atomic and there's no backup: the original file is opened for
  writing first, so an exception part-way truncates the user's mod.
- Vanilla entities are edited in place and never reloaded, so edits leak into the
  next New or Load (even after "discard"), and "modified from vanilla" compares
  against already-changed data.
- The static reference caches are never cleared.
- Bless and Template edits throw `ArgumentException` after the change is applied,
  and there's no global exception handler.
- Several edits bypass undo and dirty tracking: the name setter (TODO at
  `EntityViewModel.cs:1429`), the copy-command setters, `Reset*`, and ModInfo.
- Every load deletes `<mod>-log.txt` next to the user's mod.

**Four resolvers, not two:**
- `IDEntity.TryGet`: lazy and final-state; it never reads the `_materialized`
  snapshot.
- `EntityViewModel`: layered access plus its own copy-chain walkers.
- `ModExporter` + `NormalizeCopies`: snapshot-based.
- `ChangesModExporter`: no copy handling, and a different entity order.

Direction:
1. **One exporter.** Save = in-memory `Mod` → `ModExporter` (+ `NormalizeCopies`),
   written to a temp file and swapped in, keeping a `.bak`. Delete
   `ChangesModExporter`.
2. **Copy-on-write vanilla.** The first edit of a vanilla entity creates a sparse
   `#select` entity in the mod instead of changing the shared vanilla object.
3. **One "effective property" API in the core** (vanilla base → copy snapshot by
   group → own properties → clears). Use it for both display and export, and
   delete the view models' chain walkers.
4. Then the oracle covers the GUI's save path automatically, and the
   scripted-edit tests (§6, layer 3) cover edits.

Sizing: 1 and 2 are days; 3 is a week or more, given the 2,039-line `EntityViewModel`.

### 5b. Export order, and the copy redesign

**Export order is the root of several problems:**
- Properties are re-sorted on every add.
- Entities are written sorted by ID, with mod-range IDs before vanilla-range IDs.
- Entity types are written in a fixed order: weapons, armor, monsters, …,
  spells, items.

That creates forward references: a monster's `#startitem` or
`#onebattlespell` can point at a mod item or spell defined later in the file,
and copies can point forward. It also causes Bug B.

The core audit recommends **exporting in original file order, block by block**.
That removes the forward-reference and Bug B classes in one change. It also
makes load→save fidelity nearly free for unedited entities, so the copy machinery
only has to handle entities the user actually edited.

The cost is a model change: an entity must remember which source block each
property came from (`#newmonster` plus later `#selectmonster` blocks). This is
worth deciding *before* building Phase 2 (canonical re-sort) of the copy redesign,
because the two approaches pull in opposite directions. A hybrid also works:
original order by default, canonical order only for entities edited in the session.

**The uncommitted copy redesign is safe to commit as a harness-only checkpoint.**
The diff only adds code; the only thing that runs in the GUI is
`CaptureCopySnapshot`, which writes a private field. Don't wire `NormalizeCopies`
into the GUI save yet.

Bugs to fix next:
- The flatten path ignores clears that come after the copy (`#copystats X` then
  `#clearweapons` re-adds X's weapons). It also treats any own instance of a list
  command as overriding the whole list (`IDEntity.cs:544-555`).
- A forward `#copyspr` is never handled, so sprites are lost on reload.
- `Property.Clone` uses `MemberwiseClone`, which shares inner
  `MonsterRef`/`MontagRef` objects between the copy and the original.
- Stale comments claim deferred vanilla/forward snapshots get completed. They
  don't; the code returns early.
- `GetMonsterGroup` defaults to Special and `GetNationGroup` defaults to
  NationSettings, so parse-time clears delete unrelated earlier properties
  (e.g. `#name`/`#era` before `#clearnation`).
- Hazard for the next phase: if dependencies are attached before parsing,
  `SelectEntity`/`NewEntity` return the shared global vanilla entity, and mod
  edits would be written into it.

---

## 6. Testing: getting to end-to-end automation

| Layer | What | Needs oracle? | Status |
|---|---|---|---|
| 0 | **Oracle on dom6inspector**: run `roundtrip_check.js`'s headless loader against dom6inspector's `scripts/`, with its Dom6 CSV gamedata as the base. Real vanilla data means `#select` blocks resolve, and the parser knows Dom6 commands. | — | Prototype works (`tools/oracle6-prototype/`). About 77s per parse, so load the CSVs once and `structuredClone` the base for each side. Snapshot before post-processing (it throws `o.uwcom.push is not a function`). |
| 1 | **xUnit project** (keep the CLI verbs): command-map coverage vs `docs/pdf_extracted/commands.json`; parse→export→parse→export idempotence (byte-identical second export); one regression fixture per bug in §4a | No | Not started |
| 2 | **Oracle differential**: each fixture and corpus mod → `Dom5Tests roundtrip` → checker with `--json` output. Commit a per-field **baseline**; fail CI when any count *rises* (a ratchet). That lets you ship progress while known diffs remain. | Yes | Manual loop exists |
| 3 | **Scripted-edit tests**: load → apply edits through the same API the view models call → save through the GUI's save path → the oracle diff must equal exactly the intended edits. You planned this in January ("test some small edits so we can see if the edits are what we expect"). | Yes | Not started |
| 4 | **Vanilla fidelity**: `parse(vanilla.dm)` vs the CSV data (dom6inspector's `runAllFullArrayComparisonTests`), run headless | Yes | Exists, browser-only |
| 5 | **Merge tests** (when merge returns): `parse(merged)` = `parse(A)` ∪ `parse(B)` under the ID-remap map | Yes | Later |
| 6 | **In-game probe kit**: one tiny mod per open rule question (§4b), plus known ones (`#growhp`/`#shrinkhp` adjacency, `#copyspr` scope). Log the results in a `docs/GAME_SEMANTICS.md` table; each answer becomes a fixture plus an oracle rule. About an hour in game settles all of §4b. | — | Not started |

**Corpus:**
- DomEnhanced 2.13 (Dom6).
- The **137 Dom5 mods** in `%APPDATA%\Dominions5\mods`, as a robustness corpus:
  no exceptions on parse, idempotent export.
- A handful of current Dom6 mods from the workshop or forums. No Dom6 install or
  workshop content was found on this machine.

**CI (GitHub Actions):**
- `windows-latest`: setup-dotnet 8 → setup-node → check out the inspector (pinned
  submodule) → build the solution → `dotnet test` → oracle on the fixtures and
  DomEnhanced → upload the diff report as an artifact.
- Whole loop today: DomEnhanced round-trip in about 3.4s, oracle check in about
  15s. That's fast enough for every push.
- The core and tests are `net8.0` and could also run on Linux; only the WPF
  project needs Windows.

---

## 7. Inspector data pipeline: updating from the live repo

**Facts:**
- `vanilla.dm`'s header reads "Dominions 6 Full Data Export, Generated
  2026-01-08", which is the output of dom6inspector's
  `ModExport.exportEverythingToFile()`.
- Your clone is based on upstream **6.31** (2025-10-31). Upstream `main` is
  **8 commits ahead**, at **6.37** (2026-09-18). That includes 4 game-data
  updates (6.33, 6.34, 6.35, 6.37) touching 77 gamedata files, and +445 lines in
  `parsemod.js` (new mod commands, gold-cost fix, event keys).
- Your local `parsemod.js` changes (+36 committed, +44/−8 uncommitted) will
  likely conflict with upstream's. `MUnit.js` changed by 461 lines upstream too.
- The export is **deterministic and reproducible headless**. Run under Node
  against the working tree with 6.31 data, it matches `vanilla.dm` byte for byte
  apart from the timestamp line.
- Regenerating with **6.37** gamedata works with 0 export errors. The diff is
  about 1,298 lines: +23 units, +3 weapons, +7 spells, +1 site, plus changes to
  72 units, 2 weapons, 2 spells and 2 items.
- Your local exporter patches are required. Upstream `main` scripts plus only
  `ModExport.js` drop many flags (`#cheapgod20` ×571, `#ironarmor`,
  `#flammable`, …).
- Upstream data updates are manual "Update to 6.xx" commits built from
  community dumps of the game executable. Upstream lagged the 6.37 patch by
  about 9 days. No fork is ahead of upstream.
- `dom5inspectorkc` is only line-ending churn plus the two untracked harness
  scripts. Its Dom5 gamedata is never used. **Retire it as the oracle.**

### 7a. Automation design

**Key simplification: treat data and code separately.** Refreshing the data
doesn't require a rebase. Take `gamedata/` from upstream HEAD and run *your
pinned* exporter on it; that's exactly how the 6.37 regeneration above was done.
Rebase your script patches onto upstream only when you want new parser coverage,
and do that by hand, since conflicts are likely.

**Layout:**
```
kcopley/dom6inspector   (new fork; branch dom5parser-oracle = upstream/main + your patchset)
  scripts/DMI/ModExport*.js, parser fixes
  scripts/headless/boot.js            shims + CSV load (expose parseData properly, no regex patch)
  scripts/headless/export-vanilla.js  header "-- Source: larzm42/dom6inspector@<sha> (Dominions 6.37) --"
                                      instead of a timestamp, so diffs show only data changes
  scripts/headless/verify-export.js   headless runAllFullArrayComparisonTests (+ unmapped-property audit)
  scripts/headless/roundtrip_check.js ported harness, CSV base, --json output
Dom5Parser
  tools/oracle/inspector              git submodule pinned to that fork branch
  vanilla.dm                          generated, committed
  .github/workflows/ci.yml            build + tests + oracle on fixtures and DomEnhanced (ratchet baseline)
  .github/workflows/update-vanilla.yml
```

Keeping the harness in the GPL fork, which it loads at runtime, keeps the MIT
Dom5Parser tree free of GPL code. That's a judgment call, not legal advice. The
alternative is a pinned upstream SHA plus a `patches/` folder in Dom5Parser.

**`update-vanilla.yml`** (weekly cron plus manual dispatch):
1. Fetch larzm42 `main`. If the `gamedata/` tree hash hasn't changed, exit.
2. Regenerate `vanilla.dm` with the pinned exporter.
3. Run verify-export (CSV vs re-parsed export), then the oracle on the fixtures.
4. Open a PR with the game version, entity-count deltas and changed IDs in the
   description (`peter-evans/create-pull-request`).
5. Optional: try rebasing the fork's patchset onto upstream, and open an issue on
   conflict. Pushing to the fork from CI needs a PAT or deploy key.

**`ci.yml`:**
- Build `Dom5Tests` (which pulls in Dom5Edit, net8.0).
- Build the full solution only on `windows-latest`, because of WPF. On Linux,
  use a core-only solution filter or `-p:EnableWindowsTargeting=true`.
- Run from the repo root (`VanillaLoader` looks for `vanilla.dm` relative to the
  working and base directories).
- Run the oracle as non-blocking until the ratchet baseline is committed.

---

## 8. Path to release

**Recommended v1: "Dom6 single-mod editor."** Open a mod, view and edit it, and
save it without losing data. Cut merging, entity create/delete, and Dom5 support
from v1.

| Milestone | Contents | Size |
|---|---|---|
| M0 Secure | §3 (fork and push dom6inspector, commit and push Dom5Parser, move the harness into the repo) | Hours |
| M1 Oracle + CI | Port the harness to dom6inspector at 6.37; regenerate `vanilla.dm`; `--json` output, ratchet baseline, GitHub Actions | 1–3 sessions |
| M2 Fidelity bugs | §4a fixes with fixtures; §4c harness fixes; raw passthrough for unknown and global commands, plus comment and quoting fixes (§4d); in-game probes for §4b and apply the answers; decide on export order (§5b); flatten fixes, `name_before_copy`, Bug B | 3–5 sessions |
| M3 One save path | §5 steps 1–2, plus atomic save and backup; scripted-edit tests | ~1 week |
| M4 Robustness | Global exception handler and log; Bless/Template fix (or hide those tabs); vanilla reload on New/Load; route every edit through undo; show parse issues after load; stop deleting log files | 2–3 sessions |
| M5 Package | Copy the data files to output (`vanilla.dm` and the spell-effect JSON aren't in any csproj `Content` today); hard error if `vanilla.dm` is missing, and no silent fallback to the hardcoded Dom5 spell table; self-contained single-file publish; version and icon; Dom6 naming (window title, exe); README rewrite; sprites as an optional user-supplied folder (don't bundle Illwinter art); release workflow on tag | 1–2 sessions |

- **v1.1:** entity creation, async load with progress, the shared resolver (§5 step 3).
- **v2:** merging. It has no callers today and is broken in several independent
  ways (core audit):
  - `ResolveDependencies` always throws.
  - Vanilla is never attached as a dependency.
  - `EntitySet.Merge` gives new IDs to sets whose `START_ID` is 0, so
    `#selectpoptype 25` becomes `#selectpoptype 0` and templates move to the
    wrong nation.
  - A `#newmonster` below 5000 is silently discarded.
  - Running out of IDs only logs.
  - Numberless events and mercs get IDs.
  - `DisableMages`:
    - Disabled entities are never copied into the merged mod.
    - It wipes the mod's own edits to those monsters.
    - Protected copy sources are aggregated too late to protect another mod's copies.
    - The mage table is the Dom5 roster.
  - Not remapped: summon weapon `#dmg` monster IDs (stored as a string), the ±1
    `#growhp`/`#shrinkhp`/`#xpshape` pairs (plain ints), sprite paths.
  - Cross-mod name collisions are never detected.
  - `#domversion` is stamped 5.00.
  - What works: resolved references follow `Entity.ID`, and montag, enchantment
    and event-code IDs are remapped.

  Rebuild it with a merge round-trip test (§6, layer 5) from the start.

**Decisions for you:**
- Is merging out of v1? Recommended: yes. Rewrite the README so it doesn't
  promise merging.
- Ship `vanilla.dm` in releases? It's derived from the same CSVs dom6inspector
  publishes openly. Sprites and descriptions should stay user-supplied.
- Rename `Dom5*` to Dom6 for the exe and window title now, or at release?
