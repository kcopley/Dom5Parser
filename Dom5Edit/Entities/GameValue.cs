namespace Dom5Edit.Entities
{
    /// <summary>
    /// A value the game stores for a vanilla entity that no mod command can set (from the
    /// exe-written vanilla data's "-- ro: label = value" lines, tools/dom6exe). Shown read-only;
    /// never exported.
    /// </summary>
    public class GameValue
    {
        public string Label { get; }
        public string Value { get; }

        public GameValue(string label, string value)
        {
            Label = label;
            Value = value;
        }

        public override string ToString() => $"{Label}: {Value}";
    }
}
