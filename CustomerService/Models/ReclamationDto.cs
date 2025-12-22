namespace CustomerService.Models
{
    public class ReclamationDto
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public string Description { get; set; } = string.Empty;
        public ReclamationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        //public int? InterventionId { get; set; }
    }
}
