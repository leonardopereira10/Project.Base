# Project.Base

Project.Base is a .NET boilerplate/scaffolding library for building REST API applications with a clean layered architecture. It provides a fully generic framework — abstract controllers, services, repositories, converters, and validators — so you only need to inherit from the base classes and implement a few methods to have a complete CRUD API with validation, pagination, dynamic search, and DTO auto-conversion.

## Getting started

To start using Project.Base in your own application, you need to:

1. **Install via NuGet** (see below), or
2. Clone this repository and add references to the 5 projects.
3. For each entity you want to expose as an API, create:
   - A **Domain Entity** inheriting from `BaseObjectWithId`
   - A **DTO** inheriting from `DtoBase`
   - A **Converter** inheriting from `DefaultConverter<TObj, TDto>`
   - A **Validator** inheriting from `BaseAbstractValidator<TObject>`
   - A **Repository** inheriting from `GenericRepository<TObjeto>`
   - A **Service** inheriting from `BaseService<TObject, TDto>`
   - A **Controller** inheriting from `AbstractController<TDto>`

### Installation

Install via NuGet package manager:

```bash
dotnet add package Project.Base
```

Or via Package Manager Console:

```
Install-Package Project.Base
```

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) (LTS) or later
- An IDE supporting .NET (Visual Studio 2022, JetBrains Rider, VS Code)
- A relational database (Entity Framework Core supports SQL Server, PostgreSQL, SQLite, In-Memory, etc.)

## Usage

### 1. Define your Entity

```csharp
public class Product : BaseObjectWithId
{
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
}
```

### 2. Define your DTO

```csharp
public class ProductDto : DtoBase
{
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
}
```

### 3. Create the Converter (uses Mapster)

```csharp
public class ProductConverter : DefaultConverter<Product, ProductDto>
{
    // Mapster handles the mapping automatically.
    // Override Convert() / Convert(TDto) if you need custom mapping rules.
}
```

### 4. Create the Validator (uses FluentValidation)

```csharp
public class ProductValidator : BaseAbstractValidator<Product>
{
    public override void AssignCommonValidations()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Price)
            .GreaterThan(0);
    }
}
```

### 5. Create the Repository (uses EF Core)

```csharp
public class ProductRepository : GenericRepository<Product>
{
    public ProductRepository(DbContext context) : base(context) { }

    // ListWithSearchTerm is already implemented with dynamic search
    // across all string properties. Override only if you need custom logic.
}
```

### 6. Create the Service

```csharp
public class ProductService : BaseService<Product, ProductDto>
{
    public ProductService(IBaseObjectWithIdRepository<Product> repository)
        : base(repository) { }

    protected override IBaseAbstractValidator<Product> Validator()
        => new ProductValidator();

    protected override IDefaultConverter<Product, ProductDto> Converter()
        => new ProductConverter();
}
```

### 7. Create the Controller

```csharp
[Route("api/[controller]")]
[ApiController]
public class ProductController : AbstractController<ProductDto>
{
    public ProductController(IBaseService<ProductDto> service)
        : base(service) { }
}
```

### API Endpoints

Your controller automatically exposes these endpoints (inherited from `AbstractController<TDto>`):

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/{controller}` | Insert a new entity (body: DTO) |
| `PUT` | `/api/{controller}` | Update an existing entity (body: DTO) |
| `DELETE` | `/api/{controller}?id={guid}` | Delete an entity by ID |
| `GET` | `/api/{controller}/findbyid?id={guid}` | Find a single entity by ID |
| `GET` | `/api/{controller}/findall` | Find all entities |
| `GET` | `/api/{controller}/find?page=1&limit=10&order=0` | Paginated search |

### Response Format

All responses are wrapped in `DtoOutput<TDto>`:

```json
{
  "success": true,
  "resultSet": [
    {
      "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "name": "Sample Product",
      "price": 29.99,
      "description": "A sample"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1
}
```

On validation failure:

```json
{
  "success": false,
  "resultSet": [
    {
      "id": "00000000-0000-0000-0000-000000000000",
      "name": "",
      "price": -1,
      "description": null
    }
  ],
  "validationFails": [
    {
      "message": "Name must not be empty.",
      "property": "Name",
      "isImpeditive": true
    },
    {
      "message": "Price must be greater than 0.",
      "property": "Price",
      "isImpeditive": true
    }
  ]
}
```

### Dynamic Search

The `ListWithSearchTerm` method in `GenericRepository` automatically searches across all `string` properties of your entity when no `searchTarget` is specified. To search a specific field, pass `searchTarget` as a query parameter.

### Pagination

Use the `Find` endpoint with query parameters:

```
GET /api/product/find?page=1&limit=20&order=0&searchTarget=Name&searchTerm=sample
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `page` | int | Page number (default: 1) |
| `limit` | int | Items per page |
| `order` | `EnumOrder` | `0` = DESCENDING, `1` = ASCENDING |
| `searchTarget` | string? | Property name to filter on |
| `searchTerm` | string? | Text to search for |

## Feedback

- **Issues**: Open an issue on the [GitHub repository](https://github.com/leonardopereira10/Project.Base/issues)
- **Discussions**: Start a conversation in the [GitHub Discussions](https://github.com/leonardopercavallo/Project.Base/discussions)
- **Contributing**: Pull requests are welcome. Please ensure unit tests pass before submitting.
