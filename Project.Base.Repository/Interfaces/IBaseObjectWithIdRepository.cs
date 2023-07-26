using Project.Base.Domain.Object.Shared;

namespace Project.Base.Repository.Interfaces;

public interface IBaseObjectWithIdRepository<TObjectWithID> : IGenericRepository<TObjectWithID> where TObjectWithID : BaseObjectWithId
{
    TObjectWithID GetById(Guid objectId);

    TObjectWithID Delete(Guid objectId);
}
