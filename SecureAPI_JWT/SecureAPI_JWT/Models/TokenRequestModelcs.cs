namespace SecureAPI_JWT.Models
{
    using System.ComponentModel.DataAnnotations;

    namespace SecureAPI_JWT.Models
    {
        public class TokenRequestModel
        {
            [Required]
            public string Email { get; set; }

            [Required]
            public string Password { get; set; }
        }
    }
}
