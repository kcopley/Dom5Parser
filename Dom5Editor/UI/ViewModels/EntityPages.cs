using System.Collections.ObjectModel;
using System.Windows.Input;
using Dom5Editor.UI.Controls;
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
            EntityType.NAMETYPE => new NametypePageViewModel(session, item),
            EntityType.EVENT => new EventPageViewModel(session, item),
            EntityType.WEAPON => new WeaponPageViewModel(session, item),
            EntityType.MERCENARY or EntityType.POPTYPE or EntityType.BLESS or EntityType.TEMPLATE => new FormPageViewModel(session, item),
            _ => new EntityPageViewModel(session, item),
        };
    }

    /// <summary>
    /// A weapon: its stats as a block (damage, attacks, length; attack, defence, resources; range,
    /// ammunition, area), its damage type and how much strength it adds, and its flags in groups
    /// (kind of damage, element, what resists it, who it can't harm), each with the manual's hint.
    /// </summary>
    public sealed class WeaponPageViewModel : EntityPageViewModel
    {
        public WeaponPageViewModel(EditorSession session, EntityListItem item) : base(session, item) { }

        protected override void BuildPanels(HashSet<Command> covered)
        {
            NumberField Stat(string label, Command c, string icon, Func<string, string>? note = null)
            {
                covered.Add(c);
                return new NumberField(this, label, c, note: note) { Icon = icon };
            }
            var stats = new StatsPanel();
            var a = new StatsPanel.Column();
            a.Cells.Add(Stat("Damage", Command.DMG, "dmg"));
            a.Cells.Add(Stat("Attacks", Command.NRATT, "nratt", t => int.TryParse(t, out var n) && n < 0 ? $"one every {-n} rounds" : ""));
            a.Cells.Add(Stat("Length", Command.LEN, "len"));
            var b = new StatsPanel.Column();
            b.Cells.Add(Stat("Attack", Command.ATT, "att"));
            b.Cells.Add(Stat("Defence", Command.DEF, "def"));
            b.Cells.Add(Stat("Resources", Command.RCOST, "res"));
            var c = new StatsPanel.Column();
            c.Cells.Add(Stat("Range", Command.RANGE, "range", t => t.Length == 0 || t == "0" ? "melee" : ""));
            c.Cells.Add(Stat("Ammunition", Command.AMMO, "ammo"));
            c.Cells.Add(Stat("Area", Command.AOE, "aoe"));
            stats.Columns.Add(a);
            stats.Columns.Add(b);
            stats.Columns.Add(c);
            Panels.Add(stats);

            var damage = new FieldsPanel("DAMAGE");
            var types = new CommandChoiceField(this, "Damage type", new[]
            {
                (Command.DT_NORMAL, "Normal"), (Command.DT_STUN, "Fatigue (stun)"), (Command.DT_SIZESTUN, "Fatigue, less on large"),
                (Command.DT_REALSTUN, "Stun (100 is standard)"), (Command.DT_PARALYZE, "Paralyze"), (Command.DT_POISON, "Poison over rounds"),
                (Command.DT_CAP, "Capped (max 1 HP)"), (Command.DT_DEMON, "Anti-demon (x2)"), (Command.DT_HOLY, "Holy (x3 undead, demons)"),
                (Command.DT_MAGIC, "Anti-magic beings (x2)"), (Command.DT_SMALL, "Small targets (x2)"), (Command.DT_LARGE, "Large targets (x3)"),
                (Command.DT_CONSTRUCTONLY, "Inanimate only"), (Command.DT_RAISE, "Raises the killed"), (Command.DT_INTERRUPT, "Interrupt only"),
                (Command.DT_WEAKNESS, "Drains strength"), (Command.DT_DRAIN, "Drains life"), (Command.DT_WEAPONDRAIN, "Drains life (max 5)"),
                (Command.DT_AFF, "Affliction"), (Command.DT_BOUNCEKILL, "Bounces (chain)"),
            }, defaultIndex: 0);
            damage.Fields.Add(types);
            var str = new CommandChoiceField(this, "Strength added", new[]
            {
                (Command.FULLSTR, "All of it"), (Command.HALFSTR, "Half (bows)"), (Command.THIRDSTR, "A third (crossbows)"), (Command.NOSTR, "None"),
            }, defaultIndex: 0);
            damage.Fields.Add(str);
            damage.Fields.Add(new RefField(this, "Secondary effect", Command.SECONDARYEFFECT, EntityType.WEAPON,
                tooltip: "#secondaryeffect: a weapon (effect) that also hits when this one does damage"));
            damage.Fields.Add(new RefField(this, "Always also", Command.SECONDARYEFFECTALWAYS, EntityType.WEAPON,
                tooltip: "#secondaryeffectalways: a weapon (effect) that also hits whenever this one hits, damage or not"));
            Panels.Add(damage);
            covered.UnionWith(types.Commands);
            covered.UnionWith(str.Commands);
            covered.UnionWith(new[] { Command.SECONDARYEFFECT, Command.SECONDARYEFFECTALWAYS });

            var flags = new FlagsPanel("QUALITIES");
            flags.Add(this, covered, "Kind", (Command.SLASH, "Slash"), (Command.PIERCE, "Pierce"), (Command.BLUNT, "Blunt"),
                (Command.MAGIC, "Magic"), (Command.ARMORPIERCING, "Armor piercing"), (Command.ARMORNEGATING, "Armor negating"),
                (Command.TWOHANDED, "Two-handed"), (Command.CHARGE, "Charge"), (Command.FLAIL, "Flail"), (Command.BONUS, "Bonus weapon"));
            flags.Add(this, covered, "Element", (Command.FIRE, "Fire"), (Command.COLD, "Cold"), (Command.SHOCK, "Shock"), (Command.ACID, "Acid"),
                (Command.POISON, "Poison (immunity)"));
            flags.Add(this, covered, "Resisted by", (Command.MRNEGATES, "MR"), (Command.MRNEGATESEASILY, "MR, easily"), (Command.HARDMRNEG, "MR, with a penalty"),
                (Command.MRHALF, "MR halves it"), (Command.DEFROLL, "Defence roll"), (Command.MORROLL, "Morale roll"), (Command.SIZERESIST, "Size"));
            flags.Add(this, covered, "Can't harm", (Command.FRIENDLYIMMUNE, "Friends"), (Command.ENEMYIMMUNE, "Enemies"), (Command.UNDEADIMMUNE, "Undead"),
                (Command.INANIMATEIMMUNE, "Lifeless"), (Command.FLYINGIMMUNE, "Flyers"), (Command.ILLUSIONSIMMUNE, "Illusions"),
                (Command.SPIRITFORMIMMUNE, "Spirit forms"), (Command.MIND, "The mindless"));
            flags.Add(this, covered, "Only harms", (Command.SACREDONLY, "Sacred"), (Command.UNDEADONLY, "Undead"), (Command.DEMONONLY, "Demons"),
                (Command.DEMONUNDEAD, "Demons and undead"), (Command.MAGICONLY, "Magic beings"));
            Panels.Add(flags);
        }
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
            spell.Fields.Add(new ChoiceField(this, "Path", Command.PATH, "0", Data.GameTables.SpellPaths, -1));
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

            // range, precision, area and number of effects, with what their encoded values mean
            NumberField Stat(string label, Command c, string icon, Func<string, string> note)
            {
                covered.Add(c);
                return new NumberField(this, label, c, note: note) { Icon = icon };
            }
            var stats = new StatsPanel { Title = "COMBAT" };
            var left = new StatsPanel.Column();
            left.Cells.Add(Stat("Range", Command.RANGE, "range", t => int.TryParse(t, out var r) && r >= 5000 ? $"{r - 5000} + 5 per caster level" : ""));
            left.Cells.Add(Stat("Precision", Command.PRECISION, "prec", _ => ""));
            var right = new StatsPanel.Column();
            right.Cells.Add(Stat("Area", Command.AOE, "aoe", AreaNote));
            right.Cells.Add(Stat("Effects", Command.NREFF, "nratt", EffectsNote));
            stats.Columns.Add(left);
            stats.Columns.Add(right);
            Panels.Add(stats);

            // a global/province enchantment or a cause-event spell: the events it drives
            if (effect is int fx && (Dom5Edit.Events.EventInfo.IsEnchantmentEffect(fx) || Dom5Edit.Events.EventInfo.IsCauseEventEffect(fx)))
                Panels.Add(new SpellEventsPanel(this, fx));
            covered.UnionWith(new[] { Command.SCHOOL, Command.RESEARCHLEVEL, Command.PATH, Command.PATHLEVEL, Command.FATIGUECOST, Command.EFFECT, Command.DAMAGE });
        }

        /// <summary>#aoe: squares; 666 the whole battlefield, 662-665 a share of it; +1000 grows with the caster's level.</summary>
        private static string AreaNote(string text)
        {
            if (!int.TryParse(text, out var a))
                return "";
            return a switch
            {
                666 => "the whole battlefield",
                663 => "half the squares",
                665 => "a quarter of the squares",
                664 => "10% of the squares",
                662 => "5% of the squares",
                >= 1000 => $"{a % 1000} + 1 per caster level",
                _ => "",
            };
        }

        /// <summary>#nreff: +1000 per 1000 gives that many more per caster level; +500, one more per two levels above the requirement.</summary>
        private static string EffectsNote(string text)
        {
            if (!int.TryParse(text, out var n) || n < 500)
                return "";
            if (n % 1000 >= 500 && n < 1000)
                return $"{n - 500} + 1 per 2 levels above the requirement";
            return $"{n % 1000} + {n / 1000} per caster level";
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

    /// <summary>
    /// The events a spell drives: for an enchantment (its #damage is the enchantment number) the
    /// events that require it; for a cause-event spell, the events with its #id. And a button
    /// that makes a new one.
    /// </summary>
    public sealed class SpellEventsPanel
    {
        private readonly EntityPageViewModel _page;
        private readonly bool _enchantment;
        private readonly long _number;

        public SpellEventsPanel(EntityPageViewModel page, int effect)
        {
            _page = page;
            _enchantment = Dom5Edit.Events.EventInfo.IsEnchantmentEffect(effect);
            _number = Dom5Edit.Events.EventInfo.Number(page.Resolved.Get(Command.DAMAGE)?.Property) ?? 0;
            var graph = page.Session.Events;
            var spell = page.Session.Editor.OwnEntity(page.Entity) ?? page.Entity;
            // one link per event, with the lines it checks the spell with
            foreach (var g in graph.From(spell).Where(l => !l.IsEventToEvent).GroupBy(l => l.To, ReferenceEqualityComparer.Instance))
            {
                var e = (IDEntity)g.Key!;
                Events.Add(new LinkChip(string.Join(", ", g.Select(l => EventPageViewModel.Name(l.Checker?.Command ?? Command.ID)).Distinct()),
                    Dom5Edit.Events.EventInfo.Title(graph.LinesOf(e)), () => page.Session.Navigate(e)));
            }
            NewCommand = new RelayCommand(MakeEvent);
        }

        public string Title => _enchantment ? $"EVENTS OF ENCHANTMENT {_number}" : $"EVENTS IT CAUSES (EVENT ID {_number})";
        public string Hint => Events.Count == 0
            ? (_enchantment ? $"No event checks enchantment {_number} (#req_ench and the like)." : $"No event has #id {_number}.")
            : "";
        public bool HasHint => Hint.Length > 0;
        public ObservableCollection<LinkChip> Events { get; } = new ObservableCollection<LinkChip>();
        public ICommand NewCommand { get; }
        public string NewLabel => _enchantment ? "+ New event while it's active" : "+ New event it causes";

        private void MakeEvent()
        {
            IDEntity? made = null;
            _page.EditRun(_enchantment ? $"New event for enchantment {_number}" : $"New event with id {_number}", (ed, tx) =>
            {
                made = tx.Create(EntityType.EVENT, null);
                tx.Add(made, Command.RARITY, "5");
                if (_enchantment)
                {
                    tx.Add(made, Command.REQ_ENCH, _number.ToString());
                    tx.Add(made, Command.REQ_PERMONTH, "1");
                }
                else
                    tx.Add(made, Command.ID, _number.ToString());
                tx.Add(made, Command.MSG, $"\"{_page.DisplayName.Replace("\"", "'")}: what happens.\"");
            });
            if (made != null && _page.Error == null)
                _page.Session.Navigate(made);
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

        /// <summary>A recruit's key stats, as table columns.</summary>
        public static readonly IReadOnlyList<ReferenceListPanel.TableColumn> UnitColumns = new ReferenceListPanel.TableColumn[]
        {
            new("HP", "hp", r => r.Get(Command.HP)?.Arguments ?? "", "#hp: hit points"),
            new("Att", "att", r => r.Get(Command.ATT)?.Arguments ?? "", "#att: attack skill"),
            new("Def", "def", r => r.Get(Command.DEF)?.Arguments ?? "", "#def: defence skill"),
            new("Prot", "prot", r => r.Get(Command.PROT)?.Arguments ?? "", "#prot: natural protection"),
            new("MR", "mr", r => r.Get(Command.MR)?.Arguments ?? "", "#mr: magic resistance"),
            new("Gold", "gold", r => Gold(r.Get(Command.GCOST)?.Arguments), "#gcost: gold (\"auto\": worked out by the game, from a base)"),
            new("Res", "res", r => r.Get(Command.RCOST)?.Arguments ?? "", "#rcost: resources (weapons and armor add theirs)"),
        };

        private static string Gold(string? v) => int.TryParse(v, out var g) && g >= 5000 ? $"auto{(g - 10000 >= 0 ? "+" : "")}{g - 10000}" : v ?? "";

        protected override void BuildPanels(HashSet<Command> covered)
        {
            Panels.Add(new ReferenceListPanel(this, "RECRUITS", Command.ADDRECUNIT, EntityType.MONSTER, columns: UnitColumns));
            Panels.Add(new ReferenceListPanel(this, "COMMANDERS", Command.ADDRECCOM, EntityType.MONSTER, columns: UnitColumns));
            Panels.Add(new ReferenceListPanel(this, "FOREIGN RECRUITS", Command.ADDFOREIGNUNIT, EntityType.MONSTER, columns: UnitColumns));
            Panels.Add(new ReferenceListPanel(this, "FOREIGN COMMANDERS", Command.ADDFOREIGNCOM, EntityType.MONSTER, columns: UnitColumns));
            covered.UnionWith(new[] { Command.ADDRECUNIT, Command.ADDRECCOM, Command.ADDFOREIGNUNIT, Command.ADDFOREIGNCOM });

            var start = new ArmyPanel(this, covered, "STARTING ARMY", "What the nation starts with in its capital.");
            start.Add("Commander", Command.STARTCOM);
            start.Add("Scout", Command.STARTSCOUT);
            start.Add("Troops 1", Command.STARTUNITTYPE1, Command.STARTUNITNBRS1);
            start.Add("Troops 2", Command.STARTUNITTYPE2, Command.STARTUNITNBRS2);
            start.Add("Troops 3", Command.STARTUNITTYPE3, Command.STARTUNITNBRS3, always: false);
            Panels.Add(start);
            Panels.Add(new ReferenceListPanel(this, "START SITES", Command.STARTSITE, EntityType.SITE));
            covered.Add(Command.STARTSITE);

            var pd = new ArmyPanel(this, covered, "PROVINCE DEFENCE",
                "The numbers are units per 10 points of province defence (the manual: #defmult1 20 gives 2 units per point, the default for unit 1; the others default to 10).");
            pd.Add("Commander 1", Command.DEFCOM1);
            pd.Add("Commander 2", Command.DEFCOM2);
            pd.Add("Unit 1", Command.DEFUNIT1, Command.DEFMULT1, "per 10 PD:");
            pd.Add("Unit 1B", Command.DEFUNIT1B, Command.DEFMULT1B, "per 10 PD:");
            pd.Add("Unit 1C", Command.DEFUNIT1C, Command.DEFMULT1C, "per 10 PD:", always: false);
            pd.Add("Unit 1D", Command.DEFUNIT1D, Command.DEFMULT1D, "per 10 PD:", always: false);
            pd.Add("Unit 2", Command.DEFUNIT2, Command.DEFMULT2, "per 10 PD:");
            pd.Add("Unit 2B", Command.DEFUNIT2B, Command.DEFMULT2B, "per 10 PD:");
            pd.Heading("Walls and guards");
            pd.Add("Wall commander", Command.WALLCOM);
            pd.Add("Wall unit", Command.WALLUNIT, Command.WALLMULT, "×");
            pd.Add("Guard commander", Command.GUARDCOM);
            pd.Add("Guard unit", Command.GUARDUNIT, Command.GUARDMULT, "×");
            pd.Add("Guard spirit", Command.GUARDSPIRIT, always: false);
            int before = pd.Rows.Count;
            pd.Heading("Underwater");
            bool uw = pd.Add("Commander 1", Command.UWDEFCOM1, always: false) | pd.Add("Commander 2", Command.UWDEFCOM2, always: false)
                | pd.Add("Unit 1", Command.UWDEFUNIT1, Command.UWDEFMULT1, "per 10 PD:", always: false) | pd.Add("Unit 1B", Command.UWDEFUNIT1B, Command.UWDEFMULT1B, "per 10 PD:", always: false)
                | pd.Add("Unit 1C", Command.UWDEFUNIT1C, Command.UWDEFMULT1C, "per 10 PD:", always: false) | pd.Add("Unit 1D", Command.UWDEFUNIT1D, Command.UWDEFMULT1D, "per 10 PD:", always: false)
                | pd.Add("Unit 2", Command.UWDEFUNIT2, Command.UWDEFMULT2, "per 10 PD:", always: false) | pd.Add("Unit 2B", Command.UWDEFUNIT2B, Command.UWDEFMULT2B, "per 10 PD:", always: false)
                | pd.Add("Wall commander", Command.UWWALLCOM, always: false) | pd.Add("Wall unit", Command.UWWALLUNIT, Command.UWWALLMULT, "×", always: false)
                | pd.Add("Guard commander", Command.UWGUARDCOM, always: false) | pd.Add("Guard unit", Command.UWGUARDUNIT, Command.UWGUARDMULT, "×", always: false);
            if (!uw)
                pd.Rows.RemoveAt(before);
            before = pd.Rows.Count;
            pd.Heading("In conquered forts (foreign)");
            bool foreign = pd.Add("Wall commander", Command.FOREIGNWALLCOM, always: false) | pd.Add("Wall unit", Command.FOREIGNWALLUNIT, Command.FOREIGNWALLMULT, "×", always: false)
                | pd.Add("Guard commander", Command.FOREIGNGUARDCOM, always: false) | pd.Add("Guard unit", Command.FOREIGNGUARDUNIT, Command.FOREIGNGUARDMULT, "×", always: false);
            if (!foreign)
                pd.Rows.RemoveAt(before);
            Panels.Add(pd);

            Panels.Add(new ReferenceListPanel(this, "PRETENDERS ADDED", Command.ADDGOD, EntityType.MONSTER));
            Panels.Add(new ReferenceListPanel(this, "PRETENDERS REMOVED", Command.DELGOD, EntityType.MONSTER));
            Panels.Add(new ReferenceListPanel(this, "PRETENDERS 20% CHEAPER", Command.CHEAPGOD20, EntityType.MONSTER));
            Panels.Add(new ReferenceListPanel(this, "PRETENDERS 40% CHEAPER", Command.CHEAPGOD40, EntityType.MONSTER));
            covered.UnionWith(new[] { Command.ADDGOD, Command.DELGOD, Command.CHEAPGOD20, Command.CHEAPGOD40 });
        }
    }

    /// <summary>A name list (nametype): the names, one per line, edited as text.</summary>
    public sealed class NametypePageViewModel : EntityPageViewModel
    {
        public NametypePageViewModel(EditorSession session, EntityListItem item) : base(session, item) { }

        protected override void BuildPanels(HashSet<Command> covered)
        {
            Panels.Add(new NamesPanel(this));
            covered.Add(Command.ADDNAME);
        }
    }

    /// <summary>Armor: its type (shield, body, helmet, barding) and its stats as a block.</summary>
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
            NumberField Stat(string label, Command c, string icon)
            {
                covered.Add(c);
                return new NumberField(this, label, c) { Icon = icon };
            }
            var stats = new StatsPanel();
            var a = new StatsPanel.Column();
            a.Cells.Add(Stat("Protection", Command.PROT, "prot"));
            a.Cells.Add(Stat("Defence", Command.DEF, "def"));
            var b = new StatsPanel.Column();
            b.Cells.Add(Stat("Encumbrance", Command.ENC, "enc"));
            b.Cells.Add(Stat("Resources", Command.RCOST, "res"));
            stats.Columns.Add(a);
            stats.Columns.Add(b);
            Panels.Add(stats);
        }
    }

    /// <summary>
    /// A type with few commands (mercenary, poptype, bless, AI template): every command shown as
    /// a field, set or not, by its badge config's sections, so a new one has its form to fill;
    /// repeatable unit lists as tables, flags as checkboxes.
    /// </summary>
    public sealed class FormPageViewModel : EntityPageViewModel
    {
        public FormPageViewModel(EditorSession session, EntityListItem item) : base(session, item) { }

        protected override void BuildPanels(HashSet<Command> covered)
        {
            var config = Data.BadgeConfigLoader.LoadConfig(ConfigName(Type));
            if (config == null)
                return;
            foreach (var section in config.Sections)
            {
                var title = (section.DisplayName ?? section.Id).ToUpperInvariant();
                var fields = new FieldsPanel(title);
                var flags = new FlagsPanel(title);
                var lists = new List<object>();
                foreach (var def in section.Commands)
                {
                    if (!Data.BadgeConfigLoader.TryGetCommand(def, out var c) || !Entity.GetPropertyMap().ContainsKey(c) || covered.Contains(c))
                        continue;
                    var label = def.Display ?? def.Name;
                    var kind = (def.Type ?? "flag").ToLowerInvariant();
                    var refType = Data.BadgeConfigLoader.GetEntityTypeFromRefType(def.RefType ?? "");
                    if (kind == "flag")
                    {
                        flags.Add(this, covered, "", (c, label));
                        continue;
                    }
                    covered.Add(c);
                    if (kind == "ref" && refType is EntityType rt)
                    {
                        if (Dom5Edit.Resolve.GameRules.IsRepeatable(Type, c))
                            lists.Add(new ReferenceListPanel(this, c switch
                            {
                                Command.ADDRECUNIT => "RECRUITS", Command.ADDRECCOM => "RECRUITABLE COMMANDERS", _ => label.ToUpperInvariant(),
                            }, c, rt, columns: rt == EntityType.MONSTER ? NationPageViewModel.UnitColumns : null));
                        else
                            fields.Fields.Add(new RefField(this, label, c, rt));
                    }
                    else if (System.Text.RegularExpressions.Regex.IsMatch(def.Name, @"^path\d$"))
                        fields.Fields.Add(new ChoiceField(this, label, c, null, Data.GameTables.PathsOrNone, -1));
                    else
                        fields.Fields.Add(new NumberField(this, label, c));
                }
                if (fields.Fields.Count > 0)
                    Panels.Add(fields);
                if (flags.Groups.Count > 0)
                    Panels.Add(fields.Fields.Count > 0 ? flags.Untitled() : flags);
                foreach (var l in lists)
                    Panels.Add(l);
            }
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

    /// <summary>
    /// A monster: its stats laid out like the game's unit window, weapons and armor as tables, magic,
    /// cost, body and item slots in panels; the rest in its badge sections.
    /// </summary>
    public sealed class MonsterPageViewModel : EntityPageViewModel
    {
        public MonsterPageViewModel(EditorSession session, EntityListItem item) : base(session, item) { }

        protected override IEnumerable<string> PanelSections => new[] { "magicpaths", "magic" };

        public static readonly IReadOnlyList<ReferenceListPanel.TableColumn> WeaponColumns = new ReferenceListPanel.TableColumn[]
        {
            new("Dmg", "dmg", r => r.Get(Command.DMG)?.Arguments ?? "", "#dmg: damage (strength is added for melee)"),
            new("Att", "att", r => Signed(r.Get(Command.ATT)?.Arguments), "#att: attack bonus"),
            new("Def", "def", r => Signed(r.Get(Command.DEF)?.Arguments), "#def: defence bonus"),
            new("Len", "len", r => r.Get(Command.LEN)?.Arguments ?? "", "#len: length"),
            new("×", "nratt", r => r.Get(Command.NRATT)?.Arguments ?? "1", "#nratt: attacks per round (negative: one attack every N rounds)"),
            new("Range", null, r => r.Get(Command.RANGE)?.Arguments ?? "", "#range: missile range (blank: melee)"),
        };

        public static readonly IReadOnlyList<ReferenceListPanel.TableColumn> ArmorColumns = new ReferenceListPanel.TableColumn[]
        {
            new("Prot", "prot", r => r.Get(Command.PROT)?.Arguments ?? "", "#prot: protection"),
            new("Def", "def", r => Signed(r.Get(Command.DEF)?.Arguments), "#def: defence modifier (shields: parry)"),
            new("Enc", "enc", r => r.Get(Command.ENC)?.Arguments ?? "", "#enc: encumbrance"),
            new("Type", null, r => ArmorType(r.Get(Command.TYPE)?.Arguments), "#type: shield, body armor, helmet, barding"),
        };

        private static string Signed(string? v) => int.TryParse(v, out var n) && n > 0 ? "+" + n : v ?? "";

        private static string ArmorType(string? v) => v switch
        {
            "4" => "Shield", "5" => "Body", "6" => "Helmet", "8" => "Misc", "9" => "Barding", null => "", _ => v,
        };

        protected override void BuildPanels(HashSet<Command> covered)
        {
            Panels.Add(BuildStats(covered));
            Panels.Add(new ReferenceListPanel(this, "WEAPONS", Command.WEAPON, EntityType.WEAPON, columns: WeaponColumns));
            Panels.Add(new ReferenceListPanel(this, "ARMOR", Command.ARMOR, EntityType.ARMOR, columns: ArmorColumns));
            Panels.Add(new MagicPanel(this));
            var body = new FieldsPanel("BODY");
            body.Fields.Add(new CommandChoiceField(this, "Body", new[]
            {
                (Command.HUMANOID, "Humanoid"), (Command.MOUNTEDHUMANOID, "Mounted humanoid"), (Command.QUADRUPED, "Quadruped"),
                (Command.LIZARD, "Lizard"), (Command.NAGA, "Naga"), (Command.SNAKE, "Snake"), (Command.BIRD, "Bird"),
                (Command.DJINN, "Djinn"), (Command.TROGLODYTE, "Troglodyte"), (Command.MISCSHAPE, "Other shape"),
            }, tooltip: "Body shape: hit locations; it also sets item slots (set them below after it)", defaultIndex: 0));
            Panels.Add(body);
            Panels.Add(new ItemSlotsPanel(this));
            covered.Add(Command.ITEMSLOTS);
            covered.UnionWith(new[] { Command.WEAPON, Command.ARMOR, Command.MAGICSKILL, Command.CUSTOMMAGIC });
            foreach (var f in body.Fields.OfType<CommandChoiceField>())
                covered.UnionWith(f.Commands);
        }

        /// <summary>The game's unit window: body, combat, movement and age; leadership at the foot of each; cost below.</summary>
        private StatsPanel BuildStats(HashSet<Command> covered)
        {
            NumberField Stat(string label, Command c, string icon, string tip, Func<string, string>? note = null)
            {
                covered.Add(c);
                return new NumberField(this, label, c, defaultValue: GameDefault(c), note: note, tooltip: tip) { Icon = icon };
            }
            LeaderField Leader(string label, string icon, Command bonus, int? dflt, string tip, params Command[] tiers)
            {
                covered.UnionWith(tiers);
                covered.Add(bonus);
                return new LeaderField(this, label, icon, tiers, bonus, dflt, tip);
            }

            var stats = new StatsPanel();
            var body = new StatsPanel.Column();
            body.Cells.Add(Stat("Hit points", Command.HP, "hp", "#hp: maximum hit points. A human has 10, a giant 30, a huge dragon 125."));
            body.Cells.Add(Stat("Size", Command.SIZE, "size", "#size: 1 bug, 2 hoburg, 3 human or wolf, 4 bandar or lion, 5 enkidu or horse, 6 jotun or ice drake, 7 anakite, 8 small titan, 9 large titan or elephant, 10 dragon. Flyers usually get 1 more."));
            body.Cells.Add(Stat("Protection", Command.PROT, "prot", "#prot: natural protection (0 for humans, 5 a lizardman, about 18 a scaly dragon); armor adds to it."));
            body.Cells.Add(Stat("Magic resistance", Command.MR, "mr", "#mr: magic resistance. A human has 10, 1st level mages 13, 3rd level mages 15; above 18 only void beings."));
            body.Cells.Add(Stat("Morale", Command.MOR, "mor", "#mor: morale. A human soldier has 10, a minotaur 13. 50 makes it mindless (it dissolves without leadership); undead usually have 30."));
            body.Cells.Add(Leader("Leadership", "leader", Command.COMMAND, null, "Leadership class: how many units it can lead (OK, 50, is the standard for commanders; poor, 10, for mages).",
                Command.NOLEADER, Command.POORLEADER, Command.OKLEADER, Command.GOODLEADER, Command.EXPERTLEADER, Command.SUPERIORLEADER));
            var combat = new StatsPanel.Column();
            combat.Cells.Add(Stat("Strength", Command.STR, "str", "#str: strength, added to melee damage. A human soldier has 10, a giant 20, a dragon 25 or more."));
            combat.Cells.Add(Stat("Attack", Command.ATT, "att", "#att: attack skill. A human soldier has 10; only the elite of the elite 15."));
            combat.Cells.Add(Stat("Defence", Command.DEF, "def", "#def: defence skill. A human soldier has 10."));
            combat.Cells.Add(Stat("Precision", Command.PREC, "prec", "#prec: precision, for missiles and spells. A human archer has 10."));
            combat.Cells.Add(Stat("Combat speed", Command.AP, "ap", "#ap: combat speed (action points) when unencumbered: about 12 for a human, 20 a knight, 25 light cavalry."));
            combat.Cells.Add(Leader("Magic leadership", "mleader", Command.MAGICCOMMAND, 0, "Magic leadership: how many magic beings it can lead.",
                Command.NOMAGICLEADER, Command.POORMAGICLEADER, Command.OKMAGICLEADER, Command.GOODMAGICLEADER, Command.EXPERTMAGICLEADER, Command.SUPERIORMAGICLEADER));
            var move = new StatsPanel.Column();
            move.Cells.Add(Stat("Map move", Command.MAPMOVE, "mapmove", "#mapmove: speed on the world map. A human has 14, a horse 20; 0 can't move (except by rituals). Armor lowers it, a mount raises it. Old values 1-3 are converted."));
            move.Cells.Add(Stat("Encumbrance", Command.ENC, "enc", "#enc: encumbrance. Humans have 3; undead and machines 0 (they never tire from fighting, only from spells)."));
            move.Cells.Add(Stat("Resource size", Command.RESSIZE, "ressize", "#ressize: the size its resource cost is worked out from (1-10); not set: its size. Use 3 for a human-sized flier of size 4."));
            move.Cells.Add(Stat("Start age", Command.STARTAGE, "age", "#startage: its age when it appears. Usually not set: the game works it out from max age and skills. 0 clears it, -1 means age 0.",
                t => t.Length == 0 ? "auto" : ""));
            move.Cells.Add(Stat("Max age", Command.MAXAGE, "age", "#maxage: after this age it risks afflictions and death. Default 50 for humans, 500 undead, 1000 demons; each magic level adds half of it.",
                t => t.Length == 0 ? "default" : ""));
            move.Cells.Add(Leader("Undead leadership", "uleader", Command.UNDCOMMAND, 0, "Undead leadership: how many undead (and demons) it can lead.",
                Command.NOUNDEADLEADER, Command.POORUNDEADLEADER, Command.OKUNDEADLEADER, Command.GOODUNDEADLEADER, Command.EXPERTUNDEADLEADER, Command.SUPERIORUNDEADLEADER));
            stats.Columns.Add(body);
            stats.Columns.Add(combat);
            stats.Columns.Add(move);

            stats.Footer.Add(Stat("Gold", Command.GCOST, "gold", "#gcost: gold cost (design points for pretenders). Most human troops cost 10. Add 10000 to the base price to have the game work it out (age, magic, skills): 10010 is a base of 10.", GoldNote));
            stats.Footer.Add(Stat("Resources", Command.RCOST, "res", "#rcost: resource cost; its weapons' and armor's costs are added. Most human troops have 1."));
            stats.Footer.Add(Stat("Recruit points", Command.RPCOST, "rp", "#rpcost: recruitment points. 1 is standard for a simple commander, about 10 for a soldier. A base times 1000 has the game work it out (10000: base 10).",
                t => int.TryParse(t, out var rp) && rp >= 1000 ? $"worked out by the game (base {rp / 1000})" : ""));
            return stats;
        }

        private static string GoldNote(string text)
        {
            if (!int.TryParse(text, out var g) || g < 5000)
                return "";
            int rest = g - 10000;
            return rest == 0 ? "worked out by the game" : $"worked out by the game {(rest > 0 ? "+" : "-")} {Math.Abs(rest)}";
        }

        /// <summary>What the game uses when the monster sets none: resource size is its size; horrors have spirit sight.</summary>
        protected override string? GameDefault(Command c) => c switch
        {
            Command.RESSIZE => Resolved.Get(Command.SIZE)?.Arguments,
            Command.SPIRITSIGHT => Resolved.Has(Command.HORROR) ? "1" : null,
            _ => DerivedValue(c),
        };

    }
}
