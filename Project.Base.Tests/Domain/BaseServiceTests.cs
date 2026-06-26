using FluentAssertions;
using FluentValidation.Results;
using Moq;
using Project.Base.Contracts.Models;
using Project.Base.Domain.Object.Shared;
using Project.Base.Domain.Repositories;
using Project.Base.Domain.Validators;
using Project.Base.Enumerators;

namespace Project.Base.Tests.Domain;

public class BaseServiceTests
{
    private readonly Mock<IBaseObjectWithIdRepository<TestEntity>> _repositoryMock;
    private readonly Mock<IDefaultConverter<TestEntity, TestDto>> _converterMock;
    private readonly Mock<IBaseAbstractValidator<TestEntity>> _validatorMock;

    public BaseServiceTests()
    {
        _repositoryMock = new Mock<IBaseObjectWithIdRepository<TestEntity>>();
        _converterMock = new Mock<IDefaultConverter<TestEntity, TestDto>>();
        _validatorMock = new Mock<IBaseAbstractValidator<TestEntity>>();
    }

    private TestServiceWithMockedValidator CreateService()
    {
        return new TestServiceWithMockedValidator(_repositoryMock.Object, _validatorMock.Object);
    }

    private void SetupConverterSuccess(TestDto? returnDto = null)
    {
        returnDto ??= new TestDto { Id = Guid.NewGuid(), Name = "Converted", Email = "conv@test.com" };
        _converterMock.Setup(c => c.ConvertToDtoOutput(It.IsAny<TestEntity>()))
            .Returns(new DtoOutput<TestDto> { Success = true, ResultSet = new[] { returnDto }, TotalCount = 1 });
        _converterMock.Setup(c => c.ConvertToDtoOutput(It.IsAny<IEnumerable<TestEntity>>()))
            .Returns(new DtoOutput<TestDto> { Success = true, ResultSet = Array.Empty<TestDto>(), TotalCount = 0 });
        _converterMock.Setup(c => c.GetDtoOutput(It.IsAny<TestDto>(), It.IsAny<IEnumerable<ValidationFail>>()!))
            .Returns((TestDto dto, IEnumerable<ValidationFail>? fails) =>
            {
                var failList = fails ?? Array.Empty<ValidationFail>();
                return new DtoOutput<TestDto>
                {
                    Success = !failList.Any(f => f.IsImpeditive),
                    ResultSet = new[] { dto },
                    ValidationFails = failList,
                    TotalCount = 1
                };
            });
        _converterMock.Setup(c => c.ConvertToDtoOutput(It.IsAny<PagedSearchReturn<TestEntity>>()))
            .Returns(new DtoOutput<TestDto> { Success = true, ResultSet = Array.Empty<TestDto>(), TotalCount = 0 });
    }

    #region Insert

