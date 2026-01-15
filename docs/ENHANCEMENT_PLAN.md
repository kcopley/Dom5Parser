# UI Editor Enhancement Plan

This document outlines the planned enhancements to build out Dom5Parser into a full UI-based .dm file editor.

**Last Updated:** 2026-01-10

## Goal

Create a UI-based editor that:
- Displays all entities (monsters, weapons, spells, etc.) in navigable lists
- Provides editing interfaces for each entity type
- Saves changes back to .dm format cleanly
- **Primary focus: Dominions 6** (Dom5 support maintained where practical)

---

## Current Status

### Work Priorities

1. ~~**JSON-Driven UI** - Badge system with JSON configuration~~ (COMPLETE)
2. ~~**Undo/Redo Infrastructure** - CommandHistory integration~~ (COMPLETE)
3. ~~**ChangesMod Session Tracking** - Track edits for export~~ (COMPLETE)
4. **Other Entity Views** - WeaponView, ArmorView, SpellView, etc.
5. **Feature Expansion** - Entity navigation, sprite preview, validation report panel

### Command Coverage

**MonsterView:** 572 / 560 commands (102% - all reference commands + extras)

| Category | Commands | Status |
|----------|----------|--------|
| Stats | 12 | Complete |
| Magic Paths | 2 | Complete |
| Types | 31 | Complete |
| General | 345 | Complete (movement, recruitment, shapechange, summoning, etc.) |
| Combat | 163 | Complete (abilities, auras, powers) |
| Resistances | 19 | Complete |
| Equipment (WEAPON, ARMOR) | N/A | Complete (with copystats inheritance) |
| Magic (CUSTOMMAGIC) | 1 | Complete (PathToggleButton + CustomMagicEditor controls) |

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

- ~~Sprite Preview - Display monster/item sprites~~ DONE 2026-01-08 (data loading + multi-format display)
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
- Integration with EntityViewModels via `BuildBadgesFromSection()`
- Fixed JSON loading path resolution for .NET 8
- Removed legacy hardcoded command arrays (all commands now driven by JSON)

### 6.4: Entity View Architecture

**MonsterView Sections:**
1. Identity - Name, ID, Sprites, Description
2. Copy From - Displays copystats/copyspr references when present
3. Core Stats - HP, STR, ATT, DEF, etc. (12 commands)
4. Magic Paths - MagicPathEditor control (2 commands)
5. Equipment - Horizontal layout with weapons (left) and armor (right)
   - ID-based selection from vanilla + mod entities
   - Copystats inheritance support with "inh" badge
   - Hyperlink-style names (prepared for navigation)
6. Abilities (badge-based):
   - Types (31 commands - read-only, inherited)
   - General (345 commands - movement, recruitment, shapechange, summoning, special)
   - Combat (163 commands - abilities, auras, powers, battle effects)
   - Resistances (19 commands - colored badges)

### 6.5: Legacy ViewModel Deprecation

| System | Location | Status |
|--------|----------|--------|
| Legacy VMs | `Dom5Editor/VMs/` | DEPRECATED |
| New UI VMs | `Dom5Editor/UI/ViewModels/` | ACTIVE |

**Keep:** `ModViewModel.cs`, `RelayCommand.cs`, `ViewModelBase.cs`
**Remove after migration:** All other legacy VMs and views

### 6.6: JSON-Driven ViewModel Property Migration (COMPLETE)

**Completed 2026-01-07:** Removed hardcoded public properties from all ViewModels and replaced them with fully JSON-driven badge rendering.

**Problem:** EntityViewModels.cs contains 722 public properties across 11 ViewModels, with each stat/flag requiring 4 properties:
```csharp
// OLD: 4 properties per stat = massive boilerplate
public int? Protection { get => GetIntProperty(Command.PROT); set => ... }
public bool IsProtectionModified => IsIntPropertyModifiedFromVanilla(Command.PROT);
public bool IsProtectionSessionEdit => IsPropertyEditedInSession(Command.PROT);
public bool IsProtectionInherited => IsIntPropertyInherited(Command.PROT);
```

