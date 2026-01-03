# Code Improvements

Small fixes and organizational improvements to simplify the codebase before implementing larger features.

---

## Dom5Edit/Entities/

### IDEntity.cs

1. **Lines 135-140: Abstract methods throwing NotImplementedException**
   - `GetNewCommand()`, `GetSelectCommand()`, `GetEntityType()`, `GetPropertyMap()` all throw `NotImplementedException`
   - **Fix:** Make these `abstract` methods instead of `virtual` with throwing bodies
   ```csharp
   internal abstract Command GetNewCommand();
   internal abstract Command GetSelectCommand();
   internal abstract EntityType GetEntityType();
   internal abstract Dictionary<Command, Func<Property>> GetPropertyMap();
   ```

2. **Lines 443-451: TryGetCopyFrom/TryGetCopySpr throwing NotImplementedException**
   - Same issue as above
   - **Fix:** Make `abstract` or provide a default implementation that returns `false`

3. **Line 113: HasFlag usage is incorrect**
   ```csharp
   if ((ReturnType.TRUE | ReturnType.COPIED).HasFlag(exists))
   ```
   - This checks if the combined flags contain `exists`, which is backwards
   - **Fix:** Should be `exists.HasFlag(ReturnType.TRUE) || exists.HasFlag(ReturnType.COPIED)` or simpler: `exists != ReturnType.FALSE`

4. **Lines 453-475: sort_properties uses magic numbers**
   - Property ordering logic uses hardcoded return values 1-5
   - **Fix:** Consider using an enum for sort priority or moving ordering metadata to PropertyDefinition

5. **Line 227: Unnecessary re-sort after RemoveProperty**
   - `RemoveProperty` calls `OrderBy` after removing, but the list was already sorted
   - **Fix:** Remove the re-sort since removal doesn't change relative order

### EntitySet.cs

1. **Lines 72-76: Debug code left in production**
   ```csharp
   if (id == 7712 || id == 7713)
   {
       int abc = 0;
       abc++;
   }
   ```
   - **Fix:** Remove this debugging code

2. **Lines 159-188: Complex export logic with repeated patterns**
   - Four nearly identical foreach loops for export
   - **Fix:** Consolidate into a helper method that takes the collection and filter

### Monster.cs, Weapon.cs, Item.cs, Nation.cs, Event.cs, Site.cs, Armor.cs, Spell.cs

1. **Static constructor property maps are verbose**
   - Monster.cs has ~560 lines just for property mappings
   - Each entity repeats the same pattern
   - **Future improvement:** Move to data-driven approach (see ENHANCEMENT_PLAN.md)

2. **Repeated TryGetCopyFrom pattern**
   - Each entity with copy support has nearly identical `TryGetCopyFrom` implementation
   - **Fix:** Extract to base class with configurable copy command:
   ```csharp
   // In IDEntity
   protected virtual Command? CopyCommand => null;
   public override bool TryGetCopyFrom(out IDEntity copy) {
       if (CopyCommand.HasValue && TryGet<Reference>(CopyCommand.Value, out var ref, false) == ReturnType.TRUE) {
           copy = ref.Entity;
           return true;
       }
       copy = null;
       return false;
   }

   // In Monster
   protected override Command? CopyCommand => Command.COPYSTATS;
   ```

---

## Dom5Edit/Props/

### Property.cs

1. **Missing static Create method**
   - Nation.cs line 235 uses `Property.Create` but base class doesn't have it
   - **Fix:** Add to base class or ensure only concrete types have Create methods

### IntProperty.cs

1. **Lines 41-43: Hardcoded ERA special case**
   ```csharp
   if (Command == Command.ERA)
   {
       return s + " " + 2 + " -- " + Comment;
   }
   ```
   - Hardcoded value `2` for ERA command - unclear why
   - **Fix:** Either document why or remove this special case

2. **Line 69: Magic default value**
   ```csharp
   return new IntProperty() { Value = 10 };
   ```
   - Arbitrary default value of 10
   - **Fix:** Consider making this configurable or document the reasoning

