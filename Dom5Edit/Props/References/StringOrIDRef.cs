using Dom5Edit.Commands;
using Dom5Edit.Entities;

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
                _nameWhenResolved = null;
                // Setting an ID means we have a value
                if (value != 0)
                {
                    HasValue = true;
                    IsStringRef = false;
                }
                if (Parent?.ParentMod?.IsLoaded == true)
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
                _nameWhenResolved = null;
                if (Parent?.ParentMod?.IsLoaded == true)
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
            // Clear existing entity before resolving to ensure we look up the new ID
            // This is critical when re-resolving after an ID change via the UI
            Entity = null;
            Resolved = false;

            if (Parent.ParentMod.TryGet(GetEntityType(), _id, _name, out IDEntity e))
            {
                Entity = e;
                Resolved = true;
                _nameWhenResolved ??= e.Name;
            }
            //move start ID to be in an entitytype set
            //handle non-resolved separately... these are ones in a dependency?
            if (!Resolved && !IsStringRef && _id > Parent.ParentMod.GetStartID(GetEntityType()))
            {
                Parent.ParentMod.Log(GetEntityType() + " not resolved for: " + this._id);
            }
        }

        internal override void FollowTarget(IDEntity moved)
        {
            if (ReferenceEquals(Entity, moved) && !IsStringRef)
                _id = moved.ID;
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

            if (HasValue && !(Parent?.ParentMod?.LineWasTrimmed ?? true))
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

        private static readonly List<Command> _StringExported = new List<Command>()
        {
            Command.STARTSITE,
            Command.SPELL,
            Command.AUTOSPELL,
        };

        // the target's name when this was first resolved: a different one at saving means it was
        // renamed in the editor since
        private string _nameWhenResolved;

        /// <summary>
        /// The name to write: as the author wrote it, so the game finds what it found before (a
        /// #copystats after #name gives the monster the copied name, and a reference by the first
        /// name then finds nothing in game, which a rewrite must not change); the target's new
        /// name when it was renamed in the editor; in a merge, the target's name.
        /// </summary>
        private string ExportName()
        {
            if (!Resolved || Entity == null)
                return _name;
            if (Parent?.ParentMod?.KeepReferenceForms == false)
                return Entity.TryGetName(out var merged) ? merged : _name;
            var now = Entity.Name;
            if (string.IsNullOrEmpty(_name) || (!string.IsNullOrEmpty(now) && _nameWhenResolved != null && now != _nameWhenResolved))
                return string.IsNullOrEmpty(now) ? _name : now;
            return _name;
        }

        public override string ToExportString()
        {
            if (!CommandsMap.TryGetString(Command, out string s)) return "";

            if (Parent?.ParentMod?.KeepReferenceForms == false)
            {
                // a merge: by number where there is one
                if (Entity != null && Entity.ID != -1)
                    IsStringRef = false;

                if (_StringExported.Contains(Command))
                {
                    IsStringRef = true;
                }
            }

            if (IsStringRef)
            {
                string _exportName = ExportName();

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
                    return HasValue ? s + " " + _exportID : s; // no value given, none written
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
