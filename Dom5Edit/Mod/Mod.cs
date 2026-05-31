using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Edit.Validation;

namespace Dom5Edit
{
    public class Mod
    {
        // Parser and exporter instances for delegation
        private readonly ModParser _parser;
        private readonly ModExporter _exporter;

        public string ModName { get; set; }
        public string ModFileName { get { return Path.GetFileName(FullFilePath); } }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Version { get; set; }
        public string DomVersion { get; set; }

        private List<string> _dependencies = new List<string>();
        public List<Mod> Dependencies { get; set; } = new List<Mod>();
        public List<string> DisabledNations = new List<string>();

        /// <summary>
        /// Issues detected during parsing (duplicates, invalid commands, etc.)
        /// </summary>
        private List<ParseIssue> _parseIssues = new List<ParseIssue>();
        public IReadOnlyList<ParseIssue> ParseIssues => _parseIssues.AsReadOnly();

        /// <summary>
        /// Adds a parse issue to the collection.
        /// </summary>
        public void AddParseIssue(ParseIssueType issueType, string message)
        {
            _parseIssues.Add(new ParseIssue(LineNumber, issueType, message));
        }

        public void AddParseIssue(ParseIssueType issueType, string message, int lineNumber)
        {
            _parseIssues.Add(new ParseIssue(lineNumber, issueType, message));
        }

        /// <summary>
        /// Clears all parse issues (call before re-parsing).
        /// </summary>
        public void ClearParseIssues()
        {
            _parseIssues.Clear();
        }

        public Dictionary<Command, EntityType> CommandEntityMap { get; } = new Dictionary<Command, EntityType>()
        {
            { Command.NEWMONSTER, EntityType.MONSTER },
            { Command.SELECTMONSTER, EntityType.MONSTER },
            { Command.NEWWEAPON, EntityType.WEAPON },
            { Command.SELECTWEAPON, EntityType.WEAPON },
            { Command.NEWARMOR, EntityType.ARMOR },
            { Command.SELECTARMOR, EntityType.ARMOR },
            { Command.NEWSPELL, EntityType.SPELL },
            { Command.SELECTSPELL, EntityType.SPELL },
            { Command.NEWITEM, EntityType.ITEM },
            { Command.SELECTITEM, EntityType.ITEM },
            { Command.NEWEVENT, EntityType.EVENT },
            { Command.NEWMERC, EntityType.MERCENARY },
            { Command.NEWNATION, EntityType.NATION },
            { Command.SELECTNAMETYPE, EntityType.NAMETYPE },
            { Command.SELECTNATION, EntityType.NATION },
            { Command.SELECTPOPTYPE, EntityType.POPTYPE },
            { Command.SELECTSITE, EntityType.SITE },
            { Command.NEWSITE, EntityType.SITE },
            { Command.SELECTEVENT, EntityType.EVENT },
            { Command.SELECTBLESS, EntityType.BLESS },
            { Command.NEWTEMPLATE, EntityType.TEMPLATE },
        };
        private Dictionary<Type, EntityType> TypeEntityMap { get; } = new Dictionary<Type, EntityType>()
        {
            { typeof(Monster), EntityType.MONSTER },
            { typeof(Item), EntityType.ITEM },
            { typeof(Spell), EntityType.SPELL },
            { typeof(Weapon), EntityType.WEAPON },
            { typeof(Nation), EntityType.NATION },
            { typeof(Armor), EntityType.ARMOR },
            { typeof(Mercenary), EntityType.MERCENARY },
            { typeof(Site), EntityType.SITE },
            { typeof(Event), EntityType.EVENT },
            { typeof(Poptype), EntityType.POPTYPE },
            { typeof(Nametype), EntityType.NAMETYPE },
            { typeof(Montag), EntityType.MONTAG },
            { typeof(RestrictedItem), EntityType.RESTRICTED_ITEM },
            { typeof(Enchantment), EntityType.ENCHANTMENT },
            { typeof(EventCode), EntityType.EVENT_CODE },
            { typeof(EventVar), EntityType.EVENT_VAR },
            { typeof(EventCodeEffect), EntityType.EVENT_CODE_EFFECT },
            { typeof(Bless), EntityType.BLESS },
            { typeof(Template), EntityType.TEMPLATE },
        };

