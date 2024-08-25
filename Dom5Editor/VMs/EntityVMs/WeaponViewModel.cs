using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.VMs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

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