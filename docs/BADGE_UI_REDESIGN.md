# Badge-Based UI Redesign

## Overview

This document outlines the significant change from manually-defined code sections to a JSON-driven badge system for entity properties in the Dom5Editor.

## Goals

1. **Compact Layout**: Display 70-100 properties visible without scrolling using horizontal wrapping badges
2. **Data-Driven**: Define all commands and categories in JSON files instead of hardcoded C# arrays
3. **Verifiable**: Easily compare JSON definitions against the full command list to identify missing commands
4. **Maintainable**: Add new commands by editing JSON files, not code

## Architecture

### JSON Files

Located in `Dom5Editor/Data/`:

- `monster_badges.json` - Badge categories for monsters
- `weapon_badges.json` - Badge properties for weapons
- `armor_badges.json` - Badge properties for armor
- `item_badges.json` - Badge properties for items
- `site_badges.json` - Badge properties for sites
- `spell_badges.json` - Badge properties for spells

### JSON Structure

The JSON uses a `sections` array with each section having a renderer type:

```json
{
  "sections": [
    {
      "id": "stats",
      "displayName": "STATS",
      "description": "Core monster statistics",
      "renderer": "statsGrid",
      "readOnly": false,
      "commands": [
        { "name": "hp", "display": "HP", "type": "int", "default": 10 },
        { "name": "str", "display": "STR", "type": "int", "default": 10 }
      ]
    },
    {
      "id": "magicpaths",
      "displayName": "MAGIC PATHS",
      "renderer": "magicPathEditor",
      "commands": [
        { "name": "magicskill", "display": "Magic Skill", "type": "path", "paths": ["F", "A", "W", "E", "S", "D", "N", "B", "H"] },
        { "name": "custommagic", "display": "Random Magic", "type": "randompaths" }
      ]
    },
    {
      "id": "types",
      "displayName": "TYPES",
      "renderer": "badge",
      "readOnly": true,
      "commands": [
        { "name": "undead", "display": "Undead", "type": "flag" }
      ]
    },
    {
      "id": "resistances",
      "displayName": "RESISTANCES",
      "renderer": "coloredBadge",
      "commands": [
        { "name": "fireres", "display": "Fire", "type": "int", "default": 0, "color": "#8B4513", "borderColor": "#FF8C00" }
      ]
    }
  ],
  "layout": {
    "monster": ["stats", "magicpaths", "types", "general", "combat", "resistances"]
  },
  "renderers": {
    "badge": { "description": "Standard compact badge", "supportedTypes": ["flag", "int"] },
    "coloredBadge": { "description": "Colored badge for resistances", "supportedTypes": ["int"], "requiresColor": true },
    "statsGrid": { "description": "3-column grid layout", "supportedTypes": ["int"], "columns": 3 },
    "magicPathEditor": { "description": "Magic path editor with colored levels", "supportedTypes": ["path", "randompaths"] }
  }
}
```

### Command Types

- **flag**: Boolean command (e.g., `#flying`, `#undead`)
- **int**: Integer value command (e.g., `#hp 50`, `#fireres 5`)
- **path**: Magic path level command (for magicPathEditor)
- **randompaths**: Random magic configuration (for magicPathEditor)

### Renderers

| Renderer | Description | Supported Types |
|----------|-------------|-----------------|
| `badge` | Standard compact badge with optional value | flag, int |
| `coloredBadge` | Colored badge for resistances/special values | int (requires color/borderColor) |
| `statsGrid` | 3-column grid layout for core stats | int |
| `magicPathEditor` | Custom magic path editor with colored level badges | path, randompaths |

### Magic Path Colors

Defined in the `renderers.magicPathEditor.pathColors` config:
- F (Fire): #FF4500
- A (Air): #87CEEB
- W (Water): #4169E1
- E (Earth): #8B4513
- S (Astral): #FFD700
- D (Death): #2F4F4F
- N (Nature): #228B22
- B (Blood): #8B0000
- H (Holy): #FFFFFF

## Badge Categories for Monsters

### STATS (statsGrid renderer)
Core statistics in a 3-column grid:
- HP, STR, ATT, DEF, MR, MOR, SIZE, PROT, ENC, PREC, AP, MAP

### MAGIC PATHS (magicPathEditor renderer)
Magic skill levels with colored badges per path.

### TYPES (read-only badges)
Creature type flags inherited from copystats. Not editable.
- humanoid, undead, demon, magicbeing, animal, etc.

### GENERAL (badge renderer)
Non-combat abilities including:
- Movement: flying, aquatic, teleport, etc.
- Leadership: noleader through superiorundeadleader
- Special: heal, healer, taxcollector, mason, etc.
- Stealth: spy, assassin, stealthy (int)
- Province effects: popkill, incunrest, spreaddom (int)
- Recruitment: reqlab, reqtemple
- Costs: gcost, rcost, rpcost (int)

