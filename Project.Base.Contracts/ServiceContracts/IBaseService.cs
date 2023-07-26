using Project.Base.Contracts.Models;
using Project.Base.Enumerators;

namespace Project.Base.Contracts.ServiceContracts
{
    public interface IBaseService<TDto> where TDto : DtoBase
    {
        DtoOutput<TDto> FindById(Guid id);

        DtoOutput<TDto> FindAll();

        DtoOutput<TDto> Find(int pageIndex, int pageSize, EnumOrder order, string searchTerm);

        DtoOutput<TDto> Find(int pageIndex, int pageSize, EnumOrder order, string searchTarget, string searchTerm);

        DtoOutput<TDto> Insert(TDto dto);

        DtoOutput<TDto> Update(TDto dto);

        DtoOutput<TDto> Delete(Guid id);
    }
}
