# Project.Base - Contexto do Projeto

## Visão Geral

**Project.Base** é uma biblioteca base (boilerplate/scaffolding) para construção de aplicações API REST em C# com **.NET 10.0 (LTS)**, seguindo os princípios de **arquitetura em camadas** (layered architecture) com repositórios genéricos, serviços genéricos, conversão automática DTO/Entity via Mapster, validação com FluentValidation, paginação e busca dinâmica.

É um projeto genérico que serve como ponto de partida para novos projetos — consumidores devem herdar suas classes abstratas (controllers, serviços, repositórios, validadores, conversores) para criar aplicações concretas.

> **Nota importante:** Project.Base é uma **biblioteca de herança**, não uma biblioteca NuGet. Consumidores herdam classes abstratas diretamente do código-fonte. O `AbstractController` expõe métodos protegidos que consumidores usam para criar seus endpoints concretos.

## Arquitetura

O projeto é organizado em **5 projetos separados** dentro da solution `Project.Base.sln`:

```
Project.Base/
├── Project.Base.WebApi/          # Camada de apresentação (ASP.NET Core Web API)
│   └── Controllers/
│       └── AbstractController.cs # Controlador base genérico (métodos protegidos)
│
├── Project.Base.Contracts/       # Camada de contratos (DTOs e interfaces)
│   ├── Models/
│   │   ├── DtoBase.cs            # Base para todos os DTOs (contém Guid Id)
│   │   ├── DtoOutput.cs          # Wrapper genérico de resposta com paginação
│   │   └── ValidationFail.cs     # Modelo de erro de validação
│   └── ServiceContracts/
│       └── IBaseService<TDto>    # Interface de serviço genérica (6 métodos CRUD)
│
├── Project.Base.Domain/          # Camada de domínio (lógica de negócio)
│   ├── Object/
│   │   └── Shared/
│   │       ├── BaseObjectWithId.cs  # Base para todas as entidades (Guid Id, IComparable)
│   │       └── IDefaultConverter.cs # Interface do conversor DTO<->Entity
│   ├── Converters/
│   │   └── DefaultConverter.cs      # Conversor genérico via Mapster (TypeAdapter.Adapt)
│   ├── Repositories/
│   │   ├── IGenericRepository<T>    # Interface de repositório genérico (CRUD sync+async)
│   │   ├── IBaseObjectWithIdRepository<T>  # Repositório específico para GUID
│   │   ├── PagedSearchParam.cs      # Parâmetro de busca paginada
│   │   └── PagedSearchReturn.cs     # Resultado de busca paginada
│   ├── Services/
│   │   └── BaseService<TObject, TDto>  # Serviço base genérico com validação (async/await real)
│   └── Validators/
│       ├── IBaseAbstractValidator.cs  # Interface de validador genérico
│       └── BaseAbstractValidator.cs   # Validador base com FluentValidation + validação contextual
│
├── Project.Base.Repository/      # Camada de persistência (EF Core)
│   └── Implementations/
│       ├── GenericRepository<TObjeto>       # Implementação genérica com EF Core
│       └── BaseObjectWithIdRepository<T>  # Implementação para entidades GUID
│
└── Project.Base.Enumerators/     # Utilitários (enums e internacionalização)
    ├── EnumOrder.cs               # DESCENDING / ASCENDING
    └── Globalization.resx         # Mensagens de erro localizadas
```

## Tecnologias e Dependências

| Tecnologia | Versão | Uso |
|---|---|---|
| **.NET Target** | net10.0 (LTS) | Todos os projetos |
| **ASP.NET Core** | 10.0 | Web API, MVC controllers |
| **Entity Framework Core** | 10.0.9 | Persistência data (Repository) |
| **ASP.NET Core Identity** | 10.0.9 | Auth database (Repository) |
| **FluentValidation** | 11.12.0 | Validação de objetos (Domain) |
| **Mapster** | 10.0.9 | Conversão DTO<->Entity (Domain) |
| **Swashbuckle.AspNetCore** | 10.2.3 | Documentação API (WebApi) |
| **xUnit** | 2.9.3 | Testes unitários |
| **Moq** | 4.20.72 | Mocking (Testes) |
| **FluentAssertions** | 8.2.0 | Assertions fluente (Testes) |
| **Microsoft.EntityFrameworkCore.InMemory** | 10.0.9 | Testes com DbContext in-memory |

## Construindo e Executando

### Build
```bash
dotnet build Project.Base.sln
dotnet build Project.Base.sln -c Release
```

### Executar (WebApi)
```bash
dotnet run --project Project.Base.WebApi
```

### Testes
```bash
dotnet test Project.Base.Tests
# 77 testes passando (Domain ~85-90%, Repository ~85%)
```

### Swagger/OpenAPI
Disponível em `https://localhost:{port}/swagger` quando a WebApi está rodando.

## Convenções de Código

### Naming (do `.editorconfig`)
- **Interfaces:** começam com `I` (ex: `IBaseService`, `IGenericRepository`)
- **Classes/Tipos:** PascalCase (ex: `BaseService`, `DtoOutput`)
- **Membros (propriedades, métodos, eventos):** PascalCase
- **Expression-bodied members:** true para propriedades, indexadores, acessadores e lambdas; false para métodos e construtores
- **Using directive:** outside_namespace
- **Indentação:** 4 espaços

### Nullability
- `Nullable` está **habilitado** em **todos** os projetos (Contracts, WebApi, Domain, Repository, Enumerators)

