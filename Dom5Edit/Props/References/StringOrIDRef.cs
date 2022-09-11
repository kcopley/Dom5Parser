using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public abstract class StringOrIDRef : Reference
    {
        public bool IsStringRef { get; set; } = false;
        public int ID { get; set; }
        public string Name { get; set; }
        public bool HasValue { get; set; }

        public IDEntity Entity { get; set; }
        public bool Resolved { get; set; }

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

            if (HasValue && !Parent.Parent.LineWasTrimmed)
            {
                ID = val;
                IsStringRef = false;
                Comment += remainder;
            }
            else
            {
                HasValue = s.Length > 0;
                if (HasValue)
                {
                    Name = s;
                    IsStringRef = true;
                }
            }
        }

        public override string ToString()
        {
            if (!CommandsMap.TryGetString(_command, out string s)) return "";

            if (IsStringRef)
            {
                string _exportName = Name;
                if (Resolved && Entity.TryGetName(out var _name)) _exportName = _name;

                if (!String.IsNullOrEmpty(Comment))
                {
                    if (HasValue)
                    {
                        return s + " \"" + _exportName + "\" -- " + Comment;
                    }
                    else
                    {
                        return s + " -- " + Comment;
                    }
                }
                else
                {
                    return s + " \"" + _exportName + "\"";
                }
            }
            else
            {
                int _exportID;
                if (Entity != null)
                    _exportID = Resolved ? Entity.ID : ID; //true is left, false is right
                else _exportID = ID;

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
        }
    }
}
