using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class MonsterOrMontagRef : Reference
    {
        private MontagIDRef _montagRef;
        private MonsterRef _monsterRef;

        public static Property Create()
        {
            return new MonsterOrMontagRef();
        }

        public override void Resolve()
        {
            if (_montagRef != null)
            {
                _montagRef.Resolve();
            }
            else
            {
                _monsterRef.Resolve();
            }
        }

        public override void Parse(Command c, string v, string comment)
        {
            if (int.TryParse(v, out int i))
            {
                if (i < 0) //montag
                {
                    _montagRef = new MontagIDRef();
                    _montagRef.Parent = this.Parent;
                    _montagRef.Parse(c, v, comment);
                }
                else //monster
                {
                    _monsterRef = new MonsterRef();
                    _monsterRef.Parent = this.Parent;
                    _monsterRef.Parse(c, v, comment);
                }
            }
        }

        public override string ToString()
        {
            if (_montagRef != null)
            {
                return _montagRef.ToString();
            }
            else
            {
                return _monsterRef.ToString();
            }
        }
    }
}
