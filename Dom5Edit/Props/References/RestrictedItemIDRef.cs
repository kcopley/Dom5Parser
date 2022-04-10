using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class RestrictedItemIDRef : IDRef
    {
        public static Property Create()
        {
            return new RestrictedItemIDRef();
        }

        public override void Resolve()
        {
            /*
            if (Parent.Parent.RestrictedItems.TryGetValue(ID, out RestrictedItem m))
            {
                entity = m;
                Resolved = true;
            }
            */
        }
    }
}
