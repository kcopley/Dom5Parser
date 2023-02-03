using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            if (Parent is IDEntity)
            {
                if (((IDEntity)Parent).ID == 1011)
                {
                    int a = 0;
                    a++;
                }
            }
            this._command = c;
            this.Comment = comment;
            s = s.Trim();
            var split = s.Split(' ');
            if (split.Length == 1)
            {
                HasValue = split[0].TryRetrieveNumericFromString(out int val1, out string remainder);
                if (HasValue) Value1 = val1;
                HasTwoValues = false;
                if (remainder.Length > 0) Comment += remainder;
            }
            else if (split.Length >= 2)
            {
                HasValue = split[0].TryRetrieveNumericFromString(out int val1, out string remainder);
                if (HasValue) Value1 = val1;
                if (string.IsNullOrEmpty(remainder))
                {
                    HasValue = split[1].TryRetrieveNumericFromString(out int val2, out string remainder2);
                    if (HasValue) Value2 = val2;
                    if (remainder2.Length > 0) Comment += remainder2;
                }
                else
                {
                    Value2 = 1;
                }
                HasTwoValues = true;
            }
            else
            {
                HasValue = false;
            }
        }

        //Preliminary Example only for now, not optimal
        public override string ToExportString()
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

        internal override Property GetDefault()
        {
            return new IntIntIntProperty() { Value1 = 10, Value2 = 0 };
        }
    }
}
