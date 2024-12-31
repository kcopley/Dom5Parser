using Dom5Edit.Entities;

namespace Dom5Edit.Props
{
    public class PoptypeIDRef : IDRef
    {
        public static Property Create()
        {
            return new PoptypeIDRef();
        }

        internal override EntityType GetEntityType()
        {
            return EntityType.POPTYPE;
        }
    }
}
