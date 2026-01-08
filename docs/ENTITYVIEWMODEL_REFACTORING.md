# EntityViewModel Refactoring Analysis

**Created:** 2026-01-07
**Status:** Phase 1, 2 & 3 Complete (2026-01-07)

This document analyzes `EntityViewModel.cs` (1960 lines) to identify opportunities for code reduction, generalization, and improved property type support.

---

## Current File Structure

| Section | Lines | Description |
|---------|-------|-------------|
| EntitySource enum | 18-31 | Entity origin tracking |
| Constructor & Core Fields | 37-68 | History subscription, core setup |
| History Notification | 70-104 | Undo/redo property refresh |
| Entity Navigation & Caches | 111-278 | 7 separate cache fields + switch |
| Badge Configuration | 280-324 | Config loading and caching |
| BuildBadgesFromSection | 326-509 | Core badge building logic |
| BuildReferenceBadges | 511-750 | Reference property badge handling |
| Badge Property Helpers | 752-963 | Add/Remove badge methods |
| Entity Resolution | 965-1018 | Layered entity lookup |
| GetLayeredReferenceList | 1020-1153 | Complex generic reference method |
| Basic Properties | 1155-1288 | ID, Name, DisplayName, Source |
| Property Access | 1290-1501 | Get/Set for Int, String, Command |
| Inherited Checks | 1503-1561 | 3 nearly identical methods |
| Modification Tracking | 1563-1751 | Original values, Reset, CanReset |
| Vanilla Entity | 1753-1801 | GetVanillaEntity, GetEntityTypeFromEntity |
| ModifiedFromVanilla | 1803-1895 | 3 nearly identical methods |
| Session Edit Check | 1897-1958 | PropertyType enum, utilities |

---

## Issue 1: Property Type Triplication

**Impact:** ~21 methods that could collapse to ~7 generic ones

Every property operation is duplicated 3 times for Int, String, and Command property types:

### Current Pattern (Repeated 3x)
```csharp
// Get methods (3 variants)
protected int? GetIntProperty(Command command) { ... }
protected string GetStringProperty(Command command) { ... }
protected bool GetCommandProperty(Command command) { ... }

// Set methods (3 variants)
protected void SetIntProperty(Command command, int? value, ...) { ... }
protected void SetStringProperty(Command command, string value, ...) { ... }
protected void SetCommandProperty(Command command, bool value, ...) { ... }

// Inherited checks (3 variants)
protected bool IsIntPropertyInherited(Command command) { ... }
protected bool IsStringPropertyInherited(Command command) { ... }
protected bool IsCommandPropertyInherited(Command command) { ... }

// Modified from vanilla (3 variants)
protected bool IsIntPropertyModifiedFromVanilla(Command command) { ... }
protected bool IsStringPropertyModifiedFromVanilla(Command command) { ... }
protected bool IsCommandPropertyModifiedFromVanilla(Command command) { ... }

// Original value (3 variants)
protected int? GetOriginalIntValue(Command command) { ... }
protected string GetOriginalStringValue(Command command) { ... }
protected bool GetOriginalCommandValue(Command command) { ... }

// Reset property (3 variants)
public void ResetIntProperty(Command command, string propertyName) { ... }
public void ResetStringProperty(Command command, string propertyName) { ... }
public void ResetCommandProperty(Command command, string propertyName) { ... }

// Can reset (3 variants)
public bool CanResetIntProperty(Command command) { ... }
public bool CanResetStringProperty(Command command) { ... }
public bool CanResetCommandProperty(Command command) { ... }
```

### Proposed Generic Pattern

```csharp
// Single generic get with fallback
protected T GetProperty<T>(Command command) where T : Property, new()
{
    var result = _entity.TryGet<T>(command, out var prop);
    if (result == ReturnType.TRUE || result == ReturnType.COPIED)
        return prop;

    if (_source == EntitySource.VanillaModified)
    {
        var vanillaEntity = GetVanillaEntity();
        if (vanillaEntity != null)
        {
            var vanillaResult = vanillaEntity.TryGet<T>(command, out var vanillaProp);
            if (vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED)
                return vanillaProp;
        }
    }
    return null;
}

// Convenience wrappers remain thin
protected int? GetIntProperty(Command command)
    => GetProperty<IntProperty>(command)?.Value;

protected string GetStringProperty(Command command)
    => GetProperty<StringProperty>(command)?.Value ?? string.Empty;

protected bool GetCommandProperty(Command command)
    => GetProperty<CommandProperty>(command) != null;
```

