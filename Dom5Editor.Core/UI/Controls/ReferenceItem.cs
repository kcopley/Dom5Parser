namespace Dom5Editor.UI.Controls
{
#nullable disable
    /// <summary>
    /// Represents an item in the searchable reference dropdown.
    /// </summary>
    public class ReferenceItem
    {
        public int ID { get; set; }
        public string DisplayName { get; set; }

        /// <summary>
        /// Optional tag for additional data.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>A hover hint for the entry (what a command does), or null.</summary>
        public string Tooltip { get; set; }

        /// <summary>Whether the list shows the ID (not for commands: their number means nothing to a modder).</summary>
        public bool ShowId { get; set; } = true;

        public override string ToString()
        {
            return !string.IsNullOrEmpty(DisplayName)
                ? $"{DisplayName} (#{ID})"
                : $"#{ID}";
        }
    }
#nullable restore
}
