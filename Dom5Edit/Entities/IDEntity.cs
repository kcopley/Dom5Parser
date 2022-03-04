using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Data
{
    public class IDEntity
    {
        public int ID { get; set; }

        public virtual void Parse(string[] s) { } //does nothing here

        
    }
}
