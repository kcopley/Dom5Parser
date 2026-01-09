using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dom5Edit
{
    /// <summary>
    /// Loads sprite paths and descriptions from asset files into vanilla entities.
    /// This allows the existing property/layering system to handle display naturally.
    /// </summary>
    public static class VanillaAssetLoader
    {
        private static string _assetsBasePath;

        /// <summary>
        /// Gets or sets the base path for asset files (icons/, Data/).
        /// If null, will attempt to find assets relative to the application directory.
        /// </summary>
        public static string AssetsBasePath
        {
            get => _assetsBasePath;
            set => _assetsBasePath = value;
        }

        /// <summary>
        /// Loads sprite paths and descriptions into all vanilla entities in the mod.
        /// Call this after parsing vanilla.dm.
        /// </summary>
        /// <param name="mod">The mod containing vanilla entities</param>
        public static void LoadAssets(Mod mod)
        {
            string basePath = FindAssetsBasePath();
            if (string.IsNullOrEmpty(basePath))
            {
                System.Diagnostics.Debug.WriteLine("[VanillaAssetLoader] WARNING: Assets base path not found. Sprites and descriptions will not be loaded.");
                return;
            }

            LoadMonsterAssets(mod, basePath);
            LoadItemAssets(mod, basePath);
            LoadSpellAssets(mod, basePath);
            // Sites use #look which should already be in vanilla.dm - no separate loading needed
        }

        /// <summary>
        /// Finds the base path for asset files.
        /// </summary>
        private static string FindAssetsBasePath()
        {
            if (!string.IsNullOrEmpty(_assetsBasePath) && Directory.Exists(_assetsBasePath))
                return _assetsBasePath;

            // Search common locations
            var searchPaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icons"), // Check for icons folder
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".."),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", ".."),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."),
            };

            foreach (var path in searchPaths)
            {
                string fullPath = Path.GetFullPath(path);
                // Check if this directory contains the expected asset folders
                if (Directory.Exists(Path.Combine(fullPath, "icons", "sprites")) ||
                    Directory.Exists(Path.Combine(fullPath, "Data", "unitdescr")))
                {
                    return fullPath;
                }
            }

            return null;
        }

        /// <summary>
        /// Loads sprite paths and descriptions for all monsters.
        /// </summary>
        private static void LoadMonsterAssets(Mod mod, string basePath)
        {
            string spritesPath = Path.Combine(basePath, "icons", "sprites");
            string descrPath = Path.Combine(basePath, "Data", "unitdescr");

            if (!mod.Database.TryGetValue(EntityType.MONSTER, out var monsterSet))
                return;

            foreach (var entity in monsterSet.GetFullList())
            {
                if (entity is not Monster monster)
                    continue;

                int id = monster.ID;

                // Load sprite paths (only if entity doesn't already have them)
                if (!HasProperty(monster, Command.SPR1))
                {
                    string spr1File = Path.Combine(spritesPath, $"{id:D4}_1.png");
                    if (File.Exists(spr1File))
                    {
                        SetFilePathProperty(monster, Command.SPR1, spr1File);
                    }
                }

                if (!HasProperty(monster, Command.SPR2))
                {
                    string spr2File = Path.Combine(spritesPath, $"{id:D4}_2.png");
                    if (File.Exists(spr2File))
                    {
                        SetFilePathProperty(monster, Command.SPR2, spr2File);
                    }
                }

                // Load description (only if entity doesn't already have it)
                if (!HasProperty(monster, Command.DESCR))
                {
                    string descrFile = Path.Combine(descrPath, $"{id:D4}.txt");
                    if (File.Exists(descrFile))
                    {
                        string description = ReadDescriptionFile(descrFile);
                        if (!string.IsNullOrEmpty(description))
                        {
                            SetStringProperty(monster, Command.DESCR, description);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads sprite paths and descriptions for all items.
        /// </summary>
        private static void LoadItemAssets(Mod mod, string basePath)
        {
            string spritesPath = Path.Combine(basePath, "icons", "items");
            string descrPath = Path.Combine(basePath, "Data", "itemdescr");

            if (!mod.Database.TryGetValue(EntityType.ITEM, out var itemSet))
                return;

            foreach (var entity in itemSet.GetFullList())
            {
                if (entity is not Item item)
                    continue;

                int id = item.ID;

                // Load sprite path (only if entity doesn't already have it)
                if (!HasProperty(item, Command.SPR))
                {
                    string sprFile = Path.Combine(spritesPath, $"item{id}.png");
                    if (File.Exists(sprFile))
                    {
                        SetFilePathProperty(item, Command.SPR, sprFile);
                    }
                }

                // Load description by name (only if entity doesn't already have it)
                if (!HasProperty(item, Command.DESCR))
                {
                    if (item.TryGetName(out string name) && !string.IsNullOrEmpty(name))
                    {
                        string sanitizedName = SanitizeName(name);
                        string descrFile = Path.Combine(descrPath, $"{sanitizedName}.txt");
                        if (File.Exists(descrFile))
                        {
                            string description = ReadDescriptionFile(descrFile);
                            if (!string.IsNullOrEmpty(description))
                            {
                                SetStringProperty(item, Command.DESCR, description);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads descriptions for all spells.
        /// </summary>
        private static void LoadSpellAssets(Mod mod, string basePath)
        {
            string descrPath = Path.Combine(basePath, "Data", "spelldescr");

            if (!mod.Database.TryGetValue(EntityType.SPELL, out var spellSet))
                return;

            foreach (var entity in spellSet.GetFullList())
            {
                if (entity is not Spell spell)
                    continue;

                if (!spell.TryGetName(out string name) || string.IsNullOrEmpty(name))
                    continue;

                string sanitizedName = SanitizeName(name);

                // Load main description (only if entity doesn't already have it)
                if (!HasProperty(spell, Command.DESCR))
                {
                    string descrFile = Path.Combine(descrPath, $"{sanitizedName}.txt");
                    if (File.Exists(descrFile))
                    {
                        string description = ReadDescriptionFile(descrFile);
                        if (!string.IsNullOrEmpty(description))
                        {
                            SetStringProperty(spell, Command.DESCR, description);
                        }
                    }
                }

                // Load details description (only if entity doesn't already have it)
                if (!HasProperty(spell, Command.DETAILS))
                {
                    string detailsFile = Path.Combine(descrPath, $"details{sanitizedName}.txt");
                    if (File.Exists(detailsFile))
                    {
                        string details = ReadDescriptionFile(detailsFile);
                        if (!string.IsNullOrEmpty(details))
                        {
                            SetStringProperty(spell, Command.DETAILS, details);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sanitizes a name for use in file paths.
        /// Removes all non-alphanumeric characters except hyphens.
        /// </summary>
        private static string SanitizeName(string name)
        {
            return Regex.Replace(name, @"[^a-zA-Z0-9\-]", "");
        }

        /// <summary>
        /// Reads a description file and returns its contents.
        /// </summary>
        private static string ReadDescriptionFile(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath).Trim();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[VanillaAssetLoader] Error reading {filePath}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Checks if an entity already has a property set.
        /// </summary>
        private static bool HasProperty(IDEntity entity, Command command)
        {
            return entity.Properties.Any(p => p.Command == command);
        }

        /// <summary>
        /// Sets a FilePathProperty on an entity.
        /// </summary>
        private static void SetFilePathProperty(IDEntity entity, Command command, string value)
        {
            var prop = FilePathProperty.Create() as FilePathProperty;
            if (prop != null)
            {
                prop.Parse(command, value, "");
                entity.AddProperty(prop);
            }
        }

        /// <summary>
        /// Sets a StringProperty on an entity.
        /// </summary>
        private static void SetStringProperty(IDEntity entity, Command command, string value)
        {
            var prop = StringProperty.Create() as StringProperty;
            if (prop != null)
            {
                prop.Parse(command, value, "");
                entity.AddProperty(prop);
            }
        }
    }
}
