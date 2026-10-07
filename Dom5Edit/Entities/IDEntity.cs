using Dom5Edit.Commands;
using Dom5Edit.GameData;
using Dom5Edit.Props;
using Dom5Edit.Validation;

namespace Dom5Edit.Entities
{
    public class IDEntity : Entity
    {
        private List<Property> _properties = new List<Property>();
        public IReadOnlyList<Property> Properties => _properties.AsReadOnly();

        /// <summary>
        /// Values the game stores that no command can set ("-- ro:" lines in the exe-written
        /// vanilla data). Shown read-only, never exported.
        /// </summary>
        public List<GameValue> GameValues { get; } = new List<GameValue>();

        // Copy/inheritance redesign Phase 1 (see docs/COPY_INHERITANCE_REDESIGN.md):
        // a frozen, order-correct snapshot of this entity's copy source, captured at the
        // moment the copy command was parsed (mod sources) or completed at Resolve (vanilla
        // sources). null = this entity does not copy anything. Inert unless Mod.NormalizeCopies()
        // is run; the legacy lazy TryGetCopyFrom resolution is otherwise unchanged.
        private List<Property> _materialized = null;
        public HashSet<Nation> AssociatedNations = new HashSet<Nation>();
        public bool Selected { get; set; }
        public bool Named { get; set; }

        /// <summary>The name in the entity's header (#selectbless "Death Explosion"), or null when the header has an ID.</summary>
        public string? HeaderName => Named ? _name : null;
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

        /// <summary>The entity's type (MONSTER, WEAPON, ...).</summary>
        public EntityType Kind => GetEntityType();


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
                        // (#newevent takes no number)
                        writer.WriteLine(endStr.Length > 0 ? s2 + endStr : s2);
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
            MarkEdited();
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
                // Copy/inheritance redesign Phase 1: freeze an order-correct snapshot of the
                // source's CURRENT state. Captured here (during parse) so later edits to the
                // source don't leak into this copy. Vanilla/forward sources can't be resolved
                // yet (dependencies attach after parse) and are completed in FinalizeCopyMaterialization.
                CaptureCopySnapshot(property);
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

        #region Copy materialization (Phase 1)

        /// <summary>
        /// Freezes a snapshot of a copy command's source as it exists right now, so later edits
        /// to the source do not change what this entity copied. Only resolves MOD sources already
        /// present in the local database (vanilla isn't attached during parse); vanilla and
        /// forward-referenced sources are completed later in <see cref="FinalizeCopyMaterialization"/>.
        /// </summary>
        private void CaptureCopySnapshot(Property copyProperty)
        {
            if (ParentMod == null) return;
            if (copyProperty is not StringOrIDRef copyRef) return;

            var groups = PropertyGroupMap.GetGroupsOverwrittenByCopy(copyProperty.Command);
            // A copy whose overwritten groups are entirely sprites (e.g. #copyspr) or empty freezes
            // nothing for the stat snapshot — and must NOT clobber a snapshot a prior same-entity
            // #copystats captured (Bug A). Sprites are excluded from snapshots anyway, so building
            // one here would yield an empty list and wipe the real materialization. With the snapshot
            // emptied, the re-derive then saw the weapon/armor set as "diverged" (empty vs the source's
            // real set) and emitted bogus #clear* commands — this guard removes ~279 big-mod diffs.
            // (An empty list's .All(...) is vacuously true, so this also covers the no-group case.)
            if (groups.All(g => g == PropertyGroup.Sprites)) return;

            EntityType type = ((Reference)copyRef).GetEntityType();
            if (!ParentMod.Database.TryGetValue(type, out var set)) return;

            // Local lookup only: at parse time the source's CURRENT (pre-later-edit) state is what
            // illwinter would copy. A miss means vanilla or a forward reference -> defer.
            if (!set.TryGet(copyRef.ID, copyRef.IsStringRef ? copyRef.Name : null, out var source)) return;
            if (source == null || source == this) return;

            _materialized = BuildSnapshot(source, groups);
        }