**Solution:** Define properties in JSON and render dynamically via badge panels:
```json
{
  "id": "stats",
  "layout": "grid",
  "columns": 3,
  "showDefaults": true,
  "showAddButton": false,
  "commands": [
    { "name": "prot", "display": "PROT", "type": "int", "default": 0, "allowMultiple": false }
  ]
}
```

#### ArmorViewModel Migration (Proof-of-Concept) - COMPLETE

**Completed 2026-01-06:** Migrated ArmorViewModel from hardcoded properties to JSON-driven badges.

**New Control Created:**
- `BadgeGridPanel.xaml(.cs)` - Grid-based badge panel with configurable columns
  - Uses `UniformGrid` for fixed-column layout
  - Badges render in JSON order (left-to-right, top-to-bottom)
  - Same features as `BadgeWrapPanel` (add/remove commands, reference support)

**New JSON Section Options:**
| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `layout` | string | "wrap" | "wrap" or "grid" layout mode |
| `columns` | int | 3 | Number of columns for grid layout |
| `showDefaults` | bool | false | Show all commands using default values even when entity doesn't have the property |
| `showAddButton` | bool | true | Show/hide the add dropdown for this section |

**Armor Stats Section Config:**
```json
{
  "id": "stats",
  "layout": "grid",
  "columns": 3,
  "showDefaults": true,
  "showAddButton": false,
  "commands": [
    { "name": "prot", "display": "PROT", "type": "int", "default": 0, "allowMultiple": false },
    { "name": "def", "display": "DEF", "type": "int", "default": 0, "allowMultiple": false },
    { "name": "enc", "display": "ENC", "type": "int", "default": 0, "allowMultiple": false },
    { "name": "rcost", "display": "RCOST", "type": "int", "default": 0, "allowMultiple": false },
    { "name": "type", "display": "TYPE", "type": "int", "default": 0, "allowMultiple": false }
  ]
}
```

**Properties Removed from ArmorViewModel:**
- `Protection`, `IsProtectionModified`, `IsProtectionSessionEdit`, `IsProtectionInherited`
- `Defense`, `IsDefenseModified`, `IsDefenseSessionEdit`, `IsDefenseInherited`
- `Encumbrance`, `IsEncumbranceModified`, `IsEncumbranceSessionEdit`, `IsEncumbranceInherited`
- `ResourceCost`, `IsResourceCostModified`, `IsResourceCostSessionEdit`, `IsResourceCostInherited`
- `ArmorType`, `IsArmorTypeModified`, `IsArmorTypeSessionEdit`, `IsArmorTypeInherited`

**Properties Retained (Essential):**
- `ArmorTypeDisplay` - Derived property mapping type ID to display name
- `CopyArmorDisplay`, `HasCopyArmor` - Copy reference navigation
- `PropertyBadges`, `AvailablePropertyBadges` - Existing properties section

**UI Improvements:**
1. **Editable TextBox Styling** - Value textboxes now have:
   - Subtle background tint and underline border
   - Highlight on hover (accent color)
   - Highlight on focus (gold border)
2. **Inherited Status Clearing** - When editing a default value:
   - `IsInherited` automatically set to `false`
   - `IsSessionEdit` automatically set to `true`
   - "inh" badge disappears, cyan session indicator appears

**View Update (ArmorView.xaml):**
```xml
<!-- OLD: Manual Grid with IntPropertyEditors -->
<Grid>
  <controls:IntPropertyEditor Label="PROT" Value="{Binding Protection}" .../>
  <!-- 5 more editors with 4 bindings each -->
</Grid>

<!-- NEW: JSON-driven BadgeGridPanel -->
<controls:BadgeGridPanel
    ItemsSource="{Binding StatsBadges}"
    RemoveCommand="{Binding RemoveStatsBadgeCommand}"
    Columns="3"
    ShowAddButton="False"/>
```

#### Migration Status (All Complete - 2026-01-07)

