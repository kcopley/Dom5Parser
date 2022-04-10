using Dom5Edit.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class NameProperty : StringProperty
    {
        public new static Property Create()
        {
            return new NameProperty();
        }

        public override void Parse(Command c, string s, string comment)
        {
            base.Parse(c, s, comment);
            Parent.AddNamed(s);
        }
    }
}
