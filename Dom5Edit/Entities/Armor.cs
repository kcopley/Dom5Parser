using Dom5Edit.Commands;
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

        internal override EntityType GetEntityType()
        {
            return EntityType.ARMOR;
        }
    }
}
