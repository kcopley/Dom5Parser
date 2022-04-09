using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class MonIDRef : Reference
    {
        public static Property Create()
        {
            return new MonIDRef();
        }

        private Command _command { get; set; }
        public int ID { get; set; }
        public bool HasValue { get; set; }
        public Monster Monster { get; set; }
        public bool Resolved { get; set; }

        public override void Parse(Command c, string s, string comment)
        {
            this._command = c;
            this.Comment = comment;
            HasValue = int.TryParse(s, out int val);
            if (HasValue)
            {
                ID = val;
            }
        }

        public override void Resolve()
        {
            if (Parent.Parent.Monsters.TryGetValue(ID, out var mon))
            {
                Resolved = true;
                Monster = mon;
            }
        }

        //Preliminary Example only for now, not optimal
        public override string ToString()
        {
            if (CommandsMap.TryGetString(_command, out string s))
            {
                int _exportID = ID;
                if (Resolved) _exportID = Monster.ID;

                if (!String.IsNullOrEmpty(Comment))
                {
                    if (HasValue)
                    {
                        return s + " " + _exportID + " -- " + Comment;
                    }
                    else
                    {
                        return s + " -- " + Comment;
                    }
                }
                else
                {
                    return s + " " + _exportID;
                }
            }
            else return "";
        }
    }
}
