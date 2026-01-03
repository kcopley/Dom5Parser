# UI Editor Enhancement Plan

This document outlines the planned enhancements to build out Dom5Parser into a full UI-based .dm file editor.

## Goal

Create a UI-based editor that:
- Displays all entities (monsters, weapons, spells, etc.) in navigable lists
- Provides editing interfaces for each entity type
- Saves changes back to .dm format cleanly
- **Primary focus: Dominions 6** (Dom5 support maintained where practical)

## Phase 1: Property Metadata System (COMPLETE)

Add metadata to property definitions to support UI generation.

### PropertyDefinition Class
```csharp
public class PropertyDefinition
{
    public Command Command { get; set; }
    public Type PropertyType { get; set; }

    // UI metadata
    public string DisplayName { get; set; }
    public string Category { get; set; }
    public string Tooltip { get; set; }
    public int DisplayOrder { get; set; }

    // Validation
    public int? MinValue { get; set; }
    public int? MaxValue { get; set; }
    public bool AllowMultiple { get; set; }

    // For references
    public EntityType? TargetEntityType { get; set; }
}
```

### Categories for Monster Properties (Example)
- **Identity**: Name, Fixed Name, Description
- **Sprites**: SPR1, SPR2, Speciallook, Drawsize
- **Combat Stats**: HP, STR, ATT, DEF, PREC, PROT, MR, MOR, ENC, AP
- **Movement**: Mapmove, Flying, Swimming, Aquatic, Amphibian, Sailing
- **Magic**: Magicskill, Custommagic, Magicboost, various path ranges
- **Leadership**: Leader commands, Magic leader, Undead leader
- **Equipment**: Weapon, Armor, Itemslots, Startitem
- **Resistances**: Fireres, Coldres, Shockres, Poisonres, etc.
- **Shape Changing**: Shapechange, Firstshape, Secondshape, etc.
- **Summoning**: Domsummon, Makemonsters, Battlesum, etc.
- **Recruitment**: Gcost, Rcost, Rpcost, Slowrec, Reclimit
- **Miscellaneous**: Flags and other properties

### Implementation Status
- `PropertyDefinition.cs` - Core metadata class with fluent builders
- `PropertyMetadata.cs` - Registry with lookup, validation, and sample definitions
- `MetadataLoader.cs` - JSON loader for loading metadata from extracted PDF data
- Sample metadata defined for common commands (HP, STR, weapons, etc.)

### Remaining Work
- Load comprehensive metadata from `commands_clean.json` at startup
- Integrate metadata into ViewModels for display names/tooltips
- Wire up validation using metadata constraints

## Phase 2: Mod Class Refactoring

### 2.1: Vanilla Data Migration (FIRST)

**Goal:** Switch from TSV spreadsheet loading to vanilla.dm file parsing for Dom6.

Current state:
- VanillaLoader.cs loads from TSV files in VanillaData/ folder
- Data converted to internal Mod representation

Migration tasks:
- Analyze current VanillaLoader implementation and internal format
- Verify vanilla.dm file location and format
- Update loading to parse vanilla.dm using existing Mod.Parse()
- Mark loaded entities as vanilla/read-only
- Remove or deprecate TSV loading code
- Update ID ranges for Dom6 (already partially done in Mod.cs)

This ensures all vanilla entity references resolve correctly before refactoring.

### 2.2: Split Mod Class Responsibilities

Split the ~960 line Mod class into focused components:

**ModParser**
- Line-by-line parsing logic
- Command recognition
- Multi-line string handling
- Comment preservation

**ModDocument**
- Entity storage (Database, Dependents)
- Entity queries and lookups
- Change tracking for dirty state

**ModExporter**
- Export formatting
- ID remapping during merge
- File writing

**Mod (Facade)**
- High-level operations
- Import/Export entry points
- Backwards compatibility with existing code

## Phase 3: Editor Infrastructure

### Change Notification
- Implement `INotifyPropertyChanged` on model classes or use wrapper pattern
- Propagate changes from entities to UI

### Undo/Redo
- Command pattern for entity modifications
- Undo stack per document

### Validation
- Validate entity completeness before export
- Check reference validity (referenced entities exist)
- Warn about ID range violations

### Dirty Tracking
- Track unsaved changes per mod
- Prompt before closing unsaved mods

## Phase 4: UI Components

### Entity List Views
- Sortable, filterable lists for each entity type
- Search/filter by name, ID, properties
- Multi-select for batch operations

### Entity Edit Forms
- Auto-generated from PropertyDefinition metadata
- Grouped by category with collapsible sections
- Appropriate controls per property type:
  - IntProperty → NumericUpDown
  - StringProperty → TextBox
  - CommandProperty → CheckBox
  - Reference types → ComboBox/AutoComplete with entity list
  - BitmaskProperty → Multi-select checkboxes
  - FilePathProperty → TextBox with browse button

### Reference Pickers
- Dropdown with search for selecting referenced entities
- Preview of selected entity
- "Create New" option to add entities inline

## Phase 5: Advanced Features

### Sprite Preview
- Display monster/item sprites in editor
- Support TGA format loading (already have TargaImage utility)

### Mod Comparison
- Diff two mods to see changes
- Useful for reviewing merges

### Validation Report
- Generate report of all issues in a mod
- Missing references, out-of-range values, etc.

### Export Options
- Export single nation with dependencies
- Merge multiple mods with conflict resolution UI
