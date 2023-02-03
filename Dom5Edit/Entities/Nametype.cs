using Dom5Edit.Commands;
using Dom5Edit.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Entities
{
    public class Nametype : IDEntity
    {
        public static Dictionary<Command, Func<Property>> _propertyMap = new Dictionary<Command, Func<Property>>();

        static Nametype()
        {
            //String properties
            _propertyMap.Add(Command.ADDNAME, StringProperty.Create);
            _propertyMap.Add(Command.CLEAR, CommandProperty.Create);
        }

        internal override Command GetNewCommand()
        {
            throw new NotImplementedException();
        }

        internal override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return _propertyMap;
        }

        internal override Command GetSelectCommand()
        {
            return Command.SELECTNAMETYPE;
        }

        internal override EntityType GetEntityType()
        {
            return EntityType.NAMETYPE;
        }
    }
}
