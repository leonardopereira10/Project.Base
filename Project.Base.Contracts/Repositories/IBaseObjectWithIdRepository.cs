namespace Project.Base.Contracts.Repositories;

public interface IBaseObjectWithIdRepository<TObjectWithID> : IGenericRepository<TObjectWithID>
{
    TObjectWithID GetById(Guid objectId);

    TObjectWithID Delete(Guid objectId);
}
