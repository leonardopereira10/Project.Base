using System.Linq.Expressions;
using Project.Base.Domain.Object;

namespace Project.Base.Repository.Interfaces;

public interface IBaseObjectWithIdRepository<TObjectWithID> : IGenericRepository<TObjectWithID> where TObjectWithID : BaseObjectWithId
{
    TObjectWithID Insert (TObjectWithID newObject);

    TObjectWithID GetById (Guid objectId);

    IList<TObjectWithID> List ();

    IList<TObjectWithID> List (Expression<Func<TObjectWithID, bool>> predicate);

    PagedSearchReturn<TObjectWithID> List(PagedSearchParam searchParams);

    void Update (TObjectWithID updatedObject);

    void Delete (Guid objectId);
}
