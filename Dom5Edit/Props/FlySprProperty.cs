using Dom5Edit.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class FlySprProperty : IntIntProperty
    {
        public new static Property Create()
        {
            return new FlySprProperty();
        }

        public bool HasTwoValues { get; set; }

        public override void Parse(Command c, string s, string comment)
        {
            this._command = c;
            this.Comment = comment;
            s = s.Trim();
            var split = s.Split(' ');
            if (split.Length == 1)
            {
                HasValue = int.TryParse(split[0], out int val1);
                if (HasValue) Value1 = val1;
                HasTwoValues = false;
            }
            else if (split.Length == 2)
            {
                HasValue = int.TryParse(split[0], out int val1);
                if (HasValue) Value1 = val1;
                HasValue = int.TryParse(split[1], out int val2);
                if (HasValue) Value2 = val2;
                HasTwoValues = true;
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
                        if (HasTwoValues)
                        {
                            var ret = s + " " + Value1 + " " + Value2 + " -- " + Comment;
                            return ret;
                        }
                        else
                        {
                            var ret = s + " " + Value1 + " -- " + Comment;
                            return ret;
                        }
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
                        if (HasTwoValues)
                        {
                            var ret = s + " " + Value1 + " " + Value2;
                            return ret;
                        }
                        else
                        {
                            var ret = s + " " + Value1;
                            return ret;
                        }
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
