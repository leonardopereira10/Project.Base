namespace Project.Base.Contracts.Models
{
    public class DtoOutput<Dto> where Dto : DtoBase
    {
        public DtoOutput()
        {
            Success = true;
        }

        public bool Success { get; set; }

        public IEnumerable<ValidationFail> ValidationFails { get; set; }

        public IEnumerable<Dto>? ResultSet { get; set; }

        public int Page { get; set; } = 0;

        public int PageSize { get; set; } = 0;

        public int PageCount => ResultSet == null ? 0 : ResultSet.Count();

        public int TotalCount { get; set; } = 0;

    }
}
