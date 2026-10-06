using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.GameData;
using Dom5Edit.Props;
using Dom5Edit.Resolve;

namespace Dom5Edit.Editing
{
    /// <summary>An edit that can't be made as asked (the message says why, for the user).</summary>
    public class EditException : Exception
    {
        public EditException(string message) : base(message) { }
    }

    /// <summary>
    /// Every change the editor makes to a mod, as undoable edits (docs/EDIT_FLOW.md). What an edit
    /// does depends on where the value comes from (ResolvedValue.Source):
    /// - the entity's own line: changed or removed in place;
    /// - vanilla, a copy source, or nothing: a line is added to the mod's entity for it, made on
    ///   the first edit of a vanilla entity (copy-on-write: vanilla data is never changed);
    /// - removing an inherited value: "#x 0" for an ability the game removes on 0, else the
    ///   group's clear command plus the rest of the group added back (all the game offers).
    /// Where added lines are saved is SavePlan's job; the resolver then shows the result.
    /// </summary>
    public sealed class ModEditor
    {
        public ModEditor(Mod mod)
        {
            Mod = mod;
        }

        public Mod Mod { get; }

        public ModResolver Resolver => ModResolver.For(Mod);

        /// <summary>Raised after an edit is made, undone or redone (the resolver has been reset).</summary>
        public event Action<IModEdit>? Changed;

        public ResolvedEntity Resolve(IDEntity entity) => Resolver.Resolve(entity);

        /// <summary>The mod's own entity for this one (itself, or the mod's #select of a vanilla one), or null.</summary>
        public IDEntity? OwnEntity(IDEntity entity)
        {
            if (entity.ParentMod == Mod)
                return entity;
            var type = entity.GetEntityType();
            return Mod.Database.TryGetValue(type, out var set) && set.TryGet(entity.ID, null, out var own) ? own : null;
        }

        /// <summary>
        /// Runs several changes as one edit (one undo step). Returns null if nothing changed. If the
        /// body throws, everything it did is undone and the exception passes on.
        /// </summary>
        public IModEdit? Run(string description, Action<Transaction> body)
        {
            var tx = new Transaction(this);
            try
            {
                body(tx);
            }
            catch
            {
                tx.Rollback();
                Resolver.Invalidate();
                throw;
            }
            var edit = tx.Commit(description);
            Resolver.Invalidate();
            if (edit != null)
                Changed?.Invoke(edit);
            return edit;
        }

        internal void OnUndoRedo(IModEdit edit)
        {
            Resolver.Invalidate();
            Changed?.Invoke(edit);
        }

        public IModEdit? Set(IDEntity entity, Command c, string args) =>
            Run($"Set {Name(c)} {args}".TrimEnd(), tx => tx.Set(entity, c, args));

        public IModEdit? Add(IDEntity entity, Command c, string args) =>
            Run($"Add {Name(c)} {args}".TrimEnd(), tx => tx.Add(entity, c, args));

        public IModEdit? Change(IDEntity entity, ResolvedValue value, string args) =>
            Run($"Change {Name(value.Command)} to {args}", tx => tx.Change(entity, value, args));

        public IModEdit? Remove(IDEntity entity, ResolvedValue value) =>
            Run($"Remove {Name(value.Command)} {value.Arguments}".TrimEnd(), tx => tx.Remove(entity, value));

        public IModEdit? Reset(IDEntity entity, Command c) =>
            Run($"Reset {Name(c)}", tx => tx.Reset(entity, c));

        public IModEdit? SetFlag(IDEntity entity, Command c, bool on) =>
            Run($"{(on ? "Set" : "Remove")} {Name(c)}", tx => tx.SetFlag(entity, c, on));

        public IModEdit Create(EntityType type, string? name, out IDEntity created)
        {
            IDEntity? made = null;
            var edit = Run($"New {type.ToString().ToLowerInvariant()}", tx => made = tx.Create(type, name))!;
            created = made!;
            return edit;
        }

        public IModEdit? Delete(IDEntity entity) =>
            Run($"Delete {entity.GetEntityType().ToString().ToLowerInvariant()} {entity.ID}", tx => tx.Delete(entity));

