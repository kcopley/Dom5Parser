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
        /// Returns true if this section uses a custom renderer (not standard badge).
        /// </summary>
        public bool HasCustomRenderer => Renderer != "badge" && Renderer != "coloredBadge";
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
        /// Returns true if this is a flag (boolean) command.
        /// </summary>
        public bool IsFlag => Type == "flag";

        /// <summary>
        /// Returns true if this is an integer value command.
        /// </summary>
        public bool IsInt => Type == "int";

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
