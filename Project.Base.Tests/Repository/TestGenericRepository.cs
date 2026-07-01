using Microsoft.EntityFrameworkCore;
using Project.Base.Domain.Repositories;
using Project.Base.Enumerators;
using Project.Base.Repository.Implementations;
using Project.Base.Tests.Domain;

namespace Project.Base.Tests.Repository;

public class TestGenericRepository : GenericRepository<TestEntity>
{
    public TestGenericRepository(DbContext context) : base(context)
    {
    }
}
