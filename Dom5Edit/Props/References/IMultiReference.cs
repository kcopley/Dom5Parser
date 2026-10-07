using Dom5Edit.Entities;

namespace Dom5Edit.Props
{
    /// <summary>
    /// A reference whose value stands for several entities at once: a spell's #damage that is a
    /// key into one of the game's lists (the uniques a Bind ritual picks from, the units a terrain
    /// summon gives). "Used by" lists the property under each of them.
    /// </summary>
    public interface IMultiReference
    {
        IEnumerable<IDEntity> Targets();
    }
}
