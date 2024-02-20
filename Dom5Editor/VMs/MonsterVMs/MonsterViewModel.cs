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

        public ICollectionView WeaponPropertiesView => _weaponPropertiesViewSource.View;
        public ICollectionView ArmorPropertiesView => _armorPropertiesViewSource.View;

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
        }

        public void SetMonster(Monster m)
        {
            this._entity = m;
        }

        public Monster Monster { get { return _entity as Monster; } }

        public ObservableCollection<Property> Attributes { get; }

        private void Properties_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Synchronize changes with the Monster's internal list
            _entity.Properties = Attributes.ToList();
        }

        public ICommand AddWeaponCommand { get; private set; }
        public ICommand AddArmorCommand { get; private set; }

        private void AddWeapon()
        {
            var newProperty = WeaponRef.Create();
            newProperty.Parent = _entity;
            newProperty.Parse(Command.WEAPON, "1", "");
            var vm = GetVM(newProperty);
            AllProperties.Add(vm);
        }

        private void AddArmor()
        {
            var newProperty = ArmorRef.Create();
            newProperty.Parent = _entity;
            newProperty.Parse(Command.ARMOR, "1", "");
            var vm = GetVM(newProperty);
            AllProperties.Add(vm);
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

        public NameViewModel Name
        {
            get
            {
                return new NameViewModel(_entity, Command.NAME);
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
                    return "(" + _entity.ID + ") " + _entity.Name;
                }
                else
                {
                    return "<No Name>";
                }
            }
        }

        public IntPropertyViewModel HitPoints
        {
            get
            {
                return new IntPropertyViewModel(this, "Hit Points:", _entity, Command.HP);
            }
        }

        public IntPropertyViewModel Size
        {
            get
            {
                return new IntPropertyViewModel(this, "Size:", _entity, Command.SIZE);
            }
        }

        public IntPropertyViewModel Prot
        {
            get
            {
                return new IntPropertyViewModel(this, "Nat Prot:", _entity, Command.PROT);
            }
        }

        public IntPropertyViewModel MR
        {
            get
            {
                return new IntPropertyViewModel(this, "Magic Resistance:", _entity, Command.MR);
            }
        }

        public IntPropertyViewModel Encumbrance
        {
            get
            {
                return new IntPropertyViewModel(this, "Encumbrance:", _entity, Command.ENC);
            }
        }

        public IntPropertyViewModel Attack
        {
            get
            {
                return new IntPropertyViewModel(this, "Attack:", _entity, Command.ATT);
            }
        }

        public IntPropertyViewModel Defense
        {
            get
            {
                return new IntPropertyViewModel(this, "Defense:", _entity, Command.DEF);
            }
        }

        public IntPropertyViewModel Strength
        {
            get
            {
                return new IntPropertyViewModel(this, "Strength:", _entity, Command.STR);
            }
        }

        public IntPropertyViewModel Precision
        {
            get
            {
                return new IntPropertyViewModel(this, "Precision:", _entity, Command.PREC);
            }
        }

        public IntPropertyViewModel CombatSpeed
        {
            get
            {
                return new IntPropertyViewModel(this, "Combat Speed:", _entity, Command.AP);
            }
        }

        public IntPropertyViewModel MapMove
        {
            get
            {
                return new IntPropertyViewModel(this, "Map Move:", _entity, Command.MAPMOVE);
            }
        }

        public IntPropertyViewModel Morale
        {
            get
            {
                return new IntPropertyViewModel(this, "Morale:", _entity, Command.MOR);
            }
        }

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
    }
}
