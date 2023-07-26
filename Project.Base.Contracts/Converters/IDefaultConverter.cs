using Project.Base.Contracts.Models;
using Project.Base.Domain.Object.Shared;

namespace Project.Base.Contracts.Converters
{
    internal interface IDefaultConverter<TObject, TDto>
        where TObject : BaseObjectWithId
        where TDto : DtoBase
    {
        TDto Convert(TObject obj);

        TObject Convert(TDto dtos);

        IList<TDto> Convert(IList<TObject> objects);

        IList<TObject> Convert(IList<TDto> dtos);

        DtoOutput<TDto> ConvertToDtoOutput(TDto dto);

        DtoOutput<TDto> ConvertToDtoOutput(IList<TDto> dtos);

        DtoOutput<TDto> ConvertToDtoOutput(TObject obj);

        DtoOutput<TDto> ConvertToDtoOutput(IList<TObject> objects);
    }
}
