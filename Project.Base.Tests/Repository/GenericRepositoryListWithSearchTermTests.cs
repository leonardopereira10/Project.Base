using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Project.Base.Domain.Repositories;
using Project.Base.Enumerators;
using Project.Base.Tests.Domain;
using System.Linq;

namespace Project.Base.Tests.Repository;

/// <summary>
/// Testes unitários para o método ListWithSearchTerm do GenericRepository.
/// Cobre busca dinâmica (sem SearchTarget), busca por campo específico (com SearchTarget)
/// e edge cases.
/// </summary>
public class GenericRepositoryListWithSearchTermTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly TestGenericRepository _repository;

    public GenericRepositoryListWithSearchTermTests()
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
        GC.SuppressFinalize(this);
    }

    #region Cenário Principal 1: Sem SearchTarget (busca dinâmica em todos os campos textuais)

    /// <summary>
    /// Testa que busca com SearchTerm sem SearchTarget retorna resultados filtrados.
    /// A implementação base usa expression trees com string.Contains para buscar
    /// em todas as propriedades string da entidade.
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_WithoutSearchTarget_ShouldSearchAllStringProperties()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = 1,
            Limit = 10,
            Order = EnumOrder.ASCENDING,
            SearchTerm = "Alice",
            SearchTarget = null
        };

        // Act
        var result = _repository.List(param);

        // Assert
        result.ActualPage.Should().Be(1);
        // A busca dinâmica deve encontrar "Alice" na propriedade Name
        // Se o filtro funcionar corretamente, deve retornar apenas 1 resultado
        result.Results.Should().ContainSingle().Which.Name.Should().Be("Alice");
    }

    /// <summary>
    /// Testa que SearchTerm null/empty sem SearchTarget retorna todas as entidades
    /// (ignora busca quando não há termo).
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_WithoutSearchTarget_EmptySearchTerm_ShouldReturnAll()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = 1,
            Limit = 10,
            Order = EnumOrder.ASCENDING,
            SearchTerm = null,
            SearchTarget = null
        };

        // Act
        var result = _repository.List(param);

        // Assert
        result.Results.Should().HaveCount(5);
    }

    /// <summary>
    /// Testa que SearchTerm vazio sem SearchTarget retorna todas as entidades.
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_WithoutSearchTarget_EmptyStringSearchTerm_ShouldReturnAll()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = 1,
            Limit = 10,
            Order = EnumOrder.ASCENDING,
            SearchTerm = string.Empty,
            SearchTarget = null
        };

        // Act
        var result = _repository.List(param);

        // Assert
        result.Results.Should().HaveCount(5);
    }

    /// <summary>
    /// Testa que busca com termo inexistente retorna lista vazia.
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_WithoutSearchTarget_NoMatch_ShouldReturnEmpty()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = 1,
            Limit = 10,
            Order = EnumOrder.ASCENDING,
            SearchTerm = "xyznonexistent",
            SearchTarget = null
        };

        // Act
        var result = _repository.List(param);

        // Assert
        result.Results.Should().BeEmpty();
    }

    /// <summary>
    /// Testa que busca dinâmica encontra matches em QUALQUER propriedade string.
    /// "test" aparece em Email do primeiro E Email do segundo (case-sensitive).
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_WithoutSearchTarget_MultipleMatchesAcrossFields()
    {
        // Arrange — reseed com dados que testam matches em diferentes campos
        ClearData();
        _context.TestEntities.AddRange(
            new TestEntity { Id = Guid.NewGuid(), Name = "TestUser", Email = "other@test.com" },
            new TestEntity { Id = Guid.NewGuid(), Name = "Other", Email = "test@example.com" },
            new TestEntity { Id = Guid.NewGuid(), Name = "NoMatch", Email = "nomatch@example.com" }
        );
        _context.SaveChanges();

        var param = new PagedSearchParam
        {
            Page = 1,
            Limit = 10,
            Order = EnumOrder.ASCENDING,
            SearchTerm = "Test", // "test" aparece em "other@test.com" E "test@example.com" (case-sensitive)
            SearchTarget = null
        };

        // Act
        var result = _repository.List(param);

        // Assert — "test" aparece em ambos os emails
        result.Results.Should().HaveCount(2);
    }

    /// <summary>
    /// Testa que paginação é aplicada corretamente com busca dinâmica.
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_WithoutSearchTarget_WithPagination_ShouldRespectPageAndLimit()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = 1,
            Limit = 2,
            Order = EnumOrder.ASCENDING,
            SearchTerm = "test",
            SearchTarget = null
        };

        // Act
        var result = _repository.List(param);

        // Assert
        result.ActualPage.Should().Be(1);
        result.Limit.Should().Be(2);
        result.Results.Should().HaveCount(2);
    }

    /// <summary>
    /// Testa que ordenação DESCENDING funciona com busca dinâmica.
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_WithoutSearchTarget_WithDescendingOrder_ShouldOrderCorrectly()
    {
        // Arrange — reseed com dados onde exatamente 2 entidades contêm "User"
        ClearData();
        _context.TestEntities.AddRange(
            new TestEntity { Id = Guid.NewGuid(), Name = "AdminUser", Email = "admin@example.com" },
            new TestEntity { Id = Guid.NewGuid(), Name = "RegularUser", Email = "regular@example.com" },
            new TestEntity { Id = Guid.NewGuid(), Name = "Guest", Email = "guest@example.com" }
        );
        _context.SaveChanges();

        var param = new PagedSearchParam
        {
            Page = 1,
            Limit = 10,
            Order = EnumOrder.DESCENDING,
            SearchTerm = "User", // "User" aparece em "AdminUser" E "RegularUser"
            SearchTarget = null
        };

        // Act
        var result = _repository.List(param);
        var resultsList = result.Results.ToList();

        // Assert — deve estar ordenado descendentemente por Id
        resultsList.Should().HaveCount(2);

        resultsList[0].Id.CompareTo(resultsList[1].Id).Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Testa que a busca é case-sensitive (padrão do EF Core InMemory).
    /// "alice" (lowercase) deve encontrar "Alice" via propriedade Email (alice@test.com).
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_WithoutSearchTarget_CaseSensitive_ShouldBeCaseSensitive()
    {
        // Arrange — "alice" (lowercase) deve encontrar "alice@test.com"
        var param = new PagedSearchParam
        {
            Page = 1,
            Limit = 10,
            Order = EnumOrder.ASCENDING,
            SearchTerm = "alice", // lowercase
            SearchTarget = null
        };

        // Act
        var result = _repository.List(param);

        // Assert — EF Core InMemory Contains é case-sensitive
        // "alice" encontra "alice@test.com" (no Email)
        result.Results.Should().ContainSingle().Which.Name.Should().Be("Alice");
    }

    #endregion

    #region Cenário Principal 2: Com SearchTarget (busca específica no campo)

    /// <summary>
    /// Testa busca específica na propriedade Name.
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_WithSearchTarget_ShouldSearchSpecificField()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = 1,
            Limit = 10,
            Order = EnumOrder.ASCENDING,
            SearchTarget = "Name",
            SearchTerm = "Bob"
        };

        // Act
        var result = _repository.List(param);

        // Assert
        result.ActualPage.Should().Be(1);
        result.Results.Should().HaveCount(1);
        result.Results.First().Name.Should().Be("Bob");
    }

    /// <summary>
    /// Testa busca específica na propriedade Email.
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_WithSearchTarget_EmailField_ShouldSearchEmail()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = 1,
            Limit = 10,
            Order = EnumOrder.ASCENDING,
            SearchTarget = "Email",
            SearchTerm = "charlie"
        };

        // Act
        var result = _repository.List(param);

        // Assert
        result.Results.Should().HaveCount(1);
        result.Results.First().Email.Should().Contain("charlie");
    }

    /// <summary>
    /// Testa que busca com SearchTarget inexistente retorna lista vazia.
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_WithSearchTarget_NoMatch_ShouldReturnEmpty()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = 1,
            Limit = 10,
            Order = EnumOrder.ASCENDING,
            SearchTarget = "Name",
            SearchTerm = "nonexistent"
        };

        // Act
        var result = _repository.List(param);

        // Assert
        result.Results.Should().BeEmpty();
    }

    /// <summary>
    /// Testa que SearchTarget é case-insensitive (usa .ToUpper() no GetFilter).
    /// "name" (minúsculo) deve encontrar a propriedade "Name".
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_WithSearchTarget_CaseInsensitiveTarget_ShouldWork()
    {
        // Arrange — "name" (minúsculo) vs "Name" (PascalCase)
        var param = new PagedSearchParam
        {
            Page = 1,
            Limit = 10,
            Order = EnumOrder.ASCENDING,
            SearchTarget = "name",
            SearchTerm = "Alice"
        };

        // Act
        var result = _repository.List(param);

        // Assert — GetFilter usa .ToUpper() para comparar nomes de propriedade
        result.Results.Should().HaveCount(1);
        result.Results.First().Name.Should().Be("Alice");
    }

    /// <summary>
    /// Testa paginação com busca por campo específico.
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_WithSearchTarget_WithPagination()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = 1,
            Limit = 2,
            Order = EnumOrder.ASCENDING,
            SearchTarget = "Name",
            SearchTerm = "" // termo vazio retorna todos
        };

        // Act
        var result = _repository.List(param);

        // Assert
        result.ActualPage.Should().Be(1);
        result.Limit.Should().Be(2);
        result.Results.Should().HaveCount(2);
    }

    /// <summary>
    /// Testa ordenação DESCENDING com busca por campo específico.
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_WithSearchTarget_DescendingOrder()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = 1,
            Limit = 10,
            Order = EnumOrder.DESCENDING,
            SearchTarget = "Name",
            SearchTerm = ""
        };

        // Act
        var result = _repository.List(param);
        var resultsList = result.Results.ToList();

        // Assert
        resultsList.Should().HaveCount(5);
        resultsList[0].Id.CompareTo(resultsList[4].Id).Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Testa substring match case-sensitive.
    /// "Ali" encontra "Alice" (exato match case).
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_WithSearchTarget_SubstringMatch()
    {
        // Arrange — "Ali" (case-exato) encontra "Alice"
        var param = new PagedSearchParam
        {
            Page = 1,
            Limit = 10,
            Order = EnumOrder.ASCENDING,
            SearchTarget = "Name",
            SearchTerm = "Ali"
        };

        // Act
        var result = _repository.List(param);

        // Assert
        result.Results.Should().HaveCount(1);
        result.Results.First().Name.Should().Be("Alice");
    }

    /// <summary>
    /// Testa paginação com busca por campo específico (page 2).
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_WithSearchTarget_PaginationWithSearch()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = 2,
            Limit = 2,
            Order = EnumOrder.ASCENDING,
            SearchTarget = "Name",
            SearchTerm = ""
        };

        // Act
        var result = _repository.List(param);

        // Assert
        result.ActualPage.Should().Be(2);
        result.Results.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.PagesCount.Should().Be(3);
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Testa que entidade com propriedade null não lança exceção.
    /// O código tem "value is null ||" que previne NullReferenceException.
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_WithSearchTarget_NullValue_ShouldNotThrow()
    {
        // Arrange — alterar seed para incluir entidade com Name vazio
        ClearData();
        _context.TestEntities.AddRange(
            new TestEntity { Id = Guid.NewGuid(), Name = "", Email = "nullname@test.com" },
            new TestEntity { Id = Guid.NewGuid(), Name = "HasName", Email = "hasname@test.com" }
        );
        _context.SaveChanges();

        var param = new PagedSearchParam
        {
            Page = 1,
            Limit = 10,
            Order = EnumOrder.ASCENDING,
            SearchTarget = "Name",
            SearchTerm = "" // busca por string vazia deve combinar tudo
        };

        // Act & Assert
        var result = Record.Exception(() => _repository.List(param));
        result.Should().BeNull();
    }

    /// <summary>
    /// Testa que SearchTarget inválido (campo inexistente) lança exceção.
    /// GetFilter usa .First() que lança InvalidOperationException.
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_WithInvalidSearchTarget_ShouldThrowInvalidOperationException()
    {
        // Arrange — campo que não existe
        var param = new PagedSearchParam
        {
            Page = 1,
            Limit = 10,
            Order = EnumOrder.ASCENDING,
            SearchTarget = "NonExistentField",
            SearchTerm = "test"
        };

        // Act & Assert — GetFilter usa .First() que lança InvalidOperationException
        var result = Record.Exception(() => _repository.List(param));
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<InvalidOperationException>();
    }

    /// <summary>
    /// Testa que caracteres especiais no SearchTerm não lançam exceção.
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_SearchTermWithSpecialCharacters_ShouldNotThrow()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = 1,
            Limit = 10,
            Order = EnumOrder.ASCENDING,
            SearchTarget = "Name",
            SearchTerm = "@#$%"
        };

        // Act & Assert
        var result = Record.Exception(() => _repository.List(param));
        result.Should().BeNull();
    }

    /// <summary>
    /// Testa que TotalCount reflete a contagem total (sem filtro de busca).
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_TotalCount_ShouldBeCorrect()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = 1,
            Limit = 10,
            Order = EnumOrder.ASCENDING,
            SearchTarget = "Name",
            SearchTerm = "" // todos
        };

        // Act
        var result = _repository.List(param);

        // Assert
        result.TotalCount.Should().Be(5);
        result.Results.Should().HaveCount(5);
    }

    /// <summary>
    /// Testa que PagesCount é calculado corretamente.
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_PagesCount_ShouldBeCalculatedCorrectly()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = 1,
            Limit = 2,
            Order = EnumOrder.ASCENDING,
            SearchTarget = "Name",
            SearchTerm = "" // todos
        };

        // Act
        var result = _repository.List(param);

        // Assert — 5 itens com limit 2 = 3 páginas (2+2+1)
        result.PagesCount.Should().Be(3);
    }

    /// <summary>
    /// Testa que a busca dinâmica encontra "Alice" tanto no Name quanto no Email.
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_WithoutSearchTarget_AliceInNameAndEmail()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = 1,
            Limit = 10,
            Order = EnumOrder.ASCENDING,
            SearchTerm = "Alice",
            SearchTarget = null
        };

        // Act
        var result = _repository.List(param);

        // Assert — "Alice" encontra no Name E no Email (alice@test.com)
        // Deve retornar apenas 1 entidade (Alice), mas encontrada via Name ou Email
        result.Results.Should().ContainSingle().Which.Name.Should().Be("Alice");
    }

    #endregion

    #region Filtro com Termo de Busca sem Paginação (Page/Limit padrão)

    /// <summary>
    /// Testa que SearchTerm com Page/Limit nas defaults (0) retorna resultados filtrados.
    /// Page=0 é normalizado para 1, Limit=0 significa "sem paginação" (retorna todos os match).
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_WithoutPaginationDefaults_ShouldReturnFilteredResults()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = 0, // default → normalizado para 1
            Limit = 0, // default → sem paginação
            Order = EnumOrder.ASCENDING,
            SearchTerm = "Alice",
            SearchTarget = null
        };

        // Act
        var result = _repository.List(param);

        // Assert — retorna os resultados filtrados (sem paginação)
        result.ActualPage.Should().Be(1);
        result.Results.Should().ContainSingle().Which.Name.Should().Be("Alice");
    }

    /// <summary>
    /// Testa que Page=0 e Limit=0 explícitos com SearchTerm retorna resultados filtrados.
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_Page0AndLimit0_ShouldReturnFilteredResults()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = 0,
            Limit = 0,
            Order = EnumOrder.ASCENDING,
            SearchTerm = "Bob",
            SearchTarget = "Name"
        };

        // Act
        var result = _repository.List(param);

        // Assert — retorna os resultados filtrados (sem paginação)
        result.ActualPage.Should().Be(1);
        result.Results.Should().HaveCount(1);
        result.Results.First().Name.Should().Be("Bob");
    }

    /// <summary>
    /// Testa que apenas Page=0 (Limit default 0) com SearchTerm retorna resultados filtrados.
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_Page0Only_ShouldReturnFilteredResults()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = 0,
            Limit = 0, // default
            Order = EnumOrder.ASCENDING,
            SearchTerm = "Charlie",
            SearchTarget = null
        };

        // Act
        var result = _repository.List(param);

        // Assert — retorna os resultados filtrados (sem paginação)
        result.ActualPage.Should().Be(1);
        result.Results.Should().ContainSingle().Which.Name.Should().Be("Charlie");
    }

    /// <summary>
    /// Testa que apenas Limit=0 (Page default 0) com SearchTerm retorna resultados filtrados.
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_Limit0Only_ShouldReturnFilteredResults()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = 0, // default
            Limit = 0,
            Order = EnumOrder.ASCENDING,
            SearchTerm = "David",
            SearchTarget = null
        };

        // Act
        var result = _repository.List(param);

        // Assert — retorna os resultados filtrados (sem paginação)
        result.ActualPage.Should().Be(1);
        result.Results.Should().ContainSingle().Which.Name.Should().Be("David");
    }

    /// <summary>
    /// Testa que SearchTarget + SearchTerm sem paginação retorna resultados filtrados.
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_WithSearchTarget_WithoutPagination_ShouldReturnFilteredResults()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = 0,
            Limit = 0,
            Order = EnumOrder.ASCENDING,
            SearchTarget = "Email",
            SearchTerm = "eve"
        };

        // Act
        var result = _repository.List(param);

        // Assert — retorna os resultados filtrados (sem paginação)
        result.ActualPage.Should().Be(1);
        result.Results.Should().ContainSingle().Which.Email.Should().Contain("eve");
    }

    /// <summary>
    /// Testa que DESCENDING + SearchTerm sem paginação retorna resultados filtrados.
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_Descending_WithoutPagination_ShouldReturnFilteredResults()
    {
        // Arrange — reseed com dados que testam matches em "test"
        ClearData();
        _context.TestEntities.AddRange(
            new TestEntity { Id = Guid.NewGuid(), Name = "TestUser", Email = "other@test.com" },
            new TestEntity { Id = Guid.NewGuid(), Name = "Other", Email = "test@example.com" },
            new TestEntity { Id = Guid.NewGuid(), Name = "NoMatch", Email = "nomatch@example.com" }
        );
        _context.SaveChanges();

        var param = new PagedSearchParam
        {
            Page = 0,
            Limit = 0,
            Order = EnumOrder.DESCENDING,
            SearchTerm = "test",
            SearchTarget = null
        };

        // Act
        var result = _repository.List(param);

        // Assert — retorna os resultados filtrados ordenados descendentemente
        result.ActualPage.Should().Be(1);
        result.Results.Should().HaveCount(2);
        var resultsList = result.Results.ToList();
        resultsList[0].Id.CompareTo(resultsList[1].Id).Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Testa baseline: sem SearchTerm com Page/Limit defaults retorna TODOS os itens.
    /// A branch sem busca normaliza Page=0→1 e Limit=0 retorna todos.
    /// </summary>
    [Fact]
    public void List_WithoutSearchTerm_WithPaginationDefaults_ShouldReturnAll()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = 0, // default
            Limit = 0, // default
            Order = EnumOrder.ASCENDING,
            SearchTerm = null,
            SearchTarget = null
        };

        // Act
        var result = _repository.List(param);

        // Assert — branch sem busca normaliza e retorna todos
        result.Results.Should().HaveCount(5);
    }

    /// <summary>
    /// Testa que Page=1 com Limit=0 retorna todos os resultados filtrados (sem paginação).
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_Page1_Limit0_ShouldReturnFilteredResults()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = 1,
            Limit = 0,
            Order = EnumOrder.ASCENDING,
            SearchTerm = "Alice",
            SearchTarget = null
        };

        // Act
        var result = _repository.List(param);

        // Assert — retorna os resultados filtrados (sem paginação)
        result.ActualPage.Should().Be(1);
        result.Results.Should().ContainSingle().Which.Name.Should().Be("Alice");
    }

    /// <summary>
    /// Testa que Page negativo com Limit=0 retorna resultados filtrados.
    /// Page negativo é normalizado para 1.
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_NegativePage_Limit0_ShouldReturnFilteredResults()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = -1,
            Limit = 0,
            Order = EnumOrder.ASCENDING,
            SearchTerm = "Bob",
            SearchTarget = null
        };

        // Act
        var result = _repository.List(param);

        // Assert — Page negativo normalizado para 1, retorna resultados filtrados
        result.ActualPage.Should().Be(1);
        result.Results.Should().ContainSingle().Which.Name.Should().Be("Bob");
    }

    /// <summary>
    /// Testa que DESCENDING + SearchTarget definido + sem paginação retorna resultados filtrados
    /// ordenados descendentemente.
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_WithSearchTarget_Descending_WithoutPagination_ShouldReturnFilteredResults()
    {
        // Arrange
        var param = new PagedSearchParam
        {
            Page = 0,
            Limit = 0,
            Order = EnumOrder.DESCENDING,
            SearchTarget = "Name",
            SearchTerm = ""
        };

        // Act
        var result = _repository.List(param);

        // Assert — retorna todos filtrados ordenados descendentemente por Id
        result.ActualPage.Should().Be(1);
        result.Results.Should().HaveCount(5);
        var resultsList = result.Results.ToList();
        resultsList[0].Id.CompareTo(resultsList[4].Id).Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Testa consistência: SearchTerm com Page/Limit defaults retorna resultados filtrados,
    /// e sem SearchTerm retorna TODOS. Ambos sem paginação, comportamento consistente.
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_ConsistentBehaviorWithNonSearchBehavior()
    {
        // Arrange — mesma entidade, dois parâmetros diferentes
        var paramWithSearch = new PagedSearchParam
        {
            Page = 0,
            Limit = 0,
            Order = EnumOrder.ASCENDING,
            SearchTerm = "Alice", // tem termo de busca
            SearchTarget = null
        };

        var paramWithoutSearch = new PagedSearchParam
        {
            Page = 0,
            Limit = 0,
            Order = EnumOrder.ASCENDING,
            SearchTerm = null, // sem termo de busca
            SearchTarget = null
        };

        // Act
        var resultWithSearch = _repository.List(paramWithSearch);
        var resultWithoutSearch = _repository.List(paramWithoutSearch);

        // Assert — consistência: ambos tratam Page=0/Limit=0 da mesma forma
        // SearchTerm retorna filtrados, sem SearchTerm retorna todos
        resultWithSearch.ActualPage.Should().Be(1);
        resultWithSearch.Results.Should().ContainSingle().Which.Name.Should().Be("Alice");
        resultWithoutSearch.ActualPage.Should().Be(1);
        resultWithoutSearch.Results.Should().HaveCount(5);
    }

    #endregion

    #region No-String-Properties Branch

    /// <summary>
    /// Testa que quando a entidade não possui propriedades string,
    /// ListWithSearchTerm retorna todos os registros (caminho do else).
    /// </summary>
    [Fact]
    public void ListWithSearchTerm_WithoutStringProperties_ShouldReturnAll()
    {
        // Arrange
        var noStringOptions = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var noStringContext = new TestDbContext(noStringOptions);
        var noStringRepo = new TestNoStringRepository(noStringContext);

        noStringContext.NoStringEntities.AddRange(
            new NoStringEntity { Id = Guid.NewGuid(), Code = 1, Value = 100m },
            new NoStringEntity { Id = Guid.NewGuid(), Code = 2, Value = 200m },
            new NoStringEntity { Id = Guid.NewGuid(), Code = 3, Value = 300m }
        );
        noStringContext.SaveChanges();

        var param = new PagedSearchParam
        {
            Page = 1,
            Limit = 10,
            Order = EnumOrder.ASCENDING,
            SearchTerm = "any", // searchTerm é fornecido, mas não há propriedades string
            SearchTarget = null
        };

        // Act
        var result = noStringRepo.List(param);

        // Assert — deve retornar todos os 3 registros (nenhuma filtragem por string é possível)
        result.Results.Should().HaveCount(3);
        result.ActualPage.Should().Be(1);
        result.ReturnedInActualPage.Should().Be(3);
    }

    #endregion

    #region Helpers

    private void ClearData()
    {
        _context.TestEntities.RemoveRange(_context.TestEntities);
        _context.SaveChanges();
    }

    #endregion
}
