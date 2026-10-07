using System.Security.Cryptography;
using System.Text;
using Dom5Edit.Entities;

namespace Dom5Edit
{
    /// <summary>
    /// Copies of a mod's file, so no edit or save can lose the author's work: the file as it was
    /// when it was opened, and before each save. Kept outside the game's mods folder (a .dm there
    /// would show up as another mod), per mod file, newest last; the same content isn't copied
    /// twice in a row, and the oldest go past <see cref="Keep"/>.
    /// </summary>
    public static class ModBackups
    {
        /// <summary>Where backups go (%APPDATA%\Dom5Editor\backups by default).</summary>
        public static string Folder { get; set; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dom5Editor", "backups");

        /// <summary>How many copies are kept per mod file.</summary>
        public static int Keep { get; set; } = 30;

        /// <summary>The folder for one mod file's backups: its name and a short hash of its full path (two mods may share a name).</summary>
        public static string FolderOf(string modFile)
        {
            var full = Path.GetFullPath(modFile);
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(full.ToLowerInvariant())))[..8];
            return Path.Combine(Folder, $"{Path.GetFileNameWithoutExtension(full)}-{hash}");
        }

        /// <summary>
        /// Copies the file ("2026-10-07_14-03-22_opened.dm"); nothing if it's missing or the newest
        /// copy has the same bytes. Returns the copy's path, or null.
        /// </summary>
        public static string? Backup(string modFile, string reason)
        {
            if (!File.Exists(modFile))
                return null;
            var dir = FolderOf(modFile);
            Directory.CreateDirectory(dir);
            var bytes = File.ReadAllBytes(modFile);
            var newest = Directory.GetFiles(dir, "*.dm").OrderBy(f => f, StringComparer.Ordinal).LastOrDefault();
            if (newest != null && File.ReadAllBytes(newest).AsSpan().SequenceEqual(bytes))
                return null;
            var copy = Path.Combine(dir, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}_{reason}.dm");
            File.WriteAllBytes(copy, bytes);
            foreach (var old in Directory.GetFiles(dir, "*.dm").OrderBy(f => f, StringComparer.Ordinal).SkipLast(Keep))
                File.Delete(old);
            return copy;
        }
    }

    /// <summary>
    /// Checks a saved file before it replaces the mod's file: it must read back (no error) with
    /// the same entities, type by type, as the mod being saved. A failed check keeps the old
    /// file (SafeFile) and says what differed.
    /// </summary>
    public static class SaveCheck
    {
        public static void Verify(Mod mod, string savedPath)
        {
            var back = new Mod { FullFilePath = savedPath };
            try
            {
                back.Parse(savedPath);
            }
            catch (Exception ex)
            {
                throw new IOException($"The saved file didn't read back ({ex.Message}).", ex);
            }
            var problems = new List<string>();
            foreach (EntityType type in Enum.GetValues(typeof(EntityType)))
            {
                int had = mod.Database.TryGetValue(type, out var a) ? a.GetFullList().Count : 0;
                int has = back.Database.TryGetValue(type, out var b) ? b.GetFullList().Count : 0;
                if (had != has)
                    problems.Add($"{type.ToString().ToLowerInvariant()}s: {had} in the editor, {has} in the saved file");
            }
            if (problems.Count > 0)
                throw new IOException("The saved file didn't read back the same: " + string.Join("; ", problems) + ".");
        }
    }
}
