using Project.Base.Enumerators;

namespace Project.Base.Domain.Repositories;

public class PagedSearchParam
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public EnumOrder Order { get; set; }
    public string SearchTarget { get; set; }
    public string SearchTerm { get; set; }
}
