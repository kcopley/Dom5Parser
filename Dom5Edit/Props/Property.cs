using Dom5Edit.Commands;
using Dom5Edit.Entities;

namespace Dom5Edit.Props
{
    public abstract class Property
    {
        public IDEntity Parent { get; set; }
        public string Comment { get; set; }
        public int LineNumber { get; set; }
        public abstract void Parse(Command c, string v, string comment);

        public abstract string ToExportString();

        public Command Command { get; set; }

        internal abstract Property GetDefault();

        internal abstract bool EqualsProperty<T>(T copyFrom) where T : Property, new();

        /// <summary>
        /// Returns a copy of this property. Used by the copy/inheritance materialization
        /// (see docs/COPY_INHERITANCE_REDESIGN.md) to freeze a snapshot of a copy source's
        /// values at copy time, independent of later edits to the source.
        ///
        /// MemberwiseClone is a faithful deep copy for the value-typed / string-valued
        /// properties Phase 1 bakes (IntProperty, NameProperty, etc.). Reference types may
        /// override if they hold mutable collections; the snapshot stores them but Phase 1
        /// does not bake references.
        /// </summary>
        internal virtual Property Clone()
        {
            return (Property)this.MemberwiseClone();
        }
    }
}
