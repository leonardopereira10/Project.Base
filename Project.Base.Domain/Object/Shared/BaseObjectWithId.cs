using System.Runtime.CompilerServices;

namespace Project.Base.Domain.Object.Shared;

/// <summary>
/// Serves as the base class for all domain entities, providing a globally unique identifier
/// and natural comparison support based on that identifier.
/// </summary>
public abstract class BaseObjectWithId : IComparable<BaseObjectWithId>, IEquatable<BaseObjectWithId>
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
        return Id.CompareTo(other.Id);
    }

    /// <summary>
    /// Indicates whether the current instance is equal to another instance of the same type.
    /// </summary>
    /// <param name="other">An entity to compare with the current instance.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public bool Equals(BaseObjectWithId? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    /// <summary>
    /// Indicates whether the current instance is equal to an object.
    /// </summary>
    /// <param name="obj">An object to compare with the current instance.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as BaseObjectWithId);
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>A hash code for the current instance.</returns>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// Returns a hash code for this instance, compatible with use in
    /// hash tables and colliding with <see cref="GetHashCode()"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool operator ==(BaseObjectWithId? left, BaseObjectWithId? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Returns an value that indicates whether two instances are not equal.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool operator !=(BaseObjectWithId? left, BaseObjectWithId? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Returns an value that indicates whether the first instance precedes the
    /// second instance when sorted in descending order by Id.
    /// </summary>
    public static bool operator >(BaseObjectWithId left, BaseObjectWithId right)
    {
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    /// Returns an value that indicates whether the first instance precedes the
    /// second instance or appears in the same position when sorted in descending order by Id.
    /// </summary>
    public static bool operator >=(BaseObjectWithId left, BaseObjectWithId right)
    {
        return left.CompareTo(right) >= 0;
    }

    /// <summary>
    /// Returns an value that indicates whether the first instance follows the
    /// second instance when sorted in descending order by Id.
    /// </summary>
    public static bool operator <(BaseObjectWithId left, BaseObjectWithId right)
    {
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    /// Returns an value that indicates whether the first instance follows the
    /// second instance or appears in the same position when sorted in descending order by Id.
    /// </summary>
    public static bool operator <=(BaseObjectWithId left, BaseObjectWithId right)
    {
        return left.CompareTo(right) <= 0;
    }
}
