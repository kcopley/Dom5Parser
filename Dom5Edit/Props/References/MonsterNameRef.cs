﻿using Dom5Edit.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class MonsterNameRef : Reference
    {
        public static Property Create()
        {
            return new MonsterNameRef();
        }

        private Command _command { get; set; }
        public string Name { get; set; }
        public bool HasValue { get; set; }

        public override void Parse(Command c, string s, string comment)
        {
            this._command = c;
            this.Comment = comment;
            HasValue = s.Length > 0;
            if (HasValue)
            {
                Name = s;
                this.Parent.Parent.AddMonsterNameReference(Name, this);
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
                        return s + " " + Name + " -- " + Comment;
                    }
                    else
                    {
                        return s + " -- " + Comment;
                    }
                }
                else
                {
                    return s + " " + Name;
                }
            }
            else return "";
        }
    }
}