### StringOrIDRef.cs

1. **Lines 118-123: _StringExported list is per-instance**
   - This list is created for every StringOrIDRef instance but contains static data
   - **Fix:** Make it `static readonly`
   ```csharp
   private static readonly List<Command> _StringExported = new List<Command>()
   {
       Command.STARTSITE,
       Command.SPELL,
       Command.AUTOSPELL,
   };
   ```

### Reference.cs

1. **Lines 1-7: Unused using statements**
   - `System.Collections.Generic`, `System.Linq`, `System.Text`, `System.Threading.Tasks` appear unused
   - **Fix:** Remove unused usings

---

## Dom5Edit/Mod/

### Mod.cs (963 lines)

1. **Lines 345-404 and 381-445: Duplicated GetNextCommandIndex logic**
   - The `##fullgodname##` etc. parsing is duplicated in two methods
   - **Fix:** Extract to a shared helper method
   ```csharp
   private static readonly Dictionary<string, int> SpecialTokens = new()
   {
       { "##fullgodname##", 15 },
       { "##godname##", 11 },
       { "##disname##", 11 },
       // ... etc
   };

   private int SkipSpecialToken(string s, int index)
   {
       foreach (var (token, length) in SpecialTokens)
       {
           if (s.IndexOf(token, index) == index)
               return index + length;
       }
       return -1; // not a special token
   }
   ```

2. **Lines 25-46, 47-66, 68-82, 104-111: Multiple mapping dictionaries**
   - `CommandEntityMap`, `TypeEntityMap`, `Database`, `Dependents` all express related concepts
   - **Future improvement:** Consolidate into a single `EntityTypeMetadata` class

3. **Line 142: Private field for current entity state**
   - Parsing state is stored in instance field `_currentEntity`
   - **Fix:** Consider passing as parameter to parsing methods for clearer flow

4. **Lines 117-140: ID ranges as static fields**
   - These could be organized into a configuration class
   - **Future improvement:** Load from configuration file for easier Dom5 vs Dom6 switching

### ModSet.cs

1. **Consider splitting merge logic**
   - The merge functionality is complex enough to warrant its own class
   - **Future improvement:** Create `ModMerger` class

---

## Dom5Edit/Commands/

### Command.cs (3284 lines)

1. **Massive enum file**
   - Contains ~1600+ enum values
   - Unavoidable given the .dm format, but maintenance is challenging
   - **Future improvement:** Consider code generation from a data file

### CommandsMap.cs

1. **Verify enum coverage**
   - Ensure all Command enum values have corresponding string mappings
   - **Suggestion:** Add a unit test to verify bidirectional mapping completeness

---

## Dom5Editor/

### General MVVM Improvements

1. **ViewModel duplication**
   - Many ViewModels have similar property patterns
   - **Future improvement:** Consider a generic PropertyViewModel<T> base

2. **View-Model binding could use DataTemplates**
   - Instead of explicit View/ViewModel pairs, use DataTemplates for automatic binding
   - **Future improvement:** Reduces boilerplate in MainWindow.xaml

---

## Quick Wins (Low Effort, High Impact)

1. Remove debug code from EntitySet.cs (lines 72-76)
2. Make `_StringExported` static in StringOrIDRef.cs
3. Remove unused usings in Reference.cs
4. Fix HasFlag usage in IDEntity.cs line 113
5. Make throwing methods abstract in IDEntity.cs
6. Remove unnecessary re-sort in IDEntity.RemoveProperty()

---

## Medium Effort Improvements

1. Extract duplicated special token parsing in Mod.cs
2. Consolidate TryGetCopyFrom implementations
3. Add unit test for Command-string mapping coverage

---

## Larger Refactoring (For Enhancement Plan)

1. PropertyDefinition metadata system
2. Split Mod.cs into Parser/Document/Exporter
3. Data-driven entity property maps
4. Configuration-based ID ranges
