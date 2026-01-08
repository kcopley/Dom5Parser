using Dom5Edit.Commands;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// Represents an equipment item (weapon or armor) for display in the UI.
    /// </summary>
    public class EquipmentItem
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string DisplayText => string.IsNullOrEmpty(Name) ? $"#{ID}" : $"{Name} (#{ID})";
        public bool IsModified { get; set; }
        public bool IsSessionEdit { get; set; }
        public bool IsInherited { get; set; }
        public bool CanRemove => !IsInherited;
        public Command SourceCommand { get; set; }
        /// <summary>
        /// Entity type for navigation ("weapon" or "armor").
        /// </summary>
        public string EntityType { get; set; }
    }

    /// <summary>
    /// Represents an available equipment item for selection.
    /// </summary>
    public class AvailableEquipmentItem
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string DisplayText => string.IsNullOrEmpty(Name) ? $"#{ID}" : $"{Name} (#{ID})";
        public string Source { get; set; } // "Vanilla" or "Mod"
    }

    /// <summary>
    /// Represents an item slot type option for ComboBox binding.
    /// </summary>
    public class SlotTypeOption
    {
        public int Value { get; }
        public string DisplayName { get; }

        public SlotTypeOption(int value, string displayName)
        {
            Value = value;
            DisplayName = displayName;
        }

        public override string ToString() => DisplayName;
    }
}
