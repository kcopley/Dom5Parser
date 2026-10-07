using Dom5Edit.Commands;
using Dom5Edit.Entities;

namespace Dom5Edit.Resolve
{
    /// <summary>
    /// When a reference's number means the entity it names in game ("used by" links only those).
    /// An event's path boost (#fireboost, ... #holyboost) goes to one commander: the one its target
    /// requirements pick, and then "the monster name or number will have no effect (you can set it
    /// to -1)"; only without target requirements does the number pick the monster (the event
    /// modding manual, Magic Pathboost Effects). The game's own events also boost the commander
    /// they make (#com 2449, #bloodboost 1, #addequip 1), with 1 as the placeholder.
    /// </summary>
    public static class ReferenceRules
    {
        // commands with which an event makes a commander (the one its boosts then go to)
        private static readonly HashSet<Command> MakesCommander = new()
        {
            Command.COM, Command.ZZ2COM, Command.ZZ4COM, Command.ZZ5COM, Command.STEALTHCOM, Command.ASSASSIN,
        };

        private static readonly HashSet<Command> PathBoosts = new()
        {
            Command.FIREBOOST, Command.AIRBOOST, Command.WATERBOOST, Command.EARTHBOOST, Command.ASTRALBOOST,
            Command.DEATHBOOST, Command.NATUREBOOST, Command.BLOODBOOST, Command.HOLYBOOST,
        };

        /// <summary>Whether this command's reference names its entity, given the entity's resolved values.</summary>
        public static bool NamesEntity(IDEntity entity, ResolvedEntity resolved, Command command)
        {
            if (entity.Kind == EntityType.EVENT && PathBoosts.Contains(command))
                return !resolved.Values.Any(v => MakesCommander.Contains(v.Command)
                                                 || CommandsMap.TryGetString(v.Command, out var s) && s.StartsWith("#req_targ"));
            return true;
        }
    }
}
