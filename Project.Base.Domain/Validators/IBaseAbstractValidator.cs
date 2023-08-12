using FluentValidation.Results;
using Project.Base.Contracts.Models;

namespace Project.Base.Domain.Validators
{
    public interface IBaseAbstractValidator<TObject>
    {
        public void AssignInsertValidations();

        public void AssignUpdateValidations();

        public void AssignDeleteValidations();

        public ValidationResult Validate(TObject obj);

        public IEnumerable<ValidationFail> GetValidationOutput(ValidationResult validations);
    }
}
