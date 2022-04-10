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
    public class Ench : IDEntity
    {
        public static Dictionary<Command, Func<Property>> _propertyMap = new Dictionary<Command, Func<Property>>();

        static Ench()
        {
            //String properties
            //_propertyMap.Add(Command.NAME, StringProperty.Create);
        }

        public Ench(string value, string comment, Mod _parent, bool selected = false) : base(value, comment, _parent, selected)
        {
        }

        internal override Dictionary<int, IDEntity> GetIDList()
        {
            throw new NotImplementedException();
        }

        internal override Dictionary<string, IDEntity> GetNamedList()
        {
            throw new NotImplementedException();
        }

        internal override Command GetNewCommand()
        {
            throw new NotImplementedException();
        }

        internal override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            throw new NotImplementedException();
        }

        internal override Command GetSelectCommand()
        {
            throw new NotImplementedException();
        }
    }
}