| Priority | ViewModel | Props Removed | Status |
|----------|-----------|---------------|--------|
| 1 | MercenaryViewModel | 27 | ✓ Complete |
| 2 | SiteViewModel | 16 | ✓ Complete |
| 3 | WeaponViewModel | 44 | ✓ Complete |
| 4 | SpellViewModel | 36 | ✓ Complete |
| 5 | NationViewModel | 12 | ✓ Complete |
| 6 | EventViewModel | 4 | ✓ Complete |
| 7 | ItemViewModel | 28 | ✓ Complete |
| 8 | MonsterViewModel | ~550 | ✓ Complete |

**Total: ~700+ properties removed, now JSON-driven via badge system**

### 6.7: Badge Infrastructure Generalization (COMPLETE)

**Completed 2026-01-05:** Refactored badge system to enable reuse across entity types.

**Changes:**
- Moved `BuildBadgesFromSection()` from MonsterViewModel to EntityViewModel base class
- Added `EntityTypeName` virtual property (override to specify JSON config file)
- Added helper methods for creating badge commands and value handlers
- Simplified MonsterViewModel badge code using base class helpers

**Base class methods (EntityViewModel.cs):**
```csharp
// Override this to enable badge support
protected virtual string EntityTypeName => null;  // "monster", "weapon", etc.

// Build badges from JSON config section
protected (ObservableCollection<PropertyItem> active, ObservableCollection<AvailablePropertyItem> available)
    BuildBadgesFromSection(string sectionId, EventHandler<int> valueChangedHandler = null)

// Helper methods for derived classes
protected void AddBadgeProperty(AvailablePropertyItem badge)
protected void RemoveBadgeProperty(PropertyItem badge)
protected EventHandler<int> CreateBadgeValueChangedHandler()
protected RelayCommand<PropertyItem> CreateRemoveBadgeCommand(Action refreshAction)
protected RelayCommand<AvailablePropertyItem> CreateAddBadgeCommand(Action refreshAction)
```

**Impact:** New entity views can now add badge support by:
1. Creating `{entity}_badges.json` in `Dom5Editor/Data/`
2. Adding `protected override string EntityTypeName => "{entity}";`
3. Defining badge collections using `BuildBadgesFromSection()`

### 6.8: Generic Layered Entity Resolution (COMPLETE)

**Completed 2026-01-05:** Added generic methods for layered entity/reference resolution.

**Problem:** Weapon/armor lists weren't displaying for mod-edited entities because the code only looked at the mod entity (which only has changes), not falling back to vanilla data. Each reference type needed its own fallback method.

**Solution:** Added generic resolution methods to `EntityViewModel` base class that automatically cascade through layers: mod → vanilla → copystats chain.

**Base class methods (EntityViewModel.cs):**
```csharp
// Resolve any entity by type and ID (mod → vanilla fallback)
protected IDEntity ResolveEntityReference(EntityType type, int id)

// Get entity name with fallback
protected string GetEntityName(EntityType type, int id)

// Get reference name (tries resolved entity, then layered lookup)
protected string GetReferenceName(StringOrIDRef reference, EntityType entityType)

// Get multi-value references with full layering (vanilla → mod → copystats)
protected List<(int Id, string Name, bool IsInherited, bool IsModified, bool IsSessionEdit)>
    GetLayeredReferenceList<TRef>(Command command, EntityType entityType, Func<TRef, int> getId)
```

**Impact:**
- Weapon/armor code reduced from ~200 lines to ~50 lines
- Any new reference lists (spells, items, etc.) can use `GetLayeredReferenceList<T>()`
- Single place to maintain layering logic
- Consistent behavior across all entity types

---

## Clear Commands Architecture (COMPLETE)

**Implemented:** 2026-01-09

Clear commands (`#clear`, `#clearweapons`, `#cleararmor`, `#clearmagic`, `#clearspec`) block inheritance from vanilla and copystats sources.

### Supported Clear Commands

