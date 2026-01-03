using System.Text.Json;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Edit.Metadata
{
    /// <summary>
    /// Loads property metadata from JSON files for easier maintenance.
    /// </summary>
    public static class MetadataLoader
    {
        /// <summary>
        /// JSON structure for command metadata.
        /// </summary>
        public class CommandMetadataJson
        {
            public string name { get; set; } = "";
            public string args { get; set; } = "";
            public string description { get; set; } = "";
            public List<string> entity_types { get; set; } = new();
            public List<string> arg_types { get; set; } = new();
            public int page { get; set; }

            // Extended metadata (optional)
            public string? display_name { get; set; }
            public string? category { get; set; }
            public int? display_order { get; set; }
            public int? min_value { get; set; }
            public int? max_value { get; set; }
            public bool? allow_multiple { get; set; }
        }

        /// <summary>
        /// Loads metadata from a JSON file and registers with PropertyMetadata.
        /// </summary>
        public static int LoadFromJson(string jsonPath)
        {
            if (!File.Exists(jsonPath))
            {
                return 0;
            }

            var json = File.ReadAllText(jsonPath);
            var commands = JsonSerializer.Deserialize<List<CommandMetadataJson>>(json);

            if (commands == null) return 0;

            int loaded = 0;
            foreach (var cmd in commands)
            {
                var definition = ConvertToDefinition(cmd);
                if (definition != null)
                {
                    PropertyMetadata.Register(definition);
                    loaded++;
                }
            }

            return loaded;
        }

        /// <summary>
        /// Converts JSON metadata to a PropertyDefinition.
        /// </summary>
        private static PropertyDefinition? ConvertToDefinition(CommandMetadataJson json)
        {
            // Try to find the command enum
            var cmdName = json.name.TrimStart('#').ToUpperInvariant();
            if (!Enum.TryParse<Command>(cmdName, out var command))
            {
                return null;
            }

            // Determine property type from arg_types
            var propertyType = DeterminePropertyType(json.arg_types, cmdName);

            // Determine entity types
            var entityTypes = json.entity_types
                .Select(ParseEntityType)
                .Where(e => e.HasValue)
                .Select(e => e!.Value)
                .ToList();

            // Use provided display name or generate from command name
            var displayName = json.display_name ?? FormatDisplayName(cmdName);

            // Use provided category or infer from entity/command
            var category = json.category ?? InferCategory(cmdName, json.entity_types);

            var definition = new PropertyDefinition
            {
                Command = command,
                PropertyType = propertyType,
                DisplayName = displayName,
                Category = category,
                Tooltip = json.description,
                ApplicableEntities = entityTypes,
                ManualPage = json.page,
                DisplayOrder = json.display_order ?? 100,
                MinValue = json.min_value,
                MaxValue = json.max_value,
                AllowMultiple = json.allow_multiple ?? false,
                IsFlag = json.arg_types.Count == 0 || json.arg_types.Contains("none")
            };

            // Determine target entity type for references
            definition.TargetEntityType = DetermineTargetEntity(cmdName, propertyType);

            return definition;
        }

        private static Type DeterminePropertyType(List<string> argTypes, string cmdName)
        {
            // Check for reference types based on command name
            if (cmdName.Contains("MONSTER") || cmdName == "COPYSTATS" || cmdName == "SHAPECHANGE" ||
                cmdName.StartsWith("FIRST") || cmdName.StartsWith("SECOND") || cmdName.StartsWith("THIRD"))
            {
                return typeof(MonsterRef);
            }
            if (cmdName.Contains("WEAPON"))
            {
                return typeof(WeaponRef);
            }
            if (cmdName.Contains("ARMOR"))
            {
                return typeof(ArmorRef);
            }
            if (cmdName.Contains("SPELL") || cmdName == "AUTOSPELL")
            {
                return typeof(SpellRef);
            }
            if (cmdName.Contains("ITEM"))
            {
                return typeof(ItemRef);
            }
            if (cmdName.Contains("SITE") || cmdName == "STARTSITE" || cmdName == "HOMESITE")
            {
                return typeof(SiteRef);
            }
            if (cmdName.Contains("NATION"))
            {
                return typeof(NationRef);
            }

            // Determine from arg types
            if (argTypes.Count == 0 || argTypes.Contains("none"))
            {
                return typeof(CommandProperty);
            }
            if (argTypes.Contains("string") || argTypes.Contains("filepath"))
            {
                if (argTypes.Contains("filepath"))
                    return typeof(FilePathProperty);
                return typeof(StringProperty);
            }
            if (argTypes.Contains("bitmask"))
            {
                return typeof(BitmaskProperty);
            }

            // Default to IntProperty
            return typeof(IntProperty);
        }

        private static EntityType? ParseEntityType(string name)
        {
            return name.ToLowerInvariant() switch
            {
                "monster" => EntityType.MONSTER,
                "weapon" => EntityType.WEAPON,
                "armor" => EntityType.ARMOR,
                "spell" => EntityType.SPELL,
                "item" => EntityType.ITEM,
                "site" => EntityType.SITE,
                "nation" => EntityType.NATION,
                "event" => EntityType.EVENT,
                "mercenary" => EntityType.MERCENARY,
                "poptype" => EntityType.POPTYPE,
                "nametype" => EntityType.NAMETYPE,
                "montag" => EntityType.MONTAG,
                _ => null
            };
        }

        private static string FormatDisplayName(string cmdName)
        {
            // Convert SNAKE_CASE to Title Case
            var words = cmdName.Split('_')
                .Select(w => w.Length > 0
                    ? char.ToUpper(w[0]) + w.Substring(1).ToLower()
                    : "");
            return string.Join(" ", words);
        }

        private static string InferCategory(string cmdName, List<string> entityTypes)
        {
            // Infer category from command name patterns
            if (cmdName.StartsWith("REQ_"))
                return "Requirements";
            if (cmdName.Contains("RES") && !cmdName.Contains("RESEARCH"))
                return "Resistances";
            if (cmdName.Contains("FIRE") || cmdName.Contains("COLD") || cmdName.Contains("SHOCK") ||
                cmdName.Contains("POISON") && !cmdName.Contains("RES"))
                return "Elemental";
            if (cmdName.Contains("MAGIC") || cmdName.Contains("PATH") || cmdName.Contains("SPELL"))
                return "Magic";
            if (cmdName.Contains("LEADER") || cmdName.Contains("COMMAND"))
                return "Leadership";
            if (cmdName.Contains("SHAPE") || cmdName.Contains("FORM"))
                return "Shape Changing";
            if (cmdName.Contains("SUMMON") || cmdName.Contains("DOMSUMMON") || cmdName.Contains("BATTLESUM"))
                return "Summoning";
            if (cmdName.Contains("COST") || cmdName.Contains("REC"))
                return "Recruitment";
            if (cmdName.Contains("FLY") || cmdName.Contains("SWIM") || cmdName.Contains("MOVE") ||
                cmdName.Contains("AQUATIC") || cmdName.Contains("AMPHIB"))
                return "Movement";
            if (cmdName == "HP" || cmdName == "STR" || cmdName == "ATT" || cmdName == "DEF" ||
                cmdName == "PREC" || cmdName == "PROT" || cmdName == "MR" || cmdName == "MOR")
                return "Combat Stats";
            if (cmdName == "NAME" || cmdName == "DESCR" || cmdName == "SPR1" || cmdName == "SPR2")
                return "Identity";
            if (cmdName.Contains("WEAPON") || cmdName.Contains("ARMOR"))
                return "Equipment";
            if (cmdName.Contains("DMG") || cmdName.Contains("NRATT") || cmdName.Contains("RANGE"))
                return "Weapon Stats";
            if (cmdName.Contains("EFFECT") || cmdName.Contains("AOE") || cmdName.Contains("FATIGUE"))
                return "Spell Effects";

            return "Miscellaneous";
        }

        private static EntityType? DetermineTargetEntity(string cmdName, Type propertyType)
        {
            if (propertyType == typeof(MonsterRef))
                return EntityType.MONSTER;
            if (propertyType == typeof(WeaponRef))
                return EntityType.WEAPON;
            if (propertyType == typeof(ArmorRef))
                return EntityType.ARMOR;
            if (propertyType == typeof(SpellRef))
                return EntityType.SPELL;
            if (propertyType == typeof(ItemRef))
                return EntityType.ITEM;
            if (propertyType == typeof(SiteRef))
                return EntityType.SITE;
            if (propertyType == typeof(NationRef))
                return EntityType.NATION;

            return null;
        }

        /// <summary>
        /// Exports current metadata to JSON for editing.
        /// </summary>
        public static void ExportToJson(string outputPath)
        {
            var commands = new List<CommandMetadataJson>();

            foreach (Command cmd in Enum.GetValues<Command>())
            {
                var def = PropertyMetadata.Get(cmd);
                if (def == null) continue;

                commands.Add(new CommandMetadataJson
                {
                    name = "#" + cmd.ToString().ToLowerInvariant(),
                    display_name = def.DisplayName,
                    category = def.Category,
                    description = def.Tooltip,
                    entity_types = def.ApplicableEntities.Select(e => e.ToString().ToLowerInvariant()).ToList(),
                    arg_types = DetermineArgTypes(def),
                    page = def.ManualPage,
                    display_order = def.DisplayOrder,
                    min_value = def.MinValue,
                    max_value = def.MaxValue,
                    allow_multiple = def.AllowMultiple
                });
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(commands, options);
            File.WriteAllText(outputPath, json);
        }

        private static List<string> DetermineArgTypes(PropertyDefinition def)
        {
            if (def.IsFlag)
                return new List<string> { "none" };

            var types = new List<string>();

            if (def.PropertyType == typeof(IntProperty))
                types.Add("integer");
            else if (def.PropertyType == typeof(StringProperty))
                types.Add("string");
            else if (def.PropertyType == typeof(FilePathProperty))
                types.Add("filepath");
            else if (def.PropertyType == typeof(BitmaskProperty))
                types.Add("bitmask");
            else if (def.PropertyType.Name.Contains("Ref"))
                types.Add("id");

            return types;
        }
    }
}
