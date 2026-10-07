namespace Dom5Editor.UI.Controls
{
    /// <summary>A card on the chain map: an event, or a spell that starts events.</summary>
    public sealed class ChainNode
    {
        public ChainNode(object key, string title, string subtitle, bool isCurrent, bool isSpell, Action open)
        {
            Key = key;
            Title = title;
            Subtitle = subtitle;
            IsCurrent = isCurrent;
            IsSpell = isSpell;
            Open = open;
        }

        public object Key { get; }
        public string Title { get; }
        public string Subtitle { get; }
        public bool IsCurrent { get; }
        public bool IsSpell { get; }
        public Action Open { get; }
        /// <summary>Its position in the file (order within a column).</summary>
        public int Order { get; init; }
        internal int Layer;
        internal double Y;
        internal Box Bounds;
    }

    /// <summary>A card's place on the chain map (no UI toolkit's rectangle).</summary>
    internal readonly record struct Box(double Left, double Top, double Width, double Height)
    {
        public double Right => Left + Width;
        public double Bottom => Top + Height;
        public bool Contains(double x, double y) => x >= Left && x <= Right && y >= Top && y <= Bottom;
    }

    /// <summary>An arrow on the chain map: how one card leads to another.</summary>
    public sealed class ChainEdge
    {
        public ChainEdge(ChainNode from, ChainNode to, string label, string kind)
        {
            From = from;
            To = to;
            Label = label;
            Kind = kind;
        }

        public ChainNode From { get; }
        public ChainNode To { get; }
        public string Label { get; }
        /// <summary>code, excludes, delay, variable, choice, spell.</summary>
        public string Kind { get; }
        /// <summary>Its colour (#RRGGBB): one per code or variable number on the map, else its kind's.</summary>
        public string Color { get; init; } = "#9CA3AF";
        /// <summary>Drawn dashed: a link that blocks (not while code N), a skip, a spell.</summary>
        public bool Dashed { get; init; }
    }

    /// <summary>One entry of the chain map's legend: "variable 6001" in its colour, "delay", ...</summary>
    public sealed record ChainLegendItem(string Text, string Color, bool Dashed, string Tip);
}
