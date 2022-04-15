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
        internal MontagIDRef _montagRef;
        internal MonsterRef _monsterRef;

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
            else if (_monsterRef != null)
            {
                _monsterRef.Resolve();
            }
        }

        public override void Parse(Command c, string v, string comment)
        {
            bool hasvalue = v.TryRetrieveNumericFromString(out int i, out _);

            if (hasvalue && i < 0)
            {
                _montagRef = new MontagIDRef();
                _montagRef.Parent = this.Parent;
                _montagRef.Parse(c, v, comment);
            }
            else //monster, possibly by name
            {
                _monsterRef = new MonsterRef();
                _monsterRef.Parent = this.Parent;
                _monsterRef.Parse(c, v, comment);
            }
        }

        public override string ToString()
        {
            if (_montagRef != null)
            {
                return _montagRef.ToString();
            }
            else if (_monsterRef != null)
            {
                return _monsterRef.ToString();
            }
            return "";
        }
    }
}
