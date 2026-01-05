# UI Editor Enhancement Plan

This document outlines the planned enhancements to build out Dom5Parser into a full UI-based .dm file editor.

## Goal

Create a UI-based editor that:
- Displays all entities (monsters, weapons, spells, etc.) in navigable lists
- Provides editing interfaces for each entity type
- Saves changes back to .dm format cleanly
- **Primary focus: Dominions 6** (Dom5 support maintained where practical)

---

## Current Status

### Work Priorities

1. ~~**JSON-Driven UI** - Replace hardcoded command arrays with JSON configuration~~ (COMPLETE)
2. ~~**Remove Legacy Code** - Clean up fallback arrays and deprecated ViewModels~~ (COMPLETE)
3. **Feature Expansion** - Reference editors, CUSTOMMAGIC, sprite preview

### Command Coverage

**MonsterView:** 161 / 570 commands (28%)

| Category | Status |
|----------|--------|
| Identity, Core Stats, Leadership | Complete |
| Type, Movement, Resistances, Combat, Auras | Complete (badge-based) |
| Magic (MAGICSKILL only, no CUSTOMMAGIC) | Partial |
| Equipment (WEAPON, ARMOR) | Not started |
| Shapechange, Summoning, Recruitment | Not started |

---

## Phase 1: Property Metadata System (COMPLETE)

Add metadata to property definitions to support UI generation.

### PropertyDefinition Class
```csharp
public class PropertyDefinition
{
    public Command Command { get; set; }
    public Type PropertyType { get; set; }
    public string DisplayName { get; set; }
    public string Category { get; set; }
    public string Tooltip { get; set; }
    public int DisplayOrder { get; set; }
    public int? MinValue { get; set; }
    public int? MaxValue { get; set; }
    public bool AllowMultiple { get; set; }
    public EntityType? TargetEntityType { get; set; }
}
```

### Implementation Status
- `PropertyDefinition.cs` - Core metadata class with fluent builders
- `PropertyMetadata.cs` - Registry with lookup, validation, and sample definitions
- `MetadataLoader.cs` - JSON loader for loading metadata from extracted PDF data

---

## Phase 2: Mod Class Refactoring (COMPLETE)

### 2.1: Vanilla Data Migration
- Added `GameVersion` enum (Dom5, Dom6) to select data source
- VanillaLoader supports both vanilla.dm (Dom6) and TSV spreadsheets (Dom5 legacy)
- Entities loaded from vanilla.dm are marked `IsVanilla = true`

### 2.2: Split Mod Class Responsibilities
- **ModParser** - Line-by-line parsing logic with callback-based command emission
- **ModExporter** - Export formatting for mod header and entities
- **Mod (Facade)** - Delegates parsing/export, manages entities
- **Result: Mod.cs reduced from ~960 to 550 lines (43% smaller)**

### 2.3: Dynamic Spell Effect Loading
- Created `SpellEffectData.cs` singleton for runtime spell effect mappings
- Loads from `spell_effects_mapping.json` and `spell_effect_types.json`
- Vanilla.dm warnings reduced from 110 to 0

---

## Phase 3: Editor Infrastructure (COMPLETE)

### 3.1: Edit Command Infrastructure
- `IEditCommand` interface for undo/redo support
- `CommandHistory` manages undo/redo stacks with dirty tracking
- Property commands: `SetIntPropertyCommand`, `SetStringPropertyCommand`, etc.
- Add/Remove commands: `AddPropertyCommand`, `RemovePropertyCommand`

### 3.2: ViewModel Integration
- All ViewModels use commands when History is available
- Keyboard shortcuts (Ctrl+Z, Ctrl+Y) for undo/redo
- Dirty indicator (*) in toolbar

### 3.3: Validation Framework
- `ReferenceValidator` - Validates entity references exist
- `IdRangeValidator` - Validates IDs within allowed ranges
- `DuplicateIdValidator` - Detects duplicate IDs
- `ModValidator` - Composite validator with summary results

### 3.4: Change Tracking (ChangesMod)
- `EntityChanges` - Tracks property-level changes for a single entity
- `ChangesMod` - Tracks all changes during an editing session
- `ChangesModExporter` - Exports changes standalone or merged with loaded mod

---

## Phase 4: UI Components (IN PROGRESS)

### Command Metadata from JSON
- Load all command display names and descriptions from JSON metadata file
- Display names used in UI labels
- Descriptions used for mouseover tooltips
- Categories defined in JSON for grouping in UI

### Entity List Views
- Sortable, filterable lists for each entity type
- Search/filter by name, ID, properties

### Entity Edit Forms
- Auto-generated from PropertyDefinition metadata
- Grouped by category with collapsible sections

---

## Phase 5: Advanced Features (FUTURE)

- Sprite Preview - Display monster/item sprites
- Mod Comparison - Diff two mods to see changes
- Validation Report - Generate report of all issues
- Export Options - Export single nation with dependencies

---

## Phase 6: Property Editor System (IN PROGRESS)

### 6.1: Theme System (COMPLETE)
- Dark theme with teal accents
- Modification indicators (gold = modified, cyan = session edit)
- `AppTheme.xaml`, `AppResources.xaml`, `Converters.xaml`

### 6.2: Property Editor Controls (COMPLETE)
- `IntPropertyEditor`, `StringPropertyEditor`, `CommandPropertyEditor`
- `ReferencePropertyEditor`, `CommandListEditor`, `IntPropertyListEditor`
- `MagicPathEditor` - Color-coded badges with inline editing

