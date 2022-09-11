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
                    Entity = m;
                    Resolved = true;
                }
            }
            else
            {
                if (Parent.Parent.TryGetValueItems(ID, out IDEntity m))
                {
                    Entity = m;
                    Resolved = true;
                }
            }
            if (!Resolved && !IsStringRef && ID > ModManager.ITEM_START_ID)
            {
                Parent.Parent.Log("Item not resolved for: " + this.ID);
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
