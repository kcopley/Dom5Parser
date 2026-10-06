using Dom5Edit;
using Dom5Edit.Commands;
using Dom5Edit.Editing;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Edit.Resolve;
using Dom5Editor.UI.Controls;

namespace Dom5Editor.Session
{
    /// <summary>
    /// One open mod in the editor: the model, the core editor every change goes through, undo and
    /// redo, and saving. Views read what an entity is in game from <see cref="Resolve"/> and change
    /// it only through <see cref="Edit"/> (docs/EDIT_FLOW.md, "GUI structure").
    /// </summary>
    public sealed class EditorSession
    {
        public Mod Mod { get; }
        public ModEditor Editor { get; }
        public EditHistory History { get; } = new EditHistory();

        /// <summary>The file the mod was loaded from or last saved to (null for a new, unsaved mod).</summary>
        public string? FilePath { get; private set; }

        /// <summary>Raised after every edit, undo and redo, with the edit.</summary>
        public event Action<IModEdit>? Changed;

        /// <summary>Raised when a page asks to show another entity (a reference was clicked).</summary>
        public event Action<EntityType, int>? NavigationRequested;

        private readonly Dictionary<EntityType, List<ReferenceItem>> _references = new Dictionary<EntityType, List<ReferenceItem>>();

        private EditorSession(Mod mod, string? path)
        {
            Mod = mod;
            FilePath = path;
            Editor = new ModEditor(mod);
            Editor.Changed += e =>
            {
                // names and the set of entities may have changed
                foreach (var t in e.Entities.Select(x => x.Kind).Distinct())
                    _references.Remove(t);
                Changed?.Invoke(e);
            };
        }

        public void Navigate(EntityType type, int id) => NavigationRequested?.Invoke(type, id);

        /// <summary>Every entity of a type (vanilla and the mod's), for reference pickers: ID and name in game.</summary>
        public IReadOnlyList<ReferenceItem> References(EntityType type)
        {
            if (_references.TryGetValue(type, out var list))
                return list;
            var byId = new SortedDictionary<int, ReferenceItem>();
            var unnumbered = new List<ReferenceItem>();
            void Add(IDEntity e)
            {
                var name = Resolve(e).Get(Command.NAME)?.Property is StringProperty s ? s.Value : null;
                var item = new ReferenceItem { ID = e.ID, DisplayName = string.IsNullOrEmpty(name) ? $"#{e.ID}" : name!, Tag = e };
                if (e.ID > 0) byId[e.ID] = item; else unnumbered.Add(item);
            }
            if (VanillaLoader.Vanilla?.Database.TryGetValue(type, out var vanilla) == true)
                foreach (var e in vanilla.GetFullList())
                    Add(e);
            if (Mod.Database.TryGetValue(type, out var own))
                foreach (var e in own.GetFullList())
                    Add(e);
            return _references[type] = byId.Values.Concat(unnumbered).ToList();
        }

        /// <summary>References by the JSON configs' type names ("monster", "weapon", ...).</summary>
        public IReadOnlyList<ReferenceItem> References(string? refType) =>
            Data.BadgeConfigLoader.GetEntityTypeFromRefType(refType) is EntityType t ? References(t) : Array.Empty<ReferenceItem>();

        /// <summary>An empty mod on top of the vanilla data.</summary>
        public static EditorSession New()
        {
            var mod = new Mod { ModName = "New mod" };
            mod.ResolveDependencies();
            mod.Resolve();
            return new EditorSession(mod, null);
        }

        public static EditorSession Load(string path) => new EditorSession(Mod.Import(path), path);

        public ResolvedEntity Resolve(IDEntity entity) => Editor.Resolve(entity);

        /// <summary>
        /// Makes an edit and records it for undo. Returns null on success, else why it couldn't be
        /// made (an edit the game offers no way to make, e.g. removing a base stat).
        /// </summary>
        public string? Edit(Func<ModEditor, IModEdit?> edit)
        {
            try
            {
                var done = edit(Editor);
                if (done != null)
                    History.Record(done);
                return null;
            }
            catch (EditException ex)
            {
                return ex.Message;
            }
        }

        /// <summary>Several changes as one undo step.</summary>
        public string? Edit(string description, Action<Transaction> body) => Edit(ed => ed.Run(description, body));

        public IModEdit? Undo() => History.Undo();

        public IModEdit? Redo() => History.Redo();

        /// <summary>Saves the mod (in its file's order, unedited lines as read; the previous file kept as .bak).</summary>
        public void Save(string path)
        {
            Mod.Export(path);
            FilePath = path;
            History.MarkSaved();
        }
    }
}
