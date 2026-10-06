using System.Windows;
using System.Windows.Media;

namespace Dom5Editor.UI.Controls
{
    /// <summary>
    /// A small icon for something modders recognize at a glance: a magic path or gem, a monster
    /// stat, a cost. Drawn from vector shapes (GameIcons), so it scales and needs no game art.
    /// Shown next to its text, not instead of it. Kind: "path:F" (F A W E S D N G B H, R random),
    /// "gem:F", or a stat ("hp", "att", "gold", ...). An unknown kind draws nothing.
    /// </summary>
    public sealed class GameIcon : FrameworkElement
    {
        public static readonly DependencyProperty KindProperty = DependencyProperty.Register(
            nameof(Kind), typeof(string), typeof(GameIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public GameIcon()
        {
            Width = 16;
            Height = 16;
            SnapsToDevicePixels = true;
        }

        public string? Kind
        {
            get => (string?)GetValue(KindProperty);
            set => SetValue(KindProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize) =>
            GameIcons.Get(Kind) == null ? new Size(0, 0) : new Size(double.IsNaN(Width) ? 16 : Width, double.IsNaN(Height) ? 16 : Height);

        protected override void OnRender(DrawingContext dc)
        {
            double w = ActualWidth, h = ActualHeight;
            if (GameIcons.Bitmap(Kind) is ImageSource image)
            {
                // the game's own icon, fitted in the box
                double scale = Math.Min(w / image.Width, h / image.Height);
                double iw = image.Width * scale, ih = image.Height * scale;
                dc.DrawImage(image, new Rect((w - iw) / 2, (h - ih) / 2, iw, ih));
                return;
            }
            var drawing = GameIcons.Get(Kind);
            if (drawing == null)
                return;
            dc.PushTransform(new ScaleTransform(w / 16.0, h / 16.0));
            dc.DrawDrawing(drawing);
            dc.Pop();
        }
    }

    /// <summary>
    /// The icon shapes, each drawn in a 16 x 16 box: path symbols after the game's (fire a flame,
    /// air wind, water a drop, earth a hammer, astral a star, death a skull, nature a tree,
    /// glamour crystals, blood a bowl, holy candles), used when the game's own icons aren't next to
    /// the editor; gems as cut stones in the path colors; and stats (HP a heart, protection a
    /// helmet, MR a sparkle, ...) after the game's unit window.
    /// </summary>
    public static class GameIcons
    {
        public static readonly IReadOnlyDictionary<string, string> PathColors = new Dictionary<string, string>
        {
            ["F"] = "#F06A2A", ["A"] = "#8FD3FF", ["W"] = "#3B82F6", ["E"] = "#B98446", ["S"] = "#C9A8FF",
            ["D"] = "#C98AA6", ["N"] = "#4CAF50", ["G"] = "#E37AD8", ["B"] = "#C2182E", ["H"] = "#F5C842", ["R"] = "#9E9E9E",
        };

        private static readonly Dictionary<string, Drawing?> _cache = new Dictionary<string, Drawing?>();
        private static readonly Dictionary<string, ImageSource?> _bitmaps = new Dictionary<string, ImageSource?>();

        /// <summary>
        /// The game's own icon for a path or gem, when it's next to the editor (icons/magicicons/Path_F.png,
        /// Gem_F.png): the ones players know from the game. Null otherwise (the vector shape is used).
        /// </summary>
        public static ImageSource? Bitmap(string? kind)
        {
            if (string.IsNullOrEmpty(kind) || !(kind.StartsWith("path:") || kind.StartsWith("gem:") || kind.Length == 1))
                return null;
            if (_bitmaps.TryGetValue(kind, out var cached))
                return cached;
            string file = kind.StartsWith("gem:") ? "Gem_" + kind.Substring(4) : "Path_" + (kind.StartsWith("path:") ? kind.Substring(5) : kind);
            ImageSource? image = null;
            try
            {
                var path = System.IO.Path.Combine(AppContext.BaseDirectory, "icons", "magicicons", file + ".png");
                if (System.IO.File.Exists(path))
                {
                    var b = new System.Windows.Media.Imaging.BitmapImage();
                    b.BeginInit();
                    b.UriSource = new Uri(path);
                    b.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    b.EndInit();
                    b.Freeze();
                    image = b;
                }
            }
            catch (Exception)
            {
                image = null; // a broken file: the vector shape instead
            }
            _bitmaps[kind] = image;
            return image;
        }

        public static Drawing? Get(string? kind)
        {
            if (string.IsNullOrEmpty(kind))
                return null;
            if (_cache.TryGetValue(kind, out var d))
                return d;
            d = Build(kind);
            d?.Freeze();
            _cache[kind] = d;
            return d;
        }

        /// <summary>The path letter for a path number (0 fire ... 8 blood, 9 holy), or null.</summary>
        public static string? PathLetter(int path) => path >= 0 && path <= 9 ? "FAWESDNGBH"[path].ToString() : null;

        private static Drawing? Build(string kind)
        {
            var g = new DrawingGroup();
            void Fill(string data, string color) =>
                g.Children.Add(new GeometryDrawing(Brush(color), null, Geometry.Parse(data)));
            void Stroke(string data, string color, double width) =>
                g.Children.Add(new GeometryDrawing(null, new Pen(Brush(color), width) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round, LineJoin = PenLineJoin.Round }, Geometry.Parse(data)));

            const string Drop = "M8,1.5 C8,1.5 3,7.5 3,10.5 C3,13.3 5.2,15 8,15 C10.8,15 13,13.3 13,10.5 C13,7.5 8,1.5 8,1.5 Z";
            const string Skull = "M8,1.5 C4.4,1.5 2,4 2,7.3 C2,9.4 3,10.8 4.5,11.6 L4.5,14.5 L11.5,14.5 L11.5,11.6 C13,10.8 14,9.4 14,7.3 C14,4 11.6,1.5 8,1.5 Z";
            const string SkullHoles = "M5.8,6.3 A1.5,1.5 0 1 1 5.8,9.3 A1.5,1.5 0 1 1 5.8,6.3 Z M10.2,6.3 A1.5,1.5 0 1 1 10.2,9.3 A1.5,1.5 0 1 1 10.2,6.3 Z";
            const string Star4 = "M8,0.5 L9.6,6.4 L15.5,8 L9.6,9.6 L8,15.5 L6.4,9.6 L0.5,8 L6.4,6.4 Z";
            const string Dark = "#26262C";

            if (kind.StartsWith("gem:"))
            {
                var letter = kind.Substring(4);
                if (!PathColors.TryGetValue(letter, out var c))
                    return null;
                Fill("M4,2 H12 L15,6 L8,15 L1,6 Z", c);
                Stroke("M1,6 H15 M6,6 L8,15 L10,6 M4,2 L6,6 L8,2 L10,6 L12,2", "#80FFFFFF", 0.6);
                return g;
            }
            if (kind.StartsWith("path:"))
                kind = kind.Substring(5);
            switch (kind)
            {
                // ---- magic paths ----
                case "F":
                    Fill("M8,0.8 C10.2,4 13,6 13,10 C13,13 10.6,15.2 8,15.2 C5.4,15.2 3,13 3,10 C3,7.4 4.8,6.4 5.4,4.2 C6.4,5.7 6.9,6.8 7,8 C8,6 8.5,3.5 8,0.8 Z", PathColors["F"]);
                    Fill("M8,8.8 C9.2,10.4 10.6,11 10.6,12.6 C10.6,14.1 9.4,15 8,15 C6.6,15 5.4,14.1 5.4,12.6 C5.4,11 7,10.4 8,8.8 Z", "#FFD24A");
                    break;
                case "A":
                    Stroke("M1.5,5 H10 C12,5 13,3.2 11.8,2.2 M1.5,8.2 H12.8 C14.8,8.2 15,11.2 13,11.2 M1.5,11.4 H7.6 C9.6,11.4 10,13.8 8.6,14.4", PathColors["A"], 1.6);
                    break;
                case "W":
                    Fill(Drop, PathColors["W"]);
                    Stroke("M5.6,10.4 C5.6,11.8 6.4,12.8 7.4,13", "#CCFFFFFF", 1);
                    break;
                case "E":
                    // a hammer
                    Stroke("M3,14.6 L9.6,8", "#8B5A2B", 2);
                    Fill("M7.2,1.6 L14.4,8.8 L11.8,11.4 L4.6,4.2 Z", PathColors["E"]);
                    break;
                case "S":
                    Fill(Star4, PathColors["S"]);
                    Fill("M8,5.5 L8.7,7.3 L10.5,8 L8.7,8.7 L8,10.5 L7.3,8.7 L5.5,8 L7.3,7.3 Z", "#FFFFFF");
                    break;
                case "D":
                    Fill(Skull, PathColors["D"]);
                    Fill(SkullHoles, Dark);
                    Stroke("M6.5,12.2 V14.4 M8,12.2 V14.4 M9.5,12.2 V14.4", Dark, 0.8);
                    break;
                case "N":
                    // a tree
                    Fill("M8,0.8 L12.6,6.2 H10.6 L14,10.6 H2 L5.4,6.2 H3.4 Z", PathColors["N"]);
                    Fill("M7,10.6 H9 V15.2 H7 Z", "#7C5A3C");
                    break;
                case "G":
                    // crystals
                    Fill("M6.6,15 L4.4,6 L7,1 L9.4,6 L8.6,15 Z", PathColors["G"]);
                    Fill("M9.6,15 L10.2,8 L12.6,4.6 L14,9 L11.6,15 Z M5,15 L2,9.4 L3.4,7.4 L5.8,12 Z", "#F0A8EA");
                    break;
                case "B":
                    // a bowl of blood
                    Fill("M1,4.6 C1,3.4 4.2,2.4 8,2.4 C11.8,2.4 15,3.4 15,4.6 C15,8.6 12,11 8,11 C4,11 1,8.6 1,4.6 Z", PathColors["B"]);
                    Fill("M2.6,4.6 C2.6,4 5,3.6 8,3.6 C11,3.6 13.4,4 13.4,4.6 C13.4,5.2 11,5.6 8,5.6 C5,5.6 2.6,5.2 2.6,4.6 Z", "#6E0B18");
                    Stroke("M12.6,7.4 V14.4", PathColors["B"], 1.2);
                    break;
                case "H":
                    // candles
                    Stroke("M3,9 V5.6 M6.2,9 V4.6 M9.8,9 V4.6 M13,9 V5.6 M2,9.4 H14 M8,9.4 V15 M5,15 H11", "#E5E7EB", 1.3);
                    Fill("M3,2.4 C3.6,3.4 3.8,4 3,4.8 C2.2,4 2.4,3.4 3,2.4 Z M6.2,1.4 C6.8,2.4 7,3 6.2,3.8 C5.4,3 5.6,2.4 6.2,1.4 Z M9.8,1.4 C10.4,2.4 10.6,3 9.8,3.8 C9,3 9.2,2.4 9.8,1.4 Z M13,2.4 C13.6,3.4 13.8,4 13,4.8 C12.2,4 12.4,3.4 13,2.4 Z", PathColors["H"]);
                    break;
                case "R":
                    Fill("M3.5,1.5 H12.5 C13.6,1.5 14.5,2.4 14.5,3.5 V12.5 C14.5,13.6 13.6,14.5 12.5,14.5 H3.5 C2.4,14.5 1.5,13.6 1.5,12.5 V3.5 C1.5,2.4 2.4,1.5 3.5,1.5 Z", PathColors["R"]);
                    Fill("M5,4 A1.2,1.2 0 1 1 5,6.4 A1.2,1.2 0 1 1 5,4 Z M11,9.6 A1.2,1.2 0 1 1 11,12 A1.2,1.2 0 1 1 11,9.6 Z M8,6.8 A1.2,1.2 0 1 1 8,9.2 A1.2,1.2 0 1 1 8,6.8 Z", Dark);
                    break;

                // ---- stats ----
                case "hp":
                    Fill("M8,14.6 C8,14.6 1.4,10.2 1.4,5.5 C1.4,3.3 3.1,1.8 5.1,1.8 C6.4,1.8 7.4,2.5 8,3.6 C8.6,2.5 9.6,1.8 10.9,1.8 C12.9,1.8 14.6,3.3 14.6,5.5 C14.6,10.2 8,14.6 8,14.6 Z", "#E5484D");
                    break;
                case "size":
                    Fill("M8,0.8 A2.1,2.1 0 1 1 8,5 A2.1,2.1 0 1 1 8,0.8 Z", "#A1A1AA");
                    Fill("M5,15.2 L5.4,7.6 C5.5,6.7 6.3,6 7.2,6 H8.8 C9.7,6 10.5,6.7 10.6,7.6 L11,15.2 Z", "#A1A1AA");
                    Stroke("M14,2 V14 M12.6,3.4 L14,2 L15.4,3.4 M12.6,12.6 L14,14 L15.4,12.6", "#71717A", 1);
                    break;
                case "prot":
                    Fill("M1.8,11.5 C1.8,5.4 4.6,1.8 8,1.8 C11.4,1.8 14.2,5.4 14.2,11.5 V14 H10.2 V9.4 H5.8 V14 H1.8 Z", "#A8B0BC");
                    Stroke("M8,2 V7.6", "#6B7280", 1);
                    break;
                case "mr":
                    Fill("M7,0.8 L8.3,5.7 L13.2,7 L8.3,8.3 L7,13.2 L5.7,8.3 L0.8,7 L5.7,5.7 Z", "#B07CF7");
                    Fill("M12.6,9.6 L13.2,11.8 L15.4,12.4 L13.2,13 L12.6,15.2 L12,13 L9.8,12.4 L12,11.8 Z", "#D8B4FE");
                    break;
                case "mor":
                    Stroke("M3,1 V15.2", "#D4D4D8", 1.4);
                    Fill("M3.6,1.8 H13.6 L11,5 L13.6,8.2 H3.6 Z", "#E4E4E7");
                    break;
                case "leader":
                    Fill("M1.5,11.6 L2.4,4.2 L5.5,7.8 L8,2.6 L10.5,7.8 L13.6,4.2 L14.5,11.6 Z", "#F2C94C");
                    Fill("M1.8,12.4 H14.2 V14.6 H1.8 Z", "#F2C94C");
                    break;
                case "str":
                    Fill("M0.8,6 H2.8 V10 H0.8 Z M3.4,4.2 H5.6 V11.8 H3.4 Z M10.4,4.2 H12.6 V11.8 H10.4 Z M13.2,6 H15.2 V10 H13.2 Z M5.6,7.2 H10.4 V8.8 H5.6 Z", "#F59E0B");
                    break;
                case "att":
                    Fill("M14.8,1.2 L14,4.2 L5.8,12.4 L3.6,10.2 L11.8,2 Z", "#D1D5DB");
                    Fill("M2.2,9.2 L6.8,13.8 L5.8,14.8 L1.2,10.2 Z", "#B7791F");
                    Stroke("M3.4,12.6 L1.2,14.8", "#B7791F", 1.6);
                    break;
                case "def":
                    Fill("M8,1 L14,3 V7.5 C14,11.2 11.4,13.9 8,15 C4.6,13.9 2,11.2 2,7.5 V3 Z", "#60A5FA");
                    Fill("M8,3 L12,4.4 V7.6 C12,10.2 10.3,12.2 8,13 Z", "#93C5FD");
                    break;
                case "prec":
                    Stroke("M8,1.6 A6.4,6.4 0 1 1 8,14.4 A6.4,6.4 0 1 1 8,1.6 Z M8,4.6 A3.4,3.4 0 1 1 8,11.4 A3.4,3.4 0 1 1 8,4.6 Z", "#F87171", 1.3);
                    Fill("M8,6.8 A1.2,1.2 0 1 1 8,9.2 A1.2,1.2 0 1 1 8,6.8 Z", "#F87171");
                    break;
                case "ap":
                    Fill("M9.6,0.8 L2.8,9 H7.4 L6.4,15.2 L13.2,7 H8.6 Z", "#FACC15");
                    break;
                case "mleader":
                    Stroke("M2.2,14.8 L9.6,7.4", "#A78BFA", 1.8);
                    Fill("M11.6,0.6 L12.6,3.6 L15.6,4.6 L12.6,5.6 L11.6,8.6 L10.6,5.6 L7.6,4.6 L10.6,3.6 Z", "#C4B5FD");
                    break;
                case "uleader":
                    Fill(Skull, "#9BC27A");
                    Fill(SkullHoles, Dark);
                    Stroke("M6.5,12.2 V14.4 M8,12.2 V14.4 M9.5,12.2 V14.4", Dark, 0.8);
                    break;
                case "mapmove":
                    Fill("M4,1.2 H9.2 V8.4 L14,10.4 C14.8,10.7 15.2,11.4 15.2,12.2 V14.6 H3 C3,14.6 4,10 4,8 Z", "#B08A64");
                    Fill("M3,13 H15.2 V14.6 H3 Z", "#7C5A3C");
                    break;
                case "enc":
                    Stroke("M5.2,6.2 C5.2,3.6 6.4,2 8,2 C9.6,2 10.8,3.6 10.8,6.2", "#9CA3AF", 1.6);
                    Fill("M3,8 C3,6.9 3.9,6 5,6 H11 C12.1,6 13,6.9 13,8 L14,13.6 C14.1,14.4 13.5,15 12.7,15 H3.3 C2.5,15 1.9,14.4 2,13.6 Z", "#9CA3AF");
                    break;
                case "age":
                    Fill("M3,1.2 H13 V3 C13,5.6 10,7 9,8 C10,9 13,10.4 13,13 V14.8 H3 V13 C3,10.4 6,9 7,8 C6,7 3,5.6 3,3 Z", "#D6B98C");
                    Fill("M5,13.4 C5.4,11.6 7.2,10.6 8,10.2 C8.8,10.6 10.6,11.6 11,13.4 Z M6,4 H10 C9.6,5 8.6,5.8 8,6.2 C7.4,5.8 6.4,5 6,4 Z", "#8D6E4A");
                    break;
                case "xp":
                    Fill("M8,1 L10.1,5.6 L15,6.1 L11.3,9.4 L12.4,14.3 L8,11.8 L3.6,14.3 L4.7,9.4 L1,6.1 L5.9,5.6 Z", "#FBBF24");
                    break;
                case "gold":
                    Fill("M8,1.4 A6.6,6.6 0 1 1 8,14.6 A6.6,6.6 0 1 1 8,1.4 Z", "#F2C94C");
                    Stroke("M8,3.8 A4.2,4.2 0 1 1 8,12.2 A4.2,4.2 0 1 1 8,3.8 Z", "#B7791F", 1);
                    break;
                case "res":
                    Stroke("M2.6,14.6 L9.4,7.8", "#B7791F", 1.8);
                    Fill("M7.4,2.4 L13.6,8.6 L11.4,10.8 L5.2,4.6 Z", "#A8B0BC");
                    break;
                case "rp":
                    Fill("M6,1.8 A2.2,2.2 0 1 1 6,6.2 A2.2,2.2 0 1 1 6,1.8 Z", "#A1A1AA");
                    Fill("M1.6,14.4 C1.6,10.6 3.6,7.6 6,7.6 C8.4,7.6 10.4,10.6 10.4,14.4 Z", "#A1A1AA");
                    Stroke("M12.8,5 V11 M9.8,8 H15.8", "#4ADE80", 1.6);
                    break;
                case "upkeep":
                    Fill("M8,1.4 A6.6,6.6 0 1 1 8,14.6 A6.6,6.6 0 1 1 8,1.4 Z", "#C9A23A");
                    Stroke("M4.6,8 H11.4", Dark, 1.6);
                    break;
                case "ressize":
                    Fill("M2,4.5 H14 V14.5 H2 Z", "#B08A64");
                    Stroke("M2,4.5 L5,1.5 H11 L14,4.5 M2,9.5 H14 M8,4.5 V14.5", "#7C5A3C", 1);
                    break;
                case "dmg":
                    Fill("M8,0.8 L9.6,5.2 L14.4,3.6 L11.2,7.6 L15.2,10.4 L10.4,10.6 L10.8,15.2 L8,11.4 L5.2,15.2 L5.6,10.6 L0.8,10.4 L4.8,7.6 L1.6,3.6 L6.4,5.2 Z", "#F97316");
                    break;
                case "len":
                    Stroke("M1.5,8 H14.5 M4,5.5 L1.5,8 L4,10.5 M12,5.5 L14.5,8 L12,10.5", "#A1A1AA", 1.4);
                    break;
                case "nratt":
                    Stroke("M3,13 L11,5 M6,13 L14,5", "#D1D5DB", 1.6);
                    break;
                default:
                    return null;
            }
            return g;
        }

        private static Brush Brush(string color)
        {
            var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            b.Freeze();
            return b;
        }
    }
}
