# Known Issues

Potential bugs and critical problems identified during code review.

**Last Updated:** 2026-01-05

---

## Current Priority Issues

### ~~0. Weapon/Armor Not Displaying for Mod-Edited Entities~~ FIXED (2026-01-05)

**Root Cause:** For `VanillaModified` entities (vanilla entities edited by a mod), the ViewModel was using the mod's entity which only contains the **changes** made by the mod - not the full entity data. Weapons/armor remained on the vanilla entity but weren't being fetched.

**Fix:** Added generic layered resolution methods to `EntityViewModel` base class:
- `ResolveEntityReference(type, id)` - Cascades: mod → vanilla
- `GetEntityName(type, id)` - Name lookup with fallback
- `GetReferenceName(reference, entityType)` - Reference name with fallback
- `GetLayeredReferenceList<TRef>()` - Multi-value properties with full layering

The weapon/armor lists now use `GetLayeredReferenceList<WeaponRef>()` and `GetLayeredReferenceList<ArmorRef>()`, reducing ~200 lines of duplicated code to ~50 lines.

**Files Changed:**
- `Dom5Editor/UI/ViewModels/EntityViewModel.cs` - Added generic resolution methods
- `Dom5Editor/UI/ViewModels/EntityViewModels.cs` - Simplified to use base class methods

### 1. Two Parallel ViewModel Systems (Migration Needed)

**Files:** `Dom5Editor/VMs/` vs `Dom5Editor/UI/ViewModels/`

The codebase has two separate ViewModel systems:
- **Legacy VMs** (`Dom5Editor/VMs/`) - DEPRECATED, to be removed
- **New UI VMs** (`Dom5Editor/UI/ViewModels/`) - Active development

**Migration Status:**
The new UI system is working properly. Legacy VMs should be deprecated once all functionality is migrated.

**Files to deprecate after migration:**
- `Dom5Editor/VMs/IntPropertyViewModel.cs`
- `Dom5Editor/VMs/StringViewModel.cs`
- `Dom5Editor/VMs/CommandViewModel.cs`
- `Dom5Editor/VMs/DescriptionViewModel.cs`
- `Dom5Editor/VMs/NameViewModel.cs`
- `Dom5Editor/VMs/IntIntPropertyViewModel.cs`
- `Dom5Editor/VMs/StringOrIDRefViewModel.cs`
- `Dom5Editor/VMs/RefVMs/*` (weapon, armor, monster, etc.)
- `Dom5Editor/VMs/EntityVMs/*`

**Files to keep (infrastructure):**
- `Dom5Editor/VMs/ModViewModel.cs` - Mod-level state, history management
- `Dom5Editor/VMs/RelayCommand.cs` - Command implementation
- `Dom5Editor/ViewModelBase/ViewModelBase.cs` - Base class

---

### 2. Entity Views Not Implemented

**Status:** MonsterView, WeaponView, ArmorView, and ItemView are implemented in the new UI system.

**Recent Improvements (2026-01-05):**
- ItemView: Equipment display shows referenced weapon/armor stats, damage types, special properties, and secondary effects
- WeaponView: Added damage types, special properties, and secondary effect display
- Both views support VanillaModified fallback for reference properties

Missing entity views:
- SpellView (high complexity - path requirements, effect types, gem/fatigue costs)
- SiteView
- NationView (high complexity - many reference properties)
- EventView (high complexity - event codes, conditions)

---

## Architecture Concerns

### 1. Mod Class Has Too Many Responsibilities

**File:** `Dom5Edit/Mod/Mod.cs`
**Size:** ~550 lines (reduced from 963 after ModParser/ModExporter split)

Still handles:
- Entity storage
- ID range management
- Logging
- Dependency resolution
- Nation association tracking

Consider further splitting for testability.

---

### 2. No Automated Tests

The project has no test project or test framework configured. This makes it risky to refactor and difficult to verify bug fixes.

**Recommendation:** Add a test project with at least:
- Round-trip parsing tests (parse → export → parse gives same result)
- Command mapping coverage tests
- Reference resolution tests

---

### 3. Spell.cs Exceeds Token Limits

**File:** `Dom5Edit/Entities/Spell.cs`
**Size:** ~45000 tokens

The Spell entity has so many property mappings that it exceeds reasonable file size. This indicates the need for the data-driven approach described in ENHANCEMENT_PLAN.md.

---

## Fixed Issues

### ~~Undo Support Not Implemented~~ FIXED (2026-01)

**Files:** `Dom5Editor/UI/ViewModels/EntityViewModel.cs`

The new UI ViewModels now properly use CommandHistory for edits:
- `SetStringProperty()` uses `_history.Execute(new SetStringPropertyCommand(...))`
- `SetIntProperty()` uses `_history.Execute(new SetIntPropertyCommand(...))`
- `SetCommandProperty()` uses `_history.Execute(new SetCommandPropertyCommand(...))`

Ctrl+Z/Ctrl+Y now works in the MonsterView UI.

---

### ~~ChangesMod Not Initialized~~ FIXED (2026-01)

**File:** `Dom5Editor/UI/Views/MainWindowViewModel.cs`

`MainWindowViewModel.LoadEntities<>()` now calls `vm.SetChangesMod(Changes)` on all ViewModels.
Session edit tracking is functional.

---

### ~~ComboBox Scroll Adding Items~~ FIXED

**Files:** `CommandListEditor.xaml.cs`, `IntPropertyListEditor.xaml.cs`

Added `PreviewMouseWheel` handler that checks `IsDropDownOpen` and marks event as handled if dropdown is closed.

---

### ~~Magic Path Editor Not Displaying~~ FIXED

**File:** `MagicPathEditor.xaml.cs`

Removed `DataContext = this` from constructor. Changed command bindings to `ElementName=Root`.

---

### ~~Session Edit Tracking Not Working~~ FIXED (2026-01)

**Root Cause:** In `MainWindowViewModel`, `ClearHistory()` created a new `ChangesMod` AFTER `InitializeCollections()` populated ViewModels with the old reference.

**Files Fixed:**
- `Dom5Editor/UI/Views/MainWindowViewModel.cs` - Reordered operations: `ClearHistory()` now runs BEFORE `InitializeCollections()`
- `Dom5Editor/UI/ViewModels/EntityViewModels.cs` - Badge value change handlers now update `IsSessionEdit` on the badge after edits
- `Dom5Editor/UI/ViewModels/EntityViewModels.cs` - `CanRemove` logic now checks `EntitySource` to prevent removing properties from pure vanilla entities

---

### ~~Small Bug Fixes Batch~~ FIXED

- Incorrect HasFlag usage in IDEntity.cs:113
- Hardcoded ERA value in IntProperty.cs:41-43
- Debug code in EntitySet.cs:72-76
- Unused usings in Reference.cs
- _StringExported not static in StringOrIDRef.cs
- Property.Create in Nation.cs

---

## Questions to Investigate

1. **Event.Assign calls base.Assign then repeats work** - Lines 382-389 call `base.Assign` but then re-do SetID, ParentMod, and Selected assignments. Is this intentional?

2. **IntProperty default value of 10** - Line 69 returns 10 as default. Is this meaningful or arbitrary?
