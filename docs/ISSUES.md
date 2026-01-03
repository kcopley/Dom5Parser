# Known Issues

Potential bugs and critical problems identified during code review.

---

## Potential Bugs

### 1. Incorrect HasFlag Usage in IDEntity.cs:113

**File:** `Dom5Edit/Entities/IDEntity.cs`
**Line:** 113

```csharp
if ((ReturnType.TRUE | ReturnType.COPIED).HasFlag(exists))
```

**Problem:** This logic is inverted. `HasFlag` checks if the caller contains the argument, not the other way around. This will always return true when `exists` is `TRUE` or `COPIED`, but the intent appears to be checking if `exists` is one of those values.

**Fix:**
```csharp
if (exists == ReturnType.TRUE || exists == ReturnType.COPIED)
// or
if (exists != ReturnType.FALSE)
```

**Impact:** May cause incorrect property resolution when getting entity names.

---

### 2. Hardcoded ERA Value in IntProperty.cs:41-43

**File:** `Dom5Edit/Props/IntProperty.cs`
**Lines:** 41-43

```csharp
if (Command == Command.ERA)
{
    return s + " " + 2 + " -- " + Comment;
}
```

**Problem:** The ERA value is always exported as `2` regardless of the actual value stored. This appears to be debug/test code that was never removed.

**Impact:** All mods exported will have their nations set to Era 2 (Middle Ages) regardless of the intended era.

**Fix:** Remove this special case or clarify the intent:
```csharp
// Remove lines 41-43 entirely
```

---

### 3. Debug Code in EntitySet.cs:72-76

**File:** `Dom5Edit/Entities/EntitySet.cs`
**Lines:** 72-76

```csharp
if (id == 7712 || id == 7713)
{
    int abc = 0;
    abc++;
}
```

**Problem:** Debugging breakpoint code left in production.

**Impact:** No functional impact, but indicates incomplete cleanup and reduces code quality.

**Fix:** Remove these lines.

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
