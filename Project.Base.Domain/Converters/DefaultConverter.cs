using System.Text.Json;
using Project.Base.Contracts.Models;
using Project.Base.Domain.Object.Shared;

namespace Project.Base.Domain.Converters
{
    public abstract class DefaultConverter<TObj, TDto> : IDefaultConverter<TObj, TDto>
        where TObj : BaseObjectWithId
        where TDto : DtoBase
    {
        public virtual TDto Convert(TObj obj)
        {
            return Copy<TObj, TDto>(obj);
        }

        public virtual TObj Convert(TDto dtos)
        {
            return Copy<TDto, TObj>(dtos);
        }

        public virtual IEnumerable<TDto> Convert(IEnumerable<TObj> objects)
        {
            return objects.Select(Convert);
        }

        public virtual IEnumerable<TObj> Convert(IEnumerable<TDto> dtos)
        {
            return dtos.Select(Convert);
        }

        public virtual DtoOutput<TDto> GetDtoOutput(TDto dto, IEnumerable<ValidationFail> fails = null)
        {
            fails ??= Array.Empty<ValidationFail>();

            return new DtoOutput<TDto>
            {
                Page = 0,
                PageSize = 0,
                Success = !fails.Any(x => x.IsImpeditive),
                TotalCount = 1,
                ValidationFails = fails,
                ResultSet = new[] { dto }
            };
        }

        public virtual DtoOutput<TDto> ConvertToDtoOutput(IEnumerable<TDto> dtos)
        {
            return new DtoOutput<TDto>
            {
                Page = 0,
                PageSize = 0,
                Success = true,
                TotalCount = dtos.Count(),
                ResultSet = dtos
            };
        }

        public virtual DtoOutput<TDto> ConvertToDtoOutput(TObj obj)
        {
            TDto dto = Convert(obj);

            return new DtoOutput<TDto>
            {
                Page = 0,
                PageSize = 0,
                Success = true,
                TotalCount = 1,
                ResultSet = new[] { dto }
            };
        }

        public virtual DtoOutput<TDto> ConvertToDtoOutput(IEnumerable<TObj> objects)
        {
            IEnumerable<TDto> dtos = Convert(objects);

            return new DtoOutput<TDto>
            {
                Page = 0,
                PageSize = 0,
                Success = true,
                TotalCount = dtos.Count(),
                ResultSet = dtos
            };
        }

        public virtual DtoOutput<TDto> ConvertToDtoOutput(PagedSearchReturn<TObj> pagedSearchReturn)
        {
            IEnumerable<TDto> dtos = Convert(pagedSearchReturn.Results);
            return new DtoOutput<TDto>
            {
                Page = pagedSearchReturn.ActualPage,
                PageSize = pagedSearchReturn.ReturnedInActualPage,
                Success = true,
                TotalCount = pagedSearchReturn.TotalCount,
                ResultSet = dtos
            };
        }

        protected static TOutput Copy<TInput, TOutput>(TInput input)
        {
            JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web);
            string outputAsText = JsonSerializer.Serialize(input, jsonOptions);
            return JsonSerializer.Deserialize<TOutput>(outputAsText, jsonOptions);
        }
    }
}
