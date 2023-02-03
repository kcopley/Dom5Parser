using Dom5Edit.Commands;
using Dom5Edit.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Entities
{
    public class DependentEntity
    {
        public int ID { get; set; }
        public DependentEntity Dependent;
        public List<IDEntity> ReferencedEntities = new List<IDEntity>();

        public DependentEntity(int ID)
        {
            this.ID = ID;
        }

        public int GetID()
        {
            if (Dependent != null) return Dependent.ID;
            return ID;
        }
    }
}
