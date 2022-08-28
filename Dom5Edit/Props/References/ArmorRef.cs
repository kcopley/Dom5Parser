using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class ArmorRef : StringOrIDRef
    {
        public static Property Create()
        {
            return new ArmorRef();
        }

        public override void Resolve()
        {
            if (IsStringRef)
            {
                if (Parent.Parent.TryGetValueNamedArmors(Name, out IDEntity m))
                {
                    entity = m;
                    Resolved = true;
                }
            }
            else
            {
                if (Parent.Parent.TryGetValueArmors(ID, out IDEntity m))
                {
                    entity = m;
                    Resolved = true;
                }
            }
            if (!Resolved && !IsStringRef && ID > ModManager.ARMOR_START_ID)
            {
                Parent.Parent.Log("Armor not resolved for: " + this.ID);
            }
        }
    }
}
