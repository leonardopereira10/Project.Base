using System.Linq.Expressions;
using Project.Base.Domain.Object.Shared;

namespace Project.Base.Domain.Repositories;

/// <summary>
/// Defines the contract for a generic repository providing CRUD operations
/// over entities of type <typeparamref name="TObject"/> with both synchronous
/// and asynchronous method variants.
/// </summary>
/// <typeparam name="TObject">The entity type. Must inherit from <see cref="BaseObjectWithId"/>.</typeparam>
public interface IGenericRepository<TObject> where TObject : BaseObjectWithId
{
    // ── Sync (compatibilidade) ──

    /// <summary>
    /// Inserts a new entity into the data store.
    /// </summary>
    /// <param name="newObject">The entity to insert.</param>
    /// <returns>The inserted entity, typically with generated identifiers populated.</returns>
    TObject Insert(TObject newObject);

    /// <summary>
    /// Retrieves all entities from the data store without filtering.
    /// </summary>
    /// <returns>A <see cref="IEnumerable{TObject}"/> containing all entities.</returns>
    IEnumerable<TObject> List();

    /// <summary>
    /// Retrieves entities matching the specified predicate.
    /// </summary>
    /// <param name="predicate">An expression to filter entities.</param>
    /// <returns>A <see cref="IEnumerable{TObject}"/> of entities matching the predicate.</returns>
    IEnumerable<TObject> List(Expression<Func<TObject, bool>> predicate);

    /// <summary>
    /// Retrieves a paginated subset of entities using the provided search parameters.
    /// </summary>
    /// <param name="searchParams">The pagination and sorting parameters.</param>
    /// <returns>A <see cref="PagedSearchReturn{TObject}"/> containing the paginated results.</returns>
    PagedSearchReturn<TObject> List(PagedSearchParam searchParams);

    /// <summary>
    /// Updates an existing entity in the data store.
    /// </summary>
    /// <param name="updatedObject">The entity with updated values.</param>
    /// <returns>The updated entity.</returns>
    TObject Update(TObject updatedObject);

    /// <summary>
    /// Deletes an entity from the data store.
    /// </summary>
    /// <param name="obj">The entity to delete.</param>
    /// <returns>The deleted entity.</returns>
    TObject Delete(TObject obj);

    // ── Async ──

    /// <summary>
    /// Asynchronously inserts a new entity into the data store.
    /// </summary>
    /// <param name="newObject">The entity to insert.</param>
    /// <returns>A task representing the asynchronous operation, returning the inserted entity.</returns>
    Task<TObject> InsertAsync(TObject newObject);

    /// <summary>
    /// Asynchronously retrieves all entities from the data store without filtering.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, returning all entities.</returns>
    Task<IEnumerable<TObject>> ListAsync();

    /// <summary>
    /// Asynchronously retrieves entities matching the specified predicate.
    /// </summary>
    /// <param name="predicate">An expression to filter entities.</param>
    /// <returns>A task representing the asynchronous operation, returning matching entities.</returns>
    Task<IEnumerable<TObject>> ListAsync(Expression<Func<TObject, bool>> predicate);

    /// <summary>
    /// Asynchronously updates an existing entity in the data store.
    /// </summary>
    /// <param name="updatedObject">The entity with updated values.</param>
    /// <returns>A task representing the asynchronous operation, returning the updated entity.</returns>
    Task<TObject> UpdateAsync(TObject updatedObject);

    /// <summary>
    /// Asynchronously deletes an entity from the data store.
    /// </summary>
    /// <param name="obj">The entity to delete.</param>
    /// <returns>A task representing the asynchronous operation, returning the deleted entity.</returns>
    Task<TObject> DeleteAsync(TObject obj);
}
