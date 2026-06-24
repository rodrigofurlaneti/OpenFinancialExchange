namespace OpenFinancialExchange.Domain.Primitives;

public abstract class Entity : IEquatable<Entity>
{
    public long Id { get; protected set; }

    protected Entity(long id) => Id = id;

    public bool Equals(Entity? other)
    {
        if (other is null) return false;
        if (other.GetType() != GetType()) return false;
        return other.Id == Id;
    }

    public override bool Equals(object? obj) => obj is Entity entity && Equals(entity);
    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity? left, Entity? right) => Equals(left, right);
    public static bool operator !=(Entity? left, Entity? right) => !Equals(left, right);
}
