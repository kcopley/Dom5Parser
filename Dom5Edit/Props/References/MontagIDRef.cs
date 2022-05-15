using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class MontagIDRef : IDRef
    {
        Montag _montag = null;
        bool HasMontagID { get; set; }

        public static Property Create()
        {
            return new MontagIDRef();
        }

        public override void Resolve()
        {
        }

        public override void Parse(Command c, string s, string comment)
        {
            base.Parse(c, s, comment);
            if (c != Command.MONTAG) ID = -ID;
            _montag = this.Parent.Parent.AddMontag(ID);
            HasMontagID = _montag != null;
        }

        public override string ToString()
        {
            if (CommandsMap.TryGetString(_command, out string s))
            {
                int _exportID = _montag != null ? _montag.GetID() : ID; //true is left, false is right

                if (_command != Command.MONTAG)
                {
                    _exportID = -_exportID;
                }

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
