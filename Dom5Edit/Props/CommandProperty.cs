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
    public class CommandProperty : Property
    {
        public string DisplayName
        {
            get { return "#" + this._command.ToString().ToLower(); }
        }

        public static Property Create()
        {
            return new CommandProperty();
        }

        public static CommandProperty Create(Command c, IDEntity parent)
        {
            return new CommandProperty() { _command = c, Comment = "", Parent = parent };
        }

        public override void Parse(Command c, string s, string comment)
        {
            this._command = c;
            this.Comment = comment;
        }

        //Preliminary Example only for now, not optimal
        public override string ToExportString()
        {
            if (CommandsMap.TryGetString(_command, out string s))
            {
                if (!String.IsNullOrEmpty(Comment))
                {
                    return s + " -- " + Comment;
                }
                else
                {
                    return s;
                }
            }
            else return "";
        }

        internal override Property GetDefault()
        {
            throw new NotImplementedException();
        }
    }
}
