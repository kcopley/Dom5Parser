using Dom5Edit.Entities;

namespace Dom5Edit.Props
{
    public class ArmorRef : StringOrIDRef
    {

        public static Property Create()
        {
            return new ArmorRef();
        }

        internal override EntityType GetEntityType()
        {
            return EntityType.ARMOR;
        }
    }
}
