# Known Issues

Potential bugs and critical problems identified during code review.

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
