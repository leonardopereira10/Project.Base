using FluentValidation;
using FluentValidation.Results;
using Project.Base.Contracts.Models;
using Project.Base.Domain.Object.Shared;
using Project.Base.Enumerators;

namespace Project.Base.Domain.Validators
{
    public abstract class BaseAbstractValidator<TObject> : AbstractValidator<TObject>, IBaseAbstractValidator<TObject> where TObject : BaseObjectWithId, new()
    {
        public abstract void AsignInsertValidations();

        public virtual void AsignUpdateValidations()
        {
            AsignObrigatoryId();
        }

        public virtual void AsignDeleteValidations()
        {
            AsignObrigatoryId();
        }

        public IEnumerable<ValidationFail> GetValidationOutput(ValidationResult validations)
        {
            return validations.Errors.Select(x => new ValidationFail { Message = x.ErrorMessage, Property = x.PropertyName, IsImpeditive = x.Severity == FluentValidation.Severity.Error });
        }

        private void AsignObrigatoryId()
        {
            _ = RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(string.Format(Globalization.OBRIGATORY_FIELD, "ID"));
        }
    }
}
