using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Editor.Session;

namespace Dom5Editor.UI.ViewModels
{
    /// <summary>The page for an entity: the generic page, with the type's panels.</summary>
    public static class EntityPages
    {
        public static EntityPageViewModel Create(EditorSession session, EntityListItem item) => item.Type switch
        {
            EntityType.MONSTER => new MonsterPageViewModel(session, item),
            _ => new EntityPageViewModel(session, item),
        };
    }

    /// <summary>A monster: weapons, armor and magic in panels; the rest in its badge sections.</summary>
    public sealed class MonsterPageViewModel : EntityPageViewModel
    {
        public MonsterPageViewModel(EditorSession session, EntityListItem item) : base(session, item) { }

        protected override IEnumerable<string> PanelSections => new[] { "magicpaths", "magic" };

        protected override void BuildPanels(HashSet<Command> covered)
        {
            Panels.Add(new ReferenceListPanel(this, "WEAPONS", Command.WEAPON, EntityType.WEAPON, WeaponSummary));
            Panels.Add(new ReferenceListPanel(this, "ARMOR", Command.ARMOR, EntityType.ARMOR, ArmorSummary));
            Panels.Add(new MagicPanel(this));
            covered.UnionWith(new[] { Command.WEAPON, Command.ARMOR, Command.MAGICSKILL, Command.CUSTOMMAGIC });
        }

        /// <summary>What the game uses when the monster sets none: resource size is its size; horrors have spirit sight.</summary>
        protected override string? GameDefault(Command c) => c switch
        {
            Command.RESSIZE => Resolved.Get(Command.SIZE)?.Arguments,
            Command.SPIRITSIGHT => Resolved.Has(Command.HORROR) ? "1" : null,
            _ => null,
        };

        private string WeaponSummary(int id) => Summary(EntityType.WEAPON, id,
            (Command.DMG, "dmg"), (Command.ATT, "att"), (Command.DEF, "def"), (Command.LEN, "len"), (Command.NRATT, "x"));

        private string ArmorSummary(int id) => Summary(EntityType.ARMOR, id,
            (Command.PROT, "prot"), (Command.DEF, "def"), (Command.ENC, "enc"));

        private string Summary(EntityType type, int id, params (Command Command, string Label)[] fields)
        {
            if (id <= 0 || !Session.Mod.TryGet(type, id, null, out var e))
                return "";
            var r = Session.Resolve(e);
            return string.Join("  ", fields.Select(f => r.Get(f.Command) is { } v ? $"{f.Label} {v.Arguments}" : null).Where(s => s != null));
        }
    }
}
