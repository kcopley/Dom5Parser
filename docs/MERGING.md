# Merging mods

Assessment of 2026-10-07 (the user: "possibly coming back to how mods can be merged together").
Nothing here is built yet; "measured" means it was tried.

## What the game does with several mods

The game reads enabled mods one after another, in the order they were enabled, all 15 passes
of one mod before the next (`tools/dom6exe/README.md`, "Several mods"). Each mod sees what the
ones before it made. A later mod's `#newmonster 5000` where an earlier one already made monster
5000 replaces it (the validator says so for vanilla's numbers and, since 8d13631, for a needed
mod's). So two mods that use the same numbers can't both be enabled: that's the problem a
merge solves.

## How often numbers collide (measured)

New or modded numbers (`#new*`, and `#select*` in the modding ranges) in the installed mods,
and how many each pair shares:

| Pair | Shared numbers |
|---|---|
| Forgotten Realms 0.95 x DomEnhanced 2.14 | 1,087 (1,001 monsters) |
| Confluence 1.06 x DomEnhanced 2.14 | 997 (820 monsters, 160 items) |
| Forgotten Realms x Confluence | 728 (727 monsters) |
| Sombre Warhammer x Confluence | 520 (288 weapons, 192 monsters) |
| Sombre x Forgotten Realms | 270 (183 sites) |
| Sombre x DomEnhanced | 210 |
| Sombre x PS Bloodwar | 74 |

Every big mod starts at the bottom of the modding ranges, so none of them can be enabled
alongside another without a merge.

## Two kinds of merge

1. **Stacking** (a submod over its parent, or mods meant to be enabled together): the result
   must be what the game gets from enabling them in order. One file with the mods' text one
   after the other is almost that. The difference is that one file is read type by type over
   the whole text: the second mod's weapons are read before the first mod's monsters. Name
   lookups and `#copy*` sources can then see something else. The editor already reads a
   submod over its parent ("Needs" on Mod Info), so a stacked file is only useful to ship one
   file instead of several.
2. **Side by side** (independent nation mods, the project's original purpose): one mod's new
   entities move to free numbers, and every reference to them is rewritten: by number, by
   name when the name is unambiguous, and in events, spells (`#damage` summons, monster tags),
   sites, nations and mercenaries. The old `ModSet.MergeAll` did a first version of this. It
   is dead code now: nothing calls it, and it predates the resolver, the save plan
   (original-order saving) and the reference rules ("used by").

## Plan (if wanted)

1. `Dom5Tests merge A.dm B.dm ... --out M.dm`: renumber the later mods' new entities into
   free numbers (`EntitySet.NextFreeID` per type, after every input's numbers), rewrite every
   reference (`Reference`, `IMultiReference`: the same coverage "used by" now has), write M as
   the mods' blocks in order (the save plan), with references by number wherever a name could
   resolve differently.
2. The referee: `gameread.py` replays the game's reading of several files in turn and compares
   that with the merged file, entity by entity, after mapping the renumbered ones. That is the
   same rule as for saving: what the game reads must not change.
3. Try it on the pairs above (FR + DomEnhanced is the hardest: 1,087 collisions), then on a
   nation mod plus its submods.
4. Editor: "Merge with..." makes a new mod (it never changes the inputs), with a report of
   what moved (old number -> new) for players' saved games and other submods.

Open questions for the user: whether merging independent mods is still wanted now that
submods load properly, and whether the result should keep each mod's comments and order (one
block per mod, with headers) or be regenerated.
