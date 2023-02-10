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
    public class FilePathProperty : Property
    {
        public static Property Create()
        {
            return new FilePathProperty();
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

        internal override Property GetDefault()
        {
            return new FilePathProperty() { Value = "" };
        }

        internal override bool EqualsProperty<T>(T copyFrom)
        {
            if (copyFrom is FilePathProperty)
            {
                var compare = copyFrom as FilePathProperty;
                if (this.Command == compare.Command && this.Value == compare.Value) return true;
            }
            return false;
        }
    }
}
