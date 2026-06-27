namespace Project.Base.Contracts.Models
{
    /// <summary>
    /// Generic wrapper for API responses that carries success status, validation failures,
    /// a result set of DTOs, and pagination metadata.
    /// </summary>
    /// <typeparam name="Dto">The DTO type contained in the result set. Must inherit from <see cref="DtoBase"/>.</typeparam>
    public class DtoOutput<Dto> where Dto : DtoBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DtoOutput{TDto}"/> with <see cref="Success"/> set to <c>true</c>.
        /// </summary>
        public DtoOutput()
        {
            Success = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the operation completed successfully.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the collection of validation failures, if any occurred during the operation.
        /// </summary>
        public IEnumerable<ValidationFail>? ValidationFails { get; set; }

        /// <summary>
        /// Gets or sets the collection of DTOs returned by the operation.
        /// </summary>
        public IEnumerable<Dto>? ResultSet { get; set; }

        /// <summary>
        /// Gets or sets the current page number (zero-based). Defaults to <c>0</c>.
        /// </summary>
        public int Page { get; set; } = 0;

        /// <summary>
        /// Gets or sets the number of items per page. Defaults to <c>0</c>.
        /// </summary>
        public int PageSize { get; set; } = 0;

        /// <summary>
        /// Gets the total number of pages based on the current <see cref="ResultSet"/>.
        /// Returns <c>0</c> if the result set is null.
        /// </summary>
        public int PageCount => ResultSet == null ? 0 : ResultSet.Count();

        /// <summary>
        /// Gets or sets the total number of records across all pages. Defaults to <c>0</c>.
        /// </summary>
        public int TotalCount { get; set; } = 0;
    }
}
