using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows.Media;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Editor.UI.Controls;

namespace Dom5Editor.Data
{
    /// <summary>
    /// Loads badge configuration from JSON files.
    /// </summary>
    public static class BadgeConfigLoader
    {
        private static readonly Dictionary<string, BadgeConfig> _cache = new();
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        /// <summary>
        /// Loads badge configuration for the specified entity type.
        /// </summary>
        /// <param name="entityType">Entity type (e.g., "monster", "weapon", "armor")</param>
        /// <returns>Badge configuration or null if not found</returns>
        public static BadgeConfig LoadConfig(string entityType)
        {
            var key = entityType.ToLowerInvariant();

            if (_cache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            var config = LoadFromFile(key) ?? LoadFromEmbeddedResource(key);

            if (config != null)
            {
                _cache[key] = config;
            }

            return config;
        }

        /// <summary>
        /// Clears the configuration cache, forcing reload on next access.
        /// </summary>
        public static void ClearCache()
        {
            _cache.Clear();
            _commandDescriptions = null;
        }

        #region Command Descriptions

        private static Dictionary<string, string>? _commandDescriptions;

        /// <summary>
        /// Gets command descriptions loaded from the command reference JSON.
        /// </summary>
        public static Dictionary<string, string> CommandDescriptions
        {
            get
            {
                _commandDescriptions ??= LoadCommandDescriptions();
                return _commandDescriptions;
            }
        }

        /// <summary>
        /// Loads command descriptions from docs/pdf_extracted/commands_by_entity_clean.json
        /// </summary>
        private static Dictionary<string, string> LoadCommandDescriptions()
        {
            var descriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var assemblyLocation = Path.GetDirectoryName(assembly.Location);

                // Try multiple possible locations
                var possiblePaths = new[]
                {
                    Path.Combine(assemblyLocation ?? "", "Data", "commands_by_entity_clean.json"),
                    Path.Combine(assemblyLocation ?? "", "..", "..", "..", "..", "docs", "pdf_extracted", "commands_by_entity_clean.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "docs", "pdf_extracted", "commands_by_entity_clean.json")
                };

                string? json = null;
                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        json = File.ReadAllText(path);
                        break;
                    }
                }

                if (json != null)
                {
                    using var doc = JsonDocument.Parse(json);
                    foreach (var entityType in doc.RootElement.EnumerateObject())
                    {
                        foreach (var cmd in entityType.Value.EnumerateArray())
                        {
                            if (cmd.TryGetProperty("name", out var nameEl) &&
                                cmd.TryGetProperty("description", out var descEl))
                            {
                                var name = nameEl.GetString()?.TrimStart('#');
                                var desc = descEl.GetString();
                                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(desc) && !descriptions.ContainsKey(name))
                                {
                                    descriptions[name] = desc;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading command descriptions: {ex.Message}");
            }

            return descriptions;
        }

        /// <summary>
        /// Gets the description for a command from the reference JSON.
        /// </summary>
        public static string? GetCommandDescription(string commandName)
        {
            if (string.IsNullOrEmpty(commandName))
                return null;

            var name = commandName.TrimStart('#');
            return CommandDescriptions.TryGetValue(name, out var desc) ? desc : null;
        }

        #endregion

        /// <summary>
        /// Tries to load configuration from a file in the Data folder.
        /// </summary>
        private static BadgeConfig LoadFromFile(string entityType)
        {
            try
            {
                // Try multiple base paths since Assembly.Location can be empty in .NET 8+
                var possibleBasePaths = new[]
                {
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    AppDomain.CurrentDomain.BaseDirectory,
                    Directory.GetCurrentDirectory()
                };

                foreach (var basePath in possibleBasePaths)
                {
                    if (string.IsNullOrEmpty(basePath))
                        continue;

                    var filePath = Path.Combine(basePath, "Data", $"{entityType}_badges.json");
                    if (File.Exists(filePath))
                    {
                        var json = File.ReadAllText(filePath);
                        return JsonSerializer.Deserialize<BadgeConfig>(json, _jsonOptions);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading badge config from file: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Tries to load configuration from an embedded resource.
        /// </summary>
        private static BadgeConfig LoadFromEmbeddedResource(string entityType)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"Dom5Editor.Data.{entityType}_badges.json";

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using var reader = new StreamReader(stream);
                    var json = reader.ReadToEnd();
                    return JsonSerializer.Deserialize<BadgeConfig>(json, _jsonOptions);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading badge config from resource: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Converts a BadgeCommand to a Command enum value.
        /// </summary>
        public static bool TryGetCommand(BadgeCommand badgeCommand, out Command command)
        {
            // CommandsMap uses "#" prefix, but JSON file may omit it
            var name = badgeCommand.Name;
            if (!name.StartsWith("#"))
            {
                name = "#" + name;
            }

            return CommandsMap.TryGetCommand(name, out command);
        }

        /// <summary>
        /// Creates a PropertyItem from a BadgeCommand definition and current value.
        /// For int properties, use value parameter. For intint, use value1/value2. For string, use stringValue.
        /// </summary>
        public static PropertyItem CreatePropertyItem(BadgeCommand cmdDef, int? value, bool isModified = false, bool isSessionEdit = false)
        {
            // CommandsMap uses "#" prefix, but JSON file may omit it
            var name = cmdDef.Name;
            if (!name.StartsWith("#"))
            {
                name = "#" + name;
            }
            CommandsMap.TryGetCommand(name, out Command command);

            PropertyItem property;
            if (cmdDef.IsFlag)
            {
                property = PropertyItem.CreateFlag(command, cmdDef.Display, isModified, isSessionEdit);
            }
            else if (cmdDef.HasColors)
            {
                var bgColor = ParseColor(cmdDef.Color, Color.FromRgb(60, 60, 60));
                var borderColor = ParseColor(cmdDef.BorderColor, Color.FromRgb(80, 80, 80));
                property = PropertyItem.CreateColoredValue(
                    command,
                    cmdDef.Display,
                    value ?? cmdDef.Default ?? 0,
                    bgColor,
                    borderColor,
                    Colors.White,
                    isModified,
                    isSessionEdit);
            }
            else
            {
                property = PropertyItem.CreateValue(command, cmdDef.Display, value ?? cmdDef.Default ?? 0, isModified, isSessionEdit);
            }

            SetPropertyTooltipAndIcon(property, cmdDef);
            return property;
        }

        /// <summary>
        /// Creates a PropertyItem from a BadgeCommand definition for IntIntProperty (two int values).
        /// </summary>
        public static PropertyItem CreateIntIntPropertyItem(BadgeCommand cmdDef, int value1, int value2, bool isModified = false, bool isSessionEdit = false)
        {
            var name = cmdDef.Name;
            if (!name.StartsWith("#"))
            {
                name = "#" + name;
            }
            CommandsMap.TryGetCommand(name, out Command command);

            PropertyItem property;
            if (cmdDef.HasColors)
            {
                var bgColor = ParseColor(cmdDef.Color, Color.FromRgb(60, 60, 60));
                var borderColor = ParseColor(cmdDef.BorderColor, Color.FromRgb(80, 80, 80));
                property = PropertyItem.CreateColoredIntIntValue(
                    command,
                    cmdDef.Display,
                    value1,
                    value2,
                    bgColor,
                    borderColor,
                    Colors.White,
                    isModified,
                    isSessionEdit);
            }
            else
            {
                property = PropertyItem.CreateIntIntValue(command, cmdDef.Display, value1, value2, isModified, isSessionEdit);
            }

            SetPropertyTooltipAndIcon(property, cmdDef);
            return property;
        }

        /// <summary>
        /// Creates a PropertyItem from a BadgeCommand definition for StringProperty.
        /// </summary>
        public static PropertyItem CreateStringPropertyItem(BadgeCommand cmdDef, string value, bool isModified = false, bool isSessionEdit = false)
        {
            var name = cmdDef.Name;
            if (!name.StartsWith("#"))
            {
                name = "#" + name;
            }
            CommandsMap.TryGetCommand(name, out Command command);

            var property = PropertyItem.CreateStringValue(command, cmdDef.Display, value, isModified, isSessionEdit);

            SetPropertyTooltipAndIcon(property, cmdDef);
            return property;
        }

        /// <summary>
        /// Creates a PropertyItem from a BadgeCommand definition for BitmaskProperty.
        /// </summary>
        public static PropertyItem CreateBitmaskPropertyItem(BadgeCommand cmdDef, ulong value, bool isModified = false, bool isSessionEdit = false)
        {
            var name = cmdDef.Name;
            if (!name.StartsWith("#"))
            {
                name = "#" + name;
            }
            CommandsMap.TryGetCommand(name, out Command command);

            var property = PropertyItem.CreateBitmaskValue(command, cmdDef.Display, value, isModified, isSessionEdit);

            SetPropertyTooltipAndIcon(property, cmdDef);
            return property;
        }

        /// <summary>
        /// Sets tooltip and icon on a PropertyItem from a BadgeCommand definition.
        /// </summary>
        private static void SetPropertyTooltipAndIcon(PropertyItem property, BadgeCommand cmdDef)
        {
            // Set tooltip: prefer description from property JSON, then from command reference, then fallback to command name
            if (!string.IsNullOrEmpty(cmdDef.Description))
            {
                property.Tooltip = cmdDef.Description;
            }
            else
            {
                // Try to get description from command reference JSON
                var refDescription = GetCommandDescription(cmdDef.Name);
                property.Tooltip = !string.IsNullOrEmpty(refDescription)
                    ? $"#{cmdDef.Name}: {refDescription}"
                    : $"#{cmdDef.Name}";
            }

            // Set icon if defined
            if (cmdDef.HasIcon)
            {
                property.IconPath = cmdDef.Icon;
            }
        }

        /// <summary>
        /// Creates an AvailablePropertyItem from a BadgeCommand definition.
        /// </summary>
        public static AvailablePropertyItem CreateAvailableItem(BadgeCommand cmdDef)
        {
            // CommandsMap uses "#" prefix, but JSON file may omit it
            var name = cmdDef.Name;
            if (!name.StartsWith("#"))
            {
                name = "#" + name;
            }
            CommandsMap.TryGetCommand(name, out Command command);

            return new AvailablePropertyItem
            {
                Command = command,
                DisplayName = cmdDef.Display,
                DefaultValue = cmdDef.IsInt ? (cmdDef.Default ?? 0) : null,
                IsReference = cmdDef.IsRef,
                ReferenceType = cmdDef.RefType
            };
        }

        /// <summary>
        /// Creates a PropertyItem for a reference property from a BadgeCommand definition.
        /// </summary>
        public static PropertyItem CreateReferencePropertyItem(BadgeCommand cmdDef, int referenceId, string referenceName,
            bool isModified = false, bool isSessionEdit = false)
        {
            // CommandsMap uses "#" prefix, but JSON file may omit it
            var name = cmdDef.Name;
            if (!name.StartsWith("#"))
            {
                name = "#" + name;
            }
            CommandsMap.TryGetCommand(name, out Command command);

            PropertyItem property;
            if (cmdDef.HasColors)
            {
                var bgColor = ParseColor(cmdDef.Color, Color.FromRgb(60, 60, 60));
                var borderColor = ParseColor(cmdDef.BorderColor, Color.FromRgb(80, 80, 80));
                property = PropertyItem.CreateColoredReference(
                    command,
                    cmdDef.Display,
                    referenceId,
                    referenceName,
                    cmdDef.RefType,
                    bgColor,
                    borderColor,
                    Colors.White,
                    isModified,
                    isSessionEdit);
            }
            else
            {
                property = PropertyItem.CreateReference(
                    command,
                    cmdDef.Display,
                    referenceId,
                    referenceName,
                    cmdDef.RefType,
                    isModified,
                    isSessionEdit);
            }

            // Set tooltip
            if (!string.IsNullOrEmpty(cmdDef.Description))
            {
                property.Tooltip = cmdDef.Description;
            }
            else
            {
                var refDescription = GetCommandDescription(cmdDef.Name);
                property.Tooltip = !string.IsNullOrEmpty(refDescription)
                    ? $"#{cmdDef.Name}: {refDescription}"
                    : $"#{cmdDef.Name}";
            }

            // Set icon if defined
            if (cmdDef.HasIcon)
            {
                property.IconPath = cmdDef.Icon;
            }

            return property;
        }

        /// <summary>
        /// Maps a refType string (from JSON config) to an EntityType enum value.
        /// </summary>
        public static EntityType? GetEntityTypeFromRefType(string refType)
        {
            return refType?.ToLowerInvariant() switch
            {
                "monster" => EntityType.MONSTER,
                "weapon" => EntityType.WEAPON,
                "armor" => EntityType.ARMOR,
                "item" => EntityType.ITEM,
                "spell" => EntityType.SPELL,
                "site" => EntityType.SITE,
                "nation" => EntityType.NATION,
                "event" => EntityType.EVENT,
                "mercenary" => EntityType.MERCENARY,
                "nametype" => EntityType.NAMETYPE,
                "poptype" => EntityType.POPTYPE,
                "montag" => EntityType.MONTAG,
                "enchantment" => EntityType.ENCHANTMENT,
                _ => null
            };
        }

        /// <summary>
        /// Parses a hex color string to a Color.
        /// </summary>
        public static Color ParseColor(string hexColor, Color defaultColor)
        {
            if (string.IsNullOrEmpty(hexColor))
                return defaultColor;

            try
            {
                if (hexColor.StartsWith("#"))
                    hexColor = hexColor.Substring(1);

                if (hexColor.Length == 6)
                {
                    var r = Convert.ToByte(hexColor.Substring(0, 2), 16);
                    var g = Convert.ToByte(hexColor.Substring(2, 2), 16);
                    var b = Convert.ToByte(hexColor.Substring(4, 2), 16);
                    return Color.FromRgb(r, g, b);
                }
            }
            catch
            {
                // Fall through to default
            }

            return defaultColor;
        }

        /// <summary>
        /// Gets the path color for magic path rendering.
        /// </summary>
        public static Color GetPathColor(BadgeConfig config, string path)
        {
            if (config?.Renderers != null &&
                config.Renderers.TryGetValue("magicPathEditor", out var renderer) &&
                renderer.PathColors != null &&
                renderer.PathColors.TryGetValue(path, out var colorStr))
            {
                return ParseColor(colorStr, Colors.Gray);
            }

            // Default path colors if not in config
            return path switch
            {
                "F" => Color.FromRgb(255, 69, 0),   // Fire - OrangeRed
                "A" => Color.FromRgb(135, 206, 235), // Air - SkyBlue
                "W" => Color.FromRgb(65, 105, 225),  // Water - RoyalBlue
                "E" => Color.FromRgb(139, 69, 19),   // Earth - SaddleBrown
                "S" => Color.FromRgb(255, 215, 0),   // Astral - Gold
                "D" => Color.FromRgb(47, 79, 79),    // Death - DarkSlateGray
                "N" => Color.FromRgb(34, 139, 34),   // Nature - ForestGreen
                "B" => Color.FromRgb(139, 0, 0),     // Blood - DarkRed
                "H" => Colors.White,                  // Holy - White
                _ => Colors.Gray
            };
        }
    }
}
