using System.Linq.Expressions;
using Project.Base.Domain.Object.Shared;

namespace Project.Base.Domain.Repositories;

public interface IGenericRepository<TObject> where TObject : BaseObjectWithId
{
    TObject Insert(TObject newObject);

    IEnumerable<TObject> List();

    IEnumerable<TObject> List(Expression<Func<TObject, bool>> predicate);

    PagedSearchReturn<TObject> List(PagedSearchParam searchParams);

    TObject Update(TObject updatedObject);

    TObject Delete(TObject obj);
}
