using Project.Base.Domain.Object.Shared;

namespace Project.Base.Domain.Repositories;

/// <summary>
/// Defines the contract for a repository specialized in entities with GUID identifiers
/// (<see cref="BaseObjectWithId"/>). Extends <see cref="IGenericRepository{TObject}"/>
/// with GetById and Delete operations that accept a raw Guid instead of the full entity.
/// </summary>
/// <typeparam name="TObjectWithID">The entity type. Must inherit from <see cref="BaseObjectWithId"/>.</typeparam>
public interface IBaseObjectWithIdRepository<TObjectWithID> : IGenericRepository<TObjectWithID> where TObjectWithID : BaseObjectWithId
{
    // ── Sync (compatibilidade) ──

    /// <summary>
    /// Retrieves an entity by its unique identifier synchronously.
    /// </summary>
    /// <param name="objectId">The GUID identifier of the entity to find.</param>
    /// <returns>The entity if found; otherwise, <c>null</c>.</returns>
    TObjectWithID? GetById(Guid objectId);

    /// <summary>
    /// Deletes an entity by its unique identifier synchronously.
    /// </summary>
    /// <param name="objectId">The GUID identifier of the entity to delete.</param>
    /// <returns>The deleted entity.</returns>
    TObjectWithID Delete(Guid objectId);

    // ── Async ──

    /// <summary>
    /// Retrieves an entity by its unique identifier asynchronously.
    /// </summary>
    /// <param name="objectId">The GUID identifier of the entity to find.</param>
    /// <returns>A task representing the asynchronous operation, returning the entity if found; otherwise, <c>null</c>.</returns>
    Task<TObjectWithID?> GetByIdAsync(Guid objectId);

    /// <summary>
    /// Deletes an entity by its unique identifier asynchronously.
    /// </summary>
    /// <param name="objectId">The GUID identifier of the entity to delete.</param>
    /// <returns>A task representing the asynchronous operation, returning the deleted entity.</returns>
    Task<TObjectWithID> DeleteAsync(Guid objectId);
}
