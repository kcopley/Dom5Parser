using Dom5Edit.Props;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Data
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

        public override void Parse(string[] line)
        {
            if (References.ContainsKey(line[0]))
            {
                Property prop = References[line[0]]?.Invoke();
                if (prop != null)
                {
                    properties.Add(prop);
                    if (line.Length > 1)
                    {
                        int commentIndex = line[1].IndexOf("--");
                        if (commentIndex == -1) {
                            prop.Parse(line[1], ""); //no comment included
                        }
                        else
                        {
                            string initial = line[1].Substring(0, commentIndex);
                            string comment = line[1].Substring(commentIndex + 2);
                            prop.Parse(initial, comment);
                        }
                    }
                }
            }
        }

        public static IDEntity Create()
        {
            return new Monster();
        }
    }
}
