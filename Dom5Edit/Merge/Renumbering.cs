using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Edit.Resolve;

namespace Dom5Edit.Merge
{
    /// <summary>
    /// Moves an entity, or a code, variable, enchantment or monster tag number, to another number
    /// (docs/MERGING.md). Connections are pointers: a reference holds its target and writes the
    /// target's number, a code is one shared object written by its number, a block's header is
    /// rewritten when its entity's number changed; so a move only re-keys the tables, and the
    /// numbers references keep for looking their targets up again follow.
    /// </summary>
    public static class Renumbering
    {
        /// <summary>
        /// Moves an entity of its mod to a free number. <paramref name="mods"/>: the mods whose
        /// references may point at it (its own, and mods read over it: a submod's #select of it
        /// follows it).
        /// </summary>
        public static void Move(IDEntity entity, int newId, IEnumerable<Mod> mods)
        {
            var type = entity.GetEntityType();
            int old = entity.ID;
            if (old == newId)
                return;
            if (entity.ParentMod.Database[type].Has(newId))
                throw new InvalidOperationException($"{type} {newId} is taken in {entity.ParentMod.DisplayName}");
            entity.ParentMod.Database[type].Rekey(entity, old, newId);
            entity.ID = newId;
            foreach (var mod in mods.Distinct())
            {
                // a mod over it that selects it: its entity for it moves too (its number is this one's)
                if (!ReferenceEquals(mod, entity.ParentMod) && mod.Database[type].TryGetValue(old, out var over) && ReferenceEquals(over.DependentEntity, entity))
                    mod.Database[type].Rekey(over, old, newId);
                foreach (var e in Entities(mod))
                    foreach (var p in e.Properties)
                        if (p is Reference r)
                            foreach (var part in r.Parts())
                                part.FollowTarget(entity);
                ModResolver.For(mod).Invalidate();
            }
        }

        /// <summary>
        /// Moves a code, variable, enchantment, monster tag or the like (a DependentEntity of the
        /// mod) to another number: everything that names it is written with the new one; a submod's
        /// own entry for it (linked to this one) follows.
        /// </summary>
        public static void Move(Mod mod, EntityType type, DependentEntity dependent, int newId)
        {
            var set = mod.Dependents[type];
            int old = dependent.ID;
            if (old == newId)
                return;
            if (set.ContainsKey(newId))
                throw new InvalidOperationException($"{type} {newId} is taken in {mod.DisplayName}");
            set.Remove(old);
            dependent.ID = newId;
            set[newId] = dependent;
            ModResolver.For(mod).Invalidate();
        }

        /// <summary>Every entity the mod holds (numbered, unnumbered, events).</summary>
        internal static IEnumerable<IDEntity> Entities(Mod mod) =>
            mod.Database.Values.SelectMany(s => s.GetFullList()).Concat(mod.Events).Distinct();
    }
}
