using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Entities
{
    public abstract class IDEntity : Entity
    {
        public int ID { get; private set; }
        public string IDComment { get; private set; }
        
        public virtual void SetID(string s, string comment)
        {
            if (int.TryParse(s, out int id)) ID = id;
            else ID = -1;
            IDComment = comment;
        }
    }
}