        /// <summary>
        /// Builds a deep-copied snapshot of <paramref name="source"/>'s properties for the groups a
        /// copy command overwrites, excluding sprites and the ID-relative commands (which #copystats
        /// does not copy). Includes the source's own values plus, for chains, its already-materialized
        /// inherited base for commands it did not itself set.
        /// </summary>
        private List<Property> BuildSnapshot(IDEntity source, List<PropertyGroup> groups)
        {
            bool all = groups.Contains(PropertyGroup.All);
            var snap = new List<Property>();
            var taken = new HashSet<Command>();

            foreach (var p in source._properties)
            {
                if (!ShouldSnapshot(source, p.Command, groups, all)) continue;
                // Single-valued stat/identity props can appear twice on the source when it was
                // re-#selected and edited (the parser appends rather than replaces); illwinter
                // takes the last, so the snapshot must too. Multi-valued groups accumulate.
                bool singleValued = !(p is Reference) && source.GetPropertyGroup(p.Command) == PropertyGroup.None;
                if (singleValued)
                {
                    snap.RemoveAll(x => x.Command == p.Command);
                }
                var clone = p.Clone();
                clone.Parent = this;
                snap.Add(clone);
                taken.Add(p.Command);
            }

            // Chain support (best effort): pull the source's inherited base for commands it did not
            // explicitly override. Multi-valued groups in a chain may be under-captured here; that is
            // a known Phase 1 limitation surfaced by the round-trip harness rather than a silent gap.
            if (source._materialized != null)
            {
                foreach (var p in source._materialized)
                {
                    if (taken.Contains(p.Command)) continue;
                    if (!ShouldSnapshot(source, p.Command, groups, all)) continue;
                    var clone = p.Clone();
                    clone.Parent = this;
                    snap.Add(clone);
                    taken.Add(p.Command);
                }
            }

            return snap;
        }

        /// <summary>
        /// Whether a source command belongs in a copy snapshot: not structural (copy/clear), not a
        /// sprite, and within a group the copy command overwrites. (#xpshape, #growhp and the other
        /// ID-relative commands are copied: the game's #copystats copies the whole ability list.)
        /// </summary>
        private static bool ShouldSnapshot(IDEntity source, Command command, List<PropertyGroup> groups, bool all)
        {
            if (PropertyGroupMap.IsClearCommand(command) || PropertyGroupMap.IsFullCopyCommand(command))
                return false;

            var group = source.GetPropertyGroup(command);
            if (group == PropertyGroup.Sprites)
                return false;
            if (all)
                return true;
            return groups.Contains(group);
        }

        /// <summary>First property with the given command on this entity (own properties only).</summary>
        internal Property GetProp(Command c) => _properties.FirstOrDefault(p => p.Command == c);

        /// <summary>
        /// Completes deferred (vanilla / forward) snapshots now that dependencies are resolved, then
        /// bakes any divergent inherited values into explicit overrides so a load->save is data-identical.
        /// Driven by <see cref="Mod.NormalizeCopies"/>; no-op for entities that do not copy.
        /// </summary>
        internal void FinalizeCopyMaterialization()
        {
            if (_materialized == null) return; // captured only for same-mod sources resolvable at parse
            // The snapshot was cloned at parse, before dependencies were attached, so its references
            // are unresolved (raw ids). Resolve them now so they compare and export consistently with
            // the source's own (resolved) references — otherwise every reference looks "divergent".
            foreach (var p in _materialized)
                if (p is Reference r) r.Resolve();
            BakeDivergentOverrides();
        }

