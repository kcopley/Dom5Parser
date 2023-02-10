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
        public ModViewModel Parent { get; }

        public MonsterViewModel(ModViewModel mod, Monster monster)
        {
            _monster = monster;
            Parent = mod;
        }

        public void SetMonster(Monster m)
        {
            this._monster = m;
        }

        public Monster Monster { get { return _monster; } }

        public MonsterIDViewModel CopyRef
        {
            get { return new MonsterIDViewModel(this, "Copies From:", _monster, Command.COPYSTATS); }
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

        public NameViewModel Name
        {
            get
            {
                return new NameViewModel(_monster, Command.NAME);
            }
        }

        public DescriptionViewModel Description
        {
            get
            {
                return new DescriptionViewModel(_monster, Command.DESCR);
            }
        }

        public override string DisplayName
        {
            get
            {
                if (_monster != null)
                {
                    return "(" + _monster.ID + ") " + _monster.Name;
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
                return new IntPropertyViewModel(this, "Hit Points:", _monster, Command.HP);
            }
        }
        
        public IntPropertyViewModel Size
        {
            get
            {
                return new IntPropertyViewModel(this, "Size:", _monster, Command.SIZE);
            }
        }

        public IntPropertyViewModel Prot
        {
            get
            {
                return new IntPropertyViewModel(this, "Nat Prot:", _monster, Command.PROT);
            }
        }

        public IntPropertyViewModel MR
        {
            get
            {
                return new IntPropertyViewModel(this, "Magic Resistance:", _monster, Command.MR);
            }
        }

        public IntPropertyViewModel Encumbrance
        {
            get
            {
                return new IntPropertyViewModel(this, "Encumbrance:", _monster, Command.ENC);
            }
        }

        public IntPropertyViewModel Attack
        {
            get
            {
                return new IntPropertyViewModel(this, "Attack:", _monster, Command.ATT);
            }
        }

        public IntPropertyViewModel Defense
        {
            get
            {
                return new IntPropertyViewModel(this, "Defense:", _monster, Command.DEF);
            }
        }

        public IntPropertyViewModel Strength
        {
            get
            {
                return new IntPropertyViewModel(this, "Strength:", _monster, Command.STR);
            }
        }

        public IntPropertyViewModel Precision
        {
            get
            {
                return new IntPropertyViewModel(this, "Precision:", _monster, Command.PREC);
            }
        }

        public IntPropertyViewModel CombatSpeed
        {
            get
            {
                return new IntPropertyViewModel(this, "Combat Speed:", _monster, Command.AP);
            }
        }

        public IntPropertyViewModel MapMove
        {
            get
            {
                return new IntPropertyViewModel(this, "Map Move:", _monster, Command.MAPMOVE);
            }
        }

        public IntPropertyViewModel Morale
        {
            get
            {
                return new IntPropertyViewModel(this, "Morale:", _monster, Command.MOR);
            }
        }

        public MonsterGearViewModel Weapons
        {
            get
            {
                return new MonsterGearViewModel("Morale:", _monster, Command.MOR);
            }
        }

        List<Command> CoreAttributes = new List<Command>()
        {
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
            Command.ENC,

        };

        public List<ViewModelBase> ExtraCommands
        {
            get
            {
                List<ViewModelBase> list = new List<ViewModelBase>();
                foreach (var prop in _monster.Properties)
                {
                    if (!CoreAttributes.Contains(prop.Command))
                    {
                        list.Add(GetVM(prop));
                    }
                }
                return list;
            }
        }

        private ViewModelBase GetVM(Property p)
        {
            var t = p.GetType();
            if (t.InheritsFrom(typeof(IntProperty)))
            {
                return new IntPropertyViewModel(p.Command.ToString(), _monster, p.Command);
            }
            if (t.InheritsFrom(typeof(StringProperty)))
            {
                return new StringViewModel(p.Command.ToString(), _monster, p.Command);
            }
            if (t.InheritsFrom(typeof(CommandProperty)))
            {
                return new CommandViewModel(p.Command.ToString(), _monster, p.Command);
            }
            return new CommandViewModel(p.Command.ToString(), _monster, p.Command);
        }

        public BitmapSource SpriteImage
        {
            get
            {
                var exists = _monster.TryGet<FilePathProperty>(Command.SPR1, out var property);
                if (exists == ReturnType.TRUE || exists == ReturnType.COPIED)
                {
                    var spriteAdjusted = property.Value.Trim('.').Trim('/').Replace("/", "\\");
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
                return new BitmapImage();
            }
        }

        public BitmapSource Sprite2Image
        {
            get
            {
                var exists = _monster.TryGet<FilePathProperty>(Command.SPR2, out var property);
                if (exists == ReturnType.TRUE || exists == ReturnType.COPIED)
                {
                    var spriteAdjusted = property.Value.Trim('.').Trim('/').Replace("/", "\\");
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
                return new BitmapImage();
            }
        }
    }
}
