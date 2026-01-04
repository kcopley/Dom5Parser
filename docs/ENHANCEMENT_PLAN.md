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

### 2.1: Vanilla Data Migration (COMPLETE)

**Goal:** Switch from TSV spreadsheet loading to vanilla.dm file parsing for Dom6.

**Implementation:**
- Added `GameVersion` enum (Dom5, Dom6) to select data source
- Added `IsVanilla` property to `Entity` and `DependentEntity` classes
- VanillaLoader now supports both:
  - **Dom6**: Loads from `vanilla.dm` file using existing `Mod.Parse()`
  - **Dom5**: Falls back to TSV spreadsheet loading (legacy)
- Entities loaded from vanilla.dm are marked `IsVanilla = true`
- `VanillaLoader.VanillaDmPath` can be set to specify custom vanilla.dm location
- `VanillaLoader.Reload()` forces reload after changing settings

**Usage:**
```csharp
// Default: Dom6 mode, loads from vanilla.dm
var vanilla = VanillaLoader.Vanilla;

// Switch to Dom5 mode (TSV spreadsheets)
VanillaLoader.GameVersion = GameVersion.Dom5;
VanillaLoader.Reload();

// Custom vanilla.dm path
VanillaLoader.VanillaDmPath = @"C:\Games\Dominions6\vanilla.dm";
VanillaLoader.Reload();
```

**Remaining (optional):**
- Remove or deprecate TSV loading code once Dom5 support is no longer needed
- Update ID ranges in Mod.cs for Dom6 (currently uses Dom5 ranges)

### 2.3: Dynamic Spell Effect Loading (COMPLETE)

**Goal:** Replace hardcoded Dom5 spell effect data with dynamically loaded JSON data.

**Implementation:**
- Created `SpellEffectData.cs` singleton to load spell effect mappings at runtime
- Loads from `spell_effects_mapping.json` (spell ID → effect number)
- Loads from `spell_effect_types.json` (effect classification: summon, enchant, bitmask)
- `VanillaSpellMap` now delegates to `SpellEffectData` when loaded
- Added `VanillaLoader.SpellEffectMappingPath` and `SpellEffectTypesPath` properties
- Proper cycle detection in `Spell.IsSummon()`, `IsEnchant()`, `IsEventEffect()`
- Added many missing Dom6 commands (see RECENTLY_ADDED_COMMANDS.md)

**Result:** Vanilla.dm warnings reduced from 110 to 0; DomEnhanced warnings from 695 to 284 (58% reduction). Remaining warnings are mod author issues (typos, missing entities) not parser issues.

### 2.2: Split Mod Class Responsibilities (COMPLETE)

Split the ~960 line Mod class into focused components:

