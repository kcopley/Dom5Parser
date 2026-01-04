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

## Phase 3: Editor Infrastructure

Assume that all current UI infrastructure is replaceable and can be deprecated. If it's useful, good, but nothing is required to be retained.

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

**Location:** `Dom5Editor/Commands/` (separate from `Dom5Edit/Commands/`)

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

### Files to Create

```
Dom5Editor/
  Commands/
    IEditCommand.cs           # Interface
    CommandHistory.cs         # Undo/redo stack manager
    SetPropertyCommand.cs     # Property modification command
    AddPropertyCommand.cs     # Add property command
    RemovePropertyCommand.cs  # Remove property command

Dom5Edit/
  Validation/
    IValidator.cs             # Validator interface
    ValidationIssue.cs        # Issue data class
    ReferenceValidator.cs     # Validates entity references
    IdRangeValidator.cs       # Validates ID ranges
    DuplicateIdValidator.cs   # Detects duplicate IDs
    ModValidator.cs           # Composite validator
```

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
