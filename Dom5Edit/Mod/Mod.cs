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
            // By ID: dependencies (vanilla) first, then this mod
            foreach (var m in Dependencies)
            {
                if (m.Database[t].TryGetValue(i, out entity))
                {
                    return true;
                }
            }
            if (set.TryGetValue(i, out entity)) return true;

            // By name: when vanilla and mods share a name, the game takes the lowest ID
            IDEntity best = null;
            foreach (var m in Dependencies)
            {
                if (m.Database[t].TryGetValueNamed(s, out IDEntity e) && (best == null || EntitySet<IDEntity>.IsLowerID(e.ID, best)))
                    best = e;
            }
            if (set.TryGetValueNamed(s, out IDEntity own) && (best == null || EntitySet<IDEntity>.IsLowerID(own.ID, best)))
                best = own;
            entity = best;
            return best != null;
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
        private SourceBlock? _currentBlock = null;

        /// <summary>
        /// The parsed file's entity blocks in file order (see SourceBlock). Saving writes them back
        /// in this order; entities with no block (new in the session) follow.
        /// </summary>
        public List<SourceBlock> SourceBlocks { get; } = new List<SourceBlock>();

        /// <summary>
        /// The properties live in entities when parsing finished. A block property that was live
        /// then and isn't any more was removed by an edit; one that wasn't live then was taken out
        /// by a later clear or copy in the file, and is still written.
        /// </summary>
        internal HashSet<Property> PropertiesAfterParse { get; private set; } = new HashSet<Property>(ReferenceEqualityComparer.Instance);

        /// <summary>
        /// Lines before the first block, as read: header commands (#modname, ...) and lines with no
        /// command. Written as read while the header fields are unchanged.
        /// </summary>
        internal List<(string Text, bool IsHeaderCommand)> Preamble { get; } = new List<(string, bool)>();

        /// <summary>Lines with no command after the last block, as read.</summary>
        internal List<string> TrailingTrivia { get; } = new List<string>();

        /// <summary>Whether a line was read from the mod's file (not added in the editor).</summary>
        public bool IsFromFile(Property p) => PropertiesAfterParse.Contains(p);

        /// <summary>The header fields when parsing finished (see HeaderUnchanged).</summary>
        private (string?, string?, string?, string?, string?) _headerAtParse;

        /// <summary>Whether #modname/#description/#icon/#version/#domversion are as parsed.</summary>
        internal bool HeaderUnchanged => _headerAtParse == (ModName, Description, Icon, Version, DomVersion);

        private string _currentRawText = "";

        // the line with several commands the current one came from, by the parser's number for it
        private LineGroup? _currentLineGroup;
        private readonly Dictionary<int, LineGroup> _lineGroups = new Dictionary<int, LineGroup>();
        private bool _baselineTaken;
        private readonly List<string> _pendingTrivia = new List<string>();

        /// <summary>A line with no command: kept where it was (block, before a block, or end of file).</summary>
        private void AddTrivia(string text)
        {
            if (_currentBlock != null)
                _currentBlock.Trivia.Add((_currentBlock.Properties.Count, text));
            else if (SourceBlocks.Count == 0)
                Preamble.Add((text, false));
            else
                _pendingTrivia.Add(text);
        }

        /// <summary>
        /// Save in the parsed file's block order (the default). False writes each entity as one
        /// block, by type and ID.
        /// </summary>
        public bool PreserveSourceOrder { get; set; } = true;

        /// <summary>
        /// Write unedited lines as read (the default). False regenerates every line, still in file
        /// order: a test of the export itself, which the original text would otherwise hide.
        /// </summary>
        public bool KeepOriginalText { get; set; } = true;

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
            _parser.OnTrivia = AddTrivia;
            _parser.OnCommand = cmd =>
            {
                _currentRawText = cmd.RawText ?? "";
                _currentLineGroup = cmd.LineGroup != 0 && cmd.LineText != null
                    ? (_lineGroups.TryGetValue(cmd.LineGroup, out var g) ? g : _lineGroups[cmd.LineGroup] = new LineGroup(cmd.LineText, cmd.LineCommands))
                    : null;
                LineNumber = cmd.LineNumber;
                LineWasTrimmed = _parser.LineWasTrimmed;
                HandleParsedCommand(cmd.Command, cmd.Value, cmd.Comment);
            };
            _parser.OnGameValue = (label, value) =>
            {
                if (_currentEntity is IDEntity entity)
                    entity.GameValues.Add(new GameValue(label, value));
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
                if (Logging)
                    File.Delete(logFile); //clear out an old log (only when this run writes one)
            }

            ReadFileForm(dmFile);
            _parser.NewLine = SourceNewLine ?? Environment.NewLine;
            using (StreamReader sr = File.OpenText(dmFile))
            {
                read_stream(sr);
            }
        }

        /// <summary>The file's line break as read ("\r\n" or "\n"), or null for a mod made in the editor.</summary>
        public string? SourceNewLine { get; private set; }

        /// <summary>Whether the file as read ends with a line break (a save keeps it so).</summary>
        public bool SourceEndsWithNewLine { get; private set; } = true;

        /// <summary>Whether the file as read starts with a UTF-8 byte order mark (a save keeps it).</summary>
        public bool SourceHasBom { get; private set; }

        /// <summary>The file's form: its line breaks, a last one or not, a byte order mark.</summary>
        private void ReadFileForm(string dmFile)
        {
            try
            {
                using var fs = File.OpenRead(dmFile);
                var head = new byte[Math.Min(fs.Length, 1 << 16)];
                int n = fs.Read(head, 0, head.Length);
                SourceHasBom = n >= 3 && head[0] == 0xEF && head[1] == 0xBB && head[2] == 0xBF;
                int lf = Array.IndexOf(head, (byte)'\n', 0, n);
                SourceNewLine = lf > 0 && head[lf - 1] == '\r' ? "\r\n" : lf >= 0 ? "\n" : null;
                if (fs.Length > 0)
                {
                    fs.Seek(-1, SeekOrigin.End);
                    SourceEndsWithNewLine = fs.ReadByte() == '\n';
                }
            }
            catch (IOException)
            {
                // read below, and reported there
            }
        }

        /// <summary>While the file is read: lines added then are the file's (not edits).</summary>
        internal bool IsParsing { get; private set; }

        internal void read_stream(StreamReader sr)
        {
            IsParsing = true;
            try
            {
                read_stream_core(sr);
            }
            finally
            {
                IsParsing = false;
            }
        }

        private void read_stream_core(StreamReader sr)
        {
            // Delegate to ModParser
            _parser.Parse(sr);
            LineWasTrimmed = false;
            LineNumber = -1;
            PropertiesAfterParse = new HashSet<Property>(
                Database.Values.SelectMany(set => set.GetFullList()).SelectMany(e => e.Properties), ReferenceEqualityComparer.Instance);
            if (_currentBlock != null && _currentBlock.RawEnd == null)
                _currentBlock.EndsWithoutEnd = true; // the file ends inside it
            TrailingTrivia.AddRange(_pendingTrivia);
            _pendingTrivia.Clear();
            _headerAtParse = (ModName, Description, Icon, Version, DomVersion);
        }

        /// <summary>
        /// Handles a parsed command, routing mod metadata to properties and entity commands to Parse().
        /// </summary>
        private void HandleParsedCommand(Command c, string value, string comment)
        {
            if (c == Command.MODNAME || c == Command.DESCRIPTION || c == Command.VERSION || c == Command.DOMVERSION || c == Command.ICON)
            {
                // mod header: part of the preamble before the first block, else kept in place
                if (SourceBlocks.Count == 0 && _currentBlock == null)
                    Preamble.Add((_currentRawText, true));
                else
                    AddTrivia(_currentRawText);
            }
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
                    if (_currentBlock != null)
                        _currentBlock.RawEnd = _currentRawText;
                    else
                        AddTrivia(_currentRawText); // a stray #end
                    _currentBlock = null;
                    break;
                default:
                    if (_currentEntity != null) _currentEntity.Parse(c, val, comment); //assume the command is relevant for the current entity
                    if (_currentBlock != null && _currentEntity is IDEntity parsedInto && parsedInto.LastParsedProperty != null)
                    {
                        parsedInto.LastParsedProperty.RawText = _currentRawText;
                        if (_currentLineGroup != null)
                        {
                            parsedInto.LastParsedProperty.Line = _currentLineGroup;
                            _currentLineGroup.Members.Add(parsedInto.LastParsedProperty);
                        }
                        _currentBlock.Properties.Add(parsedInto.LastParsedProperty);
                    }
                    else
                    {
                        AddTrivia(_currentRawText); // not taken by the entity (or outside a block): kept as written
                    }
                    break; //nothing
            }
            // a #new.../#select... line starts a block of the entity it named
            if (CommandEntityMap.ContainsKey(c) && _currentEntity is IDEntity blockEntity)
            {
                if (_currentBlock != null && _currentBlock.RawEnd == null)
                    _currentBlock.EndsWithoutEnd = true; // the author left out its #end
                bool selected = CommandsMap.TryGetString(c, out var header) && header.StartsWith("#select");
                _currentBlock = new SourceBlock(blockEntity, selected, val, comment)
                {
                    RawHeader = _currentRawText,
                    IdAtParse = blockEntity.ID,
                };
                _currentBlock.LeadingTrivia.AddRange(_pendingTrivia);
                _pendingTrivia.Clear();
                SourceBlocks.Add(_currentBlock);
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
            // what an unedited property exports as (docs/SAVE_FLOW.md, "Original text"); taken
            // once, after references resolve and before any edit
            if (!_baselineTaken)
            {
                foreach (var p in SourceBlocks.SelectMany(b => b.Properties))
                    p.BaselineExport = p.ToExportString();
                _baselineTaken = true;
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

        /// <summary>
        /// Copy/inheritance redesign Phase 1 (see docs/COPY_INHERITANCE_REDESIGN.md): completes any
        /// deferred copy snapshots (vanilla / forward sources) and bakes divergent inherited values
        /// into explicit overrides, so a load->save is data-identical even when a copy source is
        /// edited after being copied. Run AFTER Resolve() (dependencies must be attached) and BEFORE
        /// Export(). Currently invoked by the round-trip harness; the GUI integration is a later phase.
        /// </summary>
        public void NormalizeCopies()
        {
            foreach (var set in Database.Values)
            {
                foreach (var entity in set.GetFullList())
                {
                    entity.FinalizeCopyMaterialization();
                }
            }
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

        /// <summary>The entity class for an entity type (Monster for MONSTER, ...).</summary>
        public Type TypeOf(EntityType type)
        {
            return TypeEntityMap.First(kv => kv.Value == type).Key;
        }

        public IDEntity SelectEntity<T>(EntityType et, string val, string comment) where T : IDEntity, new()
        {
            if (int.TryParse(val, out int id) && this.TryGet(et, id, val, out IDEntity entity))
            {
                return entity;
            }
            // a second #select "Name" block continues the entity the first one selected
            if (!int.TryParse(val, out _) && !string.IsNullOrEmpty(val)
                && Database[et].GetFullList().FirstOrDefault(e => e.Selected && e.Named && string.Equals(e.HeaderName, val, StringComparison.OrdinalIgnoreCase)) is IDEntity same)
            {
                return same;
            }
            return NewEntity<T>(val, comment, true);
        }

        /// <summary>
        /// Returns this mod's own entity with the given ID, creating a sparse #select entry in
        /// this mod when only a dependency (e.g. vanilla) defines it. Edits then land in the mod
        /// instead of mutating the shared dependency entity (copy-on-write).
        /// </summary>
        public T SelectForEdit<T>(int id) where T : IDEntity, new()
        {
            if (Database[GetEntityType(typeof(T))].TryGet(id, null, out IDEntity own))
            {
                return (T)own;
            }
            var entity = new T();
            entity.Assign(id.ToString(), "", this, selected: true);
            return entity;
        }
        #endregion

    }
}
