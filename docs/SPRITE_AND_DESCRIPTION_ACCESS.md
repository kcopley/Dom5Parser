# Sprite and Description Access Patterns

This document outlines how to resolve sprites and description files for each entity type in Dominions 6. Designed for porting to other platforms (e.g., WPF).

---

## Directory Structure

```
gamedata/
├── sprites/              # Unit sprites
│   ├── 0001_1.png        # Unit ID 1, pose 1 (idle)
│   ├── 0001_2.png        # Unit ID 1, pose 2 (attack)
│   ├── 0003_rider_1.png  # Mounted unit - rider only, pose 1
│   ├── 0003_rider_2.png  # Mounted unit - rider only, pose 2
│   └── ...
│
├── items/                # Item sprites
│   ├── item1.png         # Item ID 1
│   ├── item2.png         # Item ID 2
│   └── ...
│
├── sites/                # Magic site sprites
│   ├── sites_0000.png    # Sprite number 0
│   ├── sites_0001.png    # Sprite number 1
│   └── ...
│
├── unitdescr/            # Unit descriptions (by ID)
│   ├── 0001.txt
│   ├── 0002.txt
│   └── ...
│
├── spelldescr/           # Spell descriptions (by name)
│   ├── Fireball.txt              # Main description
│   ├── detailsFireball.txt       # Extended details
│   ├── portentFireball.txt       # Dire portent info (optional)
│   ├── cureFireball.txt          # Cure info (optional)
│   └── ...
│
└── itemdescr/            # Item descriptions (by name)
    ├── SwordofHeroes.txt
    └── ...
```

---

## Resolution Logic by Entity Type

### Units

#### Sprites

```
Input:  unit.id (integer)
Output: Two sprite paths (idle and attack poses)

Path 1: sprites/{ID:D4}_1.png
Path 2: sprites/{ID:D4}_2.png

For mounted units with rider sprites:
Path 3: sprites/{ID:D4}_rider_1.png
Path 4: sprites/{ID:D4}_rider_2.png
```

**Format**: 4-digit zero-padded ID

| Unit ID | Sprite Path 1 | Sprite Path 2 |
|---------|---------------|---------------|
| 1 | `sprites/0001_1.png` | `sprites/0001_2.png` |
| 42 | `sprites/0042_1.png` | `sprites/0042_2.png` |
| 100 | `sprites/0100_1.png` | `sprites/0100_2.png` |
| 2456 | `sprites/2456_1.png` | `sprites/2456_2.png` |

**Pseudo-code**:
```csharp
string GetUnitSpritePath(int unitId, int pose = 1)
{
    return $"sprites/{unitId:D4}_{pose}.png";
}

string GetUnitRiderSpritePath(int unitId, int pose = 1)
{
    return $"sprites/{unitId:D4}_rider_{pose}.png";
}
```

#### Descriptions

```
Input:  unit.id (integer)
Output: Text file path

Path: unitdescr/{ID:D4}.txt
```

| Unit ID | Description Path |
|---------|------------------|
| 1 | `unitdescr/0001.txt` |
| 42 | `unitdescr/0042.txt` |
| 100 | `unitdescr/0100.txt` |

**Pseudo-code**:
```csharp
string GetUnitDescriptionPath(int unitId)
{
    return $"unitdescr/{unitId:D4}.txt";
}
```

---

### Items

#### Sprites

```
Input:  item.id (integer)
Output: Single sprite path

Path: items/item{ID}.png
```

**Format**: NOT zero-padded

| Item ID | Sprite Path |
|---------|-------------|
| 1 | `items/item1.png` |
| 42 | `items/item42.png` |
| 100 | `items/item100.png` |

**Pseudo-code**:
```csharp
string GetItemSpritePath(int itemId)
{
    return $"items/item{itemId}.png";
}
```

#### Descriptions

```
Input:  item.name (string)
Output: Text file path

Path: itemdescr/{SanitizedName}.txt
```

**Name Sanitization**: Remove all non-alphanumeric characters except hyphens

| Item Name | Sanitized | Description Path |
|-----------|-----------|------------------|
| "Sword of Heroes" | "SwordofHeroes" | `itemdescr/SwordofHeroes.txt` |
| "Fire Brand" | "FireBrand" | `itemdescr/FireBrand.txt` |
| "King's Crown" | "KingsCrown" | `itemdescr/KingsCrown.txt` |
| "Anti-Magic Amulet" | "Anti-MagicAmulet" | `itemdescr/Anti-MagicAmulet.txt` |

**Pseudo-code**:
```csharp
string SanitizeName(string name)
{
    return Regex.Replace(name, @"[^a-zA-Z0-9\-]", "");
}

string GetItemDescriptionPath(string itemName)
{
    return $"itemdescr/{SanitizeName(itemName)}.txt";
}
```

---

### Spells

#### Sprites

Spells do not have individual sprites.

#### Descriptions

Spells have up to 4 description files:

```
Input:  spell.name (string)
Output: Multiple text file paths

Main:    spelldescr/{SanitizedName}.txt
Details: spelldescr/details{SanitizedName}.txt
Portent: spelldescr/portent{SanitizedName}.txt  (optional)
Cure:    spelldescr/cure{SanitizedName}.txt     (optional)
```

