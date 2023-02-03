using Dom5Edit.Commands;
using Dom5Edit.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Entities
{
    public class DependentEntitySet : Dictionary<int, DependentEntity>
    {
        public bool ID_DOWN = false;
        public int START_ID { get; set; }
        public int END_ID { get; set; } = -1;
        private int CURRENT_ID { get; set; } = -1;

        public void Resolve(EntityType t, List<Mod> dependencies)
        {
            foreach (var kvp in this)
            {
                foreach (var m in dependencies)
                {
                    if (m.Dependents[t].ContainsKey(kvp.Key))
                    {
                        kvp.Value.Dependent = m.Dependents[t][kvp.Key];
                        break;
                    }
                }
            }
        }

        private int GetNextID()
        {
            if (CURRENT_ID < START_ID) CURRENT_ID = START_ID;
            //very crude search unfortunately, but should be fine for our purposes
            while (this.ContainsKey(CURRENT_ID))
            {
                if (!ID_DOWN) CURRENT_ID++;
                else CURRENT_ID--;
            }
            if (END_ID != -1 && CURRENT_ID > END_ID)
            {
            }
            return CURRENT_ID;
        }

        public void Merge(EntityType et, DependentEntitySet items)
        {
            foreach (DependentEntity item in items.Values)
            {
                if (et == EntityType.MONTAG && Montag.MontagConstants.Contains(item.ID)) continue;
                if (item.Dependent == null)
                {
                    item.ID = this.GetNextID();
                    this.Add(item.GetID(), item);
                }
            }
        }
    }
}
