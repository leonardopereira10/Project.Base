namespace Project.Base.Contracts.Models
{
    internal class DtoOutput<Dto> where Dto : DtoBase
    {
        public DtoOutput()
        {
            Success = true;
        }

        public bool Success { get; set; }

        public IEnumerable<Dto>? ResultSet { get; set; }

        public int Page { get; set; } = 0;

        public int PageSize { get; set; } = 0;

        public int PageCount => ResultSet == null ? 0 : ResultSet.Count();

        public int TotalCownt { get; set; } = 0;

    }
}
