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
    public class StringProperty : Property
    {
        public static Property Create()
        {
            return new StringProperty();
        }

        public string Value { get; set; }

        public override void Parse(Command c, string s, string comment)
        {
            this.Command = c;
            this.Value = s;
            this.Comment = comment;
        }

        //Preliminary Example only for now, not optimal
        public override string ToExportString()
        {
            if (CommandsMap.TryGetString(Command, out string s))
            {
                if (!String.IsNullOrEmpty(Comment))
                {
                    return s + " \"" + Value + "\" -- " + Comment;
                }
                else
                {
                    return s + " \"" + Value + "\"";
                }
            }
            else return "";
        }

        public string ToStringNoQuotes()
        {
            if (CommandsMap.TryGetString(Command, out string s))
            {
                if (!String.IsNullOrEmpty(Comment))
                {
                    return s + " " + Value + " -- " + Comment;
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
            return new StringProperty() { Value = "" };
        }

        internal override bool EqualsProperty<T>(T copyFrom)
        {
            if (copyFrom is StringProperty)
            {
                var compare = copyFrom as StringProperty;
                if (this.Command == compare.Command && this.Value.Equals(compare.Value)) return true;
            }
            return false;
        }
    }
}