        public Dictionary<EntityType, EntitySet<IDEntity>> Database { get; } = new Dictionary<EntityType, EntitySet<IDEntity>>()
        {
            { EntityType.WEAPON, new EntitySet<IDEntity>() { START_ID = WEAPON_START_ID, END_ID = WEAPON_END_ID } },
            { EntityType.ARMOR, new EntitySet<IDEntity>() { START_ID = ARMOR_START_ID, END_ID = ARMOR_END_ID } },
            { EntityType.MONSTER, new EntitySet<IDEntity>() { START_ID = MONSTER_START_ID, END_ID = MONSTER_END_ID } },
            { EntityType.NAMETYPE, new EntitySet<IDEntity>() { START_ID = NAMETYPE_START_ID, END_ID = NAMETYPE_END_ID } },
            { EntityType.SITE, new EntitySet<IDEntity>() { START_ID = SITE_START_ID, END_ID = SITE_END_ID } },
            { EntityType.NATION, new EntitySet<IDEntity>() { START_ID = NATION_START_ID, END_ID = NATION_END_ID } },
            { EntityType.SPELL, new EntitySet<IDEntity>() { START_ID = SPELL_START_ID, END_ID = SPELL_END_ID } },
            { EntityType.ITEM, new EntitySet<IDEntity>() { START_ID = ITEM_START_ID, END_ID = ITEM_END_ID } },
            { EntityType.POPTYPE, new EntitySet<IDEntity>() {  } },
            { EntityType.MERCENARY, new EntitySet<IDEntity>() { } },
            { EntityType.EVENT, new EntitySet<IDEntity>() { START_ID = EVENT_START_ID } },
            { EntityType.EVENT_VAR, new EntitySet<IDEntity>() { START_ID = EVENT_VAR_START_ID } },
            { EntityType.BLESS, new EntitySet<IDEntity>() { } },
            { EntityType.TEMPLATE, new EntitySet<IDEntity>() { } },
        };
        /// <summary>
        /// Try to get an Entity from the database.
        /// </summary>
        /// <param name="t">The entity type to retrieve.</param>
        /// <param name="i">The ID of the entity.</param>
        /// <param name="s">The name of the entity.</param>
        /// <param name="entity">The returned entity.</param>
        /// <returns>True if the entity exists, or false otherwise.</returns>
        public bool TryGet(EntityType t, int i, string s, out IDEntity entity)
        {
            var set = Database[t];
            foreach (var m in Dependencies)
            {
                if (m.Database[t].TryGet(i, s, out entity))
                {
                    return true;
                }
            }
            return set.TryGet(i, s, out entity);
        }

        public Dictionary<EntityType, DependentEntitySet> Dependents { get; } = new Dictionary<EntityType, DependentEntitySet>()
        {
            {  EntityType.MONTAG, new DependentEntitySet() { START_ID = MONTAG_START_ID } },
            {  EntityType.RESTRICTED_ITEM, new DependentEntitySet() { START_ID = RESTRICTED_ITEM_START_ID } },
            {  EntityType.ENCHANTMENT, new DependentEntitySet() { START_ID = ENCHANTMENT_START_ID } },
            {  EntityType.EVENT_CODE, new DependentEntitySet() { START_ID = EVENT_CODE_START_ID, ID_DOWN = true } },
            {  EntityType.EVENT_CODE_EFFECT, new DependentEntitySet() { START_ID = EVENT_CODE_EFFECT_START_ID } },
            {  EntityType.EVENT_VAR, new DependentEntitySet() { START_ID = EVENT_VAR_START_ID } },
        };

        public List<SpellDamage> SpellDamages = new List<SpellDamage>();
        public List<IDEntity> Events = new List<IDEntity>();
        public List<int> VanillaMageReferences = new List<int>();

        internal static int MONSTER_START_ID = 5000; // Dom5: 3486
		internal static int SITE_START_ID = 1700; // Dom5: 1164
		internal static int EVENT_START_ID = 4000; // Dom5: 6000
		internal static int ARMOR_START_ID = 400; // Dom5: 251
		internal static int WEAPON_START_ID = 1000; // Dom5: 763
		internal static int ITEM_START_ID = 700; // Dom5: 446
		internal static int SPELL_START_ID = 2000; // Dom5: 1177
		internal static int NAMETYPE_START_ID = 170; // Dom5: 170
		internal static int NATION_START_ID = 150; // Dom5: 109
		internal static int MONTAG_START_ID = 1000; // Dom5: 1000
		internal static int RESTRICTED_ITEM_START_ID = 1; // Dom5: 1
		internal static int ENCHANTMENT_START_ID = 200; // Dom5: 106
		internal static int EVENT_CODE_START_ID = -300; // Dom5: -300
		internal static int EVENT_VAR_START_ID = 1; // Dom5: n/a
		internal static int EVENT_CODE_EFFECT_START_ID = 50; // Dom5: 14
			
