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
    public class IDRef : Reference
    {
        public int ID { get; set; }
        public bool HasValue { get; set; }

        public IDEntity Entity { get; set; }
        public bool Resolved { get; set; }

        internal override EntityType GetEntityType()
        {
            throw new NotImplementedException();
        }

        public override bool TryGetEntity(out IDEntity e)
        {
            e = null;
            if (!Resolved) return false;
            if (Entity != null)
            {
                e = Entity;
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
            if (Parent.ParentMod.TryGet(GetEntityType(), ID, null, out IDEntity e))
            {
                Entity = e;
                Resolved = true;
            }
            //move start ID to be in an entitytype set
            //handle non-resolved separately... these are ones in a dependency?
            if (!Resolved && ID > Parent.ParentMod.GetStartID(GetEntityType()))
            {
                Parent.ParentMod.Log(GetEntityType() + " not resolved for: " + this.ID);
            }
        }

        public override string ToExportString()
        {
            if (CommandsMap.TryGetString(_command, out string s))
            {
                int _exportID = Resolved ? Entity.ID : ID; //true is left, false is right

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
