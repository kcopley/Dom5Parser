using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class SpellDamageRef : Reference
    {
        public static Property Create()
        {
            return new SpellDamageRef();
        }

        public override void Resolve()
        {
        }

        public override void Parse(Command c, string v, string comment)
        {
            base.Parse(c, v, comment);
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
