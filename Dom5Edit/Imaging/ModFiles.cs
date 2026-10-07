namespace Dom5Edit.Imaging
{
    /// <summary>
    /// A file a mod names (a sprite, a flag, its banner icon), found from the mod's own file on any
    /// system: relative to the .dm's folder, with either kind of slash ("./Mod/unit.tga",
    /// ".\Mod\unit.tga"); off Windows, where file names are case-sensitive, a name written in
    /// another case is found too.
    /// </summary>
    public static class ModFiles
    {
        /// <summary>The full path of a file a mod names, or null when it can't be found (or there's no mod file to start from).</summary>
        public static string? Resolve(string? named, string? modFile)
        {
            if (string.IsNullOrWhiteSpace(named))
                return null;
            var trimmed = named.Trim();
            string path;
            if (Path.IsPathRooted(trimmed))
                path = trimmed;
            else
            {
                if (string.IsNullOrEmpty(modFile))
                    return null;
                var relative = trimmed.Replace('\\', '/').TrimStart('.').TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                path = Path.Combine(Path.GetDirectoryName(modFile) ?? "", relative);
            }
            if (File.Exists(path) || OperatingSystem.IsWindows())
                return path;
            return FindIgnoringCase(path) ?? path;
        }

        /// <summary>The path with each part matched to what's on disk ignoring case, or null.</summary>
        private static string? FindIgnoringCase(string path)
        {
            var root = Path.GetPathRoot(path);
            if (string.IsNullOrEmpty(root))
                return null;
            var current = root;
            foreach (var part in path.Substring(root.Length).Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
            {
                var exact = Path.Combine(current, part);
                if (Directory.Exists(exact) || File.Exists(exact))
                {
                    current = exact;
                    continue;
                }
                if (!Directory.Exists(current))
                    return null;
                var match = Directory.EnumerateFileSystemEntries(current)
                    .FirstOrDefault(e => string.Equals(Path.GetFileName(e), part, StringComparison.OrdinalIgnoreCase));
                if (match == null)
                    return null;
                current = match;
            }
            return File.Exists(current) ? current : null;
        }
    }
}
