using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Entities
{
    [Flags]
    enum SlotType
    {
        NOSLOT = 1,
        //4 hands
        HAND1 = 2,
        HAND2 = 4,
        HAND3 = 8,
        HAND4 = 16,
        HAND5 = 32,
        HAND6 = 64,
        //3 heads
        HEAD1 = 128,
        HEAD2 = 256,
        HEAD3 = 512,
        //1 body
        BODY = 1024,
        //1 feet
        FEET = 2048,
        //4 misc
        MISC1 = 4096,
        MISC2 = 8192,
        MISC3 = 16384,
        MISC4 = 32768,
        MISC5 = 65536,
        MISC6 = 131072,
        //crown only on head
        CROWN = 262144
    }
}
