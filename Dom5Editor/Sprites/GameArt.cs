using System.IO;
using System.Text.Json;
using System.Windows.Media;

namespace Dom5Editor.Sprites
{
    /// <summary>
    /// The game's own icons (unit-window stats, magic paths and gems, gold and resources, ability
    /// icons), read at run time from the user's Dominions 6 install: the art isn't ours to ship.
    /// Which archive and index each key is comes from Data/game_icons.json (keys: "hp", "mr",
    /// "path:F", "gem:S", "gold", ability commands like "fireres", ...). <see cref="Icon"/> is null
    /// when the game isn't found or the key isn't mapped, so callers keep their own fallback.
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
                ImageSource? image = null;
                if (Map().TryGetValue(key, out var where))
                {
                    var archive = Archive(where.Archive);
                    image = archive?.Image(where.Index);
                }
                _icons[key] = image;
                return image;
            }
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
