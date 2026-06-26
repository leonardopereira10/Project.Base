using Project.Base.Domain.Object.Shared;

namespace Project.Base.Domain.Repositories;

public interface IBaseObjectWithIdRepository<TObjectWithID> : IGenericRepository<TObjectWithID> where TObjectWithID : BaseObjectWithId
{
    // ── Sync (compatibilidade) ──
    TObjectWithID? GetById(Guid objectId);

    TObjectWithID Delete(Guid objectId);

    // ── Async ──
    Task<TObjectWithID?> GetByIdAsync(Guid objectId);

    Task<TObjectWithID> DeleteAsync(Guid objectId);
}
