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
    public class StringOrIDRef : Reference
    {
        public bool IsStringRef { get; set; } = false;

        private int _id;
        public int ID
        {
            get
            {
                if (Entity != null)
                {
                    return Entity.ID;
                }
                else return _id;
            }
            set
            {
                _id = value;
                if (Parent.ParentMod.IsLoaded)
                {
                    Resolve();
                }
            }
        }
        private string _name;
        public string Name
        {
            get
            {
                if (Entity != null)
                {
                    return Entity.Name;
                }
                else return _name;
            }
            set
            {
                _name = value;
                if (Parent.ParentMod.IsLoaded)
                {
                    Resolve();
                }
            }
        }

        public bool HasValue { get; set; }

        public IDEntity Entity { get; set; }
        public bool Resolved { get; set; }

        internal override EntityType GetEntityType()
        {
            throw new NotImplementedException();
        }

        public override void Resolve()
        {
            if (Parent.ParentMod.TryGet(GetEntityType(), ID, Name, out IDEntity e))
            {
                Entity = e;
                Resolved = true;
            }
            //move start ID to be in an entitytype set
            //handle non-resolved separately... these are ones in a dependency?
            if (!Resolved && !IsStringRef && ID > Parent.ParentMod.GetStartID(GetEntityType()))
            {
                Parent.ParentMod.Log(GetEntityType() + " not resolved for: " + this.ID);
            }
        }

        public void SetEntity(string value)
        {
            Parse(this.Command, value, "");
            this.Resolve();
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
            this.Command = c;
            this.Comment = comment;

            HasValue = s.TryRetrieveNumericFromString(out int val, out string remainder);

            if (HasValue && !Parent.ParentMod.LineWasTrimmed)
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

        public List<Command> _StringExported = new List<Command>()
        {
            Command.STARTSITE,
            Command.SPELL,
            Command.AUTOSPELL,
        };

        public override string ToExportString()
        {
            if (!CommandsMap.TryGetString(Command, out string s)) return "";

            //add certain types to be string refs?
            if (Entity != null && Entity.ID != -1)
                IsStringRef = false;

            if (_StringExported.Contains(Command))
            {
                IsStringRef = true;
            }

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

        internal override bool EqualsProperty<T>(T copyFrom)
        {
            if (copyFrom is StringOrIDRef)
            {
                var compare = copyFrom as StringOrIDRef;
                if (this.Command == compare.Command && this.Entity == compare.Entity) return true;
            }
            return false;
        }
    }
}
