namespace Dom5Edit.Entities
{
    public class DependentEntity
    {
        public int ID { get; set; }
        public DependentEntity Dependent;
        public List<IDEntity> ReferencedEntities = new List<IDEntity>();

        public DependentEntity(int ID)
        {
            this.ID = ID;
        }

        public int GetID()
        {
            if (Dependent != null) return Dependent.ID;
            return ID;
        }
    }
}
