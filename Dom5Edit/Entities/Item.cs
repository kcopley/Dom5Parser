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
    public class Item : IDEntity
    {
        public static Dictionary<Command, Func<Property>> _propertyMap = new Dictionary<Command, Func<Property>>();

        static Item()
        {
            //String properties
            //_propertyMap.Add(Command.NAME, StringProperty.Create);
        }

        public Item(string value, string comment, Mod _parent, bool selected = false) : base(value, comment, _parent, selected)
        {
        }

        internal override Command GetNewCommand()
        {
            return Command.NEWITEM;
        }

        internal override Command GetSelectCommand()
        {
            return Command.SELECTITEM;
        }

        internal override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return _propertyMap;
        }

        internal override Dictionary<string, IDEntity> GetNamedList()
        {
            return Parent.NamedItems;
        }

        internal override Dictionary<int, IDEntity> GetIDList()
        {
            return Parent.Items;
        }
    }
}
