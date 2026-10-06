using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.GameData;

namespace Dom5Edit.Resolve
{
    /// <summary>
    /// How the game combines an entity's lines: what a copy copies, what a clear clears, and
    /// whether a line replaces an earlier one or adds to it. From the game's parser where the
    /// command catalog has it (tools/dom6exe command_effects), else from the rules in
    /// tools/dom6exe/README.md. docs/EDIT_FLOW.md, "Structured features", has the table.
    /// </summary>
    public static class GameRules
    {
        /// <summary>The copy commands: each replaces part of the entity with the source's current state.</summary>
        public static bool IsCopy(Command c) => PropertyGroupMap.IsFullCopyCommand(c);

        /// <summary>The clear commands.</summary>
        public static bool IsClear(Command c) =>
            PropertyGroupMap.IsClearCommand(c) || c == Command.CLEARREC || c == Command.CLEARDEF;

        /// <summary>The group a command's value belongs to, for copies and clears.</summary>
        public static PropertyGroup GroupOf(EntityType type, Command c)
        {
            switch (type)
            {
                case EntityType.MONSTER:
                    return PropertyGroupMap.GetMonsterGroup(c);
                case EntityType.NATION:
                    return PropertyGroupMap.GetNationGroup(c);
                case EntityType.ITEM:
                    // an item's #copyspr copies its sprite (the catalog: #spr and #copyspr write the same field)
                    return c == Command.SPR ? PropertyGroup.Sprites : PropertyGroup.None;
                case EntityType.POPTYPE:
                    if (c == Command.ADDRECUNIT || c == Command.ADDRECCOM)
                        return PropertyGroup.Recruitment;
                    if (c.ToString().StartsWith("DEF"))
                        return PropertyGroup.Defense;
                    return PropertyGroup.None;
                default:
                    return PropertyGroup.None;
            }
        }

        /// <summary>Whether a copy command copies this command's value (#copystats: all but sprites; #copyspr: sprites; the others: everything).</summary>
        public static bool Copies(EntityType type, Command copy, Command c)
        {
            var groups = PropertyGroupMap.GetGroupsOverwrittenByCopy(copy);
            return groups.Contains(PropertyGroup.All) || groups.Contains(GroupOf(type, c));
        }

        /// <summary>
        /// Whether a clear command removes this command's value. A monster's #clear resets
        /// everything but the name and sprites (the game's clear helper keeps the name; sprites
        /// aren't in what it resets); the other types' #clear keeps the name.
        /// </summary>
        public static bool Clears(EntityType type, Command clear, Command c)
        {
            if (IsIdentity(c))
                return false;
            if (clear == Command.CLEAR)
                return type != EntityType.MONSTER || GroupOf(type, c) != PropertyGroup.Sprites;
            if (type == EntityType.NATION && clear == Command.CLEARNATION)
                return ClearedByClearNation(c);
            var group = ClearedGroup(clear);
            return group.HasValue && GroupOf(type, c) == group.Value;
        }

        /// <summary>
        /// Whether an earlier copy or clear line would undo a later line about the same group: for
        /// a copy or clear line, any earlier copy or clear touching its group; for another line, a
        /// copy that copies it or a clear that clears it. Used to place added lines after them.
        /// </summary>
        public static bool Overrides(EntityType type, Command earlier, Command later)
        {
            if (!IsCopy(earlier) && !IsClear(earlier))
                return false;
            if (IsCopy(later))
                return true;
            if (IsClear(later))
            {
                var group = ClearedGroup(later);
                if (group == null || group == PropertyGroup.All)
                    return true;
                return IsCopy(earlier)
                    ? PropertyGroupMap.GetGroupsOverwrittenByCopy(earlier) is var g && (g.Contains(PropertyGroup.All) || g.Contains(group.Value))
                    : earlier == Command.CLEAR || ClearedGroup(earlier) == group;
            }
            return IsCopy(earlier) ? Copies(type, earlier, later) : Clears(type, earlier, later);
        }

        /// <summary>The group a clear command clears (All for #clear).</summary>
        public static PropertyGroup? ClearedGroup(Command clear) =>
            PropertyGroupMap.GetGroupClearedBy(clear)
            ?? (clear == Command.CLEARREC ? PropertyGroup.Recruitment
                : clear == Command.CLEARDEF ? PropertyGroup.Defense : (PropertyGroup?)null);

