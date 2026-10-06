using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Events;
using Dom5Edit.Props;

namespace Dom5Edit.GameData
{
    /// <summary>
    /// The game's own texts for vanilla entities, read from the player's Dominions6.exe like the
    /// event messages (VanillaEventMessages): monster, item and spell descriptions, a spell's
    /// details, portent and cure, a nation's description, summary and brief. The game keeps them
    /// in two lists of strings, a key (":mon20", ":Heavy Cavalry", ":era1 Abysia") before each
    /// text. tools/dom6exe texts wrote where the lists are and how each kind's key is made
    /// (game-texts.json, built in), never the texts. The bytes are used only if their checksum
    /// matches, so another game version reads nothing.
    /// </summary>
    public static class VanillaTexts
    {
        private const string ResourceName = "Dom5Edit.GameData.game-texts.json";

        /// <summary>One of the game's lists: its entries, and where each key's text is.</summary>
        private sealed class TextList
        {
            public readonly List<string> Entries = new List<string>();
            private readonly Dictionary<string, int> _keys = new Dictionary<string, int>();
            private readonly Dictionary<string, int> _modKeys = new Dictionary<string, int>();

            public void Index()
            {
                // the last of equal keys wins (the game searches from the end)
                for (int i = 0; i < Entries.Count; i++)
                {
                    var e = Entries[i];
                    if (e.StartsWith("::"))
                        _modKeys[Fold(e.Substring(2))] = i;
                    else if (e.StartsWith(":"))
                        _keys[Fold(e.Substring(1))] = i;
                }
            }

            /// <summary>The text a key finds: the first entry after the key's run of keys (none if empty).</summary>
            public string? Find(string key)
            {
                var k = Fold(key);
                if (!_modKeys.TryGetValue(k, out int i) && !_keys.TryGetValue(k, out i))
                    return null;
                while (i < Entries.Count && Entries[i].StartsWith(":"))
                    i++;
                return i < Entries.Count && Entries[i].Length > 0 ? Entries[i] : null;
            }

            /// <summary>The game compares keys ignoring case, A-Z only.</summary>
            private static string Fold(string s) => string.Create(s.Length, s, (span, src) =>
            {
                for (int i = 0; i < src.Length; i++)
                    span[i] = src[i] >= 'A' && src[i] <= 'Z' ? (char)(src[i] + 32) : src[i];
            });
        }

        /// <summary>
        /// Adds each text to its vanilla entity as a display asset (shown, never saved), unless the
        /// entity has that line already. Returns how many were added; <paramref name="status"/>
        /// says what was read, or why nothing was.
        /// </summary>
        public static int Load(Mod vanilla, out string status)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            using var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName);
            if (resource == null)
            {
                status = "no text locations built in: the game's descriptions not shown";
                return 0;
            }
            using var doc = JsonDocument.Parse(resource);
            var root = doc.RootElement;
            var exe = GameInstall.Exe();
            if (exe == null)
            {
                status = "Dominions6.exe not found: the game's descriptions not shown";
                return 0;
            }
            var wrongVersion = $"Dominions6.exe isn't version {root.GetProperty("game_version").GetString()}, where the texts were located: the game's descriptions not shown";

