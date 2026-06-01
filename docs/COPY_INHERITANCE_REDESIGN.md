# Copy / Inheritance Redesign

How the editor will faithfully import, edit, and re-export mods whose copy and
clear semantics are **order-dependent**, without breaking game behavior.

**Status:** Design agreed 2026-05-31 (kurtcop, with Bluefire & Dracolich).
Implementation in progress. See `ROUND_TRIP_TESTING.md` for the verification harness.

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
*before* A was edited, and it is the likely cause of the round-trip harness's
~25-unit "all core stats differ" divergence cluster.

**Goal:** reconcile the two — reproduce illwinter's semantics exactly on export,
while presenting a clean declarative inheritance model for editing.

---

## The algorithm: a 3-phase normalization

### Phase 1 — Materialize (parse like illwinter)
Process the mod top-to-bottom. At each copy, **embed all of the source's current
state explicitly** into the target; at each clear, **remove the covered group's
then-current values**. Result: every entity holds its true, fully-baked final
values — order-correct by construction. This is the unambiguous ground truth.

> Exclusion: the ID-relative commands (`#growhp`, `#shrinkhp`, `#xpshape`,
> `#labxpshape`) are **not** copied by `#copystats` and are never materialized via
> copy — see "ID-relative commands" below.

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
- **Not copied by `#copystats`** (confirmed) — they stay tied to each entity's own
  ID and are excluded from materialize/copy propagation.
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
- `Dom5Tests roundtrip <in.dm> <out.dm>` imports + re-exports a mod with `vanilla.dm` as base (`Dom5Tests/Program.cs`).
- `dom5inspectorkc/roundtrip_check.js <vanilla.dm> <original> <roundtrip>` diffs the two parses, exit 0/1 (untracked, lives in the separate `dom5inspectorkc` repo).
- Red test: `Dom5Tests/fixtures/copy/order_dependent_copy.dm` → **FAIL: `unit #7001 att 10|99`**. B copied A while `att=10`; the lazy exporter writes A (now `att=99`) before B and gives B no override, so reload re-copies the edited A. The materialize fix must bake `att=10` onto 7001 and turn this green.

**Confirmed decisions:** Option A (materialize inline at parse, eager snapshot); vanilla-pristine resolution; exclude sprites (`#copystats`) + the 4 ID-relative commands from copy; GUI migration deferred to the last phase.

**Key code facts (grounding for Phase 1):**
- Root cause: `IDEntity.TryGet` → `TryGetCopyFrom` resolves the source's *final* state (order-independent).
- `IDEntity.AddProperty` already calls `ApplyCopyCommand` / `ApplyClearCommand` (wipes the entity's own pre-copy/clear props). Phase 1 ADDS an eager deep-copy of the source's *current* props after that wipe.
- `ModExporter.WriteEntities` iterates `mod.Database` by type/ID — it reorders vs file order, which by itself breaks order-dependent copies.
- The inspector's `#copystats` copies `name` too (verified: 0 name diff in the red test) — materialize must copy name.

**Phase 1 — NEXT:** eager materialize in copy handling (vanilla-pristine, exclusions); add isolation tests (clear-mid-entity, copy-vanilla-then-edit-vanilla); re-run the red test (expect green) and the `de_original→de_new` comparison (expect the ~25-unit `#copystats` cluster to shrink).

## Architecture note

The transform lives in **`Dom5Edit`** (parse→materialize, export→re-sort/re-derive;
metadata via `ModParser`/`ModExporter`). Strong recommendation: make copy/inheritance
**resolution a single shared API** in the core that the GUI *consumes*, retiring
`EntityViewModel`'s parallel layered logic — so the editor displays exactly what the
exporter emits, eliminating the two-resolver drift that this work is fixing.

Touch points: `IDEntity` (TryGet / copy chain / clear / AddProperty), `PropertyGroupMap`,
`ModParser`, `ModExporter`, `Mod` (entity storage / ordering), `EntityViewModel` (consume
the shared resolution).
