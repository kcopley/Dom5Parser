using Dom5Edit.Commands;
using Dom5Edit.Entities;

namespace Dom5Edit.Props
{
    public abstract class Property
    {
        public IDEntity Parent { get; set; }
        public string Comment { get; set; }
        public abstract void Parse(Command c, string v, string comment);

        public abstract string ToExportString();

        public Command Command { get; set; }

        internal abstract Property GetDefault();

        internal abstract bool EqualsProperty<T>(T copyFrom) where T : Property, new();
    }
}
