# Reference Property Handling in Badge System

**STATUS: RESOLVED (2026-01-07)**

## Problem Statement

When adding new reference properties via the badge system (e.g., adding a fort commander to a Nation), the current implementation incorrectly uses `IntProperty.Create()`. However, reference commands use specialized property types defined in each entity's property map.

**SOLUTION IMPLEMENTED:** Added `AddPropertyFromMap()` method that uses the entity's `GetPropertyMap()` to get the correct property factory. See `EntityViewModel.cs`.

**Note:** A related bug where `HasValue` wasn't being set on programmatic ID assignment was fixed 2026-01-07. See ISSUES.md for details.

## Current (Incorrect) Implementation

```csharp
// EntityViewModel.cs - AddIntProperty method
protected void AddIntProperty(Command command, int value)
{
    var property = IntProperty.Create(command, _entity, value);  // WRONG for references
    _entity.AddProperty(property);
}
```

## The Entity Property Map System

Each entity type (Nation, Monster, etc.) has a `_propertyMap` that defines which `Property` subclass to use for each command:

```csharp
// Nation.cs - Property map setup
_propertyMap.Add(Command.ADDRECCOM, MonsterOrMontagRef.Create);
_propertyMap.Add(Command.ADDFOREIGNUNIT, MonsterRef.Create);
_propertyMap.Add(Command.STARTSITE, SiteRef.Create);
// etc.
```

### Property Type Hierarchy

```
Property (abstract base)
├── IntProperty           - Simple integer values (#hp 15)
├── StringProperty        - String values (#name "Foo")
├── CommandProperty       - Flag commands (#amphibian)
├── BitmaskProperty       - Bitmask values (#magicskill)
└── Reference (abstract)  - ID references to other entities
    ├── MonsterRef        - References to monsters
    ├── MonsterOrMontagRef - Monster or montag references
    ├── WeaponRef         - References to weapons
    ├── ArmorRef          - References to armor
    ├── SiteRef           - References to sites
    ├── SpellRef          - References to spells
    ├── ItemRef           - References to items
    ├── NationRef         - References to nations
    └── ... (other ref types)
```

## Correct Implementation Required

### Option 1: Use Entity's Property Map (Recommended)

```csharp
protected void AddReferenceProperty(Command command, int referenceId)
{
    var propertyMap = _entity.GetPropertyMap();

    if (!propertyMap.TryGetValue(command, out var factory))
    {
        throw new InvalidOperationException($"No property factory for command {command}");
    }

    // Create property using the correct factory
    var property = factory();
    property.Command = command;
    property.Parent = _entity;

    // Set the reference value based on property type
    if (property is Reference refProp)
    {
        refProp.SetID(referenceId);
    }
    else if (property is IntProperty intProp)
    {
        intProp.Value = referenceId;
    }

    _entity.AddProperty(property);
}
```

### Option 2: Create Type-Specific Factory Methods

```csharp
protected void AddMonsterRef(Command command, int monsterId)
{
    var property = MonsterOrMontagRef.Create();
    property.Command = command;
    property.Parent = _entity;
    property.SetID(monsterId);
    _entity.AddProperty(property);
}

protected void AddSiteRef(Command command, int siteId)
{
    var property = SiteRef.Create();
    // ...
}
// etc.
```

### Option 3: Extend Badge Config with Property Type

Add explicit property type to the badge JSON config:

```json
{
  "command": "addreccom",
  "displayName": "Fort Commander",
  "type": "ref",
  "refType": "monster",
  "propertyType": "MonsterOrMontagRef"  // NEW: explicit property type
}
```

## Reference Property Commands by Entity

### Nation
| Command | Property Type | Description |
|---------|--------------|-------------|
| ADDRECCOM | MonsterOrMontagRef | Fort commanders |
| ADDFOREIGNUNIT | MonsterRef | Foreign recruits |
| ADDRECUNIT | MonsterOrMontagRef | Recruit units |
| STARTSITE | SiteRef | Starting sites |
| LIKESTERR | IntProperty | Terrain preferences |

### Monster
| Command | Property Type | Description |
|---------|--------------|-------------|
| WEAPON | WeaponRef | Monster weapons |
| ARMOR | ArmorRef | Monster armor |
| FIRSTSHAPE | ShapechangeRef | Transformation target |
| SECONDSHAPE | ShapechangeRef | Second form |
| SUMMON1-5 | MonsterRef | Summoned creatures |

### Item
| Command | Property Type | Description |
|---------|--------------|-------------|
| WEAPON | WeaponRef | Item grants weapon |
| ARMOR | ArmorRef | Item grants armor |
| COPYITEM | ItemRef | Copy item stats |

## Key Methods to Implement

### 1. AddBadgeProperty Enhancement

```csharp
protected void AddBadgeProperty(AvailablePropertyItem badge)
{
    if (badge.IsReference)
    {
        AddPropertyFromMap(badge.Command, badge.DefaultValue ?? 0);
    }
    else if (badge.DefaultValue.HasValue)
    {
        SetIntProperty(badge.Command, badge.DefaultValue.Value);
    }
    else
    {
        SetCommandProperty(badge.Command, true);
    }
}

private void AddPropertyFromMap(Command command, int value)
{
    var propertyMap = _entity.GetPropertyMap();

    if (!propertyMap.TryGetValue(command, out var factory))
    {
        // Fallback to IntProperty for commands not in map
        var intProp = IntProperty.Create(command, _entity, value);
        _entity.AddProperty(intProp);
        return;
    }

    var property = factory();
    property.Command = command;
    property.Parent = _entity;

    // Set value based on actual property type
    switch (property)
    {
        case Reference refProp:
            refProp.SetID(value);
            break;
        case IntProperty intProp:
            intProp.Value = value;
            break;
        // Add other cases as needed
    }

    _entity.AddProperty(property);
}
```

### 2. RemoveBadgeProperty Enhancement

```csharp
protected void RemoveBadgeProperty(PropertyItem badge)
{
    if (badge.IsReference)
    {
        RemovePropertyByValue(badge.Command, badge.ReferenceId);
    }
    else if (badge.HasValue && badge.Value != null)
    {
        RemoveCommandProperty(badge.Command);
    }
    else
    {
        SetCommandProperty(badge.Command, false);
    }
}

private void RemovePropertyByValue(Command command, int value)
{
    Property propertyToRemove = null;

    foreach (var prop in _entity.GetMultiple(command))
    {
        int propValue = prop switch
        {
            Reference refProp => refProp.ID,
            IntProperty intProp => intProp.Value,
            _ => -1
        };

        if (propValue == value)
        {
            propertyToRemove = prop;
            break;
        }
    }

    if (propertyToRemove != null)
    {
        _entity.RemoveProperty(propertyToRemove);
    }
}
```

## Testing Checklist

- [ ] Add single fort commander to Nation (no existing commanders)
- [ ] Add second fort commander when one exists
- [ ] Remove specific fort commander (not all)
- [ ] Add/remove weapons from Monster
- [ ] Add/remove armor from Monster
- [ ] Verify undo/redo works for reference additions/removals
- [ ] Verify export produces correct .dm syntax

## Files to Modify

1. `Dom5Editor/UI/ViewModels/EntityViewModel.cs`
   - Add `AddPropertyFromMap()` method
   - Update `AddBadgeProperty()` to use new method
   - Update `RemovePropertyByValue()` to handle Reference types

2. `Dom5Edit/Entities/IDEntity.cs`
   - Ensure `GetPropertyMap()` is accessible (may need to make public)

3. Possibly `Dom5Edit/Props/References/Reference.cs`
   - Ensure `SetID()` method exists and works consistently