            // the span holding both lists and every string they point to, checked against its checksum
            var read = root.GetProperty("read");
            long start = read.GetProperty("file_offset").GetInt64();
            int length = read.GetProperty("length").GetInt32();
            var bytes = new byte[length];
            try
            {
                using var file = new FileStream(exe, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                if (start + length > file.Length)
                {
                    status = wrongVersion;
                    return 0;
                }
                file.Position = start;
                file.ReadExactly(bytes);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                status = $"{exe} couldn't be read ({ex.Message}): the game's descriptions not shown";
                return 0;
            }
            if (!Convert.ToHexString(SHA256.HashData(bytes)).Equals(read.GetProperty("sha256").GetString(), StringComparison.OrdinalIgnoreCase))
            {
                status = wrongVersion;
                return 0;
            }

            // a pointer is an address in .data: in the file's bytes, or in the zero-filled rest (an empty string)
            var data = root.GetProperty("data_section");
            long dataAddress = Hex(data.GetProperty("address").GetString()!);
            long dataOffset = data.GetProperty("file_offset").GetInt64();
            long fileSize = data.GetProperty("file_size").GetInt64(), memorySize = data.GetProperty("memory_size").GetInt64();
            string? StringAt(long address)
            {
                if (address < dataAddress || address >= dataAddress + memorySize)
                    return null;
                if (address >= dataAddress + fileSize)
                    return "";
                long at = address - dataAddress + dataOffset - start;
                if (at < 0 || at >= length)
                    return null;
                int n = bytes.AsSpan((int)at).IndexOf((byte)0);
                return n < 0 ? null : Encoding.UTF8.GetString(bytes, (int)at, n);
            }
            var lists = new Dictionary<string, TextList>();
            foreach (var l in root.GetProperty("lists").EnumerateObject())
            {
                long at = l.Value.GetProperty("file_offset").GetInt64() - start;
                int count = l.Value.GetProperty("count").GetInt32();
                var endKey = l.Value.GetProperty("end").GetString();
                var list = new TextList();
                for (int i = 0; i <= count; i++)
                {
                    var s = at + 8 * i + 8 <= length ? StringAt(BitConverter.ToInt64(bytes, (int)(at + 8 * i))) : null;
                    if (s == null || (i == count) != (s == endKey))
                    {
                        status = wrongVersion;
                        return 0;
                    }
                    if (i < count)
                        list.Entries.Add(s);
                }
                list.Index();
                lists[l.Name] = list;
            }

            // each kind's keys, tried in order ("mon{id}", then "{name}")
            int added = 0;
            var counts = new List<string>();
            foreach (var kind in root.GetProperty("texts").EnumerateObject())
            {
                var entities = vanilla.Database.FirstOrDefault(t => GameCommandCatalog.ContextOf(t.Key) == kind.Name).Value;
                if (entities == null)
                    continue;
                foreach (var text in kind.Value.EnumerateObject())
                {
                    if (!CommandsMap.TryGetCommand("#" + text.Name, out var command))
                        continue;
                    var list = lists[text.Value.GetProperty("list").GetString()!];
                    var keys = text.Value.GetProperty("keys").EnumerateArray().Select(k => k.GetString()!).ToList();
                    int n = 0;
                    foreach (var e in entities.GetFullList())
                    {
                        if (e.Properties.Any(p => p.Command == command))
                            continue;
                        var found = keys.Select(k => KeyOf(k, e)).Where(k => k != null).Select(k => list.Find(k!)).FirstOrDefault(t => t != null);
                        if (found != null && StringProperty.Create() is StringProperty prop)
                        {
                            prop.Parse(command, found, "");
                            prop.IsDisplayAsset = true; // the game's text, for showing: never written into a mod
                            e.AddProperty(prop);
                            n++;
                        }
                    }
                    added += n;
                    counts.Add($"{n} {kind.Name} {text.Name}");
                }
            }
            status = $"{added} texts read from {exe} in {watch.ElapsedMilliseconds} ms ({string.Join(", ", counts)})";
            return added;
        }

        /// <summary>A key for an entity: {id} its number, {name} its name, {era} a nation's era; null if it lacks one.</summary>
        private static string? KeyOf(string template, IDEntity e)
        {
            var key = template.Replace("{id}", e.ID.ToString());
            if (key.Contains("{name}"))
            {
                if (!e.TryGetName(out var name) || string.IsNullOrEmpty(name))
                    return null;
                key = key.Replace("{name}", name);
            }
            if (key.Contains("{era}"))
            {
                if (e.TryGet<IntProperty>(Command.ERA, out var era, checkCopy: false) == ReturnType.FALSE)
                    return null;
                key = key.Replace("{era}", era.Value.ToString());
            }
            return key;
        }

        private static long Hex(string s) => Convert.ToInt64(s.StartsWith("0x") ? s.Substring(2) : s, 16);
    }
}
