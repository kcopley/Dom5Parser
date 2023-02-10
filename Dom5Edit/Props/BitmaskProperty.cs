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
    public class BitmaskProperty : Property
    {
        public static Property Create()
        {
            return new BitmaskProperty();
        }

        public ulong Value { get; set; }
        public bool HasValue { get; set; }

        public override void Parse(Command c, string s, string comment)
        {
            this.Command = c;
            this.Comment = comment;
            HasValue = s.TryRetrieveUlongFromString(out ulong val, out string remainder);
            if (HasValue) Value = val;
            if (remainder.Length > 0) Comment += remainder;
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
                        return s + " " + Value + " -- " + Comment;
                    }
                    else
                    {
                        return s + " -- " + Comment;
                    }
                }
                else
                {
                    return s + " " + Value;
                }
            }
            else return "";
        }
        internal override Property GetDefault()
        {
            return new BitmaskProperty() { Value = 0 };
        }

        internal override bool EqualsProperty<T>(T copyFrom)
        {
            if (copyFrom is BitmaskProperty)
            {
                var compare = copyFrom as BitmaskProperty;
                if (this.Command == compare.Command && this.Value == compare.Value) return true;
            }
            return false;
        }
    }
}