        /// <summary>The clear command for a group on this entity type (#clearweapons, ...), or null.</summary>
        public static Command? ClearCommandFor(EntityType type, PropertyGroup group)
        {
            if (type == EntityType.POPTYPE)
                return group == PropertyGroup.Recruitment ? Command.CLEARREC : group == PropertyGroup.Defense ? Command.CLEARDEF : null;
            if (group == PropertyGroup.None || group == PropertyGroup.All || group == PropertyGroup.Sprites)
                return null;
            return PropertyGroupMap.GetClearCommand(group);
        }

        /// <summary>
        /// #clearnation empties the nation's abilities and its god list (tools/dom6exe: the record's
        /// ability arrays and god list), not its fields (name, epithet, era), texts or recruitment list:
        /// so a command is cleared if what it writes is abilities, or it's a god or start site line.
        /// </summary>
        private static bool ClearedByClearNation(Command c)
        {
            if (c == Command.ADDGOD || c == Command.DELGOD || c == Command.STARTSITE)
                return true;
            var e = GameCommandCatalog.EffectOf(EntityType.NATION, c);
            if (e == null)
                return false;
            var keys = e.Set.Concat(e.Add).Concat(e.Or).ToList();
            return keys.Count > 0 && keys.All(k => k.StartsWith("a"));
        }

        private static bool IsIdentity(Command c) =>
            c == Command.NAME || c == Command.FIXEDNAME || c == Command.DESCR;

        /// <summary>Commands whose lines each add an entry, which the catalog can't tell (the game handles them with their own code).</summary>
        private static readonly HashSet<Command> _repeatable = new HashSet<Command>
        {
            Command.WEAPON, Command.ARMOR, Command.CUSTOMMAGIC,
            Command.ADDRECUNIT, Command.ADDRECCOM, Command.ADDFOREIGNUNIT, Command.ADDFOREIGNCOM,
            Command.ADDGOD, Command.DELGOD, Command.STARTSITE, Command.ADDNAME, Command.RESTRICTED,
            Command.ITEM,   // a mercenary band's items (up to 4)
        };

        /// <summary>
        /// Two-argument commands whose first argument picks what the second sets (a path, a gem
        /// type, a scale): lines with different first arguments are different values. Per type: a
        /// spell's #path is "#path slot path", a site's is "#path path".
        /// </summary>
        private static readonly Dictionary<EntityType, HashSet<Command>> _keyedByFirstArgument = new Dictionary<EntityType, HashSet<Command>>
        {
            { EntityType.MONSTER, new HashSet<Command> { Command.MAGICSKILL, Command.MAGICBOOST, Command.GEMPROD } },
            { EntityType.ITEM, new HashSet<Command> { Command.MAGICBOOST, Command.GEMPROD } },
            { EntityType.SPELL, new HashSet<Command> { Command.PATH, Command.PATHLEVEL } },
            { EntityType.SITE, new HashSet<Command> { Command.GEMS } },
            { EntityType.TEMPLATE, new HashSet<Command> { Command.MAGIC, Command.SCALE } },
        };

        /// <summary>Commands that set one value among alternatives (a leader class): a later one replaces an earlier one.</summary>
        private static readonly Dictionary<Command, int> _exclusive = new Dictionary<Command, int>
        {
            { Command.NOLEADER, 1 }, { Command.POORLEADER, 1 }, { Command.OKLEADER, 1 },
            { Command.GOODLEADER, 1 }, { Command.EXPERTLEADER, 1 }, { Command.SUPERIORLEADER, 1 },
            { Command.NOMAGICLEADER, 2 }, { Command.POORMAGICLEADER, 2 }, { Command.OKMAGICLEADER, 2 },
            { Command.GOODMAGICLEADER, 2 }, { Command.EXPERTMAGICLEADER, 2 }, { Command.SUPERIORMAGICLEADER, 2 },
            { Command.NOUNDEADLEADER, 3 }, { Command.POORUNDEADLEADER, 3 }, { Command.OKUNDEADLEADER, 3 },
            { Command.GOODUNDEADLEADER, 3 }, { Command.EXPERTUNDEADLEADER, 3 }, { Command.SUPERIORUNDEADLEADER, 3 },
        };

