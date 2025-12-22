namespace SecureAPI_JWT.Models
{
    using System.ComponentModel.DataAnnotations;

    namespace SecureAPI_JWT.Models
    {
        public class AddRoleModel
        {
            [Required]
            public string UserId { get; set; }

            [Required]
            public string Role { get; set; }
        }
    }
}
