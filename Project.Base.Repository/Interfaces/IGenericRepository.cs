using Project.Base.Domain.Object;

namespace Project.Base.Repository.Interfaces;

public interface IGenericRepository<TObject> where TObject : BaseObjectWithId
{
}
