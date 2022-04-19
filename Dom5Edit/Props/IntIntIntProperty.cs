using Dom5Edit.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class IntIntIntProperty : Property
    {
        public static Property Create()
        {
            return new IntIntIntProperty();
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
            if (split.Length == 3)
            {
                HasValue = split[0].TryRetrieveNumericFromString(out int val1, out string remainder1);
                if (HasValue) Value1 = val1;
                if (string.IsNullOrEmpty(remainder1))
                {
                    HasValue = split[1].TryRetrieveNumericFromString(out int val2, out string remainder2);
                    if (HasValue) Value2 = val2;
                    if (string.IsNullOrEmpty(remainder2))
                    {
                        HasValue = split[2].TryRetrieveNumericFromString(out int val3, out string remainder3);
                        if (HasValue) Value3 = val3;
                        if (remainder3.Length > 0)
                        {
                            Comment += remainder3;
                        }
                    }
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
                        return s + " " + Value1 + " " + Value2 + " " + Value3 + " -- " + Comment;
                    }
                    else
                    {
                        return s + " -- " + Comment;
                    }
                }
                else
                {
                    if (HasValue)
                    {
                        return s + " " + Value1 + " " + Value2 + " " + Value3;
                    }
                    else
                    {
                        return s;
                    }
                }
            }
            else return "";
        }
    }
}