### 6.3: Badge-Based UI (COMPLETE)

See `BADGE_UI_REDESIGN.md` for full details.

**Completed:**
- `CompactBadge` control - Horizontal badge for all property types
- `BadgeWrapPanel` container with add dropdown
- `PropertyItem` / `AvailablePropertyItem` data models with ValueChanged event
- JSON configuration (`monster_badges.json`) with renderer hints
- `BadgeConfigLoader` - Loads property config from JSON (with `#` prefix handling for CommandsMap)
- Integration with EntityViewModels (JSON-only, no fallback)
- Removed all hardcoded Command arrays
- Removed all fallback methods
- Fixed JSON loading path resolution for .NET 8

### 6.4: Entity View Architecture

**MonsterView Sections:**
1. Identity - Name, ID, Sprites, Description
2. Core Stats - HP, STR, ATT, DEF, etc.
3. Magic Paths - MagicPathEditor control
4. Abilities (badge-based):
   - Types (read-only, inherited)
   - General (movement, leadership, special)
   - Combat (abilities + auras)
   - Resistances (colored badges)
5. Equipment - Weapon/Armor references (TODO)

### 6.5: Legacy ViewModel Deprecation

| System | Location | Status |
|--------|----------|--------|
| Legacy VMs | `Dom5Editor/VMs/` | DEPRECATED |
| New UI VMs | `Dom5Editor/UI/ViewModels/` | ACTIVE |

**Keep:** `ModViewModel.cs`, `RelayCommand.cs`, `ViewModelBase.cs`
**Remove after migration:** All other legacy VMs and views

---

## Long-Term Vision

### Replace Command Enum with JSON Definitions

Currently the `Command` enum in `Dom5Edit/Commands/Command.cs` is deeply integrated into the codebase (~1600+ enum values, 3284 lines). A future enhancement would be to define all commands in JSON files, eliminating the need for code changes when adding new commands.

**Benefits:**
- Add new commands by editing JSON, not code
- Single source of truth for command definitions
- Easier Dom5/Dom6 version support
- Community contributions without code changes

**Requirements:**
- JSON command definitions with name, description, argument types, entity types
- Runtime command lookup instead of compile-time enum
- String-based command identification throughout codebase
- Migration of all `Command` enum usages

**Implementation Approach:**
1. Create `CommandDefinition` class with all metadata
2. Load from `commands.json` at startup
3. Replace enum usages with string-based lookup
4. Maintain backwards compatibility during migration
5. Eventually deprecate Command enum

**This is a significant architectural change and should be planned as a major version milestone.**

---

## Code Improvement Backlog

### Quick Wins (Low Effort)

1. Remove debug code from EntitySet.cs (lines 72-76)
2. Make `_StringExported` static in StringOrIDRef.cs
3. Remove unused usings in Reference.cs
4. Fix HasFlag usage in IDEntity.cs line 113
5. Make throwing methods abstract in IDEntity.cs
6. Remove unnecessary re-sort in IDEntity.RemoveProperty()

### Medium Effort

1. Extract duplicated special token parsing in Mod.cs
2. Consolidate TryGetCopyFrom implementations
3. Add unit test for Command-string mapping coverage

### Larger Refactoring

1. Data-driven entity property maps (replace static constructors)
2. Configuration-based ID ranges (Dom5 vs Dom6)
3. Extract ModMerger class from ModSet

---

## Feature Backlog (Post-Redesign)

### HIGH Priority
- CUSTOMMAGIC editor (complex bitmask)
- Reference editors (WEAPON, ARMOR, COPYSTATS, STARTITEM)
- Reset to original buttons

### MEDIUM Priority
- Shapechange commands
- Summoning commands
- Property tooltips from metadata
- Sprite preview

### LOW Priority
- Recruitment commands
- NAMETYPE, MONTAG editors
- Keyboard shortcuts within views
- Copy/paste properties

---

## Files Reference

### New UI System
```
Dom5Editor/UI/
  Controls/
    CompactBadge.xaml(.cs)
    BadgeWrapPanel.xaml(.cs)
    BadgeItem.cs
    IntPropertyEditor.xaml(.cs)
    StringPropertyEditor.xaml(.cs)
    CommandPropertyEditor.xaml(.cs)
    ReferencePropertyEditor.xaml(.cs)
    MagicPathEditor.xaml(.cs)
  Views/
    MainWindow.xaml(.cs)
    MonsterView.xaml(.cs)
  ViewModels/
    EntityViewModel.cs
    EntityViewModels.cs
  Theme/
    AppTheme.xaml
    AppResources.xaml

Dom5Editor/Data/
  monster_badges.json
  BadgeConfig.cs
  BadgeConfigLoader.cs

Dom5Editor/EditCommands/
  IEditCommand.cs
  CommandHistory.cs
  Set*Command.cs
```

### Core Library
```
Dom5Edit/
  Mod/
    Mod.cs
    ModParser.cs
    ModExporter.cs
    ChangesMod.cs
    ChangesModExporter.cs
    EntityChanges.cs
  Validation/
    IValidator.cs
    ValidationIssue.cs
    *Validator.cs
  Metadata/
    PropertyDefinition.cs
    PropertyMetadata.cs
    MetadataLoader.cs
```