        /// <summary>Changes a header field: #modname, #description, #icon, #version or #domversion.</summary>
        public IModEdit? SetModInfo(Command field, string? value)
        {
            Func<string?> get;
            Action<string?> set;
            switch (field)
            {
                case Command.MODNAME: get = () => Mod.ModName; set = v => Mod.ModName = v!; break;
                case Command.DESCRIPTION: get = () => Mod.Description; set = v => Mod.Description = v!; break;
                case Command.ICON: get = () => Mod.Icon; set = v => Mod.Icon = v!; break;
                case Command.VERSION: get = () => Mod.Version; set = v => Mod.Version = v!; break;
                case Command.DOMVERSION: get = () => Mod.DomVersion; set = v => Mod.DomVersion = v!; break;
                default: throw new EditException($"{Name(field)} isn't a mod header field");
            }
            var before = get();
            if (before == value)
                return null;
            set(value);
            var edit = new ModInfoEdit($"Set {Name(field)}", set, before, value, OnUndoRedo);
            Changed?.Invoke(edit);
            return edit;
        }

        internal static string Name(Command c) => CommandsMap.TryGetString(c, out var s) ? s : c.ToString();
    }

    /// <summary>
    /// The changes of one edit, made one by one on the model (each sees the previous ones), and
    /// recorded so the whole edit undoes as one.
    /// </summary>
    public sealed class Transaction
    {
        private readonly ModEditor _editor;
        private readonly Dictionary<IDEntity, (List<Property> Lines, string Name)> _before = new(ReferenceEqualityComparer.Instance);
        private readonly List<IDEntity> _created = new List<IDEntity>();
        private readonly List<IDEntity> _deleted = new List<IDEntity>();

        internal Transaction(ModEditor editor)
        {
            _editor = editor;
        }

        private Mod Mod => _editor.Mod;

        /// <summary>What the entity is now, with this transaction's changes so far.</summary>
        public ResolvedEntity Resolve(IDEntity entity) => _editor.Resolver.Resolve(entity);

        // ---- primitives ----

        /// <summary>
        /// The mod's entity for this one, made if only vanilla has it (a #select in the mod, which
        /// starts as vanilla and holds the changes). Vanilla data is never edited.
        /// </summary>
        public IDEntity Editable(IDEntity entity)
        {
            var own = _editor.OwnEntity(entity);
            if (own != null)
                return own;
            var made = (IDEntity)Activator.CreateInstance(entity.GetType())!;
            made.Assign(entity.ID.ToString(), "", Mod, selected: true);
            made.Resolve();
            _created.Add(made);
            Changed();
            return made;
        }

        /// <summary>A new line for the entity, its arguments read as the game reads a line ("12", "4 3", "\"Spear\"", or "" for a flag).</summary>
        public Property Line(IDEntity entity, Command c, string args)
        {
            if (!entity.GetPropertyMap().TryGetValue(c, out var create))
                throw new EditException($"{ModEditor.Name(c)} isn't a {entity.GetEntityType().ToString().ToLowerInvariant()} command");
            var p = create();
            p.Parent = entity;
            args = (args ?? "").Trim();
            bool quoted = args.StartsWith("\"");
            var value = quoted ? args.Trim('"') : args;
            var was = Mod.LineWasTrimmed;
            Mod.LineWasTrimmed = quoted;
            try
            {
                p.Parse(c, value, "");
            }
            finally
            {
                Mod.LineWasTrimmed = was;
            }
            if (p is Reference r)
                r.Resolve();
            return p;
        }

        public void AddLine(IDEntity entity, Property p, bool placeFirst = false)
        {
            Touch(entity);
            p.PlaceFirst = placeFirst || GameRules.IsCopy(p.Command) || GameRules.IsClear(p.Command);
            entity.InsertLive(p);
            Changed();
        }

        public void RemoveLine(IDEntity entity, Property p)
        {
            Touch(entity);
            if (!entity.RemoveLive(p))
                throw new EditException($"{ModEditor.Name(p.Command)} isn't one of the entity's lines");
            Changed();
        }

        public void ReplaceLine(IDEntity entity, Property old, Property replacement)
        {
            Touch(entity);
            replacement.PlaceFirst = old.PlaceFirst;
            if (!entity.ReplaceLive(old, replacement))
                throw new EditException($"{ModEditor.Name(old.Command)} isn't one of the entity's lines");
            Changed();
        }

        // ---- operations (what the editor's controls do) ----

        /// <summary>
        /// Sets a single-valued command (for a keyed one, #magicskill, the value for that first
        /// argument): changes the entity's own line in place, else adds one that overrides the
        /// inherited value. On a repeatable command (#weapon) it adds an entry.
        /// </summary>
        public void Set(IDEntity entity, Command c, string args)
        {
            var type = entity.GetEntityType();
            var target = Editable(entity);
            var line = Line(target, c, args);
            if (GameRules.IsRepeatable(type, c))
            {
                AddLine(target, line);
                return;
            }
            var r = Resolve(target);
            var current = Current(r, line);
            if (current != null && r.IsEditableInPlace(current))
                ReplaceLine(target, current.Property, line);
            else
                AddLine(target, line);
        }

