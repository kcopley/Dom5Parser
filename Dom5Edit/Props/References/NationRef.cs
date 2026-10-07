using Dom5Edit.Entities;

namespace Dom5Edit.Props
{
    public class NationRef : StringOrIDRef
    {
        public static Property Create()
        {
            return new NationRef();
        }

        internal override EntityType GetEntityType()
        {
            return EntityType.NATION;
        }
    }
}