        /// <summary>
        /// Whether the copy command on this entity will resolve to the SAME state on reload that it
        /// resolved to during the original parse. The exporter emits entities in ID order, so on
        /// reload a copy only sees its source if the source is emitted earlier: a dependency
        /// (vanilla, written as the base file) or a same-mod entity with a smaller ID. A copy of a
        /// larger-ID (forward-referenced) source breaks on reload and loses everything it copied.
        /// </summary>
        private bool CopyReproducesOnReload(IDEntity src)
        {
            if (src == null) return false;
            if (src.ParentMod != ParentMod) return true;       // dependency / vanilla base: always emitted first
            if (src.ID <= 0 || ID <= 0) return false;          // unidentified: emitted last, cannot be relied on
            // EntitySet.Export writes a type's mod-range ids (>= START_ID) first, then the vanilla range,
            // each in id order: a mod-range source precedes a vanilla-range copier, not the reverse.
            int start = ParentMod.Database[ParentMod.GetEntityType(GetType())].START_ID;
            bool srcModRange = src.ID >= start, modRange = ID >= start;
            if (srcModRange != modRange) return srcModRange;
            return src.ID < ID;                                 // same section: id order
        }

        /// <summary>
        /// True when the source is this mod's sparse #select edit of an entity a dependency (vanilla)
        /// defines: on reload the copy command still reproduces the dependency's own state.
        /// </summary>
        private bool SourceExtendsDependency(IDEntity src)
        {
            if (src == null || !src.Selected || src.ID <= 0) return false;
            var et = ParentMod.GetEntityType(src.GetType());
            return ParentMod.Dependencies.Any(d => d.Database[et].TryGet(src.ID, null, out _));
        }

        /// <summary>
        /// Re-derive: reconcile the copy against what a re-loaded export would reproduce, emitting
        /// only the values the live copy would NOT reproduce (the divergences) and leaving everything
        /// else implicit behind the copy command. Generic — driven entirely by the copy command's
        /// defined scope (PropertyGroupMap) and the group machinery, with no per-property list:
        ///   - group None (stats / identity) = scalar: bake the diverging inherited value.
        ///   - clearable groups (Weapons / Armor / Magic / Special) = lists: if the copied set
        ///     diverges and the entity didn't itself touch the group, re-assert it (clear + members).
        /// Forward-referenced sources (copy breaks on the ID-ordered reload) are flattened instead.
        /// Only same-mod sources are touched; vanilla copies are left for the loader to replay.
        /// </summary>
        private void BakeDivergentOverrides()
        {
            if (!TryGetCopyFrom(out var src) || src == null) return;
            if (src.ParentMod != ParentMod) return; // vanilla / dependency: leave the live copy command

            var copyProp = _properties.FirstOrDefault(p =>
                PropertyGroupMap.IsFullCopyCommand(p.Command) && p.Command != Command.COPYSPR);
            if (copyProp == null) return;
            var groups = PropertyGroupMap.GetGroupsOverwrittenByCopy(copyProp.Command);

            if (!CopyReproducesOnReload(src))
            {
                var ownCommands = new HashSet<Command>(_properties.Select(p => p.Command));
                var toBake = _materialized.Where(mp => !ownCommands.Contains(mp.Command)).ToList();
                if (SourceExtendsDependency(src))
                {
                    // The source is this mod's edit of a vanilla entity, emitted after this copier
                    // (Bug B). On reload the copy still yields the pristine vanilla entity, so keep it
                    // and state the edits that were in place when the copy ran (the snapshot).
                    foreach (var p in toBake) AddProperty(p);
                    return;
                }
                // Forward reference: the copy resolves to nothing on reload, so emit the snapshot
                // directly and drop the dead copy command. #copyspr resolves independently and stays.
                foreach (var p in toBake) AddProperty(p);
                foreach (var p in _properties
                            .Where(p => PropertyGroupMap.IsFullCopyCommand(p.Command) && p.Command != Command.COPYSPR)
                            .ToList())
                    _properties.Remove(p);
                return;
            }

            // What the live copy reproduces on reload = the source's CURRENT resolved state.
            var srcResolved = BuildSnapshot(src, groups);

            // 1. Scalars (group None): re-assert an inherited value that diverges and isn't overridden.
            foreach (var mp in _materialized)
            {
                if (GetPropertyGroup(mp.Command) != PropertyGroup.None) continue;
                if (GetProp(mp.Command) != null) continue; // entity already states this explicitly
                var srcVal = srcResolved.LastOrDefault(sp => sp.Command == mp.Command);
                if (!CanonEquals(mp, srcVal)) AddProperty(mp);
            }

            // 2. Clearable groups (lists): if the copied set diverges and the entity didn't itself
            //    modify the group, re-assert the copied set with a clear + the members.
            foreach (var group in groups)
            {
                var clearCmd = PropertyGroupMap.GetClearCommand(group);
                if (!clearCmd.HasValue) continue; // None / non-clearable

                bool ownModified = _properties.Any(p => GetPropertyGroup(p.Command) == group)
                                   || _properties.Any(p => p.Command == clearCmd.Value);
                if (ownModified) continue;

                var matG = _materialized.Where(p => GetPropertyGroup(p.Command) == group).ToList();
                var srcG = srcResolved.Where(p => GetPropertyGroup(p.Command) == group).ToList();
                if (CanonSetEquals(matG, srcG)) continue; // copy reproduces the group as-is

                AddProperty(CommandProperty.Create(clearCmd.Value, this));
                foreach (var p in matG) AddProperty(p);
            }
        }

