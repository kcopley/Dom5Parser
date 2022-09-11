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
    public class Enchantment
    {
        public int EnchID { get; set; }
        public Enchantment DependentEnchantment;
        public List<IDEntity> ReferencedEntities = new List<IDEntity>();

        public Enchantment(int ID)
        {
            this.EnchID = ID;
        }

        public int GetID()
        {
            if (DependentEnchantment != null) return DependentEnchantment.EnchID;
            return EnchID;
        }
    }
}
