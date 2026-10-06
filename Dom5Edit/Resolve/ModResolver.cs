using System.Runtime.CompilerServices;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Edit.Resolve
{
    /// <summary>
    /// What each entity is in game after a mod: replays the lines a save of the mod writes
    /// (SavePlan), in the order it writes them, over the vanilla data, the way the game reads a mod
    /// (docs/EDIT_FLOW.md):
    /// - a #select block starts from the entity as it is so far (vanilla, or the mod's earlier
    ///   blocks); a #new block starts empty;
    /// - a copy command replaces what it copies with the source as it is at that line;
    /// - a clear command removes its group;
    /// - any other line replaces the earlier value it overwrites in game, or adds an entry for
    ///   repeatable commands (GameRules).
    /// Every value keeps where it came from (ValueSource), which decides what an edit does.
    ///
    /// Lazy, per entity: an entity's state is worked out from its own blocks, and for a copy line
    /// from the source's state just before that line's place in the save (recursively), so
    /// resolving one entity costs its blocks and its copy chain, not the whole mod. Results are
    /// kept until <see cref="Invalidate"/> (call it after every edit).
    /// </summary>
    public sealed class ModResolver
    {
        private sealed class State
        {
            public IDEntity Entity = null!;
            public IDEntity? Vanilla;
            public List<ResolvedValue> Values = new List<ResolvedValue>();
            public List<Property> Structure = new List<Property>();
            public List<Property> Removals = new List<Property>();
            public Dictionary<Command, Property> Assets = new Dictionary<Command, Property>();
            public IReadOnlyList<GameValue> GameValues = Array.Empty<GameValue>();

            public State Clone() => new State
            {
                Entity = Entity, Vanilla = Vanilla, Values = new List<ResolvedValue>(Values),
                Structure = new List<Property>(Structure), Removals = new List<Property>(Removals),
                Assets = new Dictionary<Command, Property>(Assets), GameValues = GameValues,
            };
        }

        /// <summary>One of the mod's entities: its blocks in the save, and its state after each one worked out so far.</summary>
        private sealed class Memo
        {
            public IDEntity Entity = null!;
            public IReadOnlyList<(int Position, SavePlan.Block Block)> Blocks = Array.Empty<(int, SavePlan.Block)>();
            public readonly List<State> After = new List<State>();
        }

        private static readonly ConditionalWeakTable<Mod, ModResolver> _resolvers = new ConditionalWeakTable<Mod, ModResolver>();

        private readonly Mod _mod;
        private readonly ValueSource _lineSource;
        private SavePlan? _plan;
        private readonly Dictionary<object, Memo?> _memo = new Dictionary<object, Memo?>();
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

        private SavePlan Plan => _plan ??= new SavePlan(_mod);

        /// <summary>Forget what was worked out; the next Resolve uses the mod as it is now. Call after every edit.</summary>
        public void Invalidate()
        {
            _plan = null;
            _owners = null;
            _memo.Clear();
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
            ResolvedEntity result;
            var state = StateBefore(entity, int.MaxValue);
            if (state != null)
                result = new ResolvedEntity(state.Entity, state.Vanilla, state.Values.ToList(), state.Structure.ToList(), state.Removals.ToList(), state.GameValues)
                    { Assets = new Dictionary<Command, Property>(state.Assets) };
            else if (entity.ParentMod != _mod && Base != null)
                result = Base.Resolve(entity);
            else if (Base != null && entity.ID > 0 && FindInBase(entity) is IDEntity inBase)
                result = Base.Resolve(inBase); // a mod entity no longer in the mod (an undone first edit): vanilla again
            else
                result = new ResolvedEntity(entity, null, Array.Empty<ResolvedValue>(), Array.Empty<Property>(), Array.Empty<Property>(), entity.GameValues);
            _resolved[key] = result;
            return result;
        }

        /// <summary>The entity a mod's #select changes in this mod's base (vanilla), or null.</summary>
        public IDEntity? BaseEntityOf(IDEntity entity)
        {
            if (entity.DependentEntity != null)
                return entity.DependentEntity;
            return FindInBase(entity);
        }

        /// <summary>One identity per entity across the mod and its base: type and ID, else the object.</summary>
        private static object Key(IDEntity entity)
        {
            int id = entity.ID;
            return id > 0 ? (entity.GetEntityType(), id) : entity;
        }

        /// <summary>
        /// The mod's own entity for this one's key, with its blocks, or null if the mod has none. A
        /// game entity the mod selects both by name and by ID is two objects here; their blocks are
        /// all its blocks, in the save's order.
        /// </summary>
        private Memo? MemoOf(IDEntity entity)
        {
            var key = Key(entity);
            if (_memo.TryGetValue(key, out var memo))
                return memo;
            var owners = OwnersOf(key, entity);
            memo = null;
            if (owners.Count > 0)
            {
                var blocks = owners.SelectMany(o => Plan.BlocksOf(o)).OrderBy(b => b.Position).ToList();
                if (blocks.Count > 0)
                    memo = new Memo { Entity = owners.FirstOrDefault(o => o.ID > 0 && !o.Named) ?? owners[0], Blocks = blocks };
            }
            return _memo[key] = memo;
        }

        private Dictionary<object, List<IDEntity>>? _owners;

        /// <summary>The mod's entities with this key (by type and ID: one usually, two when it selects one by name and by ID).</summary>
        private List<IDEntity> OwnersOf(object key, IDEntity entity)
        {
            if (key is not ValueTuple<EntityType, int>)
                return entity.ParentMod == _mod ? new List<IDEntity> { entity } : new List<IDEntity>();
            if (_owners == null)
            {
                _owners = new Dictionary<object, List<IDEntity>>();
                foreach (var set in _mod.Database.Values)
                    foreach (var e in set.GetFullList())
                    {
                        EntityType t;
                        try { t = e.GetEntityType(); }
                        catch (NotImplementedException) { continue; }
                        if (e.ID <= 0)
                            continue;
                        var k = (t, e.ID);
                        if (!_owners.TryGetValue(k, out var list))
                            _owners[k] = list = new List<IDEntity>();
                        list.Add(e);
                    }
            }
            return _owners.TryGetValue(key, out var found) ? found : new List<IDEntity>();
        }

        /// <summary>
        /// The entity's state just before a place in the save: after its blocks written before it;
        /// null if the mod has written none of its blocks by then (the base, or nothing, applies).
        /// </summary>
        private State? StateBefore(IDEntity entity, int position)
        {
            var memo = MemoOf(entity);
            if (memo == null)
                return null;
            int count = 0;
            while (count < memo.Blocks.Count && memo.Blocks[count].Position < position)
                count++;
            if (count == 0)
                return null;
            for (int b = memo.After.Count; b < count; b++)
            {
                var (pos, block) = memo.Blocks[b];
                bool isNew = block.Source != null ? !block.Source.Selected : !memo.Entity.Selected;
                var state = isNew ? NewState(memo.Entity)
                    : b == 0 ? StartFromBase(memo.Entity)
                    : memo.After[b - 1].Clone();
                var type = memo.Entity.GetEntityType();
                foreach (var p in block.Lines)
                    Apply(state, type, p, pos);
                memo.After.Add(state);
            }
            return memo.After[count - 1];
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
                foreach (var (c, p) in start.Assets)
                    state.Assets[c] = p;
            }
            return state;
        }

        /// <summary>A copy source as it is just before a place in the save: the mod's state so far, else the base's.</summary>
        private (IDEntity Entity, IReadOnlyList<ResolvedValue> Values, IReadOnlyList<GameValue> GameValues, IReadOnlyDictionary<Command, Property> Assets) Current(IDEntity source, int position)
        {
            var state = StateBefore(source, position);
            if (state != null)
                return (state.Entity, state.Values, state.GameValues, state.Assets);
            if (Base != null)
            {
                var inBase = source.ParentMod == _mod ? FindInBase(source) : source;
                if (inBase != null)
                {
                    var r = Base.Resolve(inBase);
                    return (r.Entity, r.Values, r.GameValues, r.Assets);
                }
            }
            return (source, Array.Empty<ResolvedValue>(), Array.Empty<GameValue>(), new Dictionary<Command, Property>());
        }

        private IDEntity? FindInBase(IDEntity entity)
        {
            var type = entity.GetEntityType();
            foreach (var dep in _mod.Dependencies)
                if (dep.Database.TryGetValue(type, out var set) && set.TryGet(entity.ID, null, out var found))
                    return found;
            return null;
        }

        private void Apply(State state, EntityType type, Property p, int position)
        {
            var c = p.Command;
            if (GameRules.IsCopy(c))
            {
                var copied = new List<ResolvedValue>();
                IReadOnlyList<GameValue>? gameValues = null;
                if (p is Reference r && r.TryGetEntity(out var target) && target != null && !ReferenceEquals(target, state.Entity))
                {
                    var source = Current(target, position);
                    copied.AddRange(source.Values.Where(v => GameRules.Copies(type, c, v.Command)).Select(v => v.CopiedBy(source.Entity)));
                    gameValues = source.GameValues;
                    foreach (var key in state.Assets.Keys.Where(k => GameRules.Copies(type, c, k)).ToList())
                        state.Assets.Remove(key);
                    foreach (var (k, a) in source.Assets.Where(x => GameRules.Copies(type, c, x.Key)))
                        state.Assets[k] = a;
                }
                state.Values.RemoveAll(v => GameRules.Copies(type, c, v.Command));
                state.Removals.RemoveAll(x => GameRules.Copies(type, c, x.Command));
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
                state.Removals.RemoveAll(x => GameRules.Clears(type, c, x.Command));
                if (c == Command.CLEAR)
                    state.GameValues = Array.Empty<GameValue>();
                state.Structure.Add(p);
                return;
            }
            if (p.IsDisplayAsset)
            {
                state.Assets[c] = p;
                return;
            }
            if (state.Assets.Count > 0)
                state.Assets.Remove(c); // a line of the data replaces the shown asset
            var value = new ResolvedValue(p, _lineSource);
            var rule = GameRules.RuleOf(type, c);
            int at = -1;
            if (rule.Replaces.Count > 0 || rule.Cancels.Count > 0)
                for (int i = state.Values.Count - 1; i >= 0; i--)
                {
                    var v = state.Values[i];
                    bool replaced = rule.Replaces.Contains(v.Command)
                                    && (!rule.Keyed || v.Selector == value.Selector && !GameRules.AddsEach(type, c, value.Selector));
                    if (replaced || rule.Cancels.Contains(v.Command))
                    {
                        state.Values.RemoveAt(i);
                        at = i;
                    }
                }
            if (state.Removals.Count > 0)
                state.Removals.RemoveAll(x => x.Command == c);
            // a line setting an ability to 0 removes it: no value, but kept to show as removed
            if (GameRules.IsRemoval(type, p))
            {
                if (_lineSource == ValueSource.Own)
                    state.Removals.Add(p);
                return;
            }
            // a value that replaces another takes its place (the game overwrites it where it is)
            if (at >= 0)
                state.Values.Insert(at, value);
            else
                state.Values.Add(value);
        }
    }
}
