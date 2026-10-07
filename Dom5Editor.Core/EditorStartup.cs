using Dom5Edit;

namespace Dom5Editor
{
    /// <summary>
    /// What any editor does before its window opens (the Windows one and the Mac/Linux one): Dom6
    /// rules, the player's game folder if they picked one, where vanilla.dm and the spell tables
    /// are (next to the editor, or up from a build folder), the supplied assets, then the game's
    /// data read. Returns null when the editor can start, else why it can't.
    /// </summary>
    public static class EditorStartup
    {
        public static string? Configure()
        {
            VanillaLoader.GameVersion = GameVersion.Dom6;

            // the game's own texts, sprites and event messages come from the player's install: a
            // folder they picked (the Load menu), else Steam's (Dom5Edit.Events.GameInstall)
            var gameFolder = Session.Settings.Load().GameFolder;
            if (!string.IsNullOrEmpty(gameFolder))
            {
                Dom5Edit.Events.GameInstall.Folder = gameFolder;
                Sprites.GameArt.Configure(gameFolder);
            }

            if (Find("vanilla.dm", Path.Combine("VanillaData", "vanilla.dm")) is string vanilla)
                VanillaLoader.VanillaDmPath = vanilla;
            if (Find("spell_effects_mapping.json") is string mapping)
                VanillaLoader.SpellEffectMappingPath = mapping;
            if (Find("spell_effect_types.json") is string types)
                VanillaLoader.SpellEffectTypesPath = types;
            if (FindAssets() is string assets)
                VanillaAssetLoader.AssetsBasePath = assets;

            if (string.IsNullOrEmpty(VanillaLoader.VanillaDmPath) || !File.Exists(VanillaLoader.VanillaDmPath))
                return "vanilla.dm wasn't found. It holds the game's own data, which the editor builds on.\n\n" +
                       $"Put vanilla.dm next to the editor ({AppContext.BaseDirectory}).";
            return null;
        }

        /// <summary>Reads the game's data now (else on first use); null if it was read, else what went wrong.</summary>
        public static string? LoadVanilla()
        {
            try
            {
                return VanillaLoader.Vanilla == null ? "vanilla.dm couldn't be read" : null;
            }
            catch (Exception ex)
            {
                return $"vanilla.dm couldn't be read: {ex.Message}";
            }
        }

        /// <summary>A file next to the editor, or up from its build folder (bin/Release/net8.0...), or in the working folder.</summary>
        private static string? Find(params string[] names)
        {
            var baseDir = AppContext.BaseDirectory;
            foreach (var name in names)
                foreach (var candidate in new[]
                         {
                             Path.Combine(baseDir, name),
                             Path.Combine(baseDir, "..", "..", "..", "..", name),
                             Path.Combine(baseDir, "..", "..", "..", name),
                             Path.Combine(baseDir, "..", "..", name),
                             Path.Combine(baseDir, "..", name),
                             Path.Combine(Directory.GetCurrentDirectory(), name),
                         })
                {
                    try
                    {
                        var full = Path.GetFullPath(candidate);
                        if (File.Exists(full))
                            return full;
                    }
                    catch (Exception)
                    {
                        // a path that can't be resolved: the next
                    }
                }
            return null;
        }

        /// <summary>The folder with the supplied assets (icons/sprites and Data/unitdescr), if there is one.</summary>
        private static string? FindAssets()
        {
            var baseDir = AppContext.BaseDirectory;
            foreach (var candidate in new[] { baseDir, Path.Combine(baseDir, ".."), Path.Combine(baseDir, "..", "..", ".."), Path.Combine(baseDir, "..", "..", "..", ".."), Directory.GetCurrentDirectory() })
            {
                try
                {
                    var full = Path.GetFullPath(candidate);
                    if (Directory.Exists(Path.Combine(full, "icons", "sprites")) && Directory.Exists(Path.Combine(full, "Data", "unitdescr")))
                        return full;
                }
                catch (Exception)
                {
                    // the next
                }
            }
            return null;
        }
    }
}
