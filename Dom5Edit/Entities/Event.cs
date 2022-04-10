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
    public class Event : IDEntity
    {
        public static Dictionary<Command, Func<Property>> _propertyMap = new Dictionary<Command, Func<Property>>();

        static Event()
        {
            //String properties
            //_propertyMap.Add(Command.NAME, StringProperty.Create);
        }

        public Event(string value, string comment, Mod _parent, bool selected = false) : base(value, comment, _parent, selected)
        {
        }

        internal override Command GetNewCommand()
        {
            return Command.NEWEVENT;
        }

        internal override Command GetSelectCommand()
        {
            throw new NotImplementedException();
        }

        internal override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return _propertyMap;
        }

        internal override Dictionary<string, IDEntity> GetNamedList()
        {
            throw new NotImplementedException();
        }

        internal override Dictionary<int, IDEntity> GetIDList()
        {
            return Parent.Events;
        }
    }
}
