using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return new IntProperty() { _command = c, Comment = "", Parent = parent, Value = val, HasValue = true };
        }

        public int Value { get; set; }
        public bool HasValue { get; set; }

        public override void Parse(Command c, string s, string comment)
        {
            this._command = c;
            this.Comment = comment;
            HasValue = s.TryRetrieveNumericFromString(out int val, out string remainder);
            if (HasValue) Value = val;
            if (remainder.Length > 0)
            {
                Comment += remainder;
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
                        if (_command == Command.ERA)
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
                    if (HasValue)
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
    }
}
