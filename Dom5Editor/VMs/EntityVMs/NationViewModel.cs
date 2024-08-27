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
    public class NationViewModel : IDViewModelBase
    {
        public NationViewModel(ModViewModel mod, Nation nation) : base(mod, nation)
        {
            CoreAttributes = new List<Command>()
            {
                Command.NAME,
                Command.ERA,
                Command.EPITHET
            };

            InitializeAttributeInfos();
            RefreshEntityProperties();
        }

        protected override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return Nation._propertyMap;
        }

        protected override void InitializeAttributeInfos()
        {
            base.InitializeAttributeInfos();
            // Add any nation-specific attribute infos here
        }

        public Nation Nation { get { return _entity as Nation; } }

        public void SetNation(Nation n)
        {
            SetEntity(n);
        }
    }
}
