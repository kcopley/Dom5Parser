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
        private readonly string altCommentDelimiter = "-";

        public string ModName { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Version { get; set; }
        public string DomVersion { get; set; }

        public Dictionary<int, IDEntity> Monsters = new Dictionary<int, IDEntity>();
        public Dictionary<string, IDEntity> NamedMonsters = new Dictionary<string, IDEntity>();
        public Dictionary<int, IDEntity> Spells = new Dictionary<int, IDEntity>();
        public Dictionary<string, IDEntity> NamedSpells = new Dictionary<string, IDEntity>();
        public List<IDEntity> SpellsWithNoNameYet = new List<IDEntity>();
        public Dictionary<int, IDEntity> Items = new Dictionary<int, IDEntity>();
        public Dictionary<string, IDEntity> NamedItems = new Dictionary<string, IDEntity>();
        public List<IDEntity> ItemsWithNoNameYet = new List<IDEntity>();
        public Dictionary<int, IDEntity> Weapons = new Dictionary<int, IDEntity>();
        public Dictionary<string, IDEntity> NamedWeapons = new Dictionary<string, IDEntity>();
        public Dictionary<int, IDEntity> Armors = new Dictionary<int, IDEntity>();
        public Dictionary<string, IDEntity> NamedArmors = new Dictionary<string, IDEntity>();
        public Dictionary<int, IDEntity> Events = new Dictionary<int, IDEntity>();
        public Dictionary<int, IDEntity> Sites = new Dictionary<int, IDEntity>();
        public Dictionary<string, IDEntity> NamedSites = new Dictionary<string, IDEntity>();
        public List<IDEntity> SitesThatNeedIDs = new List<IDEntity>();
        public Dictionary<int, IDEntity> Nametypes = new Dictionary<int, IDEntity>();
        //public Dictionary<int, MontagIDRef> Montags = new Dictionary<int, MontagIDRef>();
        public Dictionary<int, RestrictedItemIDRef> RestrictedItems = new Dictionary<int, RestrictedItemIDRef>();
        public Dictionary<int, Ench> Enchantments = new Dictionary<int, Ench>();
        public Dictionary<int, IDEntity> Nations = new Dictionary<int, IDEntity>();
        public Dictionary<string, IDEntity> NamedNations = new Dictionary<string, IDEntity>();
        public List<IDEntity> NationsWithNoID = new List<IDEntity>();
        public Dictionary<int, IDEntity> Poptypes = new Dictionary<int, IDEntity>();

        public Dictionary<int, Montag> Montags = new Dictionary<int, Montag>();

        public Dictionary<string, IDEntity> NamedMercenaries = new Dictionary<string, IDEntity>();

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

        private Entity _currentEntity = null;

        public Mod()
        {
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
                        value = value.Trim('\"');
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

        public Montag AddNonAdjustedMontag(int ID)
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

        public int GetNextEventID()
        {
            //very crude search unfortunately, but should be fine for our purposes
            while (Events.ContainsKey(_EventStartID))
            {
                _EventStartID++;
            }
            return _EventStartID;
        }

        public int GetNextSiteID()
        {
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
