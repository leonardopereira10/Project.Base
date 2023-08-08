using FluentValidation.Results;
using Project.Base.Contracts.Models;

namespace Project.Base.Domain.Validators
{
    public interface IBaseAbstractValidator<TObject>
    {
        public void AsignInsertValidations();

        public void AsignUpdateValidations();

        public void AsignDeleteValidations();

        public ValidationResult Validate(TObject obj);

        public IEnumerable<ValidationFail> GetValidationOutput(ValidationResult validations);
    }
}
