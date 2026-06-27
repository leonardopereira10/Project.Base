using FluentValidation.Results;
using Project.Base.Contracts.Models;

namespace Project.Base.Domain.Validators
{
    /// <summary>
    /// Defines the contract for a generic validator that supports contextual validation
    /// for insert, update, and delete operations. Provides methods to configure validation
    /// rules per operation and to produce structured validation failure output.
    /// </summary>
    /// <typeparam name="TObject">The type of the object being validated.</typeparam>
    public interface IBaseAbstractValidator<TObject>
    {
        /// <summary>
        /// Configures validation rules specific to the insert operation.
        /// Called when validating a new object being created.
        /// </summary>
        void AssignInsertValidations();

        /// <summary>
        /// Configures validation rules specific to the update operation.
        /// Called when validating an existing object being modified.
        /// </summary>
        void AssignUpdateValidations();

        /// <summary>
        /// Configures validation rules specific to the delete operation.
        /// Called when validating an object being deleted.
        /// </summary>
        void AssignDeleteValidations();

        /// <summary>
        /// Validates the specified object using the currently configured rules.
        /// </summary>
        /// <param name="obj">The object to validate.</param>
        /// <returns>A <see cref="ValidationResult"/> containing the validation outcome.</returns>
        ValidationResult Validate(TObject obj);

        /// <summary>
        /// Converts a <see cref="ValidationResult"/> into a collection of <see cref="ValidationFail"/> models
        /// for structured error reporting.
        /// </summary>
        /// <param name="validations">The validation result to convert.</param>
        /// <returns>A collection of <see cref="ValidationFail"/> instances representing the validation errors.</returns>
        IEnumerable<ValidationFail> GetValidationOutput(ValidationResult validations);
    }
}
