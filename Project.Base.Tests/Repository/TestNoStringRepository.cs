using Microsoft.EntityFrameworkCore;
using Project.Base.Repository.Implementations;
using Project.Base.Tests.Domain;

namespace Project.Base.Tests.Repository;

public class TestNoStringRepository : GenericRepository<NoStringEntity>
{
    public TestNoStringRepository(DbContext context) : base(context)
    {
    }
}
