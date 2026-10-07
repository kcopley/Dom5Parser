using Dom5Edit.Commands;
using Dom5Edit.Entities;

namespace Dom5Edit.Props
{
    public class MontagIDRef : IDRef, IMultiReference
    {
        DependentEntity _montag = null;
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
            _montag = this.Parent.ParentMod.AddDependent(EntityType.MONTAG, ID);
            if (_montag != null)
            {
                _montag.ReferencedEntities.Add(this.Parent as IDEntity);
            }
            HasMontagID = _montag != null;
        }

        /// <summary>
        /// For a use of a monster tag (#damage -11, #com -11: one of the tagged monsters), the
        /// monsters that carry it (#montag 11), in this mod and the ones under it (the game's).
        /// Nothing for the #montag line itself.
        /// </summary>
        public IEnumerable<IDEntity> Targets()
        {
            if (Command == Command.MONTAG || ID <= 0 || Parent?.ParentMod is not Mod mod)
                return Array.Empty<IDEntity>();
            return new[] { mod }.Concat(mod.Dependencies)
                .Select(m => m.Dependents.TryGetValue(EntityType.MONTAG, out var tags) && tags.TryGetValue(ID, out var tag) ? tag : null)
                .Where(tag => tag != null)
                .SelectMany(tag => tag!.ReferencedEntities)
                .Where(e => e is Monster && e.Properties.OfType<MontagIDRef>().Any(p => p.Command == Command.MONTAG && p.ID == ID))
                .Distinct();
        }

        public List<IDEntity> GetConnectedEntities()
        {
            return _montag.ReferencedEntities;
        }

        public override void Connect(IDEntity original)
        {
            var list = GetConnectedEntities();
            foreach (var entity in list)
            {
                entity.UsedByEntities.Add(original);
                original.RequiredEntities.Add(entity);
            }
        }

        public override string ToExportString()
        {
            if (CommandsMap.TryGetString(Command, out string s))
            {
                int _exportID = _montag != null ? _montag.GetID() : ID; //true is left, false is right

                if (Command != Command.MONTAG)
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
                    return HasValue ? s + " " + _exportID : s; // no value given, none written
                }
            }
            else return "";
        }

        internal override EntityType GetEntityType()
        {
            return EntityType.MONTAG;
        }
    }
}
