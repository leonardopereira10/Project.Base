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

    protected override PagedSearchReturn<TestEntity> ListWithSearchTerm(PagedSearchParam searchParams)
    {
        IQueryable<TestEntity> query = Persistence;

        query = searchParams.Order == EnumOrder.ASCENDING
            ? query.OrderBy(x => x.Id).Skip((searchParams.Page - 1) * searchParams.Limit).Take(searchParams.Limit)
            : query.OrderByDescending(x => x.Id).Skip((searchParams.Page - 1) * searchParams.Limit).Take(searchParams.Limit);

        var results = query.ToList();

        return new PagedSearchReturn<TestEntity>
        {
            ActualPage = searchParams.Page,
            Results = results,
            Limit = searchParams.Limit,
            ReturnedInActualPage = results.Count,
            TotalCount = Persistence.Count(),
            PagesCount = searchParams.Limit > 0
                ? (int)Math.Ceiling(Persistence.Count() / (double)searchParams.Limit)
                : 0
        };
    }
}
