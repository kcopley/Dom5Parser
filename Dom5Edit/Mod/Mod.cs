using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace Dom5Edit
{
    public class Mod
    {
        private readonly char spaceDelimiter = ' ';
        private readonly string tabDelimiter = "\t";
        private readonly string commentDelimiter = "--";
        private readonly char[] commandDelimiter = new char[] { '#' };

        public string ModName { get; set; }
        public string ModFileName { get { return Path.GetFileName(FullFilePath); } }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Version { get; set; }
        public string DomVersion { get; set; }

        private List<string> _dependencies = new List<string>();
        public List<Mod> Dependencies { get; set; } = new List<Mod>();
        public List<string> DisabledNations = new List<string>();

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
            { typeof(EventCodeEffect), EntityType.EVENT_CODE_EFFECT },
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
        };

        public List<SpellDamage> SpellDamages = new List<SpellDamage>();
        public List<IDEntity> Events = new List<IDEntity>();
        public List<int> VanillaMageReferences = new List<int>();

        internal static int MONSTER_START_ID = 3486;
        internal static int SITE_START_ID = 1164;
        internal static int EVENT_START_ID = 6000;
        internal static int ARMOR_START_ID = 251;
        internal static int WEAPON_START_ID = 763;
        internal static int ITEM_START_ID = 446;
        internal static int SPELL_START_ID = 1177;
        internal static int NAMETYPE_START_ID = 170;
        internal static int NATION_START_ID = 109;
        internal static int MONTAG_START_ID = 1000;
        internal static int RESTRICTED_ITEM_START_ID = 1;
        internal static int ENCHANTMENT_START_ID = 106;
        internal static int EVENT_CODE_START_ID = -300;
        internal static int EVENT_CODE_EFFECT_START_ID = 14;

        internal static int MONSTER_END_ID = 8999;
        internal static int SITE_END_ID = 1999;
        internal static int ARMOR_END_ID = 999;
        internal static int WEAPON_END_ID = 1999;
        internal static int ITEM_END_ID = 999;
        internal static int SPELL_END_ID = 3999;
        internal static int NAMETYPE_END_ID = 299;
        internal static int NATION_END_ID = 249;

        private Entity _currentEntity = null;

        public bool LineWasTrimmed { get; set; }

        public Mod()
        {
            Init();
            foreach (EntitySet<IDEntity> set in Database.Values)
            {
                set.Init();
            }
        }

        public Mod(string filePath)
        {
            Init();
            this.FullFilePath = filePath;
        }

        private void Init()
        {
            foreach (var kvp in Database)
            {
                kvp.Value.Parent = this;
            }
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
            string s = "";

            bool isMultiLine = false;
            string prevLine = "";

            while ((s = sr.ReadLine()) != null)
            {
                LineNumber++;
                s = s.Trim(); //remove whitespaces
                s = s.Replace('\t', ' ');
                if (s.Length < 1) continue; //empty line
                                            //pull comments first

                //mod information data
                int ind = s.IndexOf("#dependency", StringComparison.OrdinalIgnoreCase);

                if (ind != -1)
                {
                    continue; //skip these lines, grabbed above
                }

                if ((s.IndexOf("#descr") != -1 && s.Length > 6) || (s.IndexOf("#summary") != -1 && s.Length > 8) || (s.IndexOf("#msg") != -1 && s.Length > 4))
                {
                    //could be multi line description
                    //check if has both quotes
                    int firstQuote = s.IndexOf('"');
                    int secondQuote = s.IndexOf('"', firstQuote + 1);
                    //first quote mark exists, second does not
                    //either is multi-line, or quote mark forgotten on the end
                    if (firstQuote != -1 && secondQuote == -1)
                    {
                        bool hasAnotherCommand = HasCommandOnLine(s.Substring(firstQuote)); //only check after the first quote
                        if (!hasAnotherCommand)
                        {
                            isMultiLine = true;
                            prevLine = s;
                            continue;
                        } //if it has another command on that line, the quote was just forgotten
                    }
                    ProcessStringToLine(s);
                }
                else if (isMultiLine && !string.IsNullOrEmpty(prevLine))
                {
                    //already on a multi-line, does it continue?
                    int endQuote = s.IndexOf('"');
                    bool anotherCommand = HasCommandOnLine(s);

                    if (endQuote != -1 && !anotherCommand) //ends on this line
                    {
                        string endLine = prevLine + Environment.NewLine + s;
                        ProcessStringToLine(endLine);
                        prevLine = "";
                        isMultiLine = false;
                    }
                    else if (anotherCommand) // of course a mod author would end a multi-line and start another command on the same line
                    {
                        //split and add up to the # to the previous string, process it
                        //and then process the second string
                        int anotherCommandIndex = GetNextCommandIndex(s);
                        string leftsplit = s.Substring(0, anotherCommandIndex);
                        string rightsplit = s.Substring(anotherCommandIndex);
                        string multiline = prevLine + Environment.NewLine + leftsplit;
                        ProcessStringToLine(multiline);
                        ProcessStringToLine(rightsplit);
                        prevLine = "";
                        isMultiLine = false;
                    }
                    else
                    {
                        //no command, no end quote... it must continue as part of the string
                        prevLine = prevLine + Environment.NewLine + s;
                    }
                }
                else
                {
                    ProcessStringToLine(s);
                }
            }
            LineWasTrimmed = false;
            LineNumber = -1;
        }

        public bool HasDependencies()
        {
            string dmFile = this.FullFilePath;
            using (StreamReader sr = File.OpenText(dmFile))
            {
                string s = "";

                while ((s = sr.ReadLine()) != null)
                {
                    LineNumber++;
                    s = s.Trim(); //remove whitespaces
                    s = s.Replace('\t', ' ');
                    if (s.Length < 1) continue; //empty line
                                                //pull comments first

                    //mod information data
                    int ind = s.IndexOf("#dependency", StringComparison.OrdinalIgnoreCase);

                    if (ind != -1)
                    {
                        ind += 12;
                        string file = s.Substring(ind);
                        if (file.Length > 0)
                        {
                            file = file.Trim();
                            _dependencies.Add(file);
                        }
                        continue;
                    }
                }
            }

            return _dependencies?.Count > 0;
        }

        public bool HasCommandOnLine(string s)
        {
            //did they add another command alongside on this line?
            int anotherCommand = GetNextCommandIndex(s);
            if (anotherCommand < 0) return false;
            int spaceAfter = s.IndexOf(' ', anotherCommand);
            bool hasValidCommand = false;

            if (anotherCommand != -1)
            {
                string comm;
                if (spaceAfter != -1)
                {
                    comm = s.Substring(anotherCommand, spaceAfter - anotherCommand);
                }
                else
                {
                    comm = s.Substring(anotherCommand);
                }
                hasValidCommand = CommandsMap.TryGetCommand(comm, out _); //this is a valid command on this line
            }
            return hasValidCommand;
        }

        public int GetNextCommandIndex(string s)
        {
            int nextIndex = s.IndexOf('#');
            while (nextIndex != -1)
            {
                if (s.IndexOf("##landname##", nextIndex) == nextIndex)
                {
                    nextIndex += 12;
                }
                else if (s.IndexOf("##godname##", nextIndex) == nextIndex)
                {
                    nextIndex += 11;
                }
                else if (s.IndexOf("##targname##", nextIndex) == nextIndex)
                {
                    nextIndex += 12;
                }
                else if (s.IndexOf("##fullgodname##", nextIndex) == nextIndex)
                {
                    nextIndex += 15;
                }
                else
                {
                    return nextIndex;
                }
                nextIndex = s.IndexOf('#', nextIndex + 1);
            }
            return -1;
        }

        public void ProcessStringToLine(string s)
        {
            // continue on
            int commentIndex = s.IndexOf(commentDelimiter);

            //is there another command on the same line?
            List<int> commandIndexes = new List<int>();
            int index = s.IndexOf('#');
            if (index != -1 && (index < commentIndex || commentIndex == -1))
            {
                commandIndexes.Add(index);
                int nextIndex = s.IndexOf('#', index + 1);
                while (nextIndex != -1 && (nextIndex < commentIndex || commentIndex == -1))
                {
                    if (s.IndexOf("##landname##", nextIndex) == nextIndex)
                    {
                        nextIndex += 12;
                    }
                    else if (s.IndexOf("##godname##", nextIndex) == nextIndex)
                    {
                        nextIndex += 11;
                    }
                    else if (s.IndexOf("##targname##", nextIndex) == nextIndex)
                    {
                        nextIndex += 12;
                    }
                    else if (s.IndexOf("##fullgodname##", nextIndex) == nextIndex)
                    {
                        nextIndex += 15;
                    }
                    else
                    {
                        commandIndexes.Add(nextIndex);
                    }
                    nextIndex = s.IndexOf('#', nextIndex + 1);
                }

                for (int i = 0; i < commandIndexes.Count; i++)
                {
                    int nextCommand = i + 1;
                    string line;
                    if (nextCommand < commandIndexes.Count)
                    {
                        line = s.Substring(commandIndexes[i], commandIndexes[nextCommand] - commandIndexes[i]);
                    }
                    else
                    {
                        line = s.Substring(commandIndexes[i]);
                    }
                    ProcessLine(line);
                }
            }
        }

        public void ProcessLine(string s)
        {
            string line = s;
            int commentIndex = s.IndexOf(commentDelimiter);
            string comment = ""; //set to empty string, not null

            if (commentIndex == -1) //check for single dash
            {
                int singleDash = s.IndexOf('-');
                //if single dash exists, if the next character exists, and next char is not an integer
                if (singleDash != -1 && s.Length > singleDash + 1 && !int.TryParse(s[singleDash + 1].ToString(), out _))
                {
                    //if it has quotes, it could be a dash in a description
                    int quoteIndex = s.IndexOf('"');
                    if (quoteIndex != -1)
                    {
                        // assume if there's a first quote, check for a second quote mark
                        int secondQuoteIndex = s.IndexOf('"', quoteIndex + 1);
                        // only allow a single dash as a comment if it comes after a second quote mark
                        if (singleDash > secondQuoteIndex)
                        {
                            line = s.Substring(0, singleDash).Trim();
                            comment = s.Substring(singleDash + 1).Trim();
                        }
                    }
                    else //no quote marks either
                    {
                        line = s.Substring(0, singleDash).Trim();
                        comment = s.Substring(singleDash + 1).Trim();
                    }
                }
            }
            else if (commentIndex != -1) //has a comment
            {
                line = s.Substring(0, commentIndex).Trim();
                comment = s.Substring(commentIndex + 2).Trim();
            }

            //grab the command & value

            int spaceIndex = line.IndexOf(spaceDelimiter);
            int tabIndex = line.IndexOf(tabDelimiter);
            string command = line;
            string value = ""; //set to empty string, not null
            if (spaceIndex != -1) //has a value (but could be spaces before a comment? should be handled by trim above)
            {
                command = line.Substring(0, spaceIndex).Trim();
                value = line.Substring(spaceIndex + 1).Trim();
                if (value.StartsWith("\"") || value.EndsWith("\""))
                {
                    value = value.Trim('\"');
                    this.LineWasTrimmed = true;
                }
                else
                {
                    LineWasTrimmed = false;
                }
            }
            else if (tabIndex != -1)
            {
                command = line.Substring(0, tabIndex).Trim();
                value = line.Substring(tabIndex + 1).Trim();
                if (value.StartsWith("\"") || value.EndsWith("\""))
                {
                    value = value.Trim('\"');
                    this.LineWasTrimmed = true;
                }
                else
                {
                    LineWasTrimmed = false;
                }
            }
            if (CommandsMap.TryGetCommand(command, out Command c))
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
            else
            {
                if (_currentEntity != null)
                    Log("Invalid, incorrectly spelled, or nonexistent command for: " + _currentEntity.GetType() + " for command: " + command);
                else
                    Log("Invalid, incorrectly spelled, or nonexistent command for: " + command);
            }
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
            if (File.Exists(file))
            {
                if (!overwrite) return;
            }
            using (StreamWriter writer = new StreamWriter(file))
            {
                Export(writer);
            }
        }

        public void Export(StreamWriter writer)
        {
            writer.WriteLine(CommandsMap.Format(Command.MODNAME, ModName, true));
            if (Version?.Length > 0) writer.WriteLine(CommandsMap.Format(Command.VERSION, Version));
            if (DomVersion?.Length > 0) writer.WriteLine(CommandsMap.Format(Command.DOMVERSION, DomVersion));
            if (Icon?.Length > 0) writer.WriteLine(CommandsMap.Format(Command.ICON, Icon, true));
            if (Description?.Length > 0) writer.WriteLine(CommandsMap.Format(Command.DESCRIPTION, Description, true));

            writer.WriteLine();

            foreach (var kvp in Database)
            {
                kvp.Value.Export(writer);
            }
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

        #region Nation Association Code - Look at later
        //Look at this later
        private HashSet<Nation> CurrentNationSet;
        private void ExportNationSet(StreamWriter writer, HashSet<Nation> nations)
        {
            /*
            CurrentNationSet = nations;
            writer.WriteLine(CommandsMap.Format(Command.MODNAME, ModName, true));
            if (Version?.Length > 0) writer.WriteLine(CommandsMap.Format(Command.VERSION, Version));
            if (DomVersion?.Length > 0) writer.WriteLine(CommandsMap.Format(Command.DOMVERSION, DomVersion));
            if (Icon?.Length > 0) writer.WriteLine(CommandsMap.Format(Command.ICON, Icon, true));
            if (Description?.Length > 0) writer.WriteLine(CommandsMap.Format(Command.DESCRIPTION, Description, true));

            writer.WriteLine();

            Func<KeyValuePair<int, IDEntity>, bool> exclusiveDictCheck = x =>
            {
                return x.Value.AssociatedNations.IsSubsetOf(nations) && x.Value.AssociatedNations.Count > 0;
            };
            Func<IDEntity, bool> exclusiveListCheck = x =>
            {
                return x.AssociatedNations.IsSubsetOf(nations) && x.AssociatedNations.Count > 0;
            };
            Func<KeyValuePair<int, IDEntity>, bool> inclusiveDictCheck = x =>
            {
                return (x.Value.AssociatedNations.IsSubsetOf(nations) && x.Value.AssociatedNations.Count > 0) || x.Value.AssociatedNations.IsSupersetOf(nations);
            };
            Func<IDEntity, bool> inclusiveListCheck = x =>
            {
                return (x.AssociatedNations.IsSubsetOf(nations) && x.AssociatedNations.Count > 0) || x.AssociatedNations.IsSupersetOf(nations);
            };

            //Weapons - exclusive, only subset
            Export(writer, Weapons.OrderBy(x => x.Key).Where(exclusiveDictCheck), ModManager.WEAPON_START_ID);
            Export(writer, NamedWeapons.Values.ToList().Where(exclusiveListCheck));
            //Armors - exclusive, only subset
            Export(writer, Armors.OrderBy(x => x.Key).Where(exclusiveDictCheck), ModManager.ARMOR_START_ID);
            Export(writer, NamedArmors.Values.ToList().Where(exclusiveListCheck));

            //Monsters - exclusive, only subset
            Export(writer, DisabledMonsters.OrderBy(x => x.Key).Where(exclusiveDictCheck), ModManager.MONSTER_START_ID);
            Export(writer, Monsters.OrderBy(x => x.Key).Where(exclusiveDictCheck), ModManager.MONSTER_START_ID);
            Export(writer, NamedMonsters.Values.ToList().Where(exclusiveListCheck));
            //Nametypes - exclusive, only subset
            Export(writer, Nametypes.OrderBy(x => x.Key).Where(exclusiveDictCheck), ModManager.NAMETYPE_START_ID);
            //Sites - exclusive, only subset
            Export(writer, Sites.OrderBy(x => x.Key).Where(exclusiveDictCheck), ModManager.SITE_START_ID);
            Export(writer, NamedSites.Values.ToList().Where(exclusiveListCheck));
            Export(writer, SitesThatNeedIDs.Where(exclusiveListCheck));
            //Nations - exclusive, only subset
            Export(writer, Nations.OrderBy(x => x.Key).Where(exclusiveDictCheck), ModManager.NATION_START_ID);

            //spells - inclusive, subset or superset
            Export(writer, Spells.OrderBy(x => x.Key).Where(inclusiveDictCheck), ModManager.SPELL_START_ID);
            Export(writer, NamedSpells.Values.ToList().Where(inclusiveListCheck));
            Export(writer, SpellsWithNoNameYet.ToList().Where(inclusiveListCheck));

            //magic items - inclusive, subset or superset
            Export(writer, Items.OrderBy(x => x.Key).Where(inclusiveDictCheck), ModManager.ITEM_START_ID);
            Export(writer, NamedItems.Values.ToList().Where(inclusiveListCheck));
            Export(writer, ItemsWithNoNameYet.ToList().Where(inclusiveListCheck));

            Export(writer, Poptypes.OrderBy(x => x.Key).Where(exclusiveDictCheck));
            //- inclusive, subset or superset
            Export(writer, Events.Where(inclusiveListCheck));
            CurrentNationSet = null;
            */
        }

        private void AddNationAssociation(Nation nation, IDEntity entity)
        {
            if (entity.AssociatedNations.Contains(nation)) return;
            if (entity is Nation) return;
            entity.AssociatedNations.Add(nation);
            foreach (var e in entity.RequiredEntities)
            {
                AddNationAssociation(nation, e);
            }
        }
        #endregion
    }
}
