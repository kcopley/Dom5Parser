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
    public class IntProperty : Property
    {
        public static Property Create()
        {
            return new IntProperty();
        }

        public static IntProperty Create(Command c, IDEntity parent, int val)
        {
            return new IntProperty() { Command = c, Comment = "", Parent = parent, Value = val };
        }

        public int Value { get; set; } = int.MinValue;

        public override void Parse(Command c, string s, string comment)
        {
            this.Command = c;
            this.Comment = comment;
            var HasValue = s.TryRetrieveNumericFromString(out int val, out string remainder);
            if (HasValue) Value = val;
            if (remainder.Length > 0)
            {
                Comment += remainder;
            }
        }

        //Preliminary Example only for now, not optimal
        public override string ToExportString()
        {
            if (CommandsMap.TryGetString(Command, out string s))
            {
                if (!String.IsNullOrEmpty(Comment))
                {
                    if (Value != int.MinValue)
                    {
                        if (Command == Command.ERA)
                        {
                            return s + " " + 2 + " -- " + Comment;
                        }
                        return s + " " + Value + " -- " + Comment;
                    }
                    else
                    {
                        return s + " -- " + Comment;
                    }
                }
                else
                {
                    if (Value != int.MinValue)
                    {
                        return s + " " + Value;
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
            return new IntProperty() { Value = 10 };
        }

        internal override bool EqualsProperty<T>(T copyFrom)
        {
            if (copyFrom is IntProperty)
            {
                var compare = copyFrom as IntProperty;
                if (this.Command == compare.Command && this.Value == compare.Value) return true;
            }
            return false;
        }
    }
}
