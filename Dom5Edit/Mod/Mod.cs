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

        public string ModName { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Version { get; set; }
        public string DomVersion { get; set; }

        public Dictionary<int, Monster> Monsters;
        public Dictionary<int, Spell> Spells;
        public Dictionary<int, Item> Items;
        public Dictionary<int, Weapon> Weapons;
        public Dictionary<int, Armor> Armors;
        public Dictionary<int, Event> Events;
        public Dictionary<int, Site> Sites;

        public Dictionary<int, List<MonIDRef>> MonsterIDMap = new Dictionary<int, List<MonIDRef>>();
        public Dictionary<int, List<SpellIDRef>> SpellIDMap = new Dictionary<int, List<SpellIDRef>>();
        public Dictionary<int, List<WeaponIDRef>> WeaponIDMap = new Dictionary<int, List<WeaponIDRef>>();
        public Dictionary<int, List<ArmorIDRef>> ArmorIDMap = new Dictionary<int, List<ArmorIDRef>>();
        public Dictionary<int, List<ItemIDRef>> ItemIDMap = new Dictionary<int, List<ItemIDRef>>();
        public Dictionary<int, List<RestrictedItemIDRef>> RestrictedItemIDMap = new Dictionary<int, List<RestrictedItemIDRef>>();
        public Dictionary<int, List<EnchIDRef>> EnchIDMap = new Dictionary<int, List<EnchIDRef>>();
        public Dictionary<int, List<MontagIDRef>> MontagIDMap = new Dictionary<int, List<MontagIDRef>>();
        public Dictionary<int, List<NametypeIDRef>> NametypeIDMap = new Dictionary<int, List<NametypeIDRef>>();
        public Dictionary<int, List<SpellIDRef>> NationIDMap = new Dictionary<int, List<SpellIDRef>>();
        public Dictionary<int, List<SiteIDRef>> SiteIDMap = new Dictionary<int, List<SiteIDRef>>();
        
        public Dictionary<string, List<MonsterNameRef>> MonsterNameMap = new Dictionary<string, List<MonsterNameRef>>();
        public Dictionary<string, List<SpellIDRef>> SiteNameMap = new Dictionary<string, List<SpellIDRef>>();
        public Dictionary<string, List<SpellIDRef>> SpellNameMap = new Dictionary<string, List<SpellIDRef>>();
        public Dictionary<string, List<SpellIDRef>> ItemNameMap = new Dictionary<string, List<SpellIDRef>>();

        private Entity _currentEntity = null;

        public Mod()
        {
            Monsters = new Dictionary<int, Monster>();
        }

        public void Parse(string dmFile)
        {
            using (StreamReader sr = File.OpenText(dmFile))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    if (s.Length < 1) continue; //empty line
                                                //pull comments first
                    int commentIndex = s.IndexOf(commentDelimiter);
                    string line = s;
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
                    //else unrecognizable command
                }
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

        public void Export(StreamWriter writer)
        {
            writer.WriteLine(CommandsMap.Format(Command.MODNAME, ModName));
            if (Version?.Length > 0) writer.WriteLine(CommandsMap.Format(Command.VERSION, Version));
            if (DomVersion?.Length > 0) writer.WriteLine(CommandsMap.Format(Command.DOMVERSION, DomVersion));
            if (Icon?.Length > 0) writer.WriteLine(CommandsMap.Format(Command.ICON, Icon));
            if (Description?.Length > 0) writer.WriteLine(CommandsMap.Format(Command.DESCRIPTION, Description));

            writer.WriteLine();

            foreach (Monster m in Monsters.Values)
            {
                m.Export(writer);
                writer.WriteLine();
            }
        }

        public Monster NewMonster(string val, string comment, bool selected = false)
        {
            Monster m = new Monster(val, comment);
            m.Parent = this;
            Monsters.Add(m.ID, m);
            m.Selected = selected;
            return m;
        }

        public Monster SelectMonster(string val, string comment)
        {
            if (int.TryParse(val, out int id) && Monsters.TryGetValue(id, out Monster m))
            {
                return m;
            }
            else
            {
                return NewMonster(val, comment, true);
            }
        }

        /*
        public void AddMonsterIDReference(int ID, MonIDRef reference)
        {
            if (!MonsterIDMap.TryGetValue(ID, out _))
            {
                MonsterIDMap.Add(ID, new List<MonIDRef>());
            }
            MonsterIDMap[ID].Add(reference);
        }

        public void AddSpellIDReference(int ID, SpellIDRef reference)
        {
            if (!SpellIDMap.TryGetValue(ID, out _))
            {
                SpellIDMap.Add(ID, new List<SpellIDRef>());
            }
            SpellIDMap[ID].Add(reference);
        }

        public void AddItemIDReference(int ID, ItemIDRef reference)
        {
            if (!ItemIDMap.TryGetValue(ID, out _))
            {
                ItemIDMap.Add(ID, new List<ItemIDRef>());
            }
            ItemIDMap[ID].Add(reference);
        }

        public void AddWeaponIDReference(int ID, WeaponIDRef reference)
        {
            if (!WeaponIDMap.TryGetValue(ID, out _))
            {
                WeaponIDMap.Add(ID, new List<WeaponIDRef>());
            }
            WeaponIDMap[ID].Add(reference);
        }

        public void AddArmorIDReference(int ID, ArmorIDRef reference)
        {
            if (!ArmorIDMap.TryGetValue(ID, out _))
            {
                ArmorIDMap.Add(ID, new List<ArmorIDRef>());
            }
            ArmorIDMap[ID].Add(reference);
        }

        public void AddSiteIDReference(int ID, SiteIDRef reference)
        {
            if (!SiteIDMap.TryGetValue(ID, out _))
            {
                SiteIDMap.Add(ID, new List<SiteIDRef>());
            }
            SiteIDMap[ID].Add(reference);
        }

        public void AddEnchIDReference(int ID, EnchIDRef reference)
        {
            if (!EnchIDMap.TryGetValue(ID, out _))
            {
                EnchIDMap.Add(ID, new List<EnchIDRef>());
            }
            EnchIDMap[ID].Add(reference);
        }

        public void AddMonsterNameReference(string name, MonsterNameRef reference)
        {
            if (!MonsterNameMap.TryGetValue(name, out _))
            {
                MonsterNameMap.Add(name, new List<MonsterNameRef>());
            }
            MonsterNameMap[name].Add(reference);
        }

        public void AddMontagIDReference(int ID, MontagIDRef reference)
        {
            if (!MontagIDMap.TryGetValue(ID, out _))
            {
                MontagIDMap.Add(ID, new List<MontagIDRef>());
            }
            MontagIDMap[ID].Add(reference);
        }

        public void AddNametypeIDReference(int ID, NametypeIDRef reference)
        {
            if (!NametypeIDMap.TryGetValue(ID, out _))
            {
                NametypeIDMap.Add(ID, new List<NametypeIDRef>());
            }
            NametypeIDMap[ID].Add(reference);
        }

        public void AddRestrictedItemIDReference(int ID, RestrictedItemIDRef reference)
        {
            if (!RestrictedItemIDMap.TryGetValue(ID, out _))
            {
                RestrictedItemIDMap.Add(ID, new List<RestrictedItemIDRef>());
            }
            RestrictedItemIDMap[ID].Add(reference);
        }
        */

        private int _MonStartID = 3500;
        public int GetNextMonsterID()
        {
            //very crude search unfortunately, but should be fine for our purposes
            while (Monsters.ContainsKey(_MonStartID))
            {
                _MonStartID++;
            }
            return _MonStartID;
        }

        private int _SpellStartID = 1300;
        public int GetNextSpellID()
        {
            //very crude search unfortunately, but should be fine for our purposes
            while (Spells.ContainsKey(_SpellStartID))
            {
                _SpellStartID++;
            }
            return _SpellStartID;
        }

        private int _ItemStartID = 500;
        public int GetNextItemID()
        {
            //very crude search unfortunately, but should be fine for our purposes
            while (Items.ContainsKey(_ItemStartID))
            {
                _ItemStartID++;
            }
            return _ItemStartID;
        }

        private int _WepStartID = 800;
        public int GetNextWeaponID()
        {
            //very crude search unfortunately, but should be fine for our purposes
            while (Weapons.ContainsKey(_WepStartID))
            {
                _WepStartID++;
            }
            return _WepStartID;
        }

        private int _ArmorStartID = 300;
        public int GetNextArmorID()
        {
            //very crude search unfortunately, but should be fine for our purposes
            while (Armors.ContainsKey(_ArmorStartID))
            {
                _ArmorStartID++;
            }
            return _ArmorStartID;
        }

        private int _EventStartID = 6000;
        public int GetNextEventID()
        {
            //very crude search unfortunately, but should be fine for our purposes
            while (Events.ContainsKey(_EventStartID))
            {
                _EventStartID++;
            }
            return _EventStartID;
        }

        private int _SiteStartID = 6000;
        public int GetNextSiteID()
        {
            //very crude search unfortunately, but should be fine for our purposes
            while (Sites.ContainsKey(_SiteStartID))
            {
                _SiteStartID++;
            }
            return _SiteStartID;
        }
    }
}