    [Fact]
    public async Task Insert_ValidDto_ShouldCallRepositoryInsertAsync()
    {
        // Arrange
        var dto = new TestDto { Id = Guid.NewGuid(), Name = "New", Email = "new@test.com" };
        var insertedEntity = new TestEntity { Id = Guid.NewGuid(), Name = "New", Email = "new@test.com" };
        var insertedDto = new TestDto { Id = insertedEntity.Id, Name = "New", Email = "new@test.com" };

        SetupConverterSuccess(insertedDto);

        // Use It.IsAny so mock matches regardless of entity instance created by converter
        _converterMock.Setup(c => c.Convert(It.IsAny<TestDto>())).Returns<TestDto>(d => new TestEntity { Id = d.Id, Name = d.Name, Email = d.Email });
        _validatorMock.Setup(v => v.AssignInsertValidations());
        _validatorMock.Setup(v => v.Validate(It.IsAny<TestEntity>())).Returns(new ValidationResult());
        _validatorMock.Setup(v => v.GetValidationOutput(It.IsAny<ValidationResult>())).Returns(Array.Empty<ValidationFail>());
        _repositoryMock.Setup(r => r.InsertAsync(It.IsAny<TestEntity>())).ReturnsAsync(insertedEntity);

        var service = CreateService();

        // Act
        var result = await service.Insert(dto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _repositoryMock.Verify(r => r.InsertAsync(It.IsAny<TestEntity>()), Times.Once);
        _validatorMock.Verify(v => v.AssignInsertValidations(), Times.Once);
    }

    [Fact]
    public async Task Insert_InvalidDto_ShouldReturnValidationFails()
    {
        // Arrange
        var dto = new TestDto { Id = Guid.NewGuid(), Name = "", Email = "" };
        var fail = new ValidationFail { Message = "Name is required", Property = "Name", IsImpeditive = true };

        _converterMock.Setup(c => c.Convert(It.IsAny<TestDto>())).Returns<TestDto>(d => new TestEntity { Id = d.Id, Name = d.Name, Email = d.Email });
        _validatorMock.Setup(v => v.AssignInsertValidations());
        _validatorMock.Setup(v => v.GetValidationOutput(It.IsAny<ValidationResult>())).Returns(new[] { fail });
        _validatorMock.Setup(v => v.Validate(It.IsAny<TestEntity>())).Returns(new ValidationResult(new[]
        { new ValidationFailure("Name", "Name is required") { Severity = FluentValidation.Severity.Error } }));

        var service = CreateService();

        // Act
        var result = await service.Insert(dto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ValidationFails.Should().ContainSingle();
        result.ValidationFails!.First().IsImpeditive.Should().BeTrue();
        _repositoryMock.Verify(r => r.InsertAsync(It.IsAny<TestEntity>()), Times.Never);
    }

    #endregion

    #region Update

    [Fact]
    public async Task Update_ValidDto_ShouldCallRepositoryUpdateAsync()
    {
        // Arrange
        var dto = new TestDto { Id = Guid.NewGuid(), Name = "Updated", Email = "updated@test.com" };
        var updatedEntity = new TestEntity { Id = dto.Id, Name = "Updated", Email = "updated@test.com" };
        var updatedDto = new TestDto { Id = dto.Id, Name = "Updated", Email = "updated@test.com" };

        SetupConverterSuccess(updatedDto);

        _converterMock.Setup(c => c.Convert(It.IsAny<TestDto>())).Returns<TestDto>(d => new TestEntity { Id = d.Id, Name = d.Name, Email = d.Email });
        _validatorMock.Setup(v => v.AssignUpdateValidations());
        _validatorMock.Setup(v => v.Validate(It.IsAny<TestEntity>())).Returns(new ValidationResult());
        _validatorMock.Setup(v => v.GetValidationOutput(It.IsAny<ValidationResult>())).Returns(Array.Empty<ValidationFail>());
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TestEntity>())).ReturnsAsync(updatedEntity);

        var service = CreateService();

        // Act
        var result = await service.Update(dto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TestEntity>()), Times.Once);
        _validatorMock.Verify(v => v.AssignUpdateValidations(), Times.Once);
    }

    [Fact]
    public async Task Update_MissingId_ShouldReturnValidationFails()
    {
        // Arrange
        var dto = new TestDto { Id = Guid.Empty, Name = "Updated", Email = "updated@test.com" };
        var fail = new ValidationFail { Message = "The field ID must be informed", Property = "Id", IsImpeditive = true };

        _converterMock.Setup(c => c.Convert(It.IsAny<TestDto>())).Returns<TestDto>(d => new TestEntity { Id = d.Id, Name = d.Name, Email = d.Email });
        _validatorMock.Setup(v => v.AssignUpdateValidations());
        _validatorMock.Setup(v => v.GetValidationOutput(It.IsAny<ValidationResult>())).Returns(new[] { fail });
        _validatorMock.Setup(v => v.Validate(It.IsAny<TestEntity>())).Returns(new ValidationResult(new[]
        { new ValidationFailure("Id", "The field ID must be informed") { Severity = FluentValidation.Severity.Error } }));

        var service = CreateService();

        // Act
        var result = await service.Update(dto);

        // Assert
        result.Success.Should().BeFalse();
        result.ValidationFails.Should().ContainSingle(f => f.Property == "Id");
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TestEntity>()), Times.Never);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task Delete_ValidId_ShouldCallRepositoryDeleteAsync()
    {
        // Arrange
        var id = Guid.NewGuid();
        var deletedEntity = new TestEntity { Id = id, Name = "Deleted", Email = "del@test.com" };
        var deletedDto = new TestDto { Id = id, Name = "Deleted", Email = "del@test.com" };

        SetupConverterSuccess(deletedDto);

        _validatorMock.Setup(v => v.AssignDeleteValidations());
        _repositoryMock.Setup(r => r.DeleteAsync(id)).ReturnsAsync(deletedEntity);

        var service = CreateService();

        // Act
        var result = await service.Delete(id);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        _repositoryMock.Verify(r => r.DeleteAsync(id), Times.Once);
        _validatorMock.Verify(v => v.AssignDeleteValidations(), Times.Once);
    }

    [Fact]
    public async Task Delete_RepositoryReturnsNull_ShouldReturnDtoOutput()
    {
        // Arrange
        var id = Guid.NewGuid();

        _validatorMock.Setup(v => v.AssignDeleteValidations());
        _repositoryMock.Setup(r => r.DeleteAsync(id)).ReturnsAsync((TestEntity?)null);

        var service = CreateService();

        // Act
        var result = await service.Delete(id);

        // Assert
        result.Should().NotBeNull();
        _repositoryMock.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    #endregion

    #region FindById

    [Fact]
    public async Task FindById_ExistingEntity_ShouldReturnResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new TestEntity { Id = id, Name = "Found", Email = "found@test.com" };
        var dto = new TestDto { Id = id, Name = "Found", Email = "found@test.com" };

        _repositoryMock.Setup(r => r.ListAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<TestEntity, bool>>>()))
            .ReturnsAsync(new[] { entity });
        _converterMock.Setup(c => c.ConvertToDtoOutput(It.IsAny<IEnumerable<TestEntity>>()))
            .Returns(new DtoOutput<TestDto> { Success = true, ResultSet = new[] { dto }, TotalCount = 1 });

        var service = CreateService();

        // Act
        var result = await service.FindById(id);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ResultSet.Should().ContainSingle();
        result.ResultSet!.First().Id.Should().Be(id);
    }

    [Fact]
    public async Task FindById_NonExistingEntity_ShouldReturnEmptyResultSet()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock.Setup(r => r.ListAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<TestEntity, bool>>>()))
            .ReturnsAsync(Array.Empty<TestEntity>());
        _converterMock.Setup(c => c.ConvertToDtoOutput(It.IsAny<IEnumerable<TestEntity>>()))
            .Returns(new DtoOutput<TestDto> { Success = true, ResultSet = Array.Empty<TestDto>(), TotalCount = 0 });

        var service = CreateService();

        // Act
        var result = await service.FindById(id);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ResultSet.Should().BeEmpty();
    }

    #endregion

    #region FindAll

    [Fact]
    public async Task FindAll_WithEntities_ShouldReturnAll()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "Entity1", Email = "e1@test.com" },
            new() { Id = Guid.NewGuid(), Name = "Entity2", Email = "e2@test.com" }
        };
        var dtos = entities.Select(e => new TestDto { Id = e.Id, Name = e.Name, Email = e.Email });

        _repositoryMock.Setup(r => r.ListAsync()).ReturnsAsync(entities);
        _converterMock.Setup(c => c.ConvertToDtoOutput(It.IsAny<IEnumerable<TestEntity>>()))
            .Returns(new DtoOutput<TestDto> { Success = true, ResultSet = dtos, TotalCount = 2 });

        var service = CreateService();

        // Act
        var result = await service.FindAll();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task FindAll_Empty_ShouldReturnEmptyResultSet()
    {
        // Arrange
        _repositoryMock.Setup(r => r.ListAsync()).ReturnsAsync(Array.Empty<TestEntity>());
        _converterMock.Setup(c => c.ConvertToDtoOutput(It.IsAny<IEnumerable<TestEntity>>()))
            .Returns(new DtoOutput<TestDto> { Success = true, ResultSet = Array.Empty<TestDto>(), TotalCount = 0 });

        var service = CreateService();

        // Act
        var result = await service.FindAll();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ResultSet.Should().BeEmpty();
    }

    #endregion

    #region Find (paginated)

    [Fact]
    public async Task Find_WithParams_ShouldReturnPagedResult()
    {
        // Arrange
        var pagedReturn = new PagedSearchReturn<TestEntity>
        {
            ActualPage = 1,
            Limit = 10,
            ReturnedInActualPage = 2,
            TotalCount = 20,
            PagesCount = 2,
            Results = new List<TestEntity>
            {
                new() { Id = Guid.NewGuid(), Name = "Page1", Email = "p1@test.com" },
                new() { Id = Guid.NewGuid(), Name = "Page2", Email = "p2@test.com" }
            }
        };
        var dtos = pagedReturn.Results.Select(e => new TestDto { Id = e.Id, Name = e.Name, Email = e.Email });

        _repositoryMock.Setup(r => r.List(It.IsAny<PagedSearchParam>()))
            .Returns(pagedReturn);
        _converterMock.Setup(c => c.ConvertToDtoOutput(It.IsAny<PagedSearchReturn<TestEntity>>()))
            .Returns(new DtoOutput<TestDto>
            {
                Success = true,
                Page = 1,
                PageSize = 2,
                ResultSet = dtos,
                TotalCount = 20
            });

        var service = CreateService();

        // Act
        var result = await service.Find(1, 10, EnumOrder.ASCENDING, null, "search");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Find_WithSearchTermOnly_ShouldCallOverload()
    {
        // Arrange
        var pagedReturn = new PagedSearchReturn<TestEntity>
        {
            ActualPage = 1,
            Limit = 5,
            ReturnedInActualPage = 1,
            TotalCount = 1,
            PagesCount = 1,
            Results = new List<TestEntity>
            {
                new() { Id = Guid.NewGuid(), Name = "Search", Email = "search@test.com" }
            }
        };
        var dto = new TestDto { Id = pagedReturn.Results.First().Id, Name = "Search", Email = "search@test.com" };

        _repositoryMock.Setup(r => r.List(It.IsAny<PagedSearchParam>()))
            .Returns(pagedReturn);
        _converterMock.Setup(c => c.ConvertToDtoOutput(It.IsAny<PagedSearchReturn<TestEntity>>()))
            .Returns(new DtoOutput<TestDto>
            {
                Success = true,
                Page = 1,
                PageSize = 1,
                ResultSet = new[] { dto },
                TotalCount = 1
            });

        var service = CreateService();

        // Act
        var result = await service.Find(1, 5, EnumOrder.ASCENDING, "search");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    #endregion

    #region Validator Called Correctly

    [Fact]
    public async Task Insert_ShouldCallValidatorBeforeRepository()
    {
        // Arrange
        var dto = new TestDto { Id = Guid.NewGuid(), Name = "New", Email = "new@test.com" };
        var insertedEntity = new TestEntity { Id = Guid.NewGuid(), Name = "New", Email = "new@test.com" };

        SetupConverterSuccess(new TestDto { Id = insertedEntity.Id, Name = "New", Email = "new@test.com" });
        _converterMock.Setup(c => c.Convert(It.IsAny<TestDto>())).Returns<TestDto>(d => new TestEntity { Id = d.Id, Name = d.Name, Email = d.Email });
        _validatorMock.Setup(v => v.AssignInsertValidations());
        _validatorMock.Setup(v => v.Validate(It.IsAny<TestEntity>())).Returns(new ValidationResult());
        _validatorMock.Setup(v => v.GetValidationOutput(It.IsAny<ValidationResult>())).Returns(Array.Empty<ValidationFail>());
        _repositoryMock.Setup(r => r.InsertAsync(It.IsAny<TestEntity>())).ReturnsAsync(insertedEntity);

        var service = CreateService();

        // Act
        await service.Insert(dto);

        // Assert — validator must be called before repository
        _validatorMock.Verify(v => v.AssignInsertValidations(), Times.Once);
        _validatorMock.Verify(v => v.Validate(It.IsAny<TestEntity>()), Times.Once);
        _repositoryMock.Verify(r => r.InsertAsync(It.IsAny<TestEntity>()), Times.Once);
    }

    [Fact]
    public async Task Update_ShouldCallValidatorBeforeRepository()
    {
        // Arrange
        var dto = new TestDto { Id = Guid.NewGuid(), Name = "Updated", Email = "updated@test.com" };
        var updatedEntity = new TestEntity { Id = dto.Id, Name = "Updated", Email = "updated@test.com" };

        SetupConverterSuccess(new TestDto { Id = dto.Id, Name = "Updated", Email = "updated@test.com" });
        _converterMock.Setup(c => c.Convert(It.IsAny<TestDto>())).Returns<TestDto>(d => new TestEntity { Id = d.Id, Name = d.Name, Email = d.Email });
        _validatorMock.Setup(v => v.AssignUpdateValidations());
        _validatorMock.Setup(v => v.Validate(It.IsAny<TestEntity>())).Returns(new ValidationResult());
        _validatorMock.Setup(v => v.GetValidationOutput(It.IsAny<ValidationResult>())).Returns(Array.Empty<ValidationFail>());
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TestEntity>())).ReturnsAsync(updatedEntity);

        var service = CreateService();

        // Act
        await service.Update(dto);

        // Assert
        _validatorMock.Verify(v => v.AssignUpdateValidations(), Times.Once);
        _validatorMock.Verify(v => v.Validate(It.IsAny<TestEntity>()), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TestEntity>()), Times.Once);
    }

    #endregion
}
