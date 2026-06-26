using Project.Base.Contracts.Models;
using Project.Base.Domain.Converters;
using Project.Base.Domain.Object.Shared;

namespace Project.Base.Tests.Domain;

public class TestConverter : DefaultConverter<TestEntity, TestDto>
{
}

public class TestConverterMultipleProps : DefaultConverter<TestEntityMultipleProps, TestDtoMultipleProps>
{
}

public class TestEntityMultipleProps : BaseObjectWithId
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TestDtoMultipleProps : DtoBase
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime CreatedAt { get; set; }
}
