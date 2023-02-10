﻿using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Editor.VMs
{
    public class StringViewModel : PropertyViewModel
    {
        public StringViewModel(string label, IDEntity e, Command c) : base(label, e, c) { }
        public StringViewModel(IDEntity e, Command c) : base(e, c) { }

        public override string Value
        {
            get
            {
                switch (Source.TryGet(_command, out StringProperty ip))
                {
                    case ReturnType.FALSE:
                        break;
                    case ReturnType.COPIED:
                        //set to greyed out?
                        return ip.Value;
                    case ReturnType.TRUE:
                        return ip.Value;
                }
                return "";
            }
            set
            {
                Source.Set<StringProperty>(_command, i => i.Value = value);
                OnPropertyChanged(_command.ToString());
            }
        }
    }
}
