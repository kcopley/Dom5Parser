using Dom5Edit.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class IntIntProperty : Property
    {
        public static Property Create()
        {
            return new IntIntProperty();
        }

        public int Value1 { get; set; }

        public int Value2 { get; set; }

        public int Value3 { get; set; }
        public bool HasValue { get; set; }

        public override void Parse(Command c, string s, string comment)
        {
            this._command = c;
            this.Comment = comment;
            s = s.Trim();
            var split = s.Split(' ');
            if (split.Length == 2)
            {
                HasValue = int.TryParse(split[0], out int val1);
                if (HasValue) Value1 = val1;
                HasValue = int.TryParse(split[1], out int val2);
                if (HasValue) Value2 = val2;
            }
            else
            {
                HasValue = false;
            }
        }

        //Preliminary Example only for now, not optimal
        public override string ToString()
        {
            if (CommandsMap.TryGetString(_command, out string s))
            {
                if (!String.IsNullOrEmpty(Comment))
                {
                    if (HasValue)
                    {
                        var ret = s + " " + Value1 + " " + Value2 + " -- " + Comment;
                        return ret;
                    }
                    else
                    {
                        return s + " -- " + Comment;
                    }
                }
                else
                {
                    return s + " " + Value1 + " " + Value2;
                }
            }
            else return "";
        }
    }
}
