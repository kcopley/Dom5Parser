using System.IO;
using System.Text.Json;

namespace Dom5Editor.Session
{
    /// <summary>
    /// The mods each mod needs (a submod's parent), as the user set them, in the order the game
    /// reads them (the order they were enabled: tools/dom6exe/README.md, "Several mods"):
    /// remembered between runs in needed-mods.json next to the settings. The editor reads them
    /// under the mod; they are never saved, only the mod itself.
    /// </summary>
    public static class NeededMods
    {
        /// <summary>Where they're kept (the snapshot harness points it at a temp folder).</summary>
        public static string FilePath { get; set; } = Path.Combine(Settings.Folder, "needed-mods.json");

        // (paths compare without case on Windows, where the file system ignores it)
        private static StringComparer PathComparer => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

        private static Dictionary<string, List<string>> Read()
        {
            try
            {
                if (File.Exists(FilePath) && JsonSerializer.Deserialize<Dictionary<string, List<string>>>(File.ReadAllText(FilePath)) is { } all)
                    return new Dictionary<string, List<string>>(all, PathComparer);
            }
            catch (Exception)
            {
                // unreadable: nothing remembered
            }
            return new Dictionary<string, List<string>>(PathComparer);
        }

        /// <summary>The mods remembered for a mod file, in reading order (none: an empty list).</summary>
        public static IReadOnlyList<string> For(string modPath) =>
            Read().TryGetValue(Path.GetFullPath(modPath), out var needed) ? needed : Array.Empty<string>();

        /// <summary>Remembers the mods a mod file needs (none: forgets it).</summary>
        public static void Set(string modPath, IReadOnlyList<string> needed)
        {
            var all = Read();
            if (needed.Count == 0)
                all.Remove(Path.GetFullPath(modPath));
            else
                all[Path.GetFullPath(modPath)] = needed.Select(Path.GetFullPath).ToList();
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(all, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception)
            {
                // not being able to remember isn't worth an error
            }
        }
    }
}
