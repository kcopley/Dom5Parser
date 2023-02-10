using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class WeaponDamage : StringProperty
    {
        public new static Property Create()
        {
            return new WeaponDamage();
        }

        public override string ToExportString()
        {
            if (CommandsMap.TryGetString(Command, out string s))
            {
                if (!String.IsNullOrEmpty(Comment))
                {
                    return s + " " + Value + " -- " + Comment;
                }
                else
                {
                    return s + " " + Value + "";
                }
            }
            else return "";
        }
    }
}
