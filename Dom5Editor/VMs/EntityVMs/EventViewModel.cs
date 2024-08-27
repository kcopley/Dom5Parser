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
    public class EventViewModel : IDViewModelBase
    {
        public EventViewModel(ModViewModel mod, Event @event) : base(mod, @event)
        {
            CoreAttributes = new List<Command>()
            {
                Command.NAME,
                Command.DESCRIPTION
            };

            InitializeAttributeInfos();
            RefreshEntityProperties();
        }

        protected override Dictionary<Command, Func<Property>> GetPropertyMap()
        {
            return Event._propertyMap;
        }

        protected override void InitializeAttributeInfos()
        {
            base.InitializeAttributeInfos();
            // Add any event-specific attribute infos here
        }

        public Event Event { get { return _entity as Event; } }

        public void SetEvent(Event e)
        {
            SetEntity(e);
        }
    }
}
