using Microsoft.EntityFrameworkCore;
using Project.Base.Domain.Object.Shared;
using Project.Base.Domain.Repositories;

namespace Project.Base.Repository.Implementations
{
    /// <summary>
    /// Provides concrete CRUD operations for entities that inherit from <see cref="BaseObjectWithId"/>,
    /// including GUID-specific retrieval and deletion methods in both synchronous and asynchronous forms.
    /// </summary>
    /// <typeparam name="TObject">The entity type, constrained to derive from <see cref="BaseObjectWithId"/>.</typeparam>
    public abstract class BaseObjectWithIdRepository<TObject> : GenericRepository<TObject>, IBaseObjectWithIdRepository<TObject>
        where TObject : BaseObjectWithId
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseObjectWithIdRepository{TObject}"/> class
        /// using the specified database context.
        /// </summary>
        /// <param name="context">The Entity Framework database context used for data persistence.</param>
        protected BaseObjectWithIdRepository(DbContext context) : base(context) { }

        // ── Sync (compatibilidade) ──
        // FIX: corrigido bug que retornava IQueryable castado como TObject (nunca executava a query)

        /// <summary>
        /// Retrieves a single entity by its unique identifier using synchronous execution.
        /// </summary>
        /// <param name="objectId">The globally unique identifier of the entity to retrieve.</param>
        /// <returns>The entity matching the given identifier, or <c>null</c> if not found.</returns>
        public TObject? GetById(Guid objectId)
        {
            return Persistence.FirstOrDefault(x => x.Id == objectId);
        }

        /// <summary>
        /// Deletes an entity identified by its unique identifier using synchronous execution.
        /// </summary>
        /// <param name="objectId">The globally unique identifier of the entity to delete.</param>
        /// <returns>The deleted entity.</returns>
        public TObject Delete(Guid objectId)
        {
            return Delete(GetById(objectId)!);
        }

        // ── Async ──

        /// <summary>
        /// Retrieves a single entity by its unique identifier using asynchronous execution.
        /// </summary>
        /// <param name="objectId">The globally unique identifier of the entity to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains
        /// the entity matching the given identifier, or <c>null</c> if not found.</returns>
        public async Task<TObject?> GetByIdAsync(Guid objectId)
        {
            return await Persistence.FirstOrDefaultAsync(x => x.Id == objectId).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes an entity identified by its unique identifier using asynchronous execution.
        /// </summary>
        /// <param name="objectId">The globally unique identifier of the entity to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains
        /// the deleted entity.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no entity with the given identifier exists.</exception>
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