| Command | Entity | Clears |
|---------|--------|--------|
| `#clear` | Monster | Everything (all property groups) |
| `#clearweapons` | Monster | Weapon slots |
| `#cleararmor` | Monster | Armor slots |
| `#clearmagic` | Monster | Magic paths (magicskill, custommagic, magicboost) |
| `#clearspec` | Monster | Special abilities (everything not stats/identity/sprites) |
| `#cleargods` | Nation | Pretender chassis |
| `#clearsites` | Nation | Starting sites |
| `#clearrec` | Nation | Recruitment lists |
| `#clearnation` | Nation | Nation settings |
| `#cleardef` | Poptype | Province defense |

### Implementation

1. **PropertyGroupMap.cs** - Maps commands to `PropertyGroup` enum values
2. **IDEntity.cs** - `HasClearCommand()`, `IsPropertyGroupCleared()`, integrated into `TryGet<T>()`
3. **Monster.cs / Nation.cs** - Override `GetPropertyGroup()` for entity-specific mappings
4. **MonsterViewModel.cs** - Boolean properties with `SetClearCommand()` helper
5. **MonsterView.xaml** - Checkbox row in Copy From section
6. **EntityViewModel.cs** - Fallback logic checks `IsPropertyGroupCleared()` before inheritance

### Behavior

- Clear commands block inheritance for their property group from BOTH vanilla AND copystats
- Properties explicitly set on the entity are still shown (clears only affect inheritance)
- Cascading copies respect clear commands at each level of the chain
- UI checkboxes toggle clear commands with immediate property refresh

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

1. ~~Remove debug code from EntitySet.cs~~ (done)
2. ~~Make `_StringExported` static in StringOrIDRef.cs~~ (done)
3. ~~Remove unused usings in Reference.cs~~ (done)
4. ~~Fix HasFlag usage in IDEntity.cs~~ (done)
5. Make throwing methods abstract in IDEntity.cs (would require making class abstract)
6. ~~Remove unnecessary re-sort in IDEntity.RemoveProperty()~~ (done)
7. ~~Extract ViewModels to individual files~~ (done 2026-01-07) - EntityViewModels.cs split into 14 files

### Medium Effort

1. Extract duplicated special token parsing in Mod.cs
2. Consolidate TryGetCopyFrom implementations
3. Add unit test for Command-string mapping coverage

### Larger Refactoring

1. Data-driven entity property maps (replace static constructors)
2. Configuration-based ID ranges (Dom5 vs Dom6)
3. Extract ModMerger class from ModSet

### EntityViewModel Refactoring (Pre-UI Polish Priority)

See `docs/ENTITYVIEWMODEL_REFACTORING.md` for full analysis.

**Phase 1: Low Risk, High Impact** ✓ COMPLETE (2026-01-07)
- [x] Consolidate reference caches into dictionary (~35 lines saved)
- [x] Extract generic `GetProperty<T>()` base method
- [x] Add `IsPropertyInherited<T>()` generic method

**Phase 2: Medium Risk** ✓ COMPLETE (2026-01-07)
- [x] Genericize `IsPropertyModifiedFromVanilla<T>()` with value comparator
- [x] Consolidate CanReset methods into single CanResetProperty()
- [x] Eliminate PropertyType enum (was unused)
- [ ] Add `SetProperty<T>()` generic with undo/redo - Deferred (Set methods differ significantly)

