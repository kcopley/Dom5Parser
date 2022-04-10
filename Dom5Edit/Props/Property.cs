using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public abstract class Property
    {
        public Entity Parent { get; set; }
        public string Comment { get; set; }
        public abstract void Parse(Command c, string v, string comment);

        public override abstract string ToString();

        internal Command _command { get; set; }
    }
}