        /// <summary>Whether each line of the command adds an entry (weapons, recruits, repeatable abilities) rather than replacing the last.</summary>
        public static bool IsRepeatable(EntityType type, Command c)
        {
            if (_repeatable.Contains(c))
                return true;
            return GameCommandCatalog.EffectOf(type, c)?.Appends == true;
        }

        // set and bit keys of an effect as sets, for the replace test (built once per effect)
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<CommandEffect, Keys> _keys = new();

        private sealed class Keys
        {
            public HashSet<string> Covers = new HashSet<string>();   // what a later line overwrites: set + del + clears + bits
            public HashSet<string> Clears = new HashSet<string>();
        }

        private static Keys KeysOf(CommandEffect e) => _keys.GetValue(e, x =>
        {
            var k = new Keys();
            k.Covers.UnionWith(x.Set);
            k.Covers.UnionWith(x.Del);
            k.Covers.UnionWith(x.Clears.Select(b => "b" + b));
            k.Covers.UnionWith(x.Bits.Select(b => "b" + b));
            k.Clears.UnionWith(x.Clears);
            return k;
        });

        /// <summary>
        /// An ability the game removes when a line sets it to 0 (#fear 0): the ability setter
        /// removes on 0, and the command's range allows 0.
        /// </summary>
        public static bool RemovesWithZero(EntityType type, Command c) =>
            _removesWithZero.GetOrAdd((type, c), static k => RemovesWithZeroUncached(k.Item1, k.Item2));

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<(EntityType, Command), bool> _removesWithZero = new();

        private static bool RemovesWithZeroUncached(EntityType type, Command c)
        {
            if (IsRepeatable(type, c) || IsKeyedByFirstArgument(type, c))
                return false;
            var e = GameCommandCatalog.EffectOf(type, c);
            return e != null && e.Set.Count > 0 && e.Set.All(k => k.StartsWith("a")) && e.Bits.Count == 0
                   && !e.NoArgument && e.Min <= 0 && e.Max >= 0;
        }

        /// <summary>A line that removes an ability rather than setting it (#fear 0).</summary>
        public static bool IsRemoval(EntityType type, Props.Property p)
        {
            if (p is Props.CommandProperty || !RemovesWithZero(type, p.Command))
                return false;
            return p is Props.IntProperty ip ? ip.Value == 0 : ResolvedValue.ArgumentsOf(p) == "0";
        }

        /// <summary>
        /// A keyed line that adds rather than replaces: a random magic skill (#magicskill 50-53:
        /// random, elemental, sorcery, all). The manual: a #magicskill replaces the old level of
        /// that path "unless it is a random skill"; each random one is another pick.
        /// </summary>
        public static bool AddsEach(EntityType type, Command c, string? selector) =>
            type == EntityType.MONSTER && c == Command.MAGICSKILL && int.TryParse(selector, out int path) && path >= 50;

        /// <summary>Whether the command's first argument picks which value it sets (#magicskill path level).</summary>
        public static bool IsKeyedByFirstArgument(EntityType? type, Command c) =>
            type is EntityType t && _keyedByFirstArgument.TryGetValue(t, out var set) && set.Contains(c);

        /// <summary>
        /// Whether a later line replaces an earlier one, so the earlier one no longer counts: the
        /// same command (and first argument, for keyed ones); one of the same set of alternatives;
        /// or, from the game's parser, a command that rewrites everything the earlier one wrote
        /// (#humanoid after #quadruped: both set the body shape, #humanoid removes the slots).
        /// </summary>
        public static bool Replaces(EntityType type, ResolvedValue later, ResolvedValue earlier)
        {
            Command c = later.Command;
            if (IsRepeatable(type, c))
                return false;
            if (IsKeyedByFirstArgument(type, c) || IsKeyedByFirstArgument(type, earlier.Command))
                return c == earlier.Command && later.Selector == earlier.Selector && !AddsEach(type, c, later.Selector);
            if (c == earlier.Command)
                return true;
            if (_exclusive.TryGetValue(c, out int g) && _exclusive.TryGetValue(earlier.Command, out int g2) && g == g2)
                return true;
            var n = GameCommandCatalog.EffectOf(type, c);
            var x = GameCommandCatalog.EffectOf(type, earlier.Command);
            if (n == null || x == null || x.Appends || IsRepeatable(type, earlier.Command))
                return false;
            if (x.Set.Count == 0 && x.Bits.Count == 0)
                return false;
            var covers = KeysOf(n).Covers;
            foreach (var k in x.Set)
                if (!covers.Contains(k))
                    return false;
            foreach (var b in x.Bits)
                if (!covers.Contains("b" + b))
                    return false;
            return true;
        }

