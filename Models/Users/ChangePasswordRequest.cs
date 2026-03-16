using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Users
{
    public class ChangePasswordRequest
    {
        [Required]
        public string OldPassword { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}