        /// <summary>Adds an entry (a weapon, a recruit, a random magic path); on a single-valued command, Set.</summary>
        public void Add(IDEntity entity, Command c, string args)
        {
            if (!GameRules.IsRepeatable(entity.GetEntityType(), c))
            {
                Set(entity, c, args);
                return;
            }
            var target = Editable(entity);
            AddLine(target, Line(target, c, args));
        }

        /// <summary>Turns a flag on or off (no change, and no entity made, if it already is).</summary>
        public void SetFlag(IDEntity entity, Command c, bool on)
        {
            var r = Resolve(entity);
            var current = r.Get(c);
            if (on == (current != null))
                return;
            if (on)
                Set(entity, c, "");
            else
                Remove(entity, current!);
        }

        /// <summary>Changes one value (one weapon of several, one magic path) to new arguments.</summary>
        public void Change(IDEntity entity, ResolvedValue value, string args)
        {
            var type = entity.GetEntityType();
            var target = Editable(entity);
            var r = Resolve(target);
            var current = Find(r, value);
            var line = Line(target, value.Command, args);
            if (r.IsEditableInPlace(current))
            {
                ReplaceLine(target, current.Property, line);
                return;
            }
            if (!GameRules.IsRepeatable(type, value.Command))
            {
                // an inherited value: a line of our own overrides it (for a keyed command with a new
                // first argument, the old entry has to go first)
                var replaced = new ResolvedValue(line, ValueSource.Own);
                if (GameRules.IsKeyedByFirstArgument(type, value.Command) && replaced.Selector != current.Selector)
                    RemoveInherited(target, current);
                AddLine(target, line);
                return;
            }
            RewriteGroup(target, current, line);
        }

        /// <summary>
        /// Removes a value, so the entity no longer has it in game: the entity's own line goes; an
        /// inherited value is removed the way the game allows (RemoveInherited). A base stat can't
        /// be removed (only changed); removing the entity's own line puts it back to what it inherits.
        /// </summary>
        public void Remove(IDEntity entity, ResolvedValue value)
        {
            var type = entity.GetEntityType();
            var target = Editable(entity);
            var r = Resolve(target);
            var current = Find(r, value);
            if (!r.IsEditableInPlace(current))
            {
                RemoveInherited(target, current);
                return;
            }
            RemoveLine(target, current.Property);
            if (GameRules.IsRepeatable(type, current.Command))
                return;
            // an inherited value the line overrode shows again: remove it too, if the game can
            var again = Current(Resolve(target), current.Property);
            if (again != null && again.Source != ValueSource.Own && CanRemoveInherited(type, again.Command))
                RemoveInherited(target, again);
        }

        /// <summary>Drops the entity's own lines for a command, so it has what it inherits again.</summary>
        public void Reset(IDEntity entity, Command c)
        {
            var own = _editor.OwnEntity(entity);
            if (own == null)
                return;
            foreach (var p in own.Properties.Where(p => p.Command == c).ToList())
                RemoveLine(own, p);
        }

        /// <summary>Makes a new entity in the mod's ID range.</summary>
        public IDEntity Create(EntityType type, string? name)
        {
            var set = Mod.Database[type];
            int id = set.NextFreeID();
            var made = (IDEntity)Activator.CreateInstance(Mod.TypeOf(type))!;
            made.Assign(id.ToString(), "", Mod, selected: false);
            _created.Add(made);
            Changed();
            if (!string.IsNullOrEmpty(name))
                AddLine(made, Line(made, Command.NAME, "\"" + name + "\""));
            return made;
        }

        /// <summary>Deletes one of the mod's entities (for a #select of a vanilla one: drops the mod's changes to it).</summary>
        public void Delete(IDEntity entity)
        {
            var own = _editor.OwnEntity(entity) ?? throw new EditException("Vanilla data can't be deleted");
            Mod.Database[own.GetEntityType()].Remove(own);
            if (_created.Remove(own))
                return;
            _deleted.Add(own);
            Changed();
        }

        // ---- removing inherited values ----

        /// <summary>Whether the game offers a way to remove this inherited value (else it can only be changed).</summary>
        public static bool CanRemoveInherited(EntityType type, Command c) =>
            GameRules.RemovesWithZero(type, c) || GroupClear(type, c) != null;

