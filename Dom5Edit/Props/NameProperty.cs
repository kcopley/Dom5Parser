using Dom5Edit.Commands;
using Dom5Edit.Entities;
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
            if (Parent is IDEntity)
            {
                var ie = Parent as IDEntity;
                ie._name = this.Value;
                ie.ParentMod.Database[ie.GetEntityType()].GiveName(ie, this.Value);
            }
        }
    }
}
