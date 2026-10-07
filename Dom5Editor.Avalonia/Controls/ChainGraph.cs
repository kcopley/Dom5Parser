using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Dom5Editor.UI.Controls;

namespace Dom5Editor.Ava.Controls
{
    /// <summary>
    /// A chain of events drawn left to right (docs/EVENT_EDITOR.md, E-4): each card in the column
    /// after the cards leading to it, arrows labelled with what links them (code, delay, variable,
    /// choice, spell). Clicking a card opens it. As the WPF editor's ChainGraph, on the core's
    /// ChainNode/ChainEdge (their Box is the card's place).
    /// </summary>
    public sealed class ChainGraph : Control
    {
        public static readonly StyledProperty<IReadOnlyList<ChainNode>?> NodesProperty =
            AvaloniaProperty.Register<ChainGraph, IReadOnlyList<ChainNode>?>(nameof(Nodes));

        public static readonly StyledProperty<IReadOnlyList<ChainEdge>?> EdgesProperty =
            AvaloniaProperty.Register<ChainGraph, IReadOnlyList<ChainEdge>?>(nameof(Edges));

        static ChainGraph()
        {
            AffectsMeasure<ChainGraph>(NodesProperty, EdgesProperty);
            AffectsRender<ChainGraph>(NodesProperty, EdgesProperty);
        }

        public IReadOnlyList<ChainNode>? Nodes
        {
            get => GetValue(NodesProperty);
            set => SetValue(NodesProperty, value);
        }

        public IReadOnlyList<ChainEdge>? Edges
        {
            get => GetValue(EdgesProperty);
            set => SetValue(EdgesProperty, value);
        }

        private const double CardWidth = 210, CardHeight = 46, ColumnGap = 90, RowGap = 14, Pad = 8;
        private ChainNode? _hover;
        private static readonly Cursor HandCursor = new Cursor(StandardCursorType.Hand);

