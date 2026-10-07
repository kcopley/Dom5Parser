using Dom5Edit.Entities;

namespace Dom5Editor.UI.ViewModels
{
    /// <summary>What to call an entity type in a sentence (tooltips, notes): "weapon", "weapons".</summary>
    public static class Nouns
    {
        public static string Of(EntityType type) => type switch
        {
            EntityType.MONSTER => "monster",
            EntityType.ARMOR => "armor",
            EntityType.WEAPON => "weapon",
            EntityType.ITEM => "magic item",
            EntityType.SPELL => "spell",
            EntityType.SITE => "site",
            EntityType.EVENT => "event",
            EntityType.MERCENARY => "mercenary band",
            EntityType.POPTYPE => "poptype",
            EntityType.NATION => "nation",
            EntityType.NAMETYPE => "nametype",
            EntityType.MONTAG => "montag",
            EntityType.BLESS => "blessing",
            EntityType.TEMPLATE => "template",
            _ => type.ToString().ToLowerInvariant(),
        };

        public static string Plural(EntityType type) => type switch
        {
            EntityType.ARMOR => "armor",
            _ => Of(type) + "s",
        };

        /// <summary>"Axe #17", or "#17" when it has no name.</summary>
        public static string Named(string? name, int id) =>
            string.IsNullOrEmpty(name) || name == $"#{id}" ? (id > 0 ? $"#{id}" : "(unnamed)") : id > 0 ? $"{name} #{id}" : name!;
    }
}
