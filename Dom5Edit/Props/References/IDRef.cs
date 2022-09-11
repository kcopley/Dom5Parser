using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class IDRef : Reference
    {
        public int ID { get; set; }
        public bool HasValue { get; set; }

        public IDEntity entity { get; set; }
        public bool Resolved { get; set; }

        public override bool TryGetEntity(out IDEntity e)
        {
            e = null;
            if (!Resolved) return false;
            if (entity != null)
            {
                e = entity;
                return true;
            }
            return false;
        }

        public override void Parse(Command c, string s, string comment)
        {
            this._command = c;
            this.Comment = comment;
            HasValue = s.TryRetrieveNumericFromString(out int val, out string remainder);
            if (HasValue)
            {
                ID = val;
                if (remainder.Length > 0) Comment += remainder;
            }
        }

        public override void Resolve()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            if (CommandsMap.TryGetString(_command, out string s))
            {
                int _exportID = Resolved ? entity.ID : ID; //true is left, false is right

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
