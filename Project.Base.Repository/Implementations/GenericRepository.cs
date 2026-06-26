using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Project.Base.Domain.Object.Shared;
using Project.Base.Domain.Repositories;
using Project.Base.Enumerators;

namespace Project.Base.Repository.Implementations
{
    public abstract class GenericRepository<TObjeto> : IGenericRepository<TObjeto> where TObjeto : BaseObjectWithId
    {
        protected readonly DbSet<TObjeto> Persistence;
        protected readonly DbContext Context;

        protected GenericRepository(DbContext context)
        {
            Context = context;
            Persistence = Context.Set<TObjeto>();
        }

        // ── Sync (compatibilidade) ──
        public TObjeto Delete(TObjeto obj)
        {
            EntityEntry<TObjeto> deleted = Persistence.Remove(obj);
            _ = Context.SaveChanges();

            return deleted.Entity;
        }

        public TObjeto Insert(TObjeto newObject)
        {
            EntityEntry<TObjeto> added = Persistence.Add(newObject);
            _ = Context.SaveChanges();

            return added.Entity;
        }

        public IEnumerable<TObjeto> List()
        {
            return Persistence;
        }

        public IEnumerable<TObjeto> List(Expression<Func<TObjeto, bool>> predicate)
        {
            return Persistence.Where(predicate);
        }

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

        public TObjeto Update(TObjeto updatedObject)
        {
            Context.ChangeTracker.Clear();
            EntityEntry<TObjeto> obj = Persistence.Update(updatedObject);
            _ = Context.SaveChanges();

            return obj.Entity;
        }

        // ── Async ──
        public async Task<TObjeto> InsertAsync(TObjeto newObject)
        {
            EntityEntry<TObjeto> added = Persistence.Add(newObject);
            await Context.SaveChangesAsync().ConfigureAwait(false);
            return added.Entity;
        }

        public async Task<IEnumerable<TObjeto>> ListAsync()
        {
            return await Persistence.ToListAsync().ConfigureAwait(false);
        }

        public async Task<IEnumerable<TObjeto>> ListAsync(Expression<Func<TObjeto, bool>> predicate)
        {
            return await Persistence.Where(predicate).ToListAsync().ConfigureAwait(false);
        }

        public async Task<TObjeto> UpdateAsync(TObjeto updatedObject)
        {
            Context.ChangeTracker.Clear();
            EntityEntry<TObjeto> obj = Persistence.Update(updatedObject);
            await Context.SaveChangesAsync().ConfigureAwait(false);
            return obj.Entity;
        }

        public async Task<TObjeto> DeleteAsync(TObjeto obj)
        {
            EntityEntry<TObjeto> deleted = Persistence.Remove(obj);
            await Context.SaveChangesAsync().ConfigureAwait(false);
            return deleted.Entity;
        }

        // ── US-16: ListWithSearchTerm com implementação base genérica ──
        /// <summary>
        /// Busca paginada com termo de pesquisa. Quando <see cref="PagedSearchParam.SearchTarget"/>
        /// é nulo ou vazio, busca dinamicamente em todas as propriedades <c>string</c> da entidade.
        /// Quando <see cref="PagedSearchParam.SearchTarget"/> é informado, delega para
        /// <see cref="ListWithSearchTermInner"/> (comportamento existente com reflection no campo específico).
        /// Subclasses podem sobrescrever para lógica personalizada.
        /// </summary>
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
                    var method = typeof(string).GetMethod("Contains", [typeof(string)])!;
                    var searchTermConst = Expression.Constant(searchParams.SearchTerm!, typeof(string));
                    var containsCall = Expression.Call(propAccess, method, searchTermConst);

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
