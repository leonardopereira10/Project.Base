using Project.Base.Contracts.Models;

namespace Project.Base.Domain.Object.Shared
{
    public interface IDefaultConverter<TObj, TDto>
        where TDto : DtoBase
        where TObj : BaseObjectWithId
    {
        TDto Convert(TObj obj);
        TObj Convert(TDto dtos);
        IEnumerable<TDto> Convert(IEnumerable<TObj> objects);
        IEnumerable<TObj> Convert(IEnumerable<TDto> dtos);
        DtoOutput<TDto> GetDtoOutput(TDto dto, IEnumerable<ValidationFail> fails = null);
        DtoOutput<TDto> ConvertToDtoOutput(IEnumerable<TDto> dtos);
        DtoOutput<TDto> ConvertToDtoOutput(TObj obj);
        DtoOutput<TDto> ConvertToDtoOutput(IEnumerable<TObj> objects);
        DtoOutput<TDto> ConvertToDtoOutput(PagedSearchReturn<TObj> pagedSearchReturn);
    }
}
