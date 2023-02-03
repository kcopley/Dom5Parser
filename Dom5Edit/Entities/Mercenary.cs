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
    public class Mercenary : IDEntity
    {
        private static Dictionary<Command, Func<Property>> _propertyMap = new Dictionary<Command, Func<Property>>();

        static Mercenary()
        {
            _propertyMap.Add(Command.NAME, NameProperty.Create);
            _propertyMap.Add(Command.LEVEL, IntProperty.Create);
            _propertyMap.Add(Command.BOSSNAME, StringProperty.Create);
            _propertyMap.Add(Command.COM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.UNIT, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.NRUNITS, IntProperty.Create);
            _propertyMap.Add(Command.MINMEN, IntProperty.Create);
            _propertyMap.Add(Command.MINPAY, IntProperty.Create);
            _propertyMap.Add(Command.XP, IntProperty.Create);
            _propertyMap.Add(Command.RANDEQUIP, IntProperty.Create);
            _propertyMap.Add(Command.RECRATE, IntProperty.Create);
            _propertyMap.Add(Command.ITEM, ItemRef.Create);
            _propertyMap.Add(Command.ERAMASK, IntProperty.Create);
        }

        internal override Command GetNewCommand()
        {
            return Command.NEWMERC;
        }

        internal override Command GetSelectCommand()
        {
            throw new NotImplementedException();
        }

        internal override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return _propertyMap;
        }

        internal override EntityType GetEntityType()
        {
            return EntityType.MERCENARY;
        }
    }
}
