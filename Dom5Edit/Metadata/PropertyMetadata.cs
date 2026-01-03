using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Edit.Metadata
{
    /// <summary>
    /// Registry for property metadata. Provides UI hints, validation rules,
    /// and documentation for commands.
    /// </summary>
    public static class PropertyMetadata
    {
        private static Dictionary<Command, PropertyDefinition> _definitions = new();
        private static Dictionary<EntityType, List<PropertyDefinition>> _byEntity = new();
        private static Dictionary<string, List<PropertyDefinition>> _byCategory = new();
        private static bool _initialized = false;

        /// <summary>
        /// Gets the property definition for a command, or null if not found.
        /// </summary>
        public static PropertyDefinition? Get(Command command)
        {
            EnsureInitialized();
            return _definitions.TryGetValue(command, out var def) ? def : null;
        }

        /// <summary>
        /// Gets all property definitions for an entity type.
        /// </summary>
        public static IReadOnlyList<PropertyDefinition> GetForEntity(EntityType entityType)
        {
            EnsureInitialized();
            return _byEntity.TryGetValue(entityType, out var list)
                ? list.AsReadOnly()
                : Array.Empty<PropertyDefinition>();
        }

        /// <summary>
        /// Gets all property definitions grouped by category for an entity type.
        /// </summary>
        public static Dictionary<string, List<PropertyDefinition>> GetCategorized(EntityType entityType)
        {
            EnsureInitialized();
            var forEntity = GetForEntity(entityType);
            return forEntity
                .GroupBy(d => d.Category)
                .OrderBy(g => GetCategoryOrder(g.Key))
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(d => d.DisplayOrder).ToList()
                );
        }

        /// <summary>
        /// Gets display name for a command.
        /// </summary>
        public static string GetDisplayName(Command command)
        {
            var def = Get(command);
            if (def != null && !string.IsNullOrEmpty(def.DisplayName))
                return def.DisplayName;

            // Fallback: convert enum name to title case
            return FormatEnumName(command.ToString());
        }

        /// <summary>
        /// Gets tooltip for a command.
        /// </summary>
        public static string GetTooltip(Command command)
        {
            return Get(command)?.Tooltip ?? "";
        }

        /// <summary>
        /// Checks if a value is valid for a command.
        /// </summary>
        public static bool ValidateValue(Command command, int value, out string? error)
        {
            error = null;
            var def = Get(command);
            if (def == null) return true;

            if (def.MinValue.HasValue && value < def.MinValue.Value)
            {
                error = $"{def.DisplayName} must be at least {def.MinValue}";
                return false;
            }

            if (def.MaxValue.HasValue && value > def.MaxValue.Value)
            {
                error = $"{def.DisplayName} must be at most {def.MaxValue}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Registers a property definition.
        /// </summary>
        public static void Register(PropertyDefinition definition)
        {
            _definitions[definition.Command] = definition;

            // Index by entity type
            foreach (var entityType in definition.ApplicableEntities)
            {
                if (!_byEntity.ContainsKey(entityType))
                    _byEntity[entityType] = new List<PropertyDefinition>();
                _byEntity[entityType].Add(definition);
            }

            // Index by category
            if (!_byCategory.ContainsKey(definition.Category))
                _byCategory[definition.Category] = new List<PropertyDefinition>();
            _byCategory[definition.Category].Add(definition);
        }

        /// <summary>
        /// Gets all registered categories.
        /// </summary>
        public static IReadOnlyList<string> GetCategories()
        {
            EnsureInitialized();
            return _byCategory.Keys
                .OrderBy(GetCategoryOrder)
                .ToList()
                .AsReadOnly();
        }

        private static void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;
            InitializeMetadata();
            TryLoadFromJson();
        }

        /// <summary>
        /// Attempts to load additional metadata from JSON file if available.
        /// </summary>
        private static void TryLoadFromJson()
        {
            // Try to find commands_clean.json in common locations
            var possiblePaths = new[]
            {
                "docs/pdf_extracted/commands_clean.json",
                "../docs/pdf_extracted/commands_clean.json",
                "../../docs/pdf_extracted/commands_clean.json",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "commands_clean.json"),
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        var loaded = MetadataLoader.LoadFromJson(path);
                        System.Diagnostics.Debug.WriteLine($"Loaded {loaded} command definitions from {path}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to load metadata from {path}: {ex.Message}");
                    }
                    break;
                }
            }
        }

        private static int GetCategoryOrder(string category)
        {
            return category switch
            {
                "Identity" => 0,
                "Sprites" => 1,
                "Basic Stats" => 2,
                "Combat Stats" => 3,
                "Movement" => 4,
                "Magic" => 5,
                "Leadership" => 6,
                "Equipment" => 7,
                "Resistances" => 8,
                "Immunities" => 9,
                "Shape Changing" => 10,
                "Summoning" => 11,
                "Recruitment" => 12,
                "Special Abilities" => 13,
                "Miscellaneous" => 99,
                _ => 50
            };
        }

        private static string FormatEnumName(string enumName)
        {
            // Convert SNAKE_CASE or PascalCase to Title Case
            var words = enumName.Split('_')
                .Select(w => w.Length > 0
                    ? char.ToUpper(w[0]) + w.Substring(1).ToLower()
                    : "");
            return string.Join(" ", words);
        }

        /// <summary>
        /// Initializes all property metadata. This is where we define
        /// display names, categories, and validation for all commands.
        /// </summary>
        private static void InitializeMetadata()
        {
            // Monster - Identity
            Register(PropertyDefinition.CreateInt(Command.NAME, "Name", "Identity", "The name of the entity")
                .ForEntities(EntityType.MONSTER, EntityType.WEAPON, EntityType.ARMOR, EntityType.SPELL, EntityType.ITEM, EntityType.SITE, EntityType.NATION)
                .WithOrder(1));

            Register(PropertyDefinition.Create(Command.DESCR, typeof(StringProperty), "Description", "Identity", "Description text")
                .ForEntities(EntityType.MONSTER, EntityType.SPELL, EntityType.ITEM, EntityType.SITE, EntityType.NATION)
                .WithOrder(2));

            // Monster - Sprites
            Register(PropertyDefinition.Create(Command.SPR1, typeof(FilePathProperty), "Sprite 1", "Sprites", "Normal sprite image file")
                .ForEntities(EntityType.MONSTER)
                .WithOrder(1));

            Register(PropertyDefinition.Create(Command.SPR2, typeof(FilePathProperty), "Sprite 2", "Sprites", "Attack sprite image file")
                .ForEntities(EntityType.MONSTER)
                .WithOrder(2));

            Register(PropertyDefinition.CreateInt(Command.DRAWSIZE, "Draw Size", "Sprites", "Percentage size adjustment for sprite", min: -100, max: 500)
                .ForEntities(EntityType.MONSTER)
                .WithOrder(3));

            // Monster - Basic Stats
            Register(PropertyDefinition.CreateInt(Command.HP, "Hit Points", "Basic Stats", "Base hit points", min: 1, max: 999)
                .ForEntities(EntityType.MONSTER, EntityType.ITEM)
                .WithOrder(1));

            Register(PropertyDefinition.CreateInt(Command.SIZE, "Size", "Basic Stats", "Unit size (1-6)", min: 1, max: 6)
                .ForEntities(EntityType.MONSTER)
                .WithOrder(2));

            Register(PropertyDefinition.CreateInt(Command.RESSIZE, "Resource Size", "Basic Stats", "Size for resource cost calculation")
                .ForEntities(EntityType.MONSTER)
                .WithOrder(3));

            // Monster - Combat Stats
            Register(PropertyDefinition.CreateInt(Command.STR, "Strength", "Combat Stats", "Physical strength", min: 1, max: 99)
                .ForEntities(EntityType.MONSTER)
                .WithOrder(1));

            Register(PropertyDefinition.CreateInt(Command.ATT, "Attack", "Combat Stats", "Attack skill", min: 0, max: 99)
                .ForEntities(EntityType.MONSTER)
                .WithOrder(2));

            Register(PropertyDefinition.CreateInt(Command.DEF, "Defense", "Combat Stats", "Defense skill", min: 0, max: 99)
                .ForEntities(EntityType.MONSTER, EntityType.ARMOR)
                .WithOrder(3));

            Register(PropertyDefinition.CreateInt(Command.PREC, "Precision", "Combat Stats", "Ranged attack precision", min: 0, max: 99)
                .ForEntities(EntityType.MONSTER)
                .WithOrder(4));

            Register(PropertyDefinition.CreateInt(Command.PROT, "Protection", "Combat Stats", "Natural protection", min: 0, max: 99)
                .ForEntities(EntityType.MONSTER, EntityType.ARMOR)
                .WithOrder(5));

            Register(PropertyDefinition.CreateInt(Command.MR, "Magic Resistance", "Combat Stats", "Resistance to magic", min: 0, max: 99)
                .ForEntities(EntityType.MONSTER)
                .WithOrder(6));

            Register(PropertyDefinition.CreateInt(Command.MOR, "Morale", "Combat Stats", "Base morale", min: 0, max: 99)
                .ForEntities(EntityType.MONSTER)
                .WithOrder(7));

            Register(PropertyDefinition.CreateInt(Command.ENC, "Encumbrance", "Combat Stats", "Fatigue accumulation rate", min: 0, max: 99)
                .ForEntities(EntityType.MONSTER, EntityType.ARMOR)
                .WithOrder(8));

            // Monster - Movement
            Register(PropertyDefinition.CreateInt(Command.AP, "Action Points", "Movement", "Combat movement points", min: 1, max: 99)
                .ForEntities(EntityType.MONSTER)
                .WithOrder(1));

            Register(PropertyDefinition.CreateInt(Command.MAPMOVE, "Map Move", "Movement", "Strategic map movement", min: 1, max: 99)
                .ForEntities(EntityType.MONSTER)
                .WithOrder(2));

            Register(PropertyDefinition.CreateFlag(Command.FLYING, "Flying", "Movement", "Can fly")
                .ForEntities(EntityType.MONSTER)
                .WithOrder(3));

            Register(PropertyDefinition.CreateFlag(Command.SWIMMING, "Swimming", "Movement", "Can swim")
                .ForEntities(EntityType.MONSTER)
                .WithOrder(4));

            Register(PropertyDefinition.CreateFlag(Command.AQUATIC, "Aquatic", "Movement", "Aquatic creature")
                .ForEntities(EntityType.MONSTER)
                .WithOrder(5));

            Register(PropertyDefinition.CreateFlag(Command.AMPHIBIAN, "Amphibian", "Movement", "Can move on land and water")
                .ForEntities(EntityType.MONSTER)
                .WithOrder(6));

            // Monster - Equipment
            Register(PropertyDefinition.CreateRef(Command.WEAPON, typeof(WeaponRef), EntityType.WEAPON, "Weapon", "Equipment", "Assign a weapon", allowMultiple: true)
                .ForEntities(EntityType.MONSTER, EntityType.ITEM)
                .WithOrder(1));

            Register(PropertyDefinition.CreateRef(Command.ARMOR, typeof(ArmorRef), EntityType.ARMOR, "Armor", "Equipment", "Assign armor", allowMultiple: true)
                .ForEntities(EntityType.MONSTER, EntityType.ITEM)
                .WithOrder(2));

            // Monster - Recruitment
            Register(PropertyDefinition.CreateInt(Command.GCOST, "Gold Cost", "Recruitment", "Gold cost to recruit", min: 0)
                .ForEntities(EntityType.MONSTER)
                .WithOrder(1));

            Register(PropertyDefinition.CreateInt(Command.RCOST, "Resource Cost", "Recruitment", "Resource cost to recruit", min: 0)
                .ForEntities(EntityType.MONSTER, EntityType.ARMOR, EntityType.WEAPON)
                .WithOrder(2));

            Register(PropertyDefinition.CreateInt(Command.RPCOST, "Recruitment Points", "Recruitment", "Recruitment points cost")
                .ForEntities(EntityType.MONSTER)
                .WithOrder(3));

            // Monster - Resistances
            Register(PropertyDefinition.CreateInt(Command.FIRERES, "Fire Resistance", "Resistances", "Resistance to fire damage", min: -100, max: 100)
                .ForEntities(EntityType.MONSTER, EntityType.ITEM)
                .WithOrder(1));

            Register(PropertyDefinition.CreateInt(Command.COLDRES, "Cold Resistance", "Resistances", "Resistance to cold damage", min: -100, max: 100)
                .ForEntities(EntityType.MONSTER, EntityType.ITEM)
                .WithOrder(2));

            Register(PropertyDefinition.CreateInt(Command.SHOCKRES, "Shock Resistance", "Resistances", "Resistance to shock damage", min: -100, max: 100)
                .ForEntities(EntityType.MONSTER, EntityType.ITEM)
                .WithOrder(3));

            Register(PropertyDefinition.CreateInt(Command.POISONRES, "Poison Resistance", "Resistances", "Resistance to poison damage", min: -100, max: 100)
                .ForEntities(EntityType.MONSTER, EntityType.ITEM)
                .WithOrder(4));

            // Weapon Properties
            Register(PropertyDefinition.CreateInt(Command.DMG, "Damage", "Combat Stats", "Weapon damage", min: 0, max: 999)
                .ForEntities(EntityType.WEAPON)
                .WithOrder(1));

            Register(PropertyDefinition.CreateInt(Command.NRATT, "Number of Attacks", "Combat Stats", "Attacks per round", min: 1, max: 10)
                .ForEntities(EntityType.WEAPON)
                .WithOrder(2));

            Register(PropertyDefinition.CreateInt(Command.RANGE, "Range", "Combat Stats", "Weapon range (0 for melee)")
                .ForEntities(EntityType.WEAPON, EntityType.SPELL)
                .WithOrder(3));

            Register(PropertyDefinition.CreateInt(Command.AMMO, "Ammunition", "Combat Stats", "Shots before reload")
                .ForEntities(EntityType.WEAPON)
                .WithOrder(4));

            Register(PropertyDefinition.CreateInt(Command.LEN, "Length", "Combat Stats", "Weapon length for repel")
                .ForEntities(EntityType.WEAPON)
                .WithOrder(5));

            Register(PropertyDefinition.CreateFlag(Command.TWOHANDED, "Two-Handed", "Equipment", "Requires two hands")
                .ForEntities(EntityType.WEAPON)
                .WithOrder(1));

            Register(PropertyDefinition.CreateFlag(Command.NATURAL, "Natural Weapon", "Equipment", "Natural weapon (cannot be dropped)")
                .ForEntities(EntityType.WEAPON)
                .WithOrder(2));

            // Spell Properties
            Register(PropertyDefinition.CreateInt(Command.EFFECT, "Effect", "Spell Effects", "Spell effect number")
                .ForEntities(EntityType.SPELL)
                .WithOrder(1));

            Register(PropertyDefinition.CreateInt(Command.DAMAGE, "Spell Damage", "Spell Effects", "Damage or effect strength")
                .ForEntities(EntityType.SPELL)
                .WithOrder(2));

            Register(PropertyDefinition.CreateInt(Command.AOE, "Area of Effect", "Spell Effects", "Area of effect size")
                .ForEntities(EntityType.SPELL, EntityType.WEAPON)
                .WithOrder(3));

            Register(PropertyDefinition.CreateInt(Command.FATIGUECOST, "Fatigue Cost", "Spell Costs", "Fatigue cost to cast")
                .ForEntities(EntityType.SPELL)
                .WithOrder(1));

            Register(PropertyDefinition.CreateInt(Command.SCHOOL, "School", "Spell Research", "Research school (0-7)")
                .ForEntities(EntityType.SPELL)
                .WithOrder(1));

            Register(PropertyDefinition.CreateInt(Command.RESEARCHLEVEL, "Research Level", "Spell Research", "Research level required")
                .ForEntities(EntityType.SPELL)
                .WithOrder(2));

            // Nation Properties
            Register(PropertyDefinition.CreateInt(Command.ERA, "Era", "Identity", "Nation era (1=Early, 2=Middle, 3=Late)", min: 1, max: 3)
                .ForEntities(EntityType.NATION)
                .WithOrder(1));

            Register(PropertyDefinition.CreateRef(Command.STARTSITE, typeof(SiteRef), EntityType.SITE, "Starting Site", "Sites", "Starting magic site")
                .ForEntities(EntityType.NATION)
                .WithOrder(1));

            // Site Properties
            Register(PropertyDefinition.CreateInt(Command.RARITY, "Rarity", "Basic Stats", "Site rarity (0-5)", min: 0, max: 5)
                .ForEntities(EntityType.SITE, EntityType.EVENT)
                .WithOrder(1));

            Register(PropertyDefinition.CreateInt(Command.LOC, "Location", "Basic Stats", "Location type")
                .ForEntities(EntityType.SITE)
                .WithOrder(2));

            Register(PropertyDefinition.CreateInt(Command.LEVEL, "Level", "Basic Stats", "Site level for discovery")
                .ForEntities(EntityType.SITE)
                .WithOrder(3));

            Register(PropertyDefinition.CreateInt(Command.GOLD, "Gold Income", "Income", "Monthly gold income")
                .ForEntities(EntityType.SITE, EntityType.EVENT)
                .WithOrder(1));

            Register(PropertyDefinition.CreateRef(Command.HOMEMON, typeof(MonsterRef), EntityType.MONSTER, "Home Monster", "Recruitment", "Recruitable monster", allowMultiple: true)
                .ForEntities(EntityType.SITE)
                .WithOrder(1));

            Register(PropertyDefinition.CreateRef(Command.HOMECOM, typeof(MonsterRef), EntityType.MONSTER, "Home Commander", "Recruitment", "Recruitable commander", allowMultiple: true)
                .ForEntities(EntityType.SITE)
                .WithOrder(2));

            // Item Properties
            Register(PropertyDefinition.CreateInt(Command.CONSTLEVEL, "Construction Level", "Forging", "Construction level required", min: 0, max: 8)
                .ForEntities(EntityType.ITEM)
                .WithOrder(1));

            Register(PropertyDefinition.CreateInt(Command.MAINPATH, "Main Path", "Forging", "Main magic path (0-8)")
                .ForEntities(EntityType.ITEM)
                .WithOrder(2));

            Register(PropertyDefinition.CreateInt(Command.MAINLEVEL, "Main Level", "Forging", "Required path level")
                .ForEntities(EntityType.ITEM)
                .WithOrder(3));

            Register(PropertyDefinition.CreateInt(Command.GEMPROD, "Gem Production", "Magic", "Monthly gem production")
                .ForEntities(EntityType.MONSTER, EntityType.SITE)
                .WithOrder(4));

            // Note: This is a starting point. The full implementation would include
            // all ~1500+ commands with proper categorization. This can be loaded
            // from JSON generated by the PDF parser for easier maintenance.
        }
    }
}
