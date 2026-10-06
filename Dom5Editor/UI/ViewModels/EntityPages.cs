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
            EntityType.SPELL => new SpellPageViewModel(session, item),
            EntityType.ITEM => new ItemPageViewModel(session, item),
            EntityType.SITE => new SitePageViewModel(session, item),
            EntityType.ARMOR => new ArmorPageViewModel(session, item),
            EntityType.NATION => new NationPageViewModel(session, item),
            _ => new EntityPageViewModel(session, item),
        };
    }

    /// <summary>A spell: research, paths and cost; its effect, with #damage shown as what the effect reads it as.</summary>
    public sealed class SpellPageViewModel : EntityPageViewModel
    {
        public SpellPageViewModel(EditorSession session, EntityListItem item) : base(session, item) { }

        protected override void BuildPanels(HashSet<Command> covered)
        {
            var spell = new FieldsPanel("SPELL");
            spell.Fields.Add(new ChoiceField(this, "School", Command.SCHOOL, null, Data.GameTables.Schools));
            spell.Fields.Add(new NumberField(this, "Research level", Command.RESEARCHLEVEL));
            spell.Fields.Add(new ChoiceField(this, "Path", Command.PATH, "0", Data.GameTables.SpellPaths));
            spell.Fields.Add(new NumberField(this, "Path level", Command.PATHLEVEL, "0"));
            spell.Fields.Add(new ChoiceField(this, "Second path", Command.PATH, "1", Data.GameTables.SpellPaths, -1));
            spell.Fields.Add(new NumberField(this, "Second level", Command.PATHLEVEL, "1"));
            spell.Fields.Add(new NumberField(this, "Cost", Command.FATIGUECOST, note: CostNote,
                tooltip: "#fatiguecost: every 100 is one gem (or blood slave); rituals set their gem cost with it"));
            Panels.Add(spell);

            int? effect = int.TryParse(Resolved.Get(Command.EFFECT)?.Arguments, out var e) ? e : null;
            var type = effect is int n ? Data.GameTables.EffectOf(n) : null;
            var panel = new FieldsPanel("EFFECT");
            panel.Fields.Add(new ChoiceField(this, "Effect", Command.EFFECT, null, Data.GameTables.EffectChoices(effect)));
            if (type?.ArgumentType == "unit_id")
                panel.Fields.Add(new RefField(this, "Summons", Command.DAMAGE, EntityType.MONSTER,
                    tooltip: "#damage: the monster summoned (a negative number is a monster tag)"));
            else
                panel.Fields.Add(new NumberField(this, "Damage", Command.DAMAGE, note: v => DamageNote(type, v)));
            panel.Hint = type == null ? "" : $"{type.Name}: #damage is {Data.GameTables.ArgumentDescription(type.ArgumentType).ToLowerInvariant()}" +
                (type.Notes != null ? $" ({type.Notes})" : "");
            Panels.Add(panel);
            covered.UnionWith(new[] { Command.SCHOOL, Command.RESEARCHLEVEL, Command.PATH, Command.PATHLEVEL, Command.FATIGUECOST, Command.EFFECT, Command.DAMAGE });
        }

        private static string CostNote(string text)
        {
            if (!int.TryParse(text, out var cost))
                return "";
            int gems = cost / 100, fatigue = cost % 100;
            return gems > 0 ? $"{gems} gem{(gems == 1 ? "" : "s")}" + (fatigue > 0 ? $" + {fatigue} fatigue" : "") : $"{fatigue} fatigue";
        }

        private static string DamageNote(Data.GameTables.EffectType? type, string text)
        {
            if (type?.ArgumentType != "damage" || !int.TryParse(text, out var v) || Math.Abs(v) < 1000)
                return "";
            return $"{v % 1000} + {v / 1000} per caster level";
        }
    }

    /// <summary>A magic item: slot, paths to forge it, and the weapon or armor it gives.</summary>
    public sealed class ItemPageViewModel : EntityPageViewModel
    {
        public ItemPageViewModel(EditorSession session, EntityListItem item) : base(session, item) { }

        protected override void BuildPanels(HashSet<Command> covered)
        {
            var item = new FieldsPanel("ITEM");
            item.Fields.Add(new ChoiceField(this, "Type", Command.TYPE, null, Data.GameTables.ItemTypes));
            item.Fields.Add(new NumberField(this, "Construction", Command.CONSTLEVEL,
                tooltip: "#constlevel: the Construction research level needed to forge it"));
            item.Fields.Add(new ChoiceField(this, "Main path", Command.MAINPATH, null, Data.GameTables.Paths));
            item.Fields.Add(new NumberField(this, "Main level", Command.MAINLEVEL, defaultValue: "1",
                tooltip: "#mainlevel: the main path level needed (the game uses at least 1)"));
            item.Fields.Add(new ChoiceField(this, "Second path", Command.SECONDARYPATH, null, Data.GameTables.PathsOrNone, -1));
            item.Fields.Add(new NumberField(this, "Second level", Command.SECONDARYLEVEL));
            item.Fields.Add(new RefField(this, "Weapon", Command.WEAPON, EntityType.WEAPON, tooltip: "#weapon: the weapon its bearer gets"));
            item.Fields.Add(new RefField(this, "Armor", Command.ARMOR, EntityType.ARMOR, tooltip: "#armor: the armor its bearer gets"));
            Panels.Add(item);
            covered.UnionWith(new[] { Command.TYPE, Command.CONSTLEVEL, Command.MAINPATH, Command.MAINLEVEL,
                Command.SECONDARYPATH, Command.SECONDARYLEVEL, Command.WEAPON, Command.ARMOR });
        }
    }

    /// <summary>A nation: its recruits and commanders (home and foreign) as lists; the rest in its badge sections.</summary>
    public sealed class NationPageViewModel : EntityPageViewModel
    {
        public NationPageViewModel(EditorSession session, EntityListItem item) : base(session, item) { }

        protected override void BuildPanels(HashSet<Command> covered)
        {
            Panels.Add(new ReferenceListPanel(this, "RECRUITS", Command.ADDRECUNIT, EntityType.MONSTER, UnitSummary));
            Panels.Add(new ReferenceListPanel(this, "COMMANDERS", Command.ADDRECCOM, EntityType.MONSTER, UnitSummary));
            Panels.Add(new ReferenceListPanel(this, "FOREIGN RECRUITS", Command.ADDFOREIGNUNIT, EntityType.MONSTER, UnitSummary));
            Panels.Add(new ReferenceListPanel(this, "FOREIGN COMMANDERS", Command.ADDFOREIGNCOM, EntityType.MONSTER, UnitSummary));
            covered.UnionWith(new[] { Command.ADDRECUNIT, Command.ADDRECCOM, Command.ADDFOREIGNUNIT, Command.ADDFOREIGNCOM });
        }

        private string UnitSummary(int id)
        {
            if (id <= 0 || !Session.Mod.TryGet(EntityType.MONSTER, id, null, out var e))
                return "";
            var r = Session.Resolve(e);
            string V(Command c) => r.Get(c)?.Arguments ?? "-";
            var gold = int.TryParse(V(Command.GCOST), out var g) && g >= 5000 ? $"auto{(g - 10000 >= 0 ? "+" : "")}{g - 10000}" : V(Command.GCOST);
            return $"hp {V(Command.HP)}  att {V(Command.ATT)}  def {V(Command.DEF)}  prot {V(Command.PROT)}  gold {gold}";
        }
    }

    /// <summary>Armor: its type (shield, body, helmet, barding); protection and the rest in its stats.</summary>
    public sealed class ArmorPageViewModel : EntityPageViewModel
    {
        public ArmorPageViewModel(EditorSession session, EntityListItem item) : base(session, item) { }

        protected override void BuildPanels(HashSet<Command> covered)
        {
            var armor = new FieldsPanel("ARMOR");
            armor.Fields.Add(new ChoiceField(this, "Type", Command.TYPE, null, Data.GameTables.ArmorTypes,
                tooltip: "#type: which slot it's worn in; #prot protects the parts of that type"));
            Panels.Add(armor);
            covered.Add(Command.TYPE);
        }
    }

    /// <summary>A magic site: path, level and rarity, and its gem income by path.</summary>
    public sealed class SitePageViewModel : EntityPageViewModel
    {
        public SitePageViewModel(EditorSession session, EntityListItem item) : base(session, item) { }

        protected override void BuildPanels(HashSet<Command> covered)
        {
            var site = new FieldsPanel("SITE");
            site.Fields.Add(new ChoiceField(this, "Path", Command.PATH, null, Data.GameTables.Paths));
            site.Fields.Add(new ChoiceField(this, "Level", Command.LEVEL, null, Data.GameTables.SiteLevels,
                tooltip: "#level: the magic level needed to find it (0: found automatically)"));
            site.Fields.Add(new ChoiceField(this, "Rarity", Command.RARITY, null, Data.GameTables.SiteRarity));
            Panels.Add(site);
            Panels.Add(new KeyedListPanel(this, "GEMS", Command.GEMS, Data.GameTables.GemPaths, "per turn"));
            covered.UnionWith(new[] { Command.PATH, Command.LEVEL, Command.RARITY, Command.GEMS });
        }
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
            var costs = new FieldsPanel("COST, COMMAND, BODY");
            costs.Fields.Add(new NumberField(this, "Gold", Command.GCOST, note: GoldNote,
                tooltip: "#gcost: gold (and design points for pretenders); 10000 means calculated by the game, plus or minus the rest"));
            costs.Fields.Add(new NumberField(this, "Resources", Command.RCOST));
            costs.Fields.Add(new NumberField(this, "Recruit points", Command.RPCOST));
            costs.Fields.Add(new CommandChoiceField(this, "Leader", Leaders(Command.NOLEADER, Command.POORLEADER, Command.OKLEADER, Command.GOODLEADER, Command.EXPERTLEADER, Command.SUPERIORLEADER)));
            costs.Fields.Add(new CommandChoiceField(this, "Magic leader", Leaders(Command.NOMAGICLEADER, Command.POORMAGICLEADER, Command.OKMAGICLEADER, Command.GOODMAGICLEADER, Command.EXPERTMAGICLEADER, Command.SUPERIORMAGICLEADER), defaultIndex: 0));
            costs.Fields.Add(new CommandChoiceField(this, "Undead leader", Leaders(Command.NOUNDEADLEADER, Command.POORUNDEADLEADER, Command.OKUNDEADLEADER, Command.GOODUNDEADLEADER, Command.EXPERTUNDEADLEADER, Command.SUPERIORUNDEADLEADER), defaultIndex: 0));
            costs.Fields.Add(new NumberField(this, "Leadership +", Command.COMMAND, tooltip: "#command: adds this to the leadership the class gives"));
            costs.Fields.Add(new CommandChoiceField(this, "Body", new[]
            {
                (Command.HUMANOID, "Humanoid"), (Command.MOUNTEDHUMANOID, "Mounted humanoid"), (Command.QUADRUPED, "Quadruped"),
                (Command.LIZARD, "Lizard"), (Command.NAGA, "Naga"), (Command.SNAKE, "Snake"), (Command.BIRD, "Bird"),
                (Command.DJINN, "Djinn"), (Command.TROGLODYTE, "Troglodyte"), (Command.MISCSHAPE, "Other shape"),
            }, tooltip: "Body shape: hit locations; it also sets item slots (set them below after it)", defaultIndex: 0));
            Panels.Add(costs);
            Panels.Add(new ItemSlotsPanel(this));
            covered.Add(Command.ITEMSLOTS);
            covered.UnionWith(new[] { Command.WEAPON, Command.ARMOR, Command.MAGICSKILL, Command.CUSTOMMAGIC, Command.GCOST, Command.RCOST, Command.RPCOST, Command.COMMAND });
            foreach (var f in costs.Fields.OfType<CommandChoiceField>())
                covered.UnionWith(f.Commands);
        }

        private static IReadOnlyList<(Command, string)> Leaders(params Command[] tiers)
        {
            string[] names = { "None", "Poor", "OK", "Good", "Expert", "Superior" };
            return tiers.Select((c, i) => (c, names[i])).ToList();
        }

        private static string GoldNote(string text)
        {
            if (!int.TryParse(text, out var g) || g < 5000)
                return "";
            int rest = g - 10000;
            return rest == 0 ? "calculated by the game" : $"calculated by the game {(rest > 0 ? "+" : "-")} {Math.Abs(rest)}";
        }

        /// <summary>What the game uses when the monster sets none: resource size is its size; horrors have spirit sight.</summary>
        protected override string? GameDefault(Command c) => c switch
        {
            Command.RESSIZE => Resolved.Get(Command.SIZE)?.Arguments,
            Command.SPIRITSIGHT => Resolved.Has(Command.HORROR) ? "1" : null,
            _ => DerivedValue(c),
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