        /// <summary>The clear command that removes this value's group (#clearweapons, #clearspec; #clear for the simple types), or null.</summary>
        private static Command? GroupClear(EntityType type, Command c)
        {
            var group = GameRules.GroupOf(type, c);
            var clear = GameRules.ClearCommandFor(type, group);
            if (clear != null)
                return clear;
            // weapons, armor, items, spells, sites: no groups, only #clear (everything but the name)
            bool simple = type == EntityType.WEAPON || type == EntityType.ARMOR || type == EntityType.ITEM
                          || type == EntityType.SPELL || type == EntityType.SITE;
            return simple && GameRules.Clears(type, Command.CLEAR, c) ? Command.CLEAR : null;
        }

        /// <summary>Removes an inherited value: "#x 0", or the group's clear and the rest of the group added back.</summary>
        public void RemoveInherited(IDEntity target, ResolvedValue value)
        {
            var type = target.GetEntityType();
            if (GameRules.RemovesWithZero(type, value.Command))
            {
                AddLine(target, Line(target, value.Command, "0"));
                return;
            }
            RewriteGroup(target, value, null);
        }

        /// <summary>
        /// Clears the value's group and adds back every other value in it (with the one value
        /// replaced, or left out): the only way to drop or change one inherited entry of a list.
        /// The entity's own lines after the clear stay as they are.
        /// </summary>
        private void RewriteGroup(IDEntity target, ResolvedValue value, Property? replacement)
        {
            var type = target.GetEntityType();
            var clear = GroupClear(type, value.Command)
                        ?? throw new EditException($"{ModEditor.Name(value.Command)} can't be removed, only changed");
            var r = Resolve(target);
            var members = r.Values.Where(v => GameRules.Clears(type, clear, v.Command)).ToList();
            AddLine(target, Line(target, clear, ""), placeFirst: true);
            foreach (var v in members)
            {
                if (ReferenceEquals(v.Property, value.Property))
                {
                    if (replacement != null)
                        AddLine(target, replacement, placeFirst: !r.IsEditableInPlace(v));
                    if (r.IsEditableInPlace(v))
                        RemoveLine(target, v.Property);
                    continue;
                }
                if (!r.IsEditableInPlace(v))
                    AddLine(target, Line(target, v.Command, v.Arguments), placeFirst: true);
            }
        }

        // ---- helpers ----

        /// <summary>The value a line of this command would replace: the last one (for a keyed command, with the same first argument).</summary>
        private static ResolvedValue? Current(ResolvedEntity r, Property line)
        {
            if (!GameRules.IsKeyedByFirstArgument(line.Parent?.Kind, line.Command))
                return r.Get(line.Command);
            var selector = new ResolvedValue(line, ValueSource.Own).Selector;
            return r.GetAll(line.Command).LastOrDefault(v => v.Selector == selector);
        }

        /// <summary>The same value in a fresh resolve (by its line).</summary>
        private static ResolvedValue Find(ResolvedEntity r, ResolvedValue value) =>
            r.Values.FirstOrDefault(v => ReferenceEquals(v.Property, value.Property))
            ?? throw new EditException($"{ModEditor.Name(value.Command)} {value.Arguments} isn't one of the entity's values any more");

        private void Touch(IDEntity entity)
        {
            if (!_before.ContainsKey(entity))
                _before[entity] = (entity.SnapshotProperties(), entity._name);
        }

        private void Changed() => _editor.Resolver.Invalidate();

        internal void Rollback()
        {
            foreach (var (entity, (lines, name)) in _before)
            {
                entity.RestoreProperties(lines);
                entity._name = name;
            }
            foreach (var e in _created)
                Mod.Database[e.GetEntityType()].Remove(e);
            foreach (var e in _deleted)
                Mod.Database[e.GetEntityType()].Restore(e);
        }

        internal IModEdit? Commit(string description)
        {
            var states = new Dictionary<IDEntity, SnapshotEdit.EntityState>(ReferenceEqualityComparer.Instance);
            foreach (var (entity, (lines, name)) in _before)
            {
                var now = entity.SnapshotProperties();
                if (now.Count == lines.Count && now.Zip(lines).All(x => ReferenceEquals(x.First, x.Second)) && entity._name == name)
                    continue;
                states[entity] = new SnapshotEdit.EntityState { Before = lines, After = now, NameBefore = name, NameAfter = entity._name };
            }
            if (states.Count == 0 && _created.Count == 0 && _deleted.Count == 0)
                return null;
            return new SnapshotEdit(Mod, description, states, _created.ToList(), _deleted.ToList(), _editor.OnUndoRedo);
        }
    }
}
