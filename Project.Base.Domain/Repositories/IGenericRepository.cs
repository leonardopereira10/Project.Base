using System.Linq.Expressions;
using Project.Base.Domain.Object.Shared;

namespace Project.Base.Domain.Repositories;

public interface IGenericRepository<TObject> where TObject : BaseObjectWithId
{
    // ── Sync (compatibilidade) ──
    TObject Insert(TObject newObject);

    IEnumerable<TObject> List();

    IEnumerable<TObject> List(Expression<Func<TObject, bool>> predicate);

    PagedSearchReturn<TObject> List(PagedSearchParam searchParams);

    TObject Update(TObject updatedObject);

    TObject Delete(TObject obj);

    // ── Async ──
    Task<TObject> InsertAsync(TObject newObject);

    Task<IEnumerable<TObject>> ListAsync();

    Task<IEnumerable<TObject>> ListAsync(Expression<Func<TObject, bool>> predicate);

    Task<TObject> UpdateAsync(TObject updatedObject);

    Task<TObject> DeleteAsync(TObject obj);
}
