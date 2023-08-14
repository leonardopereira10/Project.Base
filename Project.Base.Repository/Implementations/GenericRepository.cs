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

        protected abstract PagedSearchReturn<TObjeto> ListWithSearchTerm(PagedSearchParam searchParams);

        public TObjeto Update(TObjeto updatedObject)
        {
            Context.ChangeTracker.Clear();
            EntityEntry<TObjeto> obj = Persistence.Update(updatedObject);
            _ = Context.SaveChanges();

            return obj.Entity;
        }

        private PagedSearchReturn<TObjeto> ListWithSearchTermInner(PagedSearchParam searchParams)
        {
            Func<TObjeto, bool> expression = GetFilter(searchParams.SearchTarget, searchParams.SearchTerm);

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
                object value = prop.GetValue(Objeto);
                return value is null || value.ToString().Contains(searchTerm);
            };
        }
    }
}
