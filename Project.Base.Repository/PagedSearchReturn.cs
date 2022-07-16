using Project.Base.Domain.Object;

namespace Project.Base.Repository;

public class PagedSearchReturn<TObject> where TObject : BaseObjectWithId

{
    public PagedSearchReturn()
    {
        Results = new List<TObject>();
    }

    IList<TObject> Results { get; set; }

    public int TotalCount { get; set; }

    public int ReturnedInActualPage { get; set; }

    public int ActualPage { get; set; }

    public int PagesCount { get; set; }
    
    
    
    
    
    
}
