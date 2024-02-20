using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.VMs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Dom5Editor
{
    public class MonsterGearViewModel : PropertyViewModel
    {
        public MonsterGearViewModel(string label, IDEntity e, Command c) : base(label, e, c) { }
        public MonsterGearViewModel(IDEntity e, Command c) : base(e, c) { }

        public override string Value
        {
            get
            {
                switch (Source.TryGet(Command, out StringOrIDRef ip))
                {
                    case ReturnType.FALSE:
                        break;
                }
                return "";
            }
            set
            {
                //set entity name here
                Source.Set<NameProperty>(Command, i => i.Value = value);
            }
        }
    }
}
