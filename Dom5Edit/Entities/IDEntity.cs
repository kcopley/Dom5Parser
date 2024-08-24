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
        private List<Property> _properties = new List<Property>();
        public IReadOnlyList<Property> Properties => _properties.AsReadOnly();
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

        public string Name
        {
            get
            {
                var exists = TryGet<NameProperty>(Command.NAME, out var np);
                if ((ReturnType.TRUE | ReturnType.COPIED).HasFlag(exists))
                {
                    return np.Value;
                }
                else
                {
                    return "";
                }
            }
            set
            {
                Property prop = Get<NameProperty>(Command.NAME);
                if (prop == null)
                {
                    prop = new StringProperty() { Command = Command.NAME, Value = value };
                    AddProperty(prop);
                }
                var str = prop as StringProperty;
                str.Value = value;
            }
        }

        internal virtual Command GetNewCommand() { throw new NotImplementedException(); }
        internal virtual Command GetSelectCommand() { throw new NotImplementedException(); }
        internal virtual EntityType GetEntityType()
        {
            throw new NotImplementedException();
        }


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

        public IEnumerable<Property> GetAllProperties()
        {
            return Properties;
        }

        public void AddProperty(Property property)
        {
            property.Parent = this;
            _properties.Add(property);
            _properties = _properties.OrderBy(sort_properties).ToList();
        }

        public void AddProperties(List<Property> props)
        {
            foreach (var p in props)
            {
                p.Parent = this;
                _properties.Add(p);
            }
            _properties = _properties.OrderBy(sort_properties).ToList();
        }

        public void RemoveProperty(Property property)
        {
            _properties.Remove(property);
            _properties = _properties.OrderBy(sort_properties).ToList();
        }

        public void ClearProperties()
        {
            _properties.Clear();
        }

        public override void Parse(Command command, string value, string comment)
        {
            if (GetPropertyMap().TryGetValue(command, out Func<Property> create))
            {
                Property prop = create.Invoke();
                prop.Parent = this; //carry the mod assignation down
                prop.Parse(command, value, comment);
                AddProperty(prop);
            }
            else
            {
                this.ParentMod.Log("Invalid, incorrectly spelled, or nonexistent command for " + this.GetType() + " for command: " + command);
            } // not recognized command, skip
            //build comment storage for in-between properties
        }

        public IEnumerable<Property> GetMultiple(Command c)
        {
            return Properties.Where(p => p.Command == c);
        }

        public IEnumerable<Property> GetCommandProperties()
        {
            return Properties.Where(p => p.GetType().Equals(typeof(CommandProperty)));
        }

        public void Set<T>(Command c, Action<T> set) where T : Property, new()
        {
            switch (this.TryGet(c, out T prop))
            {
                case ReturnType.FALSE:
                    var i = this.Create<T>(c);
                    set(i);
                    break;
                case ReturnType.COPIED:
                    var newProp = this.Create<T>(c);
                    set(newProp);
                    if (newProp.Equals(prop))
                    {
                        this.Remove<T>(c);
                    }
                    break;
                case ReturnType.TRUE:
                    var copy = this.TryGetCopyValue<T>(c, out T copyFrom);
                    if (copy == ReturnType.COPIED)
                    {
                        set(prop);
                        if (prop.EqualsProperty(copyFrom))
                        {
                            this.Remove<T>(c);
                        }
                    }
                    else
                    {
                        set(prop);
                    }
                    break;
            }
        }

        public void SetCommand<T>(Command c) where T : Property, new()
        {
            switch (this.TryGet(c, out T prop))
            {
                case ReturnType.FALSE:
                    var i = this.Create<T>(c);
                    break;
                case ReturnType.COPIED:
                    var newProp = this.Create<T>(c);
                    if (newProp.Equals(prop))
                    {
                        this.Remove<T>(c);
                    }
                    break;
                case ReturnType.TRUE:
                    var copy = this.TryGetCopyValue<T>(c, out T copyFrom);
                    if (copy == ReturnType.COPIED)
                    {
                        if (prop.Equals(copyFrom))
                        {
                            this.Remove<T>(c);
                        }
                        this.Remove<T>(c);
                    }
                    break;
            }
        }

        internal T Get<T>(Command c) where T : Property
        {
            return Properties.OfType<T>().FirstOrDefault(p => p.Command == c);
        }

        internal ReturnType Get<T>(Command c, out T t) where T : Property, new()
        {
            t = Get<T>(c);
            return t != null ? ReturnType.TRUE : ReturnType.FALSE;
        }

        public ReturnType TryGet<T>(Command c, out T ret, bool checkCopy = true) where T : Property, new()
        {
            ret = Get<T>(c);
            if (ret != null)
            {
                return ReturnType.TRUE;
            }
            if (checkCopy)
            {
                var copyExists = TryGetCopyFrom(out var copy);
                if (copyExists)
                {
                    var commandExists = copy.TryGet(c, out ret);
                    if (commandExists == ReturnType.TRUE || commandExists == ReturnType.COPIED)
                    {
                        return ReturnType.COPIED;
                    }
                }
            }
            ret = null;
            return ReturnType.FALSE;
        }

        public bool HasCommand<T>(Command c) where T : Property, new()
        {
            return TryGet<T>(c, out T ret) != ReturnType.FALSE;
        }

        public IEnumerable<Property> GetAllPropertiesIncludingCopied()
        {
            var allProperties = new List<Property>(Properties);
            GetCopiedPropertiesRecursively(allProperties, new HashSet<IDEntity> { this });
            return allProperties;
        }

        public IEnumerable<Property> GetOnlyCopiedProperties()
        {
            var copiedProperties = new List<Property>();
            GetCopiedPropertiesRecursively(copiedProperties, new HashSet<IDEntity> { this });
            return copiedProperties;
        }

        private void GetCopiedPropertiesRecursively(List<Property> propertyList, HashSet<IDEntity> visitedEntities)
        {
            if (TryGetCopyFrom(out var copiedEntity) && !visitedEntities.Contains(copiedEntity))
            {
                visitedEntities.Add(copiedEntity);

                foreach (var property in copiedEntity.Properties)
                {
                    if (!propertyList.Any(p => p.Command == property.Command && p.GetType() == property.GetType()))
                    {
                        propertyList.Add(property);
                    }
                }

                copiedEntity.GetCopiedPropertiesRecursively(propertyList, visitedEntities);
            }
        }

        /// <summary>
        /// Gets a property value, checking only entities that are copied from.
        /// </summary>
        /// <typeparam name="T">The property type being checked for.</typeparam>
        /// <param name="c">The command value used to mark a property.</param>
        /// <param name="ret">The returned property, or default (typically null) if not found.</param>
        /// <returns>True if the property exists on a copied from entity, false otherwise.</returns>
        public ReturnType TryGetCopyValue<T>(Command c, out T ret) where T : Property, new()
        {
            var copyExists = TryGetCopyFrom(out var copy);
            if (copyExists)
            {
                var commandExists = copy.TryGet(c, out ret);
                if (commandExists == ReturnType.TRUE || commandExists == ReturnType.COPIED)
                {
                    return ReturnType.COPIED;
                }
            }
            ret = null;
            return ReturnType.FALSE;
        }

        public T Create<T>(Command c) where T : Property, new()
        {
            var ret = new T() { Parent = this, Command = c };
            AddProperty(ret);
            return ret;
        }

        public bool Remove<T>(Command c) where T : Property
        {
            var property = Get<T>(c);
            if (property != null)
            {
                RemoveProperty(property);
                return true;
            }
            return false;
        }

        public void RemoveProperty(Command command)
        {
            var propertyToRemove = _properties.FirstOrDefault(p => p.Command == command);
            if (propertyToRemove != null)
            {
                _properties.Remove(propertyToRemove);
            }
        }

        public virtual bool TryGetCopyFrom(out IDEntity copy)
        {
            throw new NotImplementedException();
        }

        public virtual bool TryGetCopySpr(out IDEntity copySpr)
        {
            throw new NotImplementedException();
        }

        public int sort_properties(Property p)
        {
            switch (p.Command)
            {
                case Command.SELECTMONSTER:
                case Command.NEWMONSTER:
                    return 1;
                case Command.COPYSTATS:
                case Command.COPYSPR:
                    return 2;
                case Command.CLEAR:
                case Command.CLEARWEAPONS:
                case Command.CLEARARMOR:
                case Command.CLEARMAGIC:
                case Command.CLEARSPEC:
                    return 3;
                case Command.NAME:
                case Command.FIXEDNAME:
                    return 4;
                default:
                    return 5;
            }
        }
    }
}
