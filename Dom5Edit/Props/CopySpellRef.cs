using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class CopySpellRef : SpellRef
    {
        public new static Property Create()
        {
            return new CopySpellRef();
        }

        public bool IsVanillaID()
        {
            if (HasValue && VanillaSpellMap.ContainsSpell(this.ID)) return true;
            else if (HasValue && IsStringRef && VanillaSpellMap.ContainsSpell(Name)) return true;
            return false;
        }

        public bool IsSummon()
        {
            if (HasValue && VanillaSpellMap.IsSummonSpell(this.ID)) return true;
            else if (HasValue && IsStringRef && VanillaSpellMap.IsSummonSpell(Name)) return true;
            return false;
        }

        public bool IsEnchant()
        {
            if (HasValue && VanillaSpellMap.IsEnchantSpell(this.ID)) return true;
            else if (HasValue && IsStringRef && VanillaSpellMap.IsEnchantSpell(Name)) return true;
            return false;
        }

        public bool IsEventEffect()
        {
            if (HasValue && VanillaSpellMap.IsEventEffectSpell(this.ID)) return true;
            else if (HasValue && IsStringRef && VanillaSpellMap.IsEventEffectSpell(Name)) return true;
            return false;
        }

        public bool TryGetSpell(out Spell spell)
        {
            if (this.entity != null && this.entity is Spell)
            {
                spell = (Spell)entity;
                return true;
            }
            spell = null;
            return false;
        }
    }
}
