using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dom5Editor.Data
{
    /// <summary>
    /// Root configuration loaded from entity_badges.json files.
    /// </summary>
    public class BadgeConfig
    {
        [JsonPropertyName("sections")]
        public List<BadgeSection> Sections { get; set; } = new();

        [JsonPropertyName("layout")]
        public Dictionary<string, List<string>> Layout { get; set; } = new();

        [JsonPropertyName("renderers")]
        public Dictionary<string, RendererConfig> Renderers { get; set; } = new();

        /// <summary>
        /// Gets sections in layout order for the specified entity type.
        /// </summary>
        public IEnumerable<BadgeSection> GetSectionsForEntity(string entityType)
        {
            if (Layout.TryGetValue(entityType, out var sectionIds))
            {
                var sectionMap = new Dictionary<string, BadgeSection>();
                foreach (var section in Sections)
                {
                    sectionMap[section.Id] = section;
                }

                foreach (var id in sectionIds)
                {
                    if (sectionMap.TryGetValue(id, out var section))
                    {
                        yield return section;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a section by ID.
        /// </summary>
        public BadgeSection GetSection(string sectionId)
        {
            return Sections.Find(s => s.Id == sectionId);
        }
    }

    /// <summary>
    /// A section of badges (e.g., "types", "general", "combat", "resistances").
    /// </summary>
    public class BadgeSection
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("renderer")]
        public string Renderer { get; set; } = "badge";

        [JsonPropertyName("readOnly")]
        public bool ReadOnly { get; set; }

        [JsonPropertyName("commands")]
        public List<BadgeCommand> Commands { get; set; } = new();

        /// <summary>
        /// Layout mode: "wrap" (default) or "grid".
        /// </summary>
        [JsonPropertyName("layout")]
        public string Layout { get; set; } = "wrap";

        /// <summary>
        /// Number of columns for grid layout. Default is 3.
        /// </summary>
        [JsonPropertyName("columns")]
        public int Columns { get; set; } = 3;

        /// <summary>
        /// When true, shows all commands in this section using default values
        /// even when the entity doesn't have the property set.
        /// Useful for stats sections where you always want to see all values.
        /// </summary>
        [JsonPropertyName("showDefaults")]
        public bool ShowDefaults { get; set; } = false;

        /// <summary>
        /// When false, hides the "Add" dropdown for this section.
        /// Defaults to true. Set to false for sections where all properties
        /// are always shown (e.g., stats with showDefaults=true).
        /// </summary>
        [JsonPropertyName("showAddButton")]
        public bool ShowAddButton { get; set; } = true;

        /// <summary>
        /// Returns true if this section uses a custom renderer (not standard badge).
        /// </summary>
        public bool HasCustomRenderer => Renderer != "badge" && Renderer != "coloredBadge";

        /// <summary>
        /// Returns true if this section uses grid layout.
        /// </summary>
        public bool IsGridLayout => Layout == "grid" || Renderer == "statsGrid";
    }

    /// <summary>
    /// A command definition within a badge section.
    /// </summary>
    public class BadgeCommand
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("display")]
        public string Display { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = "flag";

        [JsonPropertyName("default")]
        public int? Default { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("borderColor")]
        public string BorderColor { get; set; }

        [JsonPropertyName("paths")]
        public List<string> Paths { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// For reference types, specifies the target entity type (monster, item, montag, nametype, nation, spell, site).
        /// </summary>
        [JsonPropertyName("refType")]
        public string RefType { get; set; }

        /// <summary>
        /// Optional icon path relative to icons folder (e.g., "magicicons/Path_F.png").
        /// </summary>
        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        /// <summary>
        /// Whether this command can appear multiple times on an entity.
        /// Defaults to true to allow adding multiple instances.
        /// </summary>
        [JsonPropertyName("allowMultiple")]
        public bool AllowMultiple { get; set; } = true;

        /// <summary>
        /// Returns true if this command has an icon defined.
        /// </summary>
        public bool HasIcon => !string.IsNullOrEmpty(Icon);

        /// <summary>
        /// Returns true if this is a flag (boolean) command.
        /// </summary>
        public bool IsFlag => Type == "flag";

        /// <summary>
        /// Returns true if this is an integer value command.
        /// </summary>
        public bool IsInt => Type == "int";

        /// <summary>
        /// Returns true if this is a reference to another entity.
        /// </summary>
        public bool IsRef => Type == "ref";

        /// <summary>
        /// Returns true if this is a bitmask+chance command (like CUSTOMMAGIC).
        /// </summary>
        public bool IsBitmaskChance => Type == "bitmaskChance";

        /// <summary>
        /// Returns true if this is a two-integer value command (e.g., #gems, #magicskill).
        /// </summary>
        public bool IsIntInt => Type == "intint";

        /// <summary>
        /// Returns true if this is a string value command (e.g., #descr).
        /// </summary>
        public bool IsString => Type == "string";

        /// <summary>
        /// Returns true if this is a bitmask value command (e.g., #itemslots).
        /// </summary>
        public bool IsBitmask => Type == "bitmask";

        /// <summary>
        /// Returns true if this command has custom colors.
        /// </summary>
        public bool HasColors => !string.IsNullOrEmpty(Color);
    }

    /// <summary>
    /// Configuration for a renderer type.
    /// </summary>
    public class RendererConfig
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("supportedTypes")]
        public List<string> SupportedTypes { get; set; } = new();

        [JsonPropertyName("requiresColor")]
        public bool RequiresColor { get; set; }

        [JsonPropertyName("columns")]
        public int? Columns { get; set; }

        [JsonPropertyName("pathColors")]
        public Dictionary<string, string> PathColors { get; set; }
    }
}
