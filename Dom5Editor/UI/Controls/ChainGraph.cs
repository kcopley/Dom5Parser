using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
        internal Rect Bounds;
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
    }

    /// <summary>
    /// A chain of events drawn left to right (docs/EVENT_EDITOR.md, E-4): each card in the column
    /// after the cards leading to it, arrows labelled with what links them (code, delay, variable,
    /// choice, spell). Clicking a card opens it.
    /// </summary>
    public sealed class ChainGraph : FrameworkElement
    {
        public static readonly DependencyProperty NodesProperty = DependencyProperty.Register(
            nameof(Nodes), typeof(IReadOnlyList<ChainNode>), typeof(ChainGraph),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty EdgesProperty = DependencyProperty.Register(
            nameof(Edges), typeof(IReadOnlyList<ChainEdge>), typeof(ChainGraph),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public IReadOnlyList<ChainNode>? Nodes
        {
            get => (IReadOnlyList<ChainNode>?)GetValue(NodesProperty);
            set => SetValue(NodesProperty, value);
        }

        public IReadOnlyList<ChainEdge>? Edges
        {
            get => (IReadOnlyList<ChainEdge>?)GetValue(EdgesProperty);
            set => SetValue(EdgesProperty, value);
        }

        private const double CardWidth = 210, CardHeight = 46, ColumnGap = 90, RowGap = 14, Pad = 8;
        private ChainNode? _hover;

        public ChainGraph()
        {
            Cursor = Cursors.Arrow;
            ToolTipService.SetInitialShowDelay(this, 300);
        }

        /// <summary>Columns by longest path from the starts (cycles cut where they close), rows by what leads in, then file order.</summary>
        private void Layout()
        {
            var nodes = Nodes ?? Array.Empty<ChainNode>();
            var edges = (Edges ?? Array.Empty<ChainEdge>()).Where(e => !ReferenceEquals(e.From, e.To)).ToList();
            var into = nodes.ToDictionary(n => n, n => edges.Where(e => ReferenceEquals(e.To, n)).Select(e => e.From).ToList());
            foreach (var n in nodes)
                n.Layer = -1;
            var visiting = new HashSet<ChainNode>();
            int LayerOf(ChainNode n)
            {
                if (n.Layer >= 0)
                    return n.Layer;
                if (!visiting.Add(n))
                    return 0; // a cycle: the edge closing it doesn't push columns
                int layer = n.IsSpell ? 0 : 0;
                foreach (var from in into[n])
                    if (!visiting.Contains(from) || from.Layer >= 0)
                        layer = Math.Max(layer, LayerOf(from) + 1);
                visiting.Remove(n);
                n.Layer = layer;
                return layer;
            }
            foreach (var n in nodes.OrderBy(n => n.Order))
                LayerOf(n);
            foreach (var column in nodes.GroupBy(n => n.Layer).OrderBy(g => g.Key))
            {
                // rows: near what leads in (the average row of its sources), then file order
                var ordered = column.OrderBy(n => into[n].Count > 0 ? into[n].Average(f => f.Y) : n.Order * 1e-6).ThenBy(n => n.Order).ToList();
                double y = Pad;
                foreach (var n in ordered)
                {
                    n.Y = y;
                    n.Bounds = new Rect(Pad + n.Layer * (CardWidth + ColumnGap), y, CardWidth, CardHeight);
                    y += CardHeight + RowGap;
                }
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Layout();
            var nodes = Nodes ?? Array.Empty<ChainNode>();
            if (nodes.Count == 0)
                return new Size(0, 0);
            return new Size(nodes.Max(n => n.Bounds.Right) + Pad, nodes.Max(n => n.Bounds.Bottom) + Pad);
        }

        private static readonly Typeface Face = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        private static readonly Typeface Bold = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);

        private static Brush Frozen(string color)
        {
            var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            b.Freeze();
            return b;
        }

        private static readonly Dictionary<string, Brush> EdgeColors = new()
        {
            ["code"] = Frozen("#E0A040"), ["excludes"] = Frozen("#E5484D"), ["delay"] = Frozen("#5BA8F5"),
            ["skip"] = Frozen("#5BA8F5"), ["variable"] = Frozen("#4CC38A"), ["choice"] = Frozen("#B07CF7"), ["spell"] = Frozen("#9CA3AF"),
        };

        private static readonly Brush CardBrush = Frozen("#2A2A35"), CurrentBrush = Frozen("#3A3020"), SpellBrush = Frozen("#252E3A"),
            HoverBrush = Frozen("#34343F"), Border = Frozen("#505060"), CurrentBorder = Frozen("#CD853F"), Text = Frozen("#E8E8E8"), Muted = Frozen("#9A9AA8");

        protected override void OnRender(DrawingContext dc)
        {
            var nodes = Nodes ?? Array.Empty<ChainNode>();
            double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
            foreach (var e in Edges ?? Array.Empty<ChainEdge>())
            {
                var brush = EdgeColors.TryGetValue(e.Kind, out var b) ? b : Muted;
                var pen = new Pen(brush, 1.4);
                if (e.Kind == "excludes" || e.Kind == "spell" || e.Kind == "skip")
                    pen.DashStyle = DashStyles.Dash;
                Point a, z;
                if (ReferenceEquals(e.From, e.To))
                    continue;
                bool backwards = e.To.Bounds.Left <= e.From.Bounds.Left;
                a = new Point(e.From.Bounds.Right, e.From.Bounds.Top + CardHeight / 2);
                z = new Point(e.To.Bounds.Left, e.To.Bounds.Top + CardHeight / 2);
                var geometry = new StreamGeometry();
                using (var g = geometry.Open())
                {
                    g.BeginFigure(a, false, false);
                    if (backwards)
                    {
                        // a link back (a cycle): around below
                        double below = Math.Max(e.From.Bounds.Bottom, e.To.Bounds.Bottom) + RowGap / 2;
                        g.BezierTo(new Point(a.X + 40, a.Y), new Point(a.X + 40, below), new Point((a.X + z.X) / 2, below), true, false);
                        g.BezierTo(new Point(z.X - 40, below), new Point(z.X - 40, z.Y), z, true, false);
                    }
                    else
                    {
                        double mid = (a.X + z.X) / 2;
                        g.BezierTo(new Point(mid, a.Y), new Point(mid, z.Y), z, true, false);
                    }
                }
                geometry.Freeze();
                dc.DrawGeometry(null, pen, geometry);
                // arrow head
                var head = new StreamGeometry();
                using (var g = head.Open())
                {
                    g.BeginFigure(z, true, true);
                    g.LineTo(new Point(z.X - 7, z.Y - 4), true, false);
                    g.LineTo(new Point(z.X - 7, z.Y + 4), true, false);
                }
                head.Freeze();
                dc.DrawGeometry(brush, null, head);
                // labels while there's room; a dense map shows them when pointing at a card
                if (!backwards && !string.IsNullOrEmpty(e.Label) && (Edges?.Count ?? 0) <= 14)
                {
                    var label = new FormattedText(e.Label, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Face, 9, brush, dpi);
                    var at = new Point((a.X + z.X) / 2 - label.Width / 2, (a.Y + z.Y) / 2 - label.Height - 1);
                    dc.DrawRectangle(Frozen("#C01A1A1F"), null, new Rect(at.X - 2, at.Y, label.Width + 4, label.Height));
                    dc.DrawText(label, at);
                }
            }
            foreach (var n in nodes)
            {
                var fill = ReferenceEquals(n, _hover) ? HoverBrush : n.IsCurrent ? CurrentBrush : n.IsSpell ? SpellBrush : CardBrush;
                dc.DrawRoundedRectangle(fill, new Pen(n.IsCurrent ? CurrentBorder : Border, n.IsCurrent ? 2 : 1), n.Bounds, 5, 5);
                var title = new FormattedText(n.Title, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, n.IsCurrent ? Bold : Face, 11, Text, dpi)
                {
                    MaxTextWidth = CardWidth - 12, MaxTextHeight = 28, Trimming = TextTrimming.CharacterEllipsis,
                };
                dc.DrawText(title, new Point(n.Bounds.Left + 6, n.Bounds.Top + 3));
                var sub = new FormattedText(n.Subtitle, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Face, 9, Muted, dpi)
                {
                    MaxTextWidth = CardWidth - 12, MaxLineCount = 1, Trimming = TextTrimming.CharacterEllipsis,
                };
                dc.DrawText(sub, new Point(n.Bounds.Left + 6, n.Bounds.Bottom - 14));
            }
        }

        private ChainNode? At(Point p) => (Nodes ?? Array.Empty<ChainNode>()).FirstOrDefault(n => n.Bounds.Contains(p));

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var n = At(e.GetPosition(this));
            if (!ReferenceEquals(n, _hover))
            {
                _hover = n;
                Cursor = n != null ? Cursors.Hand : Cursors.Arrow;
                if (n != null)
                {
                    var edges = Edges ?? Array.Empty<ChainEdge>();
                    var lines = edges.Where(x => ReferenceEquals(x.To, n)).Select(x => $"← {x.Label}: {x.From.Title}")
                        .Concat(edges.Where(x => ReferenceEquals(x.From, n)).Select(x => $"→ {x.Label}: {x.To.Title}")).Take(16);
                    ToolTip = $"{n.Title}\n{n.Subtitle}\n{string.Join("\n", lines)}{(n.IsCurrent ? "" : "\n(click to open)")}";
                }
                else
                    ToolTip = null;
                InvalidateVisual();
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = null;
            InvalidateVisual();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (At(e.GetPosition(this)) is ChainNode n && !n.IsCurrent)
                n.Open();
        }
    }
}
