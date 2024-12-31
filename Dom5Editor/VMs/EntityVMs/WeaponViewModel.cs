using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Editor
{
    public class WeaponViewModel : IDViewModelBase
    {
        public WeaponViewModel(ModViewModel mod, Weapon weapon) : base(mod, weapon)
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
            return Weapon._propertyMap;
        }

        protected override void InitializeAttributeInfos()
        {
            base.InitializeAttributeInfos();
            // Add any weapon-specific attribute infos here
        }

        public Weapon Weapon { get { return _entity as Weapon; } }

        public void SetWeapon(Weapon w)
        {
            SetEntity(w);
        }
    }
}