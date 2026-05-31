using Dom5Edit.Commands;
using Dom5Edit.Props;
using Dom5Edit.Validation;

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

            // Only link to vanilla/dependency entities for SELECTED entities (modifying existing)
            // NEW entities should not inherit from vanilla just because they share a name
            if (Selected)
            {
                foreach (var m in ParentMod.Dependencies)
                {
                    if (m.Database.TryGetValue(this.GetEntityType(), out var entitySet))
                    {
                        IDEntity entity = null;
                        bool found = false;

                        if (_id > 0)
                        {
                            // Selected by ID (e.g., #selectweapon 865) - look up by ID only
                            found = entitySet.TryGetValue(_id, out entity);
                        }
                        else if (!string.IsNullOrEmpty(_name))
                        {
                            // Selected by name (e.g., #selectweapon "Stun") - look up by name only
                            found = entitySet.TryGetValueNamed(_name, out entity);
                        }

                        if (found && entity != null)
                        {
                            _dependent = entity;
                            break; // Found it, stop looking
                        }
                    }
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
                if (exists == ReturnType.TRUE || exists == ReturnType.COPIED)
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

        /// <summary>
        /// Override GetName to return the entity's Name property for DisplayName.
        /// </summary>
        public override string GetName()
        {
            return Name;
        }

        internal virtual Command GetNewCommand() { throw new NotImplementedException(); }
        internal virtual Command GetSelectCommand() { throw new NotImplementedException(); }
        internal virtual EntityType GetEntityType()
        {
            throw new NotImplementedException();
        }


        public virtual Dictionary<Command, Func<Property>> GetPropertyMap() { throw new NotImplementedException(); }

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

            // Check if this is a clear command - if so, remove affected properties
            if (PropertyGroupMap.IsClearCommand(property.Command))
            {
                ApplyClearCommand(property);
            }
            // Check if this is a copy command - if so, remove properties that will be overwritten
            else if (PropertyGroupMap.IsFullCopyCommand(property.Command))
            {
                ApplyCopyCommand(property);
            }

            _properties.Add(property);
            _properties = _properties.OrderBy(sort_properties).ToList();
        }

        /// <summary>
        /// Handles a clear command by removing all properties in the affected group.
        /// This ensures that only properties added AFTER the clear command are retained.
        /// Also removes any previous clear command of the same type.
        /// </summary>
        private void ApplyClearCommand(Property clearProperty)
        {
            var groupToClear = PropertyGroupMap.GetGroupClearedBy(clearProperty.Command);
            if (!groupToClear.HasValue) return;

            var group = groupToClear.Value;
            var clearedProperties = new List<Property>();

            // Find all properties that will be cleared
            foreach (var prop in _properties.ToList())
            {
                // Remove previous clear command of the same type
                if (prop.Command == clearProperty.Command)
                {
                    _properties.Remove(prop);
                    continue;
                }

                // Check if this property belongs to the group being cleared
                var propGroup = GetPropertyGroup(prop.Command);
                bool shouldClear = false;

                if (group == PropertyGroup.All)
                {
                    // #clear removes everything except identity/structural commands
                    shouldClear = propGroup != PropertyGroup.None;
                }
                else
                {
                    // Specific clear - only remove matching group
                    shouldClear = propGroup == group;
                }

                if (shouldClear)
                {
                    clearedProperties.Add(prop);
                    _properties.Remove(prop);
                }
            }

            // Log a parse issue if any properties were actually cleared
            if (clearedProperties.Count > 0)
            {
                var clearedCommands = string.Join(", ", clearedProperties.Select(p =>
                    CommandsMap.TryGetString(p.Command, out var s) ? s : p.Command.ToString()));
                var clearCmdStr = CommandsMap.TryGetString(clearProperty.Command, out var cs) ? cs : clearProperty.Command.ToString();
                var message = $"{clearCmdStr} at line {clearProperty.LineNumber} cleared {clearedProperties.Count} previously defined property(s): {clearedCommands}";
                ParentMod?.AddParseIssue(ParseIssueType.PropertiesClearedBySubsequentClear, message, clearProperty.LineNumber);
            }
        }

        /// <summary>
        /// Handles a copy command by removing all properties that will be overwritten.
        /// This ensures that only properties added AFTER the copy command are retained.
        /// Also removes any previous copy command of the same type.
        /// </summary>
        private void ApplyCopyCommand(Property copyProperty)
        {
            var groupsToOverwrite = PropertyGroupMap.GetGroupsOverwrittenByCopy(copyProperty.Command);
            if (groupsToOverwrite.Count == 0) return;

            var overwrittenProperties = new List<Property>();
            bool coversAll = groupsToOverwrite.Contains(PropertyGroup.All);

            // Find all properties that will be overwritten
            foreach (var prop in _properties.ToList())
            {
                // Remove previous copy command of the same type
                if (prop.Command == copyProperty.Command)
                {
                    _properties.Remove(prop);
                    continue;
                }

                // Skip other copy/clear commands - they're structural
                if (PropertyGroupMap.IsClearCommand(prop.Command) || PropertyGroupMap.IsFullCopyCommand(prop.Command))
                    continue;

                // Check if this property belongs to a group being overwritten
                var propGroup = GetPropertyGroup(prop.Command);
                bool shouldOverwrite = false;

                if (coversAll)
                {
                    // Full copy overwrites everything except identity commands
                    shouldOverwrite = propGroup != PropertyGroup.None || !IsIdentityCommand(prop.Command);
                }
                else
                {
                    // Partial copy - only overwrite matching groups
                    shouldOverwrite = groupsToOverwrite.Contains(propGroup);
                }

                if (shouldOverwrite)
                {
                    overwrittenProperties.Add(prop);
                    _properties.Remove(prop);
                }
            }

            // Log a parse issue if any properties were actually overwritten
            if (overwrittenProperties.Count > 0)
            {
                var overwrittenCommands = string.Join(", ", overwrittenProperties.Select(p =>
                    CommandsMap.TryGetString(p.Command, out var s) ? s : p.Command.ToString()));
                var copyCmdStr = CommandsMap.TryGetString(copyProperty.Command, out var cs) ? cs : copyProperty.Command.ToString();
                var message = $"{copyCmdStr} at line {copyProperty.LineNumber} overwrites {overwrittenProperties.Count} previously defined property(s): {overwrittenCommands}";
                ParentMod?.AddParseIssue(ParseIssueType.PropertiesClearedBySubsequentClear, message, copyProperty.LineNumber);
            }
        }

        /// <summary>
        /// Checks if a command is an identity/structural command that should not be overwritten by copy.
        /// </summary>
        private static bool IsIdentityCommand(Command command)
        {
            return command switch
            {
                Command.NAME => true,
                Command.FIXEDNAME => true,
                Command.DESCR => true,
                _ => false
            };
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
                prop.LineNumber = ParentMod.LineNumber;
                prop.Parse(command, value, comment);
                AddProperty(prop);
            }
            else
            {
                var typeName = this.GetType().Name;
                var commandStr = CommandsMap.TryGetString(command, out var cmdStr) ? cmdStr : command.ToString();
                var message = $"Invalid or unknown command '{commandStr}' for {typeName}";
                this.ParentMod.Log(message);
                this.ParentMod.AddParseIssue(ParseIssueType.InvalidCommand, message);
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
            // Step 1: Check direct property on entity (ALWAYS returned if present)
            ret = Get<T>(c);
            if (ret != null)
            {
                return ReturnType.TRUE;
            }

            // Step 2: Check if property group is cleared (blocks inheritance)
            if (checkCopy && IsPropertyGroupCleared(c))
            {
                ret = null;
                return ReturnType.FALSE;
            }

            // Step 3: Check copy chain
            if (checkCopy)
            {
                var copyExists = TryGetCopyFrom(out var copy);
                if (copyExists && copy != null)
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
            if (TryGetCopyFrom(out var copiedEntity) && copiedEntity != null && !visitedEntities.Contains(copiedEntity))
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
            if (copyExists && copy != null)
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

        /// <summary>
        /// Checks if this entity has a specific clear command.
        /// </summary>
        public bool HasClearCommand(Command clearCommand)
        {
            return Properties.Any(p => p.Command == clearCommand);
        }

        /// <summary>
        /// Gets the property group for a command. Override in derived classes for entity-specific groupings.
        /// Default returns None (not clearable by specific clear commands).
        /// </summary>
        public virtual PropertyGroup GetPropertyGroup(Command command)
        {
            return PropertyGroup.None;
        }

        /// <summary>
        /// Checks if a property's group is cleared on this entity.
        /// Returns true if the property should NOT be inherited (blocked by a clear command).
        /// </summary>
        public bool IsPropertyGroupCleared(Command command)
        {
            // Total clear (#clear) blocks everything
            if (HasClearCommand(Command.CLEAR))
                return true;

            var group = GetPropertyGroup(command);
            if (group == PropertyGroup.None || group == PropertyGroup.Sprites)
                return false; // Stats and sprites not affected by specific clears

            var clearCommand = PropertyGroupMap.GetClearCommand(group);
            return clearCommand.HasValue && HasClearCommand(clearCommand.Value);
        }

        public int sort_properties(Property p)
        {
            switch (p.Command)
            {
                // Select/New commands first
                case Command.SELECTMONSTER:
                case Command.NEWMONSTER:
                case Command.SELECTWEAPON:
                case Command.NEWWEAPON:
                case Command.SELECTARMOR:
                case Command.NEWARMOR:
                case Command.SELECTITEM:
                case Command.NEWITEM:
                case Command.SELECTSPELL:
                case Command.NEWSPELL:
                case Command.SELECTSITE:
                case Command.NEWSITE:
                case Command.SELECTNATION:
                case Command.NEWNATION:
                    return 1;
                // Copy commands second (they overwrite everything)
                case Command.COPYSTATS:
                case Command.COPYSPR:
                case Command.COPYWEAPON:
                case Command.COPYARMOR:
                case Command.COPYITEM:
                case Command.COPYSPELL:
                case Command.COPYSITE:
                    return 2;
                // Clear commands third
                case Command.CLEAR:
                case Command.CLEARWEAPONS:
                case Command.CLEARARMOR:
                case Command.CLEARMAGIC:
                case Command.CLEARSPEC:
                    return 3;
                // Name commands fourth (after copy so they override copied name)
                case Command.NAME:
                case Command.FIXEDNAME:
                    return 4;
                // Everything else
                default:
                    return 5;
            }
        }
    }
}
