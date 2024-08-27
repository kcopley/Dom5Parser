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
    public class MercenaryViewModel : IDViewModelBase
    {
        public MercenaryViewModel(ModViewModel mod, Mercenary mercenary) : base(mod, mercenary)
        {
            CoreAttributes = new List<Command>()
            {
                Command.NAME,
                Command.BOSSNAME,
                Command.COM
            };

            InitializeAttributeInfos();
            RefreshEntityProperties();
        }

        protected override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return Mercenary._propertyMap;
        }

        protected override void InitializeAttributeInfos()
        {
            base.InitializeAttributeInfos();
            // Add any mercenary-specific attribute infos here
        }

        public Mercenary Mercenary { get { return _entity as Mercenary; } }

        public void SetMercenary(Mercenary m)
        {
            SetEntity(m);
        }
    }
}
