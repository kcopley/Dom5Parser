# Copy / Inheritance Redesign

How the editor will faithfully import, edit, and re-export mods whose copy and
clear semantics are **order-dependent**, without breaking game behavior.

**Status:** Design agreed 2026-05-31 (kurtcop, with Bluefire & Dracolich).
Implementation in progress. See `ROUND_TRIP_TESTING.md` for the verification harness.

> **2026-10-05: superseded for saving by original-order saving (`SAVE_FLOW.md`).** Saving the
> file's blocks in file order makes copies replay as written, so the re-derive / flatten /
> canonical re-sort below are only used by the canonical writer (`PreserveSourceOrder = false`,
> `Dom5Tests roundtrip ... canonical`). An unedited DomEnhanced save is now byte-identical, and a
> template edit reaches its copies (rule C) because it's saved in the template's block. The
> snapshot capture still runs at parse; the editor's display of copies doesn't use it yet.

---

## The problem

Dominions processes a mod **imperatively, top to bottom**. `#copystats X` (and the
other copy commands) take a **snapshot of X's state at the moment they run**, and
`#clear*` removes a property group **at the moment it runs**. There is no live
inheritance in the game — after a copy, the entity is fully independent.

The editor wants a **declarative** model — "B copies template A; edit A → B
follows" — because that is what makes editing pleasant. These two models are
genuinely different, and the current editor resolves copy chains as if they were
a static tree (B inherits A's *final* state). That is wrong whenever B copied A
*before* A was edited.

> **Correction (2026-06-01, from Phase 1 measurement):** this order-dependent-edit
> bug is real but *rare* in practice. The "~25-unit all core stats differ" cluster
> was originally blamed on it; investigation showed the cluster is actually
> **forward-reference copies broken by ID-ordered export** (see "KEY DISCOVERY" under
> Implementation status). Both are addressed below.

**Goal:** reconcile the two — reproduce illwinter's semantics exactly on export,
while presenting a clean declarative inheritance model for editing.

---

## The algorithm: a 3-phase normalization

### Phase 1 — Materialize (parse like illwinter)
Process the mod top-to-bottom. At each copy, **embed all of the source's current
state explicitly** into the target; at each clear, **remove the covered group's
then-current values**. Result: every entity holds its true, fully-baked final
values — order-correct by construction. This is the unambiguous ground truth.

> Correction (2026-10-05, read from Dominions6.exe's parser): the ID-relative commands
> (`#growhp`, `#shrinkhp`, `#xpshape`, `#labxpshape`) **are** copied by `#copystats`. They
> are abilities in the monster record, and `#copystats` copies the whole ability list.
> They are materialized like any other property. See "ID-relative commands" below.

### Phase 2 — Re-sort copies/clears to canonical positions (on export)
Hoist every copy/clear to a deterministic spot:
- Copies of a **vanilla** entity → the very top of the file (vanilla has no `#new`).
- Copies of a **mod** entity → immediately after that entity's **full definition**
  (so the copy snapshots the template's edited state — enabling live templates).
- Consolidate repeated `#select` edits of the same entity.
- Canonical order within an entity's block: **copy → clear(s) → explicit overrides.**

Re-sort is **file-reordering only**; numeric IDs never change, so ID-relative
references (below) are unaffected.

### Phase 3 — Re-derive clean inheritance (post-process)
Walk the now-canonical chains and **drop any explicit value identical to what the
entity inherits** at export-time order. What remains is "copies X, overrides these
few fields" — the declarative view used for editing. A value is dropped only when
the re-sorted export order guarantees the copy reproduces it; otherwise it is
**baked** (kept explicit).

---

## Inheritance rule: vanilla vs mod (the crux)

- Inherit from a **mod** entity → **normal**: reflects all edits. Editing a mod
  template cascades to its copies. (The template pattern works.)
- Inherit from a **vanilla** entity → **pristine vanilla only**, *not* mod edits to
  that vanilla entity. (Copies of vanilla are hoisted above any vanilla edits, so
  they snapshot the pristine state.)

Consequence: **editing a vanilla entity does not cascade into things that copy it.**
To get live-template behavior, create a *mod* entity that copies vanilla and
template off that. Pristine vanilla values come from `vanilla.dm` (authoritative).

Worked example (from the design discussion): a TEMPLATE that copies the *edited*
vanilla "Eater of Gods" stores the copied-at-the-time value explicitly (because it
differs from pristine vanilla); a later edit to Eater never reaches it.

---

## Clears

`#clear*` snapshots at its point: it erases whatever the covered group accumulated
*so far*, symmetric with copies. In Phase 1, hitting a clear removes the then-current
group values. A clear command is **emitted** in the canonical form only when the
entity inherits that group from a copy source and wants to drop/replace it; if the
group is fully explicit, the cleared items simply don't appear. Plugs into the
existing `PropertyGroupMap` / `IsPropertyGroupCleared` machinery.

---

## ID-relative commands (locked pairs)

Four commands reference an entity by **numeric ID adjacency**, not an explicit target:

| Command | Implicit target |
|---|---|
| `#growhp` | previous ID (id − 1) |
| `#shrinkhp` | next ID (id + 1) |
| `#xpshape` | next ID (id + 1) |
| `#labxpshape` | next ID (id + 1) |

Rules:
- **Copied by `#copystats`** (Dominions6.exe, 2026-10-05; this note used to say the
  opposite). The copy gets the same value; its target is the copy's own id ± 1.
- The re-sort is **safe** for them: targets are numeric IDs, and reordering the file
  doesn't change IDs.
- They are fragile under **ID mutation** (merge / new-ID allocation): a unit and its
  ±1 partner are a **locked pair** — must stay adjacent / move together. The editor
  models them as implicit references and must not break the ±1 relationship.

---

## The accepted compromise

Deep / edited-template chains (template → edited into "elite" → more copies) are
preserved **only to the first template level** on export; beyond that, divergent
values are **baked in** per entity.

**Functional fidelity is guaranteed in all cases** — including divergent re-copies
and clears, a load→save is data-identical, because divergent values are baked rather
than dropped. Only **template *style*** degrades for the rare multi-state case
(the entity reads as "copies first-template, overrides these many fields" instead of
"copied the template as it was at that moment"). This is representation, not data.

---

## Round-trip style preservation

To re-emit in the original authoring style, the exporter embeds **metadata in
comments** marking the "first real defining instance" of an entity, so re-import
re-interprets the hoisted `#new`+copy vs the `#select` that actually defines data.
Goal: a functionally-identical, style-faithful (where feasible) load→save.

---

## What can go wrong (risks, not logic flaws)

1. **Materialize must mirror illwinter's per-command copy/clear semantics exactly**
   (what `#copystats` copies vs `#copyspr`, what each `#clear*` removes, the
   ID-relative exclusions). A wrong rule here is invisible from inside the editor —
   the round-trip harness against the inspector oracle is how we catch it.
2. **Forward-reference-safe re-sort** — name-based refs and shapechange pairs must
   still resolve after reordering (copies of mod entities are placed after their
   declaration, so names resolve; shapechange is ID-relative, so it's fine).

---

## Verification plan

- **Round-trip harness** (`ROUND_TRIP_TESTING.md`): after each phase, re-run
  `roundtrip_check.js` on real mods; copy-related divergences (the ~25-unit cluster,
  the `xpshape` finding) should collapse toward zero.
- **Focused test `.dm` files**: a library of single-purpose mods, one per uncertain
  behavior (does copystats copy X? clear ordering, copy-then-edit, vanilla vs mod
  template, ID-relative + copy). Each doubles as an **in-game** test and a **harness**
  corpus entry, turning "discover the semantics" into a bounded, regression-proof loop.
- **Iterate-to-green:** encode a hypothesis for a command's semantics → run harness →
  mismatches pinpoint wrong hypotheses → refine; resolve the truly-ambiguous ones with
  a small in-game test.

---

## Implementation status

**Phase 0 — COMPLETE (2026-05-31).** Round-trip loop automated and a red baseline established:
- `Dom5Tests roundtrip <in.dm> <out.dm>` imports + re-exports a mod with `vanilla.dm` as base (`Dom5Tests/Program.cs`). Append `nonorm` to skip Phase 1 normalization (for A/B comparison).
- ~~`dom5inspectorkc/roundtrip_check.js`~~ (retired 2026-10-04: it had no vanilla base). The oracle is now `node scripts/headless/roundtrip_check.js <original> <roundtrip>` in the kcopley/dom6inspector fork, with a real CSV vanilla base; see `ROUND_TRIP_TESTING.md`. All numbers below (351/374/630) were measured with the old oracle. Under the new one, DomEnhanced is 1,478 normalized vs 1,499 `nonorm`.
- Red test: `Dom5Tests/fixtures/copy/order_dependent_copy.dm` → was FAIL `unit #7001 att 10|99`.

**Phase 1 — materialize + re-derive (combined), driven by `Mod.NormalizeCopies()`** (called after `Resolve()`, before `Export()`; round-trip harness only — GUI integration is Phase 4). Currently uncommitted.

Mechanics (in `IDEntity`):
- **Snapshot capture (materialize)** — when a copy command is parsed (`AddProperty` → `CaptureCopySnapshot`), deep-clone the SAME-MOD source's current properties into `_materialized`, frozen in file order. `BuildSnapshot` scopes to the copy command's groups (`GetGroupsOverwrittenByCopy`), excludes sprites + the 4 ID-relative commands, group-`None` deduped last-wins. Vanilla / file-order-forward sources are NOT captured (left to the loader). Refs in the clone are resolved in `FinalizeCopyMaterialization` (they were cloned pre-`Resolve`).
- **Re-derive (export)** — `BakeDivergentOverrides` emits ONLY values the live copy would NOT reproduce, keeping the copy command and leaving everything else implicit (so unrelated export bugs stay hidden). **Generic via the group machinery, no per-property list:** group `None` (stats/identity) = scalar → bake if it differs from the source's current resolved value (`BuildSnapshot(src)`); clearable groups (Weapons/Armor/Magic/Special) = lists → if the copied set diverges and the entity didn't itself touch the group, re-assert via `#clear<group>` + members. The catch-all `Special` default makes new commands work with no code change.
- **Flatten (forward references)** — if the source won't be emitted earlier on the ID-ordered reload (`CopyReproducesOnReload` false), bake the whole snapshot and DROP the dead copy command.
- **Same-mod-source guard** — only act when `src.ParentMod == this.ParentMod`.

**KEY DISCOVERY — the cluster was forward-reference copies, not order-dependent edits** (`#copystats S` with `S.ID > this.ID`, e.g. `8640 → 8649`): file-order parse resolves the source but the ID-ordered exporter emits the copier first → flatten fixes it. **KEY DISCOVERY — the harness has no vanilla units** (`vanilla.dm` is all `#selectmonster`; the inspector's `_select` throws on ids without CSV data), so vanilla `#copystats` fails symmetrically both sides → same-mod guard required, vanilla-pristine rule in-game-only.

**Status / results (updated 2026-06-08, after Bug A fix):** the 3 isolation fixtures (`order_dependent_copy`, `forward_ref_copy`, `clear_mid_entity`) PASS; round-trip² is idempotent; the Bug A repro (`copyspr_after_copystats`) now bakes correctly. **Big mod: 630 → 351 data diffs** (the `#copyspr` clobber fix alone removed 279). Normalization is now **net-positive vs the un-normalized baseline** (`nonorm` = 374): it eliminates the entire order-dependent stat/bool cluster — 28 fields × ~25 units (att, def, hp, prot, str, size, mr, mor, ap, enc, gcost, mapmove, maxage, prec, body, foot, hand, head, heal, immobile, inanimate, invisible, regeneration, stealthy, misc, nohof, nowish, unteleportable) — while introducing only **~27 new field-occurrences** (17 singletons + `spell.nations` ×10).

> **The "weapon/armor reference export bug" was a MISDIAGNOSIS.** There was no ref name→id export bug. The 393 `unit.weapons` / 117 `unit.armor` diffs were Bug A's blast radius: `#copyspr` emptied `_materialized`, so the re-derive saw the weapon/armor set as "diverged" (empty snapshot vs the source's real set) and emitted bogus `#clearweapons`/`#cleararmor`. Fixing Bug A collapsed `unit.weapons` 393→66 and `unit.armor` 117→4. Verified by reverting the one guard (630) and re-applying (351).

**Remaining 351 = pre-existing export issues in OTHER subsystems, not copy logic.** The normalized field set is (almost) a strict subset of `nonorm`: `unit.weapons` 66, `unit.onebattlespell` 59, `spell.details` 59, `unit.descr` 41, `unit.xpshape` 26, `spell.nextspell` 24, `wpn.dmg` 12, `unit.magicboost_*`, `unit.name` 7 — all present with `nonorm` too. The only things normalization itself still INTRODUCES are `spell.nations` ×10 (the one cluster worth chasing — likely `#copyspell` re-derive baking nations) plus 17 count-1 singletons (item.weapon/armor/autospell/…, spell.aoe/onlyatsite/…, unit.immortal/undeadleader, wpn.*).

### Review findings (2026-06-01, 4 parallel agents: adversarial / edge / code-quality / advocate)

Two genuine bugs (repro fixtures saved):
- **Bug A — FIXED (2026-06-08): `#copyspr` after `#copystats` wiped the stats snapshot.** `CaptureCopySnapshot` set `_materialized = BuildSnapshot(...)` even when the copy's scope was `{Sprites}` → empty snapshot OVERWROTE the good `#copystats` one. `#copystats X / #copyspr Y` is ubiquitous. **Fix applied:** bail when the overwritten groups are entirely `Sprites` (`groups.All(g => g == PropertyGroup.Sprites)`) — generic, no command-name list, and subsumes the old `Count == 0` guard. **Impact: 630 → 351 big-mod diffs** (this single guard; it was also the root cause of the phantom weapon/armor "ref bug"). Repro `Dom5Tests/fixtures/copy/copyspr_after_copystats.dm` now bakes `#att 10` correctly.
- **Bug B — logic error: `CopyReproducesOnReload` assumes pure ID order, but `EntitySet.Export` emits in sections** (`>= START_ID` block before `< START_ID`). A mod-range monster copying a mod-edited vanilla unit (id `< START_ID`, same `ParentMod`) is misclassified as "reproduces" and not flattened → forward-breaks on reload. Harness-masked by vanilla symmetry; real in-game. **Fix:** make the check section-aware (a `>= START_ID` source precedes a `< START_ID` copier; else compare IDs within the same section). Repro is in-game / needs a `#newmonster`-based vanilla-substitute fixture.

Lower-priority (logged, not yet done):
- `Canon` strips at the first `" -- "` → a value legitimately containing `" -- "` mis-compares. Prefer a comment-free `ToExportString`.
- Flatten loop calls `AddProperty` per prop → each re-sorts `_properties` (O(n²·log n)); use a bulk append-then-sort.
- `CaptureCopySnapshot` runs on every parse (incl. GUI) though only the harness reads `_materialized` — small wasted alloc; consider gating.
- `name`/`descr` declared BEFORE a copy: `sort_properties` hoists the copy above the name, and `ApplyCopyCommand`'s partial branch (copystats) wipes group-`None` identity while the `coversAll` branch (copyweapon) honors `IsIdentityCommand` — inconsistent. Repro: `name_before_copy.dm`. (Partly pre-existing.)
- **Deep multi-valued chains** are "best effort" in `BuildSnapshot` (the one genuinely weak spot all agents flagged) — add a bounding fixture; consider an `AddParseIssue` when a chain snapshot draws from a non-reproducing source.
- Nits: `GetProp` could be `private`; doc mentions removed `ResolveReloadValue`.

Advocate validated the core architecture as sound: capture-at-parse is the only correct moment; same-mod guard is principled (vanilla-pristine + oracle-symmetry); re-derive-not-flatten is what keeps regressions at zero; group-machinery genericity is real (catch-all default). Acceptable documented trade-offs: monster-focus of edit handling, re-sort deferred, vanilla-untestable.

**Deferred (later phases):** topological **re-sort** (emit sources before copiers, keep the copy command — semantically transparent, no `#copystats`-semantics replication) as the cleaner alternative to flatten; non-monster edit handling; GUI consuming the shared resolution.

**Separate, pre-existing export work (NOT copy logic — present with `nonorm`):** these surface in the remaining 351 and are unaffected by normalization, so they belong to other subsystems — `unit.weapons` (66), `unit.onebattlespell` (59), `spell.details` (59), `unit.descr` (41), `unit.xpshape` (26), `spell.nextspell` (24), `wpn.dmg` (12), `unit.magicboost_*`, `unit.name` (7). The ONE diff cluster normalization itself still introduces is `spell.nations` ×10 (chase next), plus 17 count-1 singletons.

## Architecture note

The transform lives in **`Dom5Edit`** (parse→materialize, export→re-sort/re-derive;
metadata via `ModParser`/`ModExporter`). Strong recommendation: make copy/inheritance
**resolution a single shared API** in the core that the GUI *consumes*, retiring
`EntityViewModel`'s parallel layered logic — so the editor displays exactly what the
exporter emits, eliminating the two-resolver drift that this work is fixing.

Touch points: `IDEntity` (TryGet / copy chain / clear / AddProperty), `PropertyGroupMap`,
`ModParser`, `ModExporter`, `Mod` (entity storage / ordering), `EntityViewModel` (consume
the shared resolution).
