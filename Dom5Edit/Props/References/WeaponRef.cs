using Dom5Edit.Entities;

namespace Dom5Edit.Props
{
    public class WeaponRef : StringOrIDRef
    {
        public static Property Create()
        {
            return new WeaponRef();
        }

        internal override EntityType GetEntityType()
        {
            return EntityType.WEAPON;
        }

        public override string ToExportString()
        {
            if (Entity != null && Entity.ID != -1)
                IsStringRef = false;
            return base.ToExportString();
        }
    }
}
