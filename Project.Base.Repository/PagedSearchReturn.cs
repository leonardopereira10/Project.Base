namespace Project.Base.Repository;

public class PagedSearchReturn<TObject>

{
    public IEnumerable<TObject>? Results { get; set; }

    public int TotalCount { get; set; }

    public int ReturnedInActualPage { get; set; }

    public int ActualPage { get; set; }

    public int PagesCount { get; set; }
}
