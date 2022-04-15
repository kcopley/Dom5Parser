using Dom5Edit.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class BitmaskChanceProperty : Property
    {
        public static Property Create()
        {
            return new BitmaskChanceProperty();
        }

        public ulong Bitmask { get; set; }

        public int Chance { get; set; }
        public bool HasValue { get; set; }

        public override void Parse(Command c, string s, string comment)
        {
            this._command = c;
            this.Comment = comment;
            s = s.Trim();
            var split = s.Split(' ');
            if (split.Length == 2)
            {
                HasValue = split[0].TryRetrieveUlongFromString(out ulong val, out string remainder);
                if (HasValue) Bitmask = val;
                if (remainder.Length == 0)
                {
                    HasValue = split[1].TryRetrieveNumericFromString(out int val2, out string remainder2);
                    if (HasValue) Chance = val2;
                    if (remainder2.Length > 0) comment += remainder2;
                }
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
                        return s + " " + Bitmask + " " + Chance + " -- " + Comment;
                    }
                    else
                    {
                        return s + " -- " + Comment;
                    }
                }
                else
                {
                    return s + " " + Bitmask + " " + Chance;
                }
            }
            else return "";
        }
    }
}
