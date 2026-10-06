using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Edit
{
    /// <summary>
    /// Which lines a save writes, in which block and in what order (docs/SAVE_FLOW.md): the parsed
    /// file's blocks with the session's edits placed by the placement rules, then entities that
    /// have no block. The exporter writes this plan; the resolver (Dom5Edit.Resolve) replays it, so
    /// what the editor shows is what the saved file says.
    /// </summary>
    public sealed class SavePlan
    {
        /// <summary>One block as saved: its entity, and the properties written in it, in order.</summary>
        public sealed class Block
        {
            private readonly Func<IReadOnlyList<Property>> _lines;
            private IReadOnlyList<Property>? _computed;

            public IDEntity Entity { get; }
            /// <summary>The parsed block, or null for an entity written whole after the blocks.</summary>
            public SourceBlock? Source { get; }
            /// <summary>The lines written in the block (worked out when first asked for).</summary>
            public IReadOnlyList<Property> Lines => _computed ??= _lines();

            internal Block(IDEntity entity, SourceBlock? source, Func<IReadOnlyList<Property>> lines)
            {
                Entity = entity;
                Source = source;
                _lines = lines;
            }
        }

        private readonly Mod _mod;
        private readonly HashSet<IDEntity> _held;
        private readonly Dictionary<IDEntity, List<SourceBlock>> _blocksOf = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<IDEntity, HashSet<Property>> _live = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<Property, List<Property>> _inSlot = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<SourceBlock, List<Property>> _atEnd = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<SourceBlock, List<Property>> _atStart = new(ReferenceEqualityComparer.Instance);

        /// <summary>Whether the mod is saved in its parsed file's block order (else canonically: by type and ID).</summary>
        public bool InSourceOrder { get; }

        public SavePlan(Mod mod)
        {
            _mod = mod;
            // (by reference: CommandProperty compares by value)
            _held = new HashSet<IDEntity>(mod.Database.Values.SelectMany(s => s.GetFullList()), ReferenceEqualityComparer.Instance);
            InSourceOrder = mod.PreserveSourceOrder && mod.SourceBlocks.Count > 0 && !mod.Database.Values.Any(s => s.HasDisabled);
            if (!InSourceOrder)
                return;
            foreach (var block in mod.SourceBlocks)
            {
                if (!_blocksOf.TryGetValue(block.Entity, out var list))
                    _blocksOf[block.Entity] = list = new List<SourceBlock>();
                list.Add(block);
            }

            // Live properties no block has (added in the session): a replacement goes where the
            // removed property of the same command was; a copy or clear (PlaceFirst) before the
            // entity's own lines; anything else at the end of a block.
            foreach (var (entity, blocks) in _blocksOf)
            {
                // an entity no edit has touched is written as read
                if (!_held.Contains(entity) || !entity.EditedSinceLoad)
                    continue;
                var liveNow = _live[entity] = new HashSet<Property>(entity.Properties, ReferenceEqualityComparer.Instance);
                var inBlocks = new HashSet<Property>(blocks.SelectMany(b => b.Properties), ReferenceEqualityComparer.Instance);
                var removed = blocks.SelectMany(b => b.Properties)
                    .Where(p => mod.PropertiesAfterParse.Contains(p) && !liveNow.Contains(p)).ToList();
                foreach (var p in entity.Properties.Where(p => !inBlocks.Contains(p)))
                {
                    var gap = removed.FirstOrDefault(r => r.Command == p.Command);
                    if (gap != null)
                        Add(_inSlot, gap, p);
                    else if (p.PlaceFirst)
                    {
                        var anchor = FirstPlaceAnchor(entity, blocks, p);
                        if (anchor != null)
                            Add(_inSlot, anchor, p);
                        else
                            Add(_atStart, blocks[0], p);
                    }
                    else
                        Add(_atEnd, PlacementBlock(entity, blocks, p), p);
                }
            }
        }

        /// <summary>Whether the entity is still in the mod (one deleted in the session isn't written).</summary>
        public bool Holds(IDEntity entity) => _held.Contains(entity);

        /// <summary>Whether a parsed block's property is written: live now, or taken out by a later clear or copy in the file (the game still reads it there).</summary>
        public bool Writes(SourceBlock block, Property p) =>
            !_live.TryGetValue(block.Entity, out var live) || live.Contains(p) || !_mod.PropertiesAfterParse.Contains(p);

        /// <summary>Properties added in the session that go right after this (removed) property.</summary>
        public IReadOnlyList<Property> ReplacementsAfter(Property p) =>
            _inSlot.TryGetValue(p, out var list) ? list : (IReadOnlyList<Property>)Array.Empty<Property>();

        /// <summary>Properties added in the session that go right after this block's header.</summary>
        public IReadOnlyList<Property> AddedAtStart(SourceBlock block) =>
            _atStart.TryGetValue(block, out var list) ? list : (IReadOnlyList<Property>)Array.Empty<Property>();

        /// <summary>Properties added in the session that go at the end of this block.</summary>
        public IReadOnlyList<Property> AddedAtEnd(SourceBlock block) =>
            _atEnd.TryGetValue(block, out var list) ? list : (IReadOnlyList<Property>)Array.Empty<Property>();

        /// <summary>Whether the entity has parsed blocks (else it's written whole after them).</summary>
        public bool HasBlocks(IDEntity entity) => _blocksOf.ContainsKey(entity);

        /// <summary>Every block the save writes, in order, with the properties written in each.</summary>
        public IEnumerable<Block> Blocks()
        {
            if (InSourceOrder)
            {
                foreach (var block in _mod.SourceBlocks)
                {
                    if (!Holds(block.Entity))
                        continue;
                    var b = block;
                    yield return new Block(block.Entity, block, () => LinesOf(b));
                }
            }
            foreach (var entity in WholeEntities())
            {
                var e = entity;
                yield return new Block(entity, null, () => e.Properties.ToList());
            }
        }

        private List<Property> LinesOf(SourceBlock block)
        {
            var lines = new List<Property>(AddedAtStart(block));
            foreach (var p in block.Properties)
            {
                if (Writes(block, p))
                    lines.Add(p);
                lines.AddRange(ReplacementsAfter(p));
            }
            lines.AddRange(AddedAtEnd(block));
            return lines;
        }

        private Dictionary<IDEntity, List<(int, Block)>>? _byEntity;

        /// <summary>An entity's blocks with their positions in the save (0 = first block written).</summary>
        public IReadOnlyList<(int Position, Block Block)> BlocksOf(IDEntity entity)
        {
            if (_byEntity == null)
            {
                _byEntity = new Dictionary<IDEntity, List<(int, Block)>>(ReferenceEqualityComparer.Instance);
                int position = 0;
                foreach (var b in Blocks())
                {
                    if (!_byEntity.TryGetValue(b.Entity, out var list))
                        _byEntity[b.Entity] = list = new List<(int, Block)>();
                    list.Add((position++, b));
                }
            }
            return _byEntity.TryGetValue(entity, out var found) ? found : (IReadOnlyList<(int, Block)>)Array.Empty<(int, Block)>();
        }

        /// <summary>Entities written whole, after the blocks (all of them for a canonical save), in the order they're written.</summary>
        public IEnumerable<IDEntity> WholeEntities()
        {
            foreach (var set in _mod.Database.Values)
                foreach (var entity in set.ExportOrder())
                    if (!HasBlocks(entity))
                        yield return entity;
        }

        /// <summary>
        /// The block an added property goes at the end of: the entity's first block, so copies
        /// made after it carry the value (rule C), unless a later block of the entity sets the same
        /// command, clears its group or copies over it; then the last block, so it takes effect.
        /// </summary>
        private static SourceBlock PlacementBlock(IDEntity entity, List<SourceBlock> blocks, Property p)
        {
            var group = entity.GetPropertyGroup(p.Command);
            bool overriddenLater = blocks.Skip(1).SelectMany(b => b.Properties).Any(q =>
                q.Command == p.Command
                || PropertyGroupMap.GetGroupClearedBy(q.Command) is PropertyGroup cleared
                    && (cleared == PropertyGroup.All || cleared == group)
                || PropertyGroupMap.IsFullCopyCommand(q.Command)
                    && PropertyGroupMap.GetGroupsOverwrittenByCopy(q.Command) is var copied
                    && (copied.Contains(PropertyGroup.All) || copied.Contains(group)));
            return overriddenLater ? blocks[^1] : blocks[0];
        }

        /// <summary>
        /// Where a PlaceFirst line goes: right after the entity's last written copy or clear line that
        /// touches the same group (one before it would undo it), else (null) at the start of the first
        /// block. A clear and the lines re-added after it share the group, so they land together.
        /// </summary>
        private Property? FirstPlaceAnchor(IDEntity entity, List<SourceBlock> blocks, Property p)
        {
            var type = entity.GetEntityType();
            Property? anchor = null;
            foreach (var block in blocks)
                foreach (var q in block.Properties)
                {
                    if (!Writes(block, q) || ReferenceEquals(q, p))
                        continue;
                    if (Resolve.GameRules.Overrides(type, q.Command, p.Command))
                        anchor = q;
                }
            return anchor;
        }

        private static void Add<TKey>(Dictionary<TKey, List<Property>> map, TKey key, Property p) where TKey : notnull
        {
            if (!map.TryGetValue(key, out var list))
                map[key] = list = new List<Property>();
            list.Add(p);
        }
    }
}
