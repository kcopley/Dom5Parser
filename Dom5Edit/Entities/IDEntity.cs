using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Mods;
using Dom5Edit.Props;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Entities
{
    public abstract class IDEntity : Entity
    {
        public List<Property> Properties = new List<Property>();
        public bool Selected { get; set; }
        public bool Named { get; set; }
        internal string _name;

        public IDEntity(string value, string comment, Mod _parent, bool selected = false)
        {
            this.SetID(value, comment);
            Parent = _parent;
            Selected = selected;
            if (ID == -1)
            {
                _name = value;
                Named = true;
                GetNamedList().Add(_name, this);
            }
            else
            {
                if (!GetIDList().ContainsKey(ID)) GetIDList().Add(ID, this);
            }
        }

        protected IDEntity()
        {
        }

        public int ID
        {
            get; set;
        }

        public string IDComment { get; private set; }

        public override void AddNamed(string s)
        {
            if (!GetNamedList().ContainsKey(s)) GetNamedList().Add(s, this);
        }

        public override bool TryGetIDValue(int id, out IDEntity e)
        {
            if (GetIDList().TryGetValue(id, out IDEntity m))
            {
                e = m;
                return true;
            }
            e = null;
            return false;
        }

        public override bool TryGetNamedValue(string s, out IDEntity e)
        {
            if (GetNamedList().TryGetValue(s, out IDEntity m))
            {
                e = m;
                return true;
            }
            e = null;
            return false;
        }

        public virtual void SetID(string s, string comment)
        {
            if (s.TryRetrieveNumericFromString(out int id, out string remainder))
            {
                ID = id;
                if (remainder.Length > 0) comment += remainder;
            }
            else
            {
                ID = -1;
            }
            IDComment = comment;
        }

        private bool _resolved = false;
        public void Resolve()
        {
            if (_resolved) return;
            foreach (Property prop in Properties)
            {
                if (prop is Reference p)
                {
                    p.Resolve();
                }
            }
            _resolved = true;
        }

        internal bool TryGetName(out string name)
        {
            foreach (var prop in Properties)
            {
                if (prop is NameProperty)
                {
                    name = ((NameProperty)prop).Value;
                    return true;
                }
            }
            name = "";
            return false;
        }

        internal abstract Command GetNewCommand();
        internal abstract Command GetSelectCommand();

        internal abstract Dictionary<string, IDEntity> GetNamedList();
        internal abstract Dictionary<int, IDEntity> GetIDList();

        internal abstract Dictionary<Command, Func<Property>> GetPropertyMap();

        public override void Export(StreamWriter writer)
        {
            string endStr = "";
            if (IDComment.Length > 0)
            {
                endStr = " -- " + IDComment;
            }
            if (Selected)
            {
                if (CommandsMap.TryGetString(GetSelectCommand(), out var s1))
                {
                    if (Named)
                    {
                        writer.WriteLine(s1 + " \"" + this._name + "\"" + endStr);
                    }
                    else
                    {
                        writer.WriteLine(s1 + " " + this.ID + endStr);
                    }
                }
            }
            else
            {
                if (CommandsMap.TryGetString(GetNewCommand(), out var s2))
                {
                    if (Named)
                    {
                        writer.WriteLine(s2 + " \"" + this._name + "\"" + endStr);
                    }
                    else if (ID != -1)
                    {
                        writer.WriteLine(s2 + " " + this.ID + endStr);
                    }
                    else
                    {
                        writer.WriteLine(s2 + " " + endStr);
                    }
                }
            }
            foreach (Property p in Properties)
            {
                var write = p.ToString();
                writer.WriteLine(write);
            }
            if (CommandsMap.TryGetString(Command.END, out var s))
            {
                writer.WriteLine(s);
            }
        }

        public override void Parse(Command command, string value, string comment)
        {
            if (GetPropertyMap().TryGetValue(command, out Func<Property> create))
            {
                Property prop = create.Invoke();
                prop.Parent = this; //carry the mod assignation down
                prop.Parse(command, value, comment);
                Properties.Add(prop);
            }
            //else not recognized command, skip
            //build comment storage for in-between properties
        }
    }
}
