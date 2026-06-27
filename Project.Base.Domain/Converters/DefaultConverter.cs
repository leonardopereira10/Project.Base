using Mapster;
using Project.Base.Contracts.Models;
using Project.Base.Domain.Object.Shared;
using Project.Base.Domain.Repositories;

namespace Project.Base.Domain.Converters
{
    /// <summary>
    /// Abstract base converter that implements <see cref="IDefaultConverter{TObj, TDto}"/>
    /// using Mapster's <see cref="TypeAdapter"/> for automatic object-to-DTO and DTO-to-object mapping.
    /// Provides methods for single object conversion, collection conversion, and wrapping results
    /// in <see cref="DtoOutput{TDto}"/> with pagination metadata.
    /// </summary>
    /// <typeparam name="TObj">The domain entity type. Must inherit from <see cref="BaseObjectWithId"/>.</typeparam>
    /// <typeparam name="TDto">The DTO type. Must inherit from <see cref="DtoBase"/>.</typeparam>
    public abstract class DefaultConverter<TObj, TDto> : IDefaultConverter<TObj, TDto>
        where TObj : BaseObjectWithId
        where TDto : DtoBase
    {
        /// <summary>
        /// Converts a domain entity to its corresponding DTO using Mapster.
        /// </summary>
        /// <param name="obj">The entity instance to convert.</param>
        /// <returns>The converted DTO.</returns>
        public virtual TDto Convert(TObj obj)
        {
            return Copy<TObj, TDto>(obj);
        }

        /// <summary>
        /// Converts a DTO to its corresponding domain entity using Mapster.
        /// </summary>
        /// <param name="dtos">The DTO instance to convert.</param>
        /// <returns>The converted entity.</returns>
        public virtual TObj Convert(TDto dtos)
        {
            return Copy<TDto, TObj>(dtos);
        }

        /// <summary>
        /// Converts a collection of domain entities to their corresponding DTOs.
        /// </summary>
        /// <param name="objects">The collection of entities to convert.</param>
        /// <returns>The converted collection of DTOs.</returns>
        public virtual IEnumerable<TDto> Convert(IEnumerable<TObj> objects)
        {
            return objects.Select(Convert);
        }

        /// <summary>
        /// Converts a collection of DTOs to their corresponding domain entities.
        /// </summary>
        /// <param name="dtos">The collection of DTOs to convert.</param>
        /// <returns>The converted collection of entities.</returns>
        public virtual IEnumerable<TObj> Convert(IEnumerable<TDto> dtos)
        {
            return dtos.Select(Convert);
        }

        /// <summary>
        /// Creates a <see cref="DtoOutput{TDto}"/> wrapper around a single DTO, optionally including validation failures.
        /// When <paramref name="fails"/> is provided, <see cref="DtoOutput{TDto}.Success"/> reflects whether any impeditive failures exist.
        /// </summary>
        /// <param name="dto">The DTO to wrap.</param>
        /// <param name="fails">Optional collection of validation failures.</param>
        /// <returns>A populated <see cref="DtoOutput{TDto}"/> instance.</returns>
        public virtual DtoOutput<TDto> GetDtoOutput(TDto dto, IEnumerable<ValidationFail>? fails = null)
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

        /// <summary>
        /// Converts a collection of DTOs into a <see cref="DtoOutput{TDto}"/> with default pagination values.
        /// </summary>
        /// <param name="dtos">The collection of DTOs to wrap.</param>
        /// <returns>A <see cref="DtoOutput{TDto}"/> containing the DTOs and total count.</returns>
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

        /// <summary>
        /// Converts a single domain entity to a DTO and wraps it in a <see cref="DtoOutput{TDto}"/>.
        /// </summary>
        /// <param name="obj">The entity to convert and wrap.</param>
        /// <returns>A <see cref="DtoOutput{TDto}"/> containing the converted DTO.</returns>
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

        /// <summary>
        /// Converts a collection of domain entities to DTOs and wraps them in a <see cref="DtoOutput{TDto}"/>.
        /// </summary>
        /// <param name="objects">The collection of entities to convert and wrap.</param>
        /// <returns>A <see cref="DtoOutput{TDto}"/> containing the converted DTOs and total count.</returns>
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

        /// <summary>
        /// Converts a paginated search result (<see cref="PagedSearchReturn{TObj}"/>) to a <see cref="DtoOutput{TDto}"/>
        /// preserving pagination metadata (page, pageSize, totalCount).
        /// </summary>
        /// <param name="pagedSearchReturn">The paginated entity result to convert.</param>
        /// <returns>A <see cref="DtoOutput{TDto}"/> with DTOs and pagination information.</returns>
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

        /// <summary>
        /// Performs a type-safe conversion between <typeparamref name="TInput"/> and <typeparamref name="TOutput"/> using Mapster.
        /// </summary>
        /// <typeparam name="TInput">The source type.</typeparam>
        /// <typeparam name="TOutput">The destination type.</typeparam>
        /// <param name="input">The input object to convert.</param>
        /// <returns>The converted object of type <typeparamref name="TOutput"/>.</returns>
        protected static TOutput Copy<TInput, TOutput>(TInput input)
        {
            return TypeAdapter.Adapt<TInput, TOutput>(input);
        }
    }
}
