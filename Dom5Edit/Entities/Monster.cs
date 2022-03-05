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
        public static string Import { get { return "#newmonster"; } }
        public static Dictionary<string, Func<Property>> References = null;

        public List<Property> properties = new List<Property>();

        public Monster()
        {
            if (References == null)
            {
                References = new Dictionary<string, Func<Property>>();

                References.Add(Name.Import, Name.Create);
            }
        }

        public override void Parse(string command, string value, string comment)
        {
            if (command.EqualsIgnoreCase(Import))
            {
                SetID(value);
            }
            if (References.ContainsKey(command))
            {
                Property prop = References[command]?.Invoke();
                if (prop != null)
                {
                    properties.Add(prop);
                    if (value != "")
                    {
                        prop.Parse(value, comment);
                    }
                }
            }
        }

        public static IDEntity Create()
        {
            return new Monster();
        }

        public static string GetImport()
        {
            return Import;
        }
    }
}
