using System.Collections.ObjectModel;
using System.Windows.Input;
using Dom5Editor.UI.Controls;
using Dom5Edit.Commands;
using Dom5Edit.Derived;
using Dom5Edit.Entities;
using Dom5Edit.Resolve;
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
            // how a unit uses it (the inspector's notes): the strength its damage gets, precision for a missile weapon
            var w = WeaponStats.Of(Resolved);
            NumberField Stat(string label, Command c, string icon, Func<string, string>? note = null, string derived = "", string tip = "")
            {
                covered.Add(c);
                return new NumberField(this, label, c, note: note) { Icon = icon, Derived = derived, DerivedTip = tip, DerivedWidth = 72 };
            }
            var (strength, strengthTip) = w.Dmg == null ? ("", "")
                : w.Strength switch
                {
                    StrengthAdded.None => ("(no str)", "The wielder's strength isn't added to its damage (#nostr)"),
                    StrengthAdded.Half => ("(+str/2)", $"In battle half the wielder's strength is added: a strength 10 soldier does {w.Dmg + 5}"),
                    StrengthAdded.Third => ("(+str/3)", $"In battle a third of the wielder's strength is added: a strength 10 soldier does {w.Dmg + 3}"),
                    _ when w.TwoHanded && !w.Missile => ("(+str x1.25)", $"Two-handed: in battle the wielder's strength x1.25 is added: a strength 10 soldier does {w.Dmg + 12}"),
                    _ => ("(+str)", $"In battle the wielder's strength is added: a strength 10 soldier does {w.Dmg + 10}"),
                };
            var stats = new StatsPanel();
            var a = new StatsPanel.Column();
            a.Cells.Add(Stat("Damage", Command.DMG, "dmg", derived: strength, tip: strengthTip));
            a.Cells.Add(Stat("Attacks", Command.NRATT, "nratt", t => int.TryParse(t, out var n) && n < 0 ? $"one every {-n} rounds" : ""));
            a.Cells.Add(Stat("Length", Command.LEN, "len"));
            var b = new StatsPanel.Column();
            b.Cells.Add(Stat("Attack", Command.ATT, "att", derived: w.Missile ? "(precision)" : "",
                tip: w.Missile ? "A missile weapon (it has a range): its #att is added to the unit's precision, not attack" : ""));
            b.Cells.Add(Stat("Defence", Command.DEF, "def"));
            b.Cells.Add(Stat("Resources", Command.RCOST, "res"));
            var c = new StatsPanel.Column();
            c.Cells.Add(Stat("Range", Command.RANGE, "range", t => t.Length == 0 || t == "0" ? "melee" : "",
                derived: w.Range is <= -1 and >= -5 ? (w.Range == -1 ? "(str)" : $"(str/{-w.Range})") : "",
                tip: w.Range < 0 ? $"A negative range is the wielder's strength{(w.Range < -1 ? $" divided by {-w.Range}" : "")}: {10 / -w.Range} for a strength 10 soldier" : ""));
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

        /// <summary>
        /// The construction levels the manual lists (#constlevel "can be 1,3,5,7,9 or 11,13,15 for items
        /// that cannot be forged"; vanilla's items use only these), and the item's own if it's another.
        /// </summary>
        private static IReadOnlyList<ChoiceOption> ConstLevels(int? current)
        {
            var list = new List<ChoiceOption>
            {
                new(1, "1"), new(3, "3"), new(5, "5"), new(7, "7"), new(9, "9"),
                new(11, "11: can't be forged"), new(13, "13: can't be forged, a unique artifact"), new(15, "15: can't be forged, unique per nation"),
            };
            if (current is int c && !list.Any(o => o.Value == c))
                list.Insert(0, new ChoiceOption(c, $"{c} (not a level the manual lists)"));
            return list;
        }

        protected override void BuildPanels(HashSet<Command> covered)
        {
            // what it costs to forge (gems by path level) and what its weapon and armor are (the inspector's notes)
            int? Int(Command c) => UnitStats.Int(Resolved, c);
            (string Text, string Tip) Gems(int? path, int level, Command cost)
            {
                int pct = Int(cost) ?? 0;
                if (path is not int p || p < 0 || p >= MagicPanel.PathNames.Length || ItemCost.Gems(level, pct) is not int g)
                    return ("", "");
                string what = p == 8 ? "blood slaves" : MagicPanel.PathNames[p].ToLowerInvariant() + " gems";
                return ($"({g} {what})", $"Forging it costs {g} {what} for this path: 5, 10, 15, 25, 40, 60, 80, 100, 120 for levels 1 to 9"
                    + (pct != 0 ? $", {(pct > 0 ? "+" : "")}{pct}% ({EntityPageViewModel.CommandName(cost)})" : ""));
            }
            var main = Gems(Int(Command.MAINPATH) ?? 0, Math.Max(Int(Command.MAINLEVEL) ?? 1, 1), Command.ITEMCOST1);
            var second = Int(Command.SECONDARYPATH) is int sp && sp >= 0 ? Gems(sp, Int(Command.SECONDARYLEVEL) ?? 0, Command.ITEMCOST2) : ("", "");
            ResolvedEntity? Ref(Command c, EntityType t) =>
                UnitStats.RefId(Resolved.Get(c)?.Property) is int id && id > 0 && Session.Mod.TryGet(t, id, null, out var e) ? Session.Resolve(e) : null;
            var weapon = Ref(Command.WEAPON, EntityType.WEAPON) is ResolvedEntity rw ? WeaponStats.Of(rw) : null;
            var armorGiven = Ref(Command.ARMOR, EntityType.ARMOR) is ResolvedEntity ra ? ArmorStats.Of(ra) : null;
            string Signed(int n) => n > 0 ? "+" + n : n.ToString();

            var item = new FieldsPanel("ITEM");
            item.Fields.Add(new ChoiceField(this, "Type", Command.TYPE, null, Data.GameTables.ItemTypes));
            int? constLevel = Int(Command.CONSTLEVEL);
            item.Fields.Add(new ChoiceField(this, "Construction", Command.CONSTLEVEL, null, ConstLevels(constLevel),
                tooltip: "#constlevel: the Construction research level needed to forge it: 1, 3, 5, 7 or 9 (11, 13, 15: it can't be forged)"));
            item.Fields.Add(new ChoiceField(this, "Main path", Command.MAINPATH, null, Data.GameTables.Paths));
            item.Fields.Add(new NumberField(this, "Main level", Command.MAINLEVEL, defaultValue: "1",
                tooltip: "#mainlevel: the main path level needed (the game uses at least 1)") { Derived = main.Text, DerivedTip = main.Tip });
            item.Fields.Add(new ChoiceField(this, "Second path", Command.SECONDARYPATH, null, Data.GameTables.PathsOrNone, -1));
            item.Fields.Add(new NumberField(this, "Second level", Command.SECONDARYLEVEL) { Derived = second.Item1, DerivedTip = second.Item2 });
            item.Fields.Add(new RefField(this, "Weapon", Command.WEAPON, EntityType.WEAPON, tooltip: "#weapon: the weapon its bearer gets")
            {
                Derived = weapon == null ? "" : $"(dmg {weapon.Dmg?.ToString() ?? "-"}, {(weapon.Missile ? "prec" : "att")} {Signed(weapon.Att)}, {(weapon.Missile ? $"range {weapon.Range}" : $"len {weapon.Len}")})",
                DerivedTip = weapon == null ? "" : $"{weapon.Name}: damage {weapon.Dmg?.ToString() ?? "(an effect)"}" + (weapon.Strength switch
                {
                    StrengthAdded.None => ", no strength added", StrengthAdded.Half => " + half the bearer's strength",
                    StrengthAdded.Third => " + a third of the bearer's strength", _ => " + the bearer's strength",
                }) + $"; {(weapon.Missile ? "precision" : "attack")} {Signed(weapon.Att)}, defence {Signed(weapon.Def)}",
            });
            item.Fields.Add(new RefField(this, "Armor", Command.ARMOR, EntityType.ARMOR, tooltip: "#armor: the armor its bearer gets")
            {
                Derived = armorGiven == null ? "" : $"(prot {(armorGiven.Type == ArmorStats.Shield ? armorGiven.Prot : Math.Max(armorGiven.ProtBody, armorGiven.ProtHead))}, def {Signed(armorGiven.Def)}, enc {armorGiven.Enc})",
                DerivedTip = armorGiven == null ? "" : $"{armorGiven.Name}: protection head {armorGiven.ProtHead}, body {armorGiven.ProtBody}"
                    + (armorGiven.Type == ArmorStats.Shield ? $"; a shield: parry {armorGiven.Parry}, {armorGiven.DefShown} defence" : $"; defence {Signed(armorGiven.Def)}")
                    + $", encumbrance {armorGiven.Enc}",
            });
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

        private static readonly IReadOnlyList<ChoiceOption> Eras = new[]
        {
            new ChoiceOption(1, "Early"), new ChoiceOption(2, "Middle"), new ChoiceOption(3, "Late"), new ChoiceOption(0, "Disabled (0: not playable)"),
        };

        protected override void BuildPanels(HashSet<Command> covered)
        {
            // what a new nation needs first: its epithet and era (written right after the name), its colors
            var nation = new FieldsPanel("NATION");
            nation.Fields.Add(new NumberField(this, "Epithet", Command.EPITHET, tooltip: "#epithet: shown after the name (\"Enigma of Steel\")") { BoxWidth = 300 });
            nation.Fields.Add(new ChoiceField(this, "Era", Command.ERA, null, Eras,
                tooltip: "#era: the era the nation is played in (not set: middle). Saved right after the name and epithet, as the manual asks"));
            nation.Fields.Add(new NumberField(this, "Color", Command.COLOR, note: t => t.Length == 0 ? "not set: black" : "red green blue, each 0 to 1",
                tooltip: "#color: the nation's color in the score graphs, its background and flag (red green blue, each 0 to 1)") { BoxWidth = 140 });
            nation.Fields.Add(new NumberField(this, "Second color", Command.SECONDARYCOLOR, note: t => t.Length == 0 ? "not set: the background uses the color" : "red green blue, each 0 to 1",
                tooltip: "#secondarycolor: the background's second color and the flag's border (red green blue, each 0 to 1)") { BoxWidth = 140 });
            Panels.Add(nation);
            covered.UnionWith(new[] { Command.EPITHET, Command.ERA, Command.COLOR, Command.SECONDARYCOLOR });

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
            // what a unit gets from it (the inspector's notes): a shield's parry, protection by part, body armor's map move penalty
            var s = ArmorStats.Of(Resolved);
            NumberField Stat(string label, Command c, string icon, string derived = "", string tip = "")
            {
                covered.Add(c);
                return new NumberField(this, label, c) { Icon = icon, Derived = derived, DerivedTip = tip, DerivedWidth = 84 };
            }
            // by part, when the box doesn't say it (#protparts, or parts no command sets)
            string part = Resolved.Has(Command.PROT) && s.General == 0 ? ""
                : s.ProtHead != 0 && s.ProtBody != 0 ? $"(h{s.ProtHead} b{s.ProtBody})"
                : s.ProtHead != 0 ? $"(head {s.ProtHead})" : s.ProtBody != 0 ? $"(body {s.ProtBody})" : "";
            string partTip = $"Protection by part: head {s.ProtHead}, body {s.ProtBody}"
                + (s.General != 0 ? $"\n{s.General} of it protects every part (the game's data; no command sets it)" : "")
                + "\nA unit's protection: (body x4 + head) / 5, each part's armor a over natural protection p as p + a - p x a / 40";
            var stats = new StatsPanel();
            var a = new StatsPanel.Column();
            a.Cells.Add(Stat("Protection", Command.PROT, "prot", part, part.Length > 0 ? partTip : ""));
            a.Cells.Add(Stat("Defence", Command.DEF, "def", s.Type == ArmorStats.Shield ? $"(parry {s.Parry})" : "",
                s.Type == ArmorStats.Shield
                    ? $"A shield's #def is its parry less its encumbrance: parry {s.Parry} = {s.Def} + {s.Enc}.\nA unit gets +{s.Parry} shield parry and {s.DefShown} for its encumbrance (net {s.Def})."
                    : ""));
            var b = new StatsPanel.Column();
            b.Cells.Add(Stat("Encumbrance", Command.ENC, "enc", s.MovePen > 0 ? $"(map move -{s.MovePen})" : "",
                s.MovePen > 0
                    ? $"Body armor slows a unit on the map: -{s.MovePen}" + (s.MovePenAbility != null ? " (the game's own value)" : $": twice its encumbrance{(s.Magic ? " less one (magic armor)" : "")}, at most 6")
                      + ".\nHalf for a unit with no encumbrance of its own; none with #nomovepen. In battle a unit's combat speed drops by its armor's encumbrance (half when mounted)."
                    : ""));
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
                    // (a clear, #clearrec, is a line of the CLEARS picker at the top, saved before the lists;
                    // as a checkbox here it showed off while set, the resolver keeping it with the copies)
                    if (!Data.BadgeConfigLoader.TryGetCommand(def, out var c) || !Entity.GetPropertyMap().ContainsKey(c) || covered.Contains(c)
                        || Dom5Edit.Resolve.GameRules.IsClear(c))
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

            // the units the site gives: recruitable by whoever owns it, or by its first owner only; summoned each month
            foreach (var (c, title) in new[]
            {
                (Command.MON, "RECRUITABLE UNITS"), (Command.COM, "RECRUITABLE COMMANDERS"),
                (Command.HOMEMON, "UNITS FOR ITS FIRST OWNER ONLY"), (Command.HOMECOM, "COMMANDERS FOR ITS FIRST OWNER ONLY"),
                (Command.SUMMON, "SUMMONED BY ENTERING IT (A MAGE OF ITS PATH)"),
            })
            {
                if (!Entity.GetPropertyMap().ContainsKey(c))
                    continue;
                // (shown when set; the others stay addable in the badges)
                if (!Resolved.Has(c) && c != Command.MON && c != Command.COM)
                    continue;
                Panels.Add(new ReferenceListPanel(this, title, c, EntityType.MONSTER, columns: NationPageViewModel.UnitColumns));
                covered.Add(c);
            }
        }
    }

    /// <summary>
    /// A monster: its stats laid out like the game's unit window, weapons and armor as tables, magic,
    /// cost, body and item slots in panels; the rest in its badge sections. Next to a stat, in
    /// brackets, what the game makes of it with the monster's gear, paths and age (Dom5Edit.Derived:
    /// the dom6inspector's formulas), and in the weapon table each weapon's attack and damage as
    /// this unit wields it.
    /// </summary>
    public sealed class MonsterPageViewModel : EntityPageViewModel
    {
        public MonsterPageViewModel(EditorSession session, EntityListItem item) : base(session, item) { }

        protected override IEnumerable<string> PanelSections => new[] { "magicpaths", "magic" };

        /// <summary>What the game makes of the monster's stats (worked out on each rebuild of the page).</summary>
        public UnitTotals? Totals { get; private set; }

        /// <summary>Why the totals take it as a commander or a unit (map move, path resistances, gold).</summary>
        public string RoleNote { get; private set; } = "";

        private IReadOnlyList<ReferenceListPanel.TableColumn> WeaponColumns()
        {
            WeaponTotals? Of(ResolvedValue v) => Totals?.Weapons.FirstOrDefault(w => ReferenceEquals(w.Weapon.Line, v));
            return new ReferenceListPanel.TableColumn[]
            {
                new("Dmg", "dmg", r => r.Get(Command.DMG)?.Arguments ?? "", "#dmg: damage. In brackets: with the strength the game adds, as this unit wields it")
                {
                    Width = 72,
                    ForRow = (v, r) =>
                    {
                        var dmg = r.Get(Command.DMG)?.Arguments ?? "";
                        if (Of(v) is not WeaponTotals w || w.Damage is not int total || w.StrengthAdded == 0)
                            return (dmg, null);
                        return ($"{dmg} ({total})", $"Damage {total}: {w.Weapon.Dmg} + {w.StrengthAdded} ({w.StrengthNote}, strength {Totals!.Str.Value})");
                    },
                },
                new("Att", "att", r => Signed(r.Get(Command.ATT)?.Arguments), "#att: attack bonus (a missile weapon's: precision). In brackets: this unit's attack (precision) with it")
                {
                    Width = 72,
                    ForRow = (v, r) =>
                    {
                        var att = Signed(r.Get(Command.ATT)?.Arguments);
                        if (Of(v) is not WeaponTotals w)
                            return (att, null);
                        string what = w.Weapon.Missile ? "Precision" : "Attack";
                        return ($"{(att.Length > 0 ? att : "0")} ({w.Attack})", $"{what} with it: {w.Attack} ({Parts(w.AttackParts)})" +
                            (w.Weapon.Missile ? "\nA missile weapon: its #att is precision" : ""));
                    },
                },
                new("Def", "def", r => Signed(r.Get(Command.DEF)?.Arguments), "#def: defence bonus (added to the unit's defence)"),
                new("Len", "len", r => r.Get(Command.LEN)?.Arguments ?? "", "#len: length. In brackets: the length this unit fights at, when it isn't")
                {
                    ForRow = (v, r) =>
                    {
                        var len = r.Get(Command.LEN)?.Arguments ?? "";
                        if (Of(v) is not WeaponTotals w || w.Weapon.Missile || len == w.Reach.ToString())
                            return (len, null);
                        return ($"{len} ({w.Reach})", w.ReachNote.Length > 0 ? $"Length {w.Reach}: {w.ReachNote}" : $"Length {w.Reach} (never below 0)");
                    },
                },
                new("×", "nratt", r => r.Get(Command.NRATT)?.Arguments ?? "1", "#nratt: attacks per round (negative: one attack every N rounds)"),
                new("Range", null, r => r.Get(Command.RANGE)?.Arguments ?? "", "#range: missile range (blank: melee). In brackets: this unit's range, when it depends on its strength")
                {
                    ForRow = (v, r) =>
                    {
                        var range = r.Get(Command.RANGE)?.Arguments ?? "";
                        if (Of(v) is not WeaponTotals w || !w.Weapon.Missile || range == w.Reach.ToString())
                            return (range, null);
                        return ($"{range} ({w.Reach})", $"Range {w.Reach}" + (w.ReachNote.Length > 0 ? $": {w.ReachNote} ({Totals!.Str.Value})" : ""));
                    },
                },
            };
        }

        private IReadOnlyList<ReferenceListPanel.TableColumn> ArmorColumns()
        {
            ArmorStats? Of(ResolvedValue v) => Totals?.Unit.Armor.FirstOrDefault(a => ReferenceEquals(a.Line, v));
            return new ReferenceListPanel.TableColumn[]
            {
                new("Prot", "prot", r => r.Get(Command.PROT)?.Arguments ?? "", "#prot: protection (#protparts: head and body)")
                {
                    ForRow = (v, r) =>
                    {
                        var prot = r.Get(Command.PROT)?.Arguments ?? "";
                        if (prot.Length > 0 || Of(v) is not ArmorStats a || a.ProtBody == 0 && a.ProtHead == 0)
                            return (prot, null);
                        return ($"h{a.ProtHead} b{a.ProtBody}", $"Head {a.ProtHead}, body {a.ProtBody}" + (a.General != 0 ? $" ({a.General} of it for both, from the game's data)" : ""));
                    },
                },
                new("Def", "def", r => Signed(r.Get(Command.DEF)?.Arguments), "#def: defence modifier (a shield's: its parry less its encumbrance)")
                {
                    ForRow = (v, r) =>
                    {
                        var def = Signed(r.Get(Command.DEF)?.Arguments);
                        if (Of(v) is not ArmorStats a || a.Type != ArmorStats.Shield)
                            return (def, null);
                        return (def, $"A shield: parry {a.Parry} (#def {a.Def} + encumbrance {a.Enc}), and its encumbrance counts against defence: {a.DefShown}");
                    },
                },
                new("Enc", "enc", r => r.Get(Command.ENC)?.Arguments ?? "", "#enc: encumbrance"),
                new("Type", null, r => ArmorType(r.Get(Command.TYPE)?.Arguments), "#type: shield, body armor, helmet, barding"),
            };
        }

        private static string Signed(string? v) => int.TryParse(v, out var n) && n > 0 ? "+" + n : v ?? "";

        private static string ArmorType(string? v) => v switch
        {
            "4" => "Shield", "5" => "Body", "6" => "Helmet", "8" => "Misc", "9" => "Barding", null => "", _ => v,
        };

        // how a nation (or a site) uses a monster: as a commander or as a unit
        private static readonly HashSet<Command> CommanderUses = new HashSet<Command>
        {
            Command.ADDRECCOM, Command.ADDFOREIGNCOM, Command.STARTCOM, Command.DEFCOM1, Command.DEFCOM2, Command.WALLCOM, Command.GUARDCOM,
            Command.UWDEFCOM1, Command.UWDEFCOM2, Command.UWWALLCOM, Command.UWGUARDCOM, Command.FOREIGNWALLCOM, Command.FOREIGNGUARDCOM,
            Command.ADDGOD, Command.COM, Command.HOMECOM,
        };
        private static readonly HashSet<Command> UnitUses = new HashSet<Command>
        {
            Command.ADDRECUNIT, Command.ADDFOREIGNUNIT, Command.STARTUNITTYPE1, Command.STARTUNITTYPE2, Command.STARTUNITTYPE3,
            Command.DEFUNIT1, Command.DEFUNIT1B, Command.DEFUNIT1C, Command.DEFUNIT1D, Command.DEFUNIT2, Command.DEFUNIT2B,
            Command.WALLUNIT, Command.GUARDUNIT, Command.UWDEFUNIT1, Command.UWDEFUNIT1B, Command.UWDEFUNIT1C, Command.UWDEFUNIT1D,
            Command.UWDEFUNIT2, Command.UWDEFUNIT2B, Command.UWWALLUNIT, Command.UWGUARDUNIT, Command.FOREIGNWALLUNIT, Command.FOREIGNGUARDUNIT,
            Command.MON, Command.HOMEMON,
        };

        /// <summary>
        /// Works out what the game makes of the monster (Dom5Edit.Derived). Whether it counts as a
        /// commander isn't in its data but in how it's recruited: as one by a nation or a site, or,
        /// recruited as nothing, a mage (the inspector's rule); a pretender pays design points.
        /// </summary>
        private UnitTotals WorkOutTotals()
        {
            ResolvedEntity? Find(EntityType t, int id) => id > 0 && Session.Mod.TryGet(t, id, null, out var e) ? Session.Resolve(e) : null;
            var stats = UnitStats.Of(Resolved, Find);
            var uses = ID > 0 ? Session.Usage.UsedBy(EntityType.MONSTER, ID) : Array.Empty<UsageIndex.Use>();
            var asCommander = uses.FirstOrDefault(u => CommanderUses.Contains(u.Via));
            var asUnit = uses.FirstOrDefault(u => UnitUses.Contains(u.Via));
            stats.Pretender = uses.Any(u => u.Via == Command.ADDGOD) || Resolved.Has(Command.PATHCOST) || Resolved.Has(Command.STARTDOM);
            stats.Commander = asCommander != null || asUnit == null && stats.IsMage;
            stats.CommanderCost = stats.Commander;
            string Use(UsageIndex.Use u) => $"{CommandName(u.Via)} in {NameOf(u.Type, u.Id)} #{u.Id}";
            RoleNote = asCommander != null ? Use(asCommander)
                : stats.Commander ? "a mage no one recruits as a unit"
                : asUnit != null ? Use(asUnit) : "no one recruits it, and it's no mage";
            return UnitTotals.Compute(stats, id => Find(EntityType.MONSTER, id) is ResolvedEntity m ? UnitStats.Of(m, Find) : null);
        }

        protected override void BuildPanels(HashSet<Command> covered)
        {
            Totals = WorkOutTotals();
            Panels.Add(BuildStats(covered));
            Panels.Add(new ReferenceListPanel(this, "WEAPONS", Command.WEAPON, EntityType.WEAPON, columns: WeaponColumns()));
            Panels.Add(new ReferenceListPanel(this, "ARMOR", Command.ARMOR, EntityType.ARMOR, columns: ArmorColumns()));
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
            var t = Totals!;
            // what the game makes of each stat (shown in brackets when it isn't the box's value)
            var derived = new Dictionary<Command, (DerivedValue Value, string What)>
            {
                { Command.HP, (t.Hp, "Hit points") }, { Command.PROT, (t.Prot, "Protection") }, { Command.STR, (t.Str, "Strength") },
                { Command.ATT, (t.Att, "Attack") }, { Command.DEF, (t.Def, "Defence") }, { Command.PREC, (t.Prec, "Precision") },
                { Command.AP, (t.Ap, "Combat speed") }, { Command.MAPMOVE, (t.MapMove, "Map move") }, { Command.ENC, (t.Enc, "Encumbrance") },
                { Command.STARTAGE, (t.StartAge, "Start age") }, { Command.MAXAGE, (t.MaxAge, "Max age") },
            };
            if (t.Rcost != null)
                derived[Command.RCOST] = (t.Rcost, "Resources");
            NumberField Stat(string label, Command c, string icon, string tip, Func<string, string>? note = null)
            {
                covered.Add(c);
                string text = "", hint = "";
                if (derived.TryGetValue(c, out var d))
                {
                    if (d.Value.Differs)
                        text = $"({d.Value.Value})";
                    hint = DerivedHint(d.What, d.Value, Extra(c));
                }
                else if (c == Command.GCOST && t.Gold is int gold && gold != (int.TryParse(Resolved.Get(Command.GCOST)?.Arguments, out var g) ? g : 0))
                {
                    text = $"({gold})";
                    hint = string.Join("\n", new[] { $"Gold in game: {gold} (worked out {Role})" }.Concat(t.GoldNotes));
                }
                return new NumberField(this, label, c, defaultValue: GameDefault(c), note: note, tooltip: tip)
                {
                    Icon = icon, Derived = text, DerivedTip = hint, DerivedWidth = 46,
                };
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
                text => int.TryParse(text, out var rp) && rp >= 1000 ? $"worked out by the game (base {rp / 1000})" : ""));
            AddNotes(stats, t);
            return stats;
        }

        /// <summary>"as a commander (#addreccom in Ulm #33)": what the totals take the monster for.</summary>
        private string Role => Totals?.Unit.Pretender == true ? "as a pretender"
            : $"as a {(Totals?.Unit.Commander == true ? "commander" : "unit")} ({RoleNote})";

        /// <summary>"11 unit's attack, -3 dual wield, +0 weapon": a sum's parts, the first as it is.</summary>
        private static string Parts(IEnumerable<Contribution> parts) =>
            string.Join(", ", parts.Select((p, i) => i == 0 ? $"{p.Amount} {p.Label}" : p.ToString()));

        /// <summary>A derived value's tooltip: the value in game, where it starts, what changes it.</summary>
        private static string DerivedHint(string what, DerivedValue v, IEnumerable<string>? extra = null) =>
            string.Join("\n", new[] { $"{what} in game: {v.Value}" }.Concat(v.Breakdown().Select(l => "   " + l)).Concat(extra ?? Array.Empty<string>()));

        /// <summary>More lines for a stat's tooltip: per-weapon attack and precision, dual wield, protection by part, why it's a commander.</summary>
        private IEnumerable<string> Extra(Command c)
        {
            var t = Totals!;
            switch (c)
            {
                case Command.ATT:
                    foreach (var w in t.Weapons.Where(w => !w.Weapon.Missile))
                        yield return $"With {w.Weapon.Name}: {w.Attack} ({Parts(w.AttackParts)})";
                    break;
                case Command.PREC:
                    foreach (var w in t.Weapons.Where(w => w.Weapon.Missile))
                        yield return $"With {w.Weapon.Name}: {w.Attack} ({Parts(w.AttackParts)})";
                    break;
                case Command.MAPMOVE:
                    yield return $"Worked out {Role}";
                    break;
                case Command.PROT:
                    if (t.Armor.Any(a => a.ProtBody != 0 || a.ProtHead != 0))
                        yield return "Armor over natural protection p: p + armor - p x armor / 40";
                    break;
            }
        }

        /// <summary>
        /// Values the game works out that have no box of their own: leadership with what paths add,
        /// resistances commanders get from paths, auras, casting encumbrance, old age.
        /// </summary>
        private void AddNotes(StatsPanel stats, UnitTotals t)
        {
            void Note(string text, string tip) => stats.Notes.Add(new DerivedNote(text, tip));
            // (a unit is what's assumed; a commander or pretender is said)
            if (t.Unit.Commander || t.Unit.Pretender)
                Note(t.Unit.Pretender ? "a pretender" : "a commander",
                    $"Worked out {Role}.\nCommanders move 2 more on the map, get resistances from their paths and pay for leadership and magic."
                    + (t.Unit.Pretender ? "\nA pretender's #gcost is its design point cost: no gold or resources are worked out." : ""));
            int ClassPlusBonus(Command bonus, params Command[] tiers)
            {
                var v = Resolved.Values.LastOrDefault(x => Array.IndexOf(tiers, x.Command) >= 0);
                int cls = v == null ? (tiers[0] == Command.NOLEADER ? 50 : 0) : LeaderField.TierValues[Array.IndexOf(tiers, v.Command)];
                return cls + (int.TryParse(Resolved.Get(bonus)?.Arguments, out var b) ? b : 0);
            }
            void Leader(string what, DerivedValue v, int shown)
            {
                if (v.Value != shown)
                    Note($"{what} {v.Value}", DerivedHint(what, v, new[] { $"(the class and bonus above: {shown})" }));
            }
            Leader("leadership", t.Leader, ClassPlusBonus(Command.COMMAND, Command.NOLEADER, Command.POORLEADER, Command.OKLEADER, Command.GOODLEADER, Command.EXPERTLEADER, Command.SUPERIORLEADER));
            Leader("magic leadership", t.MagicLeader, ClassPlusBonus(Command.MAGICCOMMAND, Command.NOMAGICLEADER, Command.POORMAGICLEADER, Command.OKMAGICLEADER, Command.GOODMAGICLEADER, Command.EXPERTMAGICLEADER, Command.SUPERIORMAGICLEADER));
            Leader("undead leadership", t.UndeadLeader, ClassPlusBonus(Command.UNDCOMMAND, Command.NOUNDEADLEADER, Command.POORUNDEADLEADER, Command.OKUNDEADLEADER, Command.GOODUNDEADLEADER, Command.EXPERTUNDEADLEADER, Command.SUPERIORUNDEADLEADER));
            foreach (var (what, v) in new[]
            {
                ("fire resistance", t.FireRes), ("cold resistance", t.ColdRes), ("shock resistance", t.ShockRes), ("poison resistance", t.PoisonRes),
                ("supply", t.SupplyBonus), ("fear", t.Fear), ("heat aura", t.Heat), ("cold aura", t.Cold), ("fire shield", t.FireShield),
            })
                if (v.Parts.Count > 0)
                    Note($"{what} {v.Value}", DerivedHint(what, v));
            if (t.CastingEnc is int cast && t.Unit.IsMage)
            {
                int armor = t.Armor.Sum(a => a.Enc);
                Note($"casting encumbrance {cast}", $"Encumbrance when casting spells: {cast}\n   {cast - 2 * armor} its own\n   +{2 * armor} twice its armor's");
            }
            if (t.DualWield != 0)
                Note($"dual wield {t.DualWield}", "Attack with each melee weapon:\n" + string.Join("\n", t.DualWieldNotes.Select(n => "   " + n)));
            if (t.IsOld)
                Note("old", $"Start age {t.StartAge.Value} is past max age {t.MaxAge.Value}: old age lowers strength, attack, defence, precision, hit points and combat speed and raises encumbrance (in brackets)");
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
