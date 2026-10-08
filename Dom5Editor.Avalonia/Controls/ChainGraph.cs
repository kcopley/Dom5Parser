using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;
using Dom5Editor.UI;
using Dom5Editor.UI.Controls;

namespace Dom5Editor.Ava.Controls
{
    /// <summary>
    /// A chain of events drawn left to right (docs/EVENT_EDITOR.md, E-4): each card in the column
    /// after the cards leading to it, arrows in the colour of what links them (one per code or
    /// variable number; delay, choice, blocks and spell their own: ChainEdge.Color).
    /// - Pointing at a card shows what it's linked to: its own arrows bold and labelled, the rest
    ///   of the chain before and after it plain, everything else faded. A click keeps that (a click
    ///   on empty space lets go); a double-click opens the event. Pointing at an arrow names it.
    /// - Dragging with the middle button, or the left one on empty space, moves the map in the
    ///   scroll viewer around it; Ctrl+wheel zooms at the pointer; ZoomIn/ZoomOut/Fit for buttons.
    /// On the core's ChainNode/ChainEdge (their Box is the card's place, unzoomed).
    /// </summary>
    public sealed class ChainGraph : Control
    {
        public static readonly StyledProperty<IReadOnlyList<ChainNode>?> NodesProperty =
            AvaloniaProperty.Register<ChainGraph, IReadOnlyList<ChainNode>?>(nameof(Nodes));

        public static readonly StyledProperty<IReadOnlyList<ChainEdge>?> EdgesProperty =
            AvaloniaProperty.Register<ChainGraph, IReadOnlyList<ChainEdge>?>(nameof(Edges));

        public static readonly StyledProperty<double> ZoomProperty =
            AvaloniaProperty.Register<ChainGraph, double>(nameof(Zoom), 1.0, coerce: (_, z) => Math.Clamp(z, MinZoom, MaxZoom));

