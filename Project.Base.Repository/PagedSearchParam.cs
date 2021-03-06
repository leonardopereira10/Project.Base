using Project.Base.Enumerators;

namespace Project.Base.Repository;

public class PagedSearchParam
{    
    public int Page { get; set; }
    public int Count { get; set; }
    public EnumOrder Order { get; set; }
    public string? SearchTerm { get; set; }
}
