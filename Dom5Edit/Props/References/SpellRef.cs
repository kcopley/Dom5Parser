using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class SpellRef : StringOrIDRef
    {
        public static Property Create()
        {
            return new SpellRef();
        }

        public override void Resolve()
        {
            if (IsStringRef)
            {
                if (Parent.Parent.TryGetValueNamedSpells(Name, out IDEntity m))
                {
                    entity = m;
                    Resolved = true;
                }
            }
            else
            {
                if (Parent.Parent.TryGetValueSpells(ID, out IDEntity m))
                {
                    entity = m;
                    Resolved = true;
                }
            }
            if (!Resolved && !IsStringRef && ID > ModManager.SPELL_START_ID)
            {
                Parent.Parent.Log("Spell not resolved for: " + this.ID);
            }
        }

        public override string ToString()
        {
            if ((_command == Command.NEXTSPELL) && entity != null && entity.ID != -1)
                IsStringRef = false;
            return base.ToString();
        }
    }
}
