using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class ItemRef : StringOrIDRef
    {
        public static Property Create()
        {
            return new ItemRef();
        }

        public override void Resolve()
        {
            if (IsStringRef)
            {
                if (Parent.Parent.TryGetValueNamedItems(Name, out IDEntity m))
                {
                    entity = m;
                    Resolved = true;
                }
            }
            else
            {
                if (Parent.Parent.TryGetValueItems(ID, out IDEntity m))
                {
                    entity = m;
                    Resolved = true;
                }
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