### Linguagem
- Mensagens de validação usam resource files (`Globalization.resx`) para internacionalização
- O `.editorconfig` contém apenas regras C# (regras VB removidas)

## Padrões Arquiteturais

| Padrão | Aplicação |
|---|---|
| **Layered Architecture** | 5 projetos separados por responsabilidade |
| **Repository Pattern** | `IGenericRepository<T>` com implementação EF Core genérica (CRUD sync+async) |
| **DTO Pattern** | `DtoBase` como base, `DtoOutput<T>` como wrapper de resposta com paginação |
| **Converter Pattern** | `IDefaultConverter<TObj, TDto>` com Mapster (`TypeAdapter.Adapt`) |
| **Validator Pattern** | `BaseAbstractValidator<T>` com FluentValidation + validação contextual (insert/update/delete) |
| **Template Method** | Classes base definem fluxo; subclasses sobrescrevem métodos abstratos (`Validator()`, `Converter()`, `AssignCommonValidations()`) |
| **Generic Programming** | Genéricos em todas as camadas (repositórios, serviços, controllers, conversores) |
| **Dependency Injection** | Injeção via construtor em `AbstractController` e `BaseService` |
| **Paginação** | `PagedSearchParam` / `PagedSearchReturn` com Skip/Take via EF Core |
| **Busca Dinâmica** | `ListWithSearchTerm` busca em todas as propriedades `string` via Expression Trees |

## Status das Correções (Sprints 1–3)

| Issue (antes) | Status | Correção |
|---|---|---|
| `Task.Factory.StartNew()` no BaseService | ✅ Corrigido (Sprint 1) | async/await real com `ConfigureAwait(false)` |
| JSON serialization no DefaultConverter | ✅ Corrigido (Sprint 2) | Mapster `TypeAdapter.Adapt` |
| `GetById` com cast incorreto | ✅ Corrigido (Sprint 1) | `FirstOrDefaultAsync()` no EF Core |
| `NotImplementedException` no Find | ✅ Corrigido (Sprint 1) | Implementação completa |
| `EnumOrder.DOWNWARD` | ✅ Corrigido (Sprint 1) | Renomeado para `EnumOrder.DESCENDING` |
| `GenericRepository.List()` sem execução | ✅ Corrigido (Sprint 1) | Retorna `List<T>` executado |
| `ListWithSearchTerm` abstrato | ✅ Corrigido (Sprint 3) | Implementação genérica com Expression Trees |
| Nullable desabilitado | ✅ Corrigido (Sprint 1) | Habilitado em todos os projetos |
| Regras VB no .editorconfig | ✅ Corrigido (Sprint 3) | Removidas |

## Como Estender

Para criar uma aplicação concreta usando este projeto base:

1. **Entidade:** Herde de `BaseObjectWithId`
2. **DTO:** Herde de `DtoBase`
3. **Converter:** Herde de `DefaultConverter<TObj, TDto>` (Mapster mapeia automaticamente)
4. **Validator:** Herde de `BaseAbstractValidator<TObject>`, implemente `AssignCommonValidations()`
5. **Repository:** Herde de `GenericRepository<TObjeto>`, sobrescreva `ListWithSearchTerm` se precisar de lógica customizada
6. **Service:** Herde de `BaseService<TObject, TDto>`, implemente `Validator()` e `Converter()`
7. **Controller:** Herde de `AbstractController<TDto>`, registre DI para service/repo

### Exemplo Rápido

```csharp
// 1. Entity
public class Product : BaseObjectWithId
{
    public required string Name { get; set; }
    public decimal Price { get; set; }
}

// 2. DTO
public class ProductDto : DtoBase
{
    public required string Name { get; set; }
    public decimal Price { get; set; }
}

// 3. Converter (Mapster mapeia automaticamente)
public class ProductConverter : DefaultConverter<Product, ProductDto> { }

// 4. Validator
public class ProductValidator : BaseAbstractValidator<Product>
{
    public override void AssignCommonValidations()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}

// 5. Repository
public class ProductRepository : GenericRepository<Product>
{
    public ProductRepository(DbContext context) : base(context) { }
}

// 6. Service
public class ProductService : BaseService<Product, ProductDto>
{
    public ProductService(IBaseObjectWithIdRepository<Product> repository)
        : base(repository) { }

    protected override IBaseAbstractValidator<Product> Validator()
        => new ProductValidator();

    protected override IDefaultConverter<Product, ProductDto> Converter()
        => new ProductConverter();
}

// 7. Controller
[Route("api/[controller]")]
[ApiController]
public class ProductController : AbstractController<ProductDto>
{
    public ProductController(IBaseService<ProductDto> service) : base(service) { }
}
```

## Arquivos Chave para Referência

- `Project.Base.WebApi/Controllers/AbstractController.cs` — Controlador base genérico (métodos protegidos)
- `Project.Base.Domain/Services/BaseService.cs` — Lógica de serviço genérica (async/await real)
- `Project.Base.Domain/Validators/BaseAbstractValidator.cs` — Validação base com FluentValidation
- `Project.Base.Domain/Converters/DefaultConverter.cs` — Conversor genérico com Mapster
- `Project.Base.Repository/Implementations/GenericRepository.cs` — Implementação EF Core com busca dinâmica
- `Project.Base.Contracts/Models/DtoOutput.cs` — Wrapper de resposta da API
- `Project.Base.Tests/` — 77 testes unitários (xUnit + Moq + FluentAssertions)
