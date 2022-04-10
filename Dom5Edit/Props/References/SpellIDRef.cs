using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class SpellIDRef : IDRef
    {
        public static Property Create()
        {
            return new SpellIDRef();
        }

        public override void Resolve()
        {
            if (Parent.Parent.Spells.TryGetValue(ID, out IDEntity m))
            {
                entity = m;
                Resolved = true;
            }
        }
    }
}
