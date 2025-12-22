namespace CustomerService.Models.DTOs
{
    public class UpdateCustomerRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}