### Challenge: Value Comparison

Different property types have different value access patterns:
- `IntProperty`: `.Value` (int)
- `StringProperty`: `.Value` (string)
- `CommandProperty`: existence only (no value)

**Potential Solutions:**
1. Add `IPropertyValue<T>` interface to Property classes (requires Dom5Edit changes)
2. Use `Func<T, object>` value extractor parameter
3. Keep type-specific comparison methods, genericize everything else

---

## Issue 2: Missing Property Types in Badge System

**Impact:** Several property types cannot be used in JSON-driven badge UI

`BuildBadgesFromSection()` (lines 376-428) only handles:
- `IsFlag` → `CommandProperty`
- `IsInt` → `IntProperty`

### Property Types NOT Supported

| Type | Example Commands | Current Handling |
|------|------------------|------------------|
| `IntIntProperty` | #gems, #path, #pathlevel | Manual in ViewModels |
| `BitmaskProperty` | #itemslots | Not supported |
| `BitmaskChanceProperty` | #custommagic | Custom editor |
| `FloatProperty` | Various float values | Not supported |
| `Reference types` | #weapon, #armor, #copyspell | Separate method |
| `StringProperty` | #descr, #spr1 | Not in badge system |

### Proposed Extension

Add `type` options to badge JSON:
```json
{ "name": "gems", "type": "intint", "display": "Gems" }
{ "name": "itemslots", "type": "bitmask", "display": "Item Slots" }
{ "name": "descr", "type": "string", "display": "Description" }
```

Requires corresponding handlers in `BuildBadgesFromSection()`.

---

## Issue 3: Cached Entity Lists Proliferation

**Impact:** 7 separate static fields + switch statement (~60 lines)

### Current Pattern
```csharp
// 7 cache fields
private static IReadOnlyList<ReferenceItem> _cachedMonsterRefs;
private static IReadOnlyList<ReferenceItem> _cachedWeaponRefs;
private static IReadOnlyList<ReferenceItem> _cachedArmorRefs;
private static IReadOnlyList<ReferenceItem> _cachedItemRefs;
private static IReadOnlyList<ReferenceItem> _cachedSpellRefs;
private static IReadOnlyList<ReferenceItem> _cachedSiteRefs;
private static IReadOnlyList<ReferenceItem> _cachedNationRefs;

// Switch statement for lookup
protected static IEnumerable<ReferenceItem> GetAvailableReferencesForType(string refType)
{
    switch (refType.ToLowerInvariant())
    {
        case "monster": return _cachedMonsterRefs ??= ConvertToReferenceItems(CachedMonsters);
        case "weapon": return _cachedWeaponRefs ??= ConvertToReferenceItems(CachedWeapons);
        // ... 5 more cases
    }
}
```

### Proposed Consolidation
```csharp
private static readonly Dictionary<string, IReadOnlyList<ReferenceItem>> _referenceItemCaches = new();

protected static IEnumerable<ReferenceItem> GetAvailableReferencesForType(string refType)
{
    if (string.IsNullOrEmpty(refType)) return null;

    var key = refType.ToLowerInvariant();
    if (_referenceItemCaches.TryGetValue(key, out var cached))
        return cached;

    var source = GetCachedListForType(key);
    if (source == null) return null;

    cached = ConvertToReferenceItems(source);
    _referenceItemCaches[key] = cached;
    return cached;
}

private static IReadOnlyList<AvailableEquipmentItem> GetCachedListForType(string key)
{
    return key switch
    {
        "monster" => CachedMonsters,
        "weapon" => CachedWeapons,
        // ... etc
        _ => null
    };
}
```

