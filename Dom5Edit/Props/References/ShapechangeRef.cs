using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class ShapechangeRef : MonsterOrMontagRef
    {
        public new static Property Create()
        {
            return new ShapechangeRef();
        }

        public override void Resolve()
        {
            base.Resolve();
            if (_monsterRef != null)
            {
                _monsterRef.IsStringRef = false;
            }
        }

        public override void Parse(Command c, string v, string comment)
        {
            base.Parse(c, v, comment);
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
