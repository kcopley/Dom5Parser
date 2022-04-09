using Dom5Edit.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class CommandProperty : Property
    {
        public static Property Create()
        {
            return new CommandProperty();
        }

        private Command _command { get; set; }

        public override void Parse(Command c, string s, string comment)
        {
            this._command = c;
            this.Comment = comment;
        }

        //Preliminary Example only for now, not optimal
        public override string ToString()
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
    }
}
