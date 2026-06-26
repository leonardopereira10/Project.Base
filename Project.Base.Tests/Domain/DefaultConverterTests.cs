using FluentAssertions;
using Project.Base.Contracts.Models;
using Project.Base.Domain.Repositories;

namespace Project.Base.Tests.Domain;

public class DefaultConverterTests
{
    private readonly TestConverter _converter;

    public DefaultConverterTests()
    {
        _converter = new TestConverter();
    }

    #region Convert Entity → DTO

    [Fact]
    public void Convert_EntityToDto_ShouldMapAllProperties()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Email = "john@example.com"
        };

        // Act
        var dto = _converter.Convert(entity);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(entity.Id);
        dto.Name.Should().Be("John Doe");
        dto.Email.Should().Be("john@example.com");
    }

    [Fact]
    public void Convert_EntityToDto_WithNullId_ShouldMapDefaultGuid()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test", Email = "test@test.com" };

        // Act
        var dto = _converter.Convert(entity);

        // Assert
        dto.Should().NotBeNull();
        dto.Name.Should().Be("Test");
    }

    #endregion

    #region Convert DTO → Entity

    [Fact]
    public void Convert_DtoToEntity_ShouldMapAllProperties()
    {
        // Arrange
        var dto = new TestDto
        {
            Id = Guid.NewGuid(),
            Name = "Jane Doe",
            Email = "jane@example.com"
        };

        // Act
        var entity = _converter.Convert(dto);

        // Assert
        entity.Should().NotBeNull();
        entity.Id.Should().Be(dto.Id);
        entity.Name.Should().Be("Jane Doe");
        entity.Email.Should().Be("jane@example.com");
    }

    [Fact]
    public void Convert_DtoToEntity_WithEmptyName_ShouldMapEmptyString()
    {
        // Arrange
        var dto = new TestDto
        {
            Id = Guid.NewGuid(),
            Name = "",
            Email = "empty@test.com"
        };

        // Act
        var entity = _converter.Convert(dto);

        // Assert
        entity.Name.Should().BeEmpty();
        entity.Email.Should().Be("empty@test.com");
    }

    #endregion

    #region Convert IEnumerable<Entity> → IEnumerable<DTO>

    [Fact]
    public void Convert_EnumerableEntityToDto_ShouldMapAllItems()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "Alice", Email = "alice@test.com" },
            new() { Id = Guid.NewGuid(), Name = "Bob", Email = "bob@test.com" },
            new() { Id = Guid.NewGuid(), Name = "Charlie", Email = "charlie@test.com" }
        };

        // Act
        var dtos = _converter.Convert(entities);
        var dtoList = dtos.ToList();

        // Assert
        dtoList.Should().HaveCount(3);
        dtoList[0].Name.Should().Be("Alice");
        dtoList[1].Name.Should().Be("Bob");
        dtoList[2].Name.Should().Be("Charlie");
    }

    [Fact]
    public void Convert_EnumerableEntityToDto_WithEmptyList_ShouldReturnEmpty()
    {
        // Arrange
        var entities = new List<TestEntity>();

        // Act
        var dtos = _converter.Convert(entities);
        var dtoList = dtos.ToList();

        // Assert
        dtoList.Should().BeEmpty();
    }

    #endregion

    #region Convert IEnumerable<DTO> → IEnumerable<Entity>

    [Fact]
    public void Convert_EnumerableDtoToEntity_ShouldMapAllItems()
    {
        // Arrange
        var dtos = new List<TestDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Alice", Email = "alice@test.com" },
            new() { Id = Guid.NewGuid(), Name = "Bob", Email = "bob@test.com" }
        };

        // Act
        var entities = _converter.Convert(dtos);
        var entityList = entities.ToList();

        // Assert
        entityList.Should().HaveCount(2);
        entityList[0].Name.Should().Be("Alice");
        entityList[1].Name.Should().Be("Bob");
    }

    [Fact]
    public void Convert_EnumerableDtoToEntity_WithEmptyList_ShouldReturnEmpty()
    {
        // Arrange
        var dtos = new List<TestDto>();

        // Act
        var entities = _converter.Convert(dtos);
        var entityList = entities.ToList();

        // Assert
        entityList.Should().BeEmpty();
    }

    #endregion

    #region GetDtoOutput

    [Fact]
    public void GetDtoOutput_WithValidDto_ShouldReturnSuccessTrue()
    {
        // Arrange
        var dto = new TestDto { Id = Guid.NewGuid(), Name = "Test", Email = "test@test.com" };

        // Act
        var output = _converter.GetDtoOutput(dto);

        // Assert
        output.Success.Should().BeTrue();
        output.ValidationFails.Should().BeEmpty();
        output.ResultSet.Should().ContainSingle();
        output.ResultSet!.First().Name.Should().Be("Test");
        output.TotalCount.Should().Be(1);
        output.Page.Should().Be(0);
        output.PageSize.Should().Be(0);
    }

    [Fact]
    public void GetDtoOutput_WithFails_ShouldReturnSuccessFalse()
    {
        // Arrange
        var dto = new TestDto { Id = Guid.NewGuid(), Name = "Test", Email = "test@test.com" };
        var fails = new List<ValidationFail>
        {
            new() { Message = "Error", Property = "Name", IsImpeditive = true }
        };

        // Act
        var output = _converter.GetDtoOutput(dto, fails);

        // Assert
        output.Success.Should().BeFalse();
        output.ValidationFails.Should().ContainSingle();
        output.ValidationFails!.First().IsImpeditive.Should().BeTrue();
    }

    [Fact]
    public void GetDtoOutput_WithNullFails_ShouldNotThrow()
    {
        // Arrange
        var dto = new TestDto { Id = Guid.NewGuid(), Name = "Test", Email = "test@test.com" };

        // Act
        var output = _converter.GetDtoOutput(dto, null!);

        // Assert
        output.Success.Should().BeTrue();
        output.ValidationFails.Should().BeEmpty();
    }

    #endregion

    #region ConvertToDtoOutput (single entity)

    [Fact]
    public void ConvertToDtoOutput_Entity_ShouldConvertAndWrap()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Single", Email = "single@test.com" };

        // Act
        var output = _converter.ConvertToDtoOutput(entity);

        // Assert
        output.Success.Should().BeTrue();
        output.TotalCount.Should().Be(1);
        output.ResultSet.Should().ContainSingle();
        output.ResultSet!.First().Name.Should().Be("Single");
    }

    #endregion

    #region ConvertToDtoOutput (IEnumerable entities)

    [Fact]
    public void ConvertToDtoOutput_EnumerableEntities_ShouldConvertAndWrap()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "First", Email = "first@test.com" },
            new() { Id = Guid.NewGuid(), Name = "Second", Email = "second@test.com" }
        };

        // Act
        var output = _converter.ConvertToDtoOutput(entities);

        // Assert
        output.Success.Should().BeTrue();
        output.TotalCount.Should().Be(2);
        output.ResultSet.Should().HaveCount(2);
    }

    [Fact]
    public void ConvertToDtoOutput_EmptyEntities_ShouldReturnZeroCount()
    {
        // Arrange
        var entities = new List<TestEntity>();

        // Act
        var output = _converter.ConvertToDtoOutput(entities);

        // Assert
        output.Success.Should().BeTrue();
        output.TotalCount.Should().Be(0);
        output.ResultSet.Should().BeEmpty();
    }

    #endregion

    #region ConvertToDtoOutput (IEnumerable DTOs)

    [Fact]
    public void ConvertToDtoOutput_EnumerableDtos_ShouldWrapDirectly()
    {
        // Arrange
        var dtos = new List<TestDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Dto1", Email = "dto1@test.com" },
            new() { Id = Guid.NewGuid(), Name = "Dto2", Email = "dto2@test.com" },
            new() { Id = Guid.NewGuid(), Name = "Dto3", Email = "dto3@test.com" }
        };

        // Act
        var output = _converter.ConvertToDtoOutput(dtos);

        // Assert
        output.Success.Should().BeTrue();
        output.TotalCount.Should().Be(3);
        output.ResultSet.Should().HaveCount(3);
    }

    #endregion

    #region ConvertToDtoOutput (PagedSearchReturn)

    [Fact]
    public void ConvertToDtoOutput_PagedSearchReturn_ShouldMapPageAndResults()
    {
        // Arrange
        var pagedReturn = new PagedSearchReturn<TestEntity>
        {
            ActualPage = 2,
            Limit = 10,
            ReturnedInActualPage = 5,
            TotalCount = 50,
            PagesCount = 5,
            Results = new List<TestEntity>
            {
                new() { Id = Guid.NewGuid(), Name = "Item1", Email = "item1@test.com" },
                new() { Id = Guid.NewGuid(), Name = "Item2", Email = "item2@test.com" }
            }
        };

        // Act
        var output = _converter.ConvertToDtoOutput(pagedReturn);

        // Assert
        output.Success.Should().BeTrue();
        output.Page.Should().Be(2);
        output.PageSize.Should().Be(5);
        output.TotalCount.Should().Be(50);
        output.ResultSet.Should().HaveCount(2);
    }

    #endregion

    #region Clone / Copy

    [Fact]
    public void Convert_EntityToDtoAndBack_ShouldPreserveId()
    {
        // Arrange
        var originalId = Guid.NewGuid();
        var entity = new TestEntity { Id = originalId, Name = "CloneTest", Email = "clone@test.com" };

        // Act
        var dto = _converter.Convert(entity);
        var backToEntity = _converter.Convert(dto);

        // Assert
        backToEntity.Id.Should().Be(originalId);
        backToEntity.Name.Should().Be("CloneTest");
        backToEntity.Email.Should().Be("clone@test.com");
    }

    [Fact]
    public void Convert_MultiplePropsEntityToDto_ShouldMapAllTypes()
    {
        // Arrange
        var converter = new TestConverterMultipleProps();
        var entity = new TestEntityMultipleProps
        {
            Id = Guid.NewGuid(),
            Name = "MultiProps",
            Age = 42,
            CreatedAt = new DateTime(2025, 1, 15, 10, 30, 0)
        };

        // Act
        var dto = converter.Convert(entity);

        // Assert
        dto.Name.Should().Be("MultiProps");
        dto.Age.Should().Be(42);
        dto.CreatedAt.Should().BeCloseTo(new DateTime(2025, 1, 15, 10, 30, 0), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Convert_MultiplePropsDtoToEntity_ShouldMapAllTypes()
    {
        // Arrange
        var converter = new TestConverterMultipleProps();
        var dto = new TestDtoMultipleProps
        {
            Id = Guid.NewGuid(),
            Name = "MultiPropsDto",
            Age = 25,
            CreatedAt = new DateTime(2024, 6, 1, 0, 0, 0)
        };

        // Act
        var entity = converter.Convert(dto);

        // Assert
        entity.Name.Should().Be("MultiPropsDto");
        entity.Age.Should().Be(25);
        entity.CreatedAt.Should().BeCloseTo(new DateTime(2024, 6, 1, 0, 0, 0), TimeSpan.FromSeconds(1));
    }

    #endregion
}