		internal static int MONSTER_END_ID = 19999; // Dom5: 8999
		internal static int SITE_END_ID = 3999; // Dom5: 1999
		internal static int ARMOR_END_ID = 1999; // Dom5: 999
		internal static int WEAPON_END_ID = 3999; // Dom5: 1999
		internal static int ITEM_END_ID = 1999; // Dom5: 999
		internal static int SPELL_END_ID = 7999; // Dom5: 3999
		internal static int NAMETYPE_END_ID = 399; // Dom5: 299
		internal static int NATION_END_ID = 499; // Dom5: 249

        private Entity _currentEntity = null;

        public bool LineWasTrimmed { get; set; }

        public Mod()
        {
            _parser = new ModParser();
            _exporter = new ModExporter();
            Init();
            foreach (EntitySet<IDEntity> set in Database.Values)
            {
                set.Init();
            }
        }

        public Mod(string filePath)
        {
            _parser = new ModParser();
            _exporter = new ModExporter();
            Init();
            this.FullFilePath = filePath;
        }

        private void Init()
        {
            foreach (var kvp in Database)
            {
                kvp.Value.Parent = this;
            }
            SetupParserCallbacks();
        }

        private void SetupParserCallbacks()
        {
            _parser.OnCommand = cmd =>
            {
                LineNumber = cmd.LineNumber;
                LineWasTrimmed = _parser.LineWasTrimmed;
                HandleParsedCommand(cmd.Command, cmd.Value, cmd.Comment);
            };
            _parser.OnLog = (line, msg) =>
            {
                LineNumber = line;
                string fullMsg;
                if (_currentEntity != null)
                    fullMsg = $"Invalid, incorrectly spelled, or nonexistent command for: {_currentEntity.GetType().Name} - {msg}";
                else
                    fullMsg = msg;
                Log(fullMsg);
                AddParseIssue(ParseIssueType.InvalidCommand, fullMsg);
            };
        }

        public int LineNumber { get; private set; } = 0;
        private string logFile;
        public bool Logging { get; set; }
        public string FolderPath { get { return Path.GetDirectoryName(FullFilePath); } }
        public string FullFilePath { get; set; }
        public bool IsLoaded { get; internal set; }

        public void Log(string s)
        {
            if (!this.Logging) return;
            if (string.IsNullOrEmpty(logFile))
            {
                logFile = System.IO.Path.Combine(FolderPath, this.ModName + "-log.txt");
                File.Delete(logFile); //clear out an old log
            }
            using (StreamWriter writer = File.AppendText(logFile))
            {
                if (LineNumber != -1) writer.WriteLine("Line: " + LineNumber + " - " + s);
                else writer.WriteLine("Error: " + s);
            }
        }

        public void Parse(string dmFile)
        {
            int indexOfDotDM = dmFile.IndexOf(".dm");
            if (indexOfDotDM != -1)
            {
                logFile = dmFile.Substring(0, indexOfDotDM) + "-log.txt";
                File.Delete(logFile); //clear out an old log
            }

            using (StreamReader sr = File.OpenText(dmFile))
            {
                read_stream(sr);
            }
        }

        internal void read_stream(StreamReader sr)
        {
            // Delegate to ModParser
            _parser.Parse(sr);
            LineWasTrimmed = false;
            LineNumber = -1;
        }

        /// <summary>
        /// Handles a parsed command, routing mod metadata to properties and entity commands to Parse().
        /// </summary>
        private void HandleParsedCommand(Command c, string value, string comment)
        {
            switch (c)
            {
                case Command.MODNAME:
                    ModName = value;
                    break;
                case Command.DESCRIPTION:
                    Description = value;
                    break;
                case Command.VERSION:
                    Version = value;
                    break;
                case Command.DOMVERSION:
                    DomVersion = value;
                    break;
                case Command.ICON:
                    Icon = value;
                    break;
                default:
                    Parse(c, value, comment);
                    break;
            }
        }

        #region Legacy Parsing Methods (Dom5 TSV loading only)
        // These methods delegate to ModParser. Used by VanillaLoader for Dom5 TSV loading.
        // Can be removed when Dom5 support is no longer needed.

        [Obsolete("Use ModParser directly. Only kept for Dom5 TSV loading in VanillaLoader.")]
        public bool HasCommandOnLine(string s) => _parser.HasCommandOnLine(s);

