﻿using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class PoptypeIDRef : IDRef
    {
        public static Property Create()
        {
            return new PoptypeIDRef();
        }

        public override void Resolve()
        {
            if (Parent.Parent.TryGetValuePoptypes(ID, out IDEntity m))
            {
                entity = m;
                Resolved = true;
            }
        }
    }
}
