# Derived values: what the game makes of a monster's stats

The editor shows, next to a stat box and in brackets, the value the game uses when it isn't the
one in the box: a unit's defence with its weapons, armor and mount, protection with armor,
encumbrance and combat speed with armor, map move, ages, resources and gold. The weapon table
shows each weapon's attack and damage as this unit wields it. Values with no box of their own
(leadership with what paths add, resistances, auras, casting encumbrance) are listed under the
stats as IN GAME. Every bracketed value's tooltip lists where it starts and each part that
changes it.

The formulas are the dom6inspector's (kcopley/dom6inspector, branch export-test), ported to C#
in `Dom5Edit/Derived/`; the data is the editor's own (the mod over `vanilla.dm`, which is written
from Dominions6.exe, tools/dom6exe). `Dom5Tests derived-check` compares the two on every vanilla
unit (below).

## Code

| File | What |
|---|---|
| `Dom5Edit/Derived/UnitStats.cs` | The inputs: a monster's stats, weapons (`WeaponStats`) and armor (`ArmorStats`) as the inspector's unit window reads them, built from a resolved monster (`UnitStats.Of(ResolvedEntity, find)`). |
| `Dom5Edit/Derived/UnitTotals.cs` | The values (`UnitTotals.Compute`): each a `DerivedValue` (start, parts, notes); per-weapon `WeaponTotals`. Pure; about 0.2 ms a page. |
| `Dom5Edit/Derived/GoldCost.cs` | The recruitment price (`#gcost` 10000 and up worked out by the game). |
| `Dom5Edit/Derived/ItemCost.cs` | A magic item's gem cost to forge. |
| `Dom5Editor/UI/ViewModels/EntityPages.cs` | Monster page (`MonsterPageViewModel`: brackets, IN GAME notes, weapon and armor table cells); the weapon, armor and item pages' notes. |
| `tools/derived/inspector_totals.js` | Runs the inspector headless and dumps its values with the inputs it used. |
| `Dom5Tests/DerivedCheck.cs` | `derived-check` (the comparison) and `derived` (one monster's values and parts). |

Other pages, in the same style: a weapon's damage says how much strength it gets ("(+str)",
"(+str/2)", "(+str x1.25)" two-handed), a missile weapon's attack that it's precision, a negative
range that it's strength; a shield's defence its parry, body armor's encumbrance its map move
penalty, protection by part where `#prot` doesn't say it; an item's path levels their gem cost
("(10 fire gems)", with `#itemcost1`/`#itemcost2`) and its weapon and armor their stats.

The page works the values out on every rebuild (after each edit), so they follow edits to the
monster, to its weapons and armor, and to its copy source.

## The formulas (from the inspector)

Source: `scripts/DMI/MUnit.js` `prepareForRender` (around line 958), `prepareData_PostSiteData`
(map move), `getWpnLen` / `getWpnAtt` / `getWpnDmg` (the weapon table), `goldCost` /
`monsterGoldCost` / `magicHalfLevels`; `MArmor.js` and `MWpn.js` `prepareData_PostMod`.

- **Changes** are added as the inspector's `bonus()` does: cut to a whole number towards zero,
  nothing for 0.
- **Ages.** Not set: undead start at 0.4 x max age (187), max 500; inanimate start at 180 x size,
  max 400 x size; demons 0.4 x max (370), max 1000; others 0.6 x max (22), max 50. Each level of
  death (undead), earth (inanimate), blood (demons) or nature (others) adds half the max age;
  fire takes 5, 2 or 1 a level from the living (max age 200+, 50+, less).
- **Paths.** For a leader: fire +10 leadership; every path +10 magic leadership a level (astral
  +20); death +50 undead leadership, blood +10. Death adds to fear (or 1 a level above 5); fire to
  heat aura and fire shield, water to cold aura, when it has one; nature +10 supply a level;
  earth 3+ adds its level to protection. Commanders with 3+ in fire, water, air, nature get 2 x
  level - 1 fire, cold, shock, poison resistance. `#command`, `#magiccommand`, `#undcommand` add
  for a leader. The game's own leadership bonus (ability 160, read-only) is a part too.
