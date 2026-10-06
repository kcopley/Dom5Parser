using System.IO;
using System.Text.Json;

namespace Dom5Editor.Session
{
    /// <summary>
    /// What the editor remembers between runs (%APPDATA%\Dom5Editor\settings.json): the window's
    /// place and size, the last tab, and recently opened mods. Missing or unreadable settings
    /// just mean the defaults.
    /// </summary>
    public sealed class Settings
    {
        public double? Left { get; set; }
        public double? Top { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public bool Maximized { get; set; }
        public string? LastTab { get; set; }
        public List<string> RecentFiles { get; set; } = new List<string>();

        /// <summary>The Dominions 6 folder the user picked (when the editor doesn't find it in Steam's).</summary>
        public string? GameFolder { get; set; }

        public static string Folder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dom5Editor");
        private static string FilePath => Path.Combine(Folder, "settings.json");

        public static Settings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                    return JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath)) ?? new Settings();
            }
            catch (Exception)
            {
                // unreadable: start over
            }
            return new Settings();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Folder);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception)
            {
                // not being able to remember isn't worth an error
            }
        }

        /// <summary>Puts a file first in the recent list (eight kept).</summary>
        public void AddRecent(string path)
        {
            RecentFiles.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
            RecentFiles.Insert(0, path);
            if (RecentFiles.Count > 8)
                RecentFiles.RemoveRange(8, RecentFiles.Count - 8);
        }
    }
}
