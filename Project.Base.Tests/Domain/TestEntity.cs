using Project.Base.Contracts.Models;
using Project.Base.Domain.Object.Shared;

namespace Project.Base.Tests.Domain;

public class TestEntity : BaseObjectWithId
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class TestDto : DtoBase
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
