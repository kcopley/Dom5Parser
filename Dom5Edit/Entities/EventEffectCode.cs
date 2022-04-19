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
    public class EventEffectCode
    {
        public int EventEffectCodeID { get; set; }

        public EventEffectCode(int ID)
        {
            this.EventEffectCodeID = ID;
        }
    }
}