        static ChainGraph()
        {
            AffectsMeasure<ChainGraph>(NodesProperty, EdgesProperty, ZoomProperty);
            AffectsRender<ChainGraph>(NodesProperty, EdgesProperty, ZoomProperty);
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

        /// <summary>The map's scale (0.25 to 2): Ctrl+wheel, the zoom buttons, Fit.</summary>
        public double Zoom
        {
            get => GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }

        /// <summary>The card clicked (its links stay shown), or null.</summary>
        public ChainNode? SelectedCard => _selected;

        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        /// <summary>The whole map in the view (never larger than 100%).</summary>
        public ICommand FitCommand { get; }
        public ICommand ActualSizeCommand { get; }

        private const double CardWidth = 210, CardHeight = 46, ColumnGap = 90, RowGap = 14, Pad = 8;
        private const double MinZoom = 0.25, MaxZoom = 2;
        private ChainNode? _hover, _selected, _pressed;
        private ChainEdge? _hoverEdge;
        // a drag that moves the map: where it started (in the scroll viewer) and the offset then
        private ScrollViewer? _scroller;
        private Point _panFrom;
        private Vector _panOffset;
        private bool _panning, _panned;
        // each arrow's ends (spread along the cards' sides) and points along it, for pointing at it
        private readonly Dictionary<ChainEdge, (Point A, Point Z)> _ends = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<ChainEdge, Point[]> _paths = new(ReferenceEqualityComparer.Instance);
        private static readonly Cursor HandCursor = new Cursor(StandardCursorType.Hand), MoveCursor = new Cursor(StandardCursorType.SizeAll);

        public ChainGraph()
        {
            ToolTip.SetShowDelay(this, 300);
            Focusable = true;
            // a card's right click menu: open it here or in a window of its own (Link)
            ContextRequested += (s, e) =>
            {
                if (e.TryGetPosition(this, out var p) && At(OnMap(p)) is { IsCurrent: false } card)
                {
                    Link.Menu(card.Open).Open(this);
                    e.Handled = true;
                }
            };
            ZoomInCommand = new RelayCommand(() => ZoomAt(Zoom * 1.25, null));
            ZoomOutCommand = new RelayCommand(() => ZoomAt(Zoom / 1.25, null));
            ActualSizeCommand = new RelayCommand(() => ZoomAt(1, null));
            FitCommand = new RelayCommand(Fit);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            // the map rebuilt (after an edit): keep the card clicked, by what it is
            if (change.Property == NodesProperty)
            {
                _selected = _selected == null ? null : Nodes?.FirstOrDefault(n => Equals(n.Key, _selected.Key));
                _hover = null;
                _hoverEdge = null;
            }
        }

        /// <summary>Columns by longest path from the starts (cycles cut where they close), rows by what leads in, then file order.</summary>
        private void Layout()
        {
            var nodes = Nodes ?? Array.Empty<ChainNode>();
            var edges = Arrows();
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
            Ports(edges);
        }

        private List<ChainEdge> Arrows() => (Edges ?? Array.Empty<ChainEdge>()).Where(e => !ReferenceEquals(e.From, e.To)).ToList();

        /// <summary>
        /// Where each arrow leaves and enters its cards: spread along the side, in the order of the
        /// other ends' rows, so arrows don't all meet in one point and can be followed.
        /// </summary>
        private void Ports(List<ChainEdge> edges)
        {
            static double Middle(ChainNode n) => n.Bounds.Top + CardHeight / 2;
            static double Spread(ChainNode n, int i, int count) =>
                count == 1 ? Middle(n) : n.Bounds.Top + 8 + (CardHeight - 16) * i / (count - 1);
            var leaves = new Dictionary<ChainEdge, double>(ReferenceEqualityComparer.Instance);
            var enters = new Dictionary<ChainEdge, double>(ReferenceEqualityComparer.Instance);
            foreach (var g in edges.GroupBy(e => e.From))
            {
                var list = g.OrderBy(e => Middle(e.To)).ToList();
                for (int i = 0; i < list.Count; i++)
                    leaves[list[i]] = Spread(g.Key, i, list.Count);
            }
            foreach (var g in edges.GroupBy(e => e.To))
            {
                var list = g.OrderBy(e => Middle(e.From)).ToList();
                for (int i = 0; i < list.Count; i++)
                    enters[list[i]] = Spread(g.Key, i, list.Count);
            }
            _ends.Clear();
            foreach (var e in edges)
                _ends[e] = (new Point(e.From.Bounds.Right, leaves[e]), new Point(e.To.Bounds.Left, enters[e]));
        }

        private Size ContentSize()
        {
            var nodes = Nodes ?? Array.Empty<ChainNode>();
            return nodes.Count == 0 ? new Size(0, 0) : new Size(nodes.Max(n => n.Bounds.Right) + Pad, nodes.Max(n => n.Bounds.Bottom) + Pad);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Layout();
            var size = ContentSize();
            return new Size(size.Width * Zoom, size.Height * Zoom);
        }

        private static readonly Dictionary<(string, double), IBrush> _brushes = new();

        /// <summary>A colour, see-through for what's faded (drawn so, not under a layer's opacity, which garbles text).</summary>
        private static IBrush Brush(string color, double opacity = 1)
        {
            if (!_brushes.TryGetValue((color, opacity), out var brush))
                _brushes[(color, opacity)] = brush = new ImmutableSolidColorBrush(Color.Parse(color), opacity);
            return brush;
        }

        private const string CardColor = "#2A2A35", CurrentColor = "#3A3020", SpellColor = "#252E3A", HoverColor = "#34343F",
            CardBorder = "#505060", CurrentBorder = "#CD853F", SelectedBorder = "#E8E8E8", TextColor = "#E8E8E8", MutedColor = "#9A9AA8";
        private static readonly IBrush LabelBack = Brush("#1A1A1F", 0.88);

        private static Rect RectOf(Box b) => new Rect(b.Left, b.Top, b.Width, b.Height);

        /// <summary>
        /// What to show strongly: for a card (clicked, else pointed at), its own arrows, and the
        /// chain before and after it (what leads to it, what it leads to, on and on); for an arrow
        /// pointed at, that arrow. Null: everything as it is.
        /// </summary>
        private (HashSet<ChainEdge> Direct, HashSet<ChainEdge> Chain, HashSet<ChainNode> Cards)? Highlight(List<ChainEdge> edges)
        {
            var direct = new HashSet<ChainEdge>(ReferenceEqualityComparer.Instance);
            var chain = new HashSet<ChainEdge>(ReferenceEqualityComparer.Instance);
            var cards = new HashSet<ChainNode>(ReferenceEqualityComparer.Instance);
            var focus = _selected ?? _hover;
            if (focus == null)
            {
                if (_hoverEdge == null)
                    return null;
                direct.Add(_hoverEdge);
                cards.Add(_hoverEdge.From);
                cards.Add(_hoverEdge.To);
                return (direct, chain, cards);
            }
            cards.Add(focus);
            foreach (var e in edges.Where(e => ReferenceEquals(e.From, focus) || ReferenceEquals(e.To, focus)))
                direct.Add(e);
            // before it and after it, as far as the links go
            foreach (bool forward in new[] { true, false })
            {
                var todo = new Queue<ChainNode>(new[] { focus });
                var seen = new HashSet<ChainNode>(ReferenceEqualityComparer.Instance) { focus };
                while (todo.Count > 0)
                {
                    var n = todo.Dequeue();
                    foreach (var e in edges.Where(e => ReferenceEquals(forward ? e.From : e.To, n)))
                    {
                        chain.Add(e);
                        var next = forward ? e.To : e.From;
                        cards.Add(next);
                        if (seen.Add(next))
                            todo.Enqueue(next);
                    }
                }
            }
            if (_hoverEdge != null)
                direct.Add(_hoverEdge);
            return (direct, chain, cards);
        }

        public override void Render(DrawingContext dc)
        {
            var nodes = Nodes ?? Array.Empty<ChainNode>();
            var edges = Arrows();
            var family = TextElement.GetFontFamily(this);
            var face = new Typeface(family);
            var bold = new Typeface(family, FontStyle.Normal, FontWeight.SemiBold);
            // (transparent: the whole area takes the pointer, for the hover, the tooltip and dragging)
            dc.FillRectangle(Brushes.Transparent, new Rect(Bounds.Size));
            var lit = Highlight(edges);
            using var scale = dc.PushTransform(Matrix.CreateScale(Zoom, Zoom));
            _paths.Clear();
            // the faded ones first, the ones that matter on top
            int Rank(ChainEdge e) => lit == null ? 1 : lit.Value.Direct.Contains(e) ? 2 : lit.Value.Chain.Contains(e) ? 1 : 0;
            foreach (var e in edges.OrderBy(Rank))
            {
                if (!_ends.TryGetValue(e, out var ends))
                    continue;
                int rank = Rank(e);
                var brush = Brush(e.Color, rank switch { 2 => 1.0, 1 => lit == null ? 1.0 : 0.55, _ => 0.12 });
                var pen = new Pen(brush, rank == 2 && lit != null ? 2.6 : 1.4, e.Dashed ? DashStyle.Dash : null);
                var (a, z) = ends;
                bool backwards = e.To.Bounds.Left <= e.From.Bounds.Left;
                Point[] curve;
                if (backwards)
                {
                    // a link back (a cycle): around below
                    double below = Math.Max(e.From.Bounds.Bottom, e.To.Bounds.Bottom) + RowGap / 2;
                    var m = new Point((a.X + z.X) / 2, below);
                    curve = Bezier(a, new Point(a.X + 40, a.Y), new Point(a.X + 40, below), m)
                        .Concat(Bezier(m, new Point(z.X - 40, below), new Point(z.X - 40, z.Y), z).Skip(1)).ToArray();
                }
                else
                {
                    double mid = (a.X + z.X) / 2;
                    curve = Bezier(a, new Point(mid, a.Y), new Point(mid, z.Y), z).ToArray();
                }
                _paths[e] = curve;
                var geometry = new StreamGeometry();
                using (var g = geometry.Open())
                {
                    g.BeginFigure(curve[0], false);
                    for (int k = 1; k < curve.Length; k++)
                        g.LineTo(curve[k]);
                    g.EndFigure(false);
                }
                dc.DrawGeometry(null, pen, geometry);
                // arrow head
                var head = new StreamGeometry();
                using (var g = head.Open())
                {
                    g.BeginFigure(z, true);
                    g.LineTo(new Point(z.X - 8, z.Y - 4.5));
                    g.LineTo(new Point(z.X - 8, z.Y + 4.5));
                    g.EndFigure(true);
                }
                dc.DrawGeometry(brush, null, head);
            }
            // labels under the cards on a plain map; over them for what's pointed at or clicked (they're few)
            if (lit == null)
                Labels(dc, edges, null, face);
            foreach (var n in nodes)
            {
                // (a faded card keeps its fill, so the arrows behind it stay hidden; its text and edge dim)
                double dim = lit == null || lit.Value.Cards.Contains(n) ? 1.0 : 0.35;
                var fill = Brush(ReferenceEquals(n, _hover) ? HoverColor : n.IsCurrent ? CurrentColor : n.IsSpell ? SpellColor : CardColor);
                var border = ReferenceEquals(n, _selected) ? new Pen(Brush(SelectedBorder), 2)
                    : new Pen(Brush(n.IsCurrent ? CurrentBorder : CardBorder, dim), n.IsCurrent ? 2 : 1);
                dc.DrawRectangle(fill, border, RectOf(n.Bounds), 5, 5);
                var title = new FormattedText(n.Title, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, n.IsCurrent ? bold : face, 11, Brush(TextColor, dim))
                {
                    MaxTextWidth = CardWidth - 12, MaxTextHeight = 28, Trimming = TextTrimming.CharacterEllipsis,
                };
                dc.DrawText(title, new Point(n.Bounds.Left + 6, n.Bounds.Top + 3));
                var sub = new FormattedText(n.Subtitle, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, face, 9, Brush(MutedColor, dim))
                {
                    MaxTextWidth = CardWidth - 12, MaxLineCount = 1, Trimming = TextTrimming.CharacterEllipsis,
                };
                dc.DrawText(sub, new Point(n.Bounds.Left + 6, n.Bounds.Bottom - 14));
            }
            if (lit != null)
                Labels(dc, edges, edges.Count <= 14 ? lit.Value.Direct.Concat(lit.Value.Chain).ToHashSet() : lit.Value.Direct, face);
        }

        /// <summary>The arrows' labels: every one while there's room; for what's pointed at or clicked, those of its arrows (on a small map, its whole chain's).</summary>
        private void Labels(DrawingContext dc, List<ChainEdge> edges, HashSet<ChainEdge>? direct, Typeface face)
        {
            foreach (var e in edges)
            {
                if (string.IsNullOrEmpty(e.Label) || !_paths.TryGetValue(e, out var curve)
                    || !(direct == null ? edges.Count <= 14 && e.To.Bounds.Left > e.From.Bounds.Left : direct.Contains(e)))
                    continue;
                var label = new FormattedText(e.Label, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, face, 9, Brush(e.Color));
                var mid = curve[curve.Length / 2];
                var at = new Point(mid.X - label.Width / 2, mid.Y - label.Height - 1);
                dc.FillRectangle(LabelBack, new Rect(at.X - 2, at.Y, label.Width + 4, label.Height), 2);
                dc.DrawText(label, at);
            }
        }

        /// <summary>Points along a cubic curve (enough to draw it smoothly and to point at it).</summary>
        private static IEnumerable<Point> Bezier(Point p0, Point p1, Point p2, Point p3)
        {
            const int steps = 24;
            for (int k = 0; k <= steps; k++)
            {
                double t = (double)k / steps, u = 1 - t;
                yield return new Point(u * u * u * p0.X + 3 * u * u * t * p1.X + 3 * u * t * t * p2.X + t * t * t * p3.X,
                                       u * u * u * p0.Y + 3 * u * u * t * p1.Y + 3 * u * t * t * p2.Y + t * t * t * p3.Y);
            }
        }

        /// <summary>A pointer position on the control as a place on the (unzoomed) map.</summary>
        private Point OnMap(Point p) => new Point(p.X / Zoom, p.Y / Zoom);

        private ChainNode? At(Point p) => (Nodes ?? Array.Empty<ChainNode>()).FirstOrDefault(n => n.Bounds.Contains(p.X, p.Y));

        /// <summary>The arrow passing within a few pixels of a place on the map, or null.</summary>
        private ChainEdge? EdgeAt(Point p)
        {
            double reach = 5 / Zoom, best = reach * reach;
            ChainEdge? found = null;
            foreach (var (e, curve) in _paths)
                for (int k = 1; k < curve.Length; k++)
                {
                    double d = DistanceSquared(p, curve[k - 1], curve[k]);
                    if (d < best)
                    {
                        best = d;
                        found = e;
                    }
                }
            return found;
        }

        private static double DistanceSquared(Point p, Point a, Point b)
        {
            double dx = b.X - a.X, dy = b.Y - a.Y, len = dx * dx + dy * dy;
            double t = len == 0 ? 0 : Math.Clamp(((p.X - a.X) * dx + (p.Y - a.Y) * dy) / len, 0, 1);
            double x = a.X + t * dx - p.X, y = a.Y + t * dy - p.Y;
            return x * x + y * y;
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            var point = e.GetCurrentPoint(this);
            var card = At(OnMap(point.Position));
            if (point.Properties.IsLeftButtonPressed && card != null)
            {
                // a double-click opens it; a click (on release) keeps its links shown
                if (e.ClickCount >= 2 && !card.IsCurrent)
                {
                    card.Open();
                    e.Handled = true;
                    return;
                }
                _pressed = card;
                e.Handled = true;
                return;
            }
            if (point.Properties.IsMiddleButtonPressed || point.Properties.IsLeftButtonPressed)
            {
                _scroller = this.FindAncestorOfType<ScrollViewer>();
                _panning = true;
                _panned = false;
                _panFrom = e.GetPosition(_scroller ?? (Visual)this);
                _panOffset = _scroller?.Offset ?? default;
                e.Pointer.Capture(this);
                e.Handled = true;
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            if (_panning)
            {
                var delta = e.GetPosition(_scroller ?? (Visual)this) - _panFrom;
                if (!_panned && Math.Abs(delta.X) + Math.Abs(delta.Y) > 3)
                {
                    _panned = true;
                    Cursor = MoveCursor;
                }
                if (_panned && _scroller != null)
                    _scroller.Offset = _panOffset - delta;
                return;
            }
            var place = OnMap(e.GetPosition(this));
            var n = At(place);
            var edge = n == null ? EdgeAt(place) : null;
            if (ReferenceEquals(n, _hover) && ReferenceEquals(edge, _hoverEdge))
                return;
            _hover = n;
            _hoverEdge = edge;
            Cursor = n != null ? HandCursor : Cursor.Default;
            var edges = Edges ?? Array.Empty<ChainEdge>();
            if (n != null)
            {
                var lines = edges.Where(x => ReferenceEquals(x.To, n)).Select(x => $"← {x.Label}: {x.From.Title}")
                    .Concat(edges.Where(x => ReferenceEquals(x.From, n)).Select(x => $"→ {x.Label}: {x.To.Title}")).Take(16);
                var how = n.IsCurrent ? "click: keep its links shown" : "click: keep its links shown · double-click: open it";
                ToolTip.SetTip(this, $"{n.Title}\n{n.Subtitle}\n{string.Join("\n", lines)}\n({how})");
            }
            else if (edge != null)
                ToolTip.SetTip(this, $"{edge.Label}\n{edge.From.Title}\n→ {edge.To.Title}");
            else
                ToolTip.SetTip(this, null);
            InvalidateVisual();
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (_panning)
            {
                _panning = false;
                e.Pointer.Capture(null);
                Cursor = Cursor.Default;
                // a click on empty space (not a drag) lets go of the card clicked
                if (!_panned && e.InitialPressMouseButton == MouseButton.Left && _selected != null)
                {
                    _selected = null;
                    InvalidateVisual();
                }
                // a middle click on a card (not a drag) opens it in a window of its own, as a browser's link
                if (!_panned && e.InitialPressMouseButton == MouseButton.Middle && At(OnMap(e.GetPosition(this))) is { IsCurrent: false } card)
                    Link.InNewWindow(card.Open);
                return;
            }
            if (e.InitialPressMouseButton == MouseButton.Left && _pressed != null && ReferenceEquals(At(OnMap(e.GetPosition(this))), _pressed))
            {
                _selected = ReferenceEquals(_selected, _pressed) ? null : _pressed;
                InvalidateVisual();
            }
            _pressed = null;
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);
            if (_panning)
                return;
            _hover = null;
            _hoverEdge = null;
            Cursor = Cursor.Default;
            InvalidateVisual();
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            // Ctrl+wheel zooms at the pointer; the wheel alone scrolls, as everywhere
            if (!e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                base.OnPointerWheelChanged(e);
                return;
            }
            ZoomAt(Zoom * (e.Delta.Y > 0 ? 1.15 : 1 / 1.15), e.GetPosition(this));
            e.Handled = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape && _selected != null)
            {
                _selected = null;
                InvalidateVisual();
                e.Handled = true;
            }
        }

