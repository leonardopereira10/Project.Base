using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Project.Base.Contracts.Models;
using Project.Base.Contracts.ServiceContracts;
using Project.Base.Enumerators;

namespace Project.Base.WebApi.Controllers
{
    /// <summary>
    /// Serves as the generic base ASP.NET Core controller for all API controllers in the application.
    /// It exposes protected HTTP endpoints that delegate to <see cref="_service"/> for each CRUD operation.
    /// These endpoints are hidden from Swagger via <see cref="ApiExplorerSettingsAttribute"/> since they
    /// are intended for internal use by derived controllers.
    /// </summary>
    /// <typeparam name="TDto">
    /// The DTO type used for input and output across all endpoints. Must inherit from <see cref="DtoBase"/>.
    /// </typeparam>
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract class AbstractController<TDto> : ControllerBase where TDto : DtoBase
    {
        /// <summary>
        /// The underlying service instance used to execute business logic for CRUD operations.
        /// Injected via the constructor through dependency injection.
        /// </summary>
        protected IBaseService<TDto> _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractController{TDto}"/> class.
        /// </summary>
        /// <param name="service">The service instance injected via dependency injection.</param>
        protected AbstractController(IBaseService<TDto> service)
        {
            _service = service;
        }

        /// <summary>
        /// Verifica o resultado de uma operação e lança exceção se falhou,
        /// permitindo que o GlobalExceptionHandler retorne o status code correto.
        /// </summary>
        protected static async Task ThrowIfFailed<T>(Task<DtoOutput<T>> task, string operation)
            where T : DtoBase
        {
            var result = await task.ConfigureAwait(false);
            if (!result.Success)
            {
                var messages = result.ValidationFails?
                    .Where(v => v.IsImpeditive)
                    .Select(v => v.Message)
                    .Distinct()
                    .ToArray()
                    ?? Array.Empty<string>();

                if (messages.Length > 0)
                {
                    throw new ValidationException(string.Join("; ", messages));
                }
            }
        }

        /// <summary>
        /// Verifica se há falhas impeditivas no DtoOutput.
        /// Retorna true se houve falha (e a exceção já foi lançada).
        /// </summary>
        protected static bool CheckResult<T>(DtoOutput<T> result)
            where T : DtoBase
        {
            if (!result.Success)
            {
                var messages = result.ValidationFails?
                    .Where(v => v.IsImpeditive)
                    .Select(v => v.Message)
                    .Distinct()
                    .ToArray()
                    ?? Array.Empty<string>();

                if (messages.Length > 0)
                {
                    throw new ValidationException(string.Join("; ", messages));
                }
                // Se não há mensagens específicas, mas Success=false,
                // lança exceção genérica
                throw new ValidationException("Failed operation..");
            }
            return false;
        }

        /// <summary>
        /// Retrieves a single entity by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to retrieve.</param>
        /// <returns>
        /// An <see cref="ActionResult{TDtoOutput}"/> with HTTP 200 OK and the entity DTO on success,
        /// or HTTP 404 Not Found if the entity does not exist.
        /// </returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        protected virtual async Task<ActionResult<DtoOutput<TDto>>> FindById([FromQuery] Guid id)
        {
            DtoOutput<TDto> dto = await _service.FindById(id).ConfigureAwait(false);
            if (dto != null && !dto.Success)
            {
                return NoContent();
            }
            return Ok(dto);
        }

        /// <summary>
        /// Retrieves all entities without any filtering or pagination.
        /// </summary>
        /// <returns>
        /// An <see cref="ActionResult{TDtoOutput}"/> with HTTP 200 OK and all entity DTOs on success,
        /// or HTTP 404 Not Found if no entities exist.
        /// </returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        protected virtual async Task<ActionResult<DtoOutput<TDto>>> FindAll()
        {
            DtoOutput<TDto> dto = await _service.FindAll().ConfigureAwait(false);
            return dto == null ? NoContent() : Ok(dto);
        }

        /// <summary>
        /// Retrieves a paginated, filtered, and ordered list of entities.
        /// </summary>
        /// <param name="page">The page number to retrieve (1-based).</param>
        /// <param name="limit">The number of items per page.</param>
        /// <param name="order">The sort order direction (<see cref="EnumOrder.ASCENDING"/> or <see cref="EnumOrder.DESCENDING"/>).</param>
        /// <param name="searchTarget">
        /// The name of a specific property to search within. When null or empty, the search
        /// is performed across all string properties of the entity.
        /// </param>
        /// <param name="searchTerm">The text to search for within the specified target property.</param>
        /// <returns>
        /// An <see cref="ActionResult{TDtoOutput}"/> with HTTP 200 OK and the paginated DTOs on success,
        /// or HTTP 204 No Content if no results are found.
        /// </returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        protected virtual async Task<ActionResult<DtoOutput<TDto>>> Find(
            [FromQuery] int page,
            [FromQuery] int limit,
            [FromQuery] EnumOrder order,
            [FromQuery] string? searchTarget,
            [FromQuery] string? searchTerm)
        {
            DtoOutput<TDto> saida = await _service.Find(page, limit, order, searchTarget, searchTerm).ConfigureAwait(false);
            return saida == null || saida.TotalCount == 0 ? NoContent() : Ok(saida);
        }

        /// <summary>
        /// Creates a new entity from the provided DTO. The service layer validates the input
        /// and persists it. Returns HTTP 201 Created on success with the created entity DTO,
        /// or HTTP 400 Bad Request if validation fails.
        /// </summary>
        /// <param name="newObj">The DTO containing the data for the new entity.</param>
        /// <returns>
        /// An <see cref="ActionResult{TDtoOutput}"/> with HTTP 201 Created on success,
        /// or HTTP 400 Bad Request if validation fails.
        /// </returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        protected virtual async Task<ActionResult<DtoOutput<TDto>>> Insert([FromBody] TDto newObj)
        {
            DtoOutput<TDto> dto = await _service.Insert(newObj).ConfigureAwait(false);
            if (dto?.Success == true && dto.ResultSet != null)
            {
                return CreatedAtAction(nameof(FindById), new { id = dto.ResultSet }, dto);
            }
            if (dto != null && CheckResult(dto))
                return BadRequest(dto);
            return BadRequest(dto);
        }

        /// <summary>
        /// Updates an existing entity from the provided DTO. The service layer validates the input
        /// and persists the changes. Returns HTTP 200 OK on success, HTTP 400 Bad Request if
        /// validation fails or an exception occurs.
        /// </summary>
        /// <param name="newObj">The DTO containing the updated data for the entity.</param>
        /// <returns>
        /// An <see cref="ActionResult{TDtoOutput}"/> with HTTP 200 OK on success,
        /// or HTTP 400 Bad Request if validation fails or an exception occurs.
        /// </returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        protected virtual async Task<ActionResult<DtoOutput<TDto>>> Update([FromBody] TDto newObj)
        {
            DtoOutput<TDto> dto = await _service.Update(newObj).ConfigureAwait(false);
            if (dto?.Success == false && CheckResult(dto))
                return BadRequest(dto);
            return Ok(dto);
        }

        /// <summary>
        /// Deletes an entity by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to delete.</param>
        /// <returns>
        /// An <see cref="ActionResult"/> with HTTP 200 OK on success,
        /// or HTTP 400 Bad Request if the entity does not exist or validation fails.
        /// </returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        protected virtual ActionResult Delete([FromQuery] Guid id)
        {
            try
            {
                _ = _service.Delete(id);
            }
            catch
            {
                return BadRequest(id);
            }

            return Ok();
        }
    }
}
