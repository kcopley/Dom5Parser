using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Mods
{
    public class Mod
    {
        private readonly char spaceDelimiter = ' ';
        private readonly string commentDelimiter = "--";
        private readonly char[] commandDelimiter = new char[] { '#' };

        public string ModName { get; set; }
        public string ModFileName { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Version { get; set; }
        public string DomVersion { get; set; }

        private List<string> _dependencies = new List<string>();
        public List<Mod> Dependencies = new List<Mod>();

        public Dictionary<int, IDEntity> Monsters = new Dictionary<int, IDEntity>();
        public bool TryGetValueMonsters(int i, out IDEntity entity)
        {
            foreach (var m in Dependencies)
            {
                if (m.Monsters.TryGetValue(i, out entity)) return true;
            }
            if (Monsters.TryGetValue(i, out entity)) return true;
            return false;
        }
        public Dictionary<string, IDEntity> NamedMonsters = new Dictionary<string, IDEntity>();
        public bool TryGetValueNamedMonsters(string i, out IDEntity entity)
        {
            foreach (var m in Dependencies)
            {
                if (m.NamedMonsters.TryGetValue(i, out entity)) return true;
            }
            if (NamedMonsters.TryGetValue(i, out entity)) return true;
            return false;
        }
        public Dictionary<int, IDEntity> Spells = new Dictionary<int, IDEntity>();
        public bool TryGetValueSpells(int i, out IDEntity entity)
        {
            foreach (var m in Dependencies)
            {
                if (m.Spells.TryGetValue(i, out entity)) return true;
            }
            if (Spells.TryGetValue(i, out entity)) return true;
            return false;
        }
        public Dictionary<string, IDEntity> NamedSpells = new Dictionary<string, IDEntity>();
        public bool TryGetValueNamedSpells(string i, out IDEntity entity)
        {
            foreach (var m in Dependencies)
            {
                if (m.NamedSpells.TryGetValue(i, out entity)) return true;
            }
            if (NamedSpells.TryGetValue(i, out entity)) return true;
            return false;
        }
        public List<IDEntity> SpellsWithNoNameYet = new List<IDEntity>();
        public Dictionary<int, IDEntity> Items = new Dictionary<int, IDEntity>();
        public bool TryGetValueItems(int i, out IDEntity entity)
        {
            foreach (var m in Dependencies)
            {
                if (m.Items.TryGetValue(i, out entity)) return true;
            }
            if (Items.TryGetValue(i, out entity)) return true;
            return false;
        }

        public Dictionary<string, IDEntity> NamedItems = new Dictionary<string, IDEntity>();
        public bool TryGetValueNamedItems(string i, out IDEntity entity)
        {
            foreach (var m in Dependencies)
            {
                if (m.NamedItems.TryGetValue(i, out entity)) return true;
            }
            if (NamedItems.TryGetValue(i, out entity)) return true;
            return false;
        }

        public List<IDEntity> ItemsWithNoNameYet = new List<IDEntity>();
        public Dictionary<int, IDEntity> Weapons = new Dictionary<int, IDEntity>();
        public bool TryGetValueWeapons(int i, out IDEntity entity)
        {
            foreach (var m in Dependencies)
            {
                if (m.Weapons.TryGetValue(i, out entity)) return true;
            }
            if (Weapons.TryGetValue(i, out entity)) return true;
            return false;
        }
        public Dictionary<string, IDEntity> NamedWeapons = new Dictionary<string, IDEntity>();
        public bool TryGetValueNamedWeapons(string i, out IDEntity entity)
        {
            foreach (var m in Dependencies)
            {
                if (m.NamedWeapons.TryGetValue(i, out entity)) return true;
            }
            if (NamedWeapons.TryGetValue(i, out entity)) return true;
            return false;
        }
        public Dictionary<int, IDEntity> Armors = new Dictionary<int, IDEntity>();
        public bool TryGetValueArmors(int i, out IDEntity entity)
        {
            foreach (var m in Dependencies)
            {
                if (m.Armors.TryGetValue(i, out entity)) return true;
            }
            if (Armors.TryGetValue(i, out entity)) return true;
            return false;
        }
        public Dictionary<string, IDEntity> NamedArmors = new Dictionary<string, IDEntity>();
        public bool TryGetValueNamedArmors(string i, out IDEntity entity)
        {
            foreach (var m in Dependencies)
            {
                if (m.NamedArmors.TryGetValue(i, out entity)) return true;
            }
            if (NamedArmors.TryGetValue(i, out entity)) return true;
            return false;
        }

        public Dictionary<int, IDEntity> Sites = new Dictionary<int, IDEntity>();
        public bool TryGetValueSites(int i, out IDEntity entity)
        {
            foreach (var m in Dependencies)
            {
                if (m.Sites.TryGetValue(i, out entity)) return true;
            }
            if (Sites.TryGetValue(i, out entity)) return true;
            return false;
        }
        public Dictionary<string, IDEntity> NamedSites = new Dictionary<string, IDEntity>();
        public bool TryGetValueNamedSites(string i, out IDEntity entity)
        {
            foreach (var m in Dependencies)
            {
                if (m.NamedSites.TryGetValue(i, out entity)) return true;
            }
            if (NamedSites.TryGetValue(i, out entity)) return true;
            return false;
        }
        public List<IDEntity> SitesThatNeedIDs = new List<IDEntity>();
        public Dictionary<int, IDEntity> Nametypes = new Dictionary<int, IDEntity>();
        public bool TryGetValueNametypes(int i, out IDEntity entity)
        {
            foreach (var m in Dependencies)
            {
                if (m.Nametypes.TryGetValue(i, out entity)) return true;
            }
            if (Nametypes.TryGetValue(i, out entity)) return true;
            return false;
        }
        //public Dictionary<int, MontagIDRef> Montags = new Dictionary<int, MontagIDRef>();
        public Dictionary<int, RestrictedItem> RestrictedItems = new Dictionary<int, RestrictedItem>();
        public Dictionary<int, Enchantment> Enchantments = new Dictionary<int, Enchantment>();
        public Dictionary<int, EventCode> EventCodes = new Dictionary<int, EventCode>();
        public Dictionary<int, EventEffectCode> EventEffectCodes = new Dictionary<int, EventEffectCode>();
        public Dictionary<int, IDEntity> Nations = new Dictionary<int, IDEntity>();
        public bool TryGetValueNations(int i, out IDEntity entity)
        {
            foreach (var m in Dependencies)
            {
                if (m.Nations.TryGetValue(i, out entity)) return true;
            }
            if (Nations.TryGetValue(i, out entity)) return true;
            return false;
        }
        public Dictionary<string, IDEntity> NamedNations = new Dictionary<string, IDEntity>();
        public bool TryGetValueNamedNations(string i, out IDEntity entity)
        {
            foreach (var m in Dependencies)
            {
                if (m.NamedNations.TryGetValue(i, out entity)) return true;
            }
            if (NamedNations.TryGetValue(i, out entity)) return true;
            return false;
        }
        public List<IDEntity> NationsWithNoID = new List<IDEntity>();
        public Dictionary<int, IDEntity> Poptypes = new Dictionary<int, IDEntity>();
        public bool TryGetValuePoptypes(int i, out IDEntity entity)
        {
            foreach (var m in Dependencies)
            {
                if (m.Poptypes.TryGetValue(i, out entity)) return true;
            }
            if (Poptypes.TryGetValue(i, out entity)) return true;
            return false;
        }
        public Dictionary<int, Montag> Montags = new Dictionary<int, Montag>();

        public Dictionary<string, IDEntity> NamedMercenaries = new Dictionary<string, IDEntity>();

        public List<SpellDamage> SpellDamages = new List<SpellDamage>();

        public List<IDEntity> Events = new List<IDEntity>();

        private int _MonStartID = Importer.MONSTER_START_ID;
        private int _SiteStartID = Importer.SITE_START_ID;
        private int _EventStartID = Importer.EVENT_START_ID;
        private int _ArmorStartID = Importer.ARMOR_START_ID;
        private int _WepStartID = Importer.WEAPON_START_ID;
        private int _ItemStartID = Importer.ITEM_START_ID;
        private int _SpellStartID = Importer.SPELL_START_ID;
        private int _NametypeStartID = Importer.NAMETYPE_START_ID;
        private int _NationStartID = Importer.NATION_START_ID;
        private int _MontagStartID = Importer.MONTAG_START_ID;
        private int _RestrictedItemStartID = Importer.RESTRICTED_ITEM_START_ID;
        private int _EnchantmentStartID = Importer.ENCHANTMENT_START_ID;
        private int _EventCodeStartID = Importer.EVENT_CODE_START_ID;
        private int _EventCodeEffectStartID = Importer.EVENT_CODE_EFFECT_START_ID;

        private Entity _currentEntity = null;

        public bool LineWasTrimmed { get; set; }

        public Mod()
        {
        }

        public int LineNumber { get; private set; } = 0;
        private string logFile;
        public void Log(string s)
        {
            if (string.IsNullOrEmpty(logFile))
            {
                return;
            }
            using (StreamWriter writer = File.AppendText(logFile))
            {
                writer.WriteLine("Line: " + LineNumber + " - " + s);
            }
        }

        public void Parse(string dmFile)
        {
            int indexOfDotDM = dmFile.IndexOf(".dm");
            if (indexOfDotDM != -1)
            {
                logFile = dmFile.Substring(0, indexOfDotDM) + "-log.txt";
            }

            using (StreamReader sr = File.OpenText(dmFile))
            {
                string s = "";

                bool isMultiLine = false;
                string prevLine = "";

                while ((s = sr.ReadLine()) != null)
                {
                    LineNumber++;
                    s = s.Trim(); //remove whitespaces
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
            }
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
            if (commentIndex != -1) //has a comment
            {
                line = s.Substring(0, commentIndex).Trim();
                comment = s.Substring(commentIndex + 2).Trim();
            }

            //grab the command & value

            int spaceIndex = line.IndexOf(spaceDelimiter);
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
                    Log("Command does not exist for: "+ _currentEntity.GetType() + " for command: " + command);
                else
                    Log("Command does not exist for: " + command);
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
                    _currentEntity = NewMonster(val, comment);
                    break;
                case Command.SELECTMONSTER:
                    _currentEntity = SelectMonster(val, comment);
                    break;
                case Command.NEWARMOR:
                    _currentEntity = NewArmor(val, comment);
                    break;
                case Command.SELECTARMOR:
                    _currentEntity = SelectArmor(val, comment);
                    break;
                case Command.NEWWEAPON:
                    _currentEntity = NewWeapon(val, comment);
                    break;
                case Command.SELECTWEAPON:
                    _currentEntity = SelectWeapon(val, comment);
                    break;
                case Command.SELECTNAMETYPE:
                    _currentEntity = SelectNametype(val, comment);
                    break;
                case Command.NEWSITE:
                    _currentEntity = NewSite(val, comment);
                    break;
                case Command.SELECTSITE:
                    _currentEntity = SelectSite(val, comment);
                    break;
                case Command.NEWNATION:
                    _currentEntity = NewNation(val, comment);
                    break;
                case Command.SELECTNATION:
                    _currentEntity = SelectNation(val, comment);
                    break;
                case Command.NEWITEM:
                    _currentEntity = NewItem(val, comment);
                    break;
                case Command.SELECTITEM:
                    _currentEntity = SelectItem(val, comment);
                    break;
                case Command.NEWSPELL:
                    _currentEntity = NewSpell(val, comment);
                    break;
                case Command.SELECTSPELL:
                    _currentEntity = SelectSpell(val, comment);
                    break;
                case Command.NEWMERC:
                    _currentEntity = NewMercenary(val, comment);
                    break;
                case Command.SELECTPOPTYPE:
                    _currentEntity = SelectPoptype(val, comment);
                    break;
                case Command.NEWEVENT:
                    _currentEntity = NewEvent(val, comment);
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
            }
        }

        public void Resolve()
        {
            foreach (var kvp in Monsters)
            {
                kvp.Value.Resolve();
            }
            foreach (var kvp in NamedMonsters)
            {
                kvp.Value.Resolve();
            }
            foreach (var kvp in Armors)
            {
                kvp.Value.Resolve();
            }
            foreach (var kvp in NamedArmors)
            {
                kvp.Value.Resolve();
            }
            foreach (var kvp in Weapons)
            {
                kvp.Value.Resolve();
            }
            foreach (var kvp in NamedWeapons)
            {
                kvp.Value.Resolve();
            }
            foreach (var kvp in Nametypes)
            {
                kvp.Value.Resolve();
            }
            foreach (var kvp in Sites)
            {
                kvp.Value.Resolve();
            }
            foreach (var kvp in NamedSites)
            {
                kvp.Value.Resolve();
            }
            foreach (var kvp in SitesThatNeedIDs)
            {
                kvp.Resolve();
            }
            foreach (var kvp in Nations)
            {
                kvp.Value.Resolve();
            }
            foreach (var kvp in NamedNations)
            {
                kvp.Value.Resolve();
            }
            foreach (var kvp in NationsWithNoID)
            {
                kvp.Resolve();
            }
            foreach (var kvp in Items)
            {
                kvp.Value.Resolve();
            }
            foreach (var kvp in NamedItems)
            {
                kvp.Value.Resolve();
            }
            foreach (var kvp in ItemsWithNoNameYet)
            {
                kvp.Resolve();
            }
            foreach (var kvp in Spells)
            {
                kvp.Value.Resolve();
            }
            foreach (var kvp in NamedSpells)
            {
                kvp.Value.Resolve();
            }
            foreach (var kvp in SpellsWithNoNameYet)
            {
                kvp.Resolve();
            }
            foreach (var kvp in NamedMercenaries)
            {
                kvp.Value.Resolve();
            }
            foreach (var kvp in Poptypes)
            {
                kvp.Value.Resolve();
            }
            foreach (var kvp in Events)
            {
                kvp.Resolve();
            }
            foreach (var kvp in SpellDamages)
            {
                kvp.Resolve();
            }

            foreach (var kvp in this.Montags)
            {
                foreach (var m in Dependencies)
                {
                    if (m.Montags.ContainsKey(kvp.Key))
                    {
                        kvp.Value.DependentMontag = m.Montags[kvp.Key];
                        break;
                    }
                }
            }
            foreach (var kvp in this.Enchantments)
            {
                foreach (var m in Dependencies)
                {
                    if (m.Enchantments.ContainsKey(kvp.Key))
                    {
                        kvp.Value.DependentEnchantment = m.Enchantments[kvp.Key];
                        break;
                    }
                }
            }
            foreach (var kvp in this.RestrictedItems)
            {
                foreach (var m in Dependencies)
                {
                    if (m.RestrictedItems.ContainsKey(kvp.Key))
                    {
                        kvp.Value.DependentRestrictedItem = m.RestrictedItems[kvp.Key];
                        break;
                    }
                }
            }
            foreach (var kvp in this.EventCodes)
            {
                foreach (var m in Dependencies)
                {
                    if (m.EventCodes.ContainsKey(kvp.Key))
                    {
                        kvp.Value.DependentEventCode = m.EventCodes[kvp.Key];
                        break;
                    }
                }
            }
            foreach (var kvp in this.EventEffectCodes)
            {
                foreach (var m in Dependencies)
                {
                    if (m.EventEffectCodes.ContainsKey(kvp.Key))
                    {
                        kvp.Value.DependentEventEffectCode = m.EventEffectCodes[kvp.Key];
                        break;
                    }
                }
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

            //Weapons
            Export(writer, Weapons.Values.ToList());
            Export(writer, NamedWeapons.Values.ToList());
            //Armors
            Export(writer, Armors.Values.ToList());
            Export(writer, NamedArmors.Values.ToList());
            //Monsters
            Export(writer, Monsters.Values.ToList());
            Export(writer, NamedMonsters.Values.ToList());
            //Nametypes
            Export(writer, Nametypes.Values.ToList());
            //Sites
            Export(writer, Sites.Values.ToList());
            Export(writer, NamedSites.Values.ToList());
            Export(writer, SitesThatNeedIDs);
            //Nations
            Export(writer, Nations.Values.ToList());
            //Export(writer, NamedNations.Values.ToList()); //not needed

            //spells
            Export(writer, Spells.Values.ToList());
            Export(writer, NamedSpells.Values.ToList());
            Export(writer, SpellsWithNoNameYet.ToList());

            //magic items
            Export(writer, Items.Values.ToList());
            Export(writer, NamedItems.Values.ToList());
            Export(writer, ItemsWithNoNameYet.ToList());

            Export(writer, Poptypes.Values.ToList());
            Export(writer, Events);
        }

        public void Export(StreamWriter writer, List<Entity> entities)
        {
            foreach (Entity m in entities)
            {
                m.Export(writer);
                writer.WriteLine();
            }
        }

        public void Export(StreamWriter writer, List<IDEntity> entities)
        {
            foreach (Entity m in entities)
            {
                m.Export(writer);
                writer.WriteLine();
            }
        }

        public Montag AddMontag(int ID)
        {
            if (ID == -1) return null;
            if (Montags.TryGetValue(ID, out var m))
            {
                return m;
            }
            else
            {
                var ret = new Montag(ID);
                Montags.Add(ID, ret);
                return ret;
            }
        }

        public RestrictedItem AddRestrictedItem(int ID)
        {
            if (ID == -1) return null;
            if (RestrictedItems.TryGetValue(ID, out var m))
            {
                return m;
            }
            else
            {
                var ret = new RestrictedItem(ID);
                RestrictedItems.Add(ID, ret);
                return ret;
            }
        }

        public Enchantment AddEnchantment(int ID)
        {
            if (ID == -1) return null;
            if (Enchantments.TryGetValue(ID, out var m))
            {
                return m;
            }
            else
            {
                var ret = new Enchantment(ID);
                Enchantments.Add(ID, ret);
                return ret;
            }
        }

        public EventCode AddEventCode(int ID)
        {
            if (ID == -1) return null;
            if (EventCodes.TryGetValue(ID, out var m))
            {
                return m;
            }
            else
            {
                var ret = new EventCode(ID);
                EventCodes.Add(ID, ret);
                return ret;
            }
        }

        public EventEffectCode AddEventEffectCode(int ID)
        {
            if (ID == -1) return null;
            if (EventEffectCodes.TryGetValue(ID, out var m))
            {
                return m;
            }
            else
            {
                var ret = new EventEffectCode(ID);
                EventEffectCodes.Add(ID, ret);
                return ret;
            }
        }

        public IDEntity NewMonster(string val, string comment, bool selected = false)
        {
            Monster m = new Monster(val, comment, this, selected);
            return m;
        }

        public IDEntity SelectMonster(string val, string comment)
        {
            if (int.TryParse(val, out int id) && Monsters.TryGetValue(id, out IDEntity m))
            {
                return m;
            }
            else if (NamedMonsters.TryGetValue(val, out IDEntity m2))
            {
                return m2;
            }
            else
            {
                return NewMonster(val, comment, true);
            }
        }

        public IDEntity NewEvent(string val, string comment, bool selected = false)
        {
            Event m = new Event(val, comment, this, selected);
            return m;
        }

        public IDEntity NewSpell(string val, string comment, bool selected = false)
        {
            Spell m = new Spell(val, comment, this, selected);
            return m;
        }

        public IDEntity SelectSpell(string val, string comment)
        {
            if (int.TryParse(val, out int id) && Spells.TryGetValue(id, out IDEntity m))
            {
                return m;
            }
            else if (NamedSpells.TryGetValue(val, out IDEntity m2))
            {
                return m2;
            }
            else
            {
                return NewSpell(val, comment, true);
            }
        }

        public IDEntity SelectNametype(string val, string comment)
        {
            if (int.TryParse(val, out int id) && Nametypes.TryGetValue(id, out IDEntity m))
            {
                return m;
            }
            else
            {
                return new Nametype(val, comment, this, true);
            }
        }

        public IDEntity SelectPoptype(string val, string comment)
        {
            if (int.TryParse(val, out int id) && Poptypes.TryGetValue(id, out IDEntity m))
            {
                return m;
            }
            else
            {
                return new Poptype(val, comment, this, true);
            }
        }

        public IDEntity NewArmor(string val, string comment, bool selected = false)
        {
            Armor m = new Armor(val, comment, this, selected);
            return m;
        }

        public IDEntity SelectArmor(string val, string comment)
        {
            if (int.TryParse(val, out int id) && Armors.TryGetValue(id, out IDEntity m))
            {
                return m;
            }
            else if (NamedArmors.TryGetValue(val, out IDEntity m2))
            {
                return m2;
            }
            else
            {
                return NewArmor(val, comment, true);
            }
        }

        public IDEntity NewMercenary(string val, string comment, bool selected = false)
        {
            Mercenary m = new Mercenary(val, comment, this, selected);
            return m;
        }

        public Weapon NewWeapon(string val, string comment, bool selected = false)
        {
            Weapon m = new Weapon(val, comment, this, selected);
            return m;
        }

        public IDEntity SelectWeapon(string val, string comment)
        {
            if (int.TryParse(val, out int id) && Weapons.TryGetValue(id, out IDEntity m))
            {
                return m;
            }
            else if (NamedWeapons.TryGetValue(val, out IDEntity m2))
            {
                return m2;
            }
            else
            {
                return NewWeapon(val, comment, true);
            }
        }

        public Site NewSite(string val, string comment, bool selected = false)
        {
            Site m = new Site(val, comment, this, selected);
            return m;
        }

        public IDEntity SelectSite(string val, string comment)
        {
            if (int.TryParse(val, out int id) && Sites.TryGetValue(id, out IDEntity m))
            {
                return m;
            }
            else if (NamedSites.TryGetValue(val, out IDEntity m2))
            {
                return m2;
            }
            else
            {
                return NewSite(val, comment, true);
            }
        }

        public Nation NewNation(string val, string comment, bool selected = false)
        {
            Nation m = new Nation(val, comment, this, selected);
            return m;
        }

        public IDEntity SelectNation(string val, string comment)
        {
            if (int.TryParse(val, out int id) && Nations.TryGetValue(id, out IDEntity m))
            {
                return m;
            }
            else if (NamedNations.TryGetValue(val, out IDEntity m2))
            {
                return m2;
            }
            else
            {
                return NewNation(val, comment, true);
            }
        }

        public Item NewItem(string val, string comment, bool selected = false)
        {
            Item m = new Item(val, comment, this, selected);
            return m;
        }

        public IDEntity SelectItem(string val, string comment)
        {
            if (int.TryParse(val, out int id) && Items.TryGetValue(id, out IDEntity m))
            {
                return m;
            }
            else if (NamedItems.TryGetValue(val, out IDEntity m2))
            {
                return m2;
            }
            else
            {
                return NewItem(val, comment, true);
            }
        }

        public int GetNextMonsterID()
        {
            //very crude search unfortunately, but should be fine for our purposes
            while (Monsters.ContainsKey(_MonStartID))
            {
                _MonStartID++;
            }
            return _MonStartID;
        }

        public int GetNextSpellID()
        {
            //very crude search unfortunately, but should be fine for our purposes
            while (Spells.ContainsKey(_SpellStartID))
            {
                _SpellStartID++;
            }
            return _SpellStartID;
        }

        public int GetNextItemID()
        {
            //very crude search unfortunately, but should be fine for our purposes
            while (Items.ContainsKey(_ItemStartID))
            {
                _ItemStartID++;
            }
            return _ItemStartID;
        }

        public int GetNextMontagID()
        {
            //very crude search unfortunately, but should be fine for our purposes
            while (Montags.ContainsKey(_MontagStartID))
            {
                _MontagStartID++;
            }
            return _MontagStartID;
        }

        public int GetNextRestrictedItemID()
        {
            //very crude search unfortunately, but should be fine for our purposes
            while (RestrictedItems.ContainsKey(_RestrictedItemStartID))
            {
                _RestrictedItemStartID++;
            }
            return _RestrictedItemStartID;
        }

        public int GetNextEnchantmentID()
        {
            //very crude search unfortunately, but should be fine for our purposes
            while (Enchantments.ContainsKey(_EnchantmentStartID))
            {
                _EnchantmentStartID++;
            }
            return _EnchantmentStartID;
        }

        public int GetNextEventCodeID()
        {
            //very crude search unfortunately, but should be fine for our purposes
            while (EventCodes.ContainsKey(_EventCodeStartID))
            {
                _EventCodeStartID--;
            }
            return _EventCodeStartID;
        }

        public int GetNextEventEffectCodeStartID()
        {
            //very crude search unfortunately, but should be fine for our purposes
            while (EventEffectCodes.ContainsKey(_EventCodeEffectStartID))
            {
                _EventCodeEffectStartID++;
            }
            return _EventCodeEffectStartID;
        }

        public int GetNextWeaponID()
        {
            //very crude search unfortunately, but should be fine for our purposes
            while (Weapons.ContainsKey(_WepStartID))
            {
                _WepStartID++;
            }
            return _WepStartID;
        }

        public int GetNextArmorID()
        {
            //very crude search unfortunately, but should be fine for our purposes
            while (Armors.ContainsKey(_ArmorStartID))
            {
                _ArmorStartID++;
            }
            return _ArmorStartID;
        }

        public int GetNextSiteID()
        {
            if (_SiteStartID > 1683)
            {
                int a = 0;
                a++;
            }
            //very crude search unfortunately, but should be fine for our purposes
            while (Sites.ContainsKey(_SiteStartID))
            {
                _SiteStartID++;
            }
            return _SiteStartID;
        }

        public int GetNextNationID()
        {
            //very crude search unfortunately, but should be fine for our purposes
            while (Nations.ContainsKey(_NationStartID))
            {
                _NationStartID++;
            }
            return _NationStartID;
        }

        public int GetNextNametypeID()
        {
            //very crude search unfortunately, but should be fine for our purposes
            while (Nametypes.ContainsKey(_NametypeStartID))
            {
                _NametypeStartID++;
            }
            return _NametypeStartID;
        }
    }
}
