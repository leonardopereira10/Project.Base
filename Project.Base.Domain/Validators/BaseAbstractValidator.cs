using FluentValidation;
using FluentValidation.Results;
using Project.Base.Contracts.Models;
using Project.Base.Domain.Object.Shared;
using Project.Base.Enumerators;

namespace Project.Base.Domain.Validators
{
    public abstract class BaseAbstractValidator<TObject> : AbstractValidator<TObject>, IBaseAbstractValidator<TObject> where TObject : BaseObjectWithId
    {
        public virtual void AssignInsertValidations()
        {
            AssignCommonValidations();
        }

        public virtual void AssignUpdateValidations()
        {
            AssignObrigatoryId();
            AssignCommonValidations();
        }

        public virtual void AssignDeleteValidations()
        {
            AssignObrigatoryId();
        }

        public abstract void AssignCommonValidations();

        public IEnumerable<ValidationFail> GetValidationOutput(ValidationResult validations)
        {
            return validations.Errors.Select(x => 
                new ValidationFail 
                {
                    Message = x.ErrorMessage,
                    Property = x.PropertyName,
                    IsImpeditive = x.Severity == FluentValidation.Severity.Error 
                });
        }

        private void AssignObrigatoryId()
        {
            _ = RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(string.Format(Globalization.OBRIGATORY_FIELD, "ID"));
        }
    }
}
