using System.ComponentModel.DataAnnotations;

namespace TiendaOnlineAPI.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string FullName { get; set; }
    }
}
