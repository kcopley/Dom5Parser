using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class MonsterRef : StringOrIDRef
    {
        public static Property Create()
        {
            return new MonsterRef();
        }

        public override void Resolve()
        {
            if (IsStringRef)
            {
                if (Parent.Parent.TryGetValueNamedMonsters(Name, out IDEntity m))
                {
                    entity = m;
                    Resolved = true;
                }
            }
            else
            {
                if (Parent.Parent.TryGetValueMonsters(ID, out IDEntity m))
                {
                    entity = m;
                    Resolved = true;
                }
            }
        }

        public override string ToString()
        {
            if (entity != null && entity.ID != -1)
                IsStringRef = false;
            return base.ToString();
        }
    }
}