        [Obsolete("Use ModParser directly. Only kept for Dom5 TSV loading in VanillaLoader.")]
        public int GetNextCommandIndex(string s) => _parser.GetNextCommandIndex(s);

        [Obsolete("Use ModParser directly. Only kept for Dom5 TSV loading in VanillaLoader.")]
        public void ProcessStringToLine(string s) => _parser.ProcessStringToLine(s);

        [Obsolete("Use ModParser directly. Only kept for Dom5 TSV loading in VanillaLoader.")]
        public void ProcessLine(string s) => _parser.ProcessLine(s);

        #endregion

        public bool HasDependencies()
        {
            _dependencies = ModParser.ScanDependencies(this.FullFilePath);
            return _dependencies?.Count > 0;
        }

        /*
         * Entity processing goes here
         */
        public void Parse(Command c, string val, string comment)
        {
            switch (c)
            {
                case Command.NEWMONSTER:
                    _currentEntity = NewEntity<Monster>(val, comment);
                    break;
                case Command.NEWARMOR:
                    _currentEntity = NewEntity<Armor>(val, comment);
                    break;
                case Command.NEWWEAPON:
                    _currentEntity = NewEntity<Weapon>(val, comment);
                    break;
                case Command.NEWSITE:
                    _currentEntity = NewEntity<Site>(val, comment);
                    break;
                case Command.NEWNATION:
                    _currentEntity = NewEntity<Nation>(val, comment);
                    break;
                case Command.NEWITEM:
                    _currentEntity = NewEntity<Item>(val, comment);
                    break;
                case Command.NEWSPELL:
                    _currentEntity = NewEntity<Spell>(val, comment);
                    break;
                case Command.NEWMERC:
                    _currentEntity = NewEntity<Mercenary>(val, comment);
                    break;
                case Command.NEWEVENT:
                    _currentEntity = NewEntity<Event>(val, comment);
                    break;
                case Command.SELECTEVENT:
                    _currentEntity = SelectEntity<Event>(CommandEntityMap[c], val, comment);
                    break;
                case Command.SELECTMONSTER:
                    _currentEntity = SelectEntity<Monster>(CommandEntityMap[c], val, comment);
                    break;
                case Command.SELECTARMOR:
                    _currentEntity = SelectEntity<Armor>(CommandEntityMap[c], val, comment);
                    break;
                case Command.SELECTWEAPON:
                    _currentEntity = SelectEntity<Weapon>(CommandEntityMap[c], val, comment);
                    break;
                case Command.SELECTNAMETYPE:
                    _currentEntity = SelectEntity<Nametype>(CommandEntityMap[c], val, comment);
                    break;
                case Command.SELECTSITE:
                    _currentEntity = SelectEntity<Site>(CommandEntityMap[c], val, comment);
                    break;
                case Command.SELECTNATION:
                    _currentEntity = SelectEntity<Nation>(CommandEntityMap[c], val, comment);
                    break;
                case Command.SELECTITEM:
                    _currentEntity = SelectEntity<Item>(CommandEntityMap[c], val, comment);
                    break;
                case Command.SELECTSPELL:
                    _currentEntity = SelectEntity<Spell>(CommandEntityMap[c], val, comment);
                    break;
                case Command.SELECTPOPTYPE:
                    _currentEntity = SelectEntity<Poptype>(CommandEntityMap[c], val, comment);
                    break;
                case Command.SELECTBLESS:
                    _currentEntity = SelectEntity<Bless>(CommandEntityMap[c], val, comment);
                    break;
                case Command.NEWTEMPLATE:
                    _currentEntity = NewEntity<Template>(val, comment);
                    break;
                case Command.END:
                    _currentEntity?.SetEndComment(comment);
                    _currentEntity = null;
                    break;
                default:
                    if (_currentEntity != null) _currentEntity.Parse(c, val, comment); //assume the command is relevant for the current entity
                    //else build list of pre-entity comments, to restore before the next entity on export?
                    break; //nothing
            }
        }

        public void ResolveDependencies(List<Mod> mods)
        {
            //Dependencies.Add(VanillaLoader.Vanilla);
            foreach (var file in _dependencies)
            {
                foreach (var m in mods)
                {
                    if (m.ModFileName.EqualsIgnoreCase(file) || m.ModName.EqualsIgnoreCase(file))
                    {
                        Dependencies.Add(m);
                        break;
                    }
                }
                throw new FileNotFoundException("Error: Missing a dependency required for " + this.ModName + ". Missing Mod: " + file);
            }
        }

        public void ResolveDependencies()
        {
            Dependencies.Add(VanillaLoader.Vanilla);
        }

