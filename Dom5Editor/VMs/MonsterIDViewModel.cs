using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Editor.VMs
{
    public class MonsterIDViewModel : PropertyViewModel
    {
        public MonsterIDViewModel(MonsterViewModel monster, IDEntity e, Command c) : base(e, c)
        {
            _monster = monster;
        }
        public MonsterIDViewModel(MonsterViewModel monster, string label, IDEntity e, Command c) : base(label, e, c)
        {
            _monster = monster;
        }

        internal MonsterViewModel _monster;

        public override string Value
        {
            get
            {
                switch (Source.TryGet(_command, out StringOrIDRef ip))
                {
                    case ReturnType.FALSE:
                        break;
                    case ReturnType.COPIED:
                        //set to greyed out?
                        return ip.Command.ToString();
                    case ReturnType.TRUE:
                        return ip.Command.ToString();
                }
                return "";
            }
            set
            {
                Source.Set<StringOrIDRef>(_command, id => id.SetEntity(value));
                OnPropertyChanged(_command.ToString());
            }
        }

        public List<MonsterViewModel> IDs
        {
            get { return _monster.Parent.Monsters; }
        }

        public MonsterViewModel SelectedID
        {
            get
            { 
                if (Source.TryGet<CopyStatsRef>(_command, out var rf) == ReturnType.TRUE)
                {
                    return _monster.Parent.Monsters.Find(x => x.ID == rf.ID);
                }
                return null;
            }
            set
            {
                if (this._command == Command.COPYSTATS)
                {
                    Source.Set<CopyStatsRef>(_command, id => id.Entity = value.Monster);

                }
                else
                {
                    Source.Set<StringOrIDRef>(_command, id => id.Entity = value.Monster);
                }
                _monster.OnPropertyChanged("");
            }
        }
    }
}
