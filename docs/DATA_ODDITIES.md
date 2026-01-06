# Data Oddities and Edge Cases

This document catalogs one-off special cases, unusual data patterns, and edge cases in Dominions 6 data that require special handling during export or import.

## Table of Contents

1. [Weapons That Summon Instead of Damage](#weapons-that-summon-instead-of-damage)
2. [Special Effect Numbers](#special-effect-numbers)
3. [Negative Unit IDs (Special Summons)](#negative-unit-ids-special-summons)
4. [Magic Number Placeholders](#magic-number-placeholders)
5. [Copy/Inheritance Edge Cases](#copyinheritance-edge-cases)
6. [Hidden/Filtered Entities](#hiddenfiltered-entities)
7. [Age Value Transforms](#age-value-transforms)
8. [Property Abbreviations](#property-abbreviations)
9. [Spell Effect Encoding](#spell-effect-encoding)
10. [Event Command Numeric Prefix](#event-command-numeric-prefix)
11. [Attribute-Derived Properties (Read-Only)](#attribute-derived-properties-read-only)
12. [Weapon Bitmask Flags](#weapon-bitmask-flags)
13. [Attribute-Based Properties](#attribute-based-properties)

---

## Weapons That Summon Instead of Damage

Some weapons don't deal damage - they summon units or create effects instead.

### Summon Weapons

| Weapon ID | Name | Effect | Summons/Creates |
|-----------|------|--------|-----------------|
| **815** | Shard Illusion | Effect #1 (Summon Units) | Unit 297 "Warrior Illusion" |
| **814** | Sling of Crystal Shards | Effect #109 (Capped Damage) | Secondary: references weapon 815 |

**How it works**: Weapon 815 has `effect_number = 1` (Summon Units) with `raw_argument = 297` (the unit ID to summon). Any unit wielding this weapon will summon Warrior Illusions instead of dealing damage.

**Export consideration**: These weapons export normally - the effect type and argument are what make them summon.

### Cloud Weapons (Non-Damaging)

| Weapon ID | Name | Effect | Description |
|-----------|------|--------|-------------|
| **167** | Poison Sling | Effect #146 (Cloud) | Creates poison cloud |
| **183** | Snake Bladder Stick | Effect #146 (Cloud) | Creates cloud effect |
| **569** | Drake Gas | Effect #146 (Cloud) | Creates gas cloud |

**Code location**: `MWpn.js:56-57` hardcodes `o.dmg = "Cloud"` for effect #146.

---

## Special Effect Numbers

### Effect Number Ranges

| Range | Meaning | raw_argument Interpretation |
|-------|---------|----------------------------|
| 1 | Summon Units | Unit ID to summon |
| 2 | Normal Damage | Damage value |
| 11 | Cause Affliction | Affliction type ID (bitfield) |
| 108 | Planeshift Other | Plane ID (other_planes_lookup) |
| 146 | Cloud | Always displays as "Cloud" |
| 500-599 | Set Effect Value (N) if lower | Effect parameter index |
| 600-699 | Add To Effect Value (N) | Effect parameter index |
| >10000 | Ritual Spell | Actual effect = value - 10000 |

### Effect 500-699 (Transformation Effects)

These effects modify other effects rather than producing direct results:

```javascript
// MWpn.js:58-66
if (parseInt(effects.effect_number) >= 500 && parseInt(effects.effect_number) <= 699) {
    // raw_argument refers to unit_effects_lookup index
    o.dmg = effects_info_lookup[effect_number].name + ": " +
            unit_effects_lookup[raw_argument].name
}
```

**Examples from weapons**:
- Effect 504 + raw_argument 255: "Set Effect Value 5 if lower: [effect 255]"
- Effect 600 + raw_argument 261: "Add to Effect Value 1: [effect 261]"
- Effect 601 + raw_argument 261: "Add to Effect Value 2: [effect 261]"

---

## Negative Unit IDs (Special Summons)

Negative unit IDs in spell data represent groups of units, not individual units.

| Negative ID | Group Name | Actual Unit IDs |
|-------------|------------|-----------------|
| -16 | Yazads | 2620, 2621, 2622, 2623, 2624, 2625 |
| -17 | Yatas | 2632, 2633, 2634, 2636 |
| -21 | Dwarfs (Four Directions) | 3425, 3426, 3427, 3428 |

### Special Spell Summon Arrays

Some spells have hardcoded unit arrays:

| Spell ID | Spell Name | Unit IDs |
|----------|------------|----------|
| 380 | Angelic Host | 465, 543 |
| 1081 | Horde from Hell | 304, 303 |
| - | Tartarian Gate | 771, 772, 773, 774, 775, 776, 777 |
| - | Unleash Imprisoned Ones | 2498, 2499, 2500 |
| - | Ghost Ship Armada | 3348, 3349, 3350, 3351, 3352 |

**Code locations**:
- `SpellTables.js:84-95`
- `MSpell.js:402-411`

**Export consideration**: When exporting spells with negative unit IDs, export the negative value - the game understands these special codes.

---

## Magic Number Placeholders

### Damage Value 999

When `raw_argument = 999` in weapon effects, it's displayed as "Special" rather than a numeric damage value.

**Code**: `MWpn.js:202-204`
```javascript
if (o.dmg == 999) {
    o.dmg = 'Special';
}
```

**Export consideration**: Export as 999 - it's the actual value.

### Effect Record ID 999

In `effects_weapons.csv` and `effects_spells.csv`, raw_argument of 999 indicates "unknown" or "special handling required".

---

## Copy/Inheritance Edge Cases

When entities are copied using `#copyXXX` commands, some properties behave unexpectedly.

### Properties NOT Re-initialized After Copy

| Entity Type | Properties Not Reset |
|-------------|---------------------|
| Armor | `used_by` array (copied from source) |
| Weapon | `att` default (not reset to 0) |
| Item | `nationrebate` array (shallow copied) |

### Ignored Properties During Copy

| Entity Type | Ignored (preserved) |
|-------------|---------------------|
| All | `modded`, `id` |
| Weapon | `used_by` |
| Unit (copystats) | `sprite` |

### Shallow vs Deep Copy Issues

Only explicitly listed arrays are deep-copied:

| Entity | Deep-Copied Arrays | Shallow-Copied (Bug?) |
|--------|-------------------|----------------------|
| Item | `restricted` | `nationrebate` |
| Spell | `nations`, `notnations` | - |
| Unit | `weapons`, `armor`, `randompaths`, `startitem` | Other arrays |

**Warning**: If source entity has unlisted array properties, they become shared references.

---

## Hidden/Filtered Entities

### "Empty" Units

Units with `name === "Empty"` are filtered from display but exist in the database.

**Code**: `MUnit.js:1599-1601`
```javascript
// Bit of a hack - don't display units with the name "Empty"
// They need to exist so people can select and edit them
if (o.name === "Empty") return false;
```

**Export consideration**: These should probably be skipped during bulk export.

### Decimal ID Entities

Entities with decimal IDs (e.g., `100.01`) are virtual duplicates for shapechange forms.

**Export consideration**: Skip these - they're display-only constructs.

---

## Age Value Transforms

### startage Special Cases

| Stored Value | Meaning | Display/Export |
|--------------|---------|----------------|
| "0" | No start age | Delete property |
| "-1" | Start at age 0 | Transform to "0" |

**Code**: `MUnit.js:1160-1162`
```javascript
if (o.startage == '0') delete o.startage;
if (o.startage == '-1') o.startage = '0';
```

### Default Age Calculations by Unit Type

| Unit Type | Default startage | Default maxage |
|-----------|------------------|----------------|
| Undead | 40% of maxage (or 187) | - |
| Inanimate | 180 × size | 400 × size |
| Demon | 40% of maxage (or 370) | 1000 |
| Normal | 60% of maxage (or 22) | 50 |

**Note**: Fire magic reduces maxage for creatures with maxage < 200.

**Export consideration**: Only export explicit age values, not calculated defaults.

---

## Property Abbreviations

Some CSV fields use abbreviated names that get expanded internally.

| CSV Field | Internal Property |
|-----------|-------------------|
| `ap` | `armorpiercing` |

**Code**: `MWpn.js:181`, `MItem.js:475`
```javascript
if (key=='ap') key = 'armorpiercing';
```

**Export consideration**: Use the mod command name (`#ap`), not the expanded internal name.

---

## Spell Effect Encoding

### Ritual Spell Detection

Effect values over 10000 indicate ritual spells:

**Code**: `MSpell.js:1167-1168`
```javascript
if (parseInt(spell.effect) > 10000) {
    effect.effect_number = parseInt(spell.effect) - 10000;
    effect.ritual = 1;
}
```

| Stored Effect | Actual Effect | Is Ritual |
|---------------|---------------|-----------|
| 10002 | 2 (Damage) | Yes |
| 10011 | 11 (Affliction) | Yes |
| 2 | 2 (Damage) | No |

**Export consideration**: Export the original value (10000+) to preserve ritual flag.

---

## Event Command Numeric Prefix

Event commands starting with numbers are stored with underscore prefix:

**Code**: `parsemod.js:130-135`
```javascript
if (!fn) {
    var num = parseInt(cmd);
    if (!isNaN(num)) {
        fn = cmdlookup['_'+cmd];
    }
}
```

**Example**: Command `#1d6gold` is stored as `_1d6gold` in lookup table.

---

## JSON Deep Copy Hack

Effect objects are deep-copied using JSON serialization to avoid shared references:

**Code**: `MSpell.js:1154-1159`, `MWpn.js:460-465`
```javascript
// I don't like this JSON hack, but it's apparently the accepted JS way of doing it
effect = JSON.parse(JSON.stringify(modctx.effects_spells_lookup[spell.effect_record_id]));
```

**Implication**: Effect objects can be safely modified without affecting other entities.

---

## Commander Cost Bonuses

Special cost modifiers applied to commanders beyond base calculations:

| Attribute | Gold Bonus |
|-----------|------------|
| Stealthy | +5 |
| Autohealer | +50 |
| Autodishealer | +20 |
| Holy | ×1.3 |
| Slow to recruit | ×0.9 |

**Code**: `MUnit.js:707-729`

**Export consideration**: These are display calculations only - export base costs.

---

## LocalStorage Key Escaping

The key `#list_items` is escaped in localStorage:

**Code**: `loaddata.js:334, 343`
```javascript
if (key == '#list_items') key='\\'+key;
```

---

## Attribute-Derived Properties (Read-Only)

Some properties are derived from `attributes_by_*` lookup tables rather than stored directly in CSV. These should be treated as **read-only** in external applications - they cannot be set via standard mod commands but are derived from game data.

### Armor Material Attributes

Vanilla armor material types come from `attributes_by_armor.csv`, but the internal property names differ from mod command names:

| Attribute ID | Internal Property | Mod Command | Valid? |
|--------------|-------------------|-------------|--------|
| 267 | `ferrous` | `#ironarmor` | Yes |
| 269 | `flammable` | `#woodenarmor` | Yes |
| 557 | `magic` | `#magicarmor` | Yes |

**Code**: `MArmor.js:87-99`
```javascript
for (var oi3=0, attr; attr = modctx.attributes_by_armor[oi3]; oi3++) {
    if (attr.armor_number == o.id) {
        if (attr.attribute == "557") o.magic = 1;
        if (attr.attribute == "267") o.ferrous = 1;
        if (attr.attribute == "269") o.flammable = 1;
    }
}
```

**Export mapping**: The export correctly maps internal names to mod commands:
- `magic` → `#magicarmor`
- `ferrous` → `#ironarmor`
- `flammable` → `#woodenarmor`

**Note**: These properties come from the game executable's attribute tables. For vanilla armor, these flags are read-only data. Modders CAN add these flags to new/modified armor using the mod commands, but cannot remove them from vanilla armor.

---

## Weapon Bitmask Flags

Weapon flags are encoded in the `modifiers_mask` bitmask field of `effects_weapons.csv`, not as direct properties on weapon objects.

### Flags WITH Valid Mod Commands

These flags are extracted from the bitmask and CAN be set via mod commands:

| Bit Value | Flag Name | Mod Command |
|-----------|-----------|-------------|
| 1 | Adds Strength | not `#nostr` |
| 2 | Two-Handed | `#twohanded` |
| 64 | Armor Piercing | `#armorpiercing` |
| 128 | Armor Negating | `#armornegating` |
| 2097152 | Nonmagical | not `#magic` |
| 67108864 | Made of Iron | `#ironweapon` |

### READ-ONLY Flags (No Mod Commands)

These flags are extracted from the bitmask but have **NO corresponding mod commands**. They are exported for data completeness but cannot be used in mods - they only appear in vanilla.dm exports:

| Flag Property | Description | Notes |
|---------------|-------------|-------|
| `true` | "True" damage type | Bypasses most protections |
| `soulslaying` | Soul Slaying | |
| `ignoreshield` | Ignores Shields | |
| `aironly` | Affects Air Breathers Only | |
| `demonimmune` | No effect on Demons | |
| `defnegate` | Defense Negate | |
| `uwonly` | Use Underwater Only | |
| `noillusion` | Doesn't affect illusions/spiritforms | |
| `magiconly` | Affects Magic Beings Only | |
| `usedinmelee` | Used in melee too | For ranged weapons |
| `falsedamage` | False Damage | |
| `heavyweapon` | Higher charge bonus cap | |
| `hithead` | More likely to hit head | |
| `animalonly` | Affects Animals Only | |
| `voidsanityimmune` | Void Sanity creatures immune | |
| `userimmune` | Does not affect user | |
| `groundprojectile` | Projectiles fly along ground | |
| `fireparticles` | Adds fire particles | Visual only |
| `hasaoe` | Bypasses mirror image | Internally added |
| `mrcheckhalfdmg` | MR check for Half Damage | |

### Non-Exportable Weapon Effects (Read-Only dmg Strings)

Some weapon effects are stored in `effects_weapons.csv` via `effect_record_id` and have **NO corresponding mod commands**. These weapons cannot be fully recreated via mods, but the effect type is exported as a **quoted string for reference**:

| Effect Number | Effect Name | Example Weapons | Exported As |
|---------------|-------------|-----------------|-------------|
| 1 | Summon Units | 815 (Shard Illusion) | `#dmg "summonunits"` |
| 146 | Cloud | 167, 183, 569 | `#dmg "cloud"` |
| 108 | Planeshift Other | Various | `#dmg "planeshiftother"` |

**Why**: The `effect_record_id` points to an effects_weapons record containing `effect_number`, `raw_argument`, `modifiers_mask`, etc. There are no mod commands to set these effect numbers on weapons.

**What gets exported**: Basic weapon stats (att, def, len, etc.) and flags. The special effect is exported as a quoted string like `#dmg "cloud"` which is **READ-ONLY** - it preserves the information but will not work if imported as a mod.

**External application handling**: Treat quoted `#dmg` strings as read-only descriptive fields. Only numeric `#dmg` values are valid mod commands.

---

### Derived/Redundant Flags (NOT Exported)

These bitmask flags are **derived from other properties** and should NOT be exported:

| Bitmask Flag | Derived From | Notes |
|--------------|--------------|-------|
| `extraeffect` | `secondaryeffectalways` | Indicates weapon has always-active secondary effect |
| `extraeffectondmg` | `secondaryeffect` | Indicates weapon has on-damage secondary effect |

The actual secondary effects are exported via `#secondaryeffect` and `#secondaryeffectalways` commands (weapon references).

**How it works**: The `modifiers_mask` in the effect record is a bitmask. The code (`MWpn.js:417-455`) parses this and calls `Utils.addFlags()` to set these as properties on the weapon object.

**Export consideration**: These flags are exported with their property names (e.g., `#soulslaying`) but those are NOT valid mod commands. They should be treated as read-only data fields in external applications.

---

## Attribute-Based Properties

Some entity properties come from `attributes_by_*` lookup tables rather than direct CSV columns or bitmask flags. These are now extracted as object properties and exported.

### Weapon Attributes (from attributes_by_weapon.csv)

| Attribute ID | Name | Mod Command | Status |
|--------------|------|-------------|--------|
| 933 | AN damage affects demons/undead | `#holyifhit` | ✓ Valid command - Extracted and exported |
| 935 | +2 Att vs shield | `#flail` | ✓ Valid command - Extracted and exported |
| 942 | Can only be used when mounted | `#notdismounted` | ✓ Valid command - Extracted and exported |
| 943 | Can only be used when dismounted | `#dismounted` | ✓ Valid command - Extracted and exported |
| 266 | Material Composition: Ferrous | (overlaps with bitmask `#ironweapon`) | Display only - not extracted |
| 268 | Material Composition: Flammable | `#flammable` | **READ-ONLY** - Exported but no valid mod command |
| 482 | Not Affected by Fire Bless | `#nofirebless` | **READ-ONLY** - Exported but no valid mod command |

**Code location**: `MWpn.js:119-143` extracts these from attributes_by_weapon.csv.

### Spell Attributes (from attributes_by_spell.csv)

| Attribute ID | Name | Mod Command | Status |
|--------------|------|-------------|--------|
| 617 | Chain lightning bounces | `#maxbounces` | ✓ Extracted and exported |
| 700 | Map Range | `#provrange` | ✓ Extracted and exported |
| 701 | Destination Terrain | `#onlygeodst` | ✓ Extracted and exported |
| 702 | Source Terrain | `#onlygeosrc` | ✓ Extracted and exported |
| 703 | Cannot cast from terrain | `#nogeosrc` | ✓ Extracted and exported |
| 704 | Cannot target terrain | `#nogeodst` | ✓ Extracted and exported |
| 705 | No path through seas | `#nowatertrace` | ✓ Extracted and exported |
| 706 | No path over land | `#nolandtrace` | ✓ Extracted and exported |
| 708 | Hidden Enchantment | `#hiddenench` | ✓ Extracted and exported |
| 713 | Only cast at coast | `#onlycoastsrc` | ✓ Extracted and exported |
| 724 | Extra effect in geo | `#extraeffectgeo` | ✓ Extracted and exported |
| 725 | Can't target same province twice | `#uniquetarget` | ✓ Extracted and exported |
| 728 | Adds AN lingering heat | `#anlingeringheat` | ✓ Extracted and exported |
| 729 | Dispelled if province changes owners | `#friendlyench` | ✓ Extracted and exported |
| 731 | Can only be cast by | `#onlymnr` | ✓ Extracted and exported |
| 732 | Unit gets magic of target | `#polygetmagic` | ✓ Extracted and exported |
| 733 | Sets current province as home | `#sethome` | ✓ Extracted and exported |
| 735 | AI will not cast unless path level lower than | `#aibadlvl` | ✓ Extracted and exported |
| 738 | Only target friendly provinces | `#onlyowndst` | ✓ Extracted and exported |
| 750 | God Path Restriction | `#godpathspell` | ✓ Extracted and exported |
| 787 | Casting Time | `#casttime` | ✓ Extracted and exported |
| 790 | Remote summon commander | `#farsumcom` | ✓ Extracted and exported |
| 719 | Specific unit ability prevents casting | `#preventcast` | **READ-ONLY** - No valid mod command |
| 746 | Requires enchantment | `#requiresench` | **READ-ONLY** - No valid mod command |
| 1700 | Underwater summon | `#uwsummon` | **READ-ONLY** - No valid mod command |
| 1701 | Cold summon | `#coldsummon` | **READ-ONLY** - No valid mod command |

**Note**: Spell restriction attributes (278 for nations, 602 for realms) are also extracted and exported correctly.

**Code location**: `MSpell.js:80-175` extracts these from attributes_by_spell.csv.

### Nation Attributes (from attributes_by_nation.csv)

| Attribute ID | Name | Mod Command | Status |
|--------------|------|-------------|--------|
| 52, 100, 25 | Capital Magic Site | `#startsite` | ✓ Extracted and exported |
| 139-144 | National Unique Hero | `#hero1-6` | ✓ Extracted (skip in export - needs indexed handling) |
| 145, 146, 149 | National Generic Hero | `#multihero1-2` | ✓ Extracted (skip in export - needs indexed handling) |
| 158, 159, 739 | Coastal Fort Commander | `#coastcom` | ✓ Extracted and exported |
| 160-162, 738 | Coastal Fort Troop | `#coastunit` | ✓ Extracted and exported |
| 172, 186-188 | Underwater Fort Commander | `#uwcom` | ✓ Extracted (skip in export - needs indexed handling) |
| 189-191, 213 | Underwater Fort Troop | `#uwunit` | ✓ Extracted (skip in export - needs indexed handling) |
| 289 | Home Realm | `#homerealm` | ✓ Extracted and exported |
| 294, 412 | Terrain-Specific: Forest Recruit | `#forestrec` | ✓ Extracted and exported |
| 295, 413 | Terrain-Specific: Forest Commander | `#forestcom` | ✓ Extracted and exported |
| 296 | Terrain-Specific: Swamp Recruit | `#swamprec` | ✓ Extracted and exported |
| 297 | Terrain-Specific: Swamp Commander | `#swampcom` | ✓ Extracted and exported |
| 298, 408 | Terrain-Specific: Mountain Recruit | `#mountainrec` | ✓ Extracted and exported |
| 299, 409 | Terrain-Specific: Mountain Commander | `#mountaincom` | ✓ Extracted and exported |
| 300, 416 | Terrain-Specific: Waste Recruit | `#wasterec` | ✓ Extracted and exported |
| 301, 417 | Terrain-Specific: Waste Commander | `#wastecom` | ✓ Extracted and exported |
| 302 | Terrain-Specific: Cave Recruit | `#caverec` | ✓ Extracted and exported |
| 303 | Terrain-Specific: Cave Commander | `#cavecom` | ✓ Extracted and exported |
| 314 | Cheap God 20 | `#cheapgod20` | ✓ Extracted and exported |
| 315 | Cheap God 40 | `#cheapgod40` | ✓ Extracted and exported |
| 404, 406 | Plains Recruit | `#plainsrec` | **READ-ONLY** - Extracted but no valid mod command |
| 405, 407 | Plains Commander | `#plainscom` | **READ-ONLY** - Extracted but no valid mod command |
| 631 | Future Sites | `#futuresites` | **READ-ONLY** - Extracted but no valid mod command |

**Code location**: `MNation.js:60-96, 235-348` extracts these from attributes_by_nation.csv.

### Armor Attributes (from attributes_by_armor.csv)

| Attribute ID | Name | Mod Command | Status |
|--------------|------|-------------|--------|
| 557 | Magic | `#magicarmor` | ✓ Extracted and exported |
| 267 | Ferrous | `#ironarmor` | ✓ Extracted and exported |
| 269 | Flammable | `#woodenarmor` | ✓ Extracted and exported |

**Code location**: `MArmor.js:87-99` extracts these correctly.

### Entities Without Attribute Tables

The following entity types do NOT have corresponding `attributes_by_*` CSV files:

| Entity Type | Status |
|-------------|--------|
| **Unit** | No `attributes_by_unit` table. All unit properties come directly from BaseU.csv or mod commands. |
| **Item** | No `attributes_by_item` table. All item properties come directly from BaseI.csv or mod commands. |
| **Site** | No `attributes_by_site` table. All site properties come directly from MagicSites.csv or mod commands. |
| **Merc** | No `attributes_by_merc` table. All merc properties come directly from Mercenary.csv or mod commands. |
| **Event** | No `attributes_by_event` table. Event requirements and effects are parsed from string fields in events.csv. |

---

## Summary: Export Implications

### Skip During Export
- "Empty" named units
- Decimal ID entities (shapechange forms)
- Calculated/derived values

### Export As-Is (Special Values)
- Damage value 999 (means "Special")
- Negative unit IDs (-16, -17, -21)
- Effect values >10000 (ritual spells)
- Effect numbers 500-699 (transformation effects)

### Requires Inverse Transform
- startage "-1" → "0"
- Age defaults (don't export if matches calculated default)

### Read-Only Weapon Effects (Exported as Quoted Strings)
These weapon effects are exported as `#dmg "effectname"` for reference but CANNOT be used as valid mod commands:
- Effect #1: Summons units → `#dmg "summonunits"` (numeric argument stripped - it's the unit ID which is changeable)
- Effect #146: Creates clouds → `#dmg "cloud"`
- Effect #108: Planeshift → `#dmg "planeshiftother"`

### Read-Only Flags (No Mod Commands)
These are exported but cannot be used in mods - vanilla data only:
- Weapon: `true`, `soulslaying`, `ignoreshield`, `aironly`, `demonimmune`, `defnegate`, `uwonly`, `noillusion`, `magiconly`, `usedinmelee`, `falsedamage`, `heavyweapon`, `hithead`, `animalonly`, `voidsanityimmune`, `userimmune`, `groundprojectile`, `fireparticles`, `hasaoe`, `mrcheckhalfdmg`

### Derived Flags (NOT Exported)
- `extraeffect` and `extraeffectondmg` - derived from `secondaryeffectalways`/`secondaryeffect` references

### Property Name Mappings
Internal property names that differ from mod commands:
- Armor: `magic` → `#magicarmor`, `ferrous` → `#ironarmor`, `flammable` → `#woodenarmor`

### Attribute-Based Properties (Extracted and Exported)
These properties come from `attributes_by_*` CSV files and are now extracted as object properties:
- Weapon: `holyifhit`, `flail`, `notdismounted`, `dismounted` (valid mod commands)
- Spell: `maxbounces`, `provrange`, `onlygeodst`, `onlygeosrc`, `nogeosrc`, `nogeodst`, `nowatertrace`, `nolandtrace`, `hiddenench`, `onlycoastsrc`, `extraeffectgeo`, `uniquetarget`, `anlingeringheat`, `friendlyench`, `onlymnr`, `polygetmagic`, `sethome`, `aibadlvl`, `onlyowndst`, `godpathspell`, `casttime`, `farsumcom`
- Nation: `sites`, `coastcom`, `coastrec`, `forestrec`, `forestcom`, `swamprec`, `swampcom`, `mountainrec`, `mountaincom`, `wasterec`, `wastecom`, `caverec`, `cavecom`, `cheapgod20`, `cheapgod40`
- Armor: `magic` → `#magicarmor`, `ferrous` → `#ironarmor`, `flammable` → `#woodenarmor`

### READ-ONLY Attribute-Based Properties
These are exported but have NO valid mod commands:
- Weapon: `flammable` (attr 268), `nofirebless` (attr 482)
- Spell: `preventcast` (attr 719), `requiresench` (attr 746), `uwsummon` (attr 1700), `coldsummon` (attr 1701)
- Nation: `plainsrec` (attr 404, 406), `plainscom` (attr 405, 407), `futuresites` (attr 631)
