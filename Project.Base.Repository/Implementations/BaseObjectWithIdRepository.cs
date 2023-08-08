using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Project.Base.Contracts.Repositories;
using Project.Base.Domain.Object.Shared;

namespace Project.Base.Repository.Implementations
{
    public abstract class BaseObjectWithIdRepository<TObject> : GenericRepository<TObject>, IBaseObjectWithIdRepository<TObject>
        where TObject : BaseObjectWithId
    {
        protected BaseObjectWithIdRepository(IdentityDbContext context) : base(context) { }

        public TObject Delete(Guid objectId)
        {
            return Delete(GetById(objectId));
        }

        public TObject GetById(Guid objectId)
        {
            return (TObject)Persistence.Where(x => x.Id == objectId);
        }
    }
}
