using Dom5Edit;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Dom5Editor
{
    public class ModViewModel : ViewModelBase
    {
        private Mod _mod;

        public ModViewModel(string file)
        {
            this._mod = Mod.Import(file);
        }

        public ModViewModel(Mod m)
        {
            this._mod = m;
        }

        public bool Save(string file)
        {
            _mod.Export(file);
            return true;
        }

        public string ModFileName { get { return _mod.ModFileName; } }
        public string FullFilePath { get { return _mod.FullFilePath; } }

        public string ModName { get { return _mod.ModName; } set { _mod.ModName = value; } }
        public string ModDescription { get { return _mod.Description; } set { _mod.Description = value; } }
        public string Version { get { return _mod.Version; } set { _mod.Version = value; } }
        public string DomVersion { get { return _mod.DomVersion; } set { _mod.DomVersion = value; } }

        private MonsterViewModel _openMonster;
        public MonsterViewModel OpenMonster
        {
            get
            {
                if (_openMonster == null)
                {
                    _openMonster = null;
                }
                return _openMonster;
            }
            set
            {
                _openMonster = value;
                OnPropertyChanged("OpenMonster");
            }
        }
        private ObservableCollection<MonsterViewModel> _openMonsters;
        public ObservableCollection<MonsterViewModel> OpenMonsters
        {
            get
            {
                if (_openMonsters == null)
                {
                    _openMonsters = new ObservableCollection<MonsterViewModel>();
                }
                return _openMonsters;
            }
        }

        private List<MonsterViewModel> _monsters;
        public List<MonsterViewModel> Monsters
        {
            get
            {
                if (_monsters == null)
                {
                    var list = VanillaLoader.Vanilla.Database[EntityType.MONSTER].GetFullList();
                    _monsters = new List<MonsterViewModel>();
                    foreach (var m in list)
                    {
                        _monsters.Add(new MonsterViewModel(this, m as Monster));
                    }
                    
                    list = _mod.Database[EntityType.MONSTER].GetFullList();
                    foreach (var m in list)
                    {
                        _monsters.Add(new MonsterViewModel(this, m as Monster));
                    }
                }
                return _monsters;
            }
        }

        private WeaponViewModel _openWeapon;
        public WeaponViewModel OpenWeapon
        {
            get => _openWeapon;
            set
            {
                if (_openWeapon != value)
                {
                    _openWeapon = value;
                    OnPropertyChanged(nameof(OpenWeapon));
                }
            }
        }

        private ObservableCollection<WeaponViewModel> _openWeapons;
        public ObservableCollection<WeaponViewModel> OpenWeapons
        {
            get
            {
                if (_openWeapons == null)
                {
                    _openWeapons = new ObservableCollection<WeaponViewModel>();
                }
                return _openWeapons;
            }
        }

        private List<WeaponViewModel> _Weapons;
        public List<WeaponViewModel> Weapons
        {
            get
            {
                if (_Weapons == null)
                {
                    _Weapons = new List<WeaponViewModel>();
                    var list = VanillaLoader.Vanilla.Database[EntityType.WEAPON].GetFullList();
                    foreach (var m in list)
                    {
                        _Weapons.Add(new WeaponViewModel(this, m as Weapon));
                    }
                    list = _mod.Database[EntityType.WEAPON].GetFullList();
                    foreach (var m in list)
                    {
                        _Weapons.Add(new WeaponViewModel(this, m as Weapon));
                    }

                }
                return _Weapons;
            }
        }

        private ArmorViewModel _openArmor;
        public ArmorViewModel OpenArmor
        {
            get => _openArmor;
            set
            {
                if (_openArmor != value)
                {
                    _openArmor = value;
                    OnPropertyChanged(nameof(OpenArmor));
                }
            }
        }

        private List<ArmorViewModel> _armors;
        public List<ArmorViewModel> Armors
        {
            get
            {
                if (_armors == null)
                {
                    _armors = new List<ArmorViewModel>();
                    var list = VanillaLoader.Vanilla.Database[EntityType.ARMOR].GetFullList();
                    foreach (var m in list)
                    {
                        _armors.Add(new ArmorViewModel(this, m as Armor));
                    }
                    list = _mod.Database[EntityType.ARMOR].GetFullList();
                    foreach (var a in list)
                    {
                        _armors.Add(new ArmorViewModel(this, a as Armor));
                    }
                }
                return _armors;
            }
        }

        private SiteViewModel _openSite;
        public SiteViewModel OpenSite
        {
            get
            {
                if (_openSite == null)
                {
                    _openSite = null;
                }
                return _openSite;
            }
            set
            {
                _openSite = value;
                OnPropertyChanged(nameof(OpenSite));
            }
        }
        private ObservableCollection<SiteViewModel> _openSites;
        public ObservableCollection<SiteViewModel> OpenSites
        {
            get
            {
                if (_openSites == null)
                {
                    _openSites = new ObservableCollection<SiteViewModel>();
                }
                return _openSites;
            }
        }

        private List<SiteViewModel> _sites;
        public List<SiteViewModel> Sites
        {
            get
            {
                if (_sites == null)
                {
                    var list = VanillaLoader.Vanilla.Database[EntityType.SITE].GetFullList();
                    _sites = new List<SiteViewModel>();
                    foreach (var s in list)
                    {
                        _sites.Add(new SiteViewModel(this, s as Site));
                    }
                    list = _mod.Database[EntityType.SITE].GetFullList();
                    foreach (var i in list)
                    {
                        _sites.Add(new SiteViewModel(this, i as Site));
                    }
                }
                return _sites;
            }
        }

        private ItemViewModel _openItem;
        public ItemViewModel OpenItem
        {
            get => _openItem;
            set
            {
                if (_openItem != value)
                {
                    _openItem = value;
                    OnPropertyChanged(nameof(OpenItem));
                }
            }
        }

        private List<ItemViewModel> _items;
        public List<ItemViewModel> Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new List<ItemViewModel>();
                    var list = VanillaLoader.Vanilla.Database[EntityType.ITEM].GetFullList();
                    foreach (var i in list)
                    {
                        _items.Add(new ItemViewModel(this, i as Item));
                    }
                    list = _mod.Database[EntityType.ITEM].GetFullList();
                    foreach (var i in list)
                    {
                        _items.Add(new ItemViewModel(this, i as Item));
                    }
                }
                return _items;
            }
        }

        private SpellViewModel _openSpell;
        public SpellViewModel OpenSpell
        {
            get => _openSpell;
            set
            {
                if (_openSpell != value)
                {
                    _openSpell = value;
                    OnPropertyChanged(nameof(OpenSpell));
                }
            }
        }

        private List<SpellViewModel> _spells;
        public List<SpellViewModel> Spells
        {
            get
            {
                if (_spells == null)
                {
                    _spells = new List<SpellViewModel>();
                    var list = VanillaLoader.Vanilla.Database[EntityType.SPELL].GetFullList();
                    foreach (var s in list)
                    {
                        _spells.Add(new SpellViewModel(this, s as Spell));
                    }
                    list = _mod.Database[EntityType.SPELL].GetFullList();
                    foreach (var s in list)
                    {
                        _spells.Add(new SpellViewModel(this, s as Spell));
                    }
                }
                return _spells;
            }
        }

        private NationViewModel _openNation;
        public NationViewModel OpenNation
        {
            get => _openNation;
            set
            {
                _openNation = value;
                OnPropertyChanged(nameof(OpenNation));
            }
        }

        private List<NationViewModel> _nations;
        public List<NationViewModel> Nations
        {
            get
            {
                if (_nations == null)
                {
                    _nations = new List<NationViewModel>();
                    var list = VanillaLoader.Vanilla.Database[EntityType.NATION].GetFullList();
                    foreach (var n in list)
                    {
                        _nations.Add(new NationViewModel(this, n as Nation));
                    }
                    list = _mod.Database[EntityType.NATION].GetFullList();
                    foreach (var n in list)
                    {
                        _nations.Add(new NationViewModel(this, n as Nation));
                    }
                }
                return _nations;
            }
        }

        // New properties for Mercenaries
        private MercenaryViewModel _openMercenary;
        public MercenaryViewModel OpenMercenary
        {
            get => _openMercenary;
            set
            {
                _openMercenary = value;
                OnPropertyChanged(nameof(OpenMercenary));
            }
        }

        private List<MercenaryViewModel> _mercenaries;
        public List<MercenaryViewModel> Mercenaries
        {
            get
            {
                if (_mercenaries == null)
                {
                    _mercenaries = new List<MercenaryViewModel>();
                    var list = VanillaLoader.Vanilla.Database[EntityType.MERCENARY].GetFullList();
                    foreach (var m in list)
                    {
                        _mercenaries.Add(new MercenaryViewModel(this, m as Mercenary));
                    }
                    list = _mod.Database[EntityType.MERCENARY].GetFullList();
                    foreach (var m in list)
                    {
                        _mercenaries.Add(new MercenaryViewModel(this, m as Mercenary));
                    }
                }
                return _mercenaries;
            }
        }

        // New properties for Poptypes
        private PoptypeViewModel _openPoptype;
        public PoptypeViewModel OpenPoptype
        {
            get => _openPoptype;
            set
            {
                _openPoptype = value;
                OnPropertyChanged(nameof(OpenPoptype));
            }
        }

        private List<PoptypeViewModel> _poptypes;
        public List<PoptypeViewModel> Poptypes
        {
            get
            {
                if (_poptypes == null)
                {
                    _poptypes = new List<PoptypeViewModel>();
                    var list = VanillaLoader.Vanilla.Database[EntityType.POPTYPE].GetFullList();
                    foreach (var p in list)
                    {
                        _poptypes.Add(new PoptypeViewModel(this, p as Poptype));
                    }
                    list = _mod.Database[EntityType.POPTYPE].GetFullList();
                    foreach (var p in list)
                    {
                        _poptypes.Add(new PoptypeViewModel(this, p as Poptype));
                    }
                }
                return _poptypes;
            }
        }

        // New properties for Nametypes
        private NametypeViewModel _openNametype;
        public NametypeViewModel OpenNametype
        {
            get => _openNametype;
            set
            {
                _openNametype = value;
                OnPropertyChanged(nameof(OpenNametype));
            }
        }

        private List<NametypeViewModel> _nametypes;
        public List<NametypeViewModel> Nametypes
        {
            get
            {
                if (_nametypes == null)
                {
                    _nametypes = new List<NametypeViewModel>();
                    var list = VanillaLoader.Vanilla.Database[EntityType.NAMETYPE].GetFullList();
                    foreach (var n in list)
                    {
                        _nametypes.Add(new NametypeViewModel(this, n as Nametype));
                    }
                    list = _mod.Database[EntityType.NAMETYPE].GetFullList();
                    foreach (var n in list)
                    {
                        _nametypes.Add(new NametypeViewModel(this, n as Nametype));
                    }
                }
                return _nametypes;
            }
        }

        // New properties for Events
        private EventViewModel _openEvent;
        public EventViewModel OpenEvent
        {
            get => _openEvent;
            set
            {
                _openEvent = value;
                OnPropertyChanged(nameof(OpenEvent));
            }
        }

        private List<EventViewModel> _events;
        public List<EventViewModel> Events
        {
            get
            {
                if (_events == null)
                {
                    _events = new List<EventViewModel>();
                    var list = VanillaLoader.Vanilla.Database[EntityType.EVENT].GetFullList();
                    foreach (var e in list)
                    {
                        _events.Add(new EventViewModel(this, e as Event));
                    }
                    list = _mod.Database[EntityType.EVENT].GetFullList();
                    foreach (var e in list)
                    {
                        _events.Add(new EventViewModel(this, e as Event));
                    }
                }
                return _events;
            }
        }

        public string Icon
        {
            get
            {
                return _mod?.Icon;
            }
            set
            {
                if (_mod != null) _mod.Icon = value;
            }
        }

        public BitmapSource IconBitmap
        {
            get
            {
                if (Icon == null) return new BitmapImage();
                string test = Path.GetFileName(Icon);
                var spriteAdjusted = Icon.Trim('.').Trim('/').Replace("/", "\\");
                var dir = Path.GetDirectoryName(_mod.FullFilePath);

                var filePath = dir + '\\' + spriteAdjusted;
                try
                {
                    var targa = Paloma.TargaImage.LoadTargaImage(filePath);
                    return targa.ConvertToImage();
                }
                catch (Exception e)
                {
                    return new BitmapImage();
                }
            }
        }
    }
}
