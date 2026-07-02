using Project.Base.Domain.Object.Shared;

namespace Project.Base.Tests.Domain;

/// <summary>
/// Entity with no string properties — used to exercise the "no string properties" branch in ListWithSearchTerm.
/// </summary>
public class NoStringEntity : BaseObjectWithId
{
    public int Code { get; set; }
    public decimal Value { get; set; }
}
