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
    public class MonsterViewModel : IDViewModelBase
    {
        private CollectionViewSource _weaponPropertiesViewSource;
        private CollectionViewSource _armorPropertiesViewSource;

        public ObservableCollection<WeaponRefViewModel> WeaponProperties { get; } = new ObservableCollection<WeaponRefViewModel>();
        public ObservableCollection<ArmorRefViewModel> ArmorProperties { get; } = new ObservableCollection<ArmorRefViewModel>();

        public ICollectionView WeaponPropertiesView => _weaponPropertiesViewSource.View;
        public ICollectionView ArmorPropertiesView => _armorPropertiesViewSource.View;

        public ICommand AddWeaponCommand { get; private set; }
        public ICommand AddArmorCommand { get; private set; }
        public ICommand RemoveWeaponCommand { get; private set; }
        public ICommand RemoveArmorCommand { get; private set; }

        public MonsterViewModel(ModViewModel mod, Monster monster) : base(mod, monster)
        {
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

            InitializeAttributeInfos();
            InitializeCommands();
            InitializeCollectionViews();
            RefreshEntityProperties();
        }

        protected override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return Monster._propertyMap;
        }

        protected override void InitializeAttributeInfos()
        {
            base.InitializeAttributeInfos();

            _attributeInfos[Command.HP] = new AttributeInfo { PropertyName = nameof(HitPoints), Label = "Hit Points:" };
            _attributeInfos[Command.SIZE] = new AttributeInfo { PropertyName = nameof(Size), Label = "Size:" };
            _attributeInfos[Command.PROT] = new AttributeInfo { PropertyName = nameof(Prot), Label = "Nat Prot:" };
            _attributeInfos[Command.MR] = new AttributeInfo { PropertyName = nameof(MR), Label = "Magic Resistance:" };
            _attributeInfos[Command.ATT] = new AttributeInfo { PropertyName = nameof(Attack), Label = "Attack:" };
            _attributeInfos[Command.DEF] = new AttributeInfo { PropertyName = nameof(Defense), Label = "Defense:" };
            _attributeInfos[Command.STR] = new AttributeInfo { PropertyName = nameof(Strength), Label = "Strength:" };
            _attributeInfos[Command.PREC] = new AttributeInfo { PropertyName = nameof(Precision), Label = "Precision:" };
            _attributeInfos[Command.AP] = new AttributeInfo { PropertyName = nameof(CombatSpeed), Label = "Combat Speed:" };
            _attributeInfos[Command.MAPMOVE] = new AttributeInfo { PropertyName = nameof(MapMove), Label = "Map Move:" };
            _attributeInfos[Command.MOR] = new AttributeInfo { PropertyName = nameof(Morale), Label = "Morale:" };
            _attributeInfos[Command.ENC] = new AttributeInfo { PropertyName = nameof(Encumbrance), Label = "Encumbrance:" };

            //This is where a property is defined to be rendered in advance, as a fixed part of the UI, rather than dynamically generated from the property.
            foreach (var kvp in _attributeInfos)
            {
                if (kvp.Key == Command.NAME)
                {
                    kvp.Value.ViewModel = new StringViewModel(kvp.Value.Label, _entity, kvp.Key);
                }
                else
                {
                    kvp.Value.ViewModel = new IntPropertyViewModel(this, kvp.Value.Label, _entity, kvp.Key);
                }
            }
        }

        private void InitializeCommands()
        {
            AddWeaponCommand = new RelayCommand(AddWeapon);
            AddArmorCommand = new RelayCommand(AddArmor);
            RemoveWeaponCommand = new RelayCommand<WeaponRefViewModel>(RemoveWeapon);
            RemoveArmorCommand = new RelayCommand<ArmorRefViewModel>(RemoveArmor);
        }

        private void InitializeCollectionViews()
        {
            _weaponPropertiesViewSource = new CollectionViewSource { Source = EntityProperties };
            _weaponPropertiesViewSource.Filter += (s, e) =>
            {
                var item = e.Item as PropertyViewModel;
                e.Accepted = item != null && item.Command == Command.WEAPON;
            };

            _armorPropertiesViewSource = new CollectionViewSource { Source = EntityProperties };
            _armorPropertiesViewSource.Filter += (s, e) =>
            {
                var item = e.Item as PropertyViewModel;
                e.Accepted = item != null && item.Command == Command.ARMOR;
            };

            RefreshWeaponProperties();
            RefreshArmorProperties();
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

        public Monster Monster { get { return _entity as Monster; } }

        public void SetMonster(Monster m)
        {
            SetEntity(m);
            RefreshWeaponProperties();
            RefreshArmorProperties();
        }

        public CopyStatsRefViewModel CopyRef
        {
            get { return new CopyStatsRefViewModel(Parent, this, "Copies From:", _entity, Command.COPYSTATS); }
        }

        public DescriptionViewModel Description
        {
            get { return new DescriptionViewModel(_entity, Command.DESCR); }
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
                        return targa.ConvertToImage();
                    }
                    catch (Exception)
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
                        return targa.ConvertToImage();
                    }
                    catch (Exception)
                    {
                        return new BitmapImage();
                    }
                }
                return new BitmapImage();
            }
        }
    }
}
