namespace Project.Base.Domain.Object.Shared;

public class BaseObjectWithId : IComparable<BaseObjectWithId>
{
    public Guid Id { get; set; }

    public int CompareTo(BaseObjectWithId other)
    {
        BaseObjectWithId Temp = other;
        if (Id < Temp.Id)
        {
            return 1;
        }

        return Id > Temp.Id ? -1 : 0;
    }
}
