# Known Issues

Potential bugs and critical problems identified during code review.

---

## Current Priority: Infrastructure First

Before adding features or redesigning UI, these blocking issues must be resolved:

### 1. New UI ViewModels Missing Undo Support (BLOCKING)

**Files:** `Dom5Editor/UI/ViewModels/EntityViewModel.cs`, `EntityViewModels.cs`

**Status:** Must fix before any other work.

The new UI ViewModels (`EntityViewModel`, `MonsterViewModel`, etc.) have a `_history` field for CommandHistory but are NOT using it for edits. Property setters directly modify the entity:

```csharp
// Current (no undo):
protected void SetStringProperty(Command command, string value, ...)
{
    _entity.Set<StringProperty>(command, p => p.Value = value);  // Direct modification
    // Records to ChangesMod but not CommandHistory
}

// Should be:
protected void SetStringProperty(Command command, string value, ...)
{
    var cmd = new SetStringPropertyCommand(_entity, command, value);
    _history.Execute(cmd);  // Through CommandHistory for undo
}
```

**Impact:** Ctrl+Z/Ctrl+Y won't work in the new MonsterView UI.

**Fix Required:**
1. Wire `SetIntProperty()` through CommandHistory
2. Wire `SetStringProperty()` through CommandHistory
3. Wire `SetCommandProperty()` through CommandHistory
4. Wire reference property setters through CommandHistory
5. Initialize `_changesMod` from MainWindowViewModel
6. Test undo/redo works end-to-end

---

### 2. ChangesMod Not Initialized in New UI (BLOCKING)

**File:** `Dom5Editor/UI/ViewModels/EntityViewModel.cs`

The `_changesMod` field is never set (always null), breaking change tracking:

```csharp
if (_changesMod != null)  // Always false!
{
    _changesMod.RevertPropertyChange(_entity, command);
}
```

**Fix:** Pass ChangesMod from MainWindowViewModel when creating EntityViewModels.

---

## After Infrastructure: UI Redesign Required

See `ENHANCEMENT_PLAN.md` section 6.8 for full spec.

**Problem:** Current vertical list layouts waste too much space. With ~580 properties possible on a Monster, users need 70-100 visible at once.

**Solution:** Horizontal badge wrapping layout (like magic paths) for all property lists.

---

### 3. Two Parallel ViewModel Systems - DEPRECATION PLANNED

**Files:** `Dom5Editor/VMs/` vs `Dom5Editor/UI/ViewModels/`

The codebase has two separate ViewModel systems:
- **Legacy VMs** (`Dom5Editor/VMs/`) - DEPRECATED, to be removed
- **New UI VMs** (`Dom5Editor/UI/ViewModels/`) - Active development

**Migration Status:**
The new UI system is the future. Legacy VMs will be deprecated once all functionality is migrated.

**Features to migrate from Legacy VMs:**
| Feature | Legacy File | Status |
|---------|-------------|--------|
| CommandHistory integration | `IntPropertyViewModel.cs` | NOT MIGRATED |
| Undo/Redo in setters | All legacy VMs | NOT MIGRATED |
| ChangesMod wiring | Various | PARTIAL (null) |
| Reference ViewModels | `RefVMs/` folder | NOT STARTED |
| Mod-level undo/redo | `ModViewModel.cs` | EXISTS (keep) |

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

### 3. Session Edit Tracking for Magic Paths

**File:** `Dom5Editor/UI/ViewModels/EntityViewModels.cs` (MonsterViewModel)

When adding magic paths via `AddMagicPath()`, the `IsSessionEdit` flag is always set to `false`. Need to track which properties were added in the current session.

```csharp
public void AddMagicPath(int pathId, int level)
{
    // ... creates property but doesn't track session edit state
    MagicPaths.Add(new MagicPathItem
    {
        IsSessionEdit = false,  // Should be true for newly added paths
        // ...
    });
}
```

---

### 4. ChangesMod Revert Not Wired in New UI

**File:** `Dom5Editor/UI/ViewModels/EntityViewModel.cs`

The `SetStringProperty` method references `_changesMod.RevertPropertyChange()` but `_changesMod` is never set (always null).

```csharp
protected void SetStringProperty(Command command, string value, ...)
{
    if (_changesMod != null)  // Always null - never initialized!
    {
        _changesMod.RevertPropertyChange(_entity, command);
    }
}
```

---

## Fixed Issues (Phase 6)

### ~~ComboBox Scroll Adding Items~~ FIXED

**Files:** `CommandListEditor.xaml.cs`, `IntPropertyListEditor.xaml.cs`

**Problem:** Scrolling mouse wheel while hovering over ComboBox (dropdown closed) was adding items to the list.

**Solution:** Added `PreviewMouseWheel` handler that checks `IsDropDownOpen` and marks event as handled if dropdown is closed.

### ~~Magic Path Editor Not Displaying~~ FIXED

**File:** `MagicPathEditor.xaml.cs`

**Problem:** Nothing was displaying in the MagicPathEditor control.

**Solution:** Removed `DataContext = this` from constructor (breaks parent bindings). Changed command bindings from `RelativeSource` to `ElementName=Root`.

---

## Fixed Issues (Commit: "Small bug fixes")

### ~~1. Incorrect HasFlag Usage in IDEntity.cs:113~~ FIXED

Fixed: Changed to `if (exists == ReturnType.TRUE || exists == ReturnType.COPIED)`

### ~~2. Hardcoded ERA Value in IntProperty.cs:41-43~~ FIXED

Fixed: Removed the hardcoded ERA special case.

### ~~3. Debug Code in EntitySet.cs:72-76~~ FIXED

Fixed: Removed debug breakpoint code.

### ~~4. Unused usings in Reference.cs~~ FIXED

Fixed: Removed unused using statements.

### ~~5. _StringExported not static in StringOrIDRef.cs~~ FIXED

Fixed: Made `_StringExported` static readonly.

### ~~6. Property.Create in Nation.cs~~ FIXED

Fixed: Changed `Property.Create` to `StringProperty.Create` for DISBLESS command.

---

## Architecture Concerns

### 1. Mod Class Has Too Many Responsibilities

**File:** `Dom5Edit/Mod/Mod.cs`
**Size:** 963 lines

The Mod class handles:
- File parsing
- Entity storage
- ID range management
- Export/serialization
- Logging
- Dependency resolution
- Nation association tracking

This makes the class difficult to test, maintain, and extend. For a full editor, this should be split into focused classes.

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
**Size:** ~45000 tokens (couldn't read fully)

The Spell entity has so many property mappings that it exceeds reasonable file size. This makes it difficult to work with and indicates the need for the data-driven approach described in ENHANCEMENT_PLAN.md.

---

## Questions to Investigate

1. **Property.Create in Nation.cs:235** - Is this a compile error or was a Create method added to the abstract Property class?

2. **Event.Assign calls base.Assign then repeats work** - Lines 382-389 call `base.Assign` but then re-do SetID, ParentMod, and Selected assignments. Is this intentional?

3. **IntProperty default value of 10** - Line 69 returns 10 as default. Is this meaningful or arbitrary?