**ModParser** (COMPLETE - `Dom5Edit/Mod/ModParser.cs`)
- Line-by-line parsing logic with callback-based command emission
- Command recognition via `CommandsMap`
- Multi-line string handling for descriptions
- Comment preservation
- Placeholder skipping (##godname##, etc.)
- Static `ScanDependencies()` for quick dependency detection

**ModExporter** (COMPLETE - `Dom5Edit/Mod/ModExporter.cs`)
- Export formatting for mod header and entities
- Stream-based export
- `ExportForNations()` for filtered nation exports

**ModDocument** (DEFERRED)
- Entity storage remains in Mod class for now
- Will extract when implementing change tracking for dirty state
- Not needed until Phase 3 (Undo/Redo, Dirty Tracking)

**Mod (Facade)** (COMPLETE)
- Delegates parsing to `ModParser` via callbacks
- Delegates export to `ModExporter`
- Entity management remains in Mod (tightly coupled, works well)
- Legacy methods (`HasCommandOnLine`, `ProcessLine`, etc.) marked `[Obsolete]`, delegate to parser
- Removed dead nation association code (~80 lines)
- **Result: Mod.cs reduced from ~960 to 550 lines (43% smaller)**

## Phase 3: Editor Infrastructure (COMPLETE)

**Design Philosophy:** All current UI infrastructure (ViewModels, Views, patterns) is replaceable and can be deprecated or redesigned. If existing code is useful, reuse it; otherwise, feel free to redesign from scratch. The goal is a clean, maintainable architecture - not preserving legacy patterns.

### Current Architecture Analysis

**Model Layer (Dom5Edit):**
- `Entity`, `IDEntity`, `Property` classes are plain C# objects
- No `INotifyPropertyChanged` - model is unaware of UI
- Mutable operations: `AddProperty()`, `RemoveProperty()`, `Set<T>()` modify state directly

**ViewModel Layer (Dom5Editor):**
- `ViewModelBase` implements `INotifyPropertyChanged`
- ViewModels call `Source.Set<T>(...)` then manually call `OnPropertyChanged()`
- Pattern works but provides no undo/redo or dirty tracking

### Design Decision: Command Pattern Approach

Keep model layer clean (no WPF dependencies). All modifications go through commands that:
1. Execute changes on the model
2. Fire change notifications
3. Enable undo/redo via command stack
4. Enable dirty tracking (uncommitted commands = dirty)

### 3.1: Edit Command Infrastructure

**Location:** `Dom5Editor/EditCommands/` (separate from `Dom5Edit/Commands/` which contains mod commands)

```csharp
// Base interface for all edit commands
public interface IEditCommand
{
    string Description { get; }
    void Execute();
    void Undo();
}

// Manages undo/redo stacks per document
public class CommandHistory
{
    private Stack<IEditCommand> _undoStack = new();
    private Stack<IEditCommand> _redoStack = new();

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;
    public bool IsDirty => _undoStack.Count > 0;

    public event Action OnChange;

    public void Execute(IEditCommand command)
    {
        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear();
        OnChange?.Invoke();
    }

    public void Undo()
    {
        if (CanUndo)
        {
            var cmd = _undoStack.Pop();
            cmd.Undo();
            _redoStack.Push(cmd);
            OnChange?.Invoke();
        }
    }

    public void Redo()
    {
        if (CanRedo)
        {
            var cmd = _redoStack.Pop();
            cmd.Execute();
            _undoStack.Push(cmd);
            OnChange?.Invoke();
        }
    }

    public void MarkSaved() => _savePoint = _undoStack.Count;
    public bool IsDirty => _undoStack.Count != _savePoint;
}
```

### 3.2: Property Edit Commands

```csharp
// Generic command for modifying a property value
public class SetPropertyCommand<T> : IEditCommand where T : Property, new()
{
    private readonly IDEntity _entity;
    private readonly Command _command;
    private readonly Action<T> _setter;
    private readonly T _oldValue;
    private readonly T _newValue;

    public string Description => $"Set {_command}";

    public SetPropertyCommand(IDEntity entity, Command command, Action<T> setter)
    {
        _entity = entity;
        _command = command;
        _setter = setter;
        // Capture old value before modification
        _entity.TryGet<T>(command, out _oldValue);
    }

    public void Execute()
    {
        _entity.Set<T>(_command, _setter);
    }

    public void Undo()
    {
        if (_oldValue == null)
            _entity.Remove<T>(_command);
        else
            _entity.Set<T>(_command, p => /* restore old value */);
    }
}

// Command for adding a property (e.g., adding a weapon to a monster)
public class AddPropertyCommand : IEditCommand
{
    private readonly IDEntity _entity;
    private readonly Property _property;

    public void Execute() => _entity.AddProperty(_property);
    public void Undo() => _entity.RemoveProperty(_property);
}

// Command for removing a property
public class RemovePropertyCommand : IEditCommand
{
    private readonly IDEntity _entity;
    private readonly Property _property;
    private int _index;  // For restoring at correct position

    public void Execute()
    {
        _index = _entity.Properties.ToList().IndexOf(_property);
        _entity.RemoveProperty(_property);
    }

    public void Undo() => _entity.AddProperty(_property);  // Will sort
}
```

### 3.3: ViewModel Integration

Modify ViewModels to use commands instead of direct modification:

```csharp
// Current pattern (MonsterViewModel):
public string Value
{
    set
    {
        Source.Set<IntProperty>(Command, i => i.Value = ret);
        OnPropertyChanged("Value");
    }
}

// New pattern with commands:
public string Value
{
    set
    {
        var cmd = new SetPropertyCommand<IntProperty>(
            Source, Command, i => i.Value = ret);
        _commandHistory.Execute(cmd);
        OnPropertyChanged("Value");
    }
}
```

### 3.4: Change Notification Strategy

Use events from `CommandHistory` to trigger UI updates:

```csharp
public class ModViewModel : ViewModelBase
{
    public CommandHistory History { get; } = new();

    public ModViewModel()
    {
        History.OnChange += () =>
        {
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
            OnPropertyChanged(nameof(IsDirty));
        };
    }

    public bool CanUndo => History.CanUndo;
    public bool CanRedo => History.CanRedo;
    public bool IsDirty => History.IsDirty;

    public ICommand UndoCommand => new RelayCommand(() => History.Undo());
    public ICommand RedoCommand => new RelayCommand(() => History.Redo());
}
```

### 3.5: Validation System

**Location:** `Dom5Edit/Validation/`

```csharp
public interface IValidator
{
    IEnumerable<ValidationIssue> Validate(Mod mod);
}

public class ValidationIssue
{
    public ValidationSeverity Severity { get; set; }  // Error, Warning, Info
    public string Message { get; set; }
    public IDEntity Entity { get; set; }
    public Property Property { get; set; }
    public int? LineNumber { get; set; }
}

// Specific validators
public class ReferenceValidator : IValidator { }      // Check referenced entities exist
public class IdRangeValidator : IValidator { }        // Check IDs within allowed ranges
public class DuplicateIdValidator : IValidator { }    // Check for duplicate IDs
public class RequiredPropertyValidator : IValidator { } // Check required properties set

// Composite validator
public class ModValidator
{
    private readonly List<IValidator> _validators = new()
    {
        new ReferenceValidator(),
        new IdRangeValidator(),
        new DuplicateIdValidator()
    };

    public IEnumerable<ValidationIssue> Validate(Mod mod)
    {
        return _validators.SelectMany(v => v.Validate(mod));
    }
}
```

### Implementation Order

1. **CommandHistory class** - Core undo/redo infrastructure
2. **Base edit commands** - SetPropertyCommand, AddPropertyCommand, RemovePropertyCommand
3. **Wire into one ViewModel** - Test with IntPropertyViewModel as proof of concept
4. **Validation framework** - Add validators one at a time
5. **Dirty tracking** - Already handled by CommandHistory.IsDirty
6. **Expand to all ViewModels** - Apply pattern across the editor

### Implementation Status

**Completed - Command Infrastructure:**
- `Dom5Editor/EditCommands/IEditCommand.cs` - Base interface for edit commands
- `Dom5Editor/EditCommands/CommandHistory.cs` - Undo/redo stack with dirty tracking
- `Dom5Editor/EditCommands/SetIntPropertyCommand.cs` - Command for setting integer properties
- `Dom5Editor/EditCommands/SetStringPropertyCommand.cs` - Command for setting string properties
- `Dom5Editor/EditCommands/SetIntIntPropertyCommand.cs` - Command for setting two-integer properties
- `Dom5Editor/EditCommands/SetCommandPropertyCommand.cs` - Command for boolean flag properties
- `Dom5Editor/EditCommands/SetNameCommand.cs` - Command for entity names
- `Dom5Editor/EditCommands/SetReferenceCommand.cs` - Command for reference properties (weapon, armor, monster, etc.)
- `Dom5Editor/EditCommands/AddPropertyCommand.cs` - Command for adding properties
- `Dom5Editor/EditCommands/RemovePropertyCommand.cs` - Command for removing properties

**Completed - ViewModel Integration:**
- `ModViewModel` - Added History, IsDirty, CanUndo/CanRedo, UndoCommand/RedoCommand
- `PropertyViewModel` - Added optional CommandHistory property
- `IntPropertyViewModel` - Uses commands when History is available
- `IntIntPropertyViewModel` - Uses commands when History is available
- `StringViewModel` - Uses commands when History is available
- `CommandViewModel` - Uses commands when History is available
- `NameViewModel` - Uses commands when History is available
- `DescriptionViewModel` - Uses commands when History is available
- `WeaponRefViewModel` - Uses commands when History is available
- `ArmorRefViewModel` - Uses commands when History is available
- `MonsterRefViewModel` - Uses commands when History is available
- `CopyStatsRefViewModel` - Uses commands when History is available
- `StringOrIDRefViewModel` - Uses commands when History is available
- `IDViewModelBase` - AddProperty/RemoveProperty now use commands
- `MonsterViewModel` - AddWeapon/AddArmor/RemoveWeapon/RemoveArmor use commands

**Completed - Validation Framework:**
- `Dom5Edit/Validation/IValidator.cs` - Validator interface
- `Dom5Edit/Validation/ValidationIssue.cs` - Issue data class with severity levels
- `Dom5Edit/Validation/ReferenceValidator.cs` - Validates entity references exist
- `Dom5Edit/Validation/IdRangeValidator.cs` - Validates IDs within allowed ranges
- `Dom5Edit/Validation/DuplicateIdValidator.cs` - Detects duplicate IDs
- `Dom5Edit/Validation/ModValidator.cs` - Composite validator with summary results

**Completed UI:**
- Keyboard shortcuts (Ctrl+Z, Ctrl+Y) added to ModView
- Undo/Redo buttons in toolbar with tooltips
- Dirty indicator (*) in toolbar when unsaved changes exist

**Next Steps:**
- Integrate validation into UI (validation panel, error highlighting)

### 3.6: Change Tracking (ChangesMod) - COMPLETE

**Goal:** Implement the select-on-edit architecture described in the design considerations below.

**Implementation:**
- `Dom5Edit/Mod/EntityChanges.cs` - Tracks property-level changes for a single entity
- `Dom5Edit/Mod/ChangesMod.cs` - Tracks all changes during an editing session
- `Dom5Edit/Mod/ChangesModExporter.cs` - Exports changes standalone or merged with loaded mod

**Key Classes:**

```csharp
// Tracks changes to a single entity
public class EntityChanges
{
    public int EntityId { get; set; }
    public EntityType EntityType { get; set; }
    public bool IsVanillaOverride { get; set; }
    public Dictionary<Command, Property> ChangedProperties { get; }
    public HashSet<Command> RemovedProperties { get; }  // Only for mod entities
}

// Tracks all changes during editing session
public class ChangesMod
{
    public Mod LoadedMod { get; set; }  // Null if editing vanilla only
    public bool HasChanges { get; }

    void RecordPropertyChange(IDEntity entity, Property property);
    bool RecordPropertyRemoval(IDEntity entity, Command command);
    void AddNewEntity(IDEntity entity);
    bool RemoveEntity(IDEntity entity);  // Only for mod entities
}
```

**Integration with CommandHistory:**
- CommandHistory now has `ChangesMod` property
- When executing `IPropertyEditCommand`, automatically records to ChangesMod
- When undoing, reverts the change from ChangesMod
- ModViewModel initializes ChangesMod and wires it to CommandHistory

**Export Modes:**
1. **No mod loaded** → Exports ChangesMod only (vanilla overrides + new entities)
2. **Mod loaded** → Exports merged result (loaded mod + changes - removals)

**Three-Layer Model:**
- Vanilla (read-only) - Can only be overridden via #select* blocks
- LoadedMod (editable) - Can add, modify, or remove entities/properties
- ChangesMod (changes layer) - Tracks deltas from vanilla/mod

**Files Created:**
```
Dom5Edit/
  Mod/
    EntityChanges.cs      # [DONE] Per-entity change tracking
    ChangesMod.cs         # [DONE] Session-wide change tracking
    ChangesModExporter.cs # [DONE] Export logic for changes

Dom5Editor/
  EditCommands/
    IEditCommand.cs       # [UPDATED] Added IPropertyEditCommand interface
    CommandHistory.cs     # [UPDATED] Added ChangesMod integration
```

### Design Consideration: Change Tracking and Export (IMPLEMENTED)

**Problem:** When editing vanilla entities, how do we export only the changes?

**Options:**
1. **Diff-based export**: Track all commands executed, export only entities that were modified
2. **Clone-on-edit**: When first modifying a vanilla entity, create a copy in a "user mod" container. All edits apply to the copy. Export only exports user mod contents.
3. **Hybrid**: Clone on edit, but also track original state for "revert to vanilla" functionality

**Solution Implemented:** Option 2 (Select-on-edit) using .dm mod semantics via `ChangesMod`:
- Entities marked `IsVanilla = true` are never directly modified
- First edit on vanilla entity creates an empty "override" entity with just ID
- Only changed properties are stored in the override entity
- Export produces `#selectmonster <id>` + only the changed properties
- **Important limitation**: Cannot "remove" vanilla properties, only override or add

**Implementation (now in ChangesMod):**
```csharp
// CommandHistory.Execute() automatically records to ChangesMod:
if (command is IPropertyEditCommand propCommand)
{
    var entity = propCommand.Entity;
    if (propCommand.IsRemoval)
        ChangesMod.RecordPropertyRemoval(entity, propCommand.PropertyCommand);
    else
        ChangesMod.RecordPropertyChange(entity, propCommand.GetResultingProperty());
}

// Export produces (via ChangesModExporter):
// #selectmonster 1234
// #hp 150           <-- only the changed property
// #end
```

**UI considerations (for future implementation):**
- Show vanilla values as read-only baseline (grayed out)
- Show override values as editable (normal color)
- "Revert" button removes property from override (restores vanilla value)
- Properties that don't exist in vanilla can be added normally

### Files Created

```
Dom5Editor/
  EditCommands/               # Renamed from Commands/ to avoid confusion with Dom5Edit/Commands/
    IEditCommand.cs           # [DONE] Base interface + IPropertyEditCommand for ChangesMod
    CommandHistory.cs         # [DONE] Undo/redo stack manager with ChangesMod integration
    SetIntPropertyCommand.cs  # [DONE] Integer property command
    SetStringPropertyCommand.cs # [DONE] String property command
    SetIntIntPropertyCommand.cs # [DONE] Two-integer property command
    SetCommandPropertyCommand.cs # [DONE] Boolean flag command
    SetNameCommand.cs         # [DONE] Entity name command
    SetReferenceCommand.cs    # [DONE] Reference property command
    AddPropertyCommand.cs     # [DONE] Add property command
    RemovePropertyCommand.cs  # [DONE] Remove property command

Dom5Edit/
  Mod/
    EntityChanges.cs          # [DONE] Per-entity change tracking
    ChangesMod.cs             # [DONE] Session-wide change tracking
    ChangesModExporter.cs     # [DONE] Export logic for changes
  Validation/
    IValidator.cs             # [DONE] Validator interface
    ValidationIssue.cs        # [DONE] Issue data class
    ReferenceValidator.cs     # [DONE] Validates entity references
    IdRangeValidator.cs       # [DONE] Validates ID ranges
    DuplicateIdValidator.cs   # [DONE] Detects duplicate IDs
    ModValidator.cs           # [DONE] Composite validator
```

## Phase 4: UI Components

### Command Metadata from JSON
- Load all command display names and descriptions from JSON metadata file
- Display names used in UI labels (e.g., "Fire Resistance" instead of "FIRERES")
- Descriptions used for mouseover tooltips with detailed info
- Categories defined in JSON for grouping in UI
- Validation ranges (min/max values) stored in metadata
- Path: `Dom5Edit/CommandMetadata.json` or similar

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

## Phase 6: Property Editor System (IN PROGRESS)

Building reusable, themed property editor controls with modification tracking and visual feedback.

### 6.1: Theme System (COMPLETE)

**Location:** `Dom5Editor/UI/Theme/`

Created a centralized theming system for consistent dark-themed UI:

**Files:**
- `AppTheme.xaml` - Global color definitions and brush resources
- `AppResources.xaml` - Control styles (TextBox, Button, ComboBox, etc.)
- `Converters.xaml` - Common value converters

**Key Brushes:**
- `BackgroundBrush`, `BackgroundDarkBrush`, `BackgroundLightBrush` - Background colors
- `TextPrimaryBrush`, `TextSecondaryBrush`, `TextDisabledBrush` - Text colors
- `AccentBrush`, `AccentHighlightBrush` - Accent colors (teal theme)
- `AccentSecondaryBrush` - Secondary accent (purple)
- `ErrorBrush` - Error state color (red)
- `SessionEditBrush` - Cyan indicator for session edits
- `ModifiedFromModBrush` - Gold indicator for mod-modified values
- `ModifiedFromVanillaBrush` - Orange indicator for vanilla-modified values

### 6.2: Property Editor Controls (COMPLETE)

**Location:** `Dom5Editor/UI/Controls/`

Reusable controls for editing different property types:

#### IntPropertyEditor
- Label + TextBox for integer values
- Compact layout with 10px fonts
- Visual indicators for modification state (gold bar for modified, cyan dot for session edits)
- Inherited value badge ("inh") for copystats inheritance
- Supports tooltips from property metadata

#### StringPropertyEditor
- Label + TextBox for string values
- Same modification indicators as IntPropertyEditor

#### CommandPropertyEditor
- Toggle button for boolean flag properties (Command properties with no value)
- Shows as colored badge when enabled

#### ReferencePropertyEditor
- ComboBox for selecting referenced entities (weapons, armor, monsters)
- Shows entity name and ID in dropdown
- Auto-complete search support

#### CommandListEditor
- For lists of flag commands (type commands, movement commands, etc.)
- Shows active commands as removable colored badges
- Add dropdown with available commands
- Mouse wheel scroll fix (scroll while hovering doesn't add items)
- Auto-add on combobox selection with dispatcher delay to prevent scroll issues

#### IntPropertyListEditor
- For lists of int/int properties (resistances, combat modifiers, auras)
- Shows active properties with editable values
- Similar UX to CommandListEditor

#### MagicPathEditor (NEW)
- Specialized control for editing magic paths (MAGICSKILL command)
- Colored badges for each path (F, A, W, E, S, D, N, G, B, H)
- Path colors match Dominions game colors
- Level editor within each badge
- Inherited indicator for copystats inheritance
- Add buttons for each available path

**Magic Path Color Definitions:**
```csharp
(0, "F", "Fire",    RGB(220, 60, 40),  White)
(1, "A", "Air",     RGB(180,180,240),  Black)
(2, "W", "Water",   RGB(60, 120,200),  White)
(3, "E", "Earth",   RGB(139, 90, 43),  White)
(4, "S", "Astral",  RGB(255,215,  0),  Black)
(5, "D", "Death",   RGB(50,  50, 50),  White)
(6, "N", "Nature",  RGB(60, 160, 60),  White)
(7, "G", "Glamour", RGB(180,100,180),  White)
(8, "B", "Blood",   RGB(140, 20, 20),  White)
(9, "H", "Holy",    RGB(255,255,200),  Black)
```

### 6.3: Entity View Architecture (IN PROGRESS)

**Pattern:** MonsterView serves as the template for all entity views.

**Structure:**
```
Dom5Editor/UI/
  Views/
    MainWindow.xaml/.cs         - Main application window
    MainWindowViewModel.cs      - Main window state/logic
    MonsterView.xaml/.cs        - Template entity view (REFERENCE)
  ViewModels/
    EntityViewModel.cs          - Base class for entity ViewModels
    EntityViewModels.cs         - Concrete ViewModels (MonsterViewModel, etc.)
  Controls/
    [Property editors listed above]
```

**MonsterView Sections (Template):**
1. **Identity** - Name, ID, Sprites, Description
2. **Core Stats** - HP, STR, ATT, DEF, PREC, MR, MOR, etc.
3. **Movement** - Mapmove, flying, swimming, etc.
4. **Combat** - AWE, FEAR, BERSERK, etc. with IntPropertyListEditor
5. **Resistances** - Fire, Cold, Shock, etc. with IntPropertyListEditor
6. **Equipment** - Weapon slots, Armor with ReferencePropertyEditor
7. **Magic Paths** - MagicPathEditor control
8. **Type Commands** - CommandListEditor for unit type flags
9. **Leader Commands** - CommandListEditor for leadership abilities
10. **Special Commands** - CommandListEditor for misc abilities

### 6.4: MonsterViewModel Magic Path Support (COMPLETE)

**Implementation:**
- `MagicPathsList` - ObservableCollection of MagicPathItem for display
- `AvailableMagicPaths` - List of paths available to add
- `RefreshMagicPaths()` - Rebuilds path list from entity properties
- `AddMagicPath(pathId, level)` - Creates IntIntProperty for MAGICSKILL
- `RemoveMagicPath(pathId)` - Removes MAGICSKILL property
- `OnMagicPathLevelChanged()` - Updates existing property value
- `IsIntIntPropertyModifiedFromVanilla()` - Checks if property differs from vanilla

**Modification Tracking:**
- Compares current entity properties against vanilla entity
- Gold indicator for values modified from vanilla
- Cyan indicator for values added in current session
- "inh" badge for values inherited via copystats

### 6.5: Remaining Work - MonsterView Polish

Before templating to other entity views:

**High Priority:**
1. [ ] CUSTOMMAGIC support - Complex bitmask for random magic paths
2. [ ] Session edit tracking for magic paths (currently no cyan dot on add)
3. [ ] Reset to vanilla button for individual properties
4. [ ] Reset to vanilla button for entire entity
5. [ ] Validate magic path levels (0-10 range typically)

**Medium Priority:**
6. [ ] Sprite image display in Identity section
7. [ ] Weapon/Armor slot indicators (what slots are filled)
8. [ ] Property metadata integration (tooltips from JSON)
9. [ ] Collapsible sections for space management
10. [ ] Search/filter within property lists

**Low Priority:**
11. [ ] Keyboard shortcuts within view (Tab navigation, Enter to commit)
12. [ ] Drag-drop reordering for equipment
13. [ ] Copy/paste properties between entities

### 6.6: Template Other Entity Views

Once MonsterView is polished, create views for:

| Entity    | Priority | Complexity | Notes |
|-----------|----------|------------|-------|
| Weapon    | High     | Medium     | Similar to Monster, fewer sections |
| Armor     | High     | Low        | Simple property editor |
| Item      | High     | Medium     | Magic item commands |
| Spell     | High     | High       | Many effect types, path requirements |
| Site      | Medium   | Medium     | Gem income, recruitment |
| Nation    | Medium   | High       | Many sub-entities (recruitables, sites) |
| Event     | Low      | High       | Complex triggers and effects |
| Mercenary | Low      | Medium     | Recruitment conditions |
| Nametype  | Low      | Low        | String lists |
| Poptype   | Low      | Low        | Simple editor |

**Templating Strategy:**
1. Create base `EntityView` UserControl with common layout
2. Entity-specific views inherit layout, add custom sections
3. ViewModels share `EntityViewModelBase` for common operations
4. Property editors are fully reusable across all entity types

### 6.7: Known UI Issues

**Combobox Scroll Behavior:**
- FIXED: Scroll wheel while hovering over closed ComboBox was adding items
- Solution: PreviewMouseWheel handler checks IsDropDownOpen

**Magic Path Editor Bindings:**
- FIXED: Nothing displaying in MagicPathEditor
- Solution: Don't set `DataContext = this` in UserControl, use `ElementName=Root` for command bindings

**WPF UserControl Binding Pattern:**
```csharp
// WRONG - breaks parent DataContext inheritance:
public MyControl() {
    InitializeComponent();
    DataContext = this;  // DON'T DO THIS
}

// RIGHT - use ElementName or RelativeSource:
<Button Command="{Binding MyCommand, ElementName=Root}"/>
```
