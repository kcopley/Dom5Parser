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
            VanillaLoader.Vanilla.Export(file);
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
                    var list = _mod.Database[EntityType.MONSTER].GetFullList();
                    _monsters = new List<MonsterViewModel>();
                    foreach (var m in list)
                    {
                        _monsters.Add(new MonsterViewModel(m as Monster));
                    }
                }
                return _monsters;
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
