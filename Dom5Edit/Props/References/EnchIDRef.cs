using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class EnchIDRef : IDRef
    {
        public static Property Create()
        {
            return new EnchIDRef();
        }

        public override void Resolve()
        {
            if (Parent.Parent.Enchantments.TryGetValue(ID, out Ench m))
            {
                entity = m;
                Resolved = true;
            }
        }
    }
}