**Phase 3: Extend Badge System (Larger Scope)** ✓ COMPLETE (2026-01-07)
- [x] Add `IntIntProperty` badge support (e.g., #gems, #path)
- [x] Add `StringProperty` badge support (e.g., #descr)
- [x] Add `BitmaskProperty` badge support (e.g., #itemslots)
- [ ] Unify reference handling in main badge loop - Deferred (works but could be cleaner)

**Savings achieved:** ~130 lines in EntityViewModel. Badge JSON configs can now use `type: "intint"`, `type: "string"`, and `type: "bitmask"` for these property types.

---

## Feature Backlog (Post-Redesign)

### EXTREMELY HIGH Priority (Blocking)

- ~~**Reference Property Creation Fix**~~ **DONE (2026-01-07)** - See `docs/REFERENCE_PROPERTY_HANDLING.md`:
  - Fixed: Adding multi-value reference properties now uses correct property type
  - Added `AddPropertyFromMap()` method that uses entity's `GetPropertyMap()` to get correct factory
  - Updated `RemoveIntPropertyByValue()` to handle Reference types
  - Changed `GetPropertyMap()` from `internal` to `public` for cross-assembly access

- ~~**Reference Type Badge Support**~~ **DONE (2026-01-06)** - `BuildBadgesFromSection()` now handles `type: "ref"` badges:
  - Added `BuildReferenceBadges()` method for multi-value ref property handling
  - Added `TryExtractRefInfo()` helper that handles both `StringOrIDRef` and `MonsterOrMontagRef` types
  - `MonsterOrMontagRef.MonsterRef` and `.MontagRef` fields made public for cross-assembly access
  - References display with entity names resolved via `GetEntityName()`
  - Supports vanilla + mod layering with proper IsModified/IsInherited flags
  - Copystats chain traversal for inherited references
  - `PropertyItem` extended with `IsReference`, `ReferenceId`, `ReferenceName`, `ReferenceType`, `ReferenceDisplay`
  - `BadgeConfigLoader.CreateReferencePropertyItem()` factory method
  - `CompactBadge` updated to display reference badges with entity names
  - Reference commands always available in Add dropdown (can have multiple instances)
  - Montag references (negative IDs) display as "Montag #X"

### HIGH Priority
- **All Entity Views Complete** - Monster, Weapon, Armor, Item, Site, Spell, Nation, Event, Mercenary, Poptype, Nametype, Bless, Template
- **Mod Info View** - Complete (editing mod name, description, version, dom version, icon)
- **Remaining**: Enchantment, Montag, RestrictedItem are ID-only containers and may not need views

  **NationView Data Limitations:**
  - Vanilla nation data is **incomplete** - no full nation dump exists like for monsters/weapons/armor
  - Only partial information is available from vanilla sources (some recruitment, some pretender chassis)
  - The UI must clearly distinguish between:
    - **Partial vanilla data** - Display with "incomplete" indicator, read-only where data is missing
    - **Mod-defined nations** - Full data available, fully editable
    - **Mod modifications to vanilla** - Show what's being changed, note base data may be incomplete
  - Consider a visual indicator (e.g., warning banner, muted styling) for nations with partial data
  - Reference properties (recruitment lists, pretenders, sites) may show "[Unknown]" for unresolvable vanilla refs

  **View Complexity Pattern Guidelines:**
  - **Simple entities (Weapon, Armor, Item, Site):** Use a single unified badge panel for all boolean/flag properties. Don't split into multiple categorized sections (Elements, Materials, Damage Types, etc.) - this adds UI complexity without benefit. One flat badge collection with a single "Add" dropdown showing all available properties.
  - **Complex entities (Monster, Nation, Spell):** Use categorized sections where logical groupings help navigation (e.g., Monster has Combat, General, Resistances, Types as distinct concepts). Categories should reflect how users think about the entity, not just how commands are organized in the manual.
  - **Reference properties:** Group equipment/reference slots together (weapons, armor, spells) with navigation links to referenced entities. For sites, monster/nation references are handled as badge properties with `type: "ref"` and `refType: "monster"` or `refType: "nation"`.
  - **Stats:** Always use grid layout for numeric stats at the top of the view.

  **DONE (2026-01-06):** SpellView implemented with path requirements display, combat stats (range/precision/aoe/damage/effect/nreff), next spell chain display, fatigue cost with gem breakdown, and unified property badge panel with 40+ spell properties including restrictions, AI hints, and enchantment settings. Uses `spell_badges.json` configuration.
  **DONE (2026-01-06):** SiteView implemented with core stats (path/level/rarity), gems display with icons, copy-from reference, and unified property badge panel with 90+ properties including monster/nation references.
  **DONE (2026-01-05):** WeaponView and ArmorView refactored to use single unified badge panel.
  **DONE (2026-01-05):** ItemView implemented with construction stats, path requirements, and badge panel.
  **DONE (2026-01-05):** ItemView equipment section shows weapon/armor damage types, special properties, and secondary effects.
  **DONE (2026-01-05):** WeaponView displays damage types, special properties, and secondary effect with weapon name and chance.

- ~~**Path Selector (Spells/Items)**~~ DONE - PathSelector control in Controls/:
  - Dropdown for path (None, Fire, Air, Water, Earth, Astral, Death, Nature, Glamour, Blood)
  - Level input (0-9) shown when path selected
  - Color-coded badges matching game path colors
  - Integrated in ItemView for mainpath/secondarypath selection
- **MagicPathEditor Improvements** - Already functional in MonsterView, potential polish:
  - Full path names on hover/expanded view
  - Better visual feedback
- ~~**CUSTOMMAGIC Editor**~~ **DONE (2026-01-07)** - Random magic path editor for mages:
  - PathToggleButton + CustomMagicEditor controls with add/remove functionality
  - All 10 paths supported (F/A/W/E/S/D/N/G/B/H) with correct bitmask values
  - Layered data support (vanilla + mod merge)
- Navigation between entity views (clicking weapon/armor to view that entity)
- Reset to original buttons (per-property revert)
- **Secondary Effect Chain Display** - When a weapon has a secondary effect that itself has a secondary effect, show the full chain
- ~~**WeaponDamage Monster ID Selector**~~ **DONE (2026-01-08)** - DynamicPropertyEditor now handles mode switching:
  - Normal weapons: integer input for damage value
  - Summon weapons (dmg="summonunits"): SearchableReferenceComboBox for monster selection
  - Cloud weapons (dmg="cloud"): read-only text display
  - Control is generic and reusable for other conditional editors
- ~~**ChangesMod Identity Check on Value Reset**~~ **DONE (2026-01-09)** - Automatic removal from session tracking when property is reset to original:
  - `EntityChanges` tracks original property values (before any session edits)
  - `SetPropertyWithOriginal()` compares new values against session originals
  - When values match the original, the change is automatically reverted (removed from tracking)
  - `IPropertyEditCommand.GetOriginalProperty()` exposes the pre-edit property value
  - All edit commands (SetInt, SetString, SetCommand, SetIntInt, Remove, Add) implement identity checking
  - Works for Int, String, Command, and IntInt property types
- ~~**Searchable Dropdowns**~~ **DONE (2026-01-06)** - Reference badge selectors now use SearchableReferenceComboBox:
  - Text filter searches by name or ID
  - Click opens dropdown and selects all text for easy replacement
  - Name displayed in editable area, ID shown separately as `#123`
  - Dropdown items show `Name  #123` with styled ID
  - Navigation arrow (→) to jump to referenced entity
  - Applied to editable reference badges (mod/session properties)
  - Inherited/vanilla references remain read-only with same display format
- **Badge Size Scaling** - Consider upscaling badge sizes throughout the UI:
  - Current badge sizing (9pt text, tight padding) can feel cramped
  - Larger touch targets would improve usability
  - Consider scaling factors: 1.25x for comfortable desktop, 1.5x for touch-friendly
  - Affects: CompactBadge padding/margins, font sizes, button sizes, dropdown heights
  - Could be a user preference setting (compact/comfortable/large)
- ~~**Pre-cache Dropdown Data**~~ **DONE (2026-01-06)** - Centralized entity caches in MainWindowViewModel:
  - 7 cached lists: CachedWeapons, CachedArmors, CachedMonsters, CachedItems, CachedSpells, CachedSites, CachedNations
  - Built at mod load via `BuildEntityCaches()`, before ViewModels are created
  - O(1) HashSet deduplication, shared readonly lists across all ViewModels
- ~~**#clear Commands Support**~~ **DONE (2026-01-09)** - Full clear command implementation:
  - `PropertyGroupMap.cs` maps commands to property groups (Weapons, Armor, Magic, Special, etc.)
  - `IDEntity.HasClearCommand()` and `IsPropertyGroupCleared()` integrated into `TryGet<T>()`
  - Monster/Nation entities override `GetPropertyGroup()` for entity-specific mappings
  - MonsterViewModel: `HasClearAll`, `HasClearWeapons`, `HasClearArmor`, `HasClearMagic`, `HasClearSpec` properties
  - MonsterView: Checkbox row for all 5 clear commands with tooltips
  - EntityViewModel: Fallback logic blocks inheritance when property group is cleared
- **Read-Only Flags in Badge Configs** - Some vanilla properties have no corresponding mod commands (see `docs/DATA_ODDITIES.md`):
  - Weapon flags: `soulslaying`, `ignoreshield`, `defnegate`, `uwonly`, `noillusion`, `magiconly`, etc.
  - Weapon attributes: `flammable`, `nofirebless`
  - Spell attributes: `preventcast`, `requiresench`, `uwsummon`, `coldsummon`
  - Nation attributes: `plainsrec`, `plainscom`, `futuresites`
  - These should be marked `"readOnly": true` in badge JSON configs so they display but cannot be edited
- **Negative Unit IDs as Montag References** - Negative unit IDs in spell/weapon data represent montag groups:
  - -16 = Yazads, -17 = Yatas, -21 = Dwarfs (Four Directions)
  - UI should display montag group name and use montag selector for editing
  - Affects spell summon targets and weapon summon effects
- **Spell Fatigue/Gem Cost Editor** - Costs are encoded as `gems*1000 + fatigue`:
  - Provide separate inputs for gem cost and fatigue cost
  - Auto-encode/decode the combined value
  - Example: 2050 = 2 gems + 50 fatigue
- **Spell Path Editor** - Path requirements for spells:
  - Primary path dropdown (F/A/W/E/S/D/N/G/B) with level input
  - Optional secondary path with level
  - Map to `#school`, `#path`, `#pathlevel`, `#path2`, `#pathlevel2` commands

### MEDIUM Priority
- Weapon/Armor list with stat columns (header row with ID, Name, Atk, Dmg, etc. for weapons; ID, Name, Prot, Def, Enc for armor)
- ~~**Sprite & Description Preview**~~ DONE 2026-01-08:
  - Vanilla assets loaded as properties via `VanillaAssetLoader`
  - Multi-format support: PNG/JPG/BMP via `BitmapImage`, TGA via `TargaImage`
  - Absolute paths (vanilla) and relative paths (mods) both supported
- NAMETYPE, MONTAG editors
- ~~**Validation report panel**~~ DONE 2026-01-11:
  - `ValidationReportWindow.xaml` - Modal dialog with severity filtering
  - Summary header with error/warning/info counts
  - Search box and severity filter checkboxes
  - Issue list with click-to-navigate to entities
- **ItemSlots Bitmask Editor** - Visual editor for monster/commander item slot configuration:
  - Checkboxes/spinners for slot categories (hands 1-6, heads 1-3, body, feet, misc 1-6)
  - Auto-calculate bitmask value from selections
  - Show standard patterns (human mage: 15494, commander: 213046)
  - See `docs/DATA_TRANSFORMATIONS.md` for bitmask values
- **Leadership Selector** - Mutually exclusive dropdown for leader/magicleader/undeadleader:
  - Options: noleader, poorleader, okleader, goodleader, expertleader, superiorleader
  - Only one can be active at a time per leadership type (selecting one removes the previous)
  - Note: There is no `#leader N` command - only the named tier commands are valid
- **Calculated Values Display (Read-Only)** - Show derived values for reference:
  - Total protection (natural + armor with diminishing returns formula)
  - Total encumbrance (base + armor enc adjusted for size)
  - Total resource cost (base + weapon/armor costs scaled by ressize)
  - Commander gold cost breakdown (base + leadership + magic + holy costs)
  - Mark clearly as "calculated" and non-editable
- **Age Value Transforms** - Handle special startage values in UI:
  - Display `startage = -1` as "0" (game interprets -1 as "start at age 0")
  - `startage = 0` means "no start age" - may need special handling or omission
- **Filter Hidden Entities** - Skip display of special entities in entity lists:
  - Entities named "Empty" (placeholder entries)
  - Decimal ID entities (e.g., 100.01) - these are virtual shapechange form displays

### LOW Priority
- Keyboard shortcuts within views
- Copy/paste properties
- Mod comparison/diff tool

### Event Command Extraction
- **Parse Event Mod Manual** - Extract event-specific commands from the modding manual PDF
  - Use `tools/pdf_extractor.py` or similar to extract event command definitions
  - Build comprehensive `event_commands.json` with all event requirements and effects
  - Cross-reference with existing `event_badges.json` to identify missing commands
  - Goal: Complete coverage of all event scripting commands

### Completed
- ~~Reference editors (WEAPON, ARMOR, COPYSTATS)~~ - Equipment section complete
- ~~Shapechange commands~~ - In general section
- ~~Summoning commands~~ - In general section
- ~~Property tooltips from metadata~~ - All commands have descriptions
- ~~Recruitment commands~~ - In general section

---

## Files Reference

### New UI System
```
Dom5Editor/UI/
  Controls/
    CompactBadge.xaml(.cs)
    BadgeWrapPanel.xaml(.cs)
    BadgeGridPanel.xaml(.cs)    - Grid-based badge panel (for stats sections)
    BadgeItem.cs
    IntPropertyEditor.xaml(.cs)
    StringPropertyEditor.xaml(.cs)
    CommandPropertyEditor.xaml(.cs)
    ReferencePropertyEditor.xaml(.cs)
    MagicPathEditor.xaml(.cs)
    PathSelector.xaml(.cs)
    PathToggleButton.xaml(.cs)
    CustomMagicEditor.xaml(.cs)
    SearchableReferenceComboBox.xaml(.cs)
  Views/
    MainWindow.xaml(.cs)
    MonsterView.xaml(.cs)
    WeaponView.xaml(.cs)
    ArmorView.xaml(.cs)
    ItemView.xaml(.cs)
    SiteView.xaml(.cs)
    SpellView.xaml(.cs)
    NationView.xaml(.cs)
    EventView.xaml(.cs)
    MercenaryView.xaml(.cs)
    PoptypeView.xaml(.cs)
    NametypeView.xaml(.cs)
    BlessView.xaml(.cs)        - Bless path costs and scale requirements
    TemplateView.xaml(.cs)     - AI pretender templates
    ModInfoView.xaml(.cs)      - Mod metadata editing
  ViewModels/                  - Extracted to individual files (2026-01-07)
    EntityViewModel.cs         - Base class (1960 lines) - see ENTITYVIEWMODEL_REFACTORING.md
    MonsterViewModel.cs        - Monster editing (~1800 lines)
    ItemViewModel.cs           - Item editing
    EventViewModel.cs          - Event editing
    NationViewModel.cs         - Nation editing
    WeaponViewModel.cs         - Weapon editing
    SpellViewModel.cs          - Spell editing
    MercenaryViewModel.cs      - Mercenary editing
    SiteViewModel.cs           - Site editing
    ArmorViewModel.cs          - Armor editing
    PoptypeViewModel.cs        - Poptype editing
    NametypeViewModel.cs       - Nametype editing
    BlessViewModel.cs          - Bless editing
    TemplateViewModel.cs       - AI template editing
    CustomMagicItem.cs         - CUSTOMMAGIC bitmask handling
    EntityHelperModels.cs      - EquipmentItem, AvailableEquipmentItem, SlotTypeOption
  Theme/
    AppTheme.xaml
    AppResources.xaml

Dom5Editor/Data/
  monster_badges.json     - Monster property definitions (572 commands)
  weapon_badges.json      - Weapon property definitions
  armor_badges.json       - Armor property definitions
  item_badges.json        - Item property definitions
  site_badges.json        - Site property definitions (90+ commands, monster/nation refs)
  bless_badges.json       - Bless property definitions (19 commands)
  template_badges.json    - Template property definitions (8 commands)
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
