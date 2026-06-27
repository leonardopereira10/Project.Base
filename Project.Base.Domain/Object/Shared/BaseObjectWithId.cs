namespace Project.Base.Domain.Object.Shared;

/// <summary>
/// Serves as the base class for all domain entities, providing a globally unique identifier
/// and natural comparison support based on that identifier.
/// </summary>
public class BaseObjectWithId : IComparable<BaseObjectWithId>
{
    /// <summary>
    /// Gets or sets the globally unique identifier for this entity.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Compares the current instance to another entity of the same type and returns an integer
    /// that indicates whether the current instance precedes, follows, or appears in the same
    /// position as the other element in a sorted sequence (descending by Id).
    /// </summary>
    /// <param name="other">The entity to compare with the current instance.</param>
    /// <returns>A signed integer indicating the relative sort order: negative if the current
    /// instance's Id is greater, positive if less, or zero if equal.</returns>
    public int CompareTo(BaseObjectWithId? other)
    {
        if (other == null) return 1;

        if (Id < other.Id)
        {
            return 1;
        }

        return Id > other.Id ? -1 : 0;
    }
}