        public ChainGraph()
        {
            ToolTip.SetShowDelay(this, 300);
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
                int layer = 0;
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
                    n.Bounds = new Box(Pad + n.Layer * (CardWidth + ColumnGap), y, CardWidth, CardHeight);
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

        private static IBrush Solid(string color) => new ImmutableSolidColorBrush(Color.Parse(color));

        private static readonly Dictionary<string, IBrush> EdgeColors = new()
        {
            ["code"] = Solid("#E0A040"), ["excludes"] = Solid("#E5484D"), ["delay"] = Solid("#5BA8F5"),
            ["skip"] = Solid("#5BA8F5"), ["variable"] = Solid("#4CC38A"), ["choice"] = Solid("#B07CF7"), ["spell"] = Solid("#9CA3AF"),
        };

        private static readonly IBrush CardBrush = Solid("#2A2A35"), CurrentBrush = Solid("#3A3020"), SpellBrush = Solid("#252E3A"),
            HoverBrush = Solid("#34343F"), CardBorder = Solid("#505060"), CurrentBorder = Solid("#CD853F"), Text = Solid("#E8E8E8"),
            Muted = Solid("#9A9AA8"), LabelBack = Solid("#C01A1A1F");

        private static Rect RectOf(Box b) => new Rect(b.Left, b.Top, b.Width, b.Height);

        public override void Render(DrawingContext dc)
        {
            var nodes = Nodes ?? Array.Empty<ChainNode>();
            var edges = Edges ?? Array.Empty<ChainEdge>();
            var family = TextElement.GetFontFamily(this);
            var face = new Typeface(family);
            var bold = new Typeface(family, FontStyle.Normal, FontWeight.SemiBold);
            // (transparent: the whole area takes the pointer, for the hover and the tooltip)
            dc.FillRectangle(Brushes.Transparent, new Rect(Bounds.Size));
            foreach (var e in edges)
            {
                if (ReferenceEquals(e.From, e.To))
                    continue;
                var brush = EdgeColors.TryGetValue(e.Kind, out var b) ? b : Muted;
                var dashed = e.Kind == "excludes" || e.Kind == "spell" || e.Kind == "skip";
                var pen = new Pen(brush, 1.4, dashed ? DashStyle.Dash : null);
                bool backwards = e.To.Bounds.Left <= e.From.Bounds.Left;
                var a = new Point(e.From.Bounds.Right, e.From.Bounds.Top + CardHeight / 2);
                var z = new Point(e.To.Bounds.Left, e.To.Bounds.Top + CardHeight / 2);
                var geometry = new StreamGeometry();
                using (var g = geometry.Open())
                {
                    g.BeginFigure(a, false);
                    if (backwards)
                    {
                        // a link back (a cycle): around below
                        double below = Math.Max(e.From.Bounds.Bottom, e.To.Bounds.Bottom) + RowGap / 2;
                        g.CubicBezierTo(new Point(a.X + 40, a.Y), new Point(a.X + 40, below), new Point((a.X + z.X) / 2, below));
                        g.CubicBezierTo(new Point(z.X - 40, below), new Point(z.X - 40, z.Y), z);
                    }
                    else
                    {
                        double mid = (a.X + z.X) / 2;
                        g.CubicBezierTo(new Point(mid, a.Y), new Point(mid, z.Y), z);
                    }
                    g.EndFigure(false);
                }
                dc.DrawGeometry(null, pen, geometry);
                // arrow head
                var head = new StreamGeometry();
                using (var g = head.Open())
                {
                    g.BeginFigure(z, true);
                    g.LineTo(new Point(z.X - 7, z.Y - 4));
                    g.LineTo(new Point(z.X - 7, z.Y + 4));
                    g.EndFigure(true);
                }
                dc.DrawGeometry(brush, null, head);
                // labels while there's room; a dense map shows them when pointing at a card
                if (!backwards && !string.IsNullOrEmpty(e.Label) && edges.Count <= 14)
                {
                    var label = new FormattedText(e.Label, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, face, 9, brush);
                    var at = new Point((a.X + z.X) / 2 - label.Width / 2, (a.Y + z.Y) / 2 - label.Height - 1);
                    dc.FillRectangle(LabelBack, new Rect(at.X - 2, at.Y, label.Width + 4, label.Height));
                    dc.DrawText(label, at);
                }
            }
            foreach (var n in nodes)
            {
                var fill = ReferenceEquals(n, _hover) ? HoverBrush : n.IsCurrent ? CurrentBrush : n.IsSpell ? SpellBrush : CardBrush;
                var border = new Pen(n.IsCurrent ? CurrentBorder : CardBorder, n.IsCurrent ? 2 : 1);
                dc.DrawRectangle(fill, border, RectOf(n.Bounds), 5, 5);
                var title = new FormattedText(n.Title, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, n.IsCurrent ? bold : face, 11, Text)
                {
                    MaxTextWidth = CardWidth - 12, MaxTextHeight = 28, Trimming = TextTrimming.CharacterEllipsis,
                };
                dc.DrawText(title, new Point(n.Bounds.Left + 6, n.Bounds.Top + 3));
                var sub = new FormattedText(n.Subtitle, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, face, 9, Muted)
                {
                    MaxTextWidth = CardWidth - 12, MaxLineCount = 1, Trimming = TextTrimming.CharacterEllipsis,
                };
                dc.DrawText(sub, new Point(n.Bounds.Left + 6, n.Bounds.Bottom - 14));
            }
        }

        private ChainNode? At(Point p) => (Nodes ?? Array.Empty<ChainNode>()).FirstOrDefault(n => n.Bounds.Contains(p.X, p.Y));

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            var n = At(e.GetPosition(this));
            if (ReferenceEquals(n, _hover))
                return;
            _hover = n;
            Cursor = n != null ? HandCursor : Cursor.Default;
            if (n != null)
            {
                var edges = Edges ?? Array.Empty<ChainEdge>();
                var lines = edges.Where(x => ReferenceEquals(x.To, n)).Select(x => $"← {x.Label}: {x.From.Title}")
                    .Concat(edges.Where(x => ReferenceEquals(x.From, n)).Select(x => $"→ {x.Label}: {x.To.Title}")).Take(16);
                ToolTip.SetTip(this, $"{n.Title}\n{n.Subtitle}\n{string.Join("\n", lines)}{(n.IsCurrent ? "" : "\n(click to open)")}");
            }
            else
            {
                ToolTip.SetTip(this, null);
            }
            InvalidateVisual();
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);
            _hover = null;
            Cursor = Cursor.Default;
            InvalidateVisual();
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (e.InitialPressMouseButton == MouseButton.Left && At(e.GetPosition(this)) is ChainNode n && !n.IsCurrent)
                n.Open();
        }
    }
}
