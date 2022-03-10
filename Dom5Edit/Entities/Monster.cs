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
            _propertyMap.Add(Command.NAME, StringProperty.Create);
            _propertyMap.Add(Command.SPR1, StringProperty.Create);
            _propertyMap.Add(Command.SPR2, StringProperty.Create);

            _propertyMap.Add(Command.HP, IntProperty.Create);
            _propertyMap.Add(Command.DARKVISION, IntProperty.Create);
            _propertyMap.Add(Command.FIRERES, IntProperty.Create);
            _propertyMap.Add(Command.MR, IntProperty.Create);
            _propertyMap.Add(Command.MOR, IntProperty.Create);
            _propertyMap.Add(Command.STR, IntProperty.Create);
            _propertyMap.Add(Command.ATT, IntProperty.Create);
            _propertyMap.Add(Command.DEF, IntProperty.Create);
            _propertyMap.Add(Command.PREC, IntProperty.Create);
            _propertyMap.Add(Command.AP, IntProperty.Create);
            _propertyMap.Add(Command.MAPMOVE, IntProperty.Create);
            _propertyMap.Add(Command.ENC, IntProperty.Create);
            _propertyMap.Add(Command.SIZE, IntProperty.Create);
            _propertyMap.Add(Command.MAXAGE, IntProperty.Create);
            _propertyMap.Add(Command.HUMANOID, IntProperty.Create);
            _propertyMap.Add(Command.ITEMSLOTS, IntProperty.Create);
            _propertyMap.Add(Command.GCOST, IntProperty.Create);
            _propertyMap.Add(Command.RCOST, IntProperty.Create);
            _propertyMap.Add(Command.RPCOST, IntProperty.Create);
            _propertyMap.Add(Command.NAMETYPE, IntProperty.Create);
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
                prop.Parse(command, value, comment);
                Properties.Add(prop);
            }
            //else not recognized command, skip
            //build comment storage for in-between properties
        }
    }
}