- **Old age** (start age at or past max age): per quarter of its max age past it, 1 to 6 times:
  -1 strength, attack, defence; -0.5 precision; +1 encumbrance; -5% hit points and combat speed.
- **Defence**: +3 mounted, + each weapon's `#def`, + each armor's (a shield: minus its
  encumbrance), + the shield's parry (its `#def` + its encumbrance).
- **Armor that counts**: one of each type, the last, in the order the types first appear.
- **Protection**: armor `a` over natural protection `p` gives `p + a - p x a / 40`, for the body
  and the head; the total is `(body x 4 + head) / 5` (rounded down first when the head is over 10
  and no armor has general protection), rounded. A misc armor's `#prot` replaces natural
  protection when higher.
- **Encumbrance**: armor's encumbrance (half, rounded, when mounted) is added unless the unit has
  none of its own (0: undead, machines), and taken from combat speed. Casting encumbrance: its own
  plus twice its armor's.
- **Weapons.** More than one melee weapon (not bonus ones): each gets an attack penalty of all
  their lengths together, less ambidextrous. Attack with a melee weapon: attack + that penalty +
  the weapon's `#att`; a missile weapon's `#att` is precision. Damage: + strength (two-handed
  melee: strength x 1.25; `#halfstr` half, `#bowstr` a third, `#nostr` none). Length: size 6+
  reach one further (not bonus weapons). A negative range is strength divided by it.
- **Map move**: not set, 14. Old values below 6: x6 + 2 (+6 flying, -2 slave). Commanders +2;
  over 40 rounded to 5. Body armor slows it: twice its encumbrance (one less for magic armor), at
  most 6, or the game's own penalty (ability 582); half when the unit has no encumbrance; none
  with `#nomovepen`.
- **Resources**: `#rcost` (not set: 1) + the mount's whole resource cost + weapons' and armor's
  `#rcost` x resource size / 3; over 60000 is 1; none without a gold cost; at least 1.
- **Gold**: `#gcost` below 5000 as it is; else from the base above 10000. A commander pays for
  leadership, paths (half levels; random paths on their weakest path for a quarter, their best
  for three quarters), priest levels and spying: the largest in full, the others half; healers
  extra. Stealthy commanders +5, `#mounted` +10; sacred or priests x1.3, slow to recruit x0.9,
  commanders x1.4. A mount and extra riders at their unit price. Commanders and anything over 30
  rounded down to 5. Pretenders get none (their `#gcost` is design points).
- **Item gem cost**: 5, 10, 15, 25, 40, 60, 80, 100, 120 gems for path levels 1-9, x (1 +
  `#itemcost1` / 100) for the main path (`#itemcost2` the second), rounded (`MItem.js`).

