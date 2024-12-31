using Dom5Edit.Entities;

namespace Dom5Edit.Props
{
    public class ItemRef : StringOrIDRef
    {
        public static Property Create()
        {
            return new ItemRef();
        }

        internal override EntityType GetEntityType()
        {
            return EntityType.ITEM;
        }
    }
}
