using System.IO;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Dom5Editor.Sprites
{
    /// <summary>
    /// The game's own icons (unit-window stats, magic paths and gems, gold and resources, ability
    /// icons): compiled into the editor (Resources/game-icons.pack, tools/gameart/trs.py pack; the
    /// developers allow it for game tools), else read from the user's Dominions 6 install.
    /// Which archive and index each key is comes from Data/game_icons.json (keys: "hp", "mr",
    /// "path:F", "gem:S", "gold", ability commands like "fireres", ...). <see cref="Icon"/> is null
    /// when the game isn't found or the key isn't mapped, so callers keep their own fallback.
    /// Vanilla units', items' and sites' pictures (<see cref="Sprite"/>, <see cref="SitePicture"/>)
    /// are only ever read from the install.
    /// </summary>
    public static class GameArt
    {
        /// <summary>The default Steam install folders, tried in order after the configured one.</summary>
        public static readonly IReadOnlyList<string> DefaultFolders = new[]
        {
            @"C:\Games\Steam\steamapps\common\Dominions6\data",
            @"C:\Program Files (x86)\Steam\steamapps\common\Dominions6\data",
        };

        private static readonly object _lock = new object();
        private static string? _configured;
        private static bool _resolved;
        private static string? _folder;
        private static Dictionary<string, (string Archive, int Index)>? _map;
        private static readonly Dictionary<string, TrsArchive?> _archives = new Dictionary<string, TrsArchive?>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, ImageSource?> _icons = new Dictionary<string, ImageSource?>();

        /// <summary>
        /// Uses this folder first: the game's data folder, or the game folder that contains it.
        /// Null or empty goes back to the default locations. Clears what was loaded.
        /// </summary>
        public static void Configure(string? folder)
        {
            lock (_lock)
            {
                _configured = string.IsNullOrWhiteSpace(folder) ? null : folder.Trim();
                _resolved = false;
                _folder = null;
                _archives.Clear();
                _icons.Clear();
            }
        }

        /// <summary>The game's data folder in use, or null if no install was found.</summary>
        public static string? DataFolder
        {
            get
            {
                lock (_lock)
                    return Resolve();
            }
        }

        /// <summary>True when the game's data folder was found.</summary>
        public static bool Available => DataFolder != null;

        /// <summary>The keys Data/game_icons.json maps.</summary>
        public static IReadOnlyCollection<string> Keys
        {
            get
            {
                lock (_lock)
                    return Map().Keys;
            }
        }

        /// <summary>The game's icon for a key, frozen; null if the game or the icon isn't there.</summary>
        public static ImageSource? Icon(string? key)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            lock (_lock)
            {
                if (_icons.TryGetValue(key, out var cached))
                    return cached;
                ImageSource? image = Packed(key);
                if (image == null && Map().TryGetValue(key, out var where))
                {
                    var archive = Archive(where.Archive);
                    image = archive?.Image(where.Index);
                }
                _icons[key] = image;
                return image;
            }
        }

        /// <summary>
        /// A unit's or item's picture from the install, by the sprite number the game stores
        /// (Dom5Edit.GameSprite: monster.trs for monsters, item.trs for items) and a frame (1: the
        /// attack frame, the next image). Null when the game or the image isn't there. Decoded on
        /// first use and cached; nothing of it is shipped with the editor.
        /// </summary>
        public static BitmapSource? Sprite(string archive, int number, int frame = 0)
        {
            TrsArchive? a;
            lock (_lock)
                a = Archive(archive);
            if (a == null)
                return null;
            int index = a.SpriteIndex(number);
            return index < 0 ? null : a.Image(index + frame);
        }

        /// <summary>
        /// A site's picture from the install, as the game picks it (6.37, tools/dom6exe/sprites.py):
        /// sites.trs group path + 1 (path 0-9: fire, air, water, earth, astral, death, nature,
        /// glamour, blood, holy), image <paramref name="look"/> when it is 0-99, else image
        /// <paramref name="level"/> (0-3). A vanilla site without #look has look -1; a new site 0.
        /// </summary>
        public static BitmapSource? SitePicture(int path, int level, int look)
        {
            TrsArchive? a;
            lock (_lock)
                a = Archive("sites.trs");
            if (a == null)
                return null;
            int start = a.GroupStart(Math.Clamp(path, 0, 9) + 1);
            if (start < 0)
                return null;
            return a.Image(start + (look >= 0 && look < 100 ? look : Math.Clamp(level, 0, 3)));
        }

        // the icons compiled into the editor (tools/gameart/trs.py pack): key -> (width, height, half size, zlib BGRA)
        private static Dictionary<string, (int Width, int Height, bool Half, byte[] Data)>? _pack;

        /// <summary>How many icons are compiled into the editor.</summary>
        public static int PackedCount
        {
            get
            {
                lock (_lock)
                    return (_pack ??= ReadPack()).Count;
            }
        }

        /// <summary>The icon from the pack compiled into the editor, or null (not in it, or no pack).</summary>
        private static ImageSource? Packed(string key)
        {
            _pack ??= ReadPack();
            if (!_pack.TryGetValue(key, out var icon))
                return null;
            try
            {
                using var z = new System.IO.Compression.ZLibStream(new MemoryStream(icon.Data), System.IO.Compression.CompressionMode.Decompress);
                var pixels = new byte[icon.Width * icon.Height * 4];
                int read = 0, n;
                while (read < pixels.Length && (n = z.Read(pixels, read, pixels.Length - read)) > 0)
                    read += n;
                double dpi = icon.Half ? 192 : 96;
                var image = System.Windows.Media.Imaging.BitmapSource.Create(icon.Width, icon.Height, dpi, dpi, PixelFormats.Bgra32, null, pixels, icon.Width * 4);
                image.Freeze();
                return image;
            }
            catch (Exception)
            {
                return null; // a broken entry: the install's, or none
            }
        }

        private static Dictionary<string, (int, int, bool, byte[])> ReadPack()
        {
            var pack = new Dictionary<string, (int, int, bool, byte[])>();
            try
            {
                using var stream = typeof(GameArt).Assembly.GetManifestResourceStream("game-icons.pack");
                if (stream == null)
                    return pack;
                using var r = new BinaryReader(stream);
                if (new string(r.ReadChars(4)) != "D6IP" || r.ReadUInt16() != 1)
                    return pack;
                int count = r.ReadUInt16();
                for (int i = 0; i < count; i++)
                {
                    var key = System.Text.Encoding.UTF8.GetString(r.ReadBytes(r.ReadByte()));
                    int w = r.ReadUInt16(), h = r.ReadUInt16();
                    bool half = (r.ReadByte() & 1) != 0;
                    pack[key] = (w, h, half, r.ReadBytes(r.ReadInt32()));
                }
            }
            catch (Exception)
            {
                // a broken pack: the install's icons, or none
            }
            return pack;
        }

        private static string? Resolve()
        {
            if (_resolved)
                return _folder;
            _resolved = true;
            var candidates = new List<string>();
            if (_configured != null)
            {
                candidates.Add(_configured);
                candidates.Add(Path.Combine(_configured, "data"));
            }
            candidates.AddRange(DefaultFolders);
            // where the game's exe is (the one the vanilla events' messages are read from)
            if (Dom5Edit.Events.GameInstall.Exe() is string exe && Path.GetDirectoryName(exe) is string game)
                candidates.Add(Path.Combine(game, "data"));
            foreach (var c in candidates)
            {
                try
                {
                    if (File.Exists(Path.Combine(c, "res.trs")) || File.Exists(Path.Combine(c, "misc.trs")))
                        return _folder = c;
                }
                catch (Exception)
                {
                    // an unreadable path: try the next
                }
            }
            return _folder = null;
        }

        private static TrsArchive? Archive(string name)
        {
            if (_archives.TryGetValue(name, out var cached))
                return cached;
            TrsArchive? archive = null;
            var folder = Resolve();
            if (folder != null)
            {
                try
                {
                    var path = Path.Combine(folder, name);
                    if (File.Exists(path))
                        archive = TrsArchive.Open(path);
                }
                catch (Exception)
                {
                    archive = null; // unreadable or not a .trs file: no icons from it
                }
            }
            _archives[name] = archive;
            return archive;
        }

        private static Dictionary<string, (string Archive, int Index)> Map()
        {
            if (_map != null)
                return _map;
            var map = new Dictionary<string, (string, int)>();
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "Data", "game_icons.json");
                if (File.Exists(path))
                {
                    using var doc = JsonDocument.Parse(File.ReadAllText(path));
                    foreach (var icon in doc.RootElement.GetProperty("icons").EnumerateObject())
                    {
                        var archive = icon.Value.GetProperty("archive").GetString();
                        if (!string.IsNullOrEmpty(archive))
                            map[icon.Name] = (archive, icon.Value.GetProperty("index").GetInt32());
                    }
                }
            }
            catch (Exception)
            {
                // a broken mapping file: no game icons
            }
            return _map = map;
        }
    }
}
