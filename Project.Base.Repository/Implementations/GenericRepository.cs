using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Project.Base.Domain.Object.Shared;
using Project.Base.Domain.Repositories;
using Project.Base.Enumerators;

namespace Project.Base.Repository.Implementations
{
    /// <summary>
    /// Provides a generic Entity Framework Core implementation of <see cref="IGenericRepository{TObjeto}"/>.
    /// It handles standard CRUD operations (both synchronous and asynchronous) on entities
    /// inheriting from <see cref="BaseObjectWithId"/>, including pagination and dynamic
    /// text search using expression trees.
    /// </summary>
    /// <typeparam name="TObjeto">
    /// The entity type to persist. Must inherit from <see cref="BaseObjectWithId"/>.
    /// </typeparam>
    public abstract class GenericRepository<TObjeto> : IGenericRepository<TObjeto> where TObjeto : BaseObjectWithId
    {
        /// <summary>
        /// The <see cref="DbSet{TEntity}"/> representing the entity set for <typeparamref name="TObjeto"/>.
        /// Used to perform queries and changes against the database.
        /// </summary>
        protected readonly DbSet<TObjeto> Persistence;

        /// <summary>
        /// The <see cref="DbContext"/> used to track and persist changes to the database.
        /// </summary>
        protected readonly DbContext Context;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRepository{TObjeto}"/> class.
        /// </summary>
        /// <param name="context">The Entity Framework Core context injected via dependency injection.</param>
        protected GenericRepository(DbContext context)
        {
            Context = context;
            Persistence = Context.Set<TObjeto>();
        }

        // ── Sync (compatibilidade) ──

        /// <summary>
        /// Marks an entity for deletion and persists the change to the database.
        /// </summary>
        /// <param name="obj">The entity instance to delete.</param>
        /// <returns>The deleted entity.</returns>
        public TObjeto Delete(TObjeto obj)
        {
            EntityEntry<TObjeto> deleted = Persistence.Remove(obj);
            _ = Context.SaveChanges();

            return deleted.Entity;
        }

        /// <summary>
        /// Adds a new entity to the data source and persists the change.
        /// </summary>
        /// <param name="newObject">The entity instance to insert.</param>
        /// <returns>The newly added entity.</returns>
        public TObjeto Insert(TObjeto newObject)
        {
            EntityEntry<TObjeto> added = Persistence.Add(newObject);
            _ = Context.SaveChanges();

            return added.Entity;
        }

        /// <summary>
        /// Returns all entities without filtering.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{TObjeto}"/> of all entities in the data source.</returns>
        public IEnumerable<TObjeto> List()
        {
            return Persistence;
        }

        /// <summary>
        /// Returns entities matching the specified predicate.
        /// </summary>
        /// <param name="predicate">A lambda expression used to filter entities.</param>
        /// <returns>An <see cref="IEnumerable{TObjeto}"/> of entities matching the predicate.</returns>
        public IEnumerable<TObjeto> List(Expression<Func<TObjeto, bool>> predicate)
        {
            return Persistence.Where(predicate);
        }

        /// <summary>
        /// Returns a paginated and optionally filtered set of entities.
        /// When <see cref="PagedSearchParam.SearchTerm"/> is provided without a <see cref="PagedSearchParam.SearchTarget"/>,
        /// it performs a dynamic search across all string properties of the entity.
        /// When <see cref="PagedSearchParam.SearchTarget"/> is specified, it delegates to
        /// <see cref="ListWithSearchTermInner"/> for property-specific filtering.
        /// </summary>
        /// <param name="searchParams">
        /// A <see cref="PagedSearchParam"/> containing pagination, ordering, and search parameters.
        /// </param>
        /// <returns>
        /// A <see cref="PagedSearchReturn{TObjeto}"/> containing the paginated results and metadata.
        /// </returns>
        public PagedSearchReturn<TObjeto> List(PagedSearchParam searchParams)
        {
            if (string.IsNullOrEmpty(searchParams.SearchTerm))
            {
                IQueryable<TObjeto> persistence = Persistence;
                int itemsByPage = searchParams.Limit;
                EnumOrder order = !Enum.IsDefined(searchParams.Order) ? EnumOrder.ASCENDING : searchParams.Order;
                int page = searchParams.Page < 1 ? 1 : searchParams.Page;

                persistence = order == EnumOrder.ASCENDING ? (IQueryable<TObjeto>)persistence.OrderBy(x => x) : persistence.OrderByDescending(x => x);

                if (itemsByPage > 0)
                {
                    persistence = persistence.Skip((page - 1) * itemsByPage).Take(itemsByPage);
                }

                return new PagedSearchReturn<TObjeto>
                {
                    ActualPage = page,
                    Results = persistence,
                    ReturnedInActualPage = persistence.Count(),
                    Limit = searchParams.Limit,
                    TotalCount = Persistence.Count(),
                    PagesCount = (int)Math.Round(Persistence.Count() / (double)searchParams.Limit, MidpointRounding.ToPositiveInfinity),
                };
            }
            else
            {
                return !string.IsNullOrEmpty(searchParams.SearchTarget) ? ListWithSearchTermInner(searchParams) : ListWithSearchTerm(searchParams);
            }
        }

