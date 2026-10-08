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

        // effects whose #damage picks from one of the game's unit lists (the uniques a Bind ritual
        // chooses from, a terrain summon's units, the Tartarian Gate's): effect -> (kind, list name)
        private readonly Dictionary<int, (string Kind, string? Lookup)> _listEffects = new();
        // list name ("uniqueSummon", "terrainSummon", "tartarianGate", ...) -> key -> units
        private readonly Dictionary<string, Dictionary<int, int[]>> _keyedLists = new();
        private readonly Dictionary<string, int[]> _fixedLists = new();

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
            LoadExeEffects();
        }

        // What #damage is for each #effect, read from the game's own effect code (embedded
        // tools/dom6exe/data/spell-effects-6.37.json, "dom6exe.py spelleffects"; README "Spell
        // effects and #damage"). For every effect it lists it decides over spell_effect_types.json
        // (the inspector's tables, which had 10089/10114, 10085 and 133 wrong): monster,
        // monster_or_tag, unique_or_monster_or_tag, enchantment, event, site, or a plain value.
        private readonly Dictionary<int, string> _exeArgument = new();
        private readonly List<(int From, int To, string Argument)> _exeRanges = new();
        // 10089/10114 (unique_pick 0x1401cea80): 1-99 is a key into the game's lists of uniques,
        // this and up the monster itself
        private int _uniqueMonsterFrom = 100;
        // effects the game reads but has no case for (10086: castlabspell has no branch)
        private readonly HashSet<int> _exeNothingHappens = new();
        private const string ExeResource = "Dom5Edit.GameData.spell-effects.json";

        private void LoadExeEffects()
        {
            try
            {
                using var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(ExeResource);
                if (stream == null)
                    return;
                using var doc = JsonDocument.Parse(stream);
                var root = doc.RootElement;
                if (root.TryGetProperty("effects", out var effects))
                    foreach (var e in effects.EnumerateObject())
                        if (int.TryParse(e.Name, out int n) && e.Value.TryGetProperty("argument", out var a))
                        {
                            _exeArgument[n] = a.GetString() ?? "";
                            if (e.Value.TryGetProperty("note", out var note) && (note.GetString() ?? "").Contains("nothing happens"))
                                _exeNothingHappens.Add(n);
                        }
                if (root.TryGetProperty("ranges", out var ranges))
                    foreach (var r in ranges.EnumerateArray())
                        _exeRanges.Add((r.GetProperty("from").GetInt32(), r.GetProperty("to").GetInt32(), r.GetProperty("argument").GetString() ?? ""));
                if (root.TryGetProperty("unique_monster_from", out var u) && u.TryGetInt32(out int from))
                    _uniqueMonsterFrom = from;
            }
            catch (Exception)
            {
                // without the table the inspector-derived classification below applies
            }
        }

        /// <summary>
        /// What the game does with #damage for this effect (null: not in the exe table).
        /// "unhandled" for combat effects 1000-9999: the game has no case for them (below).
        /// </summary>
        public string? ExeArgument(int effect)
        {
            if (VanillaLoader.GameVersion != GameVersion.Dom6)
                return null;            // read from Dominions6.exe: not Dom5's effects
            if (effect >= 1000 && effect < 10000)
                return "unhandled";
            if (_exeArgument.TryGetValue(effect, out var a))
                return a;
            foreach (var (from, to, arg) in _exeRanges)
                if (effect >= from && effect <= to)
                    return arg;
            return null;
        }

        /// <summary>
        /// Why the game does nothing with this #effect, for the report (null: it has a case for
        /// it). Read from Dominions6.exe 6.37: a combat effect 1000-9999 is passed on as written
        /// (spellblastsquare 0x1401cb5cf), clouds are only 144-150 (blastsquare 0x1401b786c),
        /// hitunit compares the effect only with numbers up to 166, and nothing in the battle code
        /// divides it by 1000 (the AI's estimate reads effect % 1000: evalspell 0x1401c3b08). Its
        /// #damage is then plain damage, not a unit. A ritual effect castlabspell has no branch for
        /// (it compares one number at a time: the table has every one it handles).
        /// </summary>
        public string? NotHandledWhy(int effect)
        {
            if (VanillaLoader.GameVersion != GameVersion.Dom6 || _exeArgument.Count == 0)
                return null;
            if (effect >= 1000 && effect < 10000)
                return $"#effect {effect}: the game has no such combat effect. It passes the number on as written, and its battle code " +
                       "only acts on combat effects up to 166 (and 500-699); clouds are effects 144-150. So the spell does nothing in battle, " +
                       $"and its #damage is a plain number. (Perhaps an older way of writing {effect / 1000} rounds of effect {effect % 1000}; " +
                       "Dom6's clouds use effects 144-150 with #aoe.)";
            if (_exeNothingHappens.Contains(effect))
                return $"#effect {effect}: the game reads it but has no case for it (no branch in the ritual code): the spell does nothing";
            if (effect >= 10000 && ExeArgument(effect) == null)
                return $"#effect {effect}: the game has no such ritual effect (the ritual code has no case for it): the spell does nothing";
            return null;
        }

        /// <summary>For 10089/10114: #damage 1-99 is a key into the game's lists of uniques, not a monster.</summary>
        public bool IsUniqueListKey(int effect, long damage) =>
            ExeArgument(effect) == "unique_or_monster_or_tag" && damage >= 1 && damage < _uniqueMonsterFrom;

        /// <summary>The exe table knows this effect (its reading decides).</summary>
        public bool KnowsEffect(int effect) => ExeArgument(effect) != null;

        /// <summary>#damage is a site number (10154: addfeatnr 0x1402ac0b0).</summary>
        public bool IsSiteEffect(int effect) => ExeArgument(effect) == "site";

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
            LoadUnitLists(root);

            if (root.TryGetProperty("effect_types", out var effectTypes))
            {
                foreach (var effect in effectTypes.EnumerateObject())
                {
                    if (!int.TryParse(effect.Name, out int effectNum)) continue;

                    if (effect.Value.TryGetProperty("argument_type", out var argType))
                    {
                        string argTypeStr = argType.GetString() ?? "";
                        // the file says what each effect it lists reads: the built-in (Dom5) guess no
                        // longer applies to it (#damage 3 of Bind Heliophagus is list 3, not monster 3)
                        _summonEffects.Remove(effectNum);
                        _enchantEffects.Remove(effectNum);
                        _eventEffects.Remove(effectNum);
                        _bitmaskEffects.Remove(effectNum);
                        var lookup = effect.Value.TryGetProperty("lookup", out var lk) ? lk.GetString() : null;
                        if (argTypeStr is "unique_summon_key" or "terrain_summon_key" or "special_summon")
                            _listEffects[effectNum] = (argTypeStr, lookup);
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

        /// <summary>The game's unit lists some effects pick from (uniqueSummon, terrainSummon, special_summon_arrays).</summary>
        private void LoadUnitLists(JsonElement root)
        {
            foreach (var name in new[] { "uniqueSummon", "terrainSummon" })
            {
                if (!root.TryGetProperty(name, out var table) || table.ValueKind != JsonValueKind.Object)
                    continue;
                var keyed = new Dictionary<int, int[]>();
                foreach (var entry in table.EnumerateObject())
                    if (int.TryParse(entry.Name, out int key) && entry.Value.TryGetProperty("units", out var units))
                        keyed[key] = units.EnumerateArray().Select(u => u.GetInt32()).ToArray();
                _keyedLists[name] = keyed;
            }
            if (root.TryGetProperty("special_summon_arrays", out var arrays) && arrays.ValueKind == JsonValueKind.Object)
                foreach (var entry in arrays.EnumerateObject())
                    _fixedLists[entry.Name] = entry.Value.EnumerateArray().Select(u => u.GetInt32()).ToArray();
        }

        /// <summary>
        /// The units an effect's #damage stands for when it's a key into one of the game's lists
        /// (Bind Heliophagus: list 3, the four Heliophagi), or a fixed list (Tartarian Gate); empty
        /// otherwise. "Used by" lists the spell under each.
        /// </summary>
        public IReadOnlyList<int> UnitsPicked(int effect, long damage)
        {
            if (effect > 10000) effect -= 10000;
            // Enchant Battlefield with 43 calls the ghost ship armada (the effect table's note)
            if (effect == 81 && damage == 43 && _fixedLists.TryGetValue("ghostShipArmada", out var ships))
                return ships;
            if (!_listEffects.TryGetValue(effect, out var list))
                return Array.Empty<int>();
            if (list.Kind == "special_summon")
                return list.Lookup != null && _fixedLists.TryGetValue(list.Lookup, out var units) ? units : Array.Empty<int>();
            var table = list.Kind == "unique_summon_key" ? "uniqueSummon" : "terrainSummon";
            return _keyedLists.TryGetValue(list.Lookup ?? table, out var keyed) || _keyedLists.TryGetValue(table, out keyed)
                ? keyed.TryGetValue((int)damage, out var picked) ? picked : Array.Empty<int>()
                : Array.Empty<int>();
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
            // (not 89/114, whose #damage is a key into a list of uniques, nor 76/120/127, which pick
            // from fixed lists, nor 68: spell_effect_types.json says so for those it lists)
            _summonEffects = new HashSet<int> { 1, 21, 26, 31, 37, 38, 43, 50, 54, 62, 93, 119, 126, 130, 137, 141 };

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

        // the exe table first (by the exact effect number: a combat effect and its ritual + 10000
        // differ, e.g. 133 is a battlefield enchantment and 10085 a global one); the inspector's
        // classification (by effect % 10000) only for effects the table doesn't know
        public bool IsSummonEffect(int effect)
        {
            if (ExeArgument(effect) is string a)
                return a is "monster" or "monster_or_tag" or "unique_or_monster_or_tag";
            if (effect > 10000) effect -= 10000;
            return _summonEffects.Contains(effect);
        }

        public bool IsEnchantEffect(int effect)
        {
            if (ExeArgument(effect) is string a)
                return a == "enchantment";
            if (effect > 10000) effect -= 10000;
            return _enchantEffects.Contains(effect);
        }

        public bool IsEventEffect(int effect)
        {
            if (ExeArgument(effect) is string a)
                return a == "event";
            if (effect > 10000) effect -= 10000;
            return _eventEffects.Contains(effect);
        }

        public bool IsBitmaskEffect(int effect)
        {
            if (ExeArgument(effect) is string a)
                return a == "bitmask";
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
