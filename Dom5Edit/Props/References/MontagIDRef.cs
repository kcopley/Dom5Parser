using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class MontagIDRef : IDRef
    {
        public static Property Create()
        {
            return new MontagIDRef();
        }

        public override void Resolve()
        {
            /*
            if (Parent.Parent.Montags.TryGetValue(ID, out Montag m))
            {
                entity = m;
                Resolved = true;
            }
            */
        }
    }
}