        /// <summary>For one command of one type: which earlier commands a line of it replaces or cancels.</summary>
        internal sealed class LineRule
        {
            public bool Repeatable;
            public bool Keyed;
            public readonly HashSet<Command> Replaces = new HashSet<Command>();
            public readonly HashSet<Command> Cancels = new HashSet<Command>();
        }

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<(EntityType, Command), LineRule> _lineRules = new();

        /// <summary>
        /// <see cref="Replaces"/> and <see cref="Cancels"/> worked out once per command against every
        /// command of the type (the resolver checks every line against every value).
        /// </summary>
        internal static LineRule RuleOf(EntityType type, Command c) => _lineRules.GetOrAdd((type, c), static key => BuildRule(key.Item1, key.Item2));

        private static LineRule BuildRule(EntityType type, Command c)
        {
            var rule = new LineRule { Repeatable = IsRepeatable(type, c), Keyed = IsKeyedByFirstArgument(type, c) };
            var candidates = new HashSet<Command>(GameCommandCatalog.CommandsWithEffects(type)) { c };
            if (_exclusive.TryGetValue(c, out int g))
                candidates.UnionWith(_exclusive.Where(x => x.Value == g).Select(x => x.Key));
            foreach (var x in candidates)
            {
                if (!rule.Repeatable && ReplacesCommand(type, c, x))
                    rule.Replaces.Add(x);
                if (CancelsCommand(type, c, x))
                    rule.Cancels.Add(x);
            }
            return rule;
        }

        private static bool ReplacesCommand(EntityType type, Command c, Command earlier)
        {
            if (IsKeyedByFirstArgument(type, c) || IsKeyedByFirstArgument(type, earlier))
                return c == earlier; // and the same first argument, checked per line
            if (c == earlier)
                return true;
            if (_exclusive.TryGetValue(c, out int g) && _exclusive.TryGetValue(earlier, out int g2) && g == g2)
                return true;
            var n = GameCommandCatalog.EffectOf(type, c);
            var x = GameCommandCatalog.EffectOf(type, earlier);
            if (n == null || x == null || x.Appends || IsRepeatable(type, earlier))
                return false;
            if (x.Set.Count == 0 && x.Bits.Count == 0)
                return false;
            return x.Set.All(k => n.Set.Contains(k) || n.Del.Contains(k))
                   && x.Bits.All(b => n.Clears.Contains(b) || n.Bits.Contains(b));
        }

        private static bool CancelsCommand(EntityType type, Command c, Command earlier)
        {
            var n = GameCommandCatalog.EffectOf(type, c);
            var x = GameCommandCatalog.EffectOf(type, earlier);
            if (n == null || x == null || n.Clears.Count == 0 || x.Bits.Count == 0 || x.Set.Count > 0)
                return false;
            return x.Bits.All(b => n.Clears.Contains(b));
        }

        /// <summary>
        /// Whether a line removes an earlier one without replacing it: a command that only clears
        /// flag bits (#almostliving) removes a flag line all of whose bits it clears.
        /// </summary>
        public static bool Cancels(EntityType type, ResolvedValue later, ResolvedValue earlier)
        {
            var n = GameCommandCatalog.EffectOf(type, later.Command);
            if (n == null || n.Clears.Count == 0)
                return false;
            var x = GameCommandCatalog.EffectOf(type, earlier.Command);
            if (x == null || x.Bits.Count == 0 || x.Set.Count > 0)
                return false;
            var clears = KeysOf(n).Clears;
            foreach (var b in x.Bits)
                if (!clears.Contains(b))
                    return false;
            return true;
        }
    }
}
