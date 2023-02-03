using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.VMs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Dom5Editor
{
    public class MonsterViewModel : ViewModelBase
    {
        private Monster _monster;

        public MonsterViewModel(Monster monster)
        {
            _monster = monster;
        }

        public void SetMonster(Monster m)
        {
            this._monster = m;
        }

        public int ID
        {
            get
            {
                return _monster != null ? _monster.ID : -1;
            }
            set
            {
                if (_monster != null) _monster.ID = value;
            }
        }

        public string Name
        {
            get
            {
                return _monster?.Name;
            }
            set
            {
                if (_monster != null) _monster.Name = value;
            }
        }

        public override string DisplayName
        {
            get
            {
                return "(" + this.ID + ") " + this.Name;
            }
        }

        private IntPropertyViewModel _hp;
        public IntPropertyViewModel HitPoints
        {
            get
            {
                if (_monster != null)
                {
                    if (_hp == null || _hp.Source != _monster)
                    {
                        if (_monster.TryGet<IntProperty>(Command.HP, out IntProperty prop) == ReturnType.TRUE)
                            _hp = new IntPropertyViewModel("Hit Points", _monster, Command.HP);
                    }
                    return _hp;
                }
                throw new NullReferenceException();
            }
        }

        public int Size
        {
            get
            {
                return _monster != null ? _monster.Get<IntProperty>(Command.SIZE).Value : -1;
            }
            set
            {
                var clamped = Math.Max(Math.Min(6, value), 0);
                if (_monster != null) _monster.Get<IntProperty>(Command.SIZE, true).Value = clamped;
            }
        }

        public int MagicResistance
        {
            get
            {
                return _monster != null ? _monster.Get<IntProperty>(Command.MR).Value : -1;
            }
            set
            {
                if (_monster != null) _monster.Get<IntProperty>(Command.MR, true).Value = value;
            }
        }

        public int Strength
        {
            get
            {
                return _monster != null ? _monster.Get<IntProperty>(Command.STR).Value : -1;
            }
            set
            {
                if (_monster != null) _monster.Get<IntProperty>(Command.STR, true).Value = value;
            }
        }

        public int Attack
        {
            get
            {
                return _monster != null ? _monster.Get<IntProperty>(Command.ATT).Value : -1;
            }
            set
            {
                if (_monster != null) _monster.Get<IntProperty>(Command.ATT, true).Value = value;
            }
        }

        public int Defense
        {
            get
            {
                return _monster != null ? _monster.Get<IntProperty>(Command.DEF).Value : -1;
            }
            set
            {
                if (_monster != null) _monster.Get<IntProperty>(Command.DEF, true).Value = value;
            }
        }

        public int Precision
        {
            get
            {
                return _monster != null ? _monster.Get<IntProperty>(Command.PREC).Value : -1;
            }
            set
            {
                if (_monster != null) _monster.Get<IntProperty>(Command.PREC, true).Value = value;
            }
        }

        public List<CommandProperty> ExtraCommands
        {
            get
            {
                return _monster.GetCommandProperties().Cast<CommandProperty>().ToList();
            }
        }

        public string Sprite
        {
            get
            {
                return _monster != null ? _monster.Get<FilePathProperty>(Command.SPR1).Value : "";
            }
            set
            {
                if (_monster != null) _monster.Get<FilePathProperty>(Command.SPR1, true).Value = value;
            }
        }

        public string Sprite2
        {
            get
            {
                return _monster != null ? _monster.Get<FilePathProperty>(Command.SPR2).Value : "";
            }
            set
            {
                if (_monster != null) _monster.Get<FilePathProperty>(Command.SPR2, true).Value = value;
            }
        }

        public BitmapSource SpriteImage
        {
            get
            {
                string test = Path.GetFileName(Sprite);
                var spriteAdjusted = Sprite.Trim('.').Trim('/').Replace("/", "\\");
                var dir = Path.GetDirectoryName(_monster.ParentMod.FullFilePath);

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
        }

        public BitmapSource Sprite2Image
        {
            get
            {
                string test = Path.GetFileName(Sprite2);
                var spriteAdjusted = Sprite2.Trim('.').Trim('/').Replace("/", "\\");
                var dir = Path.GetDirectoryName(_monster.ParentMod.FullFilePath);

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
        }
    }
}
