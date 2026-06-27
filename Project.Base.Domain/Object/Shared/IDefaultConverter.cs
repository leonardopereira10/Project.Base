using Project.Base.Contracts.Models;
using Project.Base.Domain.Repositories;

namespace Project.Base.Domain.Object.Shared
{
    /// <summary>
    /// Defines the contract for a generic converter between domain entities (<typeparamref name="TObj"/>) and DTOs (<typeparamref name="TDto"/>).
    /// Provides methods for single object conversion, collection conversion, and DTO output wrapping with pagination support.
    /// </summary>
    /// <typeparam name="TObj">The domain entity type. Must inherit from <see cref="BaseObjectWithId"/>.</typeparam>
    /// <typeparam name="TDto">The DTO type. Must inherit from <see cref="DtoBase"/>.</typeparam>
    public interface IDefaultConverter<TObj, TDto>
        where TDto : DtoBase
        where TObj : BaseObjectWithId
    {
        /// <summary>
        /// Converts a domain entity to its corresponding DTO.
        /// </summary>
        /// <param name="obj">The entity instance to convert.</param>
        /// <returns>The converted DTO.</returns>
        TDto Convert(TObj obj);

        /// <summary>
        /// Converts a DTO to its corresponding domain entity.
        /// </summary>
        /// <param name="dtos">The DTO instance to convert.</param>
        /// <returns>The converted entity.</returns>
        TObj Convert(TDto dtos);

        /// <summary>
        /// Converts a collection of domain entities to their corresponding DTOs.
        /// </summary>
        /// <param name="objects">The collection of entities to convert.</param>
        /// <returns>The converted collection of DTOs.</returns>
        IEnumerable<TDto> Convert(IEnumerable<TObj> objects);

        /// <summary>
        /// Converts a collection of DTOs to their corresponding domain entities.
        /// </summary>
        /// <param name="dtos">The collection of DTOs to convert.</param>
        /// <returns>The converted collection of entities.</returns>
        IEnumerable<TObj> Convert(IEnumerable<TDto> dtos);

        /// <summary>
        /// Creates a <see cref="DtoOutput{TDto}"/> wrapper around a single DTO, optionally including validation failures.
        /// </summary>
        /// <param name="dto">The DTO to wrap.</param>
        /// <param name="fails">Optional collection of validation failures. When provided, <see cref="DtoOutput{TDto}.Success"/> reflects whether any impeditive failures exist.</param>
        /// <returns>A populated <see cref="DtoOutput{TDto}"/> instance.</returns>
        DtoOutput<TDto> GetDtoOutput(TDto dto, IEnumerable<ValidationFail>? fails = null);

        /// <summary>
        /// Converts a collection of DTOs into a paginated <see cref="DtoOutput{TDto}"/> response wrapper.
        /// </summary>
        /// <param name="dtos">The collection of DTOs to wrap.</param>
        /// <returns>A <see cref="DtoOutput{TDto}"/> containing the DTOs, total count, and default pagination values.</returns>
        DtoOutput<TDto> ConvertToDtoOutput(IEnumerable<TDto> dtos);

        /// <summary>
        /// Converts a single domain entity to a DTO and wraps it in a <see cref="DtoOutput{TDto}"/>.
        /// </summary>
        /// <param name="obj">The entity to convert and wrap.</param>
        /// <returns>A <see cref="DtoOutput{TDto}"/> containing the converted DTO.</returns>
        DtoOutput<TDto> ConvertToDtoOutput(TObj obj);

        /// <summary>
        /// Converts a collection of domain entities to DTOs and wraps them in a <see cref="DtoOutput{TDto}"/>.
        /// </summary>
        /// <param name="objects">The collection of entities to convert and wrap.</param>
        /// <returns>A <see cref="DtoOutput{TDto}"/> containing the converted DTOs.</returns>
        DtoOutput<TDto> ConvertToDtoOutput(IEnumerable<TObj> objects);

        /// <summary>
        /// Converts a paginated search result (<see cref="PagedSearchReturn{TObj}"/>) to a <see cref="DtoOutput{TDto}"/> preserving pagination metadata.
        /// </summary>
        /// <param name="pagedSearchReturn">The paginated entity result to convert.</param>
        /// <returns>A <see cref="DtoOutput{TDto}"/> with DTOs and pagination information (page, pageSize, totalCount).</returns>
        DtoOutput<TDto> ConvertToDtoOutput(PagedSearchReturn<TObj> pagedSearchReturn);
    }
}
