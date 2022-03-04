using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public abstract class Property
    {
        public string Comment { get; set; }
        public abstract void Parse(string v, string comment);
    }
}