        /// <summary>
        /// Updates an existing entity in the data source and persists the change.
        /// Clears the change tracker before the update to avoid conflicts.
        /// </summary>
        /// <param name="updatedObject">The entity instance with updated values.</param>
        /// <returns>The updated entity.</returns>
        public TObjeto Update(TObjeto updatedObject)
        {
            Context.ChangeTracker.Clear();
            EntityEntry<TObjeto> obj = Persistence.Update(updatedObject);
            _ = Context.SaveChanges();

            return obj.Entity;
        }

        // ── Async ──

        /// <summary>
        /// Asynchronously adds a new entity to the data source and persists the change.
        /// </summary>
        /// <param name="newObject">The entity instance to insert.</param>
        /// <returns>A task representing the asynchronous operation, with the newly added entity.</returns>
        public async Task<TObjeto> InsertAsync(TObjeto newObject)
        {
            EntityEntry<TObjeto> added = Persistence.Add(newObject);
            await Context.SaveChangesAsync().ConfigureAwait(false);
            return added.Entity;
        }

        /// <summary>
        /// Asynchronously returns all entities without filtering.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation, with an <see cref="IEnumerable{TObjeto}"/> of all entities.
        /// </returns>
        public async Task<IEnumerable<TObjeto>> ListAsync()
        {
            return await Persistence.ToListAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously returns entities matching the specified predicate.
        /// </summary>
        /// <param name="predicate">A lambda expression used to filter entities.</param>
        /// <returns>
        /// A task representing the asynchronous operation, with an <see cref="IEnumerable{TObjeto}"/> of matching entities.
        /// </returns>
        public async Task<IEnumerable<TObjeto>> ListAsync(Expression<Func<TObjeto, bool>> predicate)
        {
            return await Persistence.Where(predicate).ToListAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously updates an existing entity in the data source and persists the change.
        /// Clears the change tracker before the update to avoid conflicts.
        /// </summary>
        /// <param name="updatedObject">The entity instance with updated values.</param>
        /// <returns>
        /// A task representing the asynchronous operation, with the updated entity.
        /// </returns>
        public async Task<TObjeto> UpdateAsync(TObjeto updatedObject)
        {
            Context.ChangeTracker.Clear();
            EntityEntry<TObjeto> obj = Persistence.Update(updatedObject);
            await Context.SaveChangesAsync().ConfigureAwait(false);
            return obj.Entity;
        }

        /// <summary>
        /// Asynchronously marks an entity for deletion and persists the change.
        /// </summary>
        /// <param name="obj">The entity instance to delete.</param>
        /// <returns>
        /// A task representing the asynchronous operation, with the deleted entity.
        /// </returns>
        public async Task<TObjeto> DeleteAsync(TObjeto obj)
        {
            EntityEntry<TObjeto> deleted = Persistence.Remove(obj);
            await Context.SaveChangesAsync().ConfigureAwait(false);
            return deleted.Entity;
        }

        // ── US-16: ListWithSearchTerm com implementação base genérica ──

        /// <summary>
        /// Performs a paginated search with a text term. When <see cref="PagedSearchParam.SearchTarget"/>
        /// is null or empty, it dynamically searches across all <c>string</c> properties of the entity
        /// using expression trees. When <see cref="PagedSearchParam.SearchTarget"/> is provided,
        /// it delegates to <see cref="ListWithSearchTermInner"/> for property-specific filtering.
        /// Subclasses may override this method to customize the search logic.
        /// </summary>
        /// <param name="searchParams">
        /// A <see cref="PagedSearchParam"/> containing pagination, ordering, and search parameters.
        /// </param>
        /// <returns>
        /// A <see cref="PagedSearchReturn{TObjeto}"/> containing the paginated search results and metadata.
        /// </returns>
        protected virtual PagedSearchReturn<TObjeto> ListWithSearchTerm(PagedSearchParam searchParams)
        {
            // Se SearchTarget foi especificado, delega para a implementação interna existente
            if (!string.IsNullOrEmpty(searchParams.SearchTarget))
                return ListWithSearchTermInner(searchParams);

            // Busca dinâmica: encontra todas as propriedades string da entidade
            var stringProperties = typeof(TObjeto)
                .GetProperties()
                .Where(p => p.PropertyType == typeof(string))
                .ToList();

            IQueryable<TObjeto> query;

            if (stringProperties.Any())
            {
                // Constrói Expression<Func<TObjeto, bool>> dinamicamente:
                // (prop1.Contains(term) || prop2.Contains(term) || ...)
                var param = Expression.Parameter(typeof(TObjeto), "x");
                Expression? combined = null;

                foreach (var prop in stringProperties)
                {
                    var propAccess = Expression.Property(param, prop.Name);
                    var method = typeof(string).GetMethod("Contains", [typeof(string), typeof(StringComparison)])!;
                    var searchTermConst = Expression.Constant(searchParams.SearchTerm!, typeof(string));
                    var comparisonConst = Expression.Constant(StringComparison.OrdinalIgnoreCase, typeof(StringComparison));
                    var containsCall = Expression.Call(propAccess, method, searchTermConst, comparisonConst);

                    combined = combined == null ? containsCall : Expression.OrElse(combined, containsCall);
                }

                var lambda = Expression.Lambda<Func<TObjeto, bool>>(combined!, param);
                query = Persistence.Where(lambda);
            }
            else
            {
                // Sem propriedades string → retorna todos (sem filtro de texto)
                query = Persistence.AsQueryable();
            }

            // Aplica ordenação e paginação
            query = searchParams.Order == EnumOrder.ASCENDING
                ? query.OrderBy(x => x).Skip((searchParams.Page - 1) * searchParams.Limit).Take(searchParams.Limit)
                : query.OrderByDescending(x => x).Skip((searchParams.Page - 1) * searchParams.Limit).Take(searchParams.Limit);

            var results = query.ToList();

            return new PagedSearchReturn<TObjeto>
            {
                ActualPage = searchParams.Page,
                Results = results,
                Limit = searchParams.Limit,
                ReturnedInActualPage = results.Count,
                TotalCount = Persistence.Count(),
                PagesCount = (int)Math.Round(Persistence.Count() / (double)searchParams.Limit, MidpointRounding.ToPositiveInfinity),
            };
        }

        /// <summary>
        /// Performs a paginated search filtered by a specific target property name.
        /// Uses reflection to dynamically access the property and search within it.
        /// </summary>
        /// <param name="searchParams">
        /// A <see cref="PagedSearchParam"/> containing pagination, ordering, search target, and search term.
        /// </param>
        /// <returns>
        /// A <see cref="PagedSearchReturn{TObjeto}"/> containing the paginated search results and metadata.
        /// </returns>
        private PagedSearchReturn<TObjeto> ListWithSearchTermInner(PagedSearchParam searchParams)
        {
            Func<TObjeto, bool> expression = GetFilter(searchParams.SearchTarget!, searchParams.SearchTerm!);

            IEnumerable<TObjeto> query = Persistence.Where(expression);

            query = (searchParams.Order == EnumOrder.ASCENDING
                    ? query.OrderBy(x => x).Skip((searchParams.Page - 1) * searchParams.Limit).Take(searchParams.Limit)
                    : query.OrderByDescending(x => x).Skip((searchParams.Page - 1) * searchParams.Limit).Take(searchParams.Limit)).ToList();

            return new PagedSearchReturn<TObjeto>
            {
                ActualPage = searchParams.Page,
                Results = query,
                Limit = searchParams.Limit,
                ReturnedInActualPage = query.Count(),
                TotalCount = Persistence.Count(),
                PagesCount = (int)Math.Round(Persistence.Count() / (double)searchParams.Limit, MidpointRounding.ToPositiveInfinity),
            };
        }

        /// <summary>
        /// Builds a filter predicate that checks whether the value of a specific property
        /// contains the given search term (case-insensitive substring match via reflection).
        /// </summary>
        /// <param name="searchTarget">
        /// The name of the property to search within (e.g., "Name", "Description").
        /// </param>
        /// <param name="searchTerm">The text to search for within the property value.</param>
        /// <returns>
        /// A <see cref="Func{TObjeto,bool}"/> predicate that returns true when the property
        /// value is null or contains the search term.
        /// </returns>
        protected Func<TObjeto, bool> GetFilter(string searchTarget, string searchTerm)
        {
            return (Objeto) =>
            {
                System.Reflection.PropertyInfo prop = typeof(TObjeto).GetProperties().First(prop => prop.Name.ToUpper() == searchTarget.ToUpper());
                object? value = prop.GetValue(Objeto);
                return value is null || value.ToString()!.Contains(searchTerm);
            };
        }
    }
}
