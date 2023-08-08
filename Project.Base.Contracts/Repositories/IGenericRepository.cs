using System.Linq.Expressions;
using Project.Base.Contracts.Models;

namespace Project.Base.Contracts.Repositories;

public interface IGenericRepository<TObject>
{
    TObject Insert(TObject newObject);

    IEnumerable<TObject> List();

    IEnumerable<TObject> List(Expression<Func<TObject, bool>> predicate);

    PagedSearchReturn<TObject> List(PagedSearchParam searchParams);

    TObject Update(TObject updatedObject);

    TObject Delete(TObject obj);
}
