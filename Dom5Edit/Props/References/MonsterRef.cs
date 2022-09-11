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
                    Entity = m;
                    Resolved = true;
                }
            }
            else
            {
                if (Parent.Parent.TryGetValueMonsters(ID, out IDEntity m))
                {
                    Entity = m;
                    Resolved = true;
                }
            }
            if (!Resolved && !IsStringRef && ID > ModManager.MONSTER_START_ID)
            {
                Parent.Parent.Log("Monster not resolved for: " + this.ID);
            }
        }

        public override string ToString()
        {
            if (Entity != null && Entity.ID != -1)
                IsStringRef = false;
            return base.ToString();
        }
    }
}
