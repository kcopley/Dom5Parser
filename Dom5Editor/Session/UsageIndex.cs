using Dom5Edit;
using Dom5Edit.Commands;
using Dom5Edit.Editing;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Editor.Session
{
    /// <summary>
    /// Which entities refer to which ("used by"): every reference value an entity has in game
    /// (a monster's weapons, a nation's recruits, a spell's summoned unit, ...), indexed by the
    /// entity it points at. Built on first use from vanilla and the mod; after an edit, the
    /// touched entities' entries are worked out again.
    /// </summary>
    public sealed class UsageIndex
    {
        public sealed class Use
        {
            public Use(EntityType type, int id, IDEntity from, Command via)
            {
                Type = type;
                Id = id;
                From = from;
                Via = via;
            }

            public EntityType Type { get; }
            public int Id { get; }
            public IDEntity From { get; }
            public Command Via { get; }
        }

        private readonly EditorSession _session;
        private Dictionary<(EntityType, int), List<Use>>? _byTarget;
        private readonly Dictionary<(EntityType, int), List<(EntityType, int)>> _targetsOf = new();

        public UsageIndex(EditorSession session)
        {
            _session = session;
        }

        /// <summary>The entities that refer to this one, with the command they do it with.</summary>
        public IReadOnlyList<Use> UsedBy(EntityType type, int id)
        {
            Build();
            return _byTarget!.TryGetValue((type, id), out var list) ? list : (IReadOnlyList<Use>)Array.Empty<Use>();
        }

        /// <summary>Works the touched entities' entries out again.</summary>
        public void OnChanged(IModEdit edit)
        {
            if (_byTarget == null)
                return;
            foreach (var e in edit.Entities)
            {
                if (e.ID <= 0)
                    continue;
                Remove((e.Kind, e.ID));
                var current = Current(e.Kind, e.ID);
                if (current != null)
                    Add(current);
            }
        }

        private void Build()
        {
            if (_byTarget != null)
                return;
            _byTarget = new Dictionary<(EntityType, int), List<Use>>();
            var mod = _session.Mod;
            foreach (var (type, set) in mod.Database)
                foreach (var e in set.GetFullList())
                    Add(e);
            if (VanillaLoader.Vanilla != null)
                foreach (var (type, set) in VanillaLoader.Vanilla.Database)
                    foreach (var e in set.GetFullList())
                        if (e.ID <= 0 || !mod.Database.TryGetValue(type, out var own) || !own.TryGetValue(e.ID, out _))
                            Add(e);
        }

        /// <summary>The entity for a key as the game has it: the mod's, else vanilla's.</summary>
        private IDEntity? Current(EntityType type, int id)
        {
            if (_session.Mod.Database.TryGetValue(type, out var set) && set.TryGetValue(id, out var own))
                return own;
            return VanillaLoader.Vanilla?.Database.TryGetValue(type, out var vset) == true && vset.TryGetValue(id, out var v) ? v : null;
        }

        private void Add(IDEntity entity)
        {
            EntityType kind;
            try { kind = entity.Kind; }
            catch (NotImplementedException) { return; }
            var from = (kind, entity.ID);
            var targets = new List<(EntityType, int)>();
            foreach (var v in _session.Resolve(entity).Values)
            {
                if (v.Property is not Reference r || !r.TryGetEntity(out var target) || target == null || target.ID <= 0)
                    continue;
                EntityType targetKind;
                try { targetKind = target.Kind; }
                catch (NotImplementedException) { continue; }
                var key = (targetKind, target.ID);
                if (!_byTarget!.TryGetValue(key, out var list))
                    _byTarget[key] = list = new List<Use>();
                list.Add(new Use(kind, entity.ID, entity, v.Command));
                targets.Add(key);
            }
            if (entity.ID > 0)
                _targetsOf[from] = targets;
        }

        private void Remove((EntityType, int) from)
        {
            if (!_targetsOf.TryGetValue(from, out var targets))
                return;
            foreach (var t in targets.Distinct())
                if (_byTarget!.TryGetValue(t, out var list))
                    list.RemoveAll(u => u.Type == from.Item1 && u.Id == from.Item2);
            _targetsOf.Remove(from);
        }
    }
}
