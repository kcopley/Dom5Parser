using System.Text;
using System.Text.RegularExpressions;
using Dom5Edit.Entities;

namespace Dom5Edit
{
    /// <summary>
    /// Finds the mod a submod most likely needs: of the .dm files next to it (then in the folders
    /// next to its folder), the one whose #new/#select lines number most of what the mod refers to
    /// but nothing loaded defines. A quick text scan: nothing is parsed.
    /// </summary>
    public static class NeededModFinder
    {
        private static readonly Regex Defines = new Regex(@"#(?:new|select)([a-z]+)[ \t]+(\d+)", RegexOptions.Compiled);

        /// <summary>The most likely file and how many of the missing numbers it has, or null if none has half of them.</summary>
        public static (string File, int Found)? Suggest(string modFile, IReadOnlyCollection<(EntityType Type, int Id)> missing, IEnumerable<string>? skip = null)
        {
            if (missing.Count == 0 || string.IsNullOrEmpty(modFile))
                return null;
            var dir = Path.GetDirectoryName(Path.GetFullPath(modFile));
            if (dir == null)
                return null;
            var exclude = new HashSet<string>((skip ?? Array.Empty<string>()).Append(modFile).Select(Path.GetFullPath), StringComparer.OrdinalIgnoreCase);
            // next to it first (a submod beside its parent), then the other mods' folders
            var near = Best(Files(dir, false), missing, exclude);
            if (near != null)
                return near;
            var parent = Path.GetDirectoryName(dir);
            return parent == null ? null : Best(Files(parent, true).Where(f => !string.Equals(Path.GetDirectoryName(f), dir, StringComparison.OrdinalIgnoreCase)), missing, exclude);
        }

        private static IEnumerable<string> Files(string dir, bool andSubfolders)
        {
            try
            {
                var files = Directory.EnumerateFiles(dir, "*.dm");
                if (andSubfolders)
                    files = files.Concat(Directory.EnumerateDirectories(dir).SelectMany(d => Directory.EnumerateFiles(d, "*.dm")));
                return files.Take(200).ToList();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                return Array.Empty<string>();
            }
        }

        private static (string File, int Found)? Best(IEnumerable<string> files, IReadOnlyCollection<(EntityType Type, int Id)> missing, HashSet<string> exclude)
        {
            (string File, int Found)? best = null;
            foreach (var file in files)
            {
                if (exclude.Contains(Path.GetFullPath(file)))
                    continue;
                int found = Count(file, missing);
                // ties: the shorter name (Sombre_Warhammer_dom6.dm over its archived Sombre_Warhammer_static_1.611.dm)
                if (found * 2 >= missing.Count && found > 0
                    && (best == null || found > best.Value.Found || found == best.Value.Found && Path.GetFileName(file).Length < Path.GetFileName(best.Value.File).Length))
                    best = (file, found);
            }
            return best;
        }

        /// <summary>How many of the numbers the file's #new/#select lines give.</summary>
        private static int Count(string file, IReadOnlyCollection<(EntityType Type, int Id)> missing)
        {
            string text;
            try
            {
                if (new FileInfo(file).Length > 64 << 20)
                    return 0;
                text = File.ReadAllText(file, Encoding.Latin1);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                return 0;
            }
            var defined = new HashSet<(EntityType, int)>();
            foreach (Match m in Defines.Matches(text))
                if (Enum.TryParse<EntityType>(m.Groups[1].Value, true, out var type) && int.TryParse(m.Groups[2].Value, out var id))
                    defined.Add((type, id));
            return missing.Count(defined.Contains);
        }
    }
}
