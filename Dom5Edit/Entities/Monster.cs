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
    public class Monster : IDEntity
    {
        public static Dictionary<Command, Func<Property>> _propertyMap = new Dictionary<Command, Func<Property>>();

        static Monster()
        {
            _propertyMap.Add(Name.Import, Name.Create);
        }

        public List<Property> Properties = new List<Property>();

        public Monster(string value, string comment)
        {
            this.SetID(value, comment);
        }

        public override void Parse(Command command, string value, string comment)
        {
            if (_propertyMap.TryGetValue(command, out Func<Property> create))
            {
                Property prop = create.Invoke();
                prop.Parse(value, comment);
                Properties.Add(prop);
            }
            //else not recognized command, skip
            //build comment storage for in-between properties
        }
    }
}
