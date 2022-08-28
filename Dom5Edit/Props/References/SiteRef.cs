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

        public override void Resolve()
        {
            if (IsStringRef)
            {
                if (Parent.Parent.TryGetValueNamedSites(Name, out IDEntity m))
                {
                    entity = m;
                    Resolved = true;
                }
            }
            else
            {
                if (Parent.Parent.TryGetValueSites(ID, out IDEntity m))
                {
                    entity = m;
                    Resolved = true;
                }
            }
            if (!Resolved && !IsStringRef && ID > ModManager.SITE_START_ID)
            {
                Parent.Parent.Log("Site not resolved for: " + this.ID);
            }
        }
    }
}
