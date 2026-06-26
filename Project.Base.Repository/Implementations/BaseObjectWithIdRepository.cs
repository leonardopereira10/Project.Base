using Microsoft.EntityFrameworkCore;
using Project.Base.Domain.Object.Shared;
using Project.Base.Domain.Repositories;

namespace Project.Base.Repository.Implementations
{
    public abstract class BaseObjectWithIdRepository<TObject> : GenericRepository<TObject>, IBaseObjectWithIdRepository<TObject>
        where TObject : BaseObjectWithId
    {
        protected BaseObjectWithIdRepository(DbContext context) : base(context) { }

        // ── Sync (compatibilidade) ──
        // FIX: corrigido bug que retornava IQueryable castado como TObject (nunca executava a query)
        public TObject? GetById(Guid objectId)
        {
            return Persistence.FirstOrDefault(x => x.Id == objectId);
        }

        public TObject Delete(Guid objectId)
        {
            return Delete(GetById(objectId)!);
        }

        // ── Async ──
        public async Task<TObject?> GetByIdAsync(Guid objectId)
        {
            return await Persistence.FirstOrDefaultAsync(x => x.Id == objectId).ConfigureAwait(false);
        }

        public async Task<TObject> DeleteAsync(Guid objectId)
        {
            TObject? obj = await GetByIdAsync(objectId).ConfigureAwait(false);
            if (obj is null)
            {
                throw new InvalidOperationException($"Entity with id {objectId} not found.");
            }
            return await base.DeleteAsync(obj).ConfigureAwait(false);
        }
    }
}
