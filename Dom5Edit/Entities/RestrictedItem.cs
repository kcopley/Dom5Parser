using Dom5Edit.Commands;
using Dom5Edit.Mods;
using Dom5Edit.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Entities
{
    public class RestrictedItem
    {
        public int RestrictedItemID { get; set; }
        public RestrictedItem DependentRestrictedItem;
        public List<IDEntity> ReferencedEntities = new List<IDEntity>();

        public RestrictedItem(int ID)
        {
            this.RestrictedItemID = ID;
        }

        public int GetID()
        {
            if (DependentRestrictedItem != null) return DependentRestrictedItem.RestrictedItemID;
            return RestrictedItemID;
        }
    }
}
