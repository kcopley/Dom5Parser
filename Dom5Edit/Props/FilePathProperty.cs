using Dom5Edit.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class FilePathProperty : Property
    {
        public static Property Create()
        {
            return new FilePathProperty();
        }

        private Command _command { get; set; }
        public string Value { get; set; }

        public override void Parse(Command c, string s, string comment)
        {
            this._command = c;
            this.Value = s;
            this.Comment = comment;
        }

        //Preliminary Example only for now, not optimal
        public override string ToString()
        {
            if (CommandsMap.TryGetString(_command, out string s))
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
    }
}