### COMBAT (badge renderer)
Combat abilities and auras:
- Abilities: awe, fear, berserk, ethereal, etc.
- Auras: heat, cold, fireshield, poisoncloud, etc.

### RESISTANCES (coloredBadge renderer)
Colored value badges for protections:
- Elemental: fireres, coldres, shockres, poisonres
- Physical: bluntres, pierceres, slashres
- Special: regeneration, invulnerable, airshield, iceprot, magicimmune

## Migration Status

### Completed
- [x] CompactBadge control
- [x] BadgeWrapPanel container
- [x] BadgeItem data class with ValueChanged event
- [x] Types badges (read-only)
- [x] General badges (merged movement, leadership, special, province)
- [x] Combat badges (merged combat + auras)
- [x] Resistance badges (colored)
- [x] JSON structure with sections array
- [x] Stats section definition in JSON with statsGrid renderer
- [x] Magic paths section definition with magicPathEditor renderer
- [x] Layout order specification
- [x] Renderer definitions with metadata
- [x] Removed old vertical list sections from MonsterView

### Completed Recently
- [x] JSON loader for property categories (`BadgeConfigLoader.cs`)
- [x] Property model classes (`BadgeConfig.cs`, `PropertyItem`, `AvailablePropertyItem`)
- [x] Integration with EntityViewModels via `BuildBadgesFromSection()`
- [x] Tooltip descriptions from command reference JSON
- [x] Removed fallback methods (RefreshTypeBadgesFallback, etc.)
- [x] Renamed `BadgeItem` to `PropertyItem`, `AvailableBadgeItem` to `AvailablePropertyItem`
- [x] Fixed JSON command lookup (added `#` prefix handling for CommandsMap)

**Note:** Some hardcoded Command arrays still exist in EntityViewModels.cs (TypeCommands, LeaderCommands, MovementCommands, ResistanceCommands, CombatCommands, AuraCommands, SpecialCommands). These work alongside the JSON badge system.

### Future
- [ ] Weapon/Armor/Spell badge definitions (create JSON files)
- [ ] Custom renderer registration system
- [ ] Coverage verification tool (compare JSON vs full command list)
- [ ] Dynamic UI generation for stats section from JSON

### Long-Term Vision
- [ ] **Replace Command enum with JSON definitions** - Currently the `Command` enum in `Dom5Edit/Commands/Command.cs` is deeply integrated into the codebase. A future enhancement would be to define all commands in JSON files, eliminating the need for code changes when adding new commands. This would require:
  - JSON command definitions with name, description, argument types, entity types
  - Runtime command lookup instead of compile-time enum
  - Migration of all `Command` enum usages throughout the codebase
  - This is a significant architectural change and should be planned carefully

## Files Changed

### New Files
- `Dom5Editor/UI/Controls/CompactBadge.xaml(.cs)` - Individual badge control (visual rendering)
- `Dom5Editor/UI/Controls/BadgeWrapPanel.xaml(.cs)` - Container with add dropdown
- `Dom5Editor/UI/Controls/BadgeItem.cs` - Data model for properties (`PropertyItem`, `AvailablePropertyItem`)
- `Dom5Editor/Data/monster_badges.json` - Monster category definitions with renderer config
- `Dom5Editor/Data/BadgeConfig.cs` - Model classes for JSON deserialization
- `Dom5Editor/Data/BadgeConfigLoader.cs` - Loads property config from JSON, provides command descriptions for tooltips

### Modified Files
- `Dom5Editor/UI/Views/MonsterView.xaml` - Simplified to 4 property sections (Types, General, Combat, Resistances)
- `Dom5Editor/UI/ViewModels/EntityViewModels.cs` - Property collections, refresh methods, JSON-only integration
- `Dom5Editor/Dom5Editor.csproj` - Added Content item for `*_badges.json` files (copied to output directory)

## Technical Notes

### Command Name Prefix
The `CommandsMap` in `Dom5Edit/Commands/Command.cs` uses `#` prefixed command names (e.g., `#flying`, `#aquatic`). The JSON configuration files omit this prefix for readability. The `BadgeConfigLoader.TryGetCommand()` method automatically adds the `#` prefix when looking up commands.

### JSON File Location
The JSON configuration files are stored in `Dom5Editor/Data/` and copied to the output directory at build time. The `BadgeConfigLoader` searches multiple paths to find the files:
1. Assembly location + `/Data/`
2. AppDomain.BaseDirectory + `/Data/`
3. Current working directory + `/Data/`

## Verification

To verify coverage against the full command list:

```bash
python3 tools/verify_badge_coverage.py
```

This compares `Data/monster_badges.json` against `docs/pdf_extracted/commands_by_entity_clean.json` and reports any missing commands.
