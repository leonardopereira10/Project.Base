using Project.Base.Enumerators;

namespace Project.Base.Domain.Repositories;

/// <summary>
/// Defines the parameters for a paginated search query, including page size, ordering,
/// and optional text-based filtering.
/// </summary>
public class PagedSearchParam
{
    /// <summary>
    /// Gets or sets the 1-based page number to retrieve.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of items to return on a single page.
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// Gets or sets the sort direction for the result set.
    /// </summary>
    public EnumOrder Order { get; set; }

    /// <summary>
    /// Gets or sets the target column or field name to search within.
    /// </summary>
    public string? SearchTarget { get; set; }

    /// <summary>
    /// Gets or sets the text term to filter results against the search target.
    /// </summary>
    public string? SearchTerm { get; set; }
}
