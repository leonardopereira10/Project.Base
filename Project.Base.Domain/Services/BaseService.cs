using Project.Base.Contracts.Models;
using Project.Base.Contracts.ServiceContracts;
using Project.Base.Domain.Object.Shared;
using Project.Base.Domain.Repositories;
using Project.Base.Domain.Validators;
using Project.Base.Enumerators;

namespace Project.Base.Domain.Services
{
    /// <summary>
    /// Serves as the generic base class for all business services in the application.
    /// It implements <see cref="IBaseService{TDto}"/> and provides the standard CRUD
    /// workflow: validate → convert → persist → return a paginated DTO output.
    /// Subclasses must supply a <see cref="Validator"/> and a <see cref="Converter"/>
    /// to operate on concrete entity and DTO types.
    /// </summary>
    /// <typeparam name="TObject">
    /// The entity type to operate on. Must inherit from <see cref="BaseObjectWithId"/>.
    /// </typeparam>
    /// <typeparam name="TDto">
    /// The DTO type used for input and output. Must inherit from <see cref="DtoBase"/>.
    /// </typeparam>
    public abstract class BaseService<TObject, TDto> : IBaseService<TDto>
        where TDto : DtoBase
        where TObject : BaseObjectWithId
    {
        /// <summary>
        /// The underlying repository used for data persistence operations.
        /// </summary>
        protected readonly IBaseObjectWithIdRepository<TObject> _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseService{TObject,TDto}"/> class, the concrete
        /// class must have the constructor using the concrete repository and he must implement 
        /// <see cref="IBaseObjectWithIdRepository<TObject>"/>.
        /// </summary>
        /// <remarks>
        /// To use specific methods of repository you must delcare a method with CAST of _repository
        /// </remarks>
        /// <param name="repository">The repository instance injected via dependency injection.</param>
        protected BaseService(IBaseObjectWithIdRepository<TObject> repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Deletes an entity by its identifier and returns the deleted entity as a DTO output.
        /// Applies delete-specific validations before performing the operation.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to delete.</param>
        /// <returns>A <see cref="DtoOutput{TDto}"/> containing the deleted entity's DTO representation.</returns>
        public async Task<DtoOutput<TDto>> Delete(Guid id)
        {
            Validator().AssignDeleteValidations();
            var obj = await _repository.DeleteAsync(id).ConfigureAwait(false);
            return Converter().ConvertToDtoOutput(obj);
        }

        /// <summary>
        /// Finds entities with full filtering options, including a specific search target property.
        /// </summary>
        /// <param name="pageIndex">The page number to retrieve (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="order">The sort order direction (<see cref="EnumOrder.ASCENDING"/> or <see cref="EnumOrder.DESCENDING"/>).</param>
        /// <param name="searchTarget">
        /// The name of a specific property to search within. When null or empty, the search
        /// is performed across all string properties of the entity.
        /// </param>
        /// <param name="searchTerm">The text to search for within the specified target property.</param>
        /// <returns>
        /// A <see cref="DtoOutput{TDto}"/> containing the paginated and filtered entity DTOs.
        /// </returns>
        public async Task<DtoOutput<TDto>> Find(int pageIndex, int pageSize, EnumOrder order, string? searchTarget, string? searchTerm)
        {
            PagedSearchReturn<TObject> paged = _repository.List(new PagedSearchParam
            {
                Page = pageIndex,
                Limit = pageSize,
                Order = order,
                SearchTerm = searchTerm,
                SearchTarget = searchTarget
            });
            return GetPagedSearchOutput(paged);
        }

        /// <summary>
        /// Retrieves all entities without any filtering or pagination.
        /// </summary>
        /// <returns>
        /// A <see cref="DtoOutput{TDto}"/> containing all entity DTOs in the data source.
        /// </returns>
        public async Task<DtoOutput<TDto>> FindAll()
        {
            var results = await _repository.ListAsync().ConfigureAwait(false);
            return Converter().ConvertToDtoOutput(results);
        }

        /// <summary>
        /// Retrieves an entity by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to retrieve.</param>
        /// <returns>
        /// A <see cref="DtoOutput{TDto}"/> containing the entity DTO matching the given identifier.
        /// </returns>
        public async Task<DtoOutput<TDto>> FindById(Guid id)
        {
            IEnumerable<TObject> result = await _repository.ListAsync(x => x.Id == id).ConfigureAwait(false);
            return Converter().ConvertToDtoOutput(result);
        }

        /// <summary>
        /// Creates a new entity from the provided DTO, validates it using insert-specific rules,
        /// persists it to the data source, and returns the created entity as a DTO output.
        /// </summary>
        /// <param name="dto">The DTO containing the data for the new entity.</param>
        /// <returns>
        /// A <see cref="DtoOutput{TDto}"/> containing the created entity's DTO, or validation errors if any.
        /// </returns>
        public async Task<DtoOutput<TDto>> Insert(TDto dto)
        {
            TObject obj = Converter().Convert(dto);
            Validator().AssignInsertValidations();
            IEnumerable<ValidationFail> fails = Validator().GetValidationOutput(Validator().Validate(obj));

            if (fails.Any(x => x.IsImpeditive))
            {
                return Converter().GetDtoOutput(dto, fails);
            }

            TObject inserted = await _repository.InsertAsync(obj).ConfigureAwait(false);
            return Converter().ConvertToDtoOutput(inserted);
        }

        /// <summary>
        /// Updates an existing entity from the provided DTO, validates it using update-specific rules,
        /// persists the changes to the data source, and returns the updated entity as a DTO output.
        /// </summary>
        /// <param name="dto">The DTO containing the updated data for the entity.</param>
        /// <returns>
        /// A <see cref="DtoOutput{TDto}"/> containing the updated entity's DTO, or validation errors if any.
        /// </returns>
        public async Task<DtoOutput<TDto>> Update(TDto dto)
        {
            TObject obj = Converter().Convert(dto);
            Validator().AssignUpdateValidations();
            IEnumerable<ValidationFail> fails = Validator().GetValidationOutput(Validator().Validate(obj));

            if (fails.Any(x => x.IsImpeditive))
            {
                return Converter().GetDtoOutput(dto, fails);
            }

            TObject updated = await _repository.UpdateAsync(obj).ConfigureAwait(false);
            return Converter().ConvertToDtoOutput(updated);
        }

        /// <summary>
        /// Finds entities with pagination, ordering, and a search term applied across all string properties.
        /// This is a convenience overload that delegates to <see cref="Find(int,int,EnumOrder,string?,string?)"/>
        /// with <paramref name="searchTarget"/> set to null.
        /// </summary>
        /// <param name="pageIndex">The page number to retrieve (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="order">The sort order direction.</param>
        /// <param name="searchTerm">The text to search for.</param>
        /// <returns>
        /// A <see cref="DtoOutput{TDto}"/> containing the paginated and filtered entity DTOs.
        /// </returns>
        public async Task<DtoOutput<TDto>> Find(int pageIndex, int pageSize, EnumOrder order, string searchTerm)
        {
            return await Find(pageIndex, pageSize, order, null, searchTerm).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the validator instance responsible for validating <typeparamref name="TObject"/> entities.
        /// Must be implemented by subclasses to return a concrete validator.
        /// </summary>
        /// <returns>
        /// An <see cref="IBaseAbstractValidator{TObject}"/> instance used for entity validation.
        /// </returns>
        protected abstract IBaseAbstractValidator<TObject> Validator();

        /// <summary>
        /// Gets the converter instance responsible for converting between <typeparamref name="TObject"/>
        /// entities and <typeparamref name="TDto"/> DTOs.
        /// Must be implemented by subclasses to return a concrete converter.
        /// </summary>
        /// <returns>
        /// An <see cref="IDefaultConverter{TObject,TDto}"/> instance used for DTO-entity mapping.
        /// </returns>
        protected abstract IDefaultConverter<TObject, TDto> Converter();

        /// <summary>
        /// Converts a <see cref="PagedSearchReturn{TObject}"/> into a <see cref="DtoOutput{TDto}"/>
        /// with pagination metadata. Subclasses may override this method to customize the output format.
        /// </summary>
        /// <param name="pagedSearchReturn">
        /// The paginated search result containing raw entity objects.
        /// </param>
        /// <returns>
        /// A <see cref="DtoOutput{TDto}"/> containing converted DTOs and pagination information.
        /// </returns>
        protected virtual DtoOutput<TDto> GetPagedSearchOutput(PagedSearchReturn<TObject> pagedSearchReturn)
        {
            return new DtoOutput<TDto>
            {
                Page = pagedSearchReturn.ActualPage,
                PageSize = pagedSearchReturn.Limit,
                ResultSet = Converter().Convert(pagedSearchReturn.Results),
                Success = true,
                TotalCount = pagedSearchReturn.TotalCount,
                ValidationFails = null
            };
        }
    }
}
