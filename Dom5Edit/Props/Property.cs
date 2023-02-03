using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dom5Edit.Props
{
    public abstract class Property
    {
        public Entity Parent { get; set; }
        public string Comment { get; set; }
        public abstract void Parse(Command c, string v, string comment);

        public abstract string ToExportString();

        internal Command _command { get; set; }

        internal abstract Property GetDefault();
    }
}
