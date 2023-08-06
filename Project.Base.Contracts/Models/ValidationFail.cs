namespace Project.Base.Contracts.Models
{
    public class ValidationFail
    {
        public required string Message { get; set; }

        public required string Property { get; set; }

        public bool IsImpeditive { get; set; }
    }
}