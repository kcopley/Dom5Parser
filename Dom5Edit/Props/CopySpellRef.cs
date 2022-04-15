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

        public override void Parse(Command c, string s, string comment)
        {
            base.Parse(c, s, comment);
            if (Parent is Spell)
            {
                var _spell = (Spell)Parent;
                _spell.CopySpellID = this.ID;
            }
        }
    }
}
