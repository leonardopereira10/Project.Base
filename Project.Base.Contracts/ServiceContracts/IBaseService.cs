using Project.Base.Contracts.Models;
using Project.Base.Enumerators;

namespace Project.Base.Contracts.ServiceContracts
{
    public interface IBaseService<TDto> where TDto : DtoBase
    {
        Task<DtoOutput<TDto>> FindById(Guid id);

        Task<DtoOutput<TDto>> FindAll();

        Task<DtoOutput<TDto>> Find(int pageIndex, int pageSize, EnumOrder order, string searchTarget, string searchTerm);

        Task<DtoOutput<TDto>> Insert(TDto dto);

        Task<DtoOutput<TDto>> Update(TDto dto);

        Task<DtoOutput<TDto>> Delete(Guid id);
    }
}