| Spell Name | Main Path | Details Path |
|------------|-----------|--------------|
| "Fireball" | `spelldescr/Fireball.txt` | `spelldescr/detailsFireball.txt` |
| "Fire Storm" | `spelldescr/FireStorm.txt` | `spelldescr/detailsFireStorm.txt` |
| "Soul Slay" | `spelldescr/SoulSlay.txt` | `spelldescr/detailsSoulSlay.txt` |

**Pseudo-code**:
```csharp
string GetSpellDescriptionPath(string spellName)
{
    return $"spelldescr/{SanitizeName(spellName)}.txt";
}

string GetSpellDetailsPath(string spellName)
{
    return $"spelldescr/details{SanitizeName(spellName)}.txt";
}

string GetSpellPortentPath(string spellName)
{
    return $"spelldescr/portent{SanitizeName(spellName)}.txt";
}

string GetSpellCurePath(string spellName)
{
    return $"spelldescr/cure{SanitizeName(spellName)}.txt";
}
```

---

### Sites (Magic Sites)

#### Sprites

```
Input:  site.sprite (integer) - NOTE: This is the sprite number, NOT the site ID
Output: Single sprite path

Path: sites/sites_{SPRITE:D4}.png
```

**Format**: 4-digit zero-padded sprite number

**Important**: Sites use a `sprite` field from the CSV, which may differ from the site's `id`.

| Sprite Number | Sprite Path |
|---------------|-------------|
| 0 | `sites/sites_0000.png` |
| 15 | `sites/sites_0015.png` |
| 234 | `sites/sites_0234.png` |

**Pseudo-code**:
```csharp
string GetSiteSpritePath(int spriteNumber)
{
    return $"sites/sites_{spriteNumber:D4}.png";
}
```

#### Descriptions

Sites do not have separate description files. Descriptions are embedded in the CSV data.

---

## Summary Table

| Entity | Sprite Key | Sprite Format | Description Key | Description Format |
|--------|------------|---------------|-----------------|-------------------|
| **Unit** | `id` | `sprites/{id:D4}_1.png` | `id` | `unitdescr/{id:D4}.txt` |
| **Item** | `id` | `items/item{id}.png` | `name` | `itemdescr/{sanitize(name)}.txt` |
| **Spell** | N/A | N/A | `name` | `spelldescr/{sanitize(name)}.txt` |
| **Site** | `sprite` | `sites/sites_{sprite:D4}.png` | N/A | N/A (inline in CSV) |

---

## Utility Functions (C# Reference Implementation)

```csharp
public static class Dom6AssetResolver
{
    /// <summary>
    /// Pads integer to 4 digits with leading zeros
    /// </summary>
    public static string PadId(int id) => id.ToString("D4");

    /// <summary>
    /// Removes non-alphanumeric characters (except hyphens) from name
    /// </summary>
    public static string SanitizeName(string name)
    {
        return Regex.Replace(name, @"[^a-zA-Z0-9\-]", "");
    }

    // === UNITS ===

    public static string GetUnitSprite(int unitId, int pose = 1)
        => $"sprites/{unitId:D4}_{pose}.png";

    public static string GetUnitRiderSprite(int unitId, int pose = 1)
        => $"sprites/{unitId:D4}_rider_{pose}.png";

    public static string GetUnitDescription(int unitId)
        => $"unitdescr/{unitId:D4}.txt";

    // === ITEMS ===

    public static string GetItemSprite(int itemId)
        => $"items/item{itemId}.png";

    public static string GetItemDescription(string itemName)
        => $"itemdescr/{SanitizeName(itemName)}.txt";

    // === SPELLS ===

    public static string GetSpellDescription(string spellName)
        => $"spelldescr/{SanitizeName(spellName)}.txt";

    public static string GetSpellDetails(string spellName)
        => $"spelldescr/details{SanitizeName(spellName)}.txt";

    public static string GetSpellPortent(string spellName)
        => $"spelldescr/portent{SanitizeName(spellName)}.txt";

    public static string GetSpellCure(string spellName)
        => $"spelldescr/cure{SanitizeName(spellName)}.txt";

    // === SITES ===

    public static string GetSiteSprite(int spriteNumber)
        => $"sites/sites_{spriteNumber:D4}.png";
}
```

---

## Notes for WPF Implementation

1. **File Existence**: Not all description files exist. Check for file existence before loading, especially for:
   - Spell portent/cure files (many spells don't have these)
   - Rider sprites (only mounted units have these)

2. **Modded Content**: When loading mods, sprites may be provided with custom paths. The mod parser sets `spr1`/`spr2` fields that override the default ID-based paths.

3. **Description Fallback**: If a description file doesn't exist, the entity may have an inline `descr` field from the CSV or mod file.

4. **Sprite Poses**:
   - `_1.png` = idle/standing pose
   - `_2.png` = attack/action pose
   - Users typically toggle between these on click/hover

5. **Site Sprite vs ID**: Sites use a separate `sprite` field, not their `id`. Always use the `sprite` field value for image lookup.
