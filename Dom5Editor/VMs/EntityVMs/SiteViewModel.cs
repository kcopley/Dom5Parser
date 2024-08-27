using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Editor
{
    public class SiteViewModel : IDViewModelBase
    {
        public SiteViewModel(ModViewModel mod, Site site) : base(mod, site)
        {
            CoreAttributes = new List<Command>()
            {
                Command.NAME
            };

            InitializeAttributeInfos();
            RefreshEntityProperties();
        }

        protected override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return Site._propertyMap;
        }

        protected override void InitializeAttributeInfos()
        {
            base.InitializeAttributeInfos();
            // Add any site-specific attribute infos here if needed
        }

        public Site Site { get { return _entity as Site; } }

        public void SetSite(Site s)
        {
            SetEntity(s);
        }

        // Add any site-specific properties or methods here
    }
}