        /// <summary>Zooms keeping a point where it is on screen (the pointer; else the view's middle).</summary>
        private void ZoomAt(double zoom, Point? at)
        {
            var scroller = this.FindAncestorOfType<ScrollViewer>();
            double old = Zoom;
            zoom = Math.Clamp(zoom, MinZoom, MaxZoom);
            if (Math.Abs(zoom - old) < 0.001)
                return;
            // the point under the pointer, on the map, and where it is in the view
            var inView = at is Point p && scroller != null ? p - (Vector)scroller.Offset
                : scroller != null ? new Point(scroller.Viewport.Width / 2, scroller.Viewport.Height / 2) : default;
            var onMap = new Point((inView.X + (scroller?.Offset.X ?? 0)) / old, (inView.Y + (scroller?.Offset.Y ?? 0)) / old);
            Zoom = zoom;
            if (scroller != null)
            {
                scroller.UpdateLayout();
                scroller.Offset = new Vector(Math.Max(0, onMap.X * zoom - inView.X), Math.Max(0, onMap.Y * zoom - inView.Y));
            }
        }

        private void Fit()
        {
            var scroller = this.FindAncestorOfType<ScrollViewer>();
            var size = ContentSize();
            if (scroller == null || size.Width <= 0 || size.Height <= 0)
                return;
            var view = scroller.Viewport;
            Zoom = Math.Min(1, Math.Min(view.Width / size.Width, view.Height / size.Height));
            scroller.UpdateLayout();
            scroller.Offset = default;
        }
    }
}
