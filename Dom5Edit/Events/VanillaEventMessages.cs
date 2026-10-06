using System.Text;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Edit.Events
{
    /// <summary>
    /// Where the player's Dominions 6 is installed, for what the editor reads from the game itself
    /// (the vanilla events' messages). A configured folder first, else the usual Steam places.
    /// </summary>
    public static class GameInstall
    {
        /// <summary>The game's folder (the one with Dominions6.exe), when the user set it.</summary>
        public static string? Folder { get; set; }

        private static readonly string[] Usual =
        {
            @"C:\Games\Steam\steamapps\common\Dominions6",
            @"C:\Program Files (x86)\Steam\steamapps\common\Dominions6",
            @"C:\Program Files\Steam\steamapps\common\Dominions6",
            @"D:\SteamLibrary\steamapps\common\Dominions6",
            @"D:\Steam\steamapps\common\Dominions6",
        };

        /// <summary>The game's exe, or null if it isn't found.</summary>
        public static string? Exe()
        {
            var env = Environment.GetEnvironmentVariable("DOM6_EXE");
            if (!string.IsNullOrEmpty(env) && File.Exists(env))
                return env;
            foreach (var folder in new[] { Folder }.Concat(Usual))
            {
                if (string.IsNullOrEmpty(folder))
                    continue;
                var path = Path.Combine(folder, "Dominions6.exe");
                if (File.Exists(path))
                    return path;
            }
            return null;
        }
    }

    /// <summary>
    /// The vanilla events' messages, read from the player's own Dominions6.exe: the events file
    /// (tools/dom6exe events) leaves them out (they're the game's text) and says where they are
    /// ("-- messages: exe SHA offset O record R size S count N"). The table is used only if its end
    /// record reads "end" where the header says, so another game version reads nothing.
    /// </summary>
    public static class VanillaEventMessages
    {
        /// <summary>
        /// Adds each vanilla event's message to it as a display asset (shown, never saved). Returns
        /// how many were read, or why none were.
        /// </summary>
        public static string Load(Mod vanilla, string eventsFile)
        {
            string? header = File.ReadLines(eventsFile).Take(20).FirstOrDefault(l => l.StartsWith("-- messages:"));
            if (header == null)
                return "no messages line in " + Path.GetFileName(eventsFile);
            var parts = header.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            long Field(string name) => long.Parse(parts[Array.IndexOf(parts, name) + 1]);
            long offset = Field("offset"), record = Field("record"), size = Field("size"), count = Field("count");
            var exe = GameInstall.Exe();
            if (exe == null)
                return "Dominions6.exe not found: event messages not shown";
            using var stream = new FileStream(exe, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            byte[] Read(long at, int length)
            {
                var buffer = new byte[length];
                stream.Position = at;
                int read = 0;
                while (read < length)
                {
                    int n = stream.Read(buffer, read, length - read);
                    if (n <= 0)
                        break;
                    read += n;
                }
                return buffer;
            }
            if (offset + (count + 1) * record > stream.Length || Encoding.ASCII.GetString(Read(offset + count * record, 4)) != "end\0")
                return "Dominions6.exe isn't the version the events were read from: event messages not shown";
            int loaded = 0;
            if (!vanilla.Database.TryGetValue(EntityType.EVENT, out var events))
                return "no vanilla events";
            foreach (var e in events.GetFullList())
            {
                if (e.ID < 0 || e.ID >= count || e.Properties.Any(p => p.Command == Command.MSG))
                    continue;
                var bytes = Read(offset + e.ID * record, (int)size);
                int end = Array.IndexOf(bytes, (byte)0);
                var text = Encoding.UTF8.GetString(bytes, 0, end < 0 ? bytes.Length : end);
                if (StringProperty.Create() is StringProperty msg)
                {
                    msg.Parse(Command.MSG, text, "");
                    msg.IsDisplayAsset = true; // the game's text, for showing: never written into a mod
                    e.AddProperty(msg);
                    loaded++;
                }
            }
            return $"{loaded} event messages read from {exe}";
        }
    }
}
