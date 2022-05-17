using Dom5Edit.Commands;
using Dom5Edit.Mods;
using Dom5Edit.Props;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Entities
{
    public class Armor : IDEntity
    {
        private static Dictionary<Command, Func<Property>> _propertyMap = new Dictionary<Command, Func<Property>>();

        static Armor()
        {
            _propertyMap.Add(Command.NAME, NameProperty.Create);
            _propertyMap.Add(Command.CLEAR, CommandProperty.Create);
            _propertyMap.Add(Command.COPYARMOR, ArmorRef.Create);
            _propertyMap.Add(Command.TYPE, IntProperty.Create);
            _propertyMap.Add(Command.PROT, IntProperty.Create);
            _propertyMap.Add(Command.DEF, IntProperty.Create);
            _propertyMap.Add(Command.ENC, IntProperty.Create);
            _propertyMap.Add(Command.RCOST, IntProperty.Create);
            _propertyMap.Add(Command.MAGICARMOR, CommandProperty.Create);
            _propertyMap.Add(Command.IRONARMOR, CommandProperty.Create);
            _propertyMap.Add(Command.WOODENARMOR, CommandProperty.Create);
        }

        public Armor(string value, string comment, Mod _parent, bool selected = false) : base(value, comment, _parent, selected)
        {
        }

        public override void Resolve()
        {
            if (base._resolved) return;
            foreach (var m in Parent.Dependencies)
            {
                if (ID != -1 && m.Armors.TryGetValue(this.ID, out var entity))
                {
                    entity.Properties.AddRange(this.Properties);
                }
                else if (this.TryGetName(out _name) && m.NamedArmors.TryGetValue(_name, out var namedentity))
                {
                    namedentity.Properties.AddRange(this.Properties);
                }
            }
            base.Resolve();
        }

        internal override Command GetNewCommand()
        {
            return Command.NEWARMOR;
        }

        internal override Command GetSelectCommand()
        {
            return Command.SELECTARMOR;
        }

        internal override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return _propertyMap;
        }

        internal override Dictionary<string, IDEntity> GetNamedList()
        {
            return Parent.NamedArmors;
        }

        internal override Dictionary<int, IDEntity> GetIDList()
        {
            return Parent.Armors;
        }
    }
}