---

## Issue 4: PropertyType Enum Limited Use

The `PropertyType` enum (lines 1938-1943) is only used in one place:

```csharp
protected enum PropertyType { Int, String, Command }

protected bool IsPropertyModified(Command command, PropertyType propertyType)
{
    if (IsPropertyEditedInSession(command))
        return true;

    return propertyType switch
    {
        PropertyType.Int => IsIntPropertyModifiedFromVanilla(command),
        PropertyType.String => IsStringPropertyModifiedFromVanilla(command),
        PropertyType.Command => IsCommandPropertyModifiedFromVanilla(command),
        _ => false
    };
}
```

If the `IsPropertyModifiedFromVanilla` methods are genericized, this enum and method could be eliminated.

---

## Refactoring Priority

### Phase 1: Low Risk, High Impact ✓ COMPLETE (2026-01-07)
1. ~~**Consolidate reference caches** into dictionary (Issue 3)~~ - Done: 7 fields → 1 dictionary
2. ~~**Extract generic `GetProperty<T>()`** base method (Issue 1 partial)~~ - Done: base method with thin wrappers
3. ~~**Add `IsPropertyInherited<T>()`** generic method~~ - Done: single generic method

### Phase 2: Medium Risk ✓ COMPLETE (2026-01-07)
1. ~~**Genericize `IsPropertyModifiedFromVanilla<T>()`** with value comparator~~ - Done: optional Func<T,T,bool>
2. ~~**Consolidate reset methods**~~ - Done: CanResetProperty() with type-specific aliases
3. ~~**Eliminate PropertyType enum**~~ - Done: removed unused IsPropertyModified() and PropertyType enum
4. [ ] **Add `SetProperty<T>()`** generic with undo/redo - Deferred (complex, Set methods differ significantly)

### Phase 3: Extend Badge System (Larger Scope) ✓ COMPLETE (2026-01-07)
1. ~~**Add `IntIntProperty` badge support**~~ - Done: type detection, factory methods, EntityViewModel processing
2. ~~**Add `StringProperty` badge support**~~ - Done: CreateStringPropertyItem, EntityViewModel processing
3. ~~**Add `BitmaskProperty` badge support**~~ - Done: CreateBitmaskPropertyItem, EntityViewModel processing
4. [ ] **Unify reference handling in main badge loop** - Deferred (works but could be cleaner)

---

## Estimated Line Reduction

| Change | Current Lines | After | Savings | Status |
|--------|--------------|-------|---------|--------|
| Cache consolidation | ~60 | ~25 | ~35 | ✓ Done |
| Generic property access | ~210 | ~80 | ~130 | ✓ Done (Get/Inherited/Modified) |
| PropertyType elimination | ~20 | 0 | ~20 | ✓ Done |
| CanReset consolidation | ~20 | ~5 | ~15 | ✓ Done |
| GetOriginal consolidation | ~60 | ~20 | ~40 | ✓ Done |
| **Total** | | | **~240 lines** |

**Phase 1 Actual Savings:** ~70 lines (cache consolidation + Get/Inherited generics)
**Phase 2 Actual Savings:** ~60 lines (ModifiedFromVanilla + CanReset + PropertyType removal)
**Phase 3 Additions:** Badge system now supports IntIntProperty, StringProperty, and BitmaskProperty types, enabling JSON-driven UI for these property types.
**Additional Refactoring:** ~40 lines saved (GetOriginalProperty<T> generics)

---

## Related Files

- `Dom5Editor/UI/ViewModels/EntityViewModel.cs` - Target file
- `Dom5Edit/Props/Property.cs` - Base property class (may need interface)
- `Dom5Edit/Props/IntProperty.cs`, `StringProperty.cs`, `CommandProperty.cs` - Property types
- `Dom5Editor/Data/BadgeConfigLoader.cs` - Badge JSON parsing

---

## Dependencies

Before implementing:
1. Ensure all ViewModels are extracted to separate files (DONE)
2. Verify no other code depends on the specific method signatures
3. Consider backwards compatibility for any public methods
