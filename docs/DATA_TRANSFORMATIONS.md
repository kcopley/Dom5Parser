# Data Transformations Specification

This document specifies all value transformations used in Dominions 6 data, including how to implement them (from mod command to storage) and how to invert them (from storage to mod command).

## Table of Contents

1. [Numeric Transformations](#numeric-transformations)
2. [Bitmask Encodings](#bitmask-encodings)
3. [Lookup Mappings](#lookup-mappings)
4. [Auto-Calculated Values](#auto-calculated-values)
5. [Spell Encodings](#spell-encodings)
6. [Array/Reference Handling](#arrayreference-handling)

---

## Numeric Transformations

These are simple mathematical transformations where the stored value differs from the mod command value.

### incunrest (Unrest Increase)

**Purpose**: Stores unrest as a decimal value internally, but mod commands use integers.

| Direction | Formula | Example |
|-----------|---------|---------|
| **Mod → Storage** | `storage = mod_value / 10` | `#incunrest 15` → `incunrest: 1.5` |
| **Storage → Mod** | `mod_value = storage * 10` | `incunrest: 1.5` → `#incunrest 15` |

**Implementation**:
```javascript
// Import (mod to storage)
function parseIncunrest(modValue) {
    return modValue / 10;
}

// Export (storage to mod)
function exportIncunrest(storageValue) {
    return Math.round(storageValue * 10);
}
```

---

### popkill (Population Kill)

**Purpose**: Internal value is scaled up, mod command is the base value.

| Direction | Formula | Example |
|-----------|---------|---------|
| **Mod → Storage** | `storage = mod_value * 10` | `#popkill 5` → `popkill: 50` |
| **Storage → Mod** | `mod_value = storage / 10` | `popkill: 50` → `#popkill 5` |

**Implementation**:
```javascript
// Import (mod to storage)
function parsePopkill(modValue) {
    return modValue * 10;
}

// Export (storage to mod)
function exportPopkill(storageValue) {
    return Math.round(storageValue / 10);
}
```

---

### stealthy (Stealth Value)

**Purpose**: Internal stealth includes a base offset of 40.

| Direction | Formula | Example |
|-----------|---------|---------|
| **Mod → Storage** | `storage = mod_value + 40` | `#stealthy 25` → `stealthy: 65` |
| **Storage → Mod** | `mod_value = storage - 40` | `stealthy: 65` → `#stealthy 25` |

**Implementation**:
```javascript
// Import (mod to storage)
function parseStealth(modValue) {
    return modValue + 40;
}

// Export (storage to mod)
function exportStealth(storageValue) {
    return storageValue - 40;
}
```

**Notes**:
- A unit with `stealthy: 40` has no stealth bonus (exports as `#stealthy 0`)
- Values below 40 indicate negative stealth (easily spotted)

---

### defense (Mounted Defense Bonus)

**Purpose**: For mounted units, +4 defense in storage doesn't appear in mod export.

| Direction | Formula | Example |
|-----------|---------|---------|
| **Mod → Storage** | `storage = mod_value + 4` (if mounted) | `#def 12` (mounted) → `def: 16` |
| **Storage → Mod** | `mod_value = storage - 4` (if mounted) | `def: 16` (mounted) → `#def 12` |

**Implementation**:
```javascript
// Export (storage to mod)
function exportDefense(entity) {
    let def = entity.def || 0;
    if (entity.mounted) {
        def -= 4;
    }
    return def;
}
```

---

## Bitmask Encodings

These use binary flags where each bit represents a specific option.

### Item Slots

**Purpose**: Encodes which equipment slots a unit has available.

| Slot | Bit Value | Hex |
|------|-----------|-----|
| Hand 1 | 2 | 0x0002 |
| Hand 2 | 4 | 0x0004 |
| Hand 3 | 8 | 0x0008 |
| Hand 4 | 16 | 0x0010 |
| Hand 5 | 32 | 0x0020 |
| Hand 6 | 64 | 0x0040 |
| Head 1 | 128 | 0x0080 |
| Head 2 | 256 | 0x0100 |
| Head 3 | 512 | 0x0200 |
| Body | 1024 | 0x0400 |
| Foot | 2048 | 0x0800 |
| Misc 1 | 4096 | 0x1000 |
| Misc 2 | 8192 | 0x2000 |
| Misc 3 | 16384 | 0x4000 |
| Misc 4 | 32768 | 0x8000 |
| Misc 5 | 65536 | 0x10000 |
| Misc 6 | 131072 | 0x20000 |

**Standard Patterns**:
- Human mage: `15494` = 2 hands + 1 head + body + foot + 2 misc
- Commander: `213046` = 2 hands + 1 head + body + foot + 6 misc

**Implementation**:
```javascript
// Count slots by category
function countSlots(bitmask, startBit, endBit) {
    let count = 0;
    for (let bit = startBit; bit <= endBit; bit *= 2) {
        if (bitmask & bit) count++;
    }
    return count;
}

function parseItemSlots(bitmask) {
    return {
        hands: countSlots(bitmask, 2, 64),
        heads: countSlots(bitmask, 128, 512),
        body: (bitmask & 1024) ? 1 : 0,
        feet: (bitmask & 2048) ? 1 : 0,
        misc: countSlots(bitmask, 4096, 131072)
    };
}

// Export (storage to mod)
function exportItemSlots(bitmask) {
    // Use #itemslots command with full bitmask
    return `#itemslots ${bitmask}`;
}
```

---

### Magic Path Bitmask (Random Magic)

**Purpose**: Encodes which magic paths are available for random path selection.

| Path | Index | Bitmask Value |
|------|-------|---------------|
| Fire (F) | 0 | 128 (0x80) |
| Air (A) | 1 | 256 (0x100) |
| Water (W) | 2 | 512 (0x200) |
| Earth (E) | 3 | 1024 (0x400) |
| Astral (S) | 4 | 2048 (0x800) |
| Death (D) | 5 | 4096 (0x1000) |
| Nature (N) | 6 | 8192 (0x2000) |
| Glamour (G) | 7 | 16384 (0x4000) |
| Blood (B) | 8 | 32768 (0x8000) |
| Holy (H) | 9 | 65536 (0x10000) |

**Bitmask Calculation**: `bitmask = 128 << pathIndex`

**Implementation**:
```javascript
const pathLetterToIndex = { F:0, A:1, W:2, E:3, S:4, D:5, N:6, G:7, B:8, H:9 };
const pathIndexToLetter = { 0:'F', 1:'A', 2:'W', 3:'E', 4:'S', 5:'D', 6:'N', 7:'G', 8:'B', 9:'H' };
const pathLetterToBitmask = { F:128, A:256, W:512, E:1024, S:2048, D:4096, N:8192, G:16384, B:32768, H:65536 };

// Parse bitmask to array of path letters
function bitmaskToPaths(bitmask) {
    const paths = [];
    for (const [letter, value] of Object.entries(pathLetterToBitmask)) {
        if (bitmask & value) paths.push(letter);
    }
    return paths;
}

// Convert path letters to bitmask
function pathsToBitmask(pathLetters) {
    let bitmask = 0;
    for (const letter of pathLetters) {
        bitmask |= pathLetterToBitmask[letter];
    }
    return bitmask;
}

// Export custommagic command
function exportCustomMagic(mask, chance, level) {
    return `#custommagic ${mask} ${chance} ${level}`;
}
```

**Example**:
```
Fire + Water + Earth = 128 + 512 + 1024 = 1664
#custommagic 1664 100 1  -- 100% chance of 1 level in F/W/E
```

---

### Special Damage Types

**Purpose**: Weapon special damage flags.

| Flag | Bit Value | Description |
|------|-----------|-------------|
| Armor Piercing | 1 | Ignores armor |
| Armor Negating | 2 | Completely negates armor |
| Magic | 4 | Magic damage |
| Fire | 8 | Fire damage |
| Cold | 16 | Cold damage |
| Shock | 32 | Lightning damage |
| Poison | 64 | Poison damage |
| ... | ... | Additional flags |

**Implementation**:
```javascript
function hasSpecialDamage(flags, type) {
    const specialDamageFlags = {
        armorPiercing: 1,
        armorNegating: 2,
        magic: 4,
        fire: 8,
        cold: 16,
        shock: 32,
        poison: 64
    };
    return (flags & specialDamageFlags[type]) !== 0;
}
```

---

## Lookup Mappings

These convert between numeric codes and named commands.

### Leader Value

**Purpose**: Converts leadership numeric value to specific mod commands.

| Value | Command | Description |
|-------|---------|-------------|
| 0 | `#noleader` | Cannot lead |
| 10 | `#poorleader` | Poor leader (10) |
| 40 | `#okleader` | Standard leader (40) |
| 80 | `#goodleader` | Good leader (80) |
| 120 | `#expertleader` | Expert leader (120) |
| 160 | `#superiorleader` | Superior leader (160) |

**Implementation**:
```javascript
const leaderValueToCommand = {
    0: 'noleader',
    10: 'poorleader',
    40: 'okleader',
    80: 'goodleader',
    120: 'expertleader',
    160: 'superiorleader'
};

const commandToLeaderValue = {
    noleader: 0,
    poorleader: 10,
    okleader: 40,
    goodleader: 80,
    expertleader: 120,
    superiorleader: 160
};

// Export leadership
function exportLeader(value) {
    if (leaderValueToCommand[value]) {
        return `#${leaderValueToCommand[value]}`;
    }
    // Non-standard value, use #leader command
    return `#leader ${value}`;
}
```

---

### Magic Leader Value

**Purpose**: Same pattern as regular leadership but for magic leadership.

| Value | Command |
|-------|---------|
| 0 | `#nomagicleader` |
| 10 | `#poormagicleader` |
| 40 | `#okmagicleader` |
| 80 | `#goodmagicleader` |
| 120 | `#expertmagicleader` |
| 160 | `#superiormagicleader` |

---

### Undead Leader Value

**Purpose**: Same pattern for undead leadership.

| Value | Command |
|-------|---------|
| 0 | `#noundeadleader` |
| 10 | `#poorundeadleader` |
| 40 | `#okundeadleader` |
| 80 | `#goodundeadleader` |
| 120 | `#expertundeadleader` |
| 160 | `#superiorundeadleader` |

---

## Auto-Calculated Values

These values are computed from other properties and typically should NOT be exported (they're derived).

### Commander Gold Cost

**Purpose**: Auto-calculated based on unit capabilities.

**Formula Components**:
```javascript
// Leadership cost table
const leaderCostTable = {
    0: 0,      // noleader
    10: 5,     // poorleader
    40: 10,    // okleader
    80: 15,    // goodleader
    120: 40,   // expertleader
    160: 80    // superiorleader
};

// Magic path costs (per level)
const magicPathCost = 25; // per level of magic

// Spy cost
const spyCost = 10;

// Holy cost (per priest level)
const holyCostPerLevel = 15;

function calculateCommanderGoldCost(unit) {
    let cost = unit.basegoldcost || 0;

    // Add leadership costs
    cost += leaderCostTable[unit.leader] || 0;
    cost += leaderCostTable[unit.magicleader] || 0;
    cost += leaderCostTable[unit.undeadleader] || 0;

    // Add magic costs
    const magicLevels = (unit.F || 0) + (unit.A || 0) + (unit.W || 0) +
                        (unit.E || 0) + (unit.S || 0) + (unit.D || 0) +
                        (unit.N || 0) + (unit.G || 0) + (unit.B || 0);
    cost += magicLevels * magicPathCost;

    // Add holy cost
    cost += (unit.H || 0) * holyCostPerLevel;

    // Add spy cost
    if (unit.spy) cost += spyCost;

    return cost;
}
```

**Export Note**: Export the base gold cost, not calculated cost. The game recalculates.

---

### Resource Cost

**Purpose**: Calculated from base cost plus weapon/armor costs scaled by size.

**Formula**:
```javascript
function calculateResourceCost(unit) {
    let rcost = unit.basercost || 0;

    // Add weapon resource costs
    for (const weaponId of unit.weapons || []) {
        const weapon = getWeapon(weaponId);
        rcost += (weapon.rcost || 0) * (unit.ressize || 2) / 3;
    }

    // Add armor resource costs
    for (const armorId of unit.armors || []) {
        const armor = getArmor(armorId);
        rcost += (armor.rcost || 0) * (unit.ressize || 2) / 3;
    }

    return Math.round(rcost);
}
```

**Export Note**: Export base `#rcost`, not calculated total.

---

### Protection (Combined)

**Purpose**: Display value combining natural protection and armor.

**Formula**:
```javascript
// Diminishing returns formula
function calculateTotalProtection(natProt, armorProt) {
    return natProt + armorProt - Math.floor((natProt * armorProt) / 40);
}

// Inverse: Given total and armor, find natural
function extractNaturalProtection(totalProt, armorProt) {
    // Solving: total = nat + armor - (nat * armor / 40)
    // total = nat * (1 - armor/40) + armor
    // nat = (total - armor) / (1 - armor/40)
    if (armorProt >= 40) return 0; // Edge case
    return Math.round((totalProt - armorProt) / (1 - armorProt/40));
}
```

**Export Note**: Export `#prot` (natural protection only).

---

### Encumbrance

**Purpose**: Total encumbrance from armor and other sources.

**Formula**:
```javascript
function calculateEncumbrance(unit) {
    let enc = unit.baseenc || 0;

    // Add armor encumbrance (reduced by unit size)
    for (const armorId of unit.armors || []) {
        const armor = getArmor(armorId);
        const armorEnc = armor.enc || 0;
        // Larger units have relatively less encumbrance
        enc += Math.max(0, armorEnc - Math.floor((unit.size - 2) / 2));
    }

    return enc;
}
```

---

### Movement (Map Movement)

**Purpose**: Calculated from base move and armor penalties.

**Formula**:
```javascript
function calculateMapMove(unit) {
    let move = unit.basemapmove || 1;

    // Heavy armor reduces movement
    for (const armorId of unit.armors || []) {
        const armor = getArmor(armorId);
        if (armor.heavy) move = Math.max(1, move - 1);
    }

    return move;
}
```

---

## Spell Encodings

### Path Requirements

**Purpose**: Spell path requirements stored as indices.

| Property | Description |
|----------|-------------|
| `path1` | Primary path index (0-9) |
| `path1level` | Required level in primary path |
| `path2` | Secondary path index (0-9, or -1 for none) |
| `path2level` | Required level in secondary path |

**Mod Command**: `#school` and `#path`
```
#school 0        -- Fire school
#path 0          -- Primary path is Fire
#pathlevel 2     -- Requires Fire 2
#path2 4         -- Secondary path is Astral
#pathlevel2 1    -- Requires Astral 1
```

**Implementation**:
```javascript
function exportSpellPath(spell) {
    const lines = [];
    if (spell.path1 >= 0) {
        lines.push(`#school ${spell.path1}`);
        if (spell.path1level) {
            lines.push(`#pathlevel ${spell.path1level}`);
        }
    }
    if (spell.path2 >= 0) {
        lines.push(`#path2 ${spell.path2}`);
        if (spell.path2level) {
            lines.push(`#pathlevel2 ${spell.path2level}`);
        }
    }
    return lines;
}
```

---

### Fatigue/Gem Cost Encoding

**Purpose**: Spell costs encoded in single value, high bits indicate gem cost.

**Ranges**:
- 0-999: Fatigue cost
- 1000+: First digit is gem count, rest is fatigue

**Implementation**:
```javascript
function parseSpellCost(encoded) {
    if (encoded < 1000) {
        return { fatigue: encoded, gems: 0 };
    }
    const gems = Math.floor(encoded / 1000);
    const fatigue = encoded % 1000;
    return { fatigue, gems };
}

function encodeSpellCost(fatigue, gems) {
    return gems * 1000 + fatigue;
}
```

**Example**:
```
fatcost: 100   → 100 fatigue, 0 gems
fatcost: 2050  → 50 fatigue, 2 gems
```

---

### Range/AOE Level Scaling

**Purpose**: Range and AOE can scale with caster level.

**Encoding Pattern**: Base value + scaling indicator

| Encoded Value | Meaning |
|---------------|---------|
| 0-99 | Fixed value |
| 100+ | Base + (caster_level - req_level) |

**Implementation**:
```javascript
function parseScalingValue(encoded, reqLevel) {
    if (encoded < 100) {
        return { base: encoded, scaling: false };
    }
    return {
        base: encoded - 100,
        scaling: true,
        // Actual value at caster level X: base + (X - reqLevel)
    };
}

function formatScalingValue(base, scaling) {
    return scaling ? base + 100 : base;
}
```

---

## Array/Reference Handling

### Weapon/Armor References

**Purpose**: Units reference weapons/armors by ID or name.

**Storage**: Array of IDs `[5, 10, 23]`

**Export**: Multiple commands
```
#weapon 5
#weapon 10
#weapon 23
```

**Implementation**:
```javascript
function exportArrayRef(cmdName, ids) {
    return ids.map(id => `#${cmdName} ${id}`).join('\n');
}

// Or with names for readability
function exportArrayRefWithNames(cmdName, ids, lookupTable) {
    return ids.map(id => {
        const entity = lookupTable[id];
        const ref = entity?.name ? `"${entity.name}"` : id;
        return `#${cmdName} ${ref}`;
    }).join('\n');
}
```

---

### Magic Skill Export

**Purpose**: Export fixed magic path levels.

**Storage**: `{ F: 2, A: 1, D: 3 }`

**Export**:
```
#magicskill 0 2   -- Fire 2
#magicskill 1 1   -- Air 1
#magicskill 5 3   -- Death 3
```

**Implementation**:
```javascript
function exportMagicPaths(entity) {
    const lines = [];
    const paths = ['F', 'A', 'W', 'E', 'S', 'D', 'N', 'G', 'B', 'H'];

    paths.forEach((letter, index) => {
        const level = entity[letter];
        if (level && level > 0) {
            lines.push(`#magicskill ${index} ${level}`);
        }
    });

    return lines;
}
```

---

### Random Magic Export

**Purpose**: Export random path assignments.

**Storage**: Array of `{ mask, chance, level }`

**Export**:
```javascript
function exportRandomPaths(entity) {
    const lines = [];

    // Check for randompaths array
    if (entity.randompaths) {
        for (const rp of entity.randompaths) {
            lines.push(`#custommagic ${rp.mask} ${rp.chance} ${rp.level || 1}`);
        }
    }

    return lines;
}
```

---

### Site Gem Income

**Purpose**: Sites produce gems of specific paths.

**Storage**: Array like `[{ path: 0, amount: 2 }, { path: 5, amount: 1 }]`

**Export**:
```
#gems 0 2   -- 2 Fire gems
#gems 5 1   -- 1 Death gem
```

**Implementation**:
```javascript
function exportSiteGems(gems) {
    return gems.map(g => `#gems ${g.path} ${g.amount}`).join('\n');
}
```

---

### Gemprod (Unit Gem Production)

**Purpose**: Units that produce gems.

**Storage**: String like `"F1A2"` or object

**Export**:
```
#gemprod 0 1   -- Produces 1 Fire gem
#gemprod 1 2   -- Produces 2 Air gems
```

**Implementation**:
```javascript
function parseGemprodString(str) {
    const result = [];
    const regex = /([FAWESDNGBH])(\d+)/g;
    let match;
    while ((match = regex.exec(str)) !== null) {
        result.push({
            path: pathLetterToIndex[match[1]],
            amount: parseInt(match[2])
        });
    }
    return result;
}

function exportGemprod(gemprod) {
    const gems = typeof gemprod === 'string' ? parseGemprodString(gemprod) : gemprod;
    return gems.map(g => `#gemprod ${g.path} ${g.amount}`).join('\n');
}
```

---

## Summary: Export Priority

When exporting entities, use this priority:

1. **Direct Export**: Properties with simple 1:1 mapping
2. **Transform Export**: Properties needing mathematical transformation
3. **Lookup Export**: Properties mapping to named commands
4. **Skip Calculated**: Don't export auto-calculated derived values

| Category | Examples | Action |
|----------|----------|--------|
| Direct | hp, str, att, def | Export as-is |
| Transform | incunrest, popkill, stealthy | Apply inverse formula |
| Lookup | leader, magicleader | Map to named command |
| Bitmask | itemslots, custommagic mask | Export full bitmask |
| Calculated | goldcost, totalprotection | Skip (derived) |
| Array | weapons, armors | Multiple commands |

---

## Implementation Reference

See `scripts/DMI/ModExport.js` for the full implementation of these transformations in the export system.

Key functions:
- `exportEntity()` - Main export dispatcher
- `formatters.*` - Individual formatter functions
- `exportMagicPath()` - Magic skill export
- `exportRandomPaths()` - Random magic export
- `exportLeader()` - Leadership command selection
- `exportItemSlots()` - Item slot bitmask export
