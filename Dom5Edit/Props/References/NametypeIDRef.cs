using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class NametypeIDRef : IDRef
    {
        public static Property Create()
        {
            return new NametypeIDRef();
        }

        public override void Resolve()
        {
            if (Parent.Parent.TryGetValueNametypes(ID, out IDEntity m))
            {
                entity = m;
                Resolved = true;
            }
            if (!Resolved && ID > ModManager.NAMETYPE_START_ID)
            {
                Parent.Parent.Log("Nametype not resolved for: " + this.ID);
            }
        }
    }
}
