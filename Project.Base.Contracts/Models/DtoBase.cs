namespace Project.Base.Contracts.Models
{
    /// <summary>
    /// Serves as the base class for all Data Transfer Objects (DTOs), providing a globally
    /// unique identifier that is inherited by all concrete DTO types.
    /// </summary>
    public class DtoBase
    {
        /// <summary>
        /// Gets or sets the globally unique identifier for the corresponding domain entity.
        /// </summary>
        public Guid Id { get; set; }
    }
}
