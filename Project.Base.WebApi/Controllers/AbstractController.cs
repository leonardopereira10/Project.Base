using Microsoft.AspNetCore.Mvc;
using Project.Base.Contracts.Models;
using Project.Base.Contracts.ServiceContracts;
using Project.Base.Enumerators;

namespace Project.Base.WebApi.Controllers
{
    public abstract class AbstractController<TDto> : ControllerBase where TDto : DtoBase
    {
        protected IBaseService<TDto> _service;

        protected AbstractController(IBaseService<TDto> service)
        {
            _service = service;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        protected virtual ActionResult<DtoOutput<TDto>> Consultar(Guid codigo)
        {
            DtoOutput<TDto> dto = _service.FindById(codigo);
            return dto == null ? NotFound() : Ok(dto);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        protected virtual ActionResult<DtoOutput<TDto>> Find(
            [FromQuery] int page,
            [FromQuery] int limit,
            [FromQuery] EnumOrder order,
            [FromQuery] string? searchTarget,
            [FromQuery] string searchTerm)
        {
            DtoOutput<TDto> saida = searchTarget is null
                ? _service.Find(page, limit, order, searchTerm)
                : _service.Find(page, limit, order, searchTarget, searchTerm);
            return saida == null || saida.TotalCount == 0 ? NoContent() : Ok(saida);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        protected virtual ActionResult<DtoOutput<TDto>> Insert([FromBody] TDto newObj)
        {
            DtoOutput<TDto> dto = _service.Insert(newObj);
            return dto.Success ? CreatedAtAction("Insert", dto) : BadRequest(dto);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        protected virtual ActionResult<DtoOutput<TDto>> Update([FromBody] TDto newObj)
        {
            DtoOutput<TDto> dto;

            try
            {
                dto = _service.Update(newObj);

                if (!dto.Success)
                {
                    return BadRequest(dto);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

            return Ok(dto);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        protected virtual ActionResult Delete(Guid id)
        {
            try
            {
                _ = _service.Delete(id);
            }
            catch
            {
                return NotFound(id);
            }

            return Ok();
        }
    }
}
