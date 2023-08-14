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

        public Task<DtoOutput<TDto>> Delete(Guid id)
        {
            return Task<DtoOutput<TDto>>.Factory.StartNew(() =>
            {
                Validator().AssignDeleteValidations();
                return Converter().ConvertToDtoOutput(_repository.Delete(id));
            });
        }

        public Task<DtoOutput<TDto>> Find(int pageIndex, int pageSize, EnumOrder order, string searchTarget, string searchTerm)
        {
            return Task<DtoOutput<TDto>>.Factory.StartNew(() =>
            {
                return GetPagedSearchOutput(_repository.List(new PagedSearchParam { Page = pageIndex, Limit = pageSize, Order = order, SearchTerm = searchTerm, SearchTarget = searchTarget }));
            });
        }

        public Task<DtoOutput<TDto>> FindAll()
        {
            return Task<DtoOutput<TDto>>.Factory.StartNew(() =>
            {
                return Converter().ConvertToDtoOutput(_repository.List());
            });
        }

        public Task<DtoOutput<TDto>> FindById(Guid id)
        {
            return Task<DtoOutput<TDto>>.Factory.StartNew(() =>
            {
                return Converter().ConvertToDtoOutput(_repository.List(x => x.Id == id));
            });
        }

        public Task<DtoOutput<TDto>> Insert(TDto dto)
        {
            return Task<DtoOutput<TDto>>.Factory.StartNew(() =>
            {
                TObject obj = Converter().Convert(dto);
                Validator().AssignInsertValidations();
                IEnumerable<ValidationFail> fails = Validator().GetValidationOutput(Validator().Validate(obj));

                return fails.Any(x => x.IsImpeditive)
                    ? Converter().GetDtoOutput(dto, fails)
                    : Converter().ConvertToDtoOutput(_repository.Insert(obj));
            });
        }

        public Task<DtoOutput<TDto>> Update(TDto dto)
        {
            return Task<DtoOutput<TDto>>.Factory.StartNew(() =>
            {
                TObject obj = Converter().Convert(dto);
                Validator().AssignUpdateValidations();
                IEnumerable<ValidationFail> fails = Validator().GetValidationOutput(Validator().Validate(obj));

                return fails.Any(x => x.IsImpeditive)
                    ? Converter().GetDtoOutput(dto, fails)
                    : Converter().ConvertToDtoOutput(_repository.Update(obj));
            });
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

        public Task<DtoOutput<TDto>> Find(int pageIndex, int pageSize, EnumOrder order, string searchTerm)
        {
            throw new NotImplementedException();
        }
    }
}
