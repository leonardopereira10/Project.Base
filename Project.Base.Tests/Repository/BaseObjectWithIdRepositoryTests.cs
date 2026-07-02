using Microsoft.EntityFrameworkCore;
using Project.Base.Tests.Domain;
using FluentAssertions;

namespace Project.Base.Tests.Repository;

public class BaseObjectWithIdRepositoryTests
{
    private readonly TestDbContext _context;
    private readonly TestBaseObjectWithIdRepository _repository;

    public BaseObjectWithIdRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestDbContext(options);
        _repository = new TestBaseObjectWithIdRepository(_context);
    }

    #region GetById

    [Fact]
    public void GetById_ShouldReturnEntity_WhenExists()
    {
        // Arrange
        var expectedEntity = new TestEntity { Name = "Test", Email = "test@test.com" };
        _context.TestEntities.Add(expectedEntity);
        _context.SaveChanges();

        // Act
        var result = _repository.GetById(expectedEntity.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedEntity.Id);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public void GetById_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = _repository.GetById(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        // Arrange
        var expectedEntity = new TestEntity { Name = "AsyncTest", Email = "async@test.com" };
        _context.TestEntities.Add(expectedEntity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(expectedEntity.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedEntity.Id);
        result.Name.Should().Be("AsyncTest");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Delete

    [Fact]
    public void Delete_ShouldRemoveEntity_WhenExists()
    {
        // Arrange
        var entity = new TestEntity { Name = "ToDelete", Email = "delete@test.com" };
        _context.TestEntities.Add(entity);
        _context.SaveChanges();

        // Act
        _repository.Delete(entity.Id);
        _context.SaveChanges();

        // Assert
        var result = _context.TestEntities.Find(entity.Id);
        result.Should().BeNull();
    }

    [Fact]
    public void Delete_ShouldReturnDeletedEntity()
    {
        // Arrange
        var entity = new TestEntity { Name = "DeleteReturn", Email = "return@test.com" };
        _context.TestEntities.Add(entity);
        _context.SaveChanges();

        // Act
        var deletedEntity = _repository.Delete(entity.Id);

        // Assert
        deletedEntity.Should().NotBeNull();
        deletedEntity.Id.Should().Be(entity.Id);
        deletedEntity.Name.Should().Be("DeleteReturn");
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntity_WhenExists()
    {
        // Arrange
        var entity = new TestEntity { Name = "AsyncDelete", Email = "asyncdelete@test.com" };
        _context.TestEntities.Add(entity);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(entity.Id);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.TestEntities.FindAsync(entity.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowException_WhenNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await _repository.DeleteAsync(nonExistentId));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnDeletedEntity()
    {
        // Arrange
        var entity = new TestEntity { Name = "AsyncDeleteReturn", Email = "asyncreturn@test.com" };
        _context.TestEntities.Add(entity);
        await _context.SaveChangesAsync();

        // Act
        var deletedEntity = await _repository.DeleteAsync(entity.Id);

        // Assert
        deletedEntity.Should().NotBeNull();
        deletedEntity.Id.Should().Be(entity.Id);
        deletedEntity.Name.Should().Be("AsyncDeleteReturn");
    }

    #endregion
}
