using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class WeaponRef : StringOrIDRef
    {
        public static Property Create()
        {
            return new WeaponRef();
        }

        internal override EntityType GetEntityType()
        {
            return EntityType.WEAPON;
        }

        public override string ToExportString()
        {
            if (Entity != null && Entity.ID != -1)
                IsStringRef = false;
            return base.ToExportString();
        }
    }
}
