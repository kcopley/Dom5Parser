using System.Reflection;
using System.Text.Json;
using Dom5Edit.Commands;
using Dom5Edit.Entities;

namespace Dom5Edit.GameData
{
    /// <summary>
    /// The commands Dominions 6 reads for each entity type, taken from the game's own .dm parser
    /// (game-commands-*.json, written by tools/dom6exe catalog). A command the game doesn't read
    /// is kept in the mod but changes nothing in game, so it is shown read-only.
    /// </summary>
    public static class GameCommandCatalog
    {
        private const string ResourceName = "Dom5Edit.GameData.game-commands.json";

        private class Context
        {
            public bool Complete;
            public HashSet<string> Commands = new HashSet<string>();
        }

        private static readonly Dictionary<string, Context> _contexts = new Dictionary<string, Context>();
        private static readonly HashSet<string> _top = new HashSet<string>();

        /// <summary>Game version the catalog was read from (e.g. "6.37"), or null if it didn't load.</summary>
        public static string? GameVersion { get; private set; }

        static GameCommandCatalog()
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName);
            if (stream == null)
                return;
            using var doc = JsonDocument.Parse(stream);
            var root = doc.RootElement;
            GameVersion = root.GetProperty("game_version").GetString();
            foreach (var c in root.GetProperty("top").EnumerateArray())
                _top.Add(c.GetString()!);
            foreach (var ctx in root.GetProperty("contexts").EnumerateObject())
            {
                var context = new Context { Complete = ctx.Value.GetProperty("complete").GetBoolean() };
                foreach (var c in ctx.Value.GetProperty("commands").EnumerateArray())
                    context.Commands.Add(c.GetString()!);
                _contexts[ctx.Name] = context;
            }
        }

        /// <summary>The game parser's name for an entity type, or null if the catalog has none.</summary>
        public static string? ContextOf(EntityType type)
        {
            return type switch
            {
                EntityType.MONSTER => "monster",
                EntityType.WEAPON => "weapon",
                EntityType.ARMOR => "armor",
                EntityType.ITEM => "item",
                EntityType.SPELL => "spell",
                EntityType.SITE => "site",
                EntityType.NATION => "nation",
                EntityType.MERCENARY => "merc",
                EntityType.POPTYPE => "poptype",
                EntityType.NAMETYPE => "nametype",
                EntityType.EVENT => "event",
                EntityType.BLESS => "bless",
                EntityType.TEMPLATE => "template",
                _ => null,
            };
        }

        /// <summary>Whether the game reads this command for this entity (see the overload by type).</summary>
        public static bool? IsRead(IDEntity entity, Command command)
        {
            return IsRead(entity.GetEntityType(), command);
        }

        /// <summary>
        /// Whether the game reads this command for this entity type: true, false, or null when
        /// the catalog can't tell (no catalog, an entity type it doesn't cover, or one whose parser
        /// also reads commands by pattern, like events' #2d6units).
        /// </summary>
        public static bool? IsRead(EntityType type, Command command)
        {
            if (GameVersion == null || !CommandsMap.TryGetString(command, out var s))
                return null;
            var name = s.TrimStart('#');
            if (_top.Contains(name))
                return true;
            var ctx = ContextOf(type);
            if (ctx == null || !_contexts.TryGetValue(ctx, out var context))
                return null;
            if (context.Commands.Contains(name))
                return true;
            return context.Complete ? false : null;
        }
    }
}
