using System.Runtime.CompilerServices;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Edit.Resolve
{
    /// <summary>
    /// What each entity is in game after a mod: replays the lines a save of the mod writes, in
    /// the order it writes them (SavePlan), over the vanilla data, the way the game reads a mod
    /// (docs/EDIT_FLOW.md):
    /// - a #select block starts from the entity as it is so far (vanilla, or the mod's earlier
    ///   blocks); a #new block starts empty;
    /// - a copy command replaces what it copies with the source as it is at that line;
    /// - a clear command removes its group;
    /// - any other line replaces the earlier value it overwrites in game, or adds an entry for
    ///   repeatable commands (GameRules).
    /// Every value keeps where it came from (ValueSource), which decides what an edit does.
    /// The result is cached until <see cref="Invalidate"/> (call it after an edit).
    /// </summary>
    public sealed class ModResolver
    {
        private sealed class State
        {
            public IDEntity Entity = null!;
            public IDEntity? Vanilla;
            public List<ResolvedValue> Values = new List<ResolvedValue>();
            public List<Property> Structure = new List<Property>();
            public IReadOnlyList<GameValue> GameValues = Array.Empty<GameValue>();
        }

        private static readonly ConditionalWeakTable<Mod, ModResolver> _resolvers = new ConditionalWeakTable<Mod, ModResolver>();

        private readonly Mod _mod;
        private readonly ValueSource _lineSource;
        private Dictionary<object, State>? _states;
        private readonly Dictionary<object, ResolvedEntity> _resolved = new Dictionary<object, ResolvedEntity>();

        private ModResolver(Mod mod)
        {
            _mod = mod;
            // a mod other mods depend on (vanilla) is their base data
            _lineSource = mod.Dependencies.Count == 0 ? ValueSource.Vanilla : ValueSource.Own;
        }

        /// <summary>The resolver for a mod (one per mod, shared).</summary>
        public static ModResolver For(Mod mod) => _resolvers.GetValue(mod, m => new ModResolver(m));

        /// <summary>The mod this resolver replays.</summary>
        public Mod Mod => _mod;

        private ModResolver? Base => _mod.Dependencies.Count > 0 ? For(_mod.Dependencies[0]) : null;

        /// <summary>Forget the replay; the next Resolve replays the mod as it is now. Call after every edit.</summary>
        public void Invalidate()
        {
            _states = null;
            _resolved.Clear();
        }

        /// <summary>
        /// What an entity is in game. A vanilla entity the mod changes resolves to the mod's version;
        /// one it doesn't touch, to the vanilla data.
        /// </summary>
        public ResolvedEntity Resolve(IDEntity entity)
        {
            var key = Key(entity);
            if (_resolved.TryGetValue(key, out var done))
                return done;
            Walk();
            ResolvedEntity result;
            if (_states!.TryGetValue(key, out var state))
                result = new ResolvedEntity(state.Entity, state.Vanilla, state.Values.ToList(), state.Structure.ToList(), state.GameValues);
            else if (entity.ParentMod != _mod && Base != null)
                result = Base.Resolve(entity);
            else
                result = new ResolvedEntity(entity, null, Array.Empty<ResolvedValue>(), Array.Empty<Property>(), entity.GameValues);
            _resolved[key] = result;
            return result;
        }

        /// <summary>The entity a mod's #select changes in this mod's base (vanilla), or null.</summary>
        public IDEntity? BaseEntityOf(IDEntity entity)
        {
            if (entity.DependentEntity != null)
                return entity.DependentEntity;
            var type = entity.GetEntityType();
            foreach (var dep in _mod.Dependencies)
                if (dep.Database.TryGetValue(type, out var set) && set.TryGet(entity.ID, null, out var found))
                    return found;
            return null;
        }

        /// <summary>One identity per entity across the mod and its base: type and ID, else the object.</summary>
        private static object Key(IDEntity entity)
        {
            int id = entity.ID;
            return id > 0 ? (entity.GetEntityType(), id) : entity;
        }

        private void Walk()
        {
            if (_states != null)
                return;
            _states = new Dictionary<object, State>();
            foreach (var block in new SavePlan(_mod).Blocks())
            {
                var entity = block.Entity;
                var type = entity.GetEntityType();
                var key = Key(entity);
                bool isNew = block.Source != null ? !block.Source.Selected : !entity.Selected;
                if (isNew || !_states.TryGetValue(key, out var state))
                    _states[key] = state = isNew ? NewState(entity) : StartFromBase(entity);
                foreach (var p in block.Lines)
                    Apply(state, type, p);
            }
        }

        private State NewState(IDEntity entity) => new State
        {
            Entity = entity,
            GameValues = _lineSource == ValueSource.Vanilla ? entity.GameValues : Array.Empty<GameValue>(),
        };

        /// <summary>A #select of an entity this mod hasn't defined yet: it starts as the base (vanilla) has it.</summary>
        private State StartFromBase(IDEntity entity)
        {
            var state = NewState(entity);
            var vanilla = Base != null ? BaseEntityOf(entity) : null;
            if (vanilla != null)
            {
                var start = Base!.Resolve(vanilla);
                state.Vanilla = vanilla;
                state.Values.AddRange(start.Values);
                state.GameValues = start.GameValues;
            }
            return state;
        }

        /// <summary>The entity as it is at this point of the replay: the mod's state so far, else the base's.</summary>
        private (IDEntity Entity, IReadOnlyList<ResolvedValue> Values, IReadOnlyList<GameValue> GameValues) Current(IDEntity source)
        {
            if (_states!.TryGetValue(Key(source), out var state))
                return (state.Entity, state.Values, state.GameValues);
            if (Base != null)
            {
                var inBase = source.ParentMod == _mod ? FindInBase(source) : source;
                if (inBase != null)
                {
                    var r = Base.Resolve(inBase);
                    return (r.Entity, r.Values, r.GameValues);
                }
            }
            return (source, Array.Empty<ResolvedValue>(), Array.Empty<GameValue>());
        }

        private IDEntity? FindInBase(IDEntity entity)
        {
            var type = entity.GetEntityType();
            foreach (var dep in _mod.Dependencies)
                if (dep.Database.TryGetValue(type, out var set) && set.TryGet(entity.ID, null, out var found))
                    return found;
            return null;
        }

        private void Apply(State state, EntityType type, Property p)
        {
            var c = p.Command;
            if (GameRules.IsCopy(c))
            {
                var copied = new List<ResolvedValue>();
                IReadOnlyList<GameValue>? gameValues = null;
                if (p is Reference r && r.TryGetEntity(out var target) && target != null && !ReferenceEquals(target, state.Entity))
                {
                    var source = Current(target);
                    copied.AddRange(source.Values.Where(v => GameRules.Copies(type, c, v.Command)).Select(v => v.CopiedBy(source.Entity)));
                    gameValues = source.GameValues;
                }
                state.Values.RemoveAll(v => GameRules.Copies(type, c, v.Command));
                state.Values.AddRange(copied);
                if (c != Command.COPYSPR)
                    state.GameValues = gameValues ?? Array.Empty<GameValue>();
                state.Structure.RemoveAll(s => s.Command == c);
                state.Structure.Add(p);
                return;
            }
            if (GameRules.IsClear(c))
            {
                state.Values.RemoveAll(v => GameRules.Clears(type, c, v.Command));
                if (c == Command.CLEAR)
                    state.GameValues = Array.Empty<GameValue>();
                state.Structure.Add(p);
                return;
            }
            var value = new ResolvedValue(p, _lineSource);
            int at = -1;
            for (int i = state.Values.Count - 1; i >= 0; i--)
            {
                var v = state.Values[i];
                if (GameRules.Replaces(type, value, v) || GameRules.Cancels(type, value, v))
                {
                    state.Values.RemoveAt(i);
                    at = i;
                }
            }
            // a value that replaces another takes its place (the game overwrites it where it is)
            if (at >= 0)
                state.Values.Insert(at, value);
            else
                state.Values.Add(value);
        }
    }
}
