using Dom5Edit.Commands;
using Dom5Edit.Entities;

namespace Dom5Edit.Props
{
    public class SpellDamage : Reference
    {
        private string _val;

        private MonsterOrMontagRef _monRef;
        private EnchIDRef _enchRef;
        private EventEffectCodeRef _eventEffectRef;


        public static Property Create()
        {
            return new SpellDamage();
        }

        public override void Parse(Command c, string v, string comment)
        {
            _val = v;
            Comment = comment;
            Command = c;
            Parent.ParentMod.SpellDamages.Add(this);
        }

        public override void Resolve()
        {
            Spell parent = (Spell)Parent;

            // First check if the spell's effect type uses bitmask damage
            if (parent.TryGetSpellEffect(out int effect) && VanillaSpellMap.IsBitmaskEffect(effect))
            {
                // Leave as raw string for export
                return;
            }

            // Fallback: check if damage value looks like a bitmask (power of 2 > 8192)
            // These encode special damage effects, not monster IDs
            if (IsBitmaskDamage(_val))
            {
                // Leave as raw string for export
                return;
            }

            //is this a summon?
            if (parent.IsSummon()) ResolveAsSummon();
            else if (parent.IsEnchant()) ResolveAsEnchant();
            else if (parent.IsEventEffect()) ResolveAsEventEffect();

            //currently string
            //resolve to an ID
            //or a bitmask
            //if the bitmask, leave it as a string export w/ no quotes
        }

        private static bool IsBitmaskDamage(string val)
        {
            if (!int.TryParse(val.Trim(), out int damage)) return false;
            if (damage <= 8192) return false; // Could be a valid monster ID

            // Check if it's a power of 2 (single bit set) - common bitmask pattern
            // Also check for combined bitmasks (multiple bits set but still large)
            return (damage & (damage - 1)) == 0 || damage > 100000;
        }

        public override bool TryGetEntity(out IDEntity e)
        {
            e = null;
            if (_monRef != null)
            {
                return _monRef.TryGetEntity(out e);
            }
            return false;
        }

        /// <summary>
        /// Gets the raw ID value if this is an unresolved monster/montag reference.
        /// Used by validation to detect missing references.
        /// </summary>
        public int GetUnresolvedId()
        {
            if (_monRef?.MonsterRef != null && !_monRef.MonsterRef.Resolved)
            {
                return _monRef.MonsterRef.ID;
            }
            return 0;
        }

        void ResolveAsSummon()
        {
            _monRef = new MonsterOrMontagRef();
            _monRef.Parent = this.Parent;
            _monRef.Parse(this.Command, _val, Comment);
            _monRef.Resolve();
        }

        void ResolveAsEnchant()
        {
            _enchRef = new EnchIDRef();
            _enchRef.Parent = this.Parent;
            _enchRef.Parse(this.Command, _val, Comment);
            _enchRef.Resolve();
        }

        void ResolveAsEventEffect()
        {
            _eventEffectRef = new EventEffectCodeRef();
            _eventEffectRef.Parent = this.Parent;
            _eventEffectRef.Parse(this.Command, _val, Comment);
            _eventEffectRef.Resolve();
        }

        public override string ToExportString()
        {
            if (_monRef != null)
            {
                return _monRef.ToExportString();
            }

            if (_enchRef != null)
            {
                return _enchRef.ToExportString();
            }

            if (_eventEffectRef != null)
            {
                return _eventEffectRef.ToExportString();
            }

            if (CommandsMap.TryGetString(Command, out string s))
            {
                if (!String.IsNullOrEmpty(Comment))
                {
                    return s + " " + _val + " -- " + Comment;
                }
                else
                {
                    return s + " " + _val;
                }
            }
            else return "";
        }

        internal override EntityType GetEntityType()
        {
            // SpellDamage is polymorphic - return the type of the resolved reference
            if (_monRef != null)
            {
                return _monRef.GetEntityType();
            }
            if (_enchRef != null)
            {
                return _enchRef.GetEntityType();
            }
            if (_eventEffectRef != null)
            {
                return _eventEffectRef.GetEntityType();
            }
            // Unresolved (bitmask value) - return SPELL as a fallback since it's spell damage
            return EntityType.SPELL;
        }

        internal override bool EqualsProperty<T>(T copyFrom)
        {
            
            return false;
        }
    }
}
