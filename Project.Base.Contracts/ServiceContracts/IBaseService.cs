using Project.Base.Contracts.Models;
using Project.Base.Enumerators;

namespace Project.Base.Contracts.ServiceContracts
{
    /// <summary>
    /// Defines the contract for a generic business service providing CRUD operations and dynamic search capabilities.
    /// All methods return a <see cref="DtoOutput{TDto}"/> wrapper that includes success status, validation errors, and paginated results.
    /// </summary>
    /// <typeparam name="TDto">The DTO type used for all service operations. Must inherit from <see cref="DtoBase"/>.</typeparam>
    public interface IBaseService<TDto> where TDto : DtoBase
    {
        /// <summary>
        /// Retrieves a single record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the record to find.</param>
        /// <returns>A <see cref="DtoOutput{TDto}"/> containing the found record or an empty result set.</returns>
        Task<DtoOutput<TDto>> FindById(Guid id);

        /// <summary>
        /// Retrieves all records without pagination.
        /// </summary>
        /// <returns>A <see cref="DtoOutput{TDto}"/> containing all records.</returns>
        Task<DtoOutput<TDto>> FindAll();

        /// <summary>
        /// Retrieves a paginated and optionally filtered set of records using dynamic search across all string properties.
        /// </summary>
        /// <param name="pageIndex">The page number (zero-based).</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="order">The sort order (<see cref="EnumOrder.ASCENDING"/> or <see cref="EnumOrder.DESCENDING"/>).</param>
        /// <param name="searchTarget">Optional property name to restrict the search scope. Pass <c>null</c> to search all string properties.</param>
        /// <param name="searchTerm">The search term to filter results.</param>
        /// <returns>A <see cref="DtoOutput{TDto}"/> containing the paginated and filtered results.</returns>
        Task<DtoOutput<TDto>> Find(int pageIndex, int pageSize, EnumOrder order, string? searchTarget, string? searchTerm);

        /// <summary>
        /// Inserts a new record after validating the provided DTO.
        /// </summary>
        /// <param name="dto">The DTO representing the new record to insert.</param>
        /// <returns>A <see cref="DtoOutput{TDto}"/> indicating success or containing validation failures.</returns>
        Task<DtoOutput<TDto>> Insert(TDto dto);

        /// <summary>
        /// Updates an existing record identified within the DTO.
        /// </summary>
        /// <param name="dto">The DTO containing the updated data and the record identifier.</param>
        /// <returns>A <see cref="DtoOutput{TDto}"/> indicating success or containing validation failures.</returns>
        Task<DtoOutput<TDto>> Update(TDto dto);

        /// <summary>
        /// Deletes a record by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the record to delete.</param>
        /// <returns>A <see cref="DtoOutput{TDto}"/> indicating success or containing validation failures.</returns>
        Task<DtoOutput<TDto>> Delete(Guid id);
    }
}