**Commander or unit** isn't in a monster's data: it's how it's recruited. The page takes a
monster as a commander when a nation or site recruits it as one (or it's a pretender), or, when
no one recruits it as a unit, when it has magic paths (the inspector's rule). The tooltips say
which it took and why.

## Verification

```
node tools/derived/inspector_totals.js /mnt/c/Projects/dom6inspector insp.json   (~35 s)
Dom5Tests derived-check insp.json [--show N] [--field NAME]
Dom5Tests derived vanilla 3 120                                                    (one monster)
Dom5Editor.exe --snapshot --select monster 3 --derived                             (the page)
```

`inspector_totals.js` boots the inspector's data layer (its `scripts/headless/boot.js`), runs
`prepareForRender` and the weapon table functions on every unit, and writes its values with the
inputs it used. `derived-check` compares twice: **formulas** (the inspector's inputs through
`Dom5Edit.Derived`) and **vanilla.dm** (the editor's inputs, resolved from `vanilla.dm`). A
vanilla.dm difference the formulas comparison doesn't have for the same unit and value comes
from the data; the check lists the inputs that differ.

Results (2026-10-06, game 6.37; 4,348 inspector units: 4,091 monsters, 257 of them twice, one
per recruitment role):

| Values | Formulas | vanilla.dm |
|---|---|---|
| all (26 values per unit + 3 per weapon) | 131,824 / 131,936 (99.92%) | 129,474 / 131,936 (98.13%) |
| defence, attack, strength, precision, hit points, encumbrance, combat speed | 100% | 99.75-99.89% (old age: start ages) |
| protection | 4,347 / 4,348 | 2,684 / 4,348 (body armor data) |
| casting encumbrance | 100% | 4,344 / 4,348 |
| map move | 4,258 / 4,348 (map move 0) | 4,258 / 4,348 (the same) |
| start age / max age | 100% / 100% | 3,854 / 4,348 / 100% |
| leadership, magic, undead | 100% | 4,347 / 4,348, 100%, 100% |
| resistances, supply, fear, auras | 100% | fire resistance 4,324 / 4,348, the rest 100% |
| resources, gold | 100% | 100% |
| weapon attack / damage / length | 100% / 6,275 of 6,296 / 100% | 6,279 / 6,193 / 6,296 of 6,296 |
| item gem cost (529 items) | | 529 / 529 |

### Where the formulas differ on purpose

- **Map move 0** (90 units: statues, trees, oracles) stays 0: the unit doesn't move. The
  inspector converts it like an old value (2, 4 for commanders).
- **`#halfstr` / `#bowstr` on a melee weapon** (21 rows: Carrion Vine, Serpent Leg, Snake
  Tresses) add half or a third of strength; the inspector adds all of it for melee weapons.
- **An armor part of 0** (1 unit, Lion King): the inspector takes the last armor with a head
  value even when it's 0 (the Lion Cloak's), so the crown's head protection is lost, depending on
  the armor order. Here an armor sets a part only when it protects it.
- **`#thirdstr`** adds half the strength: the game's parser does for it what `#halfstr` does
  (tools/dom6exe/README.md). The inspector ignores it. (No vanilla weapon uses it.)
- The inspector's commander bonus to map move depends on its load order (a unit whose type a
  later linked shape sets misses it); the check uses the inspector's flag as it was when map
  move was worked out (`cmdrAtMove`), so these aren't counted as differences.

### Where the data differs (vanilla.dm vs the inspector's CSV)

- **Body armor protection** (98 armors, 1,663 units' protection): the inspector averages the
  torso with arms and legs, `(torso + (arms + legs) / 2) / 2`; vanilla.dm keeps the torso only
  (the game uses body and head protection; tools/dom6exe/README.md), so units in body armor show
  1-4 more protection here.
- **Start ages** (459 units): the inspector's are about 10% higher than the exe's `#startage`
  (55 vs 50). Through old age this moves a few units' other stats (8-11 units).
- **Fire resistance with a heat aura** (24 units): the inspector's data has 10 more (Summer Lion
  60, vanilla.dm `#fireres 50` and a read-only heat aura flag). The exe's ability getter adds to
  resistances in some cases (tools/dom6exe/README.md, "Intrinsic resistances"); the exact rule
  isn't in the editor yet, so the stored value is shown.
- **Effect weapons** (13 weapons): affliction weapons (nets, webs) have an affliction mask as
  `#dmg`: no damage is worked out; the inspector shows 0. Gas and curse weapons: the inspector
  shows the effect's name, here its `#dmg`.
- **Repeated random magic** (7 units, e.g. Lore Master's three `#magicskill 50 1`): the resolver
  keeps one line of a repeated `#magicskill 50/51/52` (it treats `#magicskill` as keyed by the
  path), while the game adds a pick for each (the manual: "unless it is a random skill"). A fix
  belongs in `GameRules` (Dom5Edit/Resolve). It only changes the gold of such commanders.
- Smaller: Beast Trainer's `#command -25` (not in the inspector's leadership); a few units'
  stealth below 40 (no effect on cost).
