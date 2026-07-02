using Microsoft.EntityFrameworkCore;
using Project.Base.Domain.Object.Shared;
using Project.Base.Repository.Implementations;
using Project.Base.Tests.Domain;

namespace Project.Base.Tests.Repository;

public class TestBaseObjectWithIdRepository : BaseObjectWithIdRepository<TestEntity>
{
    public TestBaseObjectWithIdRepository(DbContext context) : base(context) { }
}
