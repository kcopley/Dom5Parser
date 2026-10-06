using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Edit.Resolve
{
    /// <summary>Where a value an entity has in game comes from (docs/EDIT_FLOW.md, the source table).</summary>
    public enum ValueSource
    {
        /// <summary>A line in one of this entity's own blocks.</summary>
        Own,
        /// <summary>The vanilla entity's data, which the mod doesn't change.</summary>
        Vanilla,
        /// <summary>Brought in by one of this entity's copy commands (#copystats, #copyweapon, ...).</summary>
        Copied,
    }

    /// <summary>One value an entity has in game, and the line it came from.</summary>
    public sealed class ResolvedValue
    {
        /// <summary>The line that set it: this entity's own, the vanilla entity's, or a copy source's.</summary>
        public Property Property { get; }
        public ValueSource Source { get; }
        /// <summary>For a copied value: the entity whose copy command brought it in.</summary>
        public IDEntity? CopiedFrom { get; }
        /// <summary>For a copied value: the value in the copy source (which may itself be copied or vanilla).</summary>
        public ResolvedValue? Via { get; }

        public Command Command => Property.Command;

        /// <summary>The first argument, for commands whose first argument picks the value (#magicskill's path).</summary>
        public string? Selector { get; }

        internal ResolvedValue(Property property, ValueSource source, IDEntity? copiedFrom = null, ResolvedValue? via = null)
        {
            Property = property;
            Source = source;
            CopiedFrom = copiedFrom;
            Via = via;
            Selector = GameRules.IsKeyedByFirstArgument(property.Command) ? FirstArgument(property) : null;
        }

        internal ResolvedValue CopiedBy(IDEntity from) => new ResolvedValue(Property, ValueSource.Copied, from, this);

        /// <summary>The line's arguments as the game reads them (without the command and comment).</summary>
        public string Arguments => ArgumentsOf(Property);

        public static string ArgumentsOf(Property p)
        {
            string s = p.ToExportString() ?? "";
            int comment = s.IndexOf(" --", StringComparison.Ordinal);
            if (comment >= 0)
                s = s.Substring(0, comment);
            int space = s.IndexOf(' ');
            return space < 0 ? "" : s.Substring(space + 1).Trim();
        }

        private static string FirstArgument(Property p)
        {
            var args = ArgumentsOf(p);
            int space = args.IndexOf(' ');
            return space < 0 ? args : args.Substring(0, space);
        }

        public override string ToString() => $"{Property.ToExportString()} [{Source}{(CopiedFrom != null ? " from " + CopiedFrom.ID : "")}]";
    }

    /// <summary>
    /// What an entity is in game after the mod's lines: its values in the order the game holds
    /// them, each with where it came from, plus its own copy and clear lines and the vanilla
    /// values no command sets.
    /// </summary>
    public sealed class ResolvedEntity
    {
        /// <summary>The mod's entity, or the vanilla one when the mod has none for it.</summary>
        public IDEntity Entity { get; }

        /// <summary>The vanilla entity this one starts from (#select), or null.</summary>
        public IDEntity? Vanilla { get; }

        public IReadOnlyList<ResolvedValue> Values { get; }

        /// <summary>The entity's own copy and clear lines that still count, in order.</summary>
        public IReadOnlyList<Property> Structure { get; }

        /// <summary>The entity's own lines that remove an inherited ability (#fear 0): not values, but the editor shows them as removed.</summary>
        public IReadOnlyList<Property> Removals { get; }

        /// <summary>Values the game stores that no command can set (read-only), from vanilla or a copy source.</summary>
        public IReadOnlyList<GameValue> GameValues { get; }

        /// <summary>Vanilla sprites and descriptions the editor shows (not lines of the data; Property.IsDisplayAsset), by command.</summary>
        public IReadOnlyDictionary<Command, Property> Assets { get; internal set; } = new Dictionary<Command, Property>();

        internal ResolvedEntity(IDEntity entity, IDEntity? vanilla, IReadOnlyList<ResolvedValue> values,
                                IReadOnlyList<Property> structure, IReadOnlyList<Property> removals, IReadOnlyList<GameValue> gameValues)
        {
            Entity = entity;
            Vanilla = vanilla;
            Values = values;
            Structure = structure;
            Removals = removals;
            GameValues = gameValues;
        }

        /// <summary>The value for a single-valued command (the last line that counts), or null.</summary>
        public ResolvedValue? Get(Command c) => Values.LastOrDefault(v => v.Command == c);

        /// <summary>Every value for a command, in order (weapons, recruits, magic paths, ...).</summary>
        public IEnumerable<ResolvedValue> GetAll(Command c) => Values.Where(v => v.Command == c);

        public bool Has(Command c) => Values.Any(v => v.Command == c);

        /// <summary>The copy command that counts for this entity (#copystats, #copyweapon, ...), or null.</summary>
        public Property? CopyLine => Structure.LastOrDefault(p => GameRules.IsCopy(p.Command) && p.Command != Command.COPYSPR);

        /// <summary>Whether the value is a line this entity holds now, which an edit can change in place.</summary>
        public bool IsEditableInPlace(ResolvedValue v) =>
            v.Source == ValueSource.Own && Entity.Properties.Any(p => ReferenceEquals(p, v.Property));
    }
}
