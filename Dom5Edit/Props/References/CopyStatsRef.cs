using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class CopyStatsRef : MonsterOrMontagRef
    {
        public new static Property Create()
        {
            return new CopyStatsRef();
        }

        public override void Parse(Command c, string v, string comment)
        {
            base.Parse(c, v, comment);
            if (this._monsterRef?.ID != -1)
            {
                Parent.Parent.VanillaMageReferences.Add(this._monsterRef.ID);
            }
        }
    }
}
