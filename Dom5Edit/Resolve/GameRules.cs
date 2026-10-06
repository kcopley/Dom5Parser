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

        private static bool IsIdentity(Command c) =>
            c == Command.NAME || c == Command.FIXEDNAME || c == Command.DESCR;

        /// <summary>Commands whose lines each add an entry, which the catalog can't tell (the game handles them with their own code).</summary>
        private static readonly HashSet<Command> _repeatable = new HashSet<Command>
        {
            Command.WEAPON, Command.ARMOR, Command.CUSTOMMAGIC,
            Command.ADDRECUNIT, Command.ADDRECCOM, Command.ADDFOREIGNUNIT, Command.ADDFOREIGNCOM,
            Command.ADDGOD, Command.DELGOD, Command.STARTSITE, Command.ADDNAME, Command.RESTRICTED,
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

        /// <summary>
        /// An ability the game removes when a line sets it to 0 (#fear 0): the ability setter
        /// removes on 0, and the command's range allows 0.
        /// </summary>
        public static bool RemovesWithZero(EntityType type, Command c)
        {
            if (IsRepeatable(type, c) || IsKeyedByFirstArgument(type, c))
                return false;
            var e = GameCommandCatalog.EffectOf(type, c);
            return e != null && e.Set.Count > 0 && e.Set.All(k => k.StartsWith("a")) && e.Bits.Count == 0
                   && !e.NoArgument && e.Min <= 0 && e.Max >= 0;
        }

        /// <summary>A line that removes an ability rather than setting it (#fear 0).</summary>
        public static bool IsRemoval(EntityType type, Props.Property p) =>
            RemovesWithZero(type, p.Command) && ResolvedValue.ArgumentsOf(p) == "0";

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
                return c == earlier.Command && later.Selector == earlier.Selector;
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
            return x.Set.All(k => n.Set.Contains(k) || n.Del.Contains(k))
                   && x.Bits.All(b => n.Clears.Contains(b) || n.Bits.Contains(b));
        }

        /// <summary>
        /// Whether a line removes an earlier one without replacing it: a command that only clears
        /// flag bits (#almostliving) removes a flag line all of whose bits it clears.
        /// </summary>
        public static bool Cancels(EntityType type, ResolvedValue later, ResolvedValue earlier)
        {
            var n = GameCommandCatalog.EffectOf(type, later.Command);
            var x = GameCommandCatalog.EffectOf(type, earlier.Command);
            if (n == null || x == null || n.Clears.Count == 0 || x.Bits.Count == 0 || x.Set.Count > 0)
                return false;
            return x.Bits.All(b => n.Clears.Contains(b));
        }
    }
}
