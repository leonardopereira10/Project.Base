namespace Project.Base.Contracts.Models
{
    /// <summary>
    /// Represents a validation failure, including the error message, the property that caused
    /// the failure, and whether the failure is impeditive (blocking further processing).
    /// </summary>
    public class ValidationFail
    {
        /// <summary>
        /// Gets or sets the human-readable error message describing the validation failure.
        /// </summary>
        public required string Message { get; set; }

        /// <summary>
        /// Gets or sets the name of the property that failed validation.
        /// </summary>
        public required string Property { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this failure is impeditive,
        /// meaning it blocks further processing of the request.
        /// </summary>
        public bool IsImpeditive { get; set; }
    }
}
