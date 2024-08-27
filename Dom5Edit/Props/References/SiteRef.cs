using Dom5Edit.Entities;

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
