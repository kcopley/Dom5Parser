using Dom5Edit.Commands;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// Represents an equipment item (weapon or armor) for display in the UI.
    /// </summary>
    public class EquipmentItem
    {
        // Core identification
        public int ID { get; set; }
        public string Name { get; set; }
        public string DisplayText => string.IsNullOrEmpty(Name) ? $"#{ID}" : $"{Name} (#{ID})";

        // Status tracking
        public bool IsModified { get; set; }
        public bool IsSessionEdit { get; set; }
        public bool IsInherited { get; set; }
        public bool CanRemove => !IsInherited;
        public Command SourceCommand { get; set; }

        /// <summary>
        /// Entity type for navigation ("weapon" or "armor").
        /// </summary>
        public string EntityType { get; set; }

        // Weapon stats
        public int? Damage { get; set; }
        public int? Attack { get; set; }
        public int? Defense { get; set; }
        public int? Length { get; set; }
        public int? NumAttacks { get; set; }
        public int? Range { get; set; }
        public int? Aoe { get; set; }
        public int? Precision { get; set; }
        public string DamageTypes { get; set; }
        public bool HasDamageTypes => !string.IsNullOrEmpty(DamageTypes);

        // Armor stats
        public int? Protection { get; set; }
        public int? Encumbrance { get; set; }
        public string ArmorTypeName { get; set; }

        // Individual column display helpers for table layout
        public string DamageDisplay => (Damage ?? 0).ToString();
        public string AttackDisplay => FormatBonus(Attack);
        public string LengthDisplay => (Length ?? 0).ToString();
        public string ProtectionDisplay => (Protection ?? 0).ToString();
        public string DefenseDisplay => FormatBonus(Defense);
        public string EncumbranceDisplay => (Encumbrance ?? 0).ToString();

        // Tooltip for extended weapon info
        public string WeaponTooltip
        {
            get
            {
                var lines = new System.Collections.Generic.List<string>();
                lines.Add($"{Name ?? "(unknown)"} (#{ID})");
                lines.Add($"DMG: {Damage ?? 0}  ATT: {FormatBonus(Attack)}  DEF: {FormatBonus(Defense)}  LEN: {Length ?? 0}");
                lines.Add($"Attacks: {NumAttacks ?? 1}  Range: {FormatDash(Range)}  AoE: {FormatDash(Aoe)}  Precision: {FormatDash(Precision)}");
                if (HasDamageTypes)
                    lines.Add($"Types: {DamageTypes}");
                if (IsInherited)
                    lines.Add("(Inherited from copystats)");
                return string.Join("\n", lines);
            }
        }

        // Tooltip for extended armor info
        public string ArmorTooltip
        {
            get
            {
                var lines = new System.Collections.Generic.List<string>();
                lines.Add($"{Name ?? "(unknown)"} (#{ID})");
                lines.Add($"Protection: {Protection ?? 0}  Defense: {FormatBonus(Defense)}  Encumbrance: {Encumbrance ?? 0}");
                lines.Add($"Type: {ArmorTypeName ?? "Unknown"}");
                if (IsInherited)
                    lines.Add("(Inherited from copystats)");
                return string.Join("\n", lines);
            }
        }

        private static string FormatBonus(int? val) => val.HasValue ? (val >= 0 ? $"+{val}" : $"{val}") : "+0";
        private static string FormatDash(int? val) => val.HasValue && val != 0 ? val.ToString() : "-";
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
