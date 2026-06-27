namespace Project.Base.Domain.Repositories;

/// <summary>
/// Represents the result of a paginated search query, containing the items for the current
/// page along with metadata about the overall result set.
/// </summary>
/// <typeparam name="TObject">The type of entity contained in the result set.</typeparam>
public class PagedSearchReturn<TObject>
{
    /// <summary>
    /// Gets or sets the collection of entities returned for the current page.
    /// </summary>
    public required IEnumerable<TObject> Results { get; set; }

    /// <summary>
    /// Gets or sets the total number of entities matching the search criteria across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the actual number of entities returned in the current page.
    /// </summary>
    public int ReturnedInActualPage { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of items per page as specified in the request.
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// Gets or sets the page number that was requested and is represented by this result.
    /// </summary>
    public int ActualPage { get; set; }

    /// <summary>
    /// Gets or sets the total number of pages available based on the total count and page limit.
    /// </summary>
    public int PagesCount { get; set; }
}
