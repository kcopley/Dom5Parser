using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.VMs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Dom5Editor
{
    class AttributeInfo
    {
        public string PropertyName { get; set; }
        public string Label { get; set; }
        public PropertyViewModel ViewModel { get; set; }
    }

    public class MonsterViewModel : IDViewModelBase
    {
        private CollectionViewSource _weaponPropertiesViewSource;
        private CollectionViewSource _armorPropertiesViewSource;
        private readonly Dictionary<Command, AttributeInfo> _attributeInfos;

        public ObservableCollection<WeaponRefViewModel> WeaponProperties { get; } = new ObservableCollection<WeaponRefViewModel>();
        public ObservableCollection<ArmorRefViewModel> ArmorProperties { get; } = new ObservableCollection<ArmorRefViewModel>();

        public ICollectionView WeaponPropertiesView => _weaponPropertiesViewSource.View;
        public ICollectionView ArmorPropertiesView => _armorPropertiesViewSource.View;

        public Monster Monster { get { return _entity as Monster; } }
        public ObservableCollection<Property> Attributes { get; }
        public ICommand AddWeaponCommand { get; private set; }
        public ICommand AddArmorCommand { get; private set; }
        public ICommand RemoveWeaponCommand { get; }
        public ICommand RemoveArmorCommand { get; }

        public MonsterViewModel(ModViewModel mod, Monster monster)
        {
            _entity = monster;
            Parent = mod;

            //I can define singular attributes here, maybe?
            //Separate list that's referenced, in a hashset style

            //Core attributes that are displayed in the top section (and thus ignored in the AllProperties view)
            //Could do a sliced view off that but this is probably better
            CoreAttributes = new List<Command>()
            {
                Command.COPYSTATS,
                Command.COPYSPR,
                Command.HP,
                Command.SIZE,
                Command.PROT,
                Command.NAME,
                Command.ATT,
                Command.DEF,
                Command.MR,
                Command.SPR1,
                Command.SPR2,
                Command.DESCR,
                Command.MAXAGE,
                Command.MOR,
                Command.MAPMOVE,
                Command.AP,
                Command.PREC,
                Command.STR,
                Command.ENC
            };

            AddWeaponCommand = new RelayCommand(AddWeapon);
            AddArmorCommand = new RelayCommand(AddArmor);
            RemoveWeaponCommand = new RelayCommand<WeaponRefViewModel>(RemoveWeapon);
            RemoveArmorCommand = new RelayCommand<ArmorRefViewModel>(RemoveArmor);
            RefreshWeaponProperties();
            RefreshArmorProperties();
            RemoveCommand = new RelayCommand(RemoveProperty);
            //Attributes = new ObservableCollection<Property>(_entity.Properties);
            // Subscribe to CollectionChanged to update the Monster's list when the ObservableCollection changes
            //Attributes.CollectionChanged += Properties_CollectionChanged;

            _weaponPropertiesViewSource = new CollectionViewSource { Source = AllProperties };
            _weaponPropertiesViewSource.Filter += (s, e) =>
            {
                var item = e.Item as PropertyViewModel;
                e.Accepted = item != null && item.Command == Command.WEAPON;
            };

            _armorPropertiesViewSource = new CollectionViewSource { Source = AllProperties };
            _armorPropertiesViewSource.Filter += (s, e) =>
            {
                var item = e.Item as PropertyViewModel;
                e.Accepted = item != null && item.Command == Command.ARMOR;
            };

            var a = AllProperties;

            _attributeInfos = new Dictionary<Command, AttributeInfo>
            {
                { Command.HP, new AttributeInfo { PropertyName = nameof(HitPoints), Label = "Hit Points:" } },
                { Command.SIZE, new AttributeInfo { PropertyName = nameof(Size), Label = "Size:" } },
                { Command.PROT, new AttributeInfo { PropertyName = nameof(Prot), Label = "Nat Prot:" } },
                { Command.MR, new AttributeInfo { PropertyName = nameof(MR), Label = "Magic Resistance:" } },
                { Command.ATT, new AttributeInfo { PropertyName = nameof(Attack), Label = "Attack:" } },
                { Command.DEF, new AttributeInfo { PropertyName = nameof(Defense), Label = "Defense:" } },
                { Command.STR, new AttributeInfo { PropertyName = nameof(Strength), Label = "Strength:" } },
                { Command.PREC, new AttributeInfo { PropertyName = nameof(Precision), Label = "Precision:" } },
                { Command.AP, new AttributeInfo { PropertyName = nameof(CombatSpeed), Label = "Combat Speed:" } },
                { Command.MAPMOVE, new AttributeInfo { PropertyName = nameof(MapMove), Label = "Map Move:" } },
                { Command.MOR, new AttributeInfo { PropertyName = nameof(Morale), Label = "Morale:" } },
                { Command.ENC, new AttributeInfo { PropertyName = nameof(Encumbrance), Label = "Encumbrance:" } }
            };

            foreach (var kvp in _attributeInfos)
            {
                kvp.Value.ViewModel = new IntPropertyViewModel(this, kvp.Value.Label, _entity, kvp.Key);
            }

            _monsterName = _entity.Name;
        }

        public void SetMonster(Monster m)
        {
            this._entity = m;
        }

        private void AddWeapon()
        {
            var newProperty = WeaponRef.Create();
            newProperty.Parent = _entity;
            newProperty.Parse(Command.WEAPON, "1", "");
            _entity.AddProperty(newProperty);
            RefreshWeaponProperties();
        }

        private void AddArmor()
        {
            var newProperty = ArmorRef.Create();
            newProperty.Parent = _entity;
            newProperty.Parse(Command.ARMOR, "1", "");
            _entity.AddProperty(newProperty);
            RefreshArmorProperties();
        }

        private void RemoveWeapon(WeaponRefViewModel weaponVM)
        {
            _entity.RemoveProperty(weaponVM._property);
            RefreshWeaponProperties();
        }

        private void RemoveArmor(ArmorRefViewModel armorVM)
        {
            _entity.RemoveProperty(armorVM._property);
            RefreshArmorProperties();
        }

        private void RefreshWeaponProperties()
        {
            WeaponProperties.Clear();
            foreach (var prop in _entity.Properties.OfType<WeaponRef>())
            {
                WeaponProperties.Add(new WeaponRefViewModel(Parent, this, _entity, prop));
            }
            OnPropertyChanged(nameof(WeaponProperties));
        }

        private void RefreshArmorProperties()
        {
            ArmorProperties.Clear();
            foreach (var prop in _entity.Properties.OfType<ArmorRef>())
            {
                ArmorProperties.Add(new ArmorRefViewModel(Parent, this, _entity, prop));
            }
            OnPropertyChanged(nameof(ArmorProperties));
        }

        public CopyStatsRefViewModel CopyRef
        {
            get { return new CopyStatsRefViewModel(Parent, this, "Copies From:", _entity, Command.COPYSTATS); }
        }

        public int ID
        {
            get
            {
                return _entity != null ? _entity.ID : -1;
            }
            set
            {
                if (_entity != null) _entity.ID = value;
            }
        }

        private string _monsterName;
        public string MonsterName
        {
            get => _monsterName;
            set
            {
                if (_monsterName != value)
                {
                    _monsterName = value;
                    if (_entity != null)
                    {
                        _entity.Name = value;
                    }
                    OnPropertyChanged(nameof(MonsterName));
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        private NameViewModel _name;
        public NameViewModel Name
        {
            get
            {
                if (_name == null)
                {
                    _name = new NameViewModel(
                        "Name:",  // Add a label
                        _entity,
                        Command.NAME,
                        () => MonsterName,
                        value => MonsterName = value
                    );
                }
                return _name;
            }
        }

        public DescriptionViewModel Description
        {
            get
            {
                return new DescriptionViewModel(_entity, Command.DESCR);
            }
        }

        public override string DisplayName
        {
            get
            {
                if (_entity != null)
                {
                    return $"({_entity.ID}) {MonsterName}";
                }
                else
                {
                    return "<No Name>";
                }
            }
        }

        public PropertyViewModel GetAttribute(Command command)
        {
            return _attributeInfos.TryGetValue(command, out var info) ? info.ViewModel : null;
        }

        // Property definitions
        public IntPropertyViewModel HitPoints => GetAttribute(Command.HP) as IntPropertyViewModel;
        public IntPropertyViewModel Size => GetAttribute(Command.SIZE) as IntPropertyViewModel;
        public IntPropertyViewModel Prot => GetAttribute(Command.PROT) as IntPropertyViewModel;
        public IntPropertyViewModel MR => GetAttribute(Command.MR) as IntPropertyViewModel;
        public IntPropertyViewModel Morale => GetAttribute(Command.MOR) as IntPropertyViewModel;
        public IntPropertyViewModel Attack => GetAttribute(Command.ATT) as IntPropertyViewModel;
        public IntPropertyViewModel Defense => GetAttribute(Command.DEF) as IntPropertyViewModel;
        public IntPropertyViewModel Strength => GetAttribute(Command.STR) as IntPropertyViewModel;
        public IntPropertyViewModel Precision => GetAttribute(Command.PREC) as IntPropertyViewModel;
        public IntPropertyViewModel CombatSpeed => GetAttribute(Command.AP) as IntPropertyViewModel;
        public IntPropertyViewModel MapMove => GetAttribute(Command.MAPMOVE) as IntPropertyViewModel;
        public IntPropertyViewModel Encumbrance => GetAttribute(Command.ENC) as IntPropertyViewModel;

        public BitmapSource SpriteImage
        {
            get
            {
                var exists = _entity.TryGet<FilePathProperty>(Command.SPR1, out var property);
                if (exists == ReturnType.TRUE || exists == ReturnType.COPIED)
                {
                    var spriteAdjusted = property.Value.Trim('.').Trim('/').Replace("/", "\\");
                    var dir = Path.GetDirectoryName(_entity.ParentMod.FullFilePath);

                    var filePath = dir + '\\' + spriteAdjusted;
                    try
                    {
                        var targa = Paloma.TargaImage.LoadTargaImage(filePath);

                        var ret = targa.ConvertToImage();
                        return ret;
                    }
                    catch (Exception e)
                    {
                        return new BitmapImage();
                    }
                }
                return new BitmapImage();
            }
        }

        public BitmapSource Sprite2Image
        {
            get
            {
                var exists = _entity.TryGet<FilePathProperty>(Command.SPR2, out var property);
                if (exists == ReturnType.TRUE || exists == ReturnType.COPIED)
                {
                    var spriteAdjusted = property.Value.Trim('.').Trim('/').Replace("/", "\\");
                    var dir = Path.GetDirectoryName(_entity.ParentMod.FullFilePath);

                    var filePath = dir + '\\' + spriteAdjusted;
                    try
                    {
                        var targa = Paloma.TargaImage.LoadTargaImage(filePath);

                        var ret = targa.ConvertToImage();
                        return ret;
                    }
                    catch (Exception e)
                    {
                        return new BitmapImage();
                    }
                }
                return new BitmapImage();
            }
        }

        public ICommand RemoveCommand { get; private set; }

        private void RemoveProperty(object parameter)
        {
            if (parameter is PropertyViewModel propertyVM)
            {
                _entity.RemoveProperty(propertyVM.Command);

                // Update AllProperties
                OnPropertyChanged(nameof(AllProperties));

                // Update the specific attribute property
                if (_attributeInfos.TryGetValue(propertyVM.Command, out var info))
                {
                    OnPropertyChanged(info.PropertyName);
                    info.ViewModel.OnPropertyChanged(string.Empty);
                }
            }
        }
    }
}
