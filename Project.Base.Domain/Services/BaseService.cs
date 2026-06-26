using Project.Base.Contracts.Models;
using Project.Base.Contracts.ServiceContracts;
using Project.Base.Domain.Object.Shared;
using Project.Base.Domain.Repositories;
using Project.Base.Domain.Validators;
using Project.Base.Enumerators;

namespace Project.Base.Domain.Services
{
    public abstract class BaseService<TObject, TDto> : IBaseService<TDto>
        where TDto : DtoBase
        where TObject : BaseObjectWithId
    {
        protected readonly IBaseObjectWithIdRepository<TObject> _repository;

        protected BaseService(IBaseObjectWithIdRepository<TObject> repository)
        {
            _repository = repository;
        }

        // ── Async real (sem Task.Factory.StartNew) ──
        public async Task<DtoOutput<TDto>> Delete(Guid id)
        {
            Validator().AssignDeleteValidations();
            var obj = await _repository.DeleteAsync(id).ConfigureAwait(false);
            return Converter().ConvertToDtoOutput(obj);
        }

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

        public async Task<DtoOutput<TDto>> FindAll()
        {
            var results = await _repository.ListAsync().ConfigureAwait(false);
            return Converter().ConvertToDtoOutput(results);
        }

        public async Task<DtoOutput<TDto>> FindById(Guid id)
        {
            IEnumerable<TObject> result = await _repository.ListAsync(x => x.Id == id).ConfigureAwait(false);
            return Converter().ConvertToDtoOutput(result);
        }

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

        public async Task<DtoOutput<TDto>> Find(int pageIndex, int pageSize, EnumOrder order, string searchTerm)
        {
            return await Find(pageIndex, pageSize, order, null, searchTerm).ConfigureAwait(false);
        }

        protected abstract IBaseAbstractValidator<TObject> Validator();

        protected abstract IDefaultConverter<TObject, TDto> Converter();

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
