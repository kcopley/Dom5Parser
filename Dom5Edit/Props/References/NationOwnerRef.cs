using Dom5Edit.Entities;

namespace Dom5Edit.Props
{
    public class NationOwnerRef : NationRef
    {
        public new static Property Create()
        {
            return new NationOwnerRef();
        }

        internal override EntityType GetEntityType()
        {
            return EntityType.NATION;
        }
    }
}
