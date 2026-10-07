using System.IO;
using System.Text.Json;
using Dom5Editor.Imaging;

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
    /// and nations' flags (<see cref="NationFlag"/>, built from parts as the game does) are only
    /// ever read from the install.
    /// </summary>
    public static class GameArt
    {
        private static readonly object _lock = new object();
        private static string? _configured;
        private static bool _resolved;
        private static string? _folder;
        private static Dictionary<string, (string Archive, int Index)>? _map;
        private static readonly Dictionary<string, TrsArchive?> _archives = new Dictionary<string, TrsArchive?>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Picture?> _icons = new Dictionary<string, Picture?>();

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
                _flags.Clear();
                _flagParts = default;
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
        public static Picture? Icon(string? key)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            lock (_lock)
            {
                if (_icons.TryGetValue(key, out var cached))
                    return cached;
                Picture? image = Packed(key);
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
        public static Picture? Sprite(string archive, int number, int frame = 0)
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
        public static Picture? SitePicture(int path, int level, int look)
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

        // nations' flags built so far, by nation and tint colors, kept while something shows them
        private static readonly Dictionary<(int Nation, int Color, int Secondary), WeakReference<Picture>?> _flags = new Dictionary<(int, int, int), WeakReference<Picture>?>();
        // the parts every built flag has (pole, cloth, border) on the flag's canvas, from this archive
        private static (TrsArchive? Archive, byte[]?[] Parts) _flagParts;
        private const int FlagSize = 128;

        /// <summary>
        /// A nation's flag from the install, as the game makes it (6.37, tools/dom6exe/flags.py; the
        /// numbers are Dom5Edit.VanillaSprites.Flag): flag.trs image nation + 1. Nations 0-4
        /// (Independents, the special monster slots) have theirs in the file. Nations 5-499 get one
        /// built at start and after the mods are read: the pole, the cloth times
        /// <paramref name="color"/> (#color), its border times <paramref name="secondary"/>
        /// (#secondarycolor; unset it is black, the flag doesn't fall back to #color) and, up to
        /// nation 135, the nation's emblem, each blended over the last by its alpha and kept as RGB565
        /// (a black pixel turns transparent: a nation without colors shows a bare pole). Built when
        /// first asked for and cached per nation and colors; null when the game, the rule (no
        /// vanilla-sprites.json) or the images aren't there.
        /// </summary>
        public static Picture? NationFlag(int nation, (float R, float G, float B) color, (float R, float G, float B) secondary)
        {
            var rule = Dom5Edit.VanillaSprites.Flag;
            if (rule == null || nation < 0 || nation > rule.LastComposed)
                return null;
            lock (_lock)
            {
                var a = Archive(rule.Archive);
                if (a == null)
                    return null;
                if (!rule.IsComposed(nation))
                    return a.Image(nation + rule.ImageOffset);
                var key = (nation, Tint(color), Tint(secondary));
                if (_flags.TryGetValue(key, out var cached))
                {
                    if (cached == null)
                        return null;
                    if (cached.TryGetTarget(out var alive))
                        return alive;
                }
                Picture? image;
                try
                {
                    image = BuildFlag(a, rule, nation, key.Item2, key.Item3);
                }
                catch (Exception)
                {
                    image = null; // a damaged archive: no flag
                }
                _flags[key] = image != null ? new WeakReference<Picture>(image) : null;
                return image;
            }
        }

        // the tint bytes of a color as 0xRRGGBB
        private static int Tint((float R, float G, float B) c) =>
            Dom5Edit.NationFlagRule.Tint(c.R) << 16 | Dom5Edit.NationFlagRule.Tint(c.G) << 8 | Dom5Edit.NationFlagRule.Tint(c.B);

        private static Picture? BuildFlag(TrsArchive a, Dom5Edit.NationFlagRule rule, int nation, int color, int secondary)
        {
            if (_flagParts.Archive != a)
                _flagParts = (a, new[] { Canvas(a, rule.Pole), Canvas(a, rule.Cloth), Canvas(a, rule.Border) });
            var parts = _flagParts.Parts;
            if (parts[0] == null || parts[1] == null || parts[2] == null)
                return null;
            var flag = (byte[])parts[0]!.Clone();
            Over(flag, Tinted(parts[1]!, color));
            Over(flag, Tinted(parts[2]!, secondary));
            if (Canvas(a, rule.Emblem(nation)) is byte[] emblem)
                Over(flag, emblem);
            AsStored(flag);
            return new Picture(FlagSize, FlagSize, flag);
        }

        /// <summary>An image on the flag's 128 x 128 canvas, at the top left (how the game loads a part), as straight BGRA; null if it isn't there.</summary>
        private static byte[]? Canvas(TrsArchive a, int index)
        {
            if (index < 0 || index >= a.Count || a.DecodeBgra(index) is not byte[] px)
                return null;
            var info = a.Info(index);
            if (info.Width == FlagSize && info.Height == FlagSize)
                return px;
            var canvas = new byte[FlagSize * FlagSize * 4];
            for (int y = 0; y < Math.Min(info.Height, FlagSize); y++)
                Buffer.BlockCopy(px, y * info.Width * 4, canvas, y * FlagSize * 4, Math.Min(info.Width, FlagSize) * 4);
            return canvas;
        }

        /// <summary>A copy with each channel of the pixels shown times its tint byte / 255, truncated (the tint is 0xRRGGBB).</summary>
        private static byte[] Tinted(byte[] part, int rgb)
        {
            var px = (byte[])part.Clone();
            int r = rgb >> 16 & 0xFF, g = rgb >> 8 & 0xFF, b = rgb & 0xFF;
            for (int o = 0; o < px.Length; o += 4)
                if (px[o + 3] != 0)
                {
                    px[o] = (byte)(px[o] * b / 255);
                    px[o + 1] = (byte)(px[o + 1] * g / 255);
                    px[o + 2] = (byte)(px[o + 2] * r / 255);
                }
            return px;
        }

        /// <summary>Source over destination by the source's alpha, (s * a + d * (255 - a)) / 255 truncated; what it covers becomes opaque.</summary>
        private static void Over(byte[] dst, byte[] src)
        {
            for (int o = 0; o < dst.Length; o += 4)
            {
                int alpha = src[o + 3];
                if (alpha == 0)
                    continue;
                for (int c = 0; c < 3; c++)
                    dst[o + c] = (byte)((src[o + c] * alpha + dst[o + c] * (255 - alpha)) / 255);
                dst[o + 3] = 255;
            }
        }

        /// <summary>
        /// The flag as the game keeps it: RGB565 without alpha, then read back as it reads its
        /// images (0x0000 transparent, magenta 0xF81F a half-transparent black shadow).
        /// </summary>
        private static void AsStored(byte[] px)
        {
            for (int o = 0; o < px.Length; o += 4)
            {
                int b = px[o] & 0xF8, g = px[o + 1] & 0xFC, r = px[o + 2] & 0xF8;
                int v = px[o + 3] == 0 ? 0 : r << 8 | g << 3 | b >> 3;
                if (v == 0)
                    px[o] = px[o + 1] = px[o + 2] = px[o + 3] = 0;
                else if (v == 0xF81F)
                {
                    px[o] = px[o + 1] = px[o + 2] = 0;
                    px[o + 3] = 0x80;
                }
                else
                {
                    px[o] = (byte)b;
                    px[o + 1] = (byte)g;
                    px[o + 2] = (byte)r;
                    px[o + 3] = 255;
                }
            }
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
        private static Picture? Packed(string key)
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
                return new Picture(icon.Width, icon.Height, pixels, dpi);
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
            // the game's folder on this system (Steam's usual places and libraries; GameInstall)
            if (Dom5Edit.Events.GameInstall.GameFolder() is string game)
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
