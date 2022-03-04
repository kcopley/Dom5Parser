using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class Name : Property
    {
        public static string Import { get { return "#name"; } }
        public static Property Create()
        {
            return new Name();
        }

        public string name { get; set; }

        public override void Parse(string s, string comment)
        {
            this.name = s;
            this.Comment = comment;
        }

        //Preliminary Example only for now, not optimal
        public string Export()
        {
            return Name.Import + " " + name + " --" + Comment;
        }
    }
}
