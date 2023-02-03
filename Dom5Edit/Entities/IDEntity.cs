using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Entities
{
    public class IDEntity : Entity
    {
        public List<Property> Properties = new List<Property>();
        public HashSet<Nation> AssociatedNations = new HashSet<Nation>();
        public bool Selected { get; set; }
        public bool Named { get; set; }
        internal string _name;

        private IDEntity _dependent = null;
        internal IDEntity DependentEntity { get { return _dependent; } }

        internal virtual void Assign(string value, string comment, Mod _parent, bool selected = false)
        {
            this.SetID(value, comment);
            ParentMod = _parent;
            Selected = selected;
            ParentMod.AddEntity(GetType(), ID, _name, this);
        }

        private int _id;
        public int ID
        {
            get
            {
                if (DependentEntity != null) return DependentEntity.ID;
                else return _id;
            }
            set
            {
                _id = value;
            }
        }

        public string IDComment { get; private set; } = "";

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
                if (!string.IsNullOrEmpty(s))
                {
                    _name = s;
                    Named = true;
                }
            }
            IDComment = comment;
        }

        internal bool _resolved = false;
        public virtual void Resolve()
        {
            if (_resolved) return;
            foreach (var m in ParentMod.Dependencies)
            {
                if (m.TryGet(this.GetEntityType(), ID, _name, out IDEntity entity))
                {
                    _dependent = entity;
                }
            }
            foreach (Property prop in Properties)
            {
                if (prop is Reference p)
                {
                    p.Resolve();
                }
            }
            _resolved = true;
        }

        public virtual void Map()
        {
            foreach (Property prop in this.Properties)
            {
                if (prop is Reference)
                {
                    Reference r = prop as Reference;
                    r.Connect(this);
                }
            }
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

        internal virtual Command GetNewCommand() { throw new NotImplementedException(); }
        internal virtual Command GetSelectCommand() { throw new NotImplementedException(); }
        internal virtual EntityType GetEntityType() { throw new NotImplementedException(); }


        internal virtual Dictionary<Command, Func<Property>> GetPropertyMap() { throw new NotImplementedException(); }

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
                var write = p.ToExportString();
                writer.WriteLine(write);
            }
            if (CommandsMap.TryGetString(Command.END, out var s))
            {
                writer.WriteLine(s);
            }
        }

        public static IDEntity SelectVanillaEntity<T>(int id, Mod m) where T : IDEntity, new()
        {
            var ret = new T();
            ret.Assign(id.ToString(), "", m, true);
            return ret;
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
            else
            {
                this.ParentMod.Log("Invalid, incorrectly spelled, or nonexistent command for " + this.GetType() + " for command: " + command);
            } // not recognized command, skip
            //build comment storage for in-between properties
        }

        public IEnumerable<Property> GetMultiple(Command c)
        {
            var prop = this.Properties.FindAll(
                    delegate (Property p)
                    {
                        return p._command == c;
                    });
            return prop;
        }

        public IEnumerable<Property> GetCommandProperties()
        {
            var prop = this.Properties.FindAll(
                    delegate (Property p)
                    {
                        return p.GetType().Equals(typeof(CommandProperty));
                    });
            return prop;
        }

        internal Property Get(Command c)
        {
            var prop = this.Properties.Find(
                    delegate (Property p)
                    {
                        return p._command == c;
                    });
            return prop;
        }

        internal ReturnType Get<T>(Command c, out T t) where T : Property, new()
        {
            var ret = Get(c);
            if (ret != null)
            {
                t = ret as T;
                return ReturnType.TRUE;
            }
            else
            {
                t = new T().GetDefault() as T;
                return ReturnType.FALSE;
            }
        }

        public ReturnType TryGet<T>(Command c, out T ret) where T : Property, new()
        {
            var exists = Get<T>(c, out ret);
            if (exists == ReturnType.TRUE)
            {
                return ReturnType.TRUE;
            }
            if (exists == ReturnType.FALSE)
            {
                var copyExists = CopyFrom.TryGet<T>(c, out ret);
                if (copyExists == ReturnType.TRUE || copyExists == ReturnType.COPIED)
                {
                    return ReturnType.COPIED;
                }
            }
            ret = new T().GetDefault() as T;
            return ReturnType.FALSE;
        }

        public ReturnType TryGetCopyValue<T>(Command c, out T ret) where T : Property, new()
        {
            var exists = Get<T>(c, out ret);
            var copyExists = CopyFrom.TryGet<T>(c, out ret);
            if (copyExists == ReturnType.TRUE || copyExists == ReturnType.COPIED)
            {
                return ReturnType.COPIED;
            }
            ret = new T().GetDefault() as T;
            return ReturnType.FALSE;
        }

        public T Create<T>(Command c) where T : Property, new()
        {
            var ret = new T() { Parent = this, _command = c };
            return ret;
        }

        public ReturnType TryGetSingular(Command c, out Property prop)
        {
            prop = this.Properties.Find(
                    delegate (Property p)
                    {
                        return p._command == c;
                    });
            if (prop != null) return ReturnType.TRUE;
            if (prop == null && CopyFrom != null)
            {
                if (CopyFrom.TryGetSingular(c, out prop) != ReturnType.FALSE)
                {
                    return ReturnType.COPIED;
                }
            }
            return ReturnType.FALSE;
        }

        public void Add(Property p)
        {
            p.Parent = this;
            this.Properties.Add(p);
        }
    }
}
