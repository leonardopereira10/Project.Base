namespace Project.Base.Domain.Object.Shared;

public class BaseObjectWithId : IComparable<BaseObjectWithId>
{
    public Guid Id { get; set; }

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
