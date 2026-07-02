using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Project.Base.Domain.Repositories;
using Project.Base.Enumerators;
using Project.Base.Tests.Domain;

namespace Project.Base.Tests.Repository;

public class GenericRepositoryTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly TestGenericRepository _repository;

    public GenericRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new TestDbContext(options);
        _repository = new TestGenericRepository(_context);

        SeedData();
    }

    private void SeedData()
    {
        _context.TestEntities.AddRange(
            new TestEntity { Id = Guid.NewGuid(), Name = "Alice", Email = "alice@test.com" },
            new TestEntity { Id = Guid.NewGuid(), Name = "Bob", Email = "bob@test.com" },
            new TestEntity { Id = Guid.NewGuid(), Name = "Charlie", Email = "charlie@test.com" },
            new TestEntity { Id = Guid.NewGuid(), Name = "David", Email = "david@test.com" },
            new TestEntity { Id = Guid.NewGuid(), Name = "Eve", Email = "eve@test.com" }
        );
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region Insert

    [Fact]
    public void Insert_ShouldAddEntityAndSave()
    {
        // Arrange
        var newEntity = new TestEntity { Name = "Frank", Email = "frank@test.com" };
        var initialCount = _context.TestEntities.Count();

        // Act
        var inserted = _repository.Insert(newEntity);

        // Assert
        inserted.Should().NotBeNull();
        inserted.Id.Should().NotBeEmpty();
        inserted.Name.Should().Be("Frank");
        _context.TestEntities.Count().Should().Be(initialCount + 1);
    }

    [Fact]
    public async Task InsertAsync_ShouldAddEntityAndSave()
    {
        // Arrange
        var newEntity = new TestEntity { Name = "Grace", Email = "grace@test.com" };
        var initialCount = await _context.TestEntities.CountAsync();

        // Act
        var inserted = await _repository.InsertAsync(newEntity);

        // Assert
        inserted.Should().NotBeNull();
        inserted.Id.Should().NotBeEmpty();
        inserted.Name.Should().Be("Grace");
        (await _context.TestEntities.CountAsync()).Should().Be(initialCount + 1);
    }

    #endregion

    #region Update

    [Fact]
    public void Update_ShouldUpdateEntityAndSave()
    {
        // Arrange
        var entity = _context.TestEntities.First();
        var originalName = entity.Name;
        entity.Name = originalName + " Updated";

        // Act
        var updated = _repository.Update(entity);

        // Assert
        updated.Should().NotBeNull();
        updated.Name.Should().Be(originalName + " Updated");
        _context.TestEntities.First(e => e.Id == entity.Id).Name.Should().Be(originalName + " Updated");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntityAndSave()
    {
        // Arrange
        var entity = _context.TestEntities.First();
        var originalName = entity.Name;
        entity.Name = originalName + " Async";

        // Act
        var updated = await _repository.UpdateAsync(entity);

        // Assert
        updated.Should().NotBeNull();
        updated.Name.Should().Be(originalName + " Async");
        (await _context.TestEntities.FirstAsync(e => e.Id == entity.Id)).Name.Should().Be(originalName + " Async");
    }

    #endregion

    #region Delete

    [Fact]
    public void Delete_ShouldRemoveEntityAndSave()
    {
        // Arrange
        var entity = _context.TestEntities.First();
        var initialCount = _context.TestEntities.Count();

        // Act
        var deleted = _repository.Delete(entity);

        // Assert
        deleted.Should().NotBeNull();
        deleted.Id.Should().Be(entity.Id);
        _context.TestEntities.Count().Should().Be(initialCount - 1);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntityAndSave()
    {
        // Arrange
        var entity = _context.TestEntities.First();
        var initialCount = await _context.TestEntities.CountAsync();

        // Act
        var deleted = await _repository.DeleteAsync(entity);

        // Assert
        deleted.Should().NotBeNull();
        deleted.Id.Should().Be(entity.Id);
        (await _context.TestEntities.CountAsync()).Should().Be(initialCount - 1);
    }

    [Fact]
    public void Delete_WithNull_ShouldThrow()
    {
        // Act & Assert
        var result = Record.Exception(() => _repository.Delete(null!));
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNull_ShouldThrow()
    {
        // Act & Assert
        var result = await Record.ExceptionAsync(() => _repository.DeleteAsync(null!));
        result.Should().NotBeNull();
    }

    #endregion

    #region List (no predicate)

    [Fact]
    public void List_ShouldReturnAllEntities()
    {
        // Act
        var results = _repository.List().ToList();

        // Assert
        results.Should().HaveCount(5);
    }

    [Fact]
    public async Task ListAsync_ShouldReturnAllEntities()
    {
        // Act
        var results = await _repository.ListAsync();
        var list = results.ToList();

        // Assert
        list.Should().HaveCount(5);
    }

    #endregion

    #region List (with predicate)

    [Fact]
    public void List_WithPredicate_ShouldFilterCorrectly()
    {
        // Act
        var results = _repository.List(e => e.Name == "Alice").ToList();

        // Assert
        results.Should().HaveCount(1);
        results.First().Name.Should().Be("Alice");
    }

    [Fact]
    public async Task ListAsync_WithPredicate_ShouldFilterCorrectly()
    {
        // Act
        var results = await _repository.ListAsync(e => e.Email.Contains("@test.com"));
        var list = results.ToList();

        // Assert
        list.Should().HaveCount(5);
    }

    [Fact]
    public void List_WithNoMatchPredicate_ShouldReturnEmpty()
    {
        // Act
        var results = _repository.List(e => e.Name == "NonExistent").ToList();

        // Assert
        results.Should().BeEmpty();
    }

    #endregion

    #region List (PagedSearchParam)

    [Fact]
    public void List_WithPagedSearchParam_ShouldReturnPagedResults()
    {
        // Arrange
        var param = new PagedSearchParam { Page = 1, Limit = 2, Order = EnumOrder.ASCENDING };

        // Act
        var result = _repository.List(param);

        // Assert
        result.ActualPage.Should().Be(1);
        result.Limit.Should().Be(2);
        result.ReturnedInActualPage.Should().Be(2);
        result.TotalCount.Should().Be(5);
        result.Results.Should().HaveCount(2);
    }

    [Fact]
    public void List_WithPagedSearchParam_Page2_ShouldReturnNextPage()
    {
        // Arrange
        var param = new PagedSearchParam { Page = 2, Limit = 2, Order = EnumOrder.ASCENDING };

        // Act
        var result = _repository.List(param);

        // Assert
        result.ActualPage.Should().Be(2);
        result.ReturnedInActualPage.Should().Be(2);
        result.Results.Should().HaveCount(2);
    }

    [Fact]
    public void List_WithPagedSearchParam_Page3_ShouldReturnRemaining()
    {
        // Arrange
        var param = new PagedSearchParam { Page = 3, Limit = 2, Order = EnumOrder.ASCENDING };

        // Act
        var result = _repository.List(param);

        // Assert
        result.ActualPage.Should().Be(3);
        result.ReturnedInActualPage.Should().Be(1);
        result.Results.Should().HaveCount(1);
    }

    [Fact]
    public void List_WithPagedSearchParam_DescendingOrder_ShouldOrderCorrectly()
    {
        // Arrange
        var param = new PagedSearchParam { Page = 1, Limit = 5, Order = EnumOrder.DESCENDING };

        // Act
        var result = _repository.List(param);
        var resultsList = result.Results.ToList();

        // Assert
        resultsList.Should().HaveCount(5);
        // Descending: first Id should be lexicographically greater than last Id
        resultsList[0].Id.CompareTo(resultsList[4].Id).Should().BeGreaterThan(0);
    }

    [Fact]
    public void List_WithPagedSearchParam_Page0_ShouldReturnPage1()
    {
        // Arrange
        var param = new PagedSearchParam { Page = 0, Limit = 2, Order = EnumOrder.ASCENDING };

        // Act
        var result = _repository.List(param);

        // Assert
        result.ActualPage.Should().Be(1);
    }

    [Fact]
    public void List_WithPagedSearchParam_TotalCount_ShouldBeCorrect()
    {
        // Arrange
        var param = new PagedSearchParam { Page = 1, Limit = 2, Order = EnumOrder.ASCENDING };

        // Act
        var result = _repository.List(param);

        // Assert
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public void List_WithPagedSearchParam_PagesCount_ShouldBeCorrect()
    {
        // Arrange
        var param = new PagedSearchParam { Page = 1, Limit = 2, Order = EnumOrder.ASCENDING };

        // Act
        var result = _repository.List(param);

        // Assert
        result.PagesCount.Should().Be(3);
    }

    [Fact]
    public void List_WithPagedSearchParam_EmptyLimit_ShouldReturnAll()
    {
        // Arrange
        var param = new PagedSearchParam { Page = 1, Limit = 0, Order = EnumOrder.ASCENDING };

        // Act
        var result = _repository.List(param);

        // Assert
        result.Results.Should().HaveCount(5);
    }

    #endregion

    #region Async CRUD flow

    [Fact]
    public async Task FullCrudFlowAsync_ShouldWorkCorrectly()
    {
        // Arrange
        var initialCount = await _context.TestEntities.CountAsync();

        // Act — Insert
        var newEntity = new TestEntity { Name = "FlowTest", Email = "flow@test.com" };
        var inserted = await _repository.InsertAsync(newEntity);

        // Verify Insert
        (await _context.TestEntities.CountAsync()).Should().Be(initialCount + 1);
        inserted.Id.Should().NotBeEmpty();

        // Act — Update
        inserted.Name = "FlowTest Updated";
        var updated = await _repository.UpdateAsync(inserted);
        updated.Name.Should().Be("FlowTest Updated");

        // Act — Find
        var found = (await _repository.ListAsync(e => e.Id == updated.Id)).ToList();
        found.Should().HaveCount(1);
        found.First().Name.Should().Be("FlowTest Updated");

        // Act — Delete
        var deleted = await _repository.DeleteAsync(updated);
        deleted.Id.Should().Be(updated.Id);

        // Verify Delete
        (await _context.TestEntities.CountAsync()).Should().Be(initialCount);
    }

    [Fact]
    public async Task FullCrudFlowSync_ShouldWorkCorrectly()
    {
        // Arrange
        var initialCount = _context.TestEntities.Count();

        // Act — Insert
        var newEntity = new TestEntity { Name = "SyncFlow", Email = "syncflow@test.com" };
        var inserted = _repository.Insert(newEntity);

        // Verify Insert
        _context.TestEntities.Count().Should().Be(initialCount + 1);
        inserted.Id.Should().NotBeEmpty();

        // Act — Update
        inserted.Name = "SyncFlow Updated";
        var updated = _repository.Update(inserted);
        updated.Name.Should().Be("SyncFlow Updated");

        // Act — Find
        var found = _repository.List(e => e.Id == updated.Id).ToList();
        found.Should().HaveCount(1);
        found.First().Name.Should().Be("SyncFlow Updated");

        // Act — Delete
        var deleted = _repository.Delete(updated);
        deleted.Id.Should().Be(updated.Id);

        // Verify Delete
        _context.TestEntities.Count().Should().Be(initialCount);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Insert_WithExistingId_ShouldThrow()
    {
        // Arrange
        var existingEntity = _context.TestEntities.First();
        var newEntity = new TestEntity { Id = existingEntity.Id, Name = "Duplicate", Email = "dup@test.com" };

        // Act & Assert
        var result = Record.Exception(() => _repository.Insert(newEntity));
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task InsertAsync_WithExistingId_ShouldThrow()
    {
        // Arrange
        var existingEntity = _context.TestEntities.First();
        var newEntity = new TestEntity { Id = existingEntity.Id, Name = "DuplicateAsync", Email = "dupasync@test.com" };

        // Act & Assert
        var result = await Record.ExceptionAsync(() => _repository.InsertAsync(newEntity));
        result.Should().NotBeNull();
    }

    [Fact]
    public void Update_WithNewId_ShouldThrow()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "NewId", Email = "newid@test.com" };

        // Act & Assert
        var result = Record.Exception(() => _repository.Update(entity));
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_WithNewId_ShouldThrow()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "NewIdAsync", Email = "newidasync@test.com" };

        // Act & Assert
        var result = await Record.ExceptionAsync(() => _repository.UpdateAsync(entity));
        result.Should().NotBeNull();
    }

    [Fact]
    public void List_WithComplexPredicate_ShouldFilterCorrectly()
    {
        // Act
        var results = _repository.List(e => e.Name.StartsWith("A") || e.Name == "Eve").ToList();

        // Assert
        results.Should().HaveCount(2);
        results.Should().OnlyContain(e => e.Name == "Alice" || e.Name == "Eve");
    }

    #endregion
}
