using Dom5Edit.Commands;
using Dom5Edit.Mods;
using Dom5Edit.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Entities
{
    public class Montag
    {
        public int MontagID { get; set; }
        public Montag DependentMontag;

        public Montag(int ID)
        {
            this.MontagID = ID;
        }

        public int GetID()
        {
            if (DependentMontag != null) return DependentMontag.MontagID;
            return MontagID;
        }
    }
}
