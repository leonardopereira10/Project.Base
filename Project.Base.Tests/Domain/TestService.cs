using Project.Base.Contracts.Models;
using Project.Base.Domain.Object.Shared;
using Project.Base.Domain.Repositories;
using Project.Base.Domain.Services;
using Project.Base.Domain.Validators;

namespace Project.Base.Tests.Domain;

public class TestService : BaseService<TestEntity, TestDto>
{
    public TestService(IBaseObjectWithIdRepository<TestEntity> repository)
        : base(repository)
    {
    }

    protected override IBaseAbstractValidator<TestEntity> Validator()
    {
        return new TestValidator();
    }

    protected override IDefaultConverter<TestEntity, TestDto> Converter()
    {
        return new TestConverter();
    }
}

public class TestServiceWithMockedValidator : BaseService<TestEntity, TestDto>
{
    private readonly IBaseAbstractValidator<TestEntity> _mockedValidator;

    public TestServiceWithMockedValidator(
        IBaseObjectWithIdRepository<TestEntity> repository,
        IBaseAbstractValidator<TestEntity> mockedValidator)
        : base(repository)
    {
        _mockedValidator = mockedValidator;
    }

    protected override IBaseAbstractValidator<TestEntity> Validator() => _mockedValidator;

    protected override IDefaultConverter<TestEntity, TestDto> Converter() => new TestConverter();
}
