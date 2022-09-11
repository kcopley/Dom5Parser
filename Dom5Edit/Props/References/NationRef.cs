using Dom5Edit.Commands;
using Dom5Edit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Edit.Props
{
    public class NationRef : StringOrIDRef
    {
        public static Property Create()
        {
            return new NationRef();
        }

        public override void Resolve()
        {
            if (IsStringRef)
            {
                if (Parent.Parent.TryGetValueNamedNations(Name, out IDEntity m))
                {
                    Entity = m;
                    Resolved = true;
                }
            }
            else
            {
                if (Parent.Parent.TryGetValueNations(ID, out IDEntity m))
                {
                    Entity = m;
                    Resolved = true;
                }
            }
            if (!Resolved && !IsStringRef && ID > ModManager.NATION_START_ID)
            {
                Parent.Parent.Log("Nation not resolved for: " + this.ID);
            }
        }

        public override string ToString()
        {
            if (Entity == null && !IsStringRef && ID > ModManager.NATION_START_ID) //was definitely a modnation reference
            {
                Parent.Parent.Log("Nation for ID: " + ID + " under command: " + this._command + " was never resolved. This could cause conflicts as the nation referenced in the mod does not exist.  If it was intentional, please use the dependency feature to denote mods that are intended to modify one another. Skipping export.");
                return "";
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
