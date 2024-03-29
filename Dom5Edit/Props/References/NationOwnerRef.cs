﻿using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class NationOwnerRef : NationRef
    {
        public new static Property Create()
        {
            return new NationOwnerRef();
        }

        public override void Resolve()
        {
            if (!IsStringRef)
            {
                if (this.HasValue && this.ID <= 0)
                {
                    Resolved = true;
                    return;
                }
                else
                {
                    if (Parent.Parent.TryGetValueNations(ID, out IDEntity m))
                    {
                        Entity = m;
                        Resolved = true;
                    }
                }
            }
            if (!Resolved  && !IsStringRef && ID > ModManager.NATION_START_ID)
            {
                Parent.Parent.Log("Nation not resolved for: " + this.ID);
            }
        }
    }
}
