using Dom5Edit.Commands;
using Dom5Edit.Props;

namespace Dom5Edit.Metadata
{
    /// <summary>
    /// Extension methods for easy metadata access from properties and commands.
    /// </summary>
    public static class MetadataExtensions
    {
        /// <summary>
        /// Gets the display name for this property's command.
        /// </summary>
        public static string GetDisplayName(this Property property)
        {
            return PropertyMetadata.GetDisplayName(property.Command);
        }

        /// <summary>
        /// Gets the tooltip for this property's command.
        /// </summary>
        public static string GetTooltip(this Property property)
        {
            return PropertyMetadata.GetTooltip(property.Command);
        }

        /// <summary>
        /// Gets the category for this property's command.
        /// </summary>
        public static string GetCategory(this Property property)
        {
            var def = PropertyMetadata.Get(property.Command);
            return def?.Category ?? "Miscellaneous";
        }

        /// <summary>
        /// Validates an integer value for this property.
        /// </summary>
        public static bool Validate(this IntProperty property, out string? error)
        {
            return PropertyMetadata.ValidateValue(property.Command, property.Value, out error);
        }

        /// <summary>
        /// Gets the display name for a command.
        /// </summary>
        public static string GetDisplayName(this Command command)
        {
            return PropertyMetadata.GetDisplayName(command);
        }

        /// <summary>
        /// Gets the tooltip for a command.
        /// </summary>
        public static string GetTooltip(this Command command)
        {
            return PropertyMetadata.GetTooltip(command);
        }

        /// <summary>
        /// Gets the property definition for a command.
        /// </summary>
        public static PropertyDefinition? GetDefinition(this Command command)
        {
            return PropertyMetadata.Get(command);
        }
    }
}
