using System.Text.Json;

namespace Dom5Edit
{
    /// <summary>
    /// Dynamically loads spell effect data from JSON files.
    /// Replaces hardcoded spell effect mappings with external data that can be updated.
    /// </summary>
    public class SpellEffectData
    {
        private static SpellEffectData? _instance;
        private static readonly object _lock = new object();

        // Spell ID -> Effect Number
        private Dictionary<int, int> _spellIdToEffect = new();

        // Spell Name -> Effect Number
        private Dictionary<string, int> _spellNameToEffect = new();

        // Effect numbers that indicate summon spells (damage = monster ID)
        private HashSet<int> _summonEffects = new();

        // Effect numbers that indicate enchantment spells (damage = enchantment ID)
        private HashSet<int> _enchantEffects = new();

        // Effect numbers that indicate event effect spells
        private HashSet<int> _eventEffects = new();

        // Effect numbers that use bitmask damage values
        private HashSet<int> _bitmaskEffects = new();

        public static SpellEffectData Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new SpellEffectData();
                    }
                }
                return _instance;
            }
        }

        public bool IsLoaded { get; private set; }
        public string? LoadError { get; private set; }

        private SpellEffectData()
        {
            // Initialize with default effect types (fallback if JSON not loaded)
            InitializeDefaultEffectTypes();
        }

        /// <summary>
        /// Load spell effect data from JSON files.
        /// </summary>
        /// <param name="spellMappingPath">Path to spell_effects_mapping.json</param>
        /// <param name="effectTypesPath">Path to spell_effect_types.json (optional)</param>
        public void Load(string spellMappingPath, string? effectTypesPath = null)
        {
            try
            {
                // Load effect types first (defines how to classify effects)
                if (!string.IsNullOrEmpty(effectTypesPath) && File.Exists(effectTypesPath))
                {
                    LoadEffectTypes(effectTypesPath);
                }

                // Load spell ID -> effect mapping
                if (File.Exists(spellMappingPath))
                {
                    LoadSpellMapping(spellMappingPath);
                }

                IsLoaded = true;
                LoadError = null;
            }
            catch (Exception ex)
            {
                LoadError = ex.Message;
                IsLoaded = false;
            }
        }

        /// <summary>
        /// Reload the spell effect data (useful when JSON files are updated).
        /// </summary>
        public static void Reload(string spellMappingPath, string? effectTypesPath = null)
        {
            lock (_lock)
            {
                _instance = new SpellEffectData();
                _instance.Load(spellMappingPath, effectTypesPath);
            }
        }

        private void LoadEffectTypes(string path)
        {
            string json = File.ReadAllText(path);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("effect_types", out var effectTypes))
            {
                foreach (var effect in effectTypes.EnumerateObject())
                {
                    if (!int.TryParse(effect.Name, out int effectNum)) continue;

                    if (effect.Value.TryGetProperty("argument_type", out var argType))
                    {
                        string argTypeStr = argType.GetString() ?? "";
                        switch (argTypeStr)
                        {
                            case "unit_id":
                                _summonEffects.Add(effectNum);
                                break;
                            case "enchantment_id":
                                _enchantEffects.Add(effectNum);
                                break;
                            case "event_id":
                                _eventEffects.Add(effectNum);
                                break;
                            case "bitmask":
                                _bitmaskEffects.Add(effectNum);
                                break;
                        }
                    }
                }
            }
        }

        private void LoadSpellMapping(string path)
        {
            string json = File.ReadAllText(path);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("spell_effects", out var spellEffects))
            {
                foreach (var spell in spellEffects.EnumerateObject())
                {
                    if (!int.TryParse(spell.Name, out int spellId)) continue;

                    if (spell.Value.TryGetProperty("effect_number", out var effectNum))
                    {
                        int effect = effectNum.GetInt32();
                        _spellIdToEffect[spellId] = effect;

                        // Also map by name
                        if (spell.Value.TryGetProperty("spell_name", out var spellName))
                        {
                            string name = spellName.GetString() ?? "";
                            if (!string.IsNullOrEmpty(name))
                            {
                                _spellNameToEffect[name] = effect;
                            }
                        }
                    }
                }
            }
        }

        private void InitializeDefaultEffectTypes()
        {
            // Default summon effects (from Dom5 data as fallback)
            _summonEffects = new HashSet<int> { 1, 21, 26, 31, 37, 38, 43, 50, 54, 62, 68, 76, 89, 93, 119, 126, 127, 130, 137, 141 };

            // Default enchant effects
            _enchantEffects = new HashSet<int> { 81, 82, 83, 84, 85, 86 };

            // Default event effects
            _eventEffects = new HashSet<int> { 42 };

            // Default bitmask effects
            _bitmaskEffects = new HashSet<int> { 10, 11, 23 };
        }

        #region Public Query Methods

        public bool ContainsSpell(int spellId) => _spellIdToEffect.ContainsKey(spellId);

        public bool ContainsSpell(string spellName) => _spellNameToEffect.ContainsKey(spellName);

        public bool TryGetEffect(int spellId, out int effect) => _spellIdToEffect.TryGetValue(spellId, out effect);

        public bool TryGetEffect(string spellName, out int effect) => _spellNameToEffect.TryGetValue(spellName, out effect);

        public bool IsSummonEffect(int effect)
        {
            if (effect > 10000) effect -= 10000;
            return _summonEffects.Contains(effect);
        }

        public bool IsEnchantEffect(int effect)
        {
            if (effect > 10000) effect -= 10000;
            return _enchantEffects.Contains(effect);
        }

        public bool IsEventEffect(int effect)
        {
            if (effect > 10000) effect -= 10000;
            return _eventEffects.Contains(effect);
        }

        public bool IsBitmaskEffect(int effect)
        {
            if (effect > 10000) effect -= 10000;
            return _bitmaskEffects.Contains(effect);
        }

        public bool IsSummonSpell(int spellId)
        {
            if (TryGetEffect(spellId, out int effect))
            {
                return IsSummonEffect(effect);
            }
            return false;
        }

        public bool IsSummonSpell(string spellName)
        {
            if (TryGetEffect(spellName, out int effect))
            {
                return IsSummonEffect(effect);
            }
            return false;
        }

        public bool IsEnchantSpell(int spellId)
        {
            if (TryGetEffect(spellId, out int effect))
            {
                return IsEnchantEffect(effect);
            }
            return false;
        }

        public bool IsEnchantSpell(string spellName)
        {
            if (TryGetEffect(spellName, out int effect))
            {
                return IsEnchantEffect(effect);
            }
            return false;
        }

        public bool IsEventEffectSpell(int spellId)
        {
            if (TryGetEffect(spellId, out int effect))
            {
                return IsEventEffect(effect);
            }
            return false;
        }

        public bool IsEventEffectSpell(string spellName)
        {
            if (TryGetEffect(spellName, out int effect))
            {
                return IsEventEffect(effect);
            }
            return false;
        }

        /// <summary>
        /// Get statistics about loaded data.
        /// </summary>
        public (int spellCount, int summonEffects, int enchantEffects, int bitmaskEffects) GetStats()
        {
            return (_spellIdToEffect.Count, _summonEffects.Count, _enchantEffects.Count, _bitmaskEffects.Count);
        }

        #endregion
    }
}
