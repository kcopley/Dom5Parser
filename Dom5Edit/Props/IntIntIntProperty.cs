using Dom5Edit.Commands;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            this.Command = c;
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
        public override string ToExportString()
        {
            if (CommandsMap.TryGetString(Command, out string s))
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

        internal override Property GetDefault()
        {
            return new IntIntIntProperty() { Value1 = 10, Value2 = 0, Value3 = 0 };
        }

        internal override bool EqualsProperty<T>(T copyFrom)
        {
            if (copyFrom is IntIntIntProperty)
            {
                var compare = copyFrom as IntIntIntProperty;
                if (this.Command == compare.Command && this.Value1 == compare.Value1 && this.Value2 == compare.Value2 && this.Value3 == compare.Value3) return true;
            }
            return false;
        }
    }
}
