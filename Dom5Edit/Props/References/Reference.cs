using Dom5Edit.Commands;
using Dom5Edit.Entities;

namespace Dom5Edit.Props
{
    public abstract class Reference : Property
    {
        internal abstract EntityType GetEntityType();

        public override void Parse(Command c, string v, string comment)
        {
            throw new NotImplementedException();
        }

        public override string ToExportString()
        {
            throw new NotImplementedException();
        }

        public virtual void Connect(IDEntity original)
        {
            if (TryGetEntity(out IDEntity newEntity))
            {
                newEntity.UsedByEntities.Add(original);
                original.RequiredEntities.Add(newEntity);
            }
        }

        public abstract void Resolve();

        /// <summary>The references this one is made of (itself; a choice of two holds the one it uses).</summary>
        internal virtual IEnumerable<Reference> Parts() => new[] { this };

        /// <summary>
        /// After the target moved to another number (Merge.Renumbering): the number kept for
        /// looking it up again becomes its new one. What's written already follows the target.
        /// </summary>
        internal virtual void FollowTarget(IDEntity moved) { }

        public abstract bool TryGetEntity(out IDEntity e);

        internal override Property GetDefault()
        {
            throw new NotImplementedException();
        }
    }
}