        /// <summary>Compares two properties by export text, ignoring trailing comments.</summary>
        private static bool CanonEquals(Property a, Property b)
        {
            if (a == null || b == null) return a == null && b == null;
            return Canon(a.ToExportString()) == Canon(b.ToExportString());
        }

        /// <summary>Compares two property lists as multisets of canonical export text.</summary>
        private static bool CanonSetEquals(List<Property> a, List<Property> b)
        {
            if (a.Count != b.Count) return false;
            var sa = a.Select(p => Canon(p.ToExportString())).OrderBy(s => s, StringComparer.Ordinal).ToList();
            var sb = b.Select(p => Canon(p.ToExportString())).OrderBy(s => s, StringComparer.Ordinal).ToList();
            return sa.SequenceEqual(sb);
        }

        private static string Canon(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            int i = s.IndexOf(" -- ", StringComparison.Ordinal);
            if (i >= 0) s = s.Substring(0, i);
            return s.TrimEnd();
        }

        #endregion

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
            MarkEdited();
            foreach (var p in props)
            {
                p.Parent = this;
                _properties.Add(p);
            }
            _properties = _properties.OrderBy(sort_properties).ToList();
        }

        public void RemoveProperty(Property property)
        {
            MarkEdited();
            _properties.Remove(property);
        }

        public void ClearProperties()
        {
            MarkEdited();
            _properties.Clear();
        }

        // Edits (Dom5Edit.Editing) change the live list directly, without the copy and clear side
        // effects of AddProperty: where an added line takes effect is decided by its place in the
        // saved file (SavePlan), and an undo restores the list exactly.

        /// <summary>
        /// Whether the entity's lines may differ from what the file gave it (changed after it was
        /// read): the save plan works out placement only for these.
        /// </summary>
        internal bool EditedSinceLoad { get; private set; }

        /// <summary>For diagnostics (Dom5Editor --snapshot --time-refresh).</summary>
        public bool EditedSinceLoadPublic => EditedSinceLoad;

        /// <summary>Any change made outside the parser (an edit, or a loader adding display assets).</summary>
        private void MarkEdited()
        {
            if (ParentMod != null && !ParentMod.IsParsing)
                EditedSinceLoad = true;
        }

        internal List<Property> SnapshotProperties() => new List<Property>(_properties);

        internal void RestoreProperties(List<Property> properties)
        {
            MarkEdited();
            _properties = new List<Property>(properties);
        }

