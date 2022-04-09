using Dom5Edit.Commands;
using Dom5Edit.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Entities
{
    public class Weapon : IDEntity
    {
        public static Dictionary<Command, Func<Property>> _propertyMap = new Dictionary<Command, Func<Property>>();

        static Weapon()
        {
            //String properties
            //_propertyMap.Add(Command.NAME, StringProperty.Create);
        }

        public List<Property> Properties = new List<Property>();

        public Weapon(string value, string comment)
        {
            this.SetID(value, comment);
        }

        public override void Parse(Command command, string value, string comment)
        {
            if (_propertyMap.TryGetValue(command, out Func<Property> create))
            {
                Property prop = create.Invoke();
                prop.ParentMod = this.Parent; //carry the mod assignation down
                prop.Parse(command, value, comment);
                Properties.Add(prop);
            }
            //else not recognized command, skip
            //build comment storage for in-between properties
        }
    }
}