        public void Resolve()
        {
            foreach (var set in Database.Values)
            {
                set.Resolve();
            }

            foreach (var kvp in Events)
            {
                kvp.Resolve();
            }
            foreach (var kvp in SpellDamages)
            {
                kvp.Resolve();
            }

            foreach (var kvp in Dependents)
            {
                kvp.Value.Resolve(kvp.Key, Dependencies);
            }
            IsLoaded = true;
        }

        public void Map()
        {
            foreach (var kvp in Database)
            {
                kvp.Value.Map();
            }
        }


        #region EXPORT

        /// <summary>
        /// Exports to the folder specified. If no folder is specified, defaults to the original mod file path.
        /// </summary>
        /// <param name="file">Directory and file name to export into</param>
        /// <param name="overwrite">Whether to overwrite an existing file</param>
        public void Export(string file = null, bool overwrite = true)
        {
            if (file == null) file = FullFilePath;
            else
            {
                this.FullFilePath = file;
            }
            _exporter.Export(this, file, overwrite);
        }

        public void Export(StreamWriter writer)
        {
            _exporter.Export(this, writer);
        }

        #endregion

        #region IMPORT
        public static Mod Import(string fullfile, bool log = false)
        {
            Mod m = new Mod(fullfile);
            m.Load(log);
            m.ResolveDependencies();
            m.Resolve();
            return m;
        }

        public void Load(bool log = false)
        {
            Logging = log;
            Parse(FullFilePath);
        }

        public void OpenLog()
        {
            int indexOfDotDM = this.FullFilePath.IndexOf(".dm");
            if (indexOfDotDM != -1)
            {
                string logFile = this.FullFilePath.Substring(0, indexOfDotDM) + "-log.txt";
                if (File.Exists(logFile)) System.Diagnostics.Process.Start(logFile);
            }
        }

        #endregion

        public DependentEntity AddDependent(EntityType t, int ID)
        {
            if (ID == -1) return null;
            if (Dependents[t].TryGetValue(ID, out var m))
            {
                return m;
            }
            else
            {
                var ret = new DependentEntity(ID);
                Dependents[t].Add(ID, ret);
                return ret;
            }
        }

        public int GetStartID(EntityType t)
        {
            return Database[t].START_ID;
        }

        public void DisableMages(List<string> disabledNations)
        {
            List<int> disabledIDs = new List<int>();
            List<int> referencedIDs = this.VanillaMageReferences;

            foreach (var nation in disabledNations)
            {
                if (VanillaMageIDs.TryGetIDList(nation, out var ids))
                {
                    foreach (var id in ids)
                    {
                        if (!referencedIDs.Contains(id)) disabledIDs.Add(id);
                    }
                }
            }

            foreach (var id in disabledIDs)
            {
                Database[EntityType.MONSTER].Disable(EntityType.MONSTER, id, this);
            }
        }

        #region NEW / SELECT COMMANDS
        public IDEntity NewEntity<T>(string val, string comment, bool selected = false) where T : IDEntity, new()
        {
            // Check if entity already exists (e.g., from earlier #selectmonster before #newmonster)
            // This handles mods that use #selectmonster to add properties before #newmonster defines the entity
            if (int.TryParse(val, out int existingId) && existingId > 0)
            {
                EntityType et = GetEntityType(typeof(T));
                if (this.TryGet(et, existingId, null, out IDEntity existing))
                {
                    // Entity already exists - use it instead of creating new
                    // Update Selected flag: #newmonster means this is a new entity definition (Selected = false)
                    existing.Selected = selected;
                    return existing;
                }
            }

            // Entity doesn't exist - create new
            IDEntity id = new T();
            id.Assign(val, comment, this, selected);
            return id;
        }

        public void AddEntity(Type t, int i, string s, IDEntity entity)
        {
            EntityType et = GetEntityType(t);
            Database[et].Add(i, s, entity);
        }

        public void AddEntity<T>(int i, string s, IDEntity entity)
        {
            EntityType et = GetEntityType(typeof(T));
            Database[et].Add(i, s, entity);
        }

        public EntityType GetEntityType(Type t)
        {
            return TypeEntityMap[t];
        }

        public IDEntity SelectEntity<T>(EntityType et, string val, string comment) where T : IDEntity, new()
        {
            if (int.TryParse(val, out int id) && this.TryGet(et, id, val, out IDEntity entity))
            {
                return entity;
            }
            else
            {
                return NewEntity<T>(val, comment, true);
            }
        }
        #endregion

    }
}
