namespace CustomerService.Models
{
   public class CustomerProfileDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public List<ReclamationDto> Reclamations { get; set; } = new();
    }
}
