namespace Project.Base.Contracts.Models;

public class PagedSearchReturn<TObject>
{
    public required IEnumerable<TObject> Results { get; set; }

    public int TotalCount { get; set; }

    public int ReturnedInActualPage { get; set; }

    public int Limit { get; set; }

    public int ActualPage { get; set; }

    public int PagesCount { get; set; }
}
