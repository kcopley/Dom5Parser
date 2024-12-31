using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Editor
{
    public class NametypeViewModel : IDViewModelBase
    {
        public NametypeViewModel(ModViewModel mod, Nametype nametype) : base(mod, nametype)
        {
            CoreAttributes = new List<Command>()
            {
                Command.NAMETYPE
            };

            InitializeAttributeInfos();
            RefreshEntityProperties();
        }

        protected override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return Nametype._propertyMap;
        }

        protected override void InitializeAttributeInfos()
        {
            base.InitializeAttributeInfos();
            // Add any nametype-specific attribute infos here
        }

        public Nametype Nametype { get { return _entity as Nametype; } }

        public void SetNametype(Nametype n)
        {
            SetEntity(n);
        }
    }
}
