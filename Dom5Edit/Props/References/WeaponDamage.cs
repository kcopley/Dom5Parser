using Dom5Edit.Commands;

namespace Dom5Edit.Props
{
    /// <summary>
    /// Represents weapon damage which can be:
    /// - A numeric damage value (e.g., "5")
    /// - "summonunits" - weapon summons monster (Value becomes monster ID when overridden)
    /// - "cloud" - weapon creates a cloud effect (read-only)
    /// - Other special effect strings
    /// </summary>
    public class WeaponDamage : StringProperty
    {
        // Known special damage type strings
        public const string TypeSummonUnits = "summonunits";
        public const string TypeCloud = "cloud";
        public const string TypePlaneshiftOther = "planeshiftother";
        public const int DefaultSummonMonsterId = 297;

        public new static Property Create()
        {
            return new WeaponDamage();
        }

        /// <summary>
        /// Returns true if the value is a special effect type string (not numeric).
        /// </summary>
        public bool IsSpecialEffectType
        {
            get
            {
                if (string.IsNullOrEmpty(Value))
                    return false;
                // If it parses as an integer, it's not a special type
                return !int.TryParse(Value, out _);
            }
        }

        /// <summary>
        /// Returns true if this is a summon weapon (can edit monster ID).
        /// </summary>
        public bool IsSummonType => Value == TypeSummonUnits;

        /// <summary>
        /// Returns true if this is a cloud weapon (read-only effect).
        /// </summary>
        public bool IsCloudType => Value == TypeCloud;

        /// <summary>
        /// Gets the numeric value if available, or default for summon weapons.
        /// </summary>
        public int? NumericValue
        {
            get
            {
                if (int.TryParse(Value, out int val))
                    return val;
                if (IsSummonType)
                    return DefaultSummonMonsterId;
                return null;
            }
        }

        public override string ToExportString()
        {
            // Special effect types (cloud, summonunits, etc.) are read-only and cannot be exported.
            // They exist in vanilla.dm only for informational purposes.
            // Only numeric values (damage or monster ID) can be exported.
            if (IsSpecialEffectType)
            {
                return ""; // Don't export - inherited effect cannot be changed via #dmg
            }

            if (CommandsMap.TryGetString(Command, out string s))
            {
                if (!String.IsNullOrEmpty(Comment))
                {
                    return s + " " + Value + " -- " + Comment;
                }
                else
                {
                    return s + " " + Value;
                }
            }
            else return "";
        }
    }
}
