using Dom5Edit.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class Name : Property
    {
        public static Command Import = Command.NAME;
        public static Property Create()
        {
            return new Name();
        }

        public string name { get; set; }

        public override void Parse(Command c, string s, string comment)
        {
            this.name = s;
            this.Comment = comment;
        }

        //Preliminary Example only for now, not optimal
        public string Export()
        {
            if (!String.IsNullOrEmpty(Comment))
            {
                return Name.Import + " " + name + " --" + Comment;
            }
            else
            {
                return Name.Import + " " + name;
            }
        }
    }
}
