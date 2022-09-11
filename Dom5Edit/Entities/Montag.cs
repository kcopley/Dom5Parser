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
        public List<IDEntity> ReferencedEntities = new List<IDEntity>();

        public static List<int> MontagConstants = new List<int>()
        {
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            10,
            11,
            12,
            13,
            14,
            15,
            16,
            17,
            18,
        };

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
