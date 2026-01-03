using Dom5Edit.Commands;
using Dom5Edit.Entities;

namespace Dom5Edit.Metadata
{
    /// <summary>
    /// Defines metadata for a command/property for UI generation and validation.
    /// </summary>
    public class PropertyDefinition
    {
        /// <summary>
        /// The command this definition applies to.
        /// </summary>
        public Command Command { get; set; }

        /// <summary>
        /// The type of property (IntProperty, StringProperty, MonsterRef, etc.).
        /// </summary>
        public Type PropertyType { get; set; }

        /// <summary>
        /// Human-readable display name for the UI.
        /// </summary>
        public string DisplayName { get; set; } = "";

        /// <summary>
        /// Category for grouping in the UI (e.g., "Combat Stats", "Magic", "Movement").
        /// </summary>
        public string Category { get; set; } = "Miscellaneous";

        /// <summary>
        /// Tooltip/help text explaining the command.
        /// </summary>
        public string Tooltip { get; set; } = "";

        /// <summary>
        /// Sort order within the category (lower = earlier).
        /// </summary>
        public int DisplayOrder { get; set; } = 100;

        /// <summary>
        /// Minimum value for integer properties.
        /// </summary>
        public int? MinValue { get; set; }

        /// <summary>
        /// Maximum value for integer properties.
        /// </summary>
        public int? MaxValue { get; set; }

        /// <summary>
        /// Whether this command can appear multiple times on an entity.
        /// </summary>
        public bool AllowMultiple { get; set; } = false;

        /// <summary>
        /// For reference types, the target entity type.
        /// </summary>
        public EntityType? TargetEntityType { get; set; }

        /// <summary>
        /// Whether this is a flag command (no value, just presence).
        /// </summary>
        public bool IsFlag { get; set; } = false;

        /// <summary>
        /// The entity types this command applies to.
        /// </summary>
        public List<EntityType> ApplicableEntities { get; set; } = new List<EntityType>();

        /// <summary>
        /// The page number in the modding manual (for reference).
        /// </summary>
        public int ManualPage { get; set; } = 0;

        /// <summary>
        /// Creates a PropertyDefinition with basic info.
        /// </summary>
        public static PropertyDefinition Create(
            Command command,
            Type propertyType,
            string displayName,
            string category = "Miscellaneous",
            string tooltip = "")
        {
            return new PropertyDefinition
            {
                Command = command,
                PropertyType = propertyType,
                DisplayName = displayName,
                Category = category,
                Tooltip = tooltip
            };
        }

        /// <summary>
        /// Creates a PropertyDefinition for a flag command.
        /// </summary>
        public static PropertyDefinition CreateFlag(
            Command command,
            string displayName,
            string category = "Miscellaneous",
            string tooltip = "")
        {
            return new PropertyDefinition
            {
                Command = command,
                PropertyType = typeof(Props.CommandProperty),
                DisplayName = displayName,
                Category = category,
                Tooltip = tooltip,
                IsFlag = true
            };
        }

        /// <summary>
        /// Creates a PropertyDefinition for an integer property with range.
        /// </summary>
        public static PropertyDefinition CreateInt(
            Command command,
            string displayName,
            string category = "Miscellaneous",
            string tooltip = "",
            int? min = null,
            int? max = null)
        {
            return new PropertyDefinition
            {
                Command = command,
                PropertyType = typeof(Props.IntProperty),
                DisplayName = displayName,
                Category = category,
                Tooltip = tooltip,
                MinValue = min,
                MaxValue = max
            };
        }

        /// <summary>
        /// Creates a PropertyDefinition for a reference property.
        /// </summary>
        public static PropertyDefinition CreateRef(
            Command command,
            Type refType,
            EntityType targetType,
            string displayName,
            string category = "Miscellaneous",
            string tooltip = "",
            bool allowMultiple = false)
        {
            return new PropertyDefinition
            {
                Command = command,
                PropertyType = refType,
                DisplayName = displayName,
                Category = category,
                Tooltip = tooltip,
                TargetEntityType = targetType,
                AllowMultiple = allowMultiple
            };
        }

        /// <summary>
        /// Fluent method to set applicable entities.
        /// </summary>
        public PropertyDefinition ForEntities(params EntityType[] entities)
        {
            ApplicableEntities = entities.ToList();
            return this;
        }

        /// <summary>
        /// Fluent method to allow multiple instances.
        /// </summary>
        public PropertyDefinition WithMultiple()
        {
            AllowMultiple = true;
            return this;
        }

        /// <summary>
        /// Fluent method to set display order.
        /// </summary>
        public PropertyDefinition WithOrder(int order)
        {
            DisplayOrder = order;
            return this;
        }

        /// <summary>
        /// Fluent method to set manual page reference.
        /// </summary>
        public PropertyDefinition FromPage(int page)
        {
            ManualPage = page;
            return this;
        }
    }
}
