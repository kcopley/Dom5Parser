using Dom5Edit.Entities;

namespace Dom5Edit.Props
{
    public class NametypeIDRef : IDRef
    {
        public static Property Create()
        {
            return new NametypeIDRef();
        }

        internal override EntityType GetEntityType()
        {
            return EntityType.NAMETYPE;
        }
    }
}
