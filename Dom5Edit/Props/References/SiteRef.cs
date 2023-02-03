using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class SiteRef : StringOrIDRef
    {
        public static Property Create()
        {
            return new SiteRef();
        }

        internal override EntityType GetEntityType()
        {
            return EntityType.SITE;
        }
    }
}
