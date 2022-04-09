using Dom5Edit.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class ArmorIDRef : Reference
    {
        public static Property Create()
        {
            return new ArmorIDRef();
        }

        private Command _command { get; set; }
        public int ID { get; set; }
        public bool HasValue { get; set; }

        public override void Parse(Command c, string s, string comment)
        {
            this._command = c;
            this.Comment = comment;
            HasValue = int.TryParse(s, out int val);
            if (HasValue)
            {
                ID = val;
                this.Parent.Parent.AddArmorIDReference(ID, this);
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
                        return s + " " + ID + " -- " + Comment;
                    }
                    else
                    {
                        return s + " -- " + Comment;
                    }
                }
                else
                {
                    return s + " " + ID;
                }
            }
            else return "";
        }
    }
}
