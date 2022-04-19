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
    public class Poptype : IDEntity
    {
        private static Dictionary<Command, Func<Property>> _propertyMap = new Dictionary<Command, Func<Property>>();

        static Poptype()
        {
            //_propertyMap.Add(Command.SELECTPOPTYPE, PoptypeIDRef.Create);
            //_propertyMap.Add(Command.END, CommandProperty.Create);
            _propertyMap.Add(Command.CLEARREC, CommandProperty.Create);
            _propertyMap.Add(Command.CLEARDEF, CommandProperty.Create);
            _propertyMap.Add(Command.ADDRECUNIT, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.ADDRECCOM, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.DEFCOM1, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.DEFUNIT1, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.DEFUNIT1B, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.DEFUNIT1C, MonsterOrMontagRef.Create);
            _propertyMap.Add(Command.DEFMULT1, IntProperty.Create);
            _propertyMap.Add(Command.DEFMULT1B, IntProperty.Create);
            _propertyMap.Add(Command.DEFMULT1C, IntProperty.Create);
        }

        public Poptype(string value, string comment, Mod _parent, bool selected = false) : base(value, comment, _parent, selected)
        {
        }

        internal override Command GetNewCommand()
        {
            throw new NotImplementedException();
        }

        internal override Command GetSelectCommand()
        {
            return Command.SELECTPOPTYPE;
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
            return Parent.Poptypes;
        }
    }
}