        /// <summary>Adds a line (kept in order: copies and clears first, then the rest as added).</summary>
        internal void InsertLive(Property p)
        {
            MarkEdited();
            p.Parent = this;
            _properties.Add(p);
            _properties = _properties.OrderBy(LiveRank).ToList();
        }

        /// <summary>
        /// Where a line added in the editor goes among the entity's lines (an entity with no parsed
        /// block is saved in this order): <see cref="sort_properties"/>, with every clear among the
        /// copies and clears at the top (#clearrec after a poptype's recruits would remove them),
        /// and lines a type needs early (a nation's name, epithet and era) where it needs them.
        /// </summary>
        protected virtual int LiveRank(Property p) => Dom5Edit.Resolve.GameRules.IsClear(p.Command) ? 20 : sort_properties(p) * 10;

        /// <summary>Moves one of the lines to a place in the list (an entity with no parsed block is saved in list order).</summary>
        internal bool MoveLive(Property p, int index)
        {
            int i = _properties.FindIndex(q => ReferenceEquals(q, p));
            if (i < 0)
                return false;
            MarkEdited();
            _properties.RemoveAt(i);
            _properties.Insert(Math.Clamp(index, 0, _properties.Count), p);
            return true;
        }

        /// <summary>
        /// An entity made in the editor that's saved right after this other one (SavePlan): an
        /// event's delayed follow-up, which the game takes to be the next event in the file.
        /// </summary>
        public IDEntity? PlacedAfter
        {
            get => _placedAfter;
            internal set
            {
                _placedAfter = value;
                PlacedOrder = System.Threading.Interlocked.Increment(ref _placedCounter);
            }
        }

        private IDEntity? _placedAfter;
        private static long _placedCounter;

        /// <summary>When it was placed (a later one goes nearer its anchor).</summary>
        internal long PlacedOrder { get; private set; }

        /// <summary>Puts a new line where an old one is (by reference).</summary>
        internal bool ReplaceLive(Property old, Property replacement)
        {
            int i = _properties.FindIndex(q => ReferenceEquals(q, old));
            if (i < 0)
                return false;
            MarkEdited();
            replacement.Parent = this;
            _properties[i] = replacement;
            return true;
        }

        /// <summary>Removes this line (by reference: CommandProperty compares by value).</summary>
        internal bool RemoveLive(Property p)
        {
            int i = _properties.FindIndex(q => ReferenceEquals(q, p));
            if (i < 0)
                return false;
            MarkEdited();
            _properties.RemoveAt(i);
            return true;
        }

        /// <summary>The property the last Parse call created (null if it rejected the command).</summary>
        internal Property? LastParsedProperty { get; private set; }

        public override void Parse(Command command, string value, string comment)
        {
            LastParsedProperty = null;
            if (GetPropertyMap().TryGetValue(command, out Func<Property> create))
            {
                Property prop = create.Invoke();
                prop.Parent = this; //carry the mod assignation down
                prop.LineNumber = ParentMod.LineNumber;
                prop.Parse(command, value, comment);
                AddProperty(prop);
                LastParsedProperty = prop;
                if (GameCommandCatalog.IsRead(GetEntityType(), command) == false)
                {
                    var commandStr = CommandsMap.TryGetString(command, out var cmdStr) ? cmdStr : command.ToString();
                    ParentMod.AddParseIssue(ParseIssueType.NotReadByGame,
                        $"Dominions {GameCommandCatalog.GameVersion} doesn't read '{commandStr}' for {GetType().Name}; kept, but it changes nothing in game");
                }
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
                MarkEdited();
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
                // Clear commands share the copy rank: OrderBy is stable, so copies and clears keep the
                // order they were parsed in. Order matters in game: "#clear, #copystats X" is a clean copy,
                // "#copystats X, #clear" wipes what was copied (sorting clears after copies did that).
                case Command.CLEAR:
                case Command.CLEARWEAPONS:
                case Command.CLEARARMOR:
                case Command.CLEARMAGIC:
                case Command.CLEARSPEC:
                    return 2;
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
