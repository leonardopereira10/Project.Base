using FluentValidation;
using FluentValidation.Results;
using Project.Base.Contracts.Models;
using Project.Base.Domain.Object.Shared;
using Project.Base.Enumerators;

namespace Project.Base.Domain.Validators
{
    /// <summary>
    /// Abstract base validator that extends FluentValidation's <see cref="AbstractValidator{T}"/>
    /// and implements <see cref="IBaseAbstractValidator{TObject}"/>. Provides contextual validation
    /// support for insert, update, and delete operations using localized messages from <c>Globalization.resx</c>.
    /// Subclasses must implement <see cref="AssignCommonValidations"/> to define shared rules,
    /// and may override <see cref="AssignInsertValidations"/>, <see cref="AssignUpdateValidations"/>,
    /// and <see cref="AssignDeleteValidations"/> for operation-specific rules.
    /// </summary>
    /// <typeparam name="TObject">The type of the object being validated. Must inherit from <see cref="BaseObjectWithId"/>.</typeparam>
    public abstract class BaseAbstractValidator<TObject> : AbstractValidator<TObject>, IBaseAbstractValidator<TObject> where TObject : BaseObjectWithId
    {
        /// <summary>
        /// Configures validation rules for the insert operation.
        /// By default, invokes <see cref="AssignCommonValidations"/> without requiring an ID.
        /// </summary>
        public virtual void AssignInsertValidations()
        {
            AssignCommonValidations();
        }

        /// <summary>
        /// Configures validation rules for the update operation.
        /// By default, requires the ID to be present and then invokes <see cref="AssignCommonValidations"/>.
        /// </summary>
        public virtual void AssignUpdateValidations()
        {
            AssignObrigatoryId();
            AssignCommonValidations();
        }

        /// <summary>
        /// Configures validation rules for the delete operation.
        /// By default, only requires the ID to be present.
        /// </summary>
        public virtual void AssignDeleteValidations()
        {
            AssignObrigatoryId();
        }

        /// <summary>
        /// When overridden in a derived class, defines the common validation rules
        /// shared across all operations (insert, update, delete).
        /// </summary>
        public abstract void AssignCommonValidations();

        /// <summary>
        /// Converts a <see cref="ValidationResult"/> into a collection of <see cref="ValidationFail"/> models
        /// for structured error reporting. Each error is mapped to a <see cref="ValidationFail"/> with
        /// the error message, property name, and an <c>IsImpeditive</c> flag based on severity.
        /// </summary>
        /// <param name="validations">The validation result to convert.</param>
        /// <returns>A collection of <see cref="ValidationFail"/> instances representing the validation errors.</returns>
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

        /// <summary>
        /// Adds a rule requiring the entity's <see cref="BaseObjectWithId.Id"/> property to be non-empty.
        /// Uses a localized message from <c>Globalization.resx</c>.
        /// </summary>
        private void AssignObrigatoryId()
        {
            _ = RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage(string.Format(Globalization.OBRIGATORY_FIELD, "ID"));
        }
    }
}
